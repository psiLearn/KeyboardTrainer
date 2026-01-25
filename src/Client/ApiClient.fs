namespace KeyboardTrainer.Client

open System
open Fable.Core
open Fable.Core.JsInterop
open Fetch
open KeyboardTrainer.Shared

module ApiClient =
    let baseUrl = 
        #if DEBUG
        "http://localhost:5000"
        #else
        ""
        #endif

    /// Fetch all lessons
    let getAllLessons () : Async<Result<LessonDto list, string>> =
        async {
            try
                let! response = fetch $"{baseUrl}/api/lessons" [
                    RequestProperties.Method HttpMethod.Get
                    RequestProperties.Headers [ ContentType "application/json" ]
                ]
                
                if response.Ok then
                    let! lessons = response.json<LessonDto list>()
                    return Ok lessons
                else
                    let! error = response.text()
                    return Error error
            with
            | ex ->
                return Error ex.Message
        }

    /// Fetch a single lesson by ID
    let getLessonById (id: Guid) : Async<Result<LessonDto, string>> =
        async {
            try
                let! response = fetch $"{baseUrl}/api/lessons/{id}" [
                    RequestProperties.Method HttpMethod.Get
                    RequestProperties.Headers [ ContentType "application/json" ]
                ]
                
                if response.Ok then
                    let! lesson = response.json<LessonDto>()
                    return Ok lesson
                else if response.Status = 404 then
                    return Error "Lesson not found"
                else
                    let! error = response.text()
                    return Error error
            with
            | ex ->
                return Error ex.Message
        }

    /// Create a new lesson
    let createLesson (dto: LessonCreateDto) : Async<Result<LessonDto, string>> =
        async {
            try
                let body = toJson dto
                let! response = fetch $"{baseUrl}/api/lessons" [
                    RequestProperties.Method HttpMethod.Post
                    RequestProperties.Headers [ ContentType "application/json" ]
                    RequestProperties.Body (U3.Case1 body)
                ]
                
                if response.Status = 201 then
                    let! lesson = response.json<LessonDto>()
                    return Ok lesson
                else if response.Status = 400 then
                    let! error = response.json<ApiError>()
                    let messages = 
                        match error.Errors with
                        | Some errors -> String.concat "; " [for e in errors -> $"{e.Field}: {e.Message}"]
                        | None -> error.Message
                    return Error messages
                else
                    let! error = response.text()
                    return Error error
            with
            | ex ->
                return Error ex.Message
        }

    /// Update a lesson
    let updateLesson (id: Guid) (dto: LessonCreateDto) : Async<Result<LessonDto, string>> =
        async {
            try
                let body = toJson dto
                let! response = fetch $"{baseUrl}/api/lessons/{id}" [
                    RequestProperties.Method HttpMethod.Put
                    RequestProperties.Headers [ ContentType "application/json" ]
                    RequestProperties.Body (U3.Case1 body)
                ]
                
                if response.Ok then
                    let! lesson = response.json<LessonDto>()
                    return Ok lesson
                else if response.Status = 404 then
                    return Error "Lesson not found"
                else if response.Status = 400 then
                    let! error = response.json<ApiError>()
                    let messages = 
                        match error.Errors with
                        | Some errors -> String.concat "; " [for e in errors -> $"{e.Field}: {e.Message}"]
                        | None -> error.Message
                    return Error messages
                else
                    let! error = response.text()
                    return Error error
            with
            | ex ->
                return Error ex.Message
        }

    /// Delete a lesson
    let deleteLesson (id: Guid) : Async<Result<unit, string>> =
        async {
            try
                let! response = fetch $"{baseUrl}/api/lessons/{id}" [
                    RequestProperties.Method HttpMethod.Delete
                    RequestProperties.Headers [ ContentType "application/json" ]
                ]
                
                if response.Status = 204 then
                    return Ok ()
                else if response.Status = 404 then
                    return Error "Lesson not found"
                else
                    let! error = response.text()
                    return Error error
            with
            | ex ->
                return Error ex.Message
        }

    /// Create a new typing session
    let createSession (dto: SessionCreateDto) : Async<Result<SessionDto, string>> =
        async {
            try
                let body = toJson dto
                let! response = fetch $"{baseUrl}/api/sessions" [
                    RequestProperties.Method HttpMethod.Post
                    RequestProperties.Headers [ ContentType "application/json" ]
                    RequestProperties.Body (U3.Case1 body)
                ]
                
                if response.Status = 201 then
                    let! session = response.json<SessionDto>()
                    return Ok session
                else if response.Status = 400 then
                    let! error = response.json<ApiError>()
                    let messages = 
                        match error.Errors with
                        | Some errors -> String.concat "; " [for e in errors -> $"{e.Field}: {e.Message}"]
                        | None -> error.Message
                    return Error messages
                else
                    let! error = response.text()
                    return Error error
            with
            | ex ->
                return Error ex.Message
        }

    /// Get sessions for a lesson
    let getSessionsByLesson (lessonId: Guid) : Async<Result<SessionDto list, string>> =
        async {
            try
                let! response = fetch $"{baseUrl}/api/lessons/{lessonId}/sessions" [
                    RequestProperties.Method HttpMethod.Get
                    RequestProperties.Headers [ ContentType "application/json" ]
                ]
                
                if response.Ok then
                    let! sessions = response.json<SessionDto list>()
                    return Ok sessions
                else
                    let! error = response.text()
                    return Error error
            with
            | ex ->
                return Error ex.Message
        }

    /// Get the most recent session
    let getLastSession () : Async<Result<SessionDto, string>> =
        async {
            try
                let! response = fetch $"{baseUrl}/api/sessions/last" [
                    RequestProperties.Method HttpMethod.Get
                    RequestProperties.Headers [ ContentType "application/json" ]
                ]
                
                if response.Ok then
                    let! session = response.json<SessionDto>()
                    return Ok session
                else if response.Status = 404 then
                    return Error "No sessions found"
                else
                    let! error = response.text()
                    return Error error
            with
            | ex ->
                return Error ex.Message
        }
