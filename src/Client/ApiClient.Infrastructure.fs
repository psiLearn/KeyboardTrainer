namespace KeyboardTrainer.Client

open System
open Fable.Core
open Fable.Core.JsInterop
open KeyboardTrainer.Shared

[<AutoOpen>]
module private JsonHelpers =
    let inline toJsonString obj = JS.JSON.stringify obj
    let inline fromJsonString<'T> (json: string) : 'T = JS.JSON.parse json |> unbox<'T>

module ApiClientInfrastructure =
    let baseUrl =
        #if DEBUG
        "http://localhost:5000"
        #else
        ""
        #endif

    [<Emit("fetch($0, $1)")>]
    let private fetch (url: string, props: obj) : JS.Promise<obj> = jsNative

    let isNullOrUndefined (value: obj) =
        isNull value || obj.ReferenceEquals(value, JS.undefined)

    let tryGetProp (names: string list) (value: obj) =
        if isNullOrUndefined value then None
        else
            names
            |> List.tryPick (fun name ->
                let prop: obj = value?(name)
                if isNullOrUndefined prop then None else Some prop)

    let tryGetString (names: string list) (value: obj) =
        tryGetProp names value
        |> Option.map string
        |> Option.filter (fun text -> not (String.IsNullOrWhiteSpace text))

    let tryGetGuid (names: string list) (value: obj) =
        match tryGetString names value with
        | Some text ->
            match Guid.TryParse text with
            | true, parsed -> Some parsed
            | _ -> None
        | None -> None

    let tryGetDate (names: string list) (value: obj) =
        match tryGetString names value with
        | Some text ->
            match DateTime.TryParse text with
            | true, parsed -> Some parsed
            | _ -> None
        | None -> None

    let tryGetInt (names: string list) (value: obj) =
        match tryGetProp names value with
        | Some (:? int as number) -> Some number
        | Some (:? float as number) -> Some (int number)
        | Some (:? string as text) ->
            match Int32.TryParse text with
            | true, parsed -> Some parsed
            | _ -> None
        | _ -> None

    let tryGetFloat (names: string list) (value: obj) =
        match tryGetProp names value with
        | Some (:? float as number) -> Some number
        | Some (:? int as number) -> Some (float number)
        | Some (:? string as text) ->
            match Double.TryParse text with
            | true, parsed -> Some parsed
            | _ -> None
        | _ -> None

    let request (method: string) (url: string) (body: string option) =
        async {
            let headers = createObj [ "Content-Type" ==> "application/json" ]
            let props =
                match body with
                | Some payload -> createObj [ "method" ==> method; "headers" ==> headers; "body" ==> payload ]
                | None -> createObj [ "method" ==> method; "headers" ==> headers ]

            let! response = fetch (url, props) |> Async.AwaitPromise
            let! text = response?text() |> Async.AwaitPromise
            let status: int = response?status
            return status, text
        }

    let parseArray (body: string) (parser: obj -> 'T option) (label: string) =
        try
            let raw = fromJsonString<obj array> body
            let parsed = raw |> Array.choose parser |> Array.toList
            if raw.Length = 0 || parsed.Length > 0 then Ok parsed
            else Error (AppError.Server $"Failed to parse {label} response.")
        with ex ->
            Error (AppError.fromException ex)

    let parseSingle (body: string) (parser: obj -> 'T option) (label: string) =
        try
            let raw = fromJsonString<obj> body
            match parser raw with
            | Some parsed -> Ok parsed
            | None -> Error (AppError.Server $"Failed to parse {label} response.")
        with ex ->
            Error (AppError.fromException ex)

    let parseErrorResponse (status: int) (body: string) : AppError =
        AppError.fromApiResponse status body
