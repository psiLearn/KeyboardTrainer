-- Migration: 001_CreateLessonsAndSessionsTables
-- Date: 2026-01-27
-- Description: Create lessons and sessions tables for KeyboardTrainer MVP

-- Create UUID extension (if using PostgreSQL)
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Create Difficulty enum
CREATE TYPE difficulty AS ENUM ('A1', 'A2', 'B1', 'B2', 'C1');

-- Create ContentType enum
CREATE TYPE content_type AS ENUM ('words', 'sentences');

-- Create Language enum
CREATE TYPE language AS ENUM ('French');

-- Create lessons table
CREATE TABLE lessons (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    title VARCHAR(100) NOT NULL,
    difficulty difficulty NOT NULL,
    content_type content_type NOT NULL,
    language language NOT NULL DEFAULT 'French',
    content TEXT NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Create indexes on lessons table
CREATE INDEX idx_lessons_difficulty ON lessons(difficulty);
CREATE INDEX idx_lessons_language ON lessons(language);
CREATE INDEX idx_lessons_content_type ON lessons(content_type);

-- Create sessions table
CREATE TABLE sessions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    lesson_id UUID NOT NULL REFERENCES lessons(id) ON DELETE CASCADE,
    started_at TIMESTAMP NOT NULL,
    ended_at TIMESTAMP NOT NULL,
    wpm NUMERIC(10, 2) NOT NULL,
    cpm NUMERIC(10, 2) NOT NULL,
    accuracy NUMERIC(5, 2) NOT NULL,
    error_count INTEGER NOT NULL,
    per_key_errors JSONB NOT NULL DEFAULT '{}',
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Create indexes on sessions table
CREATE INDEX idx_sessions_lesson_id ON sessions(lesson_id);
CREATE INDEX idx_sessions_created_at ON sessions(created_at DESC);
CREATE INDEX idx_sessions_started_at ON sessions(started_at DESC);

-- Create function to update updated_at timestamp
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Create trigger for lessons table
CREATE TRIGGER update_lessons_updated_at
BEFORE UPDATE ON lessons
FOR EACH ROW
EXECUTE FUNCTION update_updated_at_column();
