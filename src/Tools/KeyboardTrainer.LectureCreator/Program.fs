module KeyboardTrainer.LectureCreator.Program

open System
open System.Data
open System.Collections.Generic
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

let private parseCsvList fieldName raw =
    match CliHelpers.validateCsvLike fieldName raw with
    | Error message -> Error message
    | Ok () ->
        let items = CliHelpers.splitCsvLike raw
        if List.isEmpty items then Error(sprintf "Missing %s entries." fieldName)
        else Ok items

let private parseSourceFiles args =
    match CliHelpers.tryGetArg "--source-files" args with
    | None -> Error "Missing --source-files for analyze mode."
    | Some raw when String.IsNullOrWhiteSpace raw -> Error "Missing --source-files for analyze mode."
    | Some raw -> parseCsvList "source-files" raw

let private parseAlphabet opts =
    match opts with
    | None -> Ok None
    | Some raw when String.IsNullOrWhiteSpace raw -> Ok None
    | Some raw ->
        match CliHelpers.validateCsvLike "alphabet" raw with
        | Error message -> Error message
        | Ok () ->
            let chars =
                CliHelpers.splitCsvLike raw
                |> List.choose CliHelpers.tryTokenToChar
                |> List.map Char.ToLowerInvariant
            Ok(Some(Set.ofList chars))

let private printAnalysis (counts: Dictionary<char, int>) =
    let total = counts.Values |> Seq.sum |> float
    printfn "Letter counts (after merge/alphabet rules):"
    for key in counts.Keys |> Seq.sort do
        let label = if key = ' ' then "space" else string key
        let value = counts[key]
        let percent = if total > 0.0 then float value / total * 100.0 else 0.0
        printfn "  %s: %d (%.1f%%)" label value percent

let private runAnalyzeMode args =
    match parseSourceFiles args with
    | Error message ->
        eprintfn "Error: %s" message
        1
    | Ok files ->
        match parseAlphabet (CliHelpers.tryGetArg "--alphabet" args) with
        | Error message -> eprintfn "Error: %s" message; 1
        | Ok alphabetSet ->
            match CliHelpers.parseMergeGroups (CliHelpers.tryGetArg "--merge-groups" args) with
            | Error message -> eprintfn "Error: %s" message; 1
            | Ok mergeMap ->
                match CliHelpers.readTextFiles files with
                | Error message -> eprintfn "Error: %s" message; 1
                | Ok texts ->
                    let counts = CliHelpers.analyzeTextList texts mergeMap alphabetSet
                    if counts.Count = 0 then
                        eprintfn "Error: No letters matched after applying merge groups and alphabet subset."
                        1
                    else
                        printAnalysis counts
                        0

[<EntryPoint>]
let main args =
    if CliHelpers.hasArg "--help" args then
        CliHelpers.usage ()
        0
    elif CliHelpers.hasArg "--analyze" args then
        runAnalyzeMode args
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
