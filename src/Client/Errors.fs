namespace KeyboardTrainer.Client

open System
open Fable.Core
open Fable.Core.JsInterop
open KeyboardTrainer.Shared

type NetworkError =
    | Offline
    | Timeout
    | ConnectionRefused
    | UnknownNetwork of string

type AppError =
    | Validation of ValidationError list
    | Server of string
    | Network of NetworkError
    | Unknown of string

module AppError =
    let private isNullOrUndefined (value: obj) =
        isNull value || obj.ReferenceEquals(value, JS.undefined)

    let private tryGetProp (name: string) (value: obj) =
        if isNullOrUndefined value then
            None
        else
            let prop: obj = value?(name)
            if isNullOrUndefined prop then None else Some prop

    let private tryGetString (names: string list) (value: obj) =
        names
        |> List.tryPick (fun name ->
            match tryGetProp name value with
            | Some (:? string as text) when not (String.IsNullOrWhiteSpace text) -> Some text
            | Some other ->
                let text = string other
                if String.IsNullOrWhiteSpace text then None else Some text
            | None -> None)

    let private tryAsArray (value: obj) =
        try
            Some (unbox<obj[]> value)
        with _ ->
            None

    let private tryParseValidationError (value: obj) =
        match tryGetString [ "field"; "Field" ] value, tryGetString [ "message"; "Message" ] value with
        | Some field, Some message -> Some { Field = field; Message = message }
        | _ -> None

    let private isValidationErrorShape (value: obj) =
        match tryGetString [ "field"; "Field" ] value, tryGetString [ "message"; "Message" ] value with
        | Some _, Some _ -> true
        | _ -> false

    let rec private extractValidationArray (value: obj) =
        match tryAsArray value with
        | Some items when items.Length > 0 ->
            if items |> Array.exists isValidationErrorShape then
                Some items
            else
                extractValidationArray items[0]
        | _ -> None

    let private tryParseErrors (value: obj) =
        if isNullOrUndefined value then
            None
        else
            match extractValidationArray value with
            | Some items ->
                let errors =
                    items
                    |> Array.choose tryParseValidationError
                    |> Array.toList
                if List.isEmpty errors then None else Some errors
            | None ->
                match tryGetString [ "case"; "Case" ] value with
                | Some "Some" ->
                    match tryGetProp "fields" value |> Option.orElseWith (fun () -> tryGetProp "Fields" value) with
                    | Some fieldsValue ->
                        match extractValidationArray fieldsValue with
                        | Some items ->
                            let errors =
                                items
                                |> Array.choose tryParseValidationError
                                |> Array.toList
                            if List.isEmpty errors then None else Some errors
                        | None -> None
                    | None -> None
                | _ -> None

    let private tryParseApiError (body: string) =
        try
            let payload = JS.JSON.parse body
            let message = tryGetString [ "message"; "Message" ] payload
            let errors =
                match tryGetProp "errors" payload |> Option.orElseWith (fun () -> tryGetProp "Errors" payload) with
                | Some errorsValue -> tryParseErrors errorsValue
                | None -> None
            Some (message, errors)
        with _ ->
            None

    let private ofStatus status =
        match status with
        | 400 -> Server "Bad request"
        | 404 -> Server "Resource not found"
        | 409 -> Server "Conflict"
        | 422 -> Server "Validation error"
        | 500 -> Server "Server error"
        | _ -> Server (sprintf "HTTP %d" status)

    let fromApiResponse (status: int) (body: string) =
        if String.IsNullOrWhiteSpace body then
            ofStatus status
        else
            match tryParseApiError body with
            | Some (messageOpt, errorsOpt) ->
                match errorsOpt with
                | Some errors when not (List.isEmpty errors) -> Validation errors
                | _ ->
                    match messageOpt with
                    | Some message when not (String.IsNullOrWhiteSpace message) -> Server message
                    | _ -> ofStatus status
            | None ->
                Server body

    let fromException (ex: exn) =
        let message = if isNull ex then "" else ex.Message
        if String.IsNullOrWhiteSpace message then
            Unknown "Unknown error"
        else
            let normalized = message.ToLowerInvariant()
            if normalized.Contains("timeout") then
                Network Timeout
            elif normalized.Contains("offline") then
                Network Offline
            elif normalized.Contains("failed to fetch") || normalized.Contains("network") then
                Network ConnectionRefused
            else
                Network (UnknownNetwork message)

    let toMessage error =
        match error with
        | Validation errors ->
            errors
            |> List.map (fun err -> sprintf "%s: %s" err.Field err.Message)
            |> String.concat "; "
        | Server msg -> msg
        | Network Offline -> "You appear to be offline."
        | Network Timeout -> "The request timed out."
        | Network ConnectionRefused -> "Unable to reach the server."
        | Network (UnknownNetwork msg) -> msg
        | Unknown msg -> msg
