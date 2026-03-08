namespace KeyboardTrainer.Server

open System
open System.Text
open System.Text.Json

module ProbabilityExerciseGenerator =
    let private tryParseWeightKey (key: string) =
        if isNull key || key.Length = 0 then
            None
        elif key.Equals("space", StringComparison.OrdinalIgnoreCase) then
            Some ' '
        elif key.Length = 1 then
            Some key[0]
        else
            None

    let private tryGetProperty (name: string) (element: JsonElement) =
        let mutable prop = Unchecked.defaultof<JsonElement>
        if element.TryGetProperty(name, &prop) then Some prop else None

    let private tryReadInt (element: JsonElement) =
        match element.ValueKind with
        | JsonValueKind.Number ->
            let mutable value = 0
            if element.TryGetInt32(&value) then Some value else None
        | JsonValueKind.String ->
            let text = element.GetString()
            match Int32.TryParse(text) with
            | true, value -> Some value
            | _ -> None
        | _ -> None

    let private tryReadFloat (element: JsonElement) =
        match element.ValueKind with
        | JsonValueKind.Number -> Some(element.GetDouble())
        | JsonValueKind.String ->
            let text = element.GetString()
            match Double.TryParse(text) with
            | true, value -> Some value
            | _ -> None
        | _ -> None

    let generateFromProbabilityJson (raw: string) (generatedLengthOverride: int option) (wordLengthOverride: int option) =
        try
            use doc = JsonDocument.Parse(raw)
            let root = doc.RootElement

            if root.ValueKind <> JsonValueKind.Object then
                Error "Probability content must be a JSON object."
            else
                let kind =
                    tryGetProperty "kind" root
                    |> Option.bind (fun value ->
                        if value.ValueKind = JsonValueKind.String then Some(value.GetString()) else None)

                if kind.IsNone || not (kind.Value.Equals("unigram-v1", StringComparison.OrdinalIgnoreCase)) then
                    Error "Unsupported probability content kind. Expected 'unigram-v1'."
                else
                    let generatedLength =
                        match generatedLengthOverride with
                        | Some value when value > 0 -> Ok value
                        | Some _ -> Error "generatedLength must be > 0"
                        | None ->
                            match tryGetProperty "generatedLength" root |> Option.bind tryReadInt with
                            | Some value when value > 0 -> Ok value
                            | Some _ -> Error "generatedLength in content must be > 0"
                            | None -> Ok 280

                    let wordLength =
                        match wordLengthOverride with
                        | Some value when value > 0 -> Ok value
                        | Some _ -> Error "wordLength must be > 0"
                        | None ->
                            match tryGetProperty "wordLength" root |> Option.bind tryReadInt with
                            | Some value when value > 0 -> Ok value
                            | Some _ -> Error "wordLength in content must be > 0"
                            | None -> Ok 5

                    match generatedLength, wordLength with
                    | Error message, _
                    | _, Error message -> Error message
                    | Ok generatedLengthValue, Ok wordLengthValue ->
                        match tryGetProperty "weights" root with
                        | None -> Error "Probability content must include a 'weights' object."
                        | Some weights when weights.ValueKind <> JsonValueKind.Object ->
                            Error "'weights' must be a JSON object."
                        | Some weights ->
                            let mutable invalidKey: string option = None
                            let entries =
                                weights.EnumerateObject()
                                |> Seq.choose (fun item ->
                                    match tryParseWeightKey item.Name with
                                    | None ->
                                        invalidKey <- Some item.Name
                                        None
                                    | Some parsedKey ->
                                        let weight = item.Value |> tryReadFloat |> Option.defaultValue 0.0
                                        if weight > 0.0 then Some(parsedKey, weight) else None)
                                |> Seq.toArray

                            if invalidKey.IsSome then
                                Error(sprintf "Invalid weight key '%s'. Keys must be one character (or 'space')." invalidKey.Value)
                            elif entries.Length = 0 then
                                Error "No positive weights found in probability content."
                            else
                                let total = entries |> Array.sumBy snd
                                if total <= 0.0 then
                                    Error "Total probability weight must be positive."
                                else
                                    let cumulative =
                                        entries
                                        |> Array.scan (fun acc (_, weight) -> acc + weight) 0.0
                                        |> Array.tail

                                    let random = Random()
                                    let builder = StringBuilder()
                                    for i in 0 .. generatedLengthValue - 1 do
                                        if wordLengthValue > 0 && i > 0 && i % wordLengthValue = 0 then
                                            builder.Append(' ') |> ignore

                                        let roll = random.NextDouble() * total
                                        let mutable idx = 0
                                        while idx < cumulative.Length - 1 && roll > cumulative[idx] do
                                            idx <- idx + 1

                                        let nextChar = fst entries[idx]
                                        builder.Append(nextChar) |> ignore

                                    Ok(builder.ToString())
        with
        | :? JsonException as ex ->
            Error(sprintf "Invalid probability JSON: %s" ex.Message)
        | ex ->
            Error(sprintf "Failed to generate probability exercise: %s" ex.Message)
