"""
WordSoul - Pokemon Gen 1 Seed Script
======================================
Generates seed_pokemon.sql with 151 Gen 1 Pokemon for the Pets table.

Image URLs: pokemondb.net BW animated sprites (Black/White / B2W2)
No external API calls needed — all data is hardcoded for speed and accuracy.

Usage:
    cd Scripts
    python seed_pokemon_gen1.py

Output:
    seed_pokemon_gen1.sql
"""

from datetime import datetime

OUTPUT_FILE = "seed_pokemon_gen1.sql"

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
    # ── Bulbasaur line ──────────────────────────────────────────────────────
    (1,  "Bulbasaur",  "bulbasaur",  4,  1, None, 2,    16,   "A strange seed was planted on its back at birth."),
    (2,  "Ivysaur",    "ivysaur",    4,  1, 1,    3,    32,   "The bulb on its back grows by drawing energy from sunlight."),
    (3,  "Venusaur",   "venusaur",   4,  3, 1,    None, None, "Its plant blooms when it absorbs solar energy, filling the air with a soothing scent."),
    # ── Charmander line ─────────────────────────────────────────────────────
    (4,  "Charmander", "charmander", 1,  1, None, 5,    16,   "The flame on its tail indicates its life force. If healthy its tail burns intensely."),
    (5,  "Charmeleon", "charmeleon", 1,  1, 4,    6,    36,   "When it swings its burning tail intensely the air around it gets very hot."),
    (6,  "Charizard",  "charizard",  1,  3, 4,    None, None, "It spits fire hot enough to melt boulders and causes forest fires unintentionally."),
    # ── Squirtle line ───────────────────────────────────────────────────────
    (7,  "Squirtle",   "squirtle",   2,  1, None, 8,    16,   "When it retracts its long neck into its shell it squirts water with great force."),
    (8,  "Wartortle",  "wartortle",  2,  1, 7,    9,    36,   "Its fur-covered tail symbolizes longevity. It is a popular symbol of good health."),
    (9,  "Blastoise",  "blastoise",  2,  3, 7,    None, None, "It crushes its foes with powerful jets of water from the cannons on its back."),
    # ── Caterpie line ───────────────────────────────────────────────────────
    (10, "Caterpie",   "caterpie",   11, 0, None, 11,   7,    "Its short feet are tipped with suction pads that enable it to tirelessly climb slopes."),
    (11, "Metapod",    "metapod",    11, 0, 10,   12,   10,   "Inside its tough shell it quietly endures hardship as it prepares to evolve."),
    (12, "Butterfree", "butterfree", 11, 2, 10,   None, None, "It collects honey daily. It can even gather pollen faster than a bee."),
    # ── Weedle line ─────────────────────────────────────────────────────────
    (13, "Weedle",     "weedle",     11, 0, None, 14,   7,    "Often found in forests, it has a sharp, venomous stinger on its head."),
    (14, "Kakuna",     "kakuna",     11, 0, 13,   15,   10,   "Almost incapable of moving, this Pokemon can only harden its shell to protect itself."),
    (15, "Beedrill",   "beedrill",   11, 2, 13,   None, None, "It has three poison stingers on its forelegs and tail. It targets the eyes first."),
    # ── Pidgey line ─────────────────────────────────────────────────────────
    (16, "Pidgey",     "pidgey",     9,  0, None, 17,   18,   "A common sight in forests and cities. It hides in tall grass and rarely fights back."),
    (17, "Pidgeotto",  "pidgeotto",  9,  0, 16,   18,   36,   "This Pokemon flies at Mach 2 speed, seeking prey and using its talons to grab them."),
    (18, "Pidgeot",    "pidgeot",    9,  2, 16,   None, None, "When it flies at high speed, the beautiful iridescence of its wings mesmerizes foes."),
    # ── Rattata line ────────────────────────────────────────────────────────
    (19, "Rattata",    "rattata",    0,  0, None, 20,   20,   "Will chew on anything with its fangs. If you see one, many more live in the area."),
    (20, "Raticate",   "raticate",   0,  0, 19,   None, None, "Its whiskers are extremely sensitive to air currents — it can tell what's around it."),
    # ── Spearow line ────────────────────────────────────────────────────────
    (21, "Spearow",    "spearow",    9,  0, None, 22,   20,   "Inept at flying high. However it can fly around very fast to protect its territory."),
    (22, "Fearow",     "fearow",     9,  0, 21,   None, None, "With its huge and magnificent wings it can keep aloft without resting for a day."),
    # ── Ekans line ──────────────────────────────────────────────────────────
    (23, "Ekans",      "ekans",      7,  0, None, 24,   22,   "Moves silently and stealthily. Eats the eggs of birds such as Spearow whole."),
    (24, "Arbok",      "arbok",      7,  0, 23,   None, None, "It is rumored that the ferocious warning pattern on its belly differs in six regions."),
    # ── Pikachu line ────────────────────────────────────────────────────────
    (25, "Pikachu",    "pikachu",    3,  2, None, 26,   None, "When several of these Pokemon gather, their electricity can build and cause lightning storms."),
    (26, "Raichu",     "raichu",     3,  2, 25,   None, None, "Its tail serves as a ground to protect itself from its own high-voltage zaps."),
    # ── Sandshrew line ──────────────────────────────────────────────────────
    (27, "Sandshrew",  "sandshrew",  8,  0, None, 28,   22,   "Burrows deep underground in arid desert areas. Surfaces only to hunt for food."),
    (28, "Sandslash",  "sandslash",  8,  0, 27,   None, None, "Curls up into a spiny ball when threatened. Its sharp claws are used to dig burrows."),
    # ── Nidoran-F line ──────────────────────────────────────────────────────
    (29, "Nidoran F",  "nidoran-f",  7,  0, None, 30,   16,   "Although small its venomous barbs render it dangerous. The female has smaller horns."),
    (30, "Nidorina",   "nidorina",   7,  0, 29,   31,   None, "The female raises its rough hide when angry. It uses its small horns to battle."),
    (31, "Nidoqueen",  "nidoqueen",  7,  1, 29,   None, None, "Its body is encased in rock-hard scales. The hard scales provide strong protection."),
    # ── Nidoran-M line ──────────────────────────────────────────────────────
    (32, "Nidoran M",  "nidoran-m",  7,  0, None, 33,   16,   "Stiffens its ears to sense danger. The larger its horns the more powerful its venom."),
    (33, "Nidorino",   "nidorino",   7,  0, 32,   34,   None, "An aggressive Pokemon that is quick to attack. The horn can inject devastating venom."),
    (34, "Nidoking",   "nidoking",   7,  1, 32,   None, None, "It uses its powerful tail in battle to topple telephone poles and trees with ease."),
    # ── Clefairy line ───────────────────────────────────────────────────────
    (35, "Clefairy",   "clefairy",   0,  1, None, 36,   None, "Widely believed to have come from outer space. They gather on moonlit nights."),
    (36, "Clefable",   "clefable",   0,  1, 35,   None, None, "A timid fairy Pokemon that is rarely seen. It will run and hide the moment it senses people."),
    # ── Vulpix line ─────────────────────────────────────────────────────────
    (37, "Vulpix",     "vulpix",     1,  1, None, 38,   None, "At birth it has just one tail. The tail splits from its tip as it grows older."),
    (38, "Ninetales",  "ninetales",  1,  2, 37,   None, None, "Very smart and very vengeful. Grabbing one of its tails could result in a 1000-year curse."),
    # ── Jigglypuff line ─────────────────────────────────────────────────────
    (39, "Jigglypuff", "jigglypuff", 0,  1, None, 40,   None, "When its huge eyes light up it sings a mysteriously soothing melody that lulls enemies to sleep."),
    (40, "Wigglytuff", "wigglytuff", 0,  1, 39,   None, None, "The body is soft and rubbery. When angered it will suck in air and inflate itself to an enormous size."),
    # ── Zubat line ──────────────────────────────────────────────────────────
    (41, "Zubat",      "zubat",      7,  0, None, 42,   22,   "Forms colonies in perpetually dark places. Uses ultrasonic waves to identify and approach targets."),
    (42, "Golbat",     "golbat",     7,  0, 41,   None, None, "Once it bites its victim it will not stop draining energy from the victim even if it gets too heavy to fly."),
    # ── Oddish line ─────────────────────────────────────────────────────────
    (43, "Oddish",     "oddish",     4,  1, None, 44,   21,   "During the day it keeps its face buried in the ground. At night it roams about and scatters its seeds."),
    (44, "Gloom",      "gloom",      4,  1, 43,   45,   None, "The petals are said to bring about feelings of happiness when smelled. Its pistil oozes a sticky nectar."),
    (45, "Vileplume",  "vileplume",  4,  2, 43,   None, None, "It has the world's largest petals. They are stored with toxic pollen which it scatters on approach."),
    # ── Paras line ──────────────────────────────────────────────────────────
    (46, "Paras",      "paras",      11, 0, None, 47,   24,   "Burrows to suck tree roots. Mushrooms named tochukaso grow on its back and absorb the host."),
    (47, "Parasect",   "parasect",   11, 0, 46,   None, None, "A host-parasite pair where the parasite controls the host. Spores spread from its mushroom on its back."),
    # ── Venonat line ────────────────────────────────────────────────────────
    (48, "Venonat",    "venonat",    11, 0, None, 49,   31,   "Lives in the shadows of tall trees where it eats insects. It is attracted by light."),
    (49, "Venomoth",   "venomoth",   11, 0, 48,   None, None, "The dust-like scales covering its wings are color coded to identify the types of poison it has."),
    # ── Diglett line ────────────────────────────────────────────────────────
    (50, "Diglett",    "diglett",    8,  0, None, 51,   26,   "A Pokemon that lives underground. It burrows through the ground at a shallow depth."),
    (51, "Dugtrio",    "dugtrio",    8,  0, 50,   None, None, "A team of Diglett triplets. It triggers huge earthquakes by burrowing 60 miles underground."),
    # ── Meowth line ─────────────────────────────────────────────────────────
    (52, "Meowth",     "meowth",     0,  0, None, 53,   28,   "Extremely greedy it sees shiny things and instantly swipes them. It hides its collection."),
    (53, "Persian",    "persian",    0,  0, 52,   None, None, "Although its fur has many admirers it is tough to raise as a pet due to its fickle meanness."),
    # ── Psyduck line ────────────────────────────────────────────────────────
    (54, "Psyduck",    "psyduck",    2,  1, None, 55,   33,   "While lulling its enemies with its vacant look it makes them sustain damage by blasting them with psychic waves."),
    (55, "Golduck",    "golduck",    2,  1, 54,   None, None, "Often seen swimming elegantly by lakesides. It is often mistaken for the Japanese Kappa."),
    # ── Mankey line ─────────────────────────────────────────────────────────
    (56, "Mankey",     "mankey",     6,  0, None, 57,   28,   "Extremely quick to anger. It takes only a slight provocation to send it into a frenzy."),
    (57, "Primeape",   "primeape",   6,  0, 56,   None, None, "Always furious and rampaging it may snap a neck or two while trembling with rage."),
    # ── Growlithe line ──────────────────────────────────────────────────────
    (58, "Growlithe",  "growlithe",  1,  1, None, 59,   None, "Very protective of its territory. It will bark and bite to keep intruders away."),
    (59, "Arcanine",   "arcanine",   1,  2, 58,   None, None, "A Pokemon that has long been admired for its beauty. It runs agilely as if on wings."),
    # ── Poliwag line ────────────────────────────────────────────────────────
    (60, "Poliwag",    "poliwag",    2,  0, None, 61,   25,   "Its newly grown legs prevent it from running. It appears to prefer swimming over everything."),
    (61, "Poliwhirl",  "poliwhirl", 2,  0, 60,   62,   None, "Capable of living in or out of water. When out of water it sweats to keep its body slimy."),
    (62, "Poliwrath",  "poliwrath",  2,  2, 60,   None, None, "An adept swimmer at both the front crawl and breaststroke. Easily overtakes Olympic swimmers."),
    # ── Abra line ───────────────────────────────────────────────────────────
    (63, "Abra",       "abra",       10, 1, None, 64,   16,   "Using its ability to read minds it will identify impending danger and teleport to safety."),
    (64, "Kadabra",    "kadabra",    10, 1, 63,   65,   None, "It emits special alpha waves from its body that induce headaches just by being close by."),
    (65, "Alakazam",   "alakazam",   10, 2, 63,   None, None, "Its brain can outperform a supercomputer. Its IQ is said to be around 5000."),
    # ── Machop line ─────────────────────────────────────────────────────────
    (66, "Machop",     "machop",     6,  1, None, 67,   28,   "Loves to build its muscles. It trains by lifting boulders in gravelly mountain ranges."),
    (67, "Machoke",    "machoke",    6,  1, 66,   68,   None, "Its muscular body is so powerful it must wear a power-save belt to regulate its power."),
    (68, "Machamp",    "machamp",    6,  3, 66,   None, None, "It quickly swings its four arms to rock its opponents with a flurry of punches and chops."),
    # ── Bellsprout line ─────────────────────────────────────────────────────
    (69, "Bellsprout", "bellsprout", 4,  1, None, 70,   21,   "A carnivorous Pokemon that traps and absorbs insects. Its thin body can whip rapidly."),
    (70, "Weepinbell", "weepinbell", 4,  1, 69,   71,   None, "It looks like a plant but it is able to move around. It catches bugs by surprising them."),
    (71, "Victreebel", "victreebel", 4,  2, 69,   None, None, "Said to live in huge colonies deep in jungles where it swallows anything that approaches."),
    # ── Tentacool line ──────────────────────────────────────────────────────
    (72, "Tentacool",  "tentacool",  2,  0, None, 73,   30,   "Drifts in shallow seas. Absorbs sunlight to convert it into beam energy."),
    (73, "Tentacruel", "tentacruel", 2,  0, 72,   None, None, "The tentacles can paralyze prey with venom. It has ninety-six and can shoot venom from all of them."),
    # ── Geodude line ────────────────────────────────────────────────────────
    (74, "Geodude",    "geodude",    12, 0, None, 75,   25,   "Found in fields and mountains. Mistaking them for boulders people often step on them."),
    (75, "Graveler",   "graveler",   12, 0, 74,   76,   None, "Rolls down slopes to move. It rolls over any obstacle without slowing or changing course."),
    (76, "Golem",      "golem",      12, 2, 74,   None, None, "Its boulder-like body is extremely hard. It sheds its skin once a year."),
    # ── Ponyta line ─────────────────────────────────────────────────────────
    (77, "Ponyta",     "ponyta",     1,  1, None, 78,   40,   "Its legs grow strong while it runs around. It runs in fields and mountains from morning to night."),
    (78, "Rapidash",   "rapidash",   1,  2, 77,   None, None, "Very competitive and loves to race. It will chase anything that moves fast in hopes of racing."),
    # ── Slowpoke line ───────────────────────────────────────────────────────
    (79, "Slowpoke",   "slowpoke",   2,  1, None, 80,   37,   "Incredibly slow and dopey. It takes 5 seconds to feel pain when under attack."),
    (80, "Slowbro",    "slowbro",    2,  2, 79,   None, None, "The Shellder that is latched onto its tail is said to feed on Slowbro's left-over scraps."),
    # ── Magnemite line ──────────────────────────────────────────────────────
    (81, "Magnemite",  "magnemite",  3,  1, None, 82,   30,   "Uses anti-gravity to stay suspended. Electro waves from thunderheads trigger it to swarm."),
    (82, "Magneton",   "magneton",   3,  1, 81,   None, None, "Formed by three Magnemite linked together. Nearby electronics are affected by its magnetism."),
    # ── Singles ─────────────────────────────────────────────────────────────
    (83, "Farfetchd",  "farfetchd",  0,  2, None, None, None, "The stalk it holds is made from a plant grown specially in most places. Always will be holding it."),
    # ── Doduo line ──────────────────────────────────────────────────────────
    (84, "Doduo",      "doduo",      9,  0, None, 85,   31,   "A bird that makes up for its poor flying with its fast foot speed. Leaves giant footprints."),
    (85, "Dodrio",     "dodrio",     9,  0, 84,   None, None, "Uses its three brains to execute complex plans. With three heads it can think three times as fast."),
    # ── Seel line ───────────────────────────────────────────────────────────
    (86, "Seel",       "seel",       2,  0, None, 87,   34,   "The protruding horn on its forehead is very hard. It is used for bashing through thick ice."),
    (87, "Dewgong",    "dewgong",    2,  0, 86,   None, None, "Loves to play in cold waters. Stores thermal energy in its body. Becomes more active in cold weather."),
    # ── Grimer line ─────────────────────────────────────────────────────────
    (88, "Grimer",     "grimer",     7,  0, None, 89,   38,   "Appears in filthy areas. Thrives by sucking up polluted sludge that is pumped out of factories."),
    (89, "Muk",        "muk",        7,  0, 88,   None, None, "Absorbing filth causes it to grow. They group together in dirty cities and sewage plants."),
    # ── Shellder line ───────────────────────────────────────────────────────
    (90, "Shellder",   "shellder",   2,  0, None, 91,   None, "Its shell is harder than diamond. Inside is a pearl found nowhere else. Spits pearls at enemies."),
    (91, "Cloyster",   "cloyster",   2,  2, 90,   None, None, "Once it slams its shell shut it is impossible to open even with great force."),
    # ── Gastly line ─────────────────────────────────────────────────────────
    (92, "Gastly",     "gastly",     13, 1, None, 93,   25,   "Almost invisible when underground it floats upward as night falls. It will curse anyone who touches it."),
    (93, "Haunter",    "haunter",    13, 1, 92,   94,   None, "Because of its ability to slip through block walls it is said to be from another dimension."),
    (94, "Gengar",     "gengar",     13, 3, 92,   None, None, "To steal the warmth of a person it will enter the room of a healthy person and hang in the shadow."),
    # ── Singles ─────────────────────────────────────────────────────────────
    (95, "Onix",       "onix",       12, 1, None, None, None, "As it grows the stone portions of its body harden to become similar to a diamond — but colored black."),
    # ── Drowzee line ────────────────────────────────────────────────────────
    (96, "Drowzee",    "drowzee",    10, 1, None, 97,   26,   "Puts enemies to sleep then eats their dreams. Occasionally gets sick from eating bad dreams."),
    (97, "Hypno",      "hypno",      10, 1, 96,   None, None, "When it locks eyes with an enemy it will use a mix of PSI moves such as Hypnosis and Confusion."),
    # ── Krabby line ─────────────────────────────────────────────────────────
    (98, "Krabby",     "krabby",     2,  0, None, 99,   28,   "Its pincers are not only powerful weapons they are used for balance when walking sideways."),
    (99, "Kingler",    "kingler",    2,  0, 98,   None, None, "The larger pincer has 10000 HP of crushing power. However its huge claw makes it hard to aim."),
    # ── Voltorb line ─────────────────────────────────────────────────────────
    (100,"Voltorb",    "voltorb",    3,  0, None, 101,  30,   "Usually found in power plants. Explodes with little provocation. Considered a problem."),
    (101,"Electrode",  "electrode",  3,  0, 100,  None, None, "Explodes if bored. Stores electricity in its body and will become unstable and explode."),
    # ── Exeggcute line ──────────────────────────────────────────────────────
    (102,"Exeggcute",  "exeggcute",  4,  1, None, 103,  None, "Often mistaken for eggs. When disturbed they quickly gather and attack in swarms."),
    (103,"Exeggutor",  "exeggutor",  4,  2, 102,  None, None, "Its three heads think independently. However they are friendly and never argue."),
    # ── Cubone line ─────────────────────────────────────────────────────────
    (104,"Cubone",     "cubone",     8,  1, None, 105,  28,   "Wears the skull of its deceased mother. Its cries echo inside the skull and come out as a sad melody."),
    (105,"Marowak",    "marowak",    8,  1, 104,  None, None, "The bone it holds is its key weapon. It throws the bone skillfully like a boomerang."),
    # ── Hitmons ─────────────────────────────────────────────────────────────
    (106,"Hitmonlee",  "hitmonlee",  6,  2, None, None, None, "When in a hurry its legs lengthen progressively. It can reach a fleeing foe in an instant."),
    (107,"Hitmonchan", "hitmonchan", 6,  2, None, None, None, "Said to possess the spirit of a boxer who had been working toward a world championship."),
    # ── Singles ─────────────────────────────────────────────────────────────
    (108,"Lickitung",  "lickitung",  0,  2, None, None, None, "Its tongue spans 6.5 feet and moves more freely than its forelegs. Its saliva melts anything organic."),
    # ── Koffing line ─────────────────────────────────────────────────────────
    (109,"Koffing",    "koffing",    7,  1, None, 110,  35,   "Because it stores several kinds of toxic gases in its body it is prone to exploding without warning."),
    (110,"Weezing",    "weezing",    7,  1, 109,  None, None, "Where two kinds of poison gases meet two Koffing can fuse over many years into one Pokemon."),
    # ── Rhyhorn line ─────────────────────────────────────────────────────────
    (111,"Rhyhorn",    "rhyhorn",    8,  0, None, 112,  42,   "Its massive bones are 1000 times harder than human bones. It can easily knock a trailer flying."),
    (112,"Rhydon",     "rhydon",     8,  2, 111,  None, None, "Protected by an armor-like hide it is capable of living in molten lava of 3600 degrees."),
    # ── Singles ──────────────────────────────────────────────────────────────
    (113,"Chansey",    "chansey",    0,  2, None, None, None, "A rare and kind-hearted Pokemon that shares its nutritious eggs generously."),
    (114,"Tangela",    "tangela",    4,  2, None, None, None, "The whole body is wrapped in blue plant vines. The vines sway as it walks."),
    (115,"Kangaskhan", "kangaskhan", 0,  2, None, None, None, "The young one in its pouch peers out to learn more about the world with its curious eyes."),
    # ── Horsea line ──────────────────────────────────────────────────────────
    (116,"Horsea",     "horsea",     2,  0, None, 117,  32,   "Capable of swimming swiftly by cleverly flapping its fan-like tail fins."),
    (117,"Seadra",     "seadra",     2,  0, 116,  None, None, "Capable of swimming backwards by rapidly flapping its wing-like pectoral fins and stout tail."),
    # ── Goldeen line ────────────────────────────────────────────────────────
    (118,"Goldeen",    "goldeen",    2,  0, None, 119,  33,   "It is a well-known swimmer. Its large and elegant fins billow like expressive skirts."),
    (119,"Seaking",    "seaking",    2,  0, 118,  None, None, "In autumn its body becomes more fatty in preparing to propose to a mate. It takes on a redder color."),
    # ── Staryu/Starmie ──────────────────────────────────────────────────────
    (120,"Staryu",     "staryu",     2,  1, None, 121,  None, "Even though its body is separated it is fine. Examine the red core of its body — it flickers."),
    (121,"Starmie",    "starmie",    2,  2, 120,  None, None, "Its central core glows with the seven colors of the rainbow. Some people call it the gem of the sea."),
    # ── Singles ──────────────────────────────────────────────────────────────
    (122,"Mr Mime",    "mr-mime",    10, 2, None, None, None, "If interrupted while miming it will slap around the offender with its broad hands."),
    (123,"Scyther",    "scyther",    11, 2, None, None, None, "With ninja-like agility and speed it can create the illusion that there is more than one."),
    (124,"Jynx",       "jynx",       10, 2, None, None, None, "It seductively wiggles its hips as it walks. It can cause people to dance in unison with it."),
    (125,"Electabuzz", "electabuzz", 3,  2, None, None, None, "Normally found near power plants it can wander into populated areas if there is a power outage."),
    (126,"Magmar",     "magmar",     1,  2, None, None, None, "Its body always burns and floats in lava. It is known to cause forest fires unintentionally."),
    (127,"Pinsir",     "pinsir",     11, 2, None, None, None, "If it grips an enemy with its pincers it will not stop squeezing until the enemy is torn in half."),
    (128,"Tauros",     "tauros",     0,  2, None, None, None, "When whipping itself with its three tails it becomes agitated and will charge blindly."),
    # ── Magikarp line ───────────────────────────────────────────────────────
    (129,"Magikarp",   "magikarp",   2,  0, None, 130,  20,   "A pathetic excuse for a Pokemon that is only capable of flopping and splashing."),
    (130,"Gyarados",   "gyarados",   2,  3, 129,  None, None, "Once it begins to rampage it will not stop until everything is obliterated. It can devastate cities."),
    # ── Singles ──────────────────────────────────────────────────────────────
    (131,"Lapras",     "lapras",     2,  3, None, None, None, "A Pokemon that has been overhunted almost to extinction. It is gentle and transports people on its back."),
    (132,"Ditto",      "ditto",      0,  1, None, None, None, "Capable of copying an enemy's genetic code to instantly transform itself into a duplicate."),
    # ── Eevee evolutions ────────────────────────────────────────────────────
    (133,"Eevee",      "eevee",      0,  2, None, None, None, "An evolutionary process began from irregular mutations. It has the potential to evolve into many forms."),
    (134,"Vaporeon",   "vaporeon",   2,  3, 133,  None, None, "Somewhat rare in the wild. Vaporeon evolved close to water. Its cell composition is similar to water molecules."),
    (135,"Jolteon",    "jolteon",    3,  3, 133,  None, None, "It accumulates negative ions in the atmosphere to blast out 10000-volt lightning bolts."),
    (136,"Flareon",    "flareon",    1,  3, 133,  None, None, "When storing thermal energy in its body its temperature could soar to over 1600 degrees."),
    # ── Singles ──────────────────────────────────────────────────────────────
    (137,"Porygon",    "porygon",    0,  2, None, None, None, "A Pokemon that consists entirely of programming code. It is capable of moving freely in cyberspace."),
    # ── Fossil line ─────────────────────────────────────────────────────────
    (138,"Omanyte",    "omanyte",    12, 0, None, 139,  40,   "An ancient and long-since-extinct Pokemon that has been regenerated from a fossil."),
    (139,"Omastar",    "omastar",    12, 2, 138,  None, None, "A prehistoric and extinct Pokemon which used its tentacles to capture prey."),
    (140,"Kabuto",     "kabuto",     12, 0, None, 141,  40,   "A prehistoric Pokemon that lived in ancient seas 300 million years ago."),
    (141,"Kabutops",   "kabutops",   12, 2, 140,  None, None, "Its sleek shape is perfect for swimming. It slices prey with its sharp sickles and drinks the body fluids."),
    # ── Singles ──────────────────────────────────────────────────────────────
    (142,"Aerodactyl", "aerodactyl", 12, 2, None, None, None, "A ferocious Pokemon from the age of dinosaurs. Though long extinct it was regenerated from old amber."),
    (143,"Snorlax",    "snorlax",    0,  3, None, None, None, "Very lazy. Just eats and sleeps. As its rotund bulk builds it becomes steadily more slothful."),
    # ── Legendary Birds ─────────────────────────────────────────────────────
    (144,"Articuno",   "articuno",   5,  4, None, None, None, "A legendary bird Pokemon that can control ice. The flapping of its wings chills the air."),
    (145,"Zapdos",     "zapdos",     3,  4, None, None, None, "A legendary bird Pokemon that has the ability to control electricity. It becomes energized by lightning strikes."),
    (146,"Moltres",    "moltres",    1,  4, None, None, None, "A legendary bird Pokemon that can control fire. When it flaps its wings it showers its feathers in flames."),
    # ── Dratini line ────────────────────────────────────────────────────────
    (147,"Dratini",    "dratini",    14, 1, None, 148,  30,   "Long thought to be a myth. It inhabits the water and sheds its skin regularly."),
    (148,"Dragonair",  "dragonair",  14, 2, 147,  149,  55,   "A mystical Pokemon that exudes a gentle aura. Has the ability to change climate conditions."),
    (149,"Dragonite",  "dragonite",  14, 3, 147,  None, None, "An extremely rare marine Pokemon. It is said to make its home somewhere in the sea."),
    # ── Legendary ────────────────────────────────────────────────────────────
    (150,"Mewtwo",     "mewtwo",     10, 4, None, None, None, "A Pokemon created by recombining Mew's genes. It is said to be the most powerful Pokemon ever."),
    (151,"Mew",        "mew",        10, 4, None, None, None, "So rare it is still said to be a mirage. Many scientists wonder if it is really the ancestor of all Pokemon."),
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
    print(" WordSoul - Pokemon Gen 1 Seed Script")
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
        f.write("-- WordSoul - Pokemon Gen 1 Seed Data (151 Pokemon)\n")
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
