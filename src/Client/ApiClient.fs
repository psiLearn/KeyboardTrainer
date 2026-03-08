namespace KeyboardTrainer.Client

open System
open Fable.Core.JsInterop
open KeyboardTrainer.Shared
open KeyboardTrainer.Client.ApiClientInfrastructure
open KeyboardTrainer.Client.ApiClientParsing

module ApiClient =
    let getAllLessons () : Async<Result<LessonDto list, AppError>> =
        async {
            try
                let url = $"{baseUrl}/api/lessons"
                let! status, body = request "GET" url None
                if status = 200 then return parseArray body tryParseLessonDto "lessons"
                else return Error (parseErrorResponse status body)
            with ex ->
                return Error (AppError.fromException ex)
        }

    let getLessonById (id: Guid) : Async<Result<LessonDto, AppError>> =
        async {
            try
                let url = $"{baseUrl}/api/lessons/{id}"
                let! status, body = request "GET" url None
                if status = 200 then return parseSingle body tryParseLessonDto "lesson"
                else return Error (parseErrorResponse status body)
            with ex ->
                return Error (AppError.fromException ex)
        }

    let getLessonExercise (id: Guid) : Async<Result<ExerciseDto, AppError>> =
        async {
            try
                let url = $"{baseUrl}/api/lessons/{id}/exercise"
                let! status, body = request "GET" url None
                if status = 200 then return parseSingle body tryParseExerciseDto "exercise"
                else return Error (parseErrorResponse status body)
            with ex ->
                return Error (AppError.fromException ex)
        }

    let createLesson (dto: LessonCreateDto) : Async<Result<LessonDto, AppError>> =
        async {
            try
                let! status, responseBody = request "POST" $"{baseUrl}/api/lessons" (Some (toJsonString dto))
                if status = 201 then return parseSingle responseBody tryParseLessonDto "lesson"
                else return Error (parseErrorResponse status responseBody)
            with ex ->
                return Error (AppError.fromException ex)
        }

    let updateLesson (id: Guid) (dto: LessonCreateDto) : Async<Result<LessonDto, AppError>> =
        async {
            try
                let! status, responseBody = request "PUT" $"{baseUrl}/api/lessons/{id}" (Some (toJsonString dto))
                if status = 200 then return parseSingle responseBody tryParseLessonDto "lesson"
                else return Error (parseErrorResponse status responseBody)
            with ex ->
                return Error (AppError.fromException ex)
        }

    let deleteLesson (id: Guid) : Async<Result<unit, AppError>> =
        async {
            try
                let! status, body = request "DELETE" $"{baseUrl}/api/lessons/{id}" None
                if status = 204 then return Ok ()
                else return Error (parseErrorResponse status body)
            with ex ->
                return Error (AppError.fromException ex)
        }

    let createSession (dto: SessionCreateDto) : Async<Result<SessionDto, AppError>> =
        async {
            try
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

                let! status, responseBody = request "POST" $"{baseUrl}/api/sessions" (Some (toJsonString payload))
                if status = 201 then return parseSingle responseBody tryParseSessionDto "session"
                else return Error (parseErrorResponse status responseBody)
            with ex ->
                return Error (AppError.fromException ex)
        }

    let getSessionsByLesson (lessonId: Guid) : Async<Result<SessionDto list, AppError>> =
        async {
            try
                let! status, body = request "GET" $"{baseUrl}/api/lessons/{lessonId}/sessions" None
                if status = 200 then return parseArray body tryParseSessionDto "sessions"
                else return Error (parseErrorResponse status body)
            with ex ->
                return Error (AppError.fromException ex)
        }

    let getLastSession () : Async<Result<SessionDto, AppError>> =
        async {
            try
                let! status, body = request "GET" $"{baseUrl}/api/sessions/last" None
                if status = 200 then return parseSingle body tryParseSessionDto "session"
                else return Error (parseErrorResponse status body)
            with ex ->
                return Error (AppError.fromException ex)
        }
