from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
from typing import Optional, Dict
import torch
from transformers import AutoTokenizer, AutoModelForCausalLM
from peft import PeftModel
import gc
import os
import json
import google.generativeai as genai
from dotenv import load_dotenv

app = FastAPI(title="Multilingual LLM Benchmark API with LLM-Judge")

load_dotenv()  # Load environment variables from .env file

# --- 1. GEMINI JUDGE CONFIGURATION ---
# Replace with your actual Gemini API key, or set it as an environment variable
GEMINI_API_KEY = os.environ.get("GEMINI_API_KEY", "womp-womp")
genai.configure(api_key=GEMINI_API_KEY)

# We use the flash model because it is fast and excellent at strict JSON extraction
judge_model = genai.GenerativeModel(
    "gemini-2.5-flash", generation_config={"response_mime_type": "application/json"}
)

# --- 2. CONFIGURAȚIE COMPLETĂ A TUTUROR MODELELOR ---
DEVICE = torch.device("cuda" if torch.cuda.is_available() else "cpu")

MODELS_CONFIG = {
    # --- Modelele GPT-2 Small (124M) ---
    "gpt2_small_base": {
        "base_model": "gpt2",
        "lora_base_dir": "./benchmark_checkpoints",
    },
    "gpt2_small_full": {
        "base_model": "gpt2",
        "lora_base_dir": "./benchmark_checkpoints_full",
    },
    "gpt2_small_replay": {
        "base_model": "gpt2",
        "lora_base_dir": "./benchmark_checkpoints_replay",
    },
    # --- Modelul Pythia (1.4B) ---
    "pythia_1.4b": {
        "base_model": "EleutherAI/pythia-1.4b",
        "lora_base_dir": "./benchmark_pythia_lora",
    },
    # --- Modelele GPT-2 Large (774M) ---
    "gpt2_large_frozen": {
        "base_model": "gpt2-large",
        "lora_base_dir": "./benchmark_gpt2_large_lora",
    },
    "gpt2_large_stable": {
        "base_model": "gpt2-large",
        "lora_base_dir": "./benchmark_gpt2_large_stable",
    },
}

active_state = {"model_name": None, "epoch": None, "model": None, "tokenizer": None}


class GenerateRequest(BaseModel):
    prompt: str
    model_name: str
    epoch: int
    temperature: float = 0.7
    max_tokens: int = 40


# --- 3. THE LLM EVALUATOR (JUDGE) ---
def evaluate_with_gemini(
    user_prompt: str, bot_response: str
) -> Optional[Dict[str, int]]:
    """
    Trims the context and asks Gemini to grade the response on 4 strict criteria.
    Returns a dictionary of scores or None if the API fails.
    """
    eval_prompt = f"""
    You are an expert linguistic and logic evaluator.
    A user asked a language model a question. You must grade the model's response based on 4 criteria.
    Provide a score from 0 to 100 for each. Be precise and strict.

    1. grammatical_structure (0-100): Are the words formed and conjugated correctly?
    2. syntactical_structure (0-100): Are the words arranged in a logical, rule-abiding order?
    3. meaning (0-100): Does the sentence make logical sense on its own, regardless of the prompt?
    4. relevance (0-100): Does it directly and accurately answer the user's specific prompt?

    Output ONLY a JSON object with these exact four keys and integer values.

    User Prompt: "{user_prompt}"
    Model Response: "{bot_response}"
    """

    try:
        result = judge_model.generate_content(eval_prompt)
        # Parse the guaranteed JSON output
        scores = json.loads(result.text)
        return scores
    except Exception as e:
        print(f"⚠️ Eroare la evaluarea Gemini: {str(e)}")
        # Safe fallback so your main API doesn't crash if Gemini timeouts
        return {"error": "Evaluation failed", "details": str(e)}


# --- 4. MANAGEMENTUL INTELIGENT AL MEMORIEI ȘI MODELELOR ---
def load_model_if_needed(model_name: str, epoch: int):
    global active_state

    if model_name not in MODELS_CONFIG:
        raise HTTPException(
            status_code=404,
            detail=f"Modelul '{model_name}' nu există. Opțiuni valide: {list(MODELS_CONFIG.keys())}",
        )

    model_path = os.path.join(
        MODELS_CONFIG[model_name]["lora_base_dir"], f"epoch_{epoch}"
    )

    if not os.path.exists(model_path):
        raise HTTPException(
            status_code=404,
            detail=f"Checkpoint-ul pentru epoca {epoch} nu a fost găsit la calea: {model_path}",
        )

    if active_state["model_name"] == model_name and active_state["epoch"] == epoch:
        return active_state["model"], active_state["tokenizer"]

    print(f"🔄 Se încarcă {model_name} (Epoch {epoch})...")

    if active_state["model"] is not None:
        del active_state["model"], active_state["tokenizer"]
        gc.collect()
        if torch.cuda.is_available():
            torch.cuda.empty_cache()

    try:
        tokenizer = AutoTokenizer.from_pretrained(model_path)
        if tokenizer.pad_token is None:
            tokenizer.pad_token = tokenizer.eos_token

        is_lora = os.path.exists(os.path.join(model_path, "adapter_config.json"))

        if is_lora:
            print(
                "   -> Tip: LoRA Adapter detectat. Se încarcă modelul de bază + adaptoarele."
            )
            base_model = AutoModelForCausalLM.from_pretrained(
                MODELS_CONFIG[model_name]["base_model"], torch_dtype=torch.float16
            ).to(DEVICE)
            model = PeftModel.from_pretrained(base_model, model_path).eval()
        else:
            print(
                "   -> Tip: Full Fine-Tuned Model detectat. Se încarcă greutățile integrale."
            )
            model = AutoModelForCausalLM.from_pretrained(
                model_path, torch_dtype=torch.float16
            ).to(DEVICE)
            model.eval()

        active_state.update(
            {
                "model_name": model_name,
                "epoch": epoch,
                "model": model,
                "tokenizer": tokenizer,
            }
        )

        return model, tokenizer

    except Exception as e:
        raise HTTPException(
            status_code=500, detail=f"Eroare critică la încărcarea modelului: {str(e)}"
        )


# --- 5. ENDPOINT-UL PRINCIPAL ---
@app.post("/generate")
async def generate_text(request: GenerateRequest):
    # 1. Load Model
    model, tokenizer = load_model_if_needed(request.model_name, request.epoch)

    # 2. Format Prompt and Generate
    formatted_prompt = f"User: {request.prompt}\nBot:"
    inputs = tokenizer(formatted_prompt, return_tensors="pt").to(DEVICE)

    with torch.no_grad():
        outputs = model.generate(
            **inputs,
            max_new_tokens=request.max_tokens,
            temperature=request.temperature,
            do_sample=True,
            pad_token_id=tokenizer.eos_token_id,
            no_repeat_ngram_size=2,
        )

    full_response = tokenizer.decode(outputs[0], skip_special_tokens=True)
    bot_reply = full_response.split("Bot:")[-1].strip()

    # 3. Pass the result to the LLM Judge
    print("Se trimite răspunsul către Gemini pentru evaluare...")
    evaluation_scores = evaluate_with_gemini(request.prompt, bot_reply)

    print(f"Evaluare Gemini: {evaluation_scores}")

    # 4. Return everything to the user
    return {
        "model_used": request.model_name,
        "epoch_used": request.epoch,
        "prompt": request.prompt,
        "response": bot_reply,
        "evaluation_scores": evaluation_scores,
    }
