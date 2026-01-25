namespace KeyboardTrainer.Server

open System
open Giraffe
open KeyboardTrainer.Shared

module SessionHandler =
    /// Validate session creation DTO
    let validateSessionCreateDto (dto: SessionCreateDto) : ValidationError list =
        [
            // LessonId validation
            if dto.LessonId = Guid.Empty then
                { Field = "lessonId"; Message = "Lesson ID is required" }

            // ClientSessionId validation
            if dto.ClientSessionId = Guid.Empty then
                { Field = "clientSessionId"; Message = "Client session ID is required" }
            
            // Accuracy validation
            if dto.Accuracy < 0.0 || dto.Accuracy > 100.0 then
                { Field = "accuracy"; Message = "Accuracy must be between 0 and 100" }
            
            // WPM validation
            if dto.Wpm < 0 then
                { Field = "wpm"; Message = "Words per minute cannot be negative" }
            
            // CPM validation
            if dto.Cpm < 0 then
                { Field = "cpm"; Message = "Characters per minute cannot be negative" }
            
            // Error count validation
            if dto.ErrorCount < 0 then
                { Field = "errorCount"; Message = "Error count cannot be negative" }
        ]

    /// HTTP handler for POST /api/sessions (create session)
    let postSession: HttpHandler =
        fun next ctx ->
            task {
                try
                    let! dto = ctx.BindJsonAsync<SessionCreateDto>()
                    
                    // Validate input
                    let errors = validateSessionCreateDto dto
                    if not (List.isEmpty errors) then
                        ctx.SetStatusCode 400
                        let error: ApiError = {
                            Message = "Validation failed"
                            StatusCode = 400
                            Errors = Some errors
                        }
                        return! json error next ctx
                    else
                        // Check if lesson exists
                        let! lesson = LessonRepository.getLessonById dto.LessonId |> Async.StartAsTask
                        match lesson with
                        | None ->
                            ctx.SetStatusCode 400
                            let error: ApiError = {
                                Message = "Invalid lesson ID"
                                StatusCode = 400
                                Errors = Some [{ Field = "lessonId"; Message = "Lesson does not exist" }]
                            }
                            return! json error next ctx
                        | Some _ ->
                            let! session = SessionRepository.createSession dto |> Async.StartAsTask
                            ctx.SetStatusCode 201
                            ctx.SetHttpHeader("Location", $"/api/sessions/{session.Id}")
                            return! json session next ctx
                with
                | :? System.Text.Json.JsonException as ex ->
                    ctx.SetStatusCode 400
                    let error: ApiError = {
                        Message = "Invalid request body"
                        StatusCode = 400
                        Errors = Some [{ Field = "body"; Message = ex.Message }]
                    }
                    return! json error next ctx
                | :? SessionRepository.SessionConflict as ex ->
                    ctx.SetStatusCode 409
                    let error: ApiError = {
                        Message = "Session ID conflict"
                        StatusCode = 409
                        Errors = Some [{ Field = "clientSessionId"; Message = ex.Message }]
                    }
                    return! json error next ctx
                | ex ->
                    ctx.SetStatusCode 500
                    let error: ApiError = {
                        Message = "Failed to create session"
                        StatusCode = 500
                        Errors = Some [{ Field = "server"; Message = ex.Message }]
                    }
                    return! json error next ctx
            }

    /// HTTP handler for GET /api/lessons/{lessonId}/sessions (get sessions for a lesson)
    let getSessionsByLesson (lessonId: Guid): HttpHandler =
        fun next ctx ->
            task {
                try
                    let! sessions = SessionRepository.getSessionsByLessonId lessonId |> Async.StartAsTask
                    return! json sessions next ctx
                with
                | ex ->
                    ctx.SetStatusCode 500
                    let error: ApiError = {
                        Message = "Failed to retrieve sessions"
                        StatusCode = 500
                        Errors = Some [{ Field = "server"; Message = ex.Message }]
                    }
                    return! json error next ctx
            }

    /// HTTP handler for GET /api/sessions/last (get most recent session)
    let getLastSession: HttpHandler =
        fun next ctx ->
            task {
                try
                    let! session = SessionRepository.getLastSession() |> Async.StartAsTask
                    match session with
                    | Some s -> return! json s next ctx
                    | None ->
                        ctx.SetStatusCode 404
                        let error: ApiError = {
                            Message = "No sessions found"
                            StatusCode = 404
                            Errors = None
                        }
                        return! json error next ctx
                with
                | ex ->
                    ctx.SetStatusCode 500
                    let error: ApiError = {
                        Message = "Failed to retrieve session"
                        StatusCode = 500
                        Errors = Some [{ Field = "server"; Message = ex.Message }]
                    }
                    return! json error next ctx
            }

