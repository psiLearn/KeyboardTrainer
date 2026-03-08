namespace KeyboardTrainer.LectureCreator

open System
open System.Collections.Generic
open System.IO
open System.Text.Json
open KeyboardTrainer.Shared

module CliHelpers =
    let isFlag (value: string) = value.StartsWith("--", StringComparison.Ordinal)

    let tryGetArg (name: string) (args: string array) =
        args
        |> Array.tryFindIndex (fun arg -> String.Equals(arg, name, StringComparison.OrdinalIgnoreCase))
        |> Option.bind (fun index ->
            if index + 1 < args.Length && not (isFlag args[index + 1]) then Some args[index + 1] else None)

    let hasArg (name: string) (args: string array) =
        args |> Array.exists (fun arg -> String.Equals(arg, name, StringComparison.OrdinalIgnoreCase))

    let splitCsvLike (value: string) =
        value.Split([| ','; ';' |], StringSplitOptions.RemoveEmptyEntries ||| StringSplitOptions.TrimEntries)
        |> Array.toList
        |> List.filter (fun item -> not (String.IsNullOrWhiteSpace item))

    let tryTokenToChar (token: string) =
        let t = token.Trim()
        if String.IsNullOrWhiteSpace t then None
        elif t.Equals("space", StringComparison.OrdinalIgnoreCase) then Some ' '
        else Some t[0]

    let parseDifficulty (value: string) =
        match value.Trim().ToUpperInvariant() with
        | "A1" -> Some Difficulty.A1
        | "A2" -> Some Difficulty.A2
        | "B1" -> Some Difficulty.B1
        | "B2" -> Some Difficulty.B2
        | "C1" -> Some Difficulty.C1
        | _ -> None

    let parseContentType (value: string) =
        match value.Trim().ToLowerInvariant() with
        | "words" -> Some ContentType.Words
        | "sentences" -> Some ContentType.Sentences
        | "probability" -> Some ContentType.Probability
        | _ -> None

    let parseLanguage (value: string) =
        match value.Trim().ToLowerInvariant() with
        | "french" -> Some Language.French
        | _ -> None

    let parsePositiveInt name fallback (raw: string option) =
        match raw with
        | Some value ->
            match Int32.TryParse(value) with
            | true, parsed when parsed > 0 -> Ok parsed
            | _ -> Error(sprintf "Invalid %s: %s" name value)
        | None -> Ok fallback

    let parseMergeGroups (raw: string option) =
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
                                | None -> invalidSource <- Some item; None)
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
                | Ok pairs -> for source, target in pairs do result <- result.Add(source, target)
                | Error message -> errors.Add(message)
            if errors.Count = 0 then Ok result else Error(String.Join(Environment.NewLine, errors))

    let prompt (text: string) =
        Console.Write(text)
        Console.ReadLine() |> Option.ofObj |> Option.defaultValue ""

    let readMultilineContent () =
        printfn "Enter lecture content. Finish with a single line containing only: END"
        let lines = ResizeArray<string>()
        let mutable doneReading = false
        while not doneReading do
            let line = Console.ReadLine()
            if isNull line || line = "END" then doneReading <- true else lines.Add(line)
        String.Join(Environment.NewLine, lines)

    let readFileContent (path: string) =
        if File.Exists(path) then Ok(File.ReadAllText(path))
        else Error(sprintf "Content file not found: %s" path)

    let readTextFiles (files: string list) =
        let mutable missing = ResizeArray<string>()
        let contents =
            files
            |> List.choose (fun filePath ->
                if File.Exists(filePath) then Some(File.ReadAllText(filePath))
                else missing.Add(filePath); None)
        if missing.Count > 0 then Error(sprintf "Missing source files: %s" (String.Join(", ", missing)))
        else Ok contents

    let buildProbabilityContent (sourceFiles: string list) (mergeMap: Map<char, char>) (alphabet: Set<char> option) (generatedLength: int) (wordLength: int) =
        match readTextFiles sourceFiles with
        | Error message -> Error message
        | Ok texts ->
            let counts = Dictionary<char, int>()
            let addCount ch = if counts.ContainsKey(ch) then counts[ch] <- counts[ch] + 1 else counts[ch] <- 1
            for text in texts do
                for rawChar in text do
                    let c = rawChar |> Char.ToLowerInvariant
                    if Char.IsLetter(c) || c = ' ' then
                        let merged = mergeMap.TryFind(c) |> Option.defaultValue c
                        let keep = alphabet |> Option.map (fun allowed -> allowed.Contains(merged)) |> Option.defaultValue true
                        if keep then addCount merged

            if counts.Count = 0 then
                Error "No letters matched after applying source files, merge groups, and alphabet subset."
            else
                let total = counts.Values |> Seq.sum |> float
                let weights = Dictionary<string, float>()
                for kv in counts do weights[string kv.Key] <- float kv.Value / total
                let alphabetList = counts.Keys |> Seq.sort |> Seq.map string |> Seq.toArray
                let payload = {| kind = "unigram-v1"; generatedLength = generatedLength; wordLength = wordLength; alphabet = alphabetList; weights = weights |}
                Ok(JsonSerializer.Serialize(payload))

    let usage () =
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
