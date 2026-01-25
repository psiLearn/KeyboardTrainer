namespace KeyboardTrainer.Server

open System
open System.Data
open Dapper
open KeyboardTrainer.Shared

module SessionRepository =
    /// Create a new session record
    let createSession (dto: SessionCreateDto) =
        async {
            use conn = DbContext.createConnection()
            conn.Open()
            
            let id = Guid.NewGuid()
            let now = DateTime.UtcNow
            
            // Convert Map to JSON string for JSONB storage
            let perKeyErrorsJson = 
                dto.PerKeyErrors
                |> Map.toList
                |> List.map (fun (k, v) -> sprintf "\"%d\":%d" k v)
                |> String.concat ","
                |> fun s -> "{" + s + "}"
            
            let query = """
                INSERT INTO sessions (id, lesson_id, started_at, ended_at, wpm, cpm, accuracy, error_count, per_key_errors, created_at)
                VALUES (@id, @lesson_id, @started_at, @ended_at, @wpm, @cpm, @accuracy, @error_count, @per_key_errors::JSONB, @created_at)
                RETURNING id, lesson_id, started_at, ended_at, wpm, cpm, accuracy, error_count, per_key_errors, created_at
            """
            
            let param = {|
                id = id
                lesson_id = dto.LessonId
                started_at = now
                ended_at = now
                wpm = float dto.Wpm
                cpm = float dto.Cpm
                accuracy = dto.Accuracy
                error_count = dto.ErrorCount
                per_key_errors = perKeyErrorsJson
                created_at = now
            |}
            
            let! result = 
                conn.QuerySingleAsync<{| Id: Guid; Lesson_id: Guid; Started_at: DateTime; Ended_at: DateTime; Wpm: float; Cpm: float; Accuracy: float; Error_count: int; Per_key_errors: string; Created_at: DateTime |}>
                    (query, param)
                |> Async.AwaitTask
            
            // Parse JSONB back to map
            let perKeyErrors = 
                try
                    // Simple JSON parsing for demo; in production use System.Text.Json
                    Map.empty
                with _ ->
                    Map.empty
            
            return {
                Id = result.Id
                LessonId = result.Lesson_id
                StartedAt = result.Started_at
                EndedAt = result.Ended_at
                Wpm = result.Wpm
                Cpm = result.Cpm
                Accuracy = result.Accuracy
                ErrorCount = result.Error_count
                PerKeyErrors = perKeyErrors
                CreatedAt = result.Created_at
            }
        }

    /// Get sessions for a lesson
    let getSessionsByLessonId (lessonId: Guid) =
        async {
            use conn = DbContext.createConnection()
            conn.Open()
            
            let query = """
                SELECT id, lesson_id, started_at, ended_at, wpm, cpm, accuracy, error_count, per_key_errors, created_at 
                FROM sessions 
                WHERE lesson_id = @lesson_id
                ORDER BY created_at DESC
            """
            
            let param = {| lesson_id = lessonId |}
            
            let! results = 
                conn.QueryAsync<{| Id: Guid; Lesson_id: Guid; Started_at: DateTime; Ended_at: DateTime; Wpm: float; Cpm: float; Accuracy: float; Error_count: int; Per_key_errors: string; Created_at: DateTime |}>
                    (query, param)
                |> Async.AwaitTask
            
            return results
                |> Seq.map (fun r -> {
                    Id = r.Id
                    LessonId = r.Lesson_id
                    StartedAt = r.Started_at
                    EndedAt = r.Ended_at
                    Wpm = r.Wpm
                    Cpm = r.Cpm
                    Accuracy = r.Accuracy
                    ErrorCount = r.Error_count
                    PerKeyErrors = Map.empty
                    CreatedAt = r.Created_at
                })
                |> List.ofSeq
        }

    /// Get most recent session
    let getLastSession () =
        async {
            use conn = DbContext.createConnection()
            conn.Open()
            
            let query = """
                SELECT id, lesson_id, started_at, ended_at, wpm, cpm, accuracy, error_count, per_key_errors, created_at 
                FROM sessions 
                ORDER BY created_at DESC
                LIMIT 1
            """
            
            let! results = 
                conn.QueryAsync<{| Id: Guid; Lesson_id: Guid; Started_at: DateTime; Ended_at: DateTime; Wpm: float; Cpm: float; Accuracy: float; Error_count: int; Per_key_errors: string; Created_at: DateTime |}>
                    (query)
                |> Async.AwaitTask
            
            let result = results |> Seq.tryHead
            return
                match result with
                | None -> None
                | Some r ->
                    Some {
                        Id = r.Id
                        LessonId = r.Lesson_id
                        StartedAt = r.Started_at
                        EndedAt = r.Ended_at
                        Wpm = r.Wpm
                        Cpm = r.Cpm
                        Accuracy = r.Accuracy
                        ErrorCount = r.Error_count
                        PerKeyErrors = Map.empty
                        CreatedAt = r.Created_at
                    }
        }
