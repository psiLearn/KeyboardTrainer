-- Migration: 005_SeedGemGameLesson
-- Date: 2026-04-26
-- Description: Seed one gem game lesson

INSERT INTO lessons (title, difficulty, content_type, language, content)
SELECT
    'Gem Game: Color Rush',
    'A2',
    'gem_game',
    'French',
    '{"rows":15,"columns":15,"tickMs":850,"durationSeconds":75,"targetScore":500,"lives":10,"showLettersInGems":false,"moveRows":true,"scoreMode":"exponential"}'
WHERE NOT EXISTS (
    SELECT 1 FROM lessons WHERE title = 'Gem Game: Color Rush'
);

UPDATE lessons
SET content = '{"rows":15,"columns":15,"tickMs":850,"durationSeconds":75,"targetScore":500,"lives":10,"showLettersInGems":false,"moveRows":true,"scoreMode":"exponential"}'
WHERE title = 'Gem Game: Color Rush'
  AND content_type = 'gem_game';


