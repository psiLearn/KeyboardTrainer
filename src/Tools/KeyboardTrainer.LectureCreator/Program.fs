module KeyboardTrainer.LectureCreator.Program

open System
open System.Collections.Generic
open System.Data
open System.IO
open System.Text.Json
open Dapper
open KeyboardTrainer.Shared
open Npgsql

type Input = {
    Title: string
    Difficulty: Difficulty
    ContentType: ContentType
    Language: Language
    Content: string
}

let private isFlag (value: string) =
    value.StartsWith("--", StringComparison.Ordinal)

let private tryGetArg (name: string) (args: string array) =
    args
    |> Array.tryFindIndex (fun arg -> String.Equals(arg, name, StringComparison.OrdinalIgnoreCase))
    |> Option.bind (fun index ->
        if index + 1 < args.Length && not (isFlag args[index + 1]) then Some args[index + 1] else None)

let private hasArg (name: string) (args: string array) =
    args
    |> Array.exists (fun arg -> String.Equals(arg, name, StringComparison.OrdinalIgnoreCase))

let private splitCsvLike (value: string) =
    value.Split([| ','; ';' |], StringSplitOptions.RemoveEmptyEntries ||| StringSplitOptions.TrimEntries)
    |> Array.toList
    |> List.filter (fun item -> not (String.IsNullOrWhiteSpace item))

let private tryTokenToChar (token: string) =
    let t = token.Trim()
    if String.IsNullOrWhiteSpace t then
        None
    elif t.Equals("space", StringComparison.OrdinalIgnoreCase) then
        Some ' '
    else
        Some t[0]

let private parseDifficulty (value: string) =
    match value.Trim().ToUpperInvariant() with
    | "A1" -> Some Difficulty.A1
    | "A2" -> Some Difficulty.A2
    | "B1" -> Some Difficulty.B1
    | "B2" -> Some Difficulty.B2
    | "C1" -> Some Difficulty.C1
    | _ -> None

let private parseContentType (value: string) =
    match value.Trim().ToLowerInvariant() with
    | "words" -> Some ContentType.Words
    | "sentences" -> Some ContentType.Sentences
    | "probability" -> Some ContentType.Probability
    | _ -> None

let private parseLanguage (value: string) =
    match value.Trim().ToLowerInvariant() with
    | "french" -> Some Language.French
    | _ -> None


let private parsePositiveInt name fallback (raw: string option) =
    match raw with
    | Some value ->
        match Int32.TryParse(value) with
        | true, parsed when parsed > 0 -> Ok parsed
        | _ -> Error(sprintf "Invalid %s: %s" name value)
    | None -> Ok fallback

let private parseMergeGroups (raw: string option) =
    let parseOne (spec: string) =
        let parts = spec.Split([| '=' |], 2, StringSplitOptions.TrimEntries)
        if parts.Length <> 2 then
            Error(sprintf "Invalid merge group: %s. Expected format like [È,E,è,e]=e" spec)
        else
            let left = parts[0].Trim().TrimStart('[').TrimEnd(']')
            let right = parts[1].Trim()
            if String.IsNullOrWhiteSpace left || String.IsNullOrWhiteSpace right then
                Error(sprintf "Invalid merge group: %s. Empty side not allowed." spec)
            else
                match tryTokenToChar right with
                | None -> Error(sprintf "Invalid merge target in group: %s" spec)
                | Some targetToken ->
                    let targetChar = targetToken |> Char.ToLowerInvariant
                    let mutable invalidSource = None
                    let pairs =
                        splitCsvLike left
                        |> List.choose (fun item ->
                            match tryTokenToChar item with
                            | Some source -> Some (source |> Char.ToLowerInvariant, targetChar)
                            | None ->
                                invalidSource <- Some item
                                None)
                    match invalidSource with
                    | Some bad -> Error(sprintf "Invalid merge source token '%s' in group: %s" bad spec)
                    | None -> Ok pairs

    match raw with
    | None -> Ok Map.empty
    | Some text when String.IsNullOrWhiteSpace text -> Ok Map.empty
    | Some text ->
        let specs = text.Split([| '|' |], StringSplitOptions.RemoveEmptyEntries ||| StringSplitOptions.TrimEntries)
        let mutable errors = ResizeArray<string>()
        let mutable result = Map.empty<char, char>
        for spec in specs do
            match parseOne spec with
            | Ok pairs ->
                for source, target in pairs do
                    result <- result.Add(source, target)
            | Error message ->
                errors.Add(message)
        if errors.Count = 0 then Ok result
        else Error(String.Join(Environment.NewLine, errors))

