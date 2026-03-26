-- Seed Data for Gym Leaders, Achievements, and GymLeaderPets
-- Note: Replace PetId values as appropriate for your production environment before executing!

-- 1. Insert Achievements
SET IDENTITY_INSERT Achievements ON;
INSERT INTO Achievements (Id, Name, Description, ConditionType, ConditionValue, RewardItemId, CreatedAt) VALUES
-- ── JOHTO (1–8) ──
(101, 'Zephyr Badge',  'Defeated Falkner, Violet City Gym Leader', 4, 1, 0, GETUTCDATE()),
(102, 'Hive Badge',    'Defeated Bugsy, Azalea Town Gym Leader',   4, 2, 0, GETUTCDATE()),
(103, 'Plain Badge',   'Defeated Whitney, Goldenrod Gym Leader',   4, 3, 0, GETUTCDATE()),
(104, 'Fog Badge',     'Defeated Morty, Ecruteak Gym Leader',      4, 4, 0, GETUTCDATE()),
(105, 'Storm Badge',   'Defeated Chuck, Cianwood Gym Leader',      4, 5, 0, GETUTCDATE()),
(106, 'Mineral Badge', 'Defeated Jasmine, Olivine Gym Leader',     4, 6, 0, GETUTCDATE()),
(107, 'Glacier Badge', 'Defeated Pryce, Mahogany Gym Leader',      4, 7, 0, GETUTCDATE()),
(108, 'Rising Badge',  'Defeated Clair, Blackthorn Gym Leader',    4, 8, 0, GETUTCDATE()),

-- ── KANTO (9–16) ──
(109, 'Boulder Badge', 'Defeated Brock, Pewter City Gym Leader',   4, 9, 0, GETUTCDATE()),
(110, 'Cascade Badge', 'Defeated Misty, Cerulean City Gym Leader', 4, 10, 0, GETUTCDATE()),
(111, 'Thunder Badge', 'Defeated Lt. Surge, Vermilion Gym Leader', 4, 11, 0, GETUTCDATE()),
(112, 'Rainbow Badge', 'Defeated Erika, Celadon Gym Leader',       4, 12, 0, GETUTCDATE()),
(113, 'Soul Badge',    'Defeated Janine, Fuchsia Gym Leader',      4, 13, 0, GETUTCDATE()),
(114, 'Marsh Badge',   'Defeated Sabrina, Saffron Gym Leader',     4, 14, 0, GETUTCDATE()),
(115, 'Volcano Badge', 'Defeated Blaine, Cinnabar Gym Leader',     4, 15, 0, GETUTCDATE()),
(116, 'Earth Badge',   'Defeated Blue, Viridian Gym Leader',       4, 16, 0, GETUTCDATE());
SET IDENTITY_INSERT Achievements OFF;

-- 2. Insert GymLeaders
SET IDENTITY_INSERT GymLeaders ON;
INSERT INTO GymLeaders (Id, GymOrder, Name, Title, Description, BadgeName, BadgeAchievementId, Theme, RequiredCefrLevel, XpThreshold, VocabThreshold, RequiredMemoryState, XpReward, QuestionCount, PassRatePercent, CooldownHours) VALUES
--── JOHTO ──
(1, 1, 'Falkner', 'Flying Leader',
'Speed and awareness.',
'Zephyr Badge', 109, 3, 0, 300, 15, 'Learning', 150, 12, 80, 12),

(2, 2, 'Bugsy', 'Bug Researcher',
'Details matter.',
'Hive Badge', 110, 10, 0, 600, 15, 'Learning', 200, 12, 80, 12),

(3, 3, 'Whitney', 'Normal Star',
'Consistency test.',
'Plain Badge', 111, 0, 1, 1000, 20, 'Learning', 250, 15, 80, 12),

(4, 4, 'Morty', 'Ghost Mystic',
'Abstract thinking.',
'Fog Badge', 112, 12, 1, 1500, 20, 'Learning', 300, 15, 80, 12),

(5, 5, 'Chuck', 'Fighting Master',
'Execution under pressure.',
'Storm Badge', 113, 7, 2, 2200, 25, 'Learning', 400, 15, 80, 12),

(6, 6, 'Jasmine', 'Steel Defender',
'Structure and discipline.',
'Mineral Badge', 114, 8, 2, 3000, 25, 'Learning', 500, 15, 80, 12),

(7, 7, 'Pryce', 'Ice Veteran',
'Cold precision.',
'Glacier Badge', 115, 14, 3, 4000, 30, 'Learning', 650, 18, 80, 12),

(8, 8, 'Clair', 'Dragon Master',
'Mastery required.',
'Rising Badge', 116, 17, 3, 5500, 30, 'Learning', 800, 18, 85, 12),

-- ── KANTO ──

(9, 9, 'Brock', 'Rock Master',
'Back to fundamentals, but harder.',
'Boulder Badge', 101, 15, 3, 7000, 35, 'Review', 900, 18, 80, 12),

