namespace KeyboardTrainer.Client

open System
open Fable.Core
open Fable.Core.JsInterop
open KeyboardTrainer.Shared

[<AutoOpen>]
module private JsonHelpers =
    /// Serialize to JSON string
    let inline toJsonString obj = JS.JSON.stringify obj

    /// Parse JSON string to type
    let inline fromJsonString<'T> (json: string) : 'T =
        JS.JSON.parse json |> unbox<'T>

module ApiClient =
    let baseUrl = 
        #if DEBUG
        "http://localhost:5000"
        #else
        ""
        #endif

    [<Emit("fetch($0, $1)")>]
    let private fetch (url: string, props: obj) : JS.Promise<obj> = jsNative

    let private request (method: string) (url: string) (body: string option) =
        async {
            let headers = createObj [ "Content-Type" ==> "application/json" ]
            let props =
                match body with
                | Some payload ->
                    createObj [
                        "method" ==> method
                        "headers" ==> headers
                        "body" ==> payload
                    ]
                | None ->
                    createObj [
                        "method" ==> method
                        "headers" ==> headers
                    ]

            let! response = fetch (url, props) |> Async.AwaitPromise
            let! text = response?text() |> Async.AwaitPromise
            let status: int = response?status
            return status, text
        }

    /// Helper to parse error response
    let parseErrorResponse (status: int) (body: string) : string =
        if String.IsNullOrWhiteSpace body then
            match status with
            | 404 -> "Resource not found"
            | 400 -> "Bad request"
            | 500 -> "Server error"
            | _ -> $"HTTP {status}"
        else
            try
                let error = fromJsonString<ApiError> body
                match error.Errors with
                | Some errors -> 
                    String.concat "; " [for e in errors -> $"{e.Field}: {e.Message}"]
                | None -> error.Message
            with _ -> body

    /// Fetch all lessons
    let getAllLessons () : Async<Result<LessonDto list, string>> =
        async {
            try
                let url = $"{baseUrl}/api/lessons"
                let! (status, body) = request "GET" url None
                
                if status = 200 then
                    let lessons = fromJsonString<LessonDto list> body
                    return Ok lessons
                else
                    return Error (parseErrorResponse status body)
            with
            | ex ->
                return Error $"Network error: {ex.Message}"
        }

    /// Fetch a single lesson by ID
    let getLessonById (id: Guid) : Async<Result<LessonDto, string>> =
        async {
            try
                let url = $"{baseUrl}/api/lessons/{id}"
                let! (status, body) = request "GET" url None
                
                if status = 200 then
                    let lesson = fromJsonString<LessonDto> body
                    return Ok lesson
                else
                    return Error (parseErrorResponse status body)
            with
            | ex ->
                return Error $"Network error: {ex.Message}"
        }

    /// Create a new lesson
    let createLesson (dto: LessonCreateDto) : Async<Result<LessonDto, string>> =
        async {
            try
                let url = $"{baseUrl}/api/lessons"
                let body = toJsonString dto
                let! (status, responseBody) = request "POST" url (Some body)
                
                if status = 201 then
                    let lesson = fromJsonString<LessonDto> responseBody
                    return Ok lesson
                else
                    return Error (parseErrorResponse status responseBody)
            with
            | ex ->
                return Error $"Network error: {ex.Message}"
        }

    /// Update a lesson
    let updateLesson (id: Guid) (dto: LessonCreateDto) : Async<Result<LessonDto, string>> =
        async {
            try
                let url = $"{baseUrl}/api/lessons/{id}"
                let body = toJsonString dto
                let! (status, responseBody) = request "PUT" url (Some body)
                
                if status = 200 then
                    let lesson = fromJsonString<LessonDto> responseBody
                    return Ok lesson
                else
                    return Error (parseErrorResponse status responseBody)
            with
            | ex ->
                return Error $"Network error: {ex.Message}"
        }

    /// Delete a lesson
    let deleteLesson (id: Guid) : Async<Result<unit, string>> =
        async {
            try
                let url = $"{baseUrl}/api/lessons/{id}"
                let! (status, body) = request "DELETE" url None
                
                if status = 204 then
                    return Ok ()
                else
                    return Error (parseErrorResponse status body)
            with
            | ex ->
                return Error $"Network error: {ex.Message}"
        }

    /// Create a new typing session
    let createSession (dto: SessionCreateDto) : Async<Result<SessionDto, string>> =
        async {
            try
                let url = $"{baseUrl}/api/sessions"
                let body = toJsonString dto
                let! (status, responseBody) = request "POST" url (Some body)
                
                if status = 201 then
                    let session = fromJsonString<SessionDto> responseBody
                    return Ok session
                else
                    return Error (parseErrorResponse status responseBody)
            with
            | ex ->
                return Error $"Network error: {ex.Message}"
        }

    /// Get sessions for a lesson
    let getSessionsByLesson (lessonId: Guid) : Async<Result<SessionDto list, string>> =
        async {
            try
                let url = $"{baseUrl}/api/lessons/{lessonId}/sessions"
                let! (status, body) = request "GET" url None
                
                if status = 200 then
                    let sessions = fromJsonString<SessionDto list> body
                    return Ok sessions
                else
                    return Error (parseErrorResponse status body)
            with
            | ex ->
                return Error $"Network error: {ex.Message}"
        }

    /// Get the most recent session
    let getLastSession () : Async<Result<SessionDto, string>> =
        async {
            try
                let url = $"{baseUrl}/api/sessions/last"
                let! (status, body) = request "GET" url None
                
                if status = 200 then
                    let session = fromJsonString<SessionDto> body
                    return Ok session
                else
                    return Error (parseErrorResponse status body)
            with
            | ex ->
                return Error $"Network error: {ex.Message}"
        }
