-- Migration: 002_SeedFrenchLessons
-- Date: 2026-01-27
-- Description: Seed database with initial French lessons for MVP testing

-- Seed A1 level lessons (words)
INSERT INTO lessons (title, difficulty, content_type, language, content)
SELECT
    'A1 Common Words',
    'A1',
    'words',
    'French',
    'bonjour café table maison école ami famille car bien oui non jour nuit matin soir'
WHERE NOT EXISTS (
    SELECT 1 FROM lessons WHERE title = 'A1 Common Words'
);

INSERT INTO lessons (title, difficulty, content_type, language, content)
SELECT
    'A1 Basic Verbs',
    'A1',
    'words',
    'French',
    'être avoir aller venir faire dire voir pouvoir vouloir devoir savoir croire penser sentir'
WHERE NOT EXISTS (
    SELECT 1 FROM lessons WHERE title = 'A1 Basic Verbs'
);

-- Seed A1 level lessons (sentences)
INSERT INTO lessons (title, difficulty, content_type, language, content)
SELECT
    'A1 Greetings',
    'A1',
    'sentences',
    'French',
    'Bonjour, comment allez-vous? Je m''appelle Marie. Où est la gare? Parlez plus lentement, s''il vous plaît.'
WHERE NOT EXISTS (
    SELECT 1 FROM lessons WHERE title = 'A1 Greetings'
);

-- Seed A2 level lessons (words)
INSERT INTO lessons (title, difficulty, content_type, language, content)
SELECT
    'A2 Food and Drinks',
    'A2',
    'words',
    'French',
    'pain beurre fromage lait œuf poulet poisson viande légume fruit riz pâtes soupe sauce'
WHERE NOT EXISTS (
    SELECT 1 FROM lessons WHERE title = 'A2 Food and Drinks'
);

-- Seed B1 level lessons (sentences)
INSERT INTO lessons (title, difficulty, content_type, language, content)
SELECT
    'B1 Conversation',
    'B1',
    'sentences',
    'French',
    'Je voudrais commander une table pour deux personnes. L''hôtel est très confortable et les employés sont très accueillants. Avez-vous des recommandations pour les restaurants près d''ici?'
WHERE NOT EXISTS (
    SELECT 1 FROM lessons WHERE title = 'B1 Conversation'
);

-- Seed B1 level lessons (words with diacritics)
INSERT INTO lessons (title, difficulty, content_type, language, content)
SELECT
    'B1 Complex Diacritics',
    'B1',
    'words',
    'French',
    'résumé café élève épée être à côté où très après étudier préféré français déjà reçu'
WHERE NOT EXISTS (
    SELECT 1 FROM lessons WHERE title = 'B1 Complex Diacritics'
);