let private prompt (text: string) =
    Console.Write(text)
    Console.ReadLine() |> Option.ofObj |> Option.defaultValue ""

let private readMultilineContent () =
    printfn "Enter lecture content. Finish with a single line containing only: END"
    let lines = ResizeArray<string>()
    let mutable doneReading = false
    while not doneReading do
        let line = Console.ReadLine()
        if isNull line || line = "END" then
            doneReading <- true
        else
            lines.Add(line)
    String.Join(Environment.NewLine, lines)

let private readFileContent (path: string) =
    if File.Exists(path) then
        Ok(File.ReadAllText(path))
    else
        Error(sprintf "Content file not found: %s" path)

let private readTextFiles (files: string list) =
    let mutable missing = ResizeArray<string>()
    let contents =
        files
        |> List.choose (fun filePath ->
            if File.Exists(filePath) then
                Some(File.ReadAllText(filePath))
            else
                missing.Add(filePath)
                None)
    if missing.Count > 0 then
        Error(sprintf "Missing source files: %s" (String.Join(", ", missing)))
    else
        Ok contents

let private buildProbabilityContent
    (sourceFiles: string list)
    (mergeMap: Map<char, char>)
    (alphabet: Set<char> option)
    (generatedLength: int)
    (wordLength: int)
    =
    match readTextFiles sourceFiles with
    | Error message -> Error message
    | Ok texts ->
        let counts = Dictionary<char, int>()
        let addCount ch =
            if counts.ContainsKey(ch) then
                counts[ch] <- counts[ch] + 1
            else
                counts[ch] <- 1

        for text in texts do
            for rawChar in text do
                let c = rawChar |> Char.ToLowerInvariant
                if Char.IsLetter(c) || c = ' ' then
                    let merged =
                        match mergeMap.TryFind(c) with
                        | Some target -> target
                        | None -> c
                    let keep =
                        match alphabet with
                        | Some allowed -> allowed.Contains(merged)
                        | None -> true
                    if keep then addCount merged

        if counts.Count = 0 then
            Error "No letters matched after applying source files, merge groups, and alphabet subset."
        else
            let total = counts.Values |> Seq.sum |> float
            let weights = Dictionary<string, float>()
            for kv in counts do
                let key = string kv.Key
                weights[key] <- float kv.Value / total

            let alphabetList =
                counts.Keys
                |> Seq.sort
                |> Seq.map string
                |> Seq.toArray

            let payload =
                {|
                    kind = "unigram-v1"
                    generatedLength = generatedLength
                    wordLength = wordLength
                    alphabet = alphabetList
                    weights = weights
                |}

            Ok(JsonSerializer.Serialize(payload))

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

let private usage () =
    printfn "KeyboardTrainer Lecture Creator"
    printfn ""
    printfn "Usage:"
    printfn "  dotnet run --project src/Tools/KeyboardTrainer.LectureCreator/KeyboardTrainer.LectureCreator.fsproj -- [options]"
    printfn ""
    printfn "Options:"
    printfn "  --title <value>                         Lecture title"
    printfn "  --difficulty <A1|A2|B1|B2|C1>          Difficulty level"
    printfn "  --content-type <words|sentences|probability>"
    printfn "  --language <French>                     Default: French"
    printfn "  --content <value>                       Inline lecture content"
    printfn "  --content-file <path>                   Load lecture content from file"
    printfn "  --interactive                           Prompt for missing values"
    printfn "  --help                                  Show this message"
    printfn ""
    printfn "Probability options (when --content-type probability):"
    printfn "  --source-files <f1,f2,...>              Build map from one or more text files"
    printfn "  --merge-groups \"[È,E,è,e]=e|[space]=space\" Merge letters into canonical form"
    printfn "  --alphabet <q,w,e,r,space>              Reduce map to a subset of letters"
    printfn "  --generated-length <int>                Default: 280"
    printfn "  --word-length <int>                     Default: 5"
    printfn ""
    printfn "Env vars (optional): DB_HOST DB_PORT DB_NAME DB_USER DB_PASSWORD DATABASE_URL"

