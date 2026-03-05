"""
WordSoul - Vocabulary Seed Script
==================================
Generates seed_vocabularies.sql with ~200 vocabulary words.

Requirements:
    pip install requests deep_translator python-dotenv

Usage:
    cd Scripts
    python seed_vocabularies.py

Output:
    seed_vocabularies.sql  (import vào Azure SQL qua SSMS)
"""

import os
import time
import requests
from datetime import datetime
from deep_translator import GoogleTranslator

# ─────────────────────────────────────────────
# CONFIG
# ─────────────────────────────────────────────
UNSPLASH_ACCESS_KEY = os.getenv("UNSPLASH_ACCESS_KEY", "F9mqbgZ-_q0Ds83LfzEPvq4BM-5d8IPzG6o7RjzLBEs")

OUTPUT_FILE = "seed_vocabularies.sql"

# ─────────────────────────────────────────────
# ENUM MAPPINGS (phải khớp với C# enum)
# ─────────────────────────────────────────────
PART_OF_SPEECH_MAP = {
    "noun": 0,
    "verb": 1,
    "adjective": 2,
    "adverb": 3,
    "pronoun": 4,
    "preposition": 5,
    "conjunction": 6,
    "interjection": 7,
}

CEFR_INT_MAP = {
    "A1": 0,
    "A2": 1,
    "B1": 2,
    "B2": 3,
    "C1": 4,
    "C2": 5,
}

