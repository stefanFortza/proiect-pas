import pandas as pd
import torch
from transformers import AutoTokenizer, AutoModelForCausalLM
from peft import PeftModel
import gc
import os
import json
import time
from tqdm import tqdm
import google.generativeai as genai

# --- 1. CONFIGURARE GEMINI ȘI MODELE ---
GEMINI_API_KEY = os.environ.get("GEMINI_API_KEY", "AIzaSyAlFu_kAO7tR7S9jlsY-fdRclY3IxgyWOM")
genai.configure(api_key=GEMINI_API_KEY)
judge_model = genai.GenerativeModel('gemini-2.5-flash', generation_config={"response_mime_type": "application/json"})

DEVICE = torch.device("cuda" if torch.cuda.is_available() else "cpu")

# Am schimbat "epoch" în "max_epochs" pentru a ști câte epoci să evaluăm pentru fiecare model
MODELS_CONFIG = {
    # "gpt2_small_base": {"base_model": "gpt2", "lora_base_dir": "./benchmark_checkpoints", "max_epochs": 5},
    # "gpt2_small_full": {"base_model": "gpt2", "lora_base_dir": "./benchmark_checkpoints_full", "max_epochs": 5},
    # "gpt2_small_replay": {"base_model": "gpt2", "lora_base_dir": "./benchmark_checkpoints_replay", "max_epochs": 5},
    # "pythia_1.4b": {"base_model": "EleutherAI/pythia-1.4b", "lora_base_dir": "./benchmark_pythia_lora", "max_epochs": 5},
    "gpt2_large_frozen": {"base_model": "gpt2-large", "lora_base_dir": "./benchmark_gpt2_large_lora", "max_epochs": 5},
    # "gpt2_large_stable": {"base_model": "gpt2-large", "lora_base_dir": "./benchmark_gpt2_large_stable", "max_epochs": 5}
}

# --- 2. TRATAREA ERORILOR ---
class ApiError(Exception):
    def __init__(self, message: str):
        self.message = message
        super().__init__(self.message)

# --- 3. FUNCȚII DE EVALUARE ---
def evaluate_with_gemini(user_prompt: str, bot_response: str, retries=3):
    """
    Evaluează răspunsul folosind Gemini, cu un mecanism de Retry în caz de limită de request-uri (Rate Limit).
    """
    eval_prompt = f"""
    You are an expert linguistic evaluator.
    A user asked an AI model a question. Grade the model's response strictly from 0 to 100 on these 4 criteria.
    If the response is gibberish or unrelated, give very low scores.

    1. grammatical_structure (0-100): Are the words formed and conjugated correctly in the target language?
    2. syntactical_structure (0-100): Are the words arranged in a logical, rule-abiding order?
    3. meaning (0-100): Does the sentence make logical sense on its own?
    4. relevance (0-100): Does it directly and accurately answer the user's specific prompt?

    Output ONLY a JSON object with these exact four keys and integer values.

    User Prompt: "{user_prompt}"
    Model Response: "{bot_response}"
    """

    for attempt in range(retries):
        try:
            result = judge_model.generate_content(eval_prompt)
            scores = json.loads(result.text)

            # Pauză mică pentru a nu enerva API-ul Gemini (evităm eroarea 429)
            time.sleep(2)
            return scores
        except Exception as e:
            if "429" in str(e):
                print(f"   [⏳ API limit] Așteptăm 15 secunde (Încercarea {attempt+1}/{retries})...")
                time.sleep(15)
            else:
                print(f"   [⚠️ Eroare Parsare] {e}")
                time.sleep(2)

    # Fallback dacă tot eșuează
    return {"grammatical_structure": 0, "syntactical_structure": 0, "meaning": 0, "relevance": 0}

