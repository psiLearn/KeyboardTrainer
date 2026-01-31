namespace KeyboardTrainer.Server

open System
open System.Data
open Dapper
open KeyboardTrainer.Shared

module LessonRepository =
    [<CLIMutable>]
    type LessonRow = {
        Id: Guid
        Title: string
        Difficulty: string
        ContentType: string
        Language: string
        Content: string
        CreatedAt: DateTime
        UpdatedAt: DateTime
    }

    let private parseDifficulty (value: string) =
        match value.ToUpperInvariant() with
        | "A1" -> A1
        | "A2" -> A2
        | "B1" -> B1
        | "B2" -> B2
        | "C1" -> C1
        | _ -> A1

    let private parseContentType (value: string) =
        match value.ToLowerInvariant() with
        | "words" -> Words
        | "sentences" -> Sentences
        | _ -> Words

    let private parseLanguage (value: string) =
        match value.ToLowerInvariant() with
        | "french" -> French
        | _ -> French

    let private toLesson (row: LessonRow) : Lesson =
        {
            Id = row.Id
            Title = row.Title
            Difficulty = parseDifficulty row.Difficulty
            ContentType = parseContentType row.ContentType
            Language = parseLanguage row.Language
            Content = row.Content
            CreatedAt = row.CreatedAt
            UpdatedAt = row.UpdatedAt
        }

    let private contentTypeToDb (value: ContentType) =
        match value with
        | Words -> "words"
        | Sentences -> "sentences"

    /// Get all lessons from database
    let getAllLessons () : Async<Lesson list> =
        async {
            use conn = DbContext.createConnection()
            conn.Open()
            
            let query = """
                SELECT
                    id AS Id,
                    title AS Title,
                    difficulty AS Difficulty,
                    content_type AS ContentType,
                    language AS Language,
                    content AS Content,
                    created_at AS CreatedAt,
                    updated_at AS UpdatedAt
                FROM lessons
                ORDER BY created_at DESC
            """
            
            let! results = 
                conn.QueryAsync<LessonRow>(query)
                |> Async.AwaitTask
            
            return results 
                |> Seq.map toLesson
                |> List.ofSeq
        }

    /// Get a single lesson by ID
    let getLessonById (id: Guid) : Async<Lesson option> =
        async {
            use conn = DbContext.createConnection()
            conn.Open()
            
            let query = """
                SELECT
                    id AS Id,
                    title AS Title,
                    difficulty AS Difficulty,
                    content_type AS ContentType,
                    language AS Language,
                    content AS Content,
                    created_at AS CreatedAt,
                    updated_at AS UpdatedAt
                FROM lessons
                WHERE id = @id
            """
            let param = {| id = id |}
            
            let! results = 
                conn.QueryAsync<LessonRow>(query, param)
                |> Async.AwaitTask
            
            let result = results |> Seq.tryHead
            return 
                match result with
                | None -> None
                | Some r -> Some (toLesson r)
        }

    /// Create a new lesson
    let createLesson (dto: LessonCreateDto) : Async<Lesson> =
        async {
            use conn = DbContext.createConnection()
            conn.Open()
            
            let id = Guid.NewGuid()
            let now = DateTime.UtcNow
            
            let query = """
                INSERT INTO lessons (id, title, difficulty, content_type, language, content, created_at, updated_at)
                VALUES (@id, @title, @difficulty::difficulty, @content_type::content_type, @language::language, @content, @created_at, @updated_at)
                RETURNING
                    id AS Id,
                    title AS Title,
                    difficulty AS Difficulty,
                    content_type AS ContentType,
                    language AS Language,
                    content AS Content,
                    created_at AS CreatedAt,
                    updated_at AS UpdatedAt
            """
            
            let param = {|
                id = id
                title = dto.Title
                difficulty = dto.Difficulty.ToString()
                content_type = contentTypeToDb dto.ContentType
                language = dto.Language.ToString()
                content = dto.TextContent
                created_at = now
                updated_at = now
            |}
            
            let! result = 
                conn.QuerySingleAsync<LessonRow>(query, param)
                |> Async.AwaitTask
            
            return toLesson result
        }

    /// Update an existing lesson
    let updateLesson (id: Guid) (dto: LessonCreateDto) : Async<Lesson option> =
        async {
            use conn = DbContext.createConnection()
            conn.Open()
            
            let query = """
                UPDATE lessons 
                SET title = @title, difficulty = @difficulty::difficulty, content_type = @content_type::content_type, 
                    language = @language::language, content = @content, updated_at = @updated_at
                WHERE id = @id
                RETURNING
                    id AS Id,
                    title AS Title,
                    difficulty AS Difficulty,
                    content_type AS ContentType,
                    language AS Language,
                    content AS Content,
                    created_at AS CreatedAt,
                    updated_at AS UpdatedAt
            """
            
            let param = {|
                id = id
                title = dto.Title
                difficulty = dto.Difficulty.ToString()
                content_type = contentTypeToDb dto.ContentType
                language = dto.Language.ToString()
                content = dto.TextContent
                updated_at = DateTime.UtcNow
            |}
            
            let! results = 
                conn.QueryAsync<LessonRow>(query, param)
                |> Async.AwaitTask
            
            let result = results |> Seq.tryHead
            return 
                match result with
                | None -> None
                | Some r -> Some (toLesson r)
        }

    /// Delete a lesson by ID
    let deleteLesson (id: Guid) : Async<bool> =
        async {
            use conn = DbContext.createConnection()
            conn.Open()
            
            let query = "DELETE FROM lessons WHERE id = @id"
            let param = {| id = id |}
            
            let! rowsAffected = 
                conn.ExecuteAsync(query, param)
                |> Async.AwaitTask
            
            return rowsAffected > 0
        }
