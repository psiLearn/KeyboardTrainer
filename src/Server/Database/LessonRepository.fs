namespace KeyboardTrainer.Server

open System
open System.Data
open Dapper
open KeyboardTrainer.Shared

module LessonRepository =
    /// Get all lessons from database
    let getAllLessons () =
        async {
            use conn = DbContext.createConnection()
            do! conn.OpenAsync() |> Async.AwaitTask
            
            let query = "SELECT id, title, difficulty, content_type, language, content, created_at, updated_at FROM lessons ORDER BY created_at DESC"
            
            let! results = 
                conn.QueryAsync<{| Id: Guid; Title: string; Difficulty: string; Content_type: string; Language: string; Content: string; Created_at: DateTime; Updated_at: DateTime |}>
                    (query)
                |> Async.AwaitTask
            
            return results 
                |> Seq.map (fun r -> {
                    Id = r.Id
                    Title = r.Title
                    Difficulty = 
                        match r.Difficulty with
                        | "A1" -> A1
                        | "A2" -> A2
                        | "B1" -> B1
                        | "B2" -> B2
                        | "C1" -> C1
                        | _ -> A1
                    ContentType = 
                        match r.Content_type with
                        | "words" -> Words
                        | "sentences" -> Sentences
                        | _ -> Words
                    Language = French
                    Content = r.Content
                    CreatedAt = r.Created_at
                    UpdatedAt = r.Updated_at
                })
                |> List.ofSeq
        }

    /// Get a single lesson by ID
    let getLessonById (id: Guid) =
        async {
            use conn = DbContext.createConnection()
            do! conn.OpenAsync() |> Async.AwaitTask
            
            let query = "SELECT id, title, difficulty, content_type, language, content, created_at, updated_at FROM lessons WHERE id = @id"
            let param = {| id = id |}
            
            let! result = 
                conn.QuerySingleOrDefaultAsync<{| Id: Guid; Title: string; Difficulty: string; Content_type: string; Language: string; Content: string; Created_at: DateTime; Updated_at: DateTime |}>
                    (query, param)
                |> Async.AwaitTask
            
            return 
                if result = null then None
                else
                    Some {
                        Id = result.Id
                        Title = result.Title
                        Difficulty = 
                            match result.Difficulty with
                            | "A1" -> A1
                            | "A2" -> A2
                            | "B1" -> B1
                            | "B2" -> B2
                            | "C1" -> C1
                            | _ -> A1
                        ContentType = 
                            match result.Content_type with
                            | "words" -> Words
                            | "sentences" -> Sentences
                            | _ -> Words
                        Language = French
                        Content = result.Content
                        CreatedAt = result.Created_at
                        UpdatedAt = result.Updated_at
                    }
        }

    /// Create a new lesson
    let createLesson (dto: LessonCreateDto) =
        async {
            use conn = DbContext.createConnection()
            do! conn.OpenAsync() |> Async.AwaitTask
            
            let id = Guid.NewGuid()
            let now = DateTime.UtcNow
            
            let query = """
                INSERT INTO lessons (id, title, difficulty, content_type, language, content, created_at, updated_at)
                VALUES (@id, @title, @difficulty, @content_type, @language, @content, @created_at, @updated_at)
                RETURNING id, title, difficulty, content_type, language, content, created_at, updated_at
            """
            
            let param = {|
                id = id
                title = dto.Title
                difficulty = dto.Difficulty.ToString()
                content_type = dto.ContentType.ToString()
                language = "French"
                content = dto.TextContent
                created_at = now
                updated_at = now
            |}
            
            let! result = 
                conn.QuerySingleAsync<{| Id: Guid; Title: string; Difficulty: string; Content_type: string; Language: string; Content: string; Created_at: DateTime; Updated_at: DateTime |}>
                    (query, param)
                |> Async.AwaitTask
            
            return {
                Id = result.Id
                Title = result.Title
                Difficulty = 
                    match result.Difficulty with
                    | "A1" -> A1
                    | "A2" -> A2
                    | "B1" -> B1
                    | "B2" -> B2
                    | "C1" -> C1
                    | _ -> A1
                ContentType = 
                    match result.Content_type with
                    | "words" -> Words
                    | "sentences" -> Sentences
                    | _ -> Words
                Language = French
                Content = result.Content
                CreatedAt = result.Created_at
                UpdatedAt = result.Updated_at
            }
        }

    /// Update an existing lesson
    let updateLesson (id: Guid) (dto: LessonCreateDto) =
        async {
            use conn = DbContext.createConnection()
            do! conn.OpenAsync() |> Async.AwaitTask
            
            let query = """
                UPDATE lessons 
                SET title = @title, difficulty = @difficulty, content_type = @content_type, 
                    content = @content, updated_at = @updated_at
                WHERE id = @id
                RETURNING id, title, difficulty, content_type, language, content, created_at, updated_at
            """
            
            let param = {|
                id = id
                title = dto.Title
                difficulty = dto.Difficulty.ToString()
                content_type = dto.ContentType.ToString()
                content = dto.TextContent
                updated_at = DateTime.UtcNow
            |}
            
            let! result = 
                conn.QuerySingleOrDefaultAsync<{| Id: Guid; Title: string; Difficulty: string; Content_type: string; Language: string; Content: string; Created_at: DateTime; Updated_at: DateTime |}>
                    (query, param)
                |> Async.AwaitTask
            
            return 
                if result = null then None
                else
                    Some {
                        Id = result.Id
                        Title = result.Title
                        Difficulty = 
                            match result.Difficulty with
                            | "A1" -> A1
                            | "A2" -> A2
                            | "B1" -> B1
                            | "B2" -> B2
                            | "C1" -> C1
                            | _ -> A1
                        ContentType = 
                            match result.Content_type with
                            | "words" -> Words
                            | "sentences" -> Sentences
                            | _ -> Words
                        Language = French
                        Content = result.Content
                        CreatedAt = result.Created_at
                        UpdatedAt = result.Updated_at
                    }
        }

    /// Delete a lesson by ID
    let deleteLesson (id: Guid) =
        async {
            use conn = DbContext.createConnection()
            do! conn.OpenAsync() |> Async.AwaitTask
            
            let query = "DELETE FROM lessons WHERE id = @id"
            let param = {| id = id |}
            
            let! rowsAffected = 
                conn.ExecuteAsync(query, param)
                |> Async.AwaitTask
            
            return rowsAffected > 0
        }
