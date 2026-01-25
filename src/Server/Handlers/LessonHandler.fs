namespace KeyboardTrainer.Server

open System
open Giraffe
open KeyboardTrainer.Shared

module LessonHandler =
    /// HTTP handler for GET /api/lessons (get all lessons)
    let getAllLessons: HttpHandler =
        fun next ctx ->
            task {
                try
                    let! lessons = LessonRepository.getAllLessons() |> Async.StartAsTask
                    let dtos = 
                        lessons 
                        |> List.map (fun lesson ->
                            {
                                Id = lesson.Id
                                Title = lesson.Title
                                Difficulty = lesson.Difficulty
                                ContentType = lesson.ContentType
                                Language = lesson.Language
                                Content = lesson.Content
                                CreatedAt = lesson.CreatedAt
                                UpdatedAt = lesson.UpdatedAt
                            } : LessonDto
                        )
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
                        let dto: LessonDto = {
                            Id = l.Id
                            Title = l.Title
                            Difficulty = l.Difficulty
                            ContentType = l.ContentType
                            Language = l.Language
                            Content = l.Content
                            CreatedAt = l.CreatedAt
                            UpdatedAt = l.UpdatedAt
                        }
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
                        ctx.SetHttpHeader "Location" $"/api/lessons/{lesson.Id}"
                        let responseDto: LessonDto = {
                            Id = lesson.Id
                            Title = lesson.Title
                            Difficulty = lesson.Difficulty
                            ContentType = lesson.ContentType
                            Language = lesson.Language
                            Content = lesson.Content
                            CreatedAt = lesson.CreatedAt
                            UpdatedAt = lesson.UpdatedAt
                        }
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
                            let responseDto: LessonDto = {
                                Id = l.Id
                                Title = l.Title
                                Difficulty = l.Difficulty
                                ContentType = l.ContentType
                                Language = l.Language
                                Content = l.Content
                                CreatedAt = l.CreatedAt
                                UpdatedAt = l.UpdatedAt
                            }
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

    /// Validate lesson creation DTO
    and validateLessonCreateDto (dto: LessonCreateDto) : ValidationError list =
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
        ]
