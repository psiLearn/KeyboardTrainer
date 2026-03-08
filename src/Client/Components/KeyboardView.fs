namespace KeyboardTrainer.Client.Components

open System
open Fable.React
open Fable.React.Props

module KeyboardView =
    type KeyHighlights = {
        NextKey: string option
        NextKeyFingerClass: string option
        UseFingerColors: bool
        LastKey: string option
        LastKeyIsError: bool option
    }

    let private rows =
        [
            [ "`"; "1"; "2"; "3"; "4"; "5"; "6"; "7"; "8"; "9"; "0"; "-"; "=" ]
            [ "Q"; "W"; "E"; "R"; "T"; "Y"; "U"; "I"; "O"; "P"; "["; "]"; "\\" ]
            [ "A"; "S"; "D"; "F"; "G"; "H"; "J"; "K"; "L"; ";"; "'" ]
            [ "Z"; "X"; "C"; "V"; "B"; "N"; "M"; ","; "."; "/" ]
            [ "Enter"; "Space" ]
        ]

    let charToKey (ch: char) =
        match ch with
        | ' ' -> Some "Space"
        | '\n' -> Some "Enter"
        | '\t' -> None
        | 'à' | 'â' | 'ä' | 'À' | 'Â' | 'Ä' -> Some "A"
        | 'ç' | 'Ç' -> Some "C"
        | 'é' | 'è' | 'ê' | 'ë' | 'É' | 'È' | 'Ê' | 'Ë' -> Some "E"
        | 'î' | 'ï' | 'Î' | 'Ï' -> Some "I"
        | 'ô' | 'ö' | 'Ô' | 'Ö' -> Some "O"
        | 'ù' | 'û' | 'ü' | 'Ù' | 'Û' | 'Ü' -> Some "U"
        | other when Char.IsLetter other -> Some (string (Char.ToUpperInvariant other))
        | other when Char.IsDigit other -> Some (string other)
        | '`' | '~' -> Some "`"
        | '-' | '_' -> Some "-"
        | '=' | '+' -> Some "="
        | '[' | '{' -> Some "["
        | ']' | '}' -> Some "]"
        | '\\' | '|' -> Some "\\"
        | ';' | ':' -> Some ";"
        | '\'' | '"' -> Some "'"
        | ',' | '<' -> Some ","
        | '.' | '>' -> Some "."
        | '/' | '?' -> Some "/"
        | '!' -> Some "1"
        | '@' -> Some "2"
        | '#' -> Some "3"
        | '$' -> Some "4"
        | '%' -> Some "5"
        | '^' -> Some "6"
        | '&' -> Some "7"
        | '*' -> Some "8"
        | '(' -> Some "9"
        | ')' -> Some "0"
        | _ -> None

    let keyToFingerClass (key: string) =
        let normalized = key.Trim().ToUpperInvariant()
        match normalized with
        | "SPACE" -> Some "finger-thumb"
        | "`" | "1" | "Q" | "A" | "Z" -> Some "finger-left-pinky"
        | "2" | "W" | "S" | "X" -> Some "finger-left-ring"
        | "3" | "E" | "D" | "C" -> Some "finger-left-middle"
        | "4" | "5" | "R" | "T" | "F" | "G" | "V" | "B" -> Some "finger-left-index"
        | "6" | "7" | "Y" | "U" | "H" | "J" | "N" | "M" -> Some "finger-right-index"
        | "8" | "I" | "K" | "," -> Some "finger-right-middle"
        | "9" | "O" | "L" | "." -> Some "finger-right-ring"
        | "0" | "-" | "=" | "P" | "[" | "]" | "\\" | ";" | "'" | "/" | "ENTER" -> Some "finger-right-pinky"
        | _ -> None

    let view (highlights: KeyHighlights) =
        let isNext label =
            highlights.NextKey
            |> Option.map (fun key -> String.Equals(key, label, StringComparison.OrdinalIgnoreCase))
            |> Option.defaultValue false
        let isLast label =
            highlights.LastKey
            |> Option.map (fun key -> String.Equals(key, label, StringComparison.OrdinalIgnoreCase))
            |> Option.defaultValue false
        let lastIsError =
            highlights.LastKeyIsError
            |> Option.defaultValue false

        div [ ClassName "keyboard" ] [
            for row in rows do
                div [ ClassName "keyboard-row" ] [
                    for key in row do
                        let isSpace = key = "Space"
                        let isEnter = key = "Enter"
                        let next = isNext key
                        let last = isLast key
                        let classes = ResizeArray<string>()
                        classes.Add("key")
                        if isSpace then classes.Add("key-space")
                        if isEnter then classes.Add("key-enter")
                        if next then classes.Add("key-next")
                        if next && highlights.UseFingerColors then
                            match highlights.NextKeyFingerClass with
                            | Some fingerClass -> classes.Add(fingerClass)
                            | None -> ()
                        if last && lastIsError then
                            classes.Add("key-error")
                        elif last then
                            classes.Add("key-correct")
                        let className = String.concat " " classes
                        div [ ClassName className ] [ str key ]
                ]
        ]
