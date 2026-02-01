namespace KeyboardTrainer.Client.Components

open System
open Fable.React
open Fable.React.Props

module KeyboardView =
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

    let view (nextKey: string option) =
        let isNext label =
            nextKey
            |> Option.map (fun key -> String.Equals(key, label, StringComparison.OrdinalIgnoreCase))
            |> Option.defaultValue false

        div [ ClassName "keyboard" ] [
            for row in rows do
                div [ ClassName "keyboard-row" ] [
                    for key in row do
                        let isSpace = key = "Space"
                        let isEnter = key = "Enter"
                        let next = isNext key
                        let className =
                            match isSpace, isEnter, next with
                            | true, _, true -> "key key-space key-next"
                            | true, _, false -> "key key-space"
                            | _, true, true -> "key key-enter key-next"
                            | _, true, false -> "key key-enter"
                            | _, _, true -> "key key-next"
                            | _ -> "key"
                        div [ ClassName className ] [ str key ]
                ]
        ]
