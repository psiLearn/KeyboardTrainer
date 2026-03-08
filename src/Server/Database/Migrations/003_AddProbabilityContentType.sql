-- Migration: 003_AddProbabilityContentType
-- Date: 2026-03-08
-- Description: Add probability lecture type to content_type enum

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_enum e
        JOIN pg_type t ON t.oid = e.enumtypid
        WHERE t.typname = 'content_type' AND e.enumlabel = 'probability'
    ) THEN
        ALTER TYPE content_type ADD VALUE 'probability';
    END IF;
END$$;

