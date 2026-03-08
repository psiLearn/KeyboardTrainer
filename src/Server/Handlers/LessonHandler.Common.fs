namespace KeyboardTrainer.Server

open System
open KeyboardTrainer.Shared

module LessonHandlerCommon =
    let private toValidationError field message = { Field = field; Message = message }

    let toLessonDto (lesson: Lesson) : LessonDto =
        {
            Id = lesson.Id
            Title = lesson.Title
            Difficulty = lesson.Difficulty
            ContentType = lesson.ContentType
            Language = lesson.Language
            Content = lesson.Content
            CreatedAt = lesson.CreatedAt
            UpdatedAt = lesson.UpdatedAt
        }

    let validateLessonCreateDto (dto: LessonCreateDto) : ValidationError list =
        [
            if String.IsNullOrWhiteSpace dto.Title then
                toValidationError "title" "Title is required"
            elif dto.Title.Length > 100 then
                toValidationError "title" "Title cannot exceed 100 characters"

            if String.IsNullOrWhiteSpace dto.TextContent then
                toValidationError "textContent" "Content is required"
            elif dto.TextContent.Length > 5000 then
                toValidationError "textContent" "Content cannot exceed 5000 characters"
        ]

    let apiError status message errors : ApiError =
        {
            Message = message
            StatusCode = status
            Errors = errors
        }
