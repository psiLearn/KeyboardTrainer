namespace KeyboardTrainer.Shared

open System

/// Lesson difficulty level (CEFR scale)
type Difficulty =
    | A1 | A2 | B1 | B2 | C1
    override this.ToString() =
        match this with
        | A1 -> "A1"
        | A2 -> "A2"
        | B1 -> "B1"
        | B2 -> "B2"
        | C1 -> "C1"

/// Type of content in a lesson
type ContentType =
    | Words
    | Sentences
    override this.ToString() =
        match this with
        | Words -> "words"
        | Sentences -> "sentences"

/// Language (French for MVP)
type Language =
    | French
    override this.ToString() = "French"

/// Lesson record - represents a typing practice lesson
type Lesson = {
    Id: Guid
    Title: string
    Difficulty: Difficulty
    ContentType: ContentType
    Language: Language
    Content: string
    CreatedAt: DateTime
    UpdatedAt: DateTime
}

/// DTO for creating/updating lessons
type LessonCreateDto = {
    Title: string
    Difficulty: Difficulty
    ContentType: ContentType
    Language: Language
    TextContent: string
    Tags: string option
}

/// DTO for lesson responses
type LessonDto = {
    Id: Guid
    Title: string
    Difficulty: Difficulty
    ContentType: ContentType
    Language: Language
    Content: string
    CreatedAt: DateTime
    UpdatedAt: DateTime
}

/// Session record - tracks typing practice results
type Session = {
    Id: Guid
    LessonId: Guid
    StartedAt: DateTime
    EndedAt: DateTime
    Wpm: float
    Cpm: float
    Accuracy: float
    ErrorCount: int
    PerKeyErrors: Map<string, int>
    CreatedAt: DateTime
}

/// DTO for creating sessions
type SessionCreateDto = {
    ClientSessionId: Guid
    LessonId: Guid
    Wpm: int
    Cpm: int
    Accuracy: float
    ErrorCount: int
    PerKeyErrors: Map<int, int>
}

/// DTO for session responses
type SessionDto = {
    Id: Guid
    LessonId: Guid
    StartedAt: DateTime
    EndedAt: DateTime
    Wpm: float
    Cpm: float
    Accuracy: float
    ErrorCount: int
    PerKeyErrors: Map<string, int>
    CreatedAt: DateTime
}

/// Validation error for API responses
type ValidationError = {
    Field: string
    Message: string
}

/// Standard API error response
type ApiError = {
    Message: string
    StatusCode: int
    Errors: ValidationError list option
}