(10, 10, 'Misty', 'Water Specialist',
'Adaptability and flow.',
'Cascade Badge', 102, 11, 3, 9000, 35, 'Review', 1000, 18, 80, 12),

(11, 11, 'Lt. Surge', 'Electric Commander',
'Fast reactions.',
'Thunder Badge', 103, 4, 4, 11000, 40, 'Review', 1100, 20, 80, 12),

(12, 12, 'Erika', 'Grass Master',
'Patience and control.',
'Rainbow Badge', 104, 1, 4, 13000, 40, 'Review', 1200, 20, 80, 12),

(13, 13, 'Janine', 'Poison Ninja',
'Traps everywhere.',
'Soul Badge', 105, 16, 4, 16000, 45, 'Review', 1300, 20, 80, 12),

(14, 14, 'Sabrina', 'Psychic Master',
'Pure logic and prediction.',
'Marsh Badge', 106, 9, 4, 20000, 45, 'Review', 1400, 22, 85, 12),

(15, 15, 'Blaine', 'Fire Master',
'Pressure and speed.',
'Volcano Badge', 107, 2, 5, 25000, 50, 'Review', 1600, 22, 85, 12),

(16, 16, 'Blue', 'Champion Rival',
'All systems combined.',
'Earth Badge', 108, 17, 5, 32000, 55, 'Review', 2000, 25, 85, 12);

SET IDENTITY_INSERT GymLeaders OFF;

-- 3. Insert GymLeaderPets
SET IDENTITY_INSERT GymLeaderPets ON;
-- NOTE: Pet IDs (50, 51, 52...) must exist in the Pets table before you insert this!
INSERT INTO GymLeaderPets (Id, GymLeaderId, PetId, SlotIndex, BotAccuracy, BotAvgResponseMs) VALUES
-- 1 Falkner
(1, 1, 17, 0, 0.55, 7000),
(2, 1, 164, 1, 0.55, 7000),
(3, 1, 18, 2, 0.55, 7000),

-- 2 Bugsy
(4, 2, 14, 0, 0.58, 6800),
(5, 2, 214, 1, 0.58, 6800),
(6, 2, 123, 2, 0.58, 6800),

-- 3 Whitney
(7, 3, 35, 0, 0.60, 6500),
(8, 3, 36, 1, 0.60, 6500),
(9, 3, 241, 2, 0.60, 6500),

-- 4 Morty
(10, 4, 92, 0, 0.63, 6200),
(11, 4, 93, 1, 0.63, 6200),
(12, 4, 94, 2, 0.63, 6200),

-- 5 Chuck
(13, 5, 106, 0, 0.67, 5800),
(14, 5, 57, 1, 0.67, 5800),
(15, 5, 62, 2, 0.67, 5800),

-- 6 Jasmine
(16, 6, 82, 0, 0.72, 5400),
(17, 6, 227, 1, 0.72, 5400),
(18, 6, 208, 2, 0.72, 5400),

-- 7 Pryce
(19, 7, 87, 0, 0.77, 5000),
(20, 7, 221, 1, 0.77, 5000),
(21, 7, 131, 2, 0.77, 5000),

-- 8 Clair
(22, 8, 148, 0, 0.82, 4500),
(23, 8, 230, 1, 0.82, 4500),
(24, 8, 149, 2, 0.82, 4500),

-- ── KANTO (9–16) ──

-- 9 Brock
(25, 9, 75, 0, 0.84, 4200),
(26, 9, 95, 1, 0.84, 4200),
(27, 9, 141, 2, 0.84, 4200),

-- 10 Misty
(28, 10, 195, 0, 0.86, 4000),
(29, 10, 55, 1, 0.86, 4000),
(30, 10, 121, 2, 0.86, 4000),

-- 11 Lt. Surge
(31, 11, 101, 0, 0.88, 3700),
(32, 11, 125, 1, 0.88, 3700),
(33, 11, 26, 2, 0.88, 3700),

-- 12 Erika
(34, 12, 189, 0, 0.90, 3400),
(35, 12, 71, 1, 0.90, 3400),
(36, 12, 182, 2, 0.90, 3400),

-- 13 Janine
(37, 13, 169, 0, 0.92, 3000),
(38, 13, 110, 1, 0.92, 3000),
(39, 13, 49, 2, 0.92, 3000),

-- 14 Sabrina
(40, 14, 196, 0, 0.94, 2600),
(41, 14, 122, 1, 0.94, 2600),
(42, 14, 65, 2, 0.94, 2600),

-- 15 Blaine
(43, 15, 126, 0, 0.96, 2200),
(44, 15, 219, 1, 0.96, 2200),
(45, 15, 78, 2, 0.96, 2200),

-- 16 Blue 
(46, 16, 59, 0, 0.98, 1800),
(47, 16, 112, 1, 0.98, 1800),
(48, 16, 130, 2, 0.98, 1800);
SET IDENTITY_INSERT GymLeaderPets OFF;