# --- 4. LOGICA DE RULARE A UNUI MODEL PENTRU O EPOCĂ SPECIFICĂ ---
def run_model_evaluation(model_name, config, epoch, dataset):
    results = []
    model_path = os.path.join(config["lora_base_dir"], f"epoch_{epoch}")

    print(f"\n==================================================")
    print(f"🚀 INIȚIALIZARE MODEL: {model_name} | EPOCA: {epoch}")
    print(f"==================================================")

    if not os.path.exists(model_path):
        print(f"❌ Calea nu există: {model_path}. Se sare peste această epocă.")
        return results

    # Încărcare Model
    tokenizer = AutoTokenizer.from_pretrained(model_path)
    if tokenizer.pad_token is None:
        tokenizer.pad_token = tokenizer.eos_token

    is_lora = os.path.exists(os.path.join(model_path, "adapter_config.json"))

    if is_lora:
        base_model = AutoModelForCausalLM.from_pretrained(config["base_model"], torch_dtype=torch.float16).to(DEVICE)
        model = PeftModel.from_pretrained(base_model, model_path).eval()
    else:
        model = AutoModelForCausalLM.from_pretrained(model_path, torch_dtype=torch.float16).to(DEVICE)
        model.eval()

    # Procesarea Datelor
    print(f"⚙️ Începe generarea și evaluarea...")
    for index, row in tqdm(dataset.iterrows(), total=len(dataset), desc=f"Evaluând {model_name} (Epoca {epoch})"):

        # Testăm pentru ambele limbi
        for lang, col_name in [("ES", "text_es"), ("EN", "text_en")]:
            prompt = row[col_name]

            # Formatarea corectă
            formatted_prompt = f"User: {prompt}\nBot:"
            inputs = tokenizer(formatted_prompt, return_tensors="pt").to(DEVICE)

            with torch.no_grad():
                outputs = model.generate(
                    **inputs,
                    max_new_tokens=40,
                    temperature=0.7,
                    do_sample=True,
                    pad_token_id=tokenizer.eos_token_id,
                    no_repeat_ngram_size=2
                )

            full_response = tokenizer.decode(outputs[0], skip_special_tokens=True)
            bot_reply = full_response.split("Bot:")[-1].strip()

            # Trimitem la Gemini pentru judecată
            scores = evaluate_with_gemini(prompt, bot_reply)

            # Salvăm linia de rezultate, inclusiv informația despre epocă
            results.append({
                "model_name": model_name,
                "epoch": epoch,
                "language": lang,
                "category": row["category"],
                "difficulty": row["difficulty"],
                "input_prompt": prompt,
                "bot_response": bot_reply,
                "expected_answer": row["expected_answer"],
                "grammatical_structure": scores.get("grammatical_structure", 0),
                "syntactical_structure": scores.get("syntactical_structure", 0),
                "meaning": scores.get("meaning", 0),
                "relevance": scores.get("relevance", 0)
            })

    # Eliberare Forțată a Memoriei VRAM pentru a face loc următoarei epoci
    del model
    del tokenizer
    gc.collect()
    if torch.cuda.is_available():
        torch.cuda.empty_cache()

    print(f"✅ Evaluare completă pentru {model_name} (Epoca {epoch}). Memorie curățată.")
    return results

# --- 5. EXECUȚIA PRINCIPALĂ ---
def main():
    DATASET_PATH = "test_set.csv"
    OUTPUT_PATH = "benchmark_results_final.csv"

    if not os.path.exists(DATASET_PATH):
        raise ApiError(f"Fișierul de date {DATASET_PATH} nu a fost găsit în directorul curent.")

    df = pd.read_csv(DATASET_PATH, on_bad_lines='skip')
    all_results = []

    # Iterăm prin fiecare model
    for model_name, config in MODELS_CONFIG.items():
        max_epochs = config.get("max_epochs", 5)
        
        # Iterăm prin fiecare epocă a modelului curent (de la 1 la max_epochs)
        for current_epoch in range(1, max_epochs + 1):
            model_results = run_model_evaluation(model_name, config, current_epoch, df)
            all_results.extend(model_results)

            # Salvăm parțial după fiecare epocă evaluată pentru a preveni pierderile de date
            if all_results:
                temp_df = pd.DataFrame(all_results)
                temp_df.to_csv(OUTPUT_PATH, index=False, encoding="utf-8")
                print(f"💾 Progresul salvat temporar în {OUTPUT_PATH}")

    print("\n🎉 BENCHMARK COMPLET! Toate modelele și epocile au fost evaluate cu succes.")
    print(f"📊 Rezultatele finale sunt în: {OUTPUT_PATH}")

if __name__ == "__main__":
    main()