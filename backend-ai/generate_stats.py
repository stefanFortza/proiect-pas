import pandas as pd
import os

class YasApiError(Exception):
    def __init__(self, message: str):
        self.message = message
        super().__init__(self.message)

def generate_benchmark_stats(input_csv="benchmark_results_final.csv", output_csv="benchmark_summary_stats_gpt2_small.csv"):
    # Verificăm dacă fișierul există
    if not os.path.exists(input_csv):
        raise YasApiError(f"Fișierul de rezultate '{input_csv}' nu a putut fi găsit!")

    # 1. Încărcăm datele
    df = pd.read_csv(input_csv)

    # 2. Definim coloanele de grupare și metricile de calculat
    grouping_columns = ['model_name', 'epoch', 'language']
    metrics = ['grammatical_structure', 'syntactical_structure', 'meaning', 'relevance']

    # 3. Calculăm media (mean) și rotunjim la 2 zecimale pentru un aspect curat
    summary_df = df.groupby(grouping_columns)[metrics].mean().round(2).reset_index()

    # 4. Salvăm tabelul într-un nou fișier CSV
    summary_df.to_csv(output_csv, index=False, encoding="utf-8")

    # 5. Afișăm tabelul frumos formatat în consolă
    print("\n📊 STATISTICI MEDII BENCHMARK (Model -> Epocă -> Limbă) 📊\n")
    print(summary_df.to_string(index=False, justify='center'))
    print(f"\n✅ Tabelul cu statistici a fost salvat cu succes în: {output_csv}")

if __name__ == "__main__":
    generate_benchmark_stats()