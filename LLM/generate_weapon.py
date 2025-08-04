import json
import os
import openai
from openai import OpenAI
import argparse

# ====== CONFIGURATION ======
persistent_data_path = r"C:\Users\jiahui li\AppData\LocalLow\DefaultCompany\Summer Project draft"
file_path = os.path.join(persistent_data_path, "radial_config.json")
model = "gpt-4o"  # Use GPT-4o
client = OpenAI()
# ===========================

def load_existing_data():
    if not os.path.exists(file_path):
        raise FileNotFoundError(f"File not found: {file_path}")
    with open(file_path, 'r') as f:
        return json.load(f)

def save_final_json(data):
    with open(file_path, 'w') as f:
        json.dump(data, f, indent=2)
    print(f"\nFinal updated config saved to: {file_path}")

def generate_mode_data_with_openai(mode_id, user_description):
    prompt = f"""
You are generating radial burst weapon configuration for a 2D shooter game.

Return only valid compact JSON — do NOT include any text, explanation, or code block (no triple backticks). Output pure JSON only.

Format:
{{
  "bulletCount": 5,
  "speeds": [10, 12, 14, 12, 10],
  "angles": [-40, -20, 0, 20, 40],
  "damage": 15,
  "cooldown": 1.2
}}

Constraints:
- bulletCount: int, 5 to 20
- speeds: list of floats (same length as bulletCount)
- angles: list of floats in degrees (same length as bulletCount)
- damage: float, between 1 and 20
- cooldown: float, between 0.2 and 2.0

User description for mode ID {mode_id}:
\"\"\"{user_description}\"\"\"
"""
    response = client.chat.completions.create(
        model=model,
        messages=[{ "role": "user", "content": prompt }],
        temperature=0.7
    )
    content = response.choices[0].message.content.strip()
    print(f"\n Mode {mode_id} LLM Output:\n{content}")
    return json.loads(content)

def update_modes(data, mode_ids_with_descriptions):
    mode_dict = {entry["key"]: entry["value"] for entry in data["modes"]}

    for mode_id, desc in mode_ids_with_descriptions.items():
        print(f"\n Updating mode '{mode_id}' with description:\n{desc}")
        mode_dict[mode_id] = generate_mode_data_with_openai(mode_id, desc)
        print(f" Mode '{mode_id}' replaced.")

    data["modes"] = [{ "key": k, "value": v } for k, v in mode_dict.items()]
    return data

if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--mode", required=True, help="Mode ID to update")
    parser.add_argument("--desc", required=True, help="Description of the mode")

    args = parser.parse_args()
    mode_id = args.mode
    desc = args.desc

    print("Loading current radial_config.json...")
    data = load_existing_data()

    if "modes" not in data or not isinstance(data["modes"], list):
        raise ValueError(" 'modes' must be a list of {key, value} objects.")

    mode_descriptions = {mode_id: desc}
    updated_data = update_modes(data, mode_descriptions)

    save_final_json(updated_data)
    print(" Script completed.")