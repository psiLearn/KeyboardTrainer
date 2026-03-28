namespace KeyboardTrainer.LectureCreator

open System
open KeyboardTrainer.Shared

module InputBuilder =
    open CliHelpers

    let buildInput (args: string array) =
        let interactive = hasArg "--interactive" args

        let title = tryGetArg "--title" args |> Option.defaultValue ""
        let difficultyText = tryGetArg "--difficulty" args |> Option.defaultValue ""
        let contentTypeText = tryGetArg "--content-type" args |> Option.defaultValue ""
        let languageText = tryGetArg "--language" args |> Option.defaultValue "French"
        let contentArg = tryGetArg "--content" args
        let contentFileArg = tryGetArg "--content-file" args
        let vocabCsvFilesArg = tryGetArg "--vocab-csv-files" args
        let vocabCsvColumnsArg = tryGetArg "--vocab-csv-columns" args

        let titleValue = if String.IsNullOrWhiteSpace title && interactive then prompt "Title: " else title
        let difficultyValue = if String.IsNullOrWhiteSpace difficultyText && interactive then prompt "Difficulty (A1|A2|B1|B2|C1): " else difficultyText
        let contentTypeValue = if String.IsNullOrWhiteSpace contentTypeText && interactive then prompt "Content type (words|sentences|probability): " else contentTypeText
        let languageValue = if String.IsNullOrWhiteSpace languageText && interactive then prompt "Language (French): " else languageText

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

                            match validateCsvLike "source-files" sourceFilesText with
                            | Error message -> Error message
                            | Ok () ->
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

                                    let alphabetRaw =
                                        match tryGetArg "--alphabet" args with
                                        | Some raw -> Some raw
                                        | None when interactive ->
                                            let raw = prompt "Alphabet subset (optional, e.g. q,w,e,r): "
                                            if String.IsNullOrWhiteSpace raw then None else Some raw
                                        | None -> None

                                    let alphabetSetResult =
                                        match alphabetRaw with
                                        | Some raw ->
                                            match validateCsvLike "alphabet" raw with
                                            | Error message -> Error message
                                            | Ok () ->
                                                Ok (splitCsvLike raw |> List.choose tryTokenToChar |> List.map Char.ToLowerInvariant |> Set.ofList |> Some)
                                        | None -> Ok None

                                    match alphabetSetResult with
                                    | Error message -> Error message
                                    | Ok alphabetSet ->
                                        match parsePositiveInt "generated-length" 280 (tryGetArg "--generated-length" args) with
                                        | Error message -> Error message
                                        | Ok generatedLength ->
                                            match parsePositiveInt "word-length" 5 (tryGetArg "--word-length" args) with
                                            | Error message -> Error message
                                            | Ok wordLength ->
                                                match parseMergeGroups mergeGroupsRaw with
                                                | Error message -> Error message
                                                | Ok mergeMap -> buildProbabilityContent sourceFiles mergeMap alphabetSet generatedLength wordLength
                    | _ ->
                        match contentArg, contentFileArg with
                        | Some text, _ when not (String.IsNullOrWhiteSpace text) -> Ok text
                        | _, Some filePath when not (String.IsNullOrWhiteSpace filePath) -> readFileContent filePath
                        | _ ->
                            let vocabCsvFilesText =
                                match vocabCsvFilesArg with
                                | Some text -> text
                                | None when interactive -> prompt "Vocabulary CSV files (optional, comma-separated): "
                                | None -> ""

                            match validateCsvLike "vocab-csv-files" vocabCsvFilesText with
                            | Error message -> Error message
                            | Ok () ->
                                let vocabCsvFiles = splitCsvLike vocabCsvFilesText
                                if not (List.isEmpty vocabCsvFiles) then
                                    let vocabColumnsResult =
                                        match vocabCsvColumnsArg with
                                        | Some raw ->
                                            match validateCsvLike "vocab-csv-columns" raw with
                                            | Ok () -> Ok(splitCsvLike raw)
                                            | Error message -> Error message
                                        | None -> Ok []

                                    match vocabColumnsResult with
                                    | Error message -> Error message
                                    | Ok vocabColumns ->
                                        buildContentFromVocabularyCsv vocabCsvFiles vocabColumns
                                elif interactive then
                                    Ok(readMultilineContent ())
                                else
                                    Error "Missing content. Use --content, --content-file, --vocab-csv-files, or --interactive."

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