# ─────────────────────────────────────────────
# WORD LIST: 200 từ, phân theo CEFR level
# format: (word, cefr_level)
# ─────────────────────────────────────────────
WORD_LIST = [
    # ── A1: Beginner (40 từ) ──────────────────
    ("apple",   "A1"), ("book",    "A1"), ("cat",     "A1"), ("dog",     "A1"),
    ("eat",     "A1"), ("fish",    "A1"), ("good",    "A1"), ("house",   "A1"),
    ("idea",    "A1"), ("jump",    "A1"), ("key",     "A1"), ("love",    "A1"),
    ("milk",    "A1"), ("name",    "A1"), ("open",    "A1"), ("play",    "A1"),
    ("read",    "A1"), ("run",     "A1"), ("sit",     "A1"), ("talk",    "A1"),
    ("use",     "A1"), ("walk",    "A1"), ("year",    "A1"), ("zero",    "A1"),
    ("baby",    "A1"), ("card",    "A1"), ("door",    "A1"), ("food",    "A1"),
    ("give",    "A1"), ("hand",    "A1"), ("call",    "A1"), ("come",    "A1"),
    ("day",     "A1"), ("find",    "A1"), ("go",      "A1"), ("know",    "A1"),
    ("like",    "A1"), ("make",    "A1"), ("see",     "A1"), ("time",    "A1"),

    # ── A2: Elementary (50 từ) ───────────────
    ("afraid",   "A2"), ("begin",   "A2"), ("carry",   "A2"), ("decide",  "A2"),
    ("early",    "A2"), ("fall",    "A2"), ("garden",  "A2"), ("happen",  "A2"),
    ("include",  "A2"), ("join",    "A2"), ("kind",    "A2"), ("large",   "A2"),
    ("minute",   "A2"), ("notice",  "A2"), ("often",   "A2"), ("paint",   "A2"),
    ("quiet",    "A2"), ("river",   "A2"), ("school",  "A2"), ("travel",  "A2"),
    ("under",    "A2"), ("visit",   "A2"), ("warm",    "A2"), ("young",   "A2"),
    ("able",     "A2"), ("bridge",  "A2"), ("color",   "A2"), ("drive",   "A2"),
    ("enjoy",    "A2"), ("fail",    "A2"), ("great",   "A2"), ("help",    "A2"),
    ("learn",    "A2"), ("mind",    "A2"), ("offer",   "A2"), ("part",    "A2"),
    ("quick",    "A2"), ("reach",   "A2"), ("result",  "A2"), ("think",   "A2"),
    ("until",    "A2"), ("water",   "A2"), ("write",   "A2"), ("always",  "A2"),
    ("change",   "A2"), ("during",  "A2"), ("family",  "A2"), ("friend",  "A2"),
    ("money",    "A2"), ("place",   "A2"),

    # ── B1: Intermediate (50 từ) ─────────────
    ("achieve",   "B1"), ("believe",   "B1"), ("career",    "B1"), ("define",    "B1"),
    ("effort",    "B1"), ("familiar",  "B1"), ("gradual",   "B1"), ("handle",    "B1"),
    ("impact",    "B1"), ("justify",   "B1"), ("knowledge", "B1"), ("level",     "B1"),
    ("manage",    "B1"), ("narrow",    "B1"), ("observe",   "B1"), ("prevent",   "B1"),
    ("quality",   "B1"), ("require",   "B1"), ("suggest",   "B1"), ("threaten",  "B1"),
    ("unique",    "B1"), ("various",   "B1"), ("wisdom",    "B1"), ("accept",    "B1"),
    ("balance",   "B1"), ("concern",   "B1"), ("develop",   "B1"), ("explore",   "B1"),
    ("flexible",  "B1"), ("govern",    "B1"), ("honest",    "B1"), ("increase",  "B1"),
    ("judge",     "B1"), ("maintain",  "B1"), ("obtain",    "B1"), ("prefer",    "B1"),
    ("request",   "B1"), ("strategy",  "B1"), ("transform", "B1"), ("understand","B1"),
    ("verify",    "B1"), ("willing",   "B1"), ("adapt",     "B1"), ("benefit",   "B1"),
    ("challenge", "B1"), ("determine", "B1"), ("focus",     "B1"), ("generate",  "B1"),
    ("improve",   "B1"), ("reduce",    "B1"),

    # ── B2: Upper Intermediate (40 từ) ───────
    ("accomplish",   "B2"), ("beneath",      "B2"), ("coincide",    "B2"), ("deliberate",  "B2"),
    ("enhance",      "B2"), ("forbid",       "B2"), ("genuine",     "B2"), ("hesitate",    "B2"),
    ("indicate",     "B2"), ("legitimate",   "B2"), ("manipulate",  "B2"), ("negotiate",   "B2"),
    ("oppose",       "B2"), ("perceive",     "B2"), ("reluctant",   "B2"), ("separate",    "B2"),
    ("tolerate",     "B2"), ("undermine",    "B2"), ("valid",       "B2"), ("withdraw",    "B2"),
    ("abstract",     "B2"), ("brilliant",    "B2"), ("comprehensive","B2"), ("dilemma",    "B2"),
    ("elaborate",    "B2"), ("fundamental",  "B2"), ("integrate",   "B2"), ("meticulous",  "B2"),
    ("nuance",       "B2"), ("obstacle",     "B2"), ("perpetual",   "B2"), ("remarkable",  "B2"),
    ("significant",  "B2"), ("transparent",  "B2"), ("ultimate",    "B2"), ("vulnerable",  "B2"),
    ("widespread",   "B2"), ("ambiguous",    "B2"), ("emphasize",   "B2"), ("interpret",   "B2"),

    # ── C1: Advanced (20 từ) ─────────────────
    ("ambivalent",  "C1"), ("benevolent",  "C1"), ("circumvent",  "C1"), ("denounce",    "C1"),
    ("eloquent",    "C1"), ("facilitate",  "C1"), ("hypothesis",  "C1"), ("implicate",   "C1"),
    ("juxtapose",   "C1"), ("meticulous",  "C1"), ("notorious",   "C1"), ("obsolete",    "C1"),
    ("paradox",     "C1"), ("rhetoric",    "C1"), ("scrutinize",  "C1"), ("tenacious",   "C1"),
    ("ubiquitous",  "C1"), ("vindicate",   "C1"), ("zealous",     "C1"), ("mitigate",    "C1"),
]

# Deduplicate giữ thứ tự
seen = set()
WORD_LIST_CLEAN = []
for item in WORD_LIST:
    if item[0] not in seen:
        seen.add(item[0])
        WORD_LIST_CLEAN.append(item)

# ─────────────────────────────────────────────
# HELPERS
# ─────────────────────────────────────────────

