namespace KeyboardTrainer.Server

open System
open System.Data
open System.Text.Json
open Dapper
open KeyboardTrainer.Shared

module SessionRepository =
    exception SessionConflict of string

    [<CLIMutable>]
    type SessionRow = {
        Id: Guid
        LessonId: Guid
        StartedAt: DateTime
        EndedAt: DateTime
        Wpm: float
        Cpm: float
        Accuracy: float
        ErrorCount: int
        PerKeyErrors: string
        CreatedAt: DateTime
    }

    let private parsePerKeyErrors (json: string) : Map<string, int> =
        if String.IsNullOrWhiteSpace json then
            Map.empty
        else
            try
                use doc = JsonDocument.Parse(json)
                doc.RootElement.EnumerateObject()
                |> Seq.choose (fun prop ->
                    match prop.Value.ValueKind with
                    | JsonValueKind.Number ->
                        match prop.Value.TryGetInt32() with
                        | true, value -> Some (prop.Name, value)
                        | _ -> None
                    | JsonValueKind.String ->
                        match Int32.TryParse(prop.Value.GetString()) with
                        | true, value -> Some (prop.Name, value)
                        | _ -> None
                    | _ -> None)
                |> Map.ofSeq
            with _ ->
                Map.empty

    let private toSession (row: SessionRow) : SessionDto =
        {
            Id = row.Id
            LessonId = row.LessonId
            StartedAt = row.StartedAt
            EndedAt = row.EndedAt
            Wpm = row.Wpm
            Cpm = row.Cpm
            Accuracy = row.Accuracy
            ErrorCount = row.ErrorCount
            PerKeyErrors = parsePerKeyErrors row.PerKeyErrors
            CreatedAt = row.CreatedAt
        }

    /// Create a new session record
    let createSession (dto: SessionCreateDto) : Async<SessionDto> =
        async {
            use conn = DbContext.createConnection()
            conn.Open()
            
            let id = dto.ClientSessionId
            let now = DateTime.UtcNow
            
            // Convert Map to JSON string for JSONB storage
            let perKeyErrorsJson = 
                dto.PerKeyErrors
                |> Map.toSeq
                |> Seq.map (fun (k, v) -> (string k, v))
                |> dict
                |> JsonSerializer.Serialize
            
            let insertQuery = """
                INSERT INTO sessions (id, lesson_id, started_at, ended_at, wpm, cpm, accuracy, error_count, per_key_errors, created_at)
                VALUES (@id, @lesson_id, @started_at, @ended_at, @wpm, @cpm, @accuracy, @error_count, @per_key_errors::JSONB, @created_at)
                ON CONFLICT (id) DO NOTHING
                RETURNING
                    id AS Id,
                    lesson_id AS LessonId,
                    started_at AS StartedAt,
                    ended_at AS EndedAt,
                    wpm AS Wpm,
                    cpm AS Cpm,
                    accuracy AS Accuracy,
                    error_count AS ErrorCount,
                    per_key_errors AS PerKeyErrors,
                    created_at AS CreatedAt
            """

            let selectByIdQuery = """
                SELECT
                    id AS Id,
                    lesson_id AS LessonId,
                    started_at AS StartedAt,
                    ended_at AS EndedAt,
                    wpm AS Wpm,
                    cpm AS Cpm,
                    accuracy AS Accuracy,
                    error_count AS ErrorCount,
                    per_key_errors AS PerKeyErrors,
                    created_at AS CreatedAt
                FROM sessions
                WHERE id = @id
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
            
            let! inserted = 
                conn.QueryAsync<SessionRow>(insertQuery, param)
                |> Async.AwaitTask

            let! result =
                match inserted |> Seq.tryHead with
                | Some row -> async { return row }
                | None ->
                    async {
                        let! rows =
                            conn.QueryAsync<SessionRow>(selectByIdQuery, param)
                            |> Async.AwaitTask
                        match rows |> Seq.tryHead with
                        | Some row ->
                            if row.LessonId <> dto.LessonId then
                                return raise (SessionConflict "ClientSessionId already used for a different lesson")
                            return row
                        | None -> return failwith "Idempotent session insert failed: session not found"
                    }
            
            return toSession result
        }

    /// Get sessions for a lesson
    let getSessionsByLessonId (lessonId: Guid) : Async<SessionDto list> =
        async {
            use conn = DbContext.createConnection()
            conn.Open()
            
            let query = """
                SELECT
                    id AS Id,
                    lesson_id AS LessonId,
                    started_at AS StartedAt,
                    ended_at AS EndedAt,
                    wpm AS Wpm,
                    cpm AS Cpm,
                    accuracy AS Accuracy,
                    error_count AS ErrorCount,
                    per_key_errors AS PerKeyErrors,
                    created_at AS CreatedAt
                FROM sessions 
                WHERE lesson_id = @lesson_id
                ORDER BY created_at DESC
            """
            
            let param = {| lesson_id = lessonId |}
            
            let! results = 
                conn.QueryAsync<SessionRow>(query, param)
                |> Async.AwaitTask
            
            return results
                |> Seq.map toSession
                |> List.ofSeq
        }

    /// Get most recent session
    let getLastSession () : Async<SessionDto option> =
        async {
            use conn = DbContext.createConnection()
            conn.Open()
            
            let query = """
                SELECT
                    id AS Id,
                    lesson_id AS LessonId,
                    started_at AS StartedAt,
                    ended_at AS EndedAt,
                    wpm AS Wpm,
                    cpm AS Cpm,
                    accuracy AS Accuracy,
                    error_count AS ErrorCount,
                    per_key_errors AS PerKeyErrors,
                    created_at AS CreatedAt
                FROM sessions 
                ORDER BY created_at DESC
                LIMIT 1
            """
            
            let! results = 
                conn.QueryAsync<SessionRow>(query)
                |> Async.AwaitTask
            
            let result = results |> Seq.tryHead
            return
                match result with
                | None -> None
                | Some r -> Some (toSession r)
        }
