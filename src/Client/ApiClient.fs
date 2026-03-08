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

    let private isNullOrUndefined (value: obj) =
        isNull value || obj.ReferenceEquals(value, JS.undefined)

    let private tryGetProp (names: string list) (value: obj) =
        if isNullOrUndefined value then None
        else
            names
            |> List.tryPick (fun name ->
                let prop: obj = value?(name)
                if isNullOrUndefined prop then None else Some prop)

    let private tryGetString (names: string list) (value: obj) =
        tryGetProp names value
        |> Option.map string
        |> Option.filter (fun text -> not (String.IsNullOrWhiteSpace text))

    let private tryGetGuid (names: string list) (value: obj) =
        match tryGetString names value with
        | Some text ->
            match Guid.TryParse text with
            | true, parsed -> Some parsed
            | _ -> None
        | None -> None

    let private tryGetDate (names: string list) (value: obj) =
        match tryGetString names value with
        | Some text ->
            match DateTime.TryParse text with
            | true, parsed -> Some parsed
            | _ -> None
        | None -> None

    let private tryGetInt (names: string list) (value: obj) =
        match tryGetProp names value with
        | Some (:? int as number) -> Some number
        | Some (:? float as number) -> Some (int number)
        | Some (:? string as text) ->
            match Int32.TryParse text with
            | true, parsed -> Some parsed
            | _ -> None
        | _ -> None

    let private tryGetFloat (names: string list) (value: obj) =
        match tryGetProp names value with
        | Some (:? float as number) -> Some number
        | Some (:? int as number) -> Some (float number)
        | Some (:? string as text) ->
            match Double.TryParse text with
            | true, parsed -> Some parsed
            | _ -> None
        | _ -> None

    let private tryGetUnionCase (value: obj) =
        match value with
        | :? string as text -> Some text
        | _ -> tryGetString [ "case"; "Case" ] value

    let private parseDifficulty (value: obj) =
        match tryGetUnionCase value |> Option.map (fun text -> text.ToLowerInvariant()) with
        | Some "a1" -> Some Difficulty.A1
        | Some "a2" -> Some Difficulty.A2
        | Some "b1" -> Some Difficulty.B1
        | Some "b2" -> Some Difficulty.B2
        | Some "c1" -> Some Difficulty.C1
        | _ -> None

    let private parseContentType (value: obj) =
        match tryGetUnionCase value |> Option.map (fun text -> text.ToLowerInvariant()) with
        | Some "words" -> Some ContentType.Words
        | Some "sentences" -> Some ContentType.Sentences
        | Some "probability" -> Some ContentType.Probability
        | _ -> None

    let private parseLanguage (value: obj) =
        match tryGetUnionCase value |> Option.map (fun text -> text.ToLowerInvariant()) with
        | Some "french" -> Some Language.French
        | _ -> None

    let private parsePerKeyErrors (value: obj) =
        if isNullOrUndefined value then
            Map.empty
        else
            try
                let parsedFromArray =
                    try
                        let items: obj array = unbox value
                        items
                        |> Array.choose (fun item ->
                            match tryGetString [ "Key"; "key" ] item, tryGetInt [ "Value"; "value" ] item with
                            | Some key, Some count -> Some (key, count)
                            | _ ->
                                try
                                    let tuple: obj array = unbox item
                                    if tuple.Length >= 2 then
                                        let key =
                                            match tuple[0] with
                                            | :? string as text -> Some text
                                            | other -> Some (string other)
                                        let count =
                                            match tuple[1] with
                                            | :? int as number -> Some number
                                            | :? float as number -> Some (int number)
                                            | :? string as text ->
                                                match Int32.TryParse text with
                                                | true, parsed -> Some parsed
                                                | _ -> None
                                            | _ -> None
                                        match key, count with
                                        | Some keyText, Some value -> Some (keyText, value)
                                        | _ -> None
                                    else
                                        None
                                with _ ->
                                    None)
                        |> Map.ofArray
                        |> Some
                    with _ ->
                        None

                match parsedFromArray with
                | Some parsed -> parsed
                | None ->
                    let keys: string[] = JS.Object.keys(value) |> unbox
                    keys
                    |> Array.choose (fun key ->
                        let raw: obj = value?(key)
                        match raw with
                        | :? int as number -> Some (key, number)
                        | :? float as number -> Some (key, int number)
                        | :? string as text ->
                            match Int32.TryParse text with
                            | true, parsed -> Some (key, parsed)
                            | _ -> None
                        | _ -> None)
                    |> Map.ofArray
            with _ ->
                Map.empty

    let private tryParseLessonDto (value: obj) =
        let difficulty =
            tryGetProp [ "difficulty"; "Difficulty" ] value
            |> Option.bind parseDifficulty
            |> Option.defaultValue Difficulty.A1
        let contentType =
            tryGetProp [ "contentType"; "ContentType" ] value
            |> Option.bind parseContentType
            |> Option.defaultValue ContentType.Words
        let language =
            tryGetProp [ "language"; "Language" ] value
            |> Option.bind parseLanguage
            |> Option.defaultValue Language.French
        let createdAt =
            tryGetDate [ "createdAt"; "CreatedAt" ] value
            |> Option.defaultValue DateTime.UtcNow
        let updatedAt =
            tryGetDate [ "updatedAt"; "UpdatedAt" ] value
            |> Option.defaultValue createdAt

        match tryGetGuid [ "id"; "Id" ] value,
              tryGetString [ "title"; "Title" ] value,
              tryGetString [ "content"; "Content" ] value with
        | Some id, Some title, Some content ->
            Some {
                Id = id
                Title = title
                Difficulty = difficulty
                ContentType = contentType
                Language = language
                Content = content
                CreatedAt = createdAt
                UpdatedAt = updatedAt
            }
        | _ -> None

    let private tryParseSessionDto (value: obj) =
        let perKeyErrors =
            match tryGetProp [ "perKeyErrors"; "PerKeyErrors" ] value with
            | Some prop -> parsePerKeyErrors prop
            | None -> Map.empty

        match tryGetGuid [ "id"; "Id" ] value,
              tryGetGuid [ "lessonId"; "LessonId" ] value,
              tryGetDate [ "startedAt"; "StartedAt" ] value,
              tryGetDate [ "endedAt"; "EndedAt" ] value,
              tryGetFloat [ "wpm"; "Wpm" ] value,
              tryGetFloat [ "cpm"; "Cpm" ] value,
              tryGetFloat [ "accuracy"; "Accuracy" ] value,
              tryGetInt [ "errorCount"; "ErrorCount" ] value,
              tryGetDate [ "createdAt"; "CreatedAt" ] value with
        | Some id, Some lessonId, Some startedAt, Some endedAt, Some wpm, Some cpm, Some accuracy, Some errorCount, Some createdAt ->
            Some {
                Id = id
                LessonId = lessonId
                StartedAt = startedAt
                EndedAt = endedAt
                Wpm = wpm
                Cpm = cpm
                Accuracy = accuracy
                ErrorCount = errorCount
                PerKeyErrors = perKeyErrors
                CreatedAt = createdAt
            }
        | _ -> None

    let private tryParseExerciseDto (value: obj) =
        match tryGetString [ "content"; "Content" ] value with
        | Some content -> Some { Content = content }
        | None -> None

    let private parseArray (body: string) (parser: obj -> 'T option) (label: string) =
        try
            let raw = fromJsonString<obj array> body
            let parsed = raw |> Array.choose parser |> Array.toList
            if raw.Length = 0 || parsed.Length > 0 then
                Ok parsed
            else
                Error (AppError.Server $"Failed to parse {label} response.")
        with ex ->
            Error (AppError.fromException ex)

    let private parseSingle (body: string) (parser: obj -> 'T option) (label: string) =
        try
            let raw = fromJsonString<obj> body
            match parser raw with
            | Some parsed -> Ok parsed
            | None -> Error (AppError.Server $"Failed to parse {label} response.")
        with ex ->
            Error (AppError.fromException ex)

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
    let parseErrorResponse (status: int) (body: string) : AppError =
        AppError.fromApiResponse status body

    /// Fetch all lessons
    let getAllLessons () : Async<Result<LessonDto list, AppError>> =
        async {
            try
                let url = $"{baseUrl}/api/lessons"
                let! (status, body) = request "GET" url None
                
                if status = 200 then
                    return parseArray body tryParseLessonDto "lessons"
                else
                    return Error (parseErrorResponse status body)
            with
            | ex ->
                return Error (AppError.fromException ex)
        }

    /// Fetch a single lesson by ID
    let getLessonById (id: Guid) : Async<Result<LessonDto, AppError>> =
        async {
            try
                let url = $"{baseUrl}/api/lessons/{id}"
                let! (status, body) = request "GET" url None
                
                if status = 200 then
                    return parseSingle body tryParseLessonDto "lesson"
                else
                    return Error (parseErrorResponse status body)
            with
            | ex ->
                return Error (AppError.fromException ex)
        }

    /// Resolve lesson content into an exercise text
    let getLessonExercise (id: Guid) : Async<Result<ExerciseDto, AppError>> =
        async {
            try
                let url = $"{baseUrl}/api/lessons/{id}/exercise"
                let! (status, body) = request "GET" url None

                if status = 200 then
                    return parseSingle body tryParseExerciseDto "exercise"
                else
                    return Error (parseErrorResponse status body)
            with
            | ex ->
                return Error (AppError.fromException ex)
        }

    /// Create a new lesson
    let createLesson (dto: LessonCreateDto) : Async<Result<LessonDto, AppError>> =
        async {
            try
                let url = $"{baseUrl}/api/lessons"
                let body = toJsonString dto
                let! (status, responseBody) = request "POST" url (Some body)
                
                if status = 201 then
                    return parseSingle responseBody tryParseLessonDto "lesson"
                else
                    return Error (parseErrorResponse status responseBody)
            with
            | ex ->
                return Error (AppError.fromException ex)
        }

    /// Update a lesson
    let updateLesson (id: Guid) (dto: LessonCreateDto) : Async<Result<LessonDto, AppError>> =
        async {
            try
                let url = $"{baseUrl}/api/lessons/{id}"
                let body = toJsonString dto
                let! (status, responseBody) = request "PUT" url (Some body)
                
                if status = 200 then
                    return parseSingle responseBody tryParseLessonDto "lesson"
                else
                    return Error (parseErrorResponse status responseBody)
            with
            | ex ->
                return Error (AppError.fromException ex)
        }

    /// Delete a lesson
    let deleteLesson (id: Guid) : Async<Result<unit, AppError>> =
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
                return Error (AppError.fromException ex)
        }

    /// Create a new typing session
    let createSession (dto: SessionCreateDto) : Async<Result<SessionDto, AppError>> =
        async {
            try
                let url = $"{baseUrl}/api/sessions"
                let perKeyErrors =
                    dto.PerKeyErrors
                    |> Map.toSeq
                    |> Seq.map (fun (key, value) -> string key ==> value)
                    |> createObj
                let payload =
                    createObj [
                        "clientSessionId" ==> dto.ClientSessionId.ToString()
                        "lessonId" ==> dto.LessonId.ToString()
                        "wpm" ==> dto.Wpm
                        "cpm" ==> dto.Cpm
                        "accuracy" ==> dto.Accuracy
                        "errorCount" ==> dto.ErrorCount
                        "perKeyErrors" ==> perKeyErrors
                    ]
                let body = toJsonString payload
                let! (status, responseBody) = request "POST" url (Some body)
                
                if status = 201 then
                    return parseSingle responseBody tryParseSessionDto "session"
                else
                    return Error (parseErrorResponse status responseBody)
            with
            | ex ->
                return Error (AppError.fromException ex)
        }

    /// Get sessions for a lesson
    let getSessionsByLesson (lessonId: Guid) : Async<Result<SessionDto list, AppError>> =
        async {
            try
                let url = $"{baseUrl}/api/lessons/{lessonId}/sessions"
                let! (status, body) = request "GET" url None
                
                if status = 200 then
                    return parseArray body tryParseSessionDto "sessions"
                else
                    return Error (parseErrorResponse status body)
            with
            | ex ->
                return Error (AppError.fromException ex)
        }

    /// Get the most recent session
    let getLastSession () : Async<Result<SessionDto, AppError>> =
        async {
            try
                let url = $"{baseUrl}/api/sessions/last"
                let! (status, body) = request "GET" url None
                
                if status = 200 then
                    return parseSingle body tryParseSessionDto "session"
                else
                    return Error (parseErrorResponse status body)
            with
            | ex ->
                return Error (AppError.fromException ex)
        }