def escape_sql(text: str) -> str:
    """Escape chuỗi để dùng trong SQL, wrap bằng N'...' cho UTF-8."""
    if text is None:
        return "NULL"
    text = text.replace("'", "''")
    return f"N'{text}'"


def fetch_dictionary(word: str) -> dict:
    """
    Gọi dictionaryapi.dev để lấy:
    - pronunciation (text IPA)
    - pronunciation_url (audio mp3)
    - part_of_speech (string)
    - definition (English, dùng cho Description)
    - example (English, dùng cho ExampleSentence)
    """
    url = f"https://api.dictionaryapi.dev/api/v2/entries/en/{word}"
    try:
        resp = requests.get(url, timeout=10)
        if resp.status_code != 200:
            return {}
        data = resp.json()
        if not data or not isinstance(data, list):
            return {}

        entry = data[0]

        # Pronunciation text (IPA)
        pronunciation = entry.get("phonetic", "")
        if not pronunciation:
            for ph in entry.get("phonetics", []):
                if ph.get("text"):
                    pronunciation = ph["text"]
                    break

        # Pronunciation audio URL
        audio_url = ""
        for ph in entry.get("phonetics", []):
            if ph.get("audio"):
                audio_url = ph["audio"]
                if audio_url.startswith("//"):
                    audio_url = "https:" + audio_url
                break

        # Lấy meaning đầu tiên có definition
        part_of_speech = ""
        definition = ""
        example = ""

        for meaning in entry.get("meanings", []):
            pos = meaning.get("partOfSpeech", "")
            for defn in meaning.get("definitions", []):
                if defn.get("definition"):
                    part_of_speech = pos
                    definition = defn["definition"]
                    example = defn.get("example", "")
                    break
            if definition:
                break

        return {
            "pronunciation": pronunciation,
            "pronunciation_url": audio_url,
            "part_of_speech": part_of_speech,
            "definition": definition,
            "example": example,
        }

    except Exception as e:
        print(f"  [!] Dictionary API error for '{word}': {e}")
        return {}


def translate_word_to_vi(word: str) -> str:
    """
    Dịch trực tiếp từ tiếng Anh → nghĩa tiếng Việt.
    Ví dụ: "apple" → "táo", "run" → "chạy", "happy" → "hạnh phúc"
    """
    try:
        translated = GoogleTranslator(source="en", target="vi").translate(word)
        return translated or word
    except Exception as e:
        print(f"  [!] Translation error for '{word}': {e}")
        return word


def fetch_unsplash_image(query: str) -> str:
    """Lấy URL ảnh đầu tiên từ Unsplash theo từ khóa."""
    url = "https://api.unsplash.com/search/photos"
    params = {
        "query": query,
        "per_page": 1,
        "orientation": "landscape",
        "client_id": UNSPLASH_ACCESS_KEY,
    }
    try:
        resp = requests.get(url, params=params, timeout=10)
        if resp.status_code != 200:
            return ""
        data = resp.json()
        results = data.get("results", [])
        if not results:
            return ""
        # URL "regular" (~1080px) — stable Unsplash CDN URL
        return results[0]["urls"]["regular"]
    except Exception as e:
        print(f"  [!] Unsplash error for '{query}': {e}")
        return ""


def map_pos(pos_string: str):
    """Map partOfSpeech string → int enum."""
    return PART_OF_SPEECH_MAP.get(pos_string.lower().strip())


# ─────────────────────────────────────────────
# MAIN
# ─────────────────────────────────────────────

