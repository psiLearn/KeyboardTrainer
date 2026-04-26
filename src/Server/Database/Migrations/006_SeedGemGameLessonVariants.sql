-- Migration: 006_SeedGemGameLessonVariants
-- Date: 2026-04-26
-- Description: Seed gem game lesson variants for movement speed and letter visibility

WITH variants (title, difficulty, content_type, language, content) AS (
    VALUES
        (
            'Gem Game: Still Rows + Letters',
            'A1',
            'gem_game',
            'French',
            '{"rows":15,"columns":15,"tickMs":850,"durationSeconds":75,"targetScore":500,"lives":10,"showLettersInGems":true,"moveRows":false}'
        ),
        (
            'Gem Game: Still Rows',
            'A1',
            'gem_game',
            'French',
            '{"rows":15,"columns":15,"tickMs":850,"durationSeconds":75,"targetScore":500,"lives":10,"showLettersInGems":false,"moveRows":false}'
        ),
        (
            'Gem Game: Slow Rows + Letters',
            'A2',
            'gem_game',
            'French',
            '{"rows":15,"columns":15,"tickMs":1600,"durationSeconds":75,"targetScore":500,"lives":10,"showLettersInGems":true,"moveRows":true}'
        ),
        (
            'Gem Game: Medium Rows + Letters',
            'A2',
            'gem_game',
            'French',
            '{"rows":15,"columns":15,"tickMs":850,"durationSeconds":75,"targetScore":500,"lives":10,"showLettersInGems":true,"moveRows":true}'
        ),
        (
            'Gem Game: Fast Rows + Letters',
            'B1',
            'gem_game',
            'French',
            '{"rows":15,"columns":15,"tickMs":400,"durationSeconds":75,"targetScore":500,"lives":10,"showLettersInGems":true,"moveRows":true}'
        ),
        (
            'Gem Game: Slow Rows',
            'A2',
            'gem_game',
            'French',
            '{"rows":15,"columns":15,"tickMs":1600,"durationSeconds":75,"targetScore":500,"lives":10,"showLettersInGems":false,"moveRows":true}'
        ),
        (
            'Gem Game: Medium Rows',
            'A2',
            'gem_game',
            'French',
            '{"rows":15,"columns":15,"tickMs":850,"durationSeconds":75,"targetScore":500,"lives":10,"showLettersInGems":false,"moveRows":true}'
        ),
        (
            'Gem Game: Fast Rows',
            'B1',
            'gem_game',
            'French',
            '{"rows":15,"columns":15,"tickMs":400,"durationSeconds":75,"targetScore":500,"lives":10,"showLettersInGems":false,"moveRows":true}'
        )
)
INSERT INTO lessons (title, difficulty, content_type, language, content)
SELECT title, difficulty::difficulty, content_type::content_type, language::language, content
FROM variants
WHERE NOT EXISTS (
    SELECT 1
    FROM lessons
    WHERE lessons.title = variants.title
      AND lessons.content_type = variants.content_type::content_type
);

WITH variants (title, content_type, content) AS (
    VALUES
        ('Gem Game: Still Rows + Letters', 'gem_game', '{"rows":15,"columns":15,"tickMs":850,"durationSeconds":75,"targetScore":500,"lives":10,"showLettersInGems":true,"moveRows":false}'),
        ('Gem Game: Still Rows', 'gem_game', '{"rows":15,"columns":15,"tickMs":850,"durationSeconds":75,"targetScore":500,"lives":10,"showLettersInGems":false,"moveRows":false}'),
        ('Gem Game: Slow Rows + Letters', 'gem_game', '{"rows":15,"columns":15,"tickMs":1600,"durationSeconds":75,"targetScore":500,"lives":10,"showLettersInGems":true,"moveRows":true}'),
        ('Gem Game: Medium Rows + Letters', 'gem_game', '{"rows":15,"columns":15,"tickMs":850,"durationSeconds":75,"targetScore":500,"lives":10,"showLettersInGems":true,"moveRows":true}'),
        ('Gem Game: Fast Rows + Letters', 'gem_game', '{"rows":15,"columns":15,"tickMs":400,"durationSeconds":75,"targetScore":500,"lives":10,"showLettersInGems":true,"moveRows":true}'),
        ('Gem Game: Slow Rows', 'gem_game', '{"rows":15,"columns":15,"tickMs":1600,"durationSeconds":75,"targetScore":500,"lives":10,"showLettersInGems":false,"moveRows":true}'),
        ('Gem Game: Medium Rows', 'gem_game', '{"rows":15,"columns":15,"tickMs":850,"durationSeconds":75,"targetScore":500,"lives":10,"showLettersInGems":false,"moveRows":true}'),
        ('Gem Game: Fast Rows', 'gem_game', '{"rows":15,"columns":15,"tickMs":400,"durationSeconds":75,"targetScore":500,"lives":10,"showLettersInGems":false,"moveRows":true}')
)
UPDATE lessons
SET content = variants.content
FROM variants
WHERE lessons.title = variants.title
  AND lessons.content_type = variants.content_type::content_type;

