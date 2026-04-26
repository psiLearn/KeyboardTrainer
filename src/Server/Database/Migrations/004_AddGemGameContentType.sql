-- Migration: 004_AddGemGameContentType
-- Date: 2026-04-26
-- Description: Add gem game lecture type to content_type enum

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_enum e
        JOIN pg_type t ON t.oid = e.enumtypid
        WHERE t.typname = 'content_type' AND e.enumlabel = 'gem_game'
    ) THEN
        ALTER TYPE content_type ADD VALUE 'gem_game';
    END IF;
END$$;