def main():
    print("=" * 60)
    print(" WordSoul - Vocabulary Seed Script")
    print(f" Words to process: {len(WORD_LIST_CLEAN)}")
    print("=" * 60)

    inserts = []

    for idx, (word, cefr) in enumerate(WORD_LIST_CLEAN, start=1):
        print(f"\n[{idx:03d}/{len(WORD_LIST_CLEAN)}] Processing: {word} ({cefr})")

        # 1️⃣  Dictionary data (pronunciation, definition EN, example EN)
        dict_data = fetch_dictionary(word)
        if not dict_data:
            print("  ⚠  No dictionary data, using minimal fallback.")

        pronunciation     = dict_data.get("pronunciation", "")
        pronunciation_url = dict_data.get("pronunciation_url", "")
        pos_string        = dict_data.get("part_of_speech", "noun")
        definition_en     = dict_data.get("definition", "")   # → Description
        example_en        = dict_data.get("example", "")      # → ExampleSentence

        # 2️⃣  Meaning = nghĩa tiếng Việt của từ (dịch trực tiếp TỪ → nghĩa)
        #     Ví dụ: "apple" → "táo", "run" → "chạy", "happy" → "hạnh phúc"
        print(f"  ↺  Translating word '{word}' → Vietnamese...")
        meaning_vi = translate_word_to_vi(word)
        print(f"  → Meaning: {meaning_vi}")
        time.sleep(0.5)

        # 3️⃣  Unsplash image
        print(f"  🖼  Fetching image...")
        image_url = fetch_unsplash_image(word)

        # 4️⃣  Build SQL values
        pos_int  = map_pos(pos_string)
        cefr_int = CEFR_INT_MAP.get(cefr, 0)

        col_values = {
            "Word":             escape_sql(word.capitalize()),
            "Meaning":          escape_sql(meaning_vi),                            # nghĩa tiếng Việt của từ
            "Pronunciation":    escape_sql(pronunciation) if pronunciation else "NULL",
            "PartOfSpeech":     str(pos_int) if pos_int is not None else "0",
            "CEFRLevel":        str(cefr_int),
            "Description":      escape_sql(definition_en) if definition_en else "NULL",  # định nghĩa tiếng Anh
            "ExampleSentence":  escape_sql(example_en)    if example_en    else "NULL",
            "ImageUrl":         escape_sql(image_url)     if image_url     else "NULL",
            "PronunciationUrl": escape_sql(pronunciation_url) if pronunciation_url else "NULL",
        }

        cols = ", ".join(col_values.keys())
        vals = ", ".join(col_values.values())
        inserts.append(f"INSERT INTO [Vocabularies] ({cols}) VALUES ({vals});")

        print(f"  ✅ OK")

        # Rate limiting: 1 giây/từ để tránh block
        time.sleep(1)

    # ─── Ghi file SQL ───────────────────────────────────────────
    with open(OUTPUT_FILE, "w", encoding="utf-8") as f:
        f.write("-- ============================================================\n")
        f.write("-- WordSoul - Vocabulary Seed Data\n")
        f.write(f"-- Generated: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}\n")
        f.write(f"-- Total words: {len(inserts)}\n")
        f.write("-- \n")
        f.write("-- HOW TO USE:\n")
        f.write("--   1. Mở SSMS → Connect Azure SQL\n")
        f.write("--   2. File → Open → File → chọn file này\n")
        f.write("--   3. Đổi [your_database_name] thành tên DB thật\n")
        f.write("--   4. Execute (F5)\n")
        f.write("-- ============================================================\n\n")

        f.write("USE [your_database_name]; -- ⚠ THAY TÊN DATABASE THẬT CỦA BẠN\n")
        f.write("GO\n\n")

        f.write("BEGIN TRANSACTION;\n")
        f.write("BEGIN TRY\n\n")

        for sql_stmt in inserts:
            f.write("    " + sql_stmt + "\n")

        f.write("\n    COMMIT TRANSACTION;\n")
        f.write(f"    PRINT N'✅ Seed thành công {len(inserts)} từ vựng!';\n")
        f.write("END TRY\n")
        f.write("BEGIN CATCH\n")
        f.write("    ROLLBACK TRANSACTION;\n")
        f.write("    PRINT N'❌ Lỗi: ' + ERROR_MESSAGE();\n")
        f.write("END CATCH\n")

    print("\n" + "=" * 60)
    print(f" ✅ Xong! {len(inserts)} từ được ghi vào '{OUTPUT_FILE}'")
    print(" 📋 Mở file SQL trong SSMS và execute để import.")
    print("=" * 60)


if __name__ == "__main__":
    main()
