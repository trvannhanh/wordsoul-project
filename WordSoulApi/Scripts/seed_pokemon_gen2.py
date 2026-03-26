"""
WordSoul - Pokemon Gen 2 Seed Script
======================================
Generates seed_pokemon_gen2.sql with 151 Gen 2 Pokemon for the Pets table.

Image URLs: pokemondb.net BW animated sprites (Black/White / B2W2)
No external API calls needed — all data is hardcoded for speed and accuracy.

Usage:
    cd Scripts
    python seed_pokemon_gen2.py

Output:
    seed_pokemon_gen2.sql
"""

from datetime import datetime

OUTPUT_FILE = "seed_pokemon_gen2.sql"

# ─────────────────────────────────────────────────────────────────────────────
# ENUM VALUES  (must match C# enums exactly)
# ─────────────────────────────────────────────────────────────────────────────
# PetRarity : Common=0, Uncommon=1, Rare=2, Epic=3, Legendary=4
# PetType   : Normal=0, Fire=1, Water=2, Electric=3, Grass=4, Ice=5,
#             Fighting=6, Poison=7, Ground=8, Flying=9, Psychic=10,
#             Bug=11, Rock=12, Ghost=13, Dragon=14, Dark=15, Steel=16, Fairy=17

# ─────────────────────────────────────────────────────────────────────────────
# POKEMON DATA
# Columns: (id, name, db_name, type_int, rarity_int,
#            base_form_id, next_evo_id, required_level, description)
#
# db_name     : used to build imageUrl  →  https://img.pokemondb.net/sprites/black-white/anim/normal/{db_name}.gif
# base_form_id: pet.Id of the first-stage form (NULL if this IS the base form)
# next_evo_id : pet.Id it evolves into  (NULL if final form / branching)
# required_level: level at which THIS form was obtained from previous (NULL = stone/trade/branch)
# ─────────────────────────────────────────────────────────────────────────────
POKEMON = [

# ── STARTERS ─────────────────────────────────────────

(152,"Chikorita","chikorita",4,1,None,153,16,"A sweet aroma gently wafts from the leaf on its head."),
(153,"Bayleef","bayleef",4,1,152,154,32,"The scent of spices comes from around its neck."),
(154,"Meganium","meganium",4,3,152,None,None,"The aroma from its petals contains a substance that calms aggressive feelings."),

(155,"Cyndaquil","cyndaquil",1,1,None,156,14,"It usually stays hunched over. If it is angry, its back erupts with flames."),
(156,"Quilava","quilava",1,1,155,157,36,"Before battle, it turns its back on the opponent to demonstrate how ferociously its fire blazes."),
(157,"Typhlosion","typhlosion",1,3,155,None,None,"It has a secret, devastating move. It rubs its blazing fur together to cause huge explosions."),

(158,"Totodile","totodile",2,1,None,159,18,"Its well-developed jaws are powerful and capable of crushing anything."),
(159,"Croconaw","croconaw",2,1,158,160,30,"Once it bites something, it won't let go until it loses its fangs."),
(160,"Feraligatr","feraligatr",2,3,158,None,None,"When it bites with its massive and powerful jaws, it shakes its head and savagely tears its victim up."),

# ── EARLY ROUTE ─────────────────────────────────────

(161,"Sentret","sentret",0,0,None,162,15,"It stands on its tail so it can see a long way."),
(162,"Furret","furret",0,0,161,None,None,"It lives in narrow burrows that fit its slim body."),

(163,"Hoothoot","hoothoot",9,0,None,164,20,"It always stands on one foot."),
(164,"Noctowl","noctowl",9,1,163,None,None,"Its eyes are specially adapted. They concentrate even faint light."),

(165,"Ledyba","ledyba",11,0,None,166,18,"It is timid and clusters together with others."),
(166,"Ledian","ledian",11,1,165,None,None,"It uses starlight as energy."),

(167,"Spinarak","spinarak",11,0,None,168,22,"It spins webs using fine threads."),
(168,"Ariados","ariados",11,1,167,None,None,"It binds its prey with string-like silk."),

# ── UNIQUE ──────────────────────────────────────────

(169,"Crobat","crobat",7,2,41,None,None,"It flies silently through the night."),

(170,"Chinchou","chinchou",2,1,None,171,27,"It uses positive and negative electricity."),
(171,"Lanturn","lanturn",2,2,170,None,None,"The light it emits is so bright."),

(172,"Pichu","pichu",3,1,None,25,None,"It plays with electricity."),

(173,"Cleffa","cleffa",0,1,None,35,None,"Because of its unusual star-like silhouette."),

(174,"Igglybuff","igglybuff",0,1,None,39,None,"It has a very soft body."),

(175,"Togepi","togepi",0,2,None,176,None,"The shell seems to be filled with joy."),
(176,"Togetic","togetic",0,2,175,None,None,"They say that it will appear before kindhearted people."),

(177,"Natu","natu",10,1,None,178,25,"It is extremely good at climbing trees."),
(178,"Xatu","xatu",10,2,177,None,None,"It is said to stay still and watch the sun all day."),

# ── GRASS / BUG MIX ─────────────────────────────────

(179,"Mareep","mareep",3,1,None,180,15,"Its wool rubs together and builds static electricity."),
(180,"Flaaffy","flaaffy",3,1,179,181,30,"Its fluffy wool stores electricity."),
(181,"Ampharos","ampharos",3,3,179,None,None,"The tip of its tail shines brightly."),

(182,"Bellossom","bellossom",4,2,43,None,None,"Plentiful in the tropics."),

(183,"Marill","marill",2,1,None,184,18,"The tip of its tail is filled with oil."),
(184,"Azumarill","azumarill",2,2,183,None,None,"It lives in water almost all day."),

(185,"Sudowoodo","sudowoodo",12,1,None,None,None,"Although it always pretends to be a tree."),

(186,"Politoed","politoed",2,2,60,None,None,"The curled hair on its head is proof."),

(187,"Hoppip","hoppip",4,0,None,188,18,"It drifts on the wind."),
(188,"Skiploom","skiploom",4,0,187,189,27,"The bloom on top opens and closes."),
(189,"Jumpluff","jumpluff",4,1,187,None,None,"Once it catches the wind."),

# ── MORE ────────────────────────────────────────────

(190,"Aipom","aipom",0,1,None,None,None,"It lives atop tall trees."),

(191,"Sunkern","sunkern",4,0,None,192,14,"It tries to move as little as possible."),
(192,"Sunflora","sunflora",4,1,191,None,None,"It converts sunlight into energy."),

(193,"Yanma","yanma",11,1,None,None,None,"It can see 360 degrees."),

(194,"Wooper","wooper",2,0,None,195,20,"This carefree Pokémon has an easygoing nature."),
(195,"Quagsire","quagsire",2,1,194,None,None,"It has a sluggish nature."),

(196,"Espeon","espeon",10,3,133,None,None,"It uses the fine hair that covers its body."),
(197,"Umbreon","umbreon",15,3,133,None,None,"When darkness falls, the rings begin to glow."),

(198,"Murkrow","murkrow",15,1,None,None,None,"Feared and loathed by many."),

(199,"Slowking","slowking",2,3,79,None,None,"Every time it yawns, Shellder injects more poison."),

(200,"Misdreavus","misdreavus",13,1,None,None,None,"It loves to bite and yank people's hair."),

(201,"Unown","unown",10,1,None,None,None,"Their shapes look like hieroglyphs."),

(202,"Wobbuffet","wobbuffet",10,2,360,None,None,"It hates light and shock."),

(203,"Girafarig","girafarig",0,1,None,None,None,"Its tail has a small brain."),

(204,"Pineco","pineco",11,0,None,205,31,"It hangs from a tree branch."),
(205,"Forretress","forretress",11,2,204,None,None,"Its entire body is shielded by a steel-hard shell."),

(206,"Dunsparce","dunsparce",0,1,None,None,None,"It hides deep inside caves."),

(207,"Gligar","gligar",8,1,None,None,None,"It glides without a sound."),

(208,"Steelix","steelix",16,3,95,None,None,"Its body has been compressed deep underground."),

(209,"Snubbull","snubbull",0,1,None,210,23,"Although it looks frightening."),
(210,"Granbull","granbull",0,2,209,None,None,"It is actually timid."),

(211,"Qwilfish","qwilfish",2,1,None,None,None,"To fire its poison spikes."),

(212,"Scizor","scizor",11,3,123,None,None,"It swings its eye-patterned pincers."),

(213,"Shuckle","shuckle",12,2,None,None,None,"The berries it stores turn into juice."),

(214,"Heracross","heracross",6,2,None,None,None,"With its Herculean powers."),

(215,"Sneasel","sneasel",15,2,None,None,None,"It is extremely vicious."),

(216,"Teddiursa","teddiursa",0,1,None,217,30,"It licks its paws."),
(217,"Ursaring","ursaring",0,2,216,None,None,"It is said to be a good swimmer."),

(218,"Slugma","slugma",1,0,None,219,38,"A common sight in volcanic areas."),
(219,"Magcargo","magcargo",1,1,218,None,None,"Its shell is made of hardened magma."),

(220,"Swinub","swinub",5,0,None,221,33,"It rubs its snout on the ground."),
(221,"Piloswine","piloswine",5,1,220,None,None,"Because the long hair all over its body obscures its sight."),

(222,"Corsola","corsola",2,1,None,None,None,"They live in warm southern seas."),

(223,"Remoraid","remoraid",2,0,None,224,25,"It shoots water out of its mouth."),
(224,"Octillery","octillery",2,1,223,None,None,"It traps enemies with its suction-cupped tentacles."),

(225,"Delibird","delibird",5,1,None,None,None,"It carries food all day long."),

(226,"Mantine","mantine",2,2,458,None,None,"As it majestically swims."),

(227,"Skarmory","skarmory",16,2,None,None,None,"Its body is encased in a sturdy shell of steel."),

(228,"Houndour","houndour",15,1,None,229,24,"It uses different kinds of cries."),
(229,"Houndoom","houndoom",15,2,228,None,None,"If you are burned by the flames."),

(230,"Kingdra","kingdra",2,3,117,None,None,"It is said that it sleeps on the seafloor."),

(231,"Phanpy","phanpy",8,1,None,232,25,"It is strong despite its compact size."),
(232,"Donphan","donphan",8,2,231,None,None,"It has sharp, hard tusks."),

(233,"Porygon2","porygon2",0,3,137,None,None,"This upgraded version of Porygon."),

(234,"Stantler","stantler",0,1,None,None,None,"Those who stare at its antlers."),

(235,"Smeargle","smeargle",0,2,None,None,None,"It marks its territory using a fluid."),

(236,"Tyrogue","tyrogue",6,1,None,None,20,"It is always bursting with energy."),

(237,"Hitmontop","hitmontop",6,2,236,None,None,"If you become enchanted by its smooth."),

(238,"Smoochum","smoochum",10,1,None,124,None,"It always rocks its head slowly."),

(239,"Elekid","elekid",3,1,None,125,None,"It generates electricity."),

(240,"Magby","magby",1,1,None,126,None,"Each and every time it inhales."),

(241,"Miltank","miltank",0,2,None,None,None,"Its milk is packed with nutrition."),

(242,"Blissey","blissey",0,3,113,None,None,"Anyone who takes even one bite."),

# ── LEGENDARIES ─────────────────────────────────────

(243,"Raikou","raikou",3,4,None,None,None,"The rain clouds it carries."),
(244,"Entei","entei",1,4,None,None,None,"A Pokemon that races across the land."),
(245,"Suicune","suicune",2,4,None,None,None,"Said to be the embodiment of north winds."),

(246,"Larvitar","larvitar",12,1,None,247,30,"It feeds on soil."),
(247,"Pupitar","pupitar",12,2,246,248,55,"Its shell is as hard as bedrock."),
(248,"Tyranitar","tyranitar",12,3,246,None,None,"Its body can't be harmed by any sort of attack."),

(249,"Lugia","lugia",10,4,None,None,None,"It sleeps in a deep-sea trench."),
(250,"Ho-oh","ho-oh",1,4,None,None,None,"Legends claim this Pokemon flies the world's skies."),
(251,"Celebi","celebi",10,4,None,None,None,"It has the power to travel across time."),

]