let private buildInput (args: string array) =
    let interactive = hasArg "--interactive" args

    let title = tryGetArg "--title" args |> Option.defaultValue ""
    let difficultyText = tryGetArg "--difficulty" args |> Option.defaultValue ""
    let contentTypeText = tryGetArg "--content-type" args |> Option.defaultValue ""
    let languageText = tryGetArg "--language" args |> Option.defaultValue "French"
    let contentArg = tryGetArg "--content" args
    let contentFileArg = tryGetArg "--content-file" args

    let titleValue =
        if String.IsNullOrWhiteSpace title && interactive then prompt "Title: " else title

    let difficultyValue =
        if String.IsNullOrWhiteSpace difficultyText && interactive then
            prompt "Difficulty (A1|A2|B1|B2|C1): "
        else
            difficultyText

    let contentTypeValue =
        if String.IsNullOrWhiteSpace contentTypeText && interactive then
            prompt "Content type (words|sentences|probability): "
        else
            contentTypeText

    let languageValue =
        if String.IsNullOrWhiteSpace languageText && interactive then prompt "Language (French): "
        else languageText

    match parseDifficulty difficultyValue, parseContentType contentTypeValue, parseLanguage languageValue with
    | None, _, _ -> Error (sprintf "Invalid difficulty: %s" difficultyValue)
    | _, None, _ -> Error (sprintf "Invalid content type: %s" contentTypeValue)
    | _, _, None -> Error (sprintf "Invalid language: %s" languageValue)
    | Some difficulty, Some contentType, Some language ->
        if String.IsNullOrWhiteSpace titleValue then
            Error "Missing title. Use --title or --interactive."
        else
            let contentResult =
                match contentType with
                | ContentType.Probability ->
                    match contentArg, contentFileArg with
                    | Some text, _ when not (String.IsNullOrWhiteSpace text) -> Ok text
                    | _, Some path when not (String.IsNullOrWhiteSpace path) -> readFileContent path
                    | _ ->
                        let sourceFilesText =
                            match tryGetArg "--source-files" args with
                            | Some text -> text
                            | None when interactive -> prompt "Source files (comma-separated): "
                            | None -> ""

                        let sourceFiles = splitCsvLike sourceFilesText
                        if List.isEmpty sourceFiles then
                            Error "Missing probability input. Use --source-files (or --content/--content-file)."
                        else
                            let mergeGroupsRaw =
                                match tryGetArg "--merge-groups" args with
                                | Some text -> Some text
                                | None when interactive ->
                                    let input = prompt "Merge groups (optional, e.g. [È,E,è,e]=e|[À,à]=a): "
                                    if String.IsNullOrWhiteSpace input then None else Some input
                                | None -> None

                            let alphabetSet =
                                match tryGetArg "--alphabet" args with
                                | Some raw ->
                                    splitCsvLike raw
                                    |> List.choose tryTokenToChar
                                    |> List.map Char.ToLowerInvariant
                                    |> Set.ofList
                                    |> Some
                                | None when interactive ->
                                    let raw = prompt "Alphabet subset (optional, e.g. q,w,e,r): "
                                    if String.IsNullOrWhiteSpace raw then None
                                    else
                                        splitCsvLike raw
                                        |> List.choose tryTokenToChar
                                        |> List.map Char.ToLowerInvariant
                                        |> Set.ofList
                                        |> Some
                                | None -> None

                            match parsePositiveInt "generated-length" 280 (tryGetArg "--generated-length" args) with
                            | Error message -> Error message
                            | Ok generatedLength ->
                                match parsePositiveInt "word-length" 5 (tryGetArg "--word-length" args) with
                                | Error message -> Error message
                                | Ok wordLength ->
                                    match parseMergeGroups mergeGroupsRaw with
                                    | Error message -> Error message
                                    | Ok mergeMap ->
                                        buildProbabilityContent sourceFiles mergeMap alphabetSet generatedLength wordLength
                | _ ->
                    match contentArg, contentFileArg with
                    | Some text, _ when not (String.IsNullOrWhiteSpace text) -> Ok text
                    | _, Some filePath when not (String.IsNullOrWhiteSpace filePath) -> readFileContent filePath
                    | _ when interactive -> Ok(readMultilineContent ())
                    | _ -> Error "Missing content. Use --content, --content-file, or --interactive."

            match contentResult with
            | Error message -> Error message
            | Ok content when String.IsNullOrWhiteSpace content -> Error "Content cannot be empty."
            | Ok content ->
                Ok {
                    Title = titleValue
                    Difficulty = difficulty
                    ContentType = contentType
                    Language = language
                    Content = content
                }

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
    if hasArg "--help" args then
        usage ()
        0
    else
        match buildInput args with
        | Error message ->
            eprintfn "Error: %s" message
            usage ()
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
