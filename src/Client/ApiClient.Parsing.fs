namespace KeyboardTrainer.Client

open System
open Fable.Core
open Fable.Core.JsInterop
open KeyboardTrainer.Shared
open KeyboardTrainer.Client.ApiClientInfrastructure

module ApiClientParsing =
    let private tryGetUnionCase (value: obj) =
        match value with
        | :? string as text -> Some text
        | _ -> tryGetString [ "case"; "Case" ] value

    let private parseDifficulty (value: obj) =
        match tryGetUnionCase value |> Option.map (fun text -> text.ToLowerInvariant()) with
        | Some "a1" -> Some Difficulty.A1
        | Some "a2" -> Some Difficulty.A2
        | Some "b1" -> Some Difficulty.B1
        | Some "b2" -> Some Difficulty.B2
        | Some "c1" -> Some Difficulty.C1
        | _ -> None

    let private parseContentType (value: obj) =
        match tryGetUnionCase value |> Option.map (fun text -> text.ToLowerInvariant()) with
        | Some "words" -> Some ContentType.Words
        | Some "sentences" -> Some ContentType.Sentences
        | Some "probability" -> Some ContentType.Probability
        | _ -> None

    let private parseLanguage (value: obj) =
        match tryGetUnionCase value |> Option.map (fun text -> text.ToLowerInvariant()) with
        | Some "french" -> Some Language.French
        | _ -> None

    let private parsePerKeyErrors (value: obj) =
        if isNullOrUndefined value then Map.empty
        else
            try
                let fromArray =
                    try
                        let items: obj array = unbox value
                        items
                        |> Array.choose (fun item ->
                            match tryGetString [ "Key"; "key" ] item, tryGetInt [ "Value"; "value" ] item with
                            | Some key, Some count -> Some (key, count)
                            | _ ->
                                try
                                    let tuple: obj array = unbox item
                                    if tuple.Length >= 2 then
                                        let key =
                                            match tuple[0] with
                                            | :? string as text -> Some text
                                            | other -> Some (string other)
                                        let count =
                                            match tuple[1] with
                                            | :? int as number -> Some number
                                            | :? float as number -> Some (int number)
                                            | :? string as text ->
                                                match Int32.TryParse text with
                                                | true, parsed -> Some parsed
                                                | _ -> None
                                            | _ -> None
                                        match key, count with
                                        | Some keyText, Some value -> Some (keyText, value)
                                        | _ -> None
                                    else
                                        None
                                with _ -> None)
                        |> Map.ofArray
                        |> Some
                    with _ -> None

                match fromArray with
                | Some parsed -> parsed
                | None ->
                    let keys: string[] = JS.Object.keys(value) |> unbox
                    keys
                    |> Array.choose (fun key ->
                        let raw: obj = value?(key)
                        match raw with
                        | :? int as number -> Some (key, number)
                        | :? float as number -> Some (key, int number)
                        | :? string as text ->
                            match Int32.TryParse text with
                            | true, parsed -> Some (key, parsed)
                            | _ -> None
                        | _ -> None)
                    |> Map.ofArray
            with _ ->
                Map.empty

    let tryParseLessonDto (value: obj) =
        let difficulty =
            tryGetProp [ "difficulty"; "Difficulty" ] value
            |> Option.bind parseDifficulty
            |> Option.defaultValue Difficulty.A1
        let contentType =
            tryGetProp [ "contentType"; "ContentType" ] value
            |> Option.bind parseContentType
            |> Option.defaultValue ContentType.Words
        let language =
            tryGetProp [ "language"; "Language" ] value
            |> Option.bind parseLanguage
            |> Option.defaultValue Language.French
        let createdAt =
            tryGetDate [ "createdAt"; "CreatedAt" ] value
            |> Option.defaultValue DateTime.UtcNow
        let updatedAt =
            tryGetDate [ "updatedAt"; "UpdatedAt" ] value
            |> Option.defaultValue createdAt

        match tryGetGuid [ "id"; "Id" ] value,
              tryGetString [ "title"; "Title" ] value,
              tryGetString [ "content"; "Content" ] value with
        | Some id, Some title, Some content ->
            Some {
                Id = id
                Title = title
                Difficulty = difficulty
                ContentType = contentType
                Language = language
                Content = content
                CreatedAt = createdAt
                UpdatedAt = updatedAt
            }
        | _ -> None

    let tryParseSessionDto (value: obj) =
        let perKeyErrors =
            match tryGetProp [ "perKeyErrors"; "PerKeyErrors" ] value with
            | Some prop -> parsePerKeyErrors prop
            | None -> Map.empty

        match tryGetGuid [ "id"; "Id" ] value,
              tryGetGuid [ "lessonId"; "LessonId" ] value,
              tryGetDate [ "startedAt"; "StartedAt" ] value,
              tryGetDate [ "endedAt"; "EndedAt" ] value,
              tryGetFloat [ "wpm"; "Wpm" ] value,
              tryGetFloat [ "cpm"; "Cpm" ] value,
              tryGetFloat [ "accuracy"; "Accuracy" ] value,
              tryGetInt [ "errorCount"; "ErrorCount" ] value,
              tryGetDate [ "createdAt"; "CreatedAt" ] value with
        | Some id, Some lessonId, Some startedAt, Some endedAt, Some wpm, Some cpm, Some accuracy, Some errorCount, Some createdAt ->
            Some {
                Id = id
                LessonId = lessonId
                StartedAt = startedAt
                EndedAt = endedAt
                Wpm = wpm
                Cpm = cpm
                Accuracy = accuracy
                ErrorCount = errorCount
                PerKeyErrors = perKeyErrors
                CreatedAt = createdAt
            }
        | _ -> None

    let tryParseExerciseDto (value: obj) =
        match tryGetString [ "content"; "Content" ] value with
        | Some content -> Some { Content = content }
        | None -> None