# ─────────────────────────────────────────────────────────────────────────────
# HELPERS
# ─────────────────────────────────────────────────────────────────────────────

def q(val):
    """Escape a string value for SQL — returns N'...' or NULL."""
    if val is None:
        return "NULL"
    return "N'" + str(val).replace("'", "''") + "'"

def v(val):
    """Return numeric value or NULL."""
    return str(val) if val is not None else "NULL"

def image_url(db_name: str) -> str:
    return f"https://img.pokemondb.net/sprites/black-white/anim/normal/{db_name}.gif"


# ─────────────────────────────────────────────────────────────────────────────
# MAIN
# ─────────────────────────────────────────────────────────────────────────────

def main():
    print("=" * 60)
    print(" WordSoul - Pokemon Gen 2 Seed Script")
    print(f" Total: {len(POKEMON)} Pokemon")
    print("=" * 60)

    lines = []
    for row in POKEMON:
        (pid, name, db_name, type_int, rarity_int,
         base_id, next_id, req_lvl, description) = row

        img = image_url(db_name)
        created = "GETUTCDATE()"

        line = (
            f"    INSERT INTO [Pets] "
            f"([Id],[Name],[Description],[ImageUrl],[Rarity],[Type],[Order],"
            f"[BaseFormId],[NextEvolutionId],[RequiredLevel],[CreatedAt],[IsActive]) "
            f"VALUES "
            f"({pid},{q(name)},{q(description)},{q(img)},{rarity_int},{type_int},{pid},"
            f"{v(base_id)},{v(next_id)},{v(req_lvl)},{created},1);"
        )
        lines.append(line)
        print(f"  [{pid:03d}] {name} | Type={type_int} Rarity={rarity_int}")

    with open(OUTPUT_FILE, "w", encoding="utf-8") as f:
        f.write("-- ============================================================\n")
        f.write("-- WordSoul - Pokemon Gen 2 Seed Data (151 Pokemon)\n")
        f.write(f"-- Generated: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}\n")
        f.write("-- Images: pokemondb.net Black/White animated sprites\n")
        f.write("--\n")
        f.write("-- HOW TO USE:\n")
        f.write("--   1. Mở SSMS → Connect Azure SQL\n")
        f.write("--   2. File → Open → chọn file này\n")
        f.write("--   3. Đổi [your_database_name] thành tên DB thật\n")
        f.write("--   4. Execute (F5)\n")
        f.write("-- ============================================================\n\n")
        f.write("USE [your_database_name]; -- ⚠ THAY TÊN DB THẬT\n")
        f.write("GO\n\n")
        f.write("SET IDENTITY_INSERT [Pets] ON;\n\n")
        f.write("BEGIN TRANSACTION;\n")
        f.write("BEGIN TRY\n\n")
        f.write("\n".join(lines))
        f.write("\n\n    COMMIT TRANSACTION;\n")
        f.write(f"    PRINT N'[OK] Seed thanh cong {len(POKEMON)} Pokemon!';\n")
        f.write("END TRY\n")
        f.write("BEGIN CATCH\n")
        f.write("    ROLLBACK TRANSACTION;\n")
        f.write("    SET IDENTITY_INSERT [Pets] OFF;\n")
        f.write("    PRINT N'[ERR] Loi: ' + ERROR_MESSAGE();\n")
        f.write("END CATCH\n\n")
        f.write("SET IDENTITY_INSERT [Pets] OFF;\n")

    print("\n" + "=" * 60)
    print(f" [OK] Xong! File '{OUTPUT_FILE}' da duoc tao.")
    print("=" * 60)


if __name__ == "__main__":
    main()
