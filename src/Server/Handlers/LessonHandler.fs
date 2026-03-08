namespace KeyboardTrainer.Server

open System
open Giraffe
open KeyboardTrainer.Shared

module LessonHandler =
    let private toLessonDto (lesson: Lesson) : LessonDto =
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

    /// Validate lesson creation DTO
    let validateLessonCreateDto (dto: LessonCreateDto) : ValidationError list =
        [
            // Title validation
            if String.IsNullOrWhiteSpace dto.Title then
                { Field = "title"; Message = "Title is required" }
            elif dto.Title.Length > 100 then
                { Field = "title"; Message = "Title cannot exceed 100 characters" }
            
            // Content validation
            if String.IsNullOrWhiteSpace dto.TextContent then
                { Field = "textContent"; Message = "Content is required" }
            elif dto.TextContent.Length > 5000 then
                { Field = "textContent"; Message = "Content cannot exceed 5000 characters" }
            
            // Language validation is implicit through the type system
            // (Language is an enum that can only have valid values)
        ]

    /// HTTP handler for GET /api/lessons (get all lessons)
    let getAllLessons: HttpHandler =
        fun next ctx ->
            task {
                try
                    let! lessons = LessonRepository.getAllLessons() |> Async.StartAsTask
                    let dtos = 
                        lessons 
                        |> List.map toLessonDto
                    return! json dtos next ctx
                with
                | ex ->
                    ctx.SetStatusCode 500
                    let error: ApiError = {
                        Message = "Failed to retrieve lessons"
                        StatusCode = 500
                        Errors = Some [{| Field = "server"; Message = ex.Message |} |> fun x -> { Field = x.Field; Message = x.Message }]
                    }
                    return! json error next ctx
            }

    /// HTTP handler for GET /api/lessons/{id} (get single lesson)
    let getLessonById (id: Guid): HttpHandler =
        fun next ctx ->
            task {
                try
                    let! lesson = LessonRepository.getLessonById id |> Async.StartAsTask
                    match lesson with
                    | Some l ->
                        let dto = toLessonDto l
                        return! json dto next ctx
                    | None ->
                        ctx.SetStatusCode 404
                        let error: ApiError = {
                            Message = $"Lesson with id {id} not found"
                            StatusCode = 404
                            Errors = None
                        }
                        return! json error next ctx
                with
                | ex ->
                    ctx.SetStatusCode 500
                    let error: ApiError = {
                        Message = "Failed to retrieve lesson"
                        StatusCode = 500
                        Errors = Some [{ Field = "server"; Message = ex.Message }]
                    }
                    return! json error next ctx
            }

    /// HTTP handler for POST /api/lessons (create lesson)
    let postLesson: HttpHandler =
        fun next ctx ->
            task {
                try
                    let! dto = ctx.BindJsonAsync<LessonCreateDto>()
                    
                    // Validate input
                    let errors = validateLessonCreateDto dto
                    if not (List.isEmpty errors) then
                        ctx.SetStatusCode 400
                        let error: ApiError = {
                            Message = "Validation failed"
                            StatusCode = 400
                            Errors = Some errors
                        }
                        return! json error next ctx
                    else
                        let! lesson = LessonRepository.createLesson dto |> Async.StartAsTask
                        ctx.SetStatusCode 201
                        ctx.SetHttpHeader("Location", $"/api/lessons/{lesson.Id}")
                        let responseDto = toLessonDto lesson
                        return! json responseDto next ctx
                with
                | :? System.Text.Json.JsonException as ex ->
                    ctx.SetStatusCode 400
                    let error: ApiError = {
                        Message = "Invalid request body"
                        StatusCode = 400
                        Errors = Some [{ Field = "body"; Message = ex.Message }]
                    }
                    return! json error next ctx
                | ex ->
                    ctx.SetStatusCode 500
                    let error: ApiError = {
                        Message = "Failed to create lesson"
                        StatusCode = 500
                        Errors = Some [{ Field = "server"; Message = ex.Message }]
                    }
                    return! json error next ctx
            }

    /// HTTP handler for PUT /api/lessons/{id} (update lesson)
    let putLesson (id: Guid): HttpHandler =
        fun next ctx ->
            task {
                try
                    let! dto = ctx.BindJsonAsync<LessonCreateDto>()
                    
                    // Validate input
                    let errors = validateLessonCreateDto dto
                    if not (List.isEmpty errors) then
                        ctx.SetStatusCode 400
                        let error: ApiError = {
                            Message = "Validation failed"
                            StatusCode = 400
                            Errors = Some errors
                        }
                        return! json error next ctx
                    else
                        let! lesson = LessonRepository.updateLesson id dto |> Async.StartAsTask
                        match lesson with
                        | Some l ->
                            let responseDto = toLessonDto l
                            return! json responseDto next ctx
                        | None ->
                            ctx.SetStatusCode 404
                            let error: ApiError = {
                                Message = $"Lesson with id {id} not found"
                                StatusCode = 404
                                Errors = None
                            }
                            return! json error next ctx
                with
                | :? System.Text.Json.JsonException as ex ->
                    ctx.SetStatusCode 400
                    let error: ApiError = {
                        Message = "Invalid request body"
                        StatusCode = 400
                        Errors = Some [{ Field = "body"; Message = ex.Message }]
                    }
                    return! json error next ctx
                | ex ->
                    ctx.SetStatusCode 500
                    let error: ApiError = {
                        Message = "Failed to update lesson"
                        StatusCode = 500
                        Errors = Some [{ Field = "server"; Message = ex.Message }]
                    }
                    return! json error next ctx
            }

    /// HTTP handler for GET /api/lessons/{id}/exercise (resolve lesson content into exercise text)
    let getLessonExercise (id: Guid): HttpHandler =
        fun next ctx ->
            task {
                try
                    let! lesson = LessonRepository.getLessonById id |> Async.StartAsTask
                    match lesson with
                    | None ->
                        ctx.SetStatusCode 404
                        let error: ApiError = {
                            Message = $"Lesson with id {id} not found"
                            StatusCode = 404
                            Errors = None
                        }
                        return! json error next ctx
                    | Some value ->
                        let resolvedResult =
                            match value.ContentType with
                            | ContentType.Probability ->
                                ProbabilityExerciseGenerator.generateFromProbabilityJson value.Content None None
                            | _ ->
                                Ok value.Content

                        match resolvedResult with
                        | Ok content ->
                            let response: ExerciseDto = { Content = content }
                            return! json response next ctx
                        | Error message ->
                            ctx.SetStatusCode 400
                            let error: ApiError = {
                                Message = "Failed to generate exercise"
                                StatusCode = 400
                                Errors = Some [{ Field = "content"; Message = message }]
                            }
                            return! json error next ctx
                with ex ->
                    ctx.SetStatusCode 500
                    let error: ApiError = {
                        Message = "Failed to resolve lesson exercise"
                        StatusCode = 500
                        Errors = Some [{ Field = "server"; Message = ex.Message }]
                    }
                    return! json error next ctx
            }

    /// HTTP handler for POST /api/exercises/probability (generate exercise text from probability JSON)
    let postProbabilityExercise: HttpHandler =
        fun next ctx ->
            task {
                try
                    let! dto = ctx.BindJsonAsync<ProbabilityExerciseRequestDto>()
                    if String.IsNullOrWhiteSpace dto.Content then
                        ctx.SetStatusCode 400
                        let error: ApiError = {
                            Message = "Validation failed"
                            StatusCode = 400
                            Errors = Some [{ Field = "content"; Message = "Content is required" }]
                        }
                        return! json error next ctx
                    else
                        match ProbabilityExerciseGenerator.generateFromProbabilityJson dto.Content dto.GeneratedLength dto.WordLength with
                        | Ok content ->
                            let response: ExerciseDto = { Content = content }
                            return! json response next ctx
                        | Error message ->
                            ctx.SetStatusCode 400
                            let error: ApiError = {
                                Message = "Failed to generate exercise"
                                StatusCode = 400
                                Errors = Some [{ Field = "content"; Message = message }]
                            }
                            return! json error next ctx
                with
                | :? System.Text.Json.JsonException as ex ->
                    ctx.SetStatusCode 400
                    let error: ApiError = {
                        Message = "Invalid request body"
                        StatusCode = 400
                        Errors = Some [{ Field = "body"; Message = ex.Message }]
                    }
                    return! json error next ctx
                | ex ->
                    ctx.SetStatusCode 500
                    let error: ApiError = {
                        Message = "Failed to generate probability exercise"
                        StatusCode = 500
                        Errors = Some [{ Field = "server"; Message = ex.Message }]
                    }
                    return! json error next ctx
            }

    /// HTTP handler for DELETE /api/lessons/{id} (delete lesson)
    let deleteLesson (id: Guid): HttpHandler =
        fun next ctx ->
            task {
                try
                    let! deleted = LessonRepository.deleteLesson id |> Async.StartAsTask
                    if deleted then
                        ctx.SetStatusCode 204
                        return! next ctx
                    else
                        ctx.SetStatusCode 404
                        let error: ApiError = {
                            Message = $"Lesson with id {id} not found"
                            StatusCode = 404
                            Errors = None
                        }
                        return! json error next ctx
                with
                | ex ->
                    ctx.SetStatusCode 500
                    let error: ApiError = {
                        Message = "Failed to delete lesson"
                        StatusCode = 500
                        Errors = Some [{ Field = "server"; Message = ex.Message }]
                    }
                    return! json error next ctx
            }

