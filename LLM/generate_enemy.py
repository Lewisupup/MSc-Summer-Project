import json
import os
from openai import OpenAI

def send_seq_and_save_json_verbatim(seq, persistent_base, filename,
                                    model="gpt-4o", temperature=0.3):

    seq_str = " ".join(seq)

    prompt = (
        f"Based on this user movement habit sequence (verbatim):\n{seq_str}\n"
        "Return PURE JSON ONLY (no text, no code fences) for the enemy moving pattern"
        "To force the player in a 2D rogue-like shooter game to move like this.\n"
        'Exact output shape:\n'
        '{"instructions":[{"dx":0,"dy":-1,"speed":3.5,"duration":1.2}]}\n'
        "Rules:\n"
        "- dx, dy ∈ {-1,0,1}\n"
        "- speed ∈ [0.0, 10.0]\n"
        "- duration ∈ [0.1, 2.0]\n"
        "- Can produce multiple instruction objects, maximum length 10\n"
        "- dy controls forward/back movement relative to the player (1 = toward, -1 = away, 0 = none)\n"
        "- dx controls strafing left/right relative to the player (1 = right strafe, -1 = left strafe, 0 = none)\n"
        "- Final movement vector is calculated as: "
        "worldDir = (toPlayer * dy) + (perpendicularToPlayer * dx), "
        "then normalized and multiplied by 'speed'\n"
    )

    client = OpenAI()
    resp = client.chat.completions.create(
        model=model,
        messages=[{"role":"user","content":prompt}],
        temperature=temperature
    )
    content = resp.choices[0].message.content.strip()

    # Must be valid JSON in the required shape
    obj = json.loads(content)

    os.makedirs(persistent_base, exist_ok=True)
    path = os.path.join(persistent_base, filename)
    with open(path, "w", encoding="utf-8") as f:
        json.dump(obj, f, indent=2)
    print(f"[movement_server] Saved: {path}")

    return obj
