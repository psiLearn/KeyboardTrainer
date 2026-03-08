module KeyboardTrainer.LectureCreator.Program

open System
open System.Data
open Dapper
open Npgsql
open KeyboardTrainer.Shared
open KeyboardTrainer.LectureCreator

let private getConnectionString () =
    let getEnv name defaultValue =
        match Environment.GetEnvironmentVariable name with
        | null
        | "" -> defaultValue
        | value -> value

    match Environment.GetEnvironmentVariable "DATABASE_URL" with
    | null
    | "" ->
        let host = getEnv "DB_HOST" "localhost"
        let port = getEnv "DB_PORT" "5434"
        let dbName = getEnv "DB_NAME" "keyboardtrainer"
        let user = getEnv "DB_USER" "keyboardtrainer"
        let password = getEnv "DB_PASSWORD" "keyboardtrainer_dev"
        $"Host={host};Port={port};Database={dbName};Username={user};Password={password}"
    | value -> value

let private createConnection () : IDbConnection =
    new NpgsqlConnection(getConnectionString()) :> IDbConnection

let private contentTypeToDb (value: ContentType) =
    match value with
    | ContentType.Words -> "words"
    | ContentType.Sentences -> "sentences"
    | ContentType.Probability -> "probability"

let private insertLecture (input: Input) =
    use conn = createConnection()
    conn.Open()

    let query = """
        INSERT INTO lessons (id, title, difficulty, content_type, language, content, created_at, updated_at)
        VALUES (@id, @title, @difficulty::difficulty, @content_type::content_type, @language::language, @content, @created_at, @updated_at)
        RETURNING id
    """

    let id = Guid.NewGuid()
    let now = DateTime.UtcNow
    let param = {|
        id = id
        title = input.Title
        difficulty = input.Difficulty.ToString()
        content_type = contentTypeToDb input.ContentType
        language = input.Language.ToString()
        content = input.Content
        created_at = now
        updated_at = now
    |}

    conn.ExecuteScalar<Guid>(query, param)

[<EntryPoint>]
let main args =
    if CliHelpers.hasArg "--help" args then
        CliHelpers.usage ()
        0
    else
        match InputBuilder.buildInput args with
        | Error message ->
            eprintfn "Error: %s" message
            CliHelpers.usage ()
            1
        | Ok input ->
            try
                let id = insertLecture input
                printfn "Lecture created successfully."
                printfn "Id: %O" id
                printfn "Title: %s" input.Title
                printfn "Difficulty: %s" (input.Difficulty.ToString())
                printfn "Content type: %s" (contentTypeToDb input.ContentType)
                printfn "Language: %s" (input.Language.ToString())
                0
            with ex ->
                eprintfn "Failed to create lecture: %s" ex.Message
                2
