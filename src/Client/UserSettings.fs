namespace KeyboardTrainer.Client

open System
open Browser.Dom
open Fable.Core
open Fable.Core.JsInterop

type UserSettings = {
    EnableLetterColors: bool
    ShowKeyboard: bool
    HighlightNextKey: bool
}

module UserSettings =
    let private storageKey = "keyboard-trainer-settings"

    let defaults = {
        EnableLetterColors = true
        ShowKeyboard = true
        HighlightNextKey = true
    }

    let private isNullOrUndefined (value: obj) =
        isNull value || obj.ReferenceEquals(value, JS.undefined)

    let private tryGetProp (names: string list) (value: obj) =
        if isNullOrUndefined value then
            None
        else
            names
            |> List.tryPick (fun name ->
                let prop: obj = value?(name)
                if isNullOrUndefined prop then None else Some prop)

    let private tryGetBool (names: string list) (value: obj) =
        match tryGetProp names value with
        | Some (:? bool as flag) -> Some flag
        | Some (:? string as text) ->
            match Boolean.TryParse text with
            | true, parsed -> Some parsed
            | _ -> None
        | _ -> None

    let load () =
        try
            let raw = window.localStorage.getItem(storageKey)
            if String.IsNullOrWhiteSpace raw then
                defaults
            else
                let parsed = JS.JSON.parse raw
                {
                    EnableLetterColors =
                        tryGetBool [ "enableLetterColors"; "EnableLetterColors" ] parsed
                        |> Option.defaultValue defaults.EnableLetterColors
                    ShowKeyboard =
                        tryGetBool [ "showKeyboard"; "ShowKeyboard" ] parsed
                        |> Option.defaultValue defaults.ShowKeyboard
                    HighlightNextKey =
                        tryGetBool [ "highlightNextKey"; "HighlightNextKey" ] parsed
                        |> Option.defaultValue defaults.HighlightNextKey
                }
        with _ ->
            defaults

    let save (settings: UserSettings) =
        try
            let payload = JS.JSON.stringify settings
            window.localStorage.setItem(storageKey, payload)
        with _ ->
            ()
