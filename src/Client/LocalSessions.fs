namespace KeyboardTrainer.Client

open System
open Fable.Core
open Fable.Core.JsInterop

module LocalSessions =
    [<Emit("window.localStorage")>]
    let private localStorage: obj = jsNative

    let private storageKey = "keyboard-trainer-sessions"
    let private currentVersion = 1

    type LocalSession = {
        Id: Guid
        LessonId: Guid
        Wpm: int
        Cpm: int
        Accuracy: float
        ErrorCount: int
        PerKeyErrors: Map<int, int>
        CreatedAt: DateTime
        SyncedWithServer: bool
    }

    type LocalSessionStored = {
        Id: string
        LessonId: string
        Wpm: int
        Cpm: int
        Accuracy: float
        ErrorCount: int
        PerKeyErrors: ErrorCount list
        CreatedAt: string
        SyncedWithServer: bool
    }

    and ErrorCount = {
        Key: int
        Count: int
    }

    type LocalStoragePayload = {
        Version: int
        Sessions: LocalSessionStored list
    }

    let private toStored (session: LocalSession) : LocalSessionStored =
        {
            Id = session.Id.ToString()
            LessonId = session.LessonId.ToString()
            Wpm = session.Wpm
            Cpm = session.Cpm
            Accuracy = session.Accuracy
            ErrorCount = session.ErrorCount
            PerKeyErrors =
                session.PerKeyErrors
                |> Map.toList
                |> List.map (fun (key, count) -> { Key = key; Count = count })
            CreatedAt = session.CreatedAt.ToString("o")
            SyncedWithServer = session.SyncedWithServer
        }

    let private tryParseGuid (value: string) =
        match Guid.TryParse value with
        | true, id -> Some id
        | _ -> None

    let private tryParseDate (value: string) =
        match DateTime.TryParse value with
        | true, date -> Some date
        | _ -> None

    let private fromStored (stored: LocalSessionStored) : LocalSession option =
        match tryParseGuid stored.Id, tryParseGuid stored.LessonId, tryParseDate stored.CreatedAt with
        | Some id, Some lessonId, Some createdAt ->
            Some {
                Id = id
                LessonId = lessonId
                Wpm = stored.Wpm
                Cpm = stored.Cpm
                Accuracy = stored.Accuracy
                ErrorCount = stored.ErrorCount
                PerKeyErrors =
                    stored.PerKeyErrors
                    |> List.map (fun item -> (item.Key, item.Count))
                    |> Map.ofList
                CreatedAt = createdAt
                SyncedWithServer = stored.SyncedWithServer
            }
        | _ -> None

    let private toJsonString value =
        System.Text.Json.JsonSerializer.Serialize(value)

    let private fromJsonString<'T> (json: string) : 'T =
        System.Text.Json.JsonSerializer.Deserialize<'T>(json)

    let private tryGetItem (key: string) =
        try
            let value: string = localStorage?getItem(key)
            if isNull value || String.IsNullOrWhiteSpace value then None else Some value
        with _ -> None

    let private setItem (key: string) (value: string) =
        localStorage?setItem(key, value) |> ignore

    let load () : LocalSession list =
        let tryParsePayload (json: string) =
            try
                let payload = fromJsonString<LocalStoragePayload> json
                if isNull (box payload) || payload.Version <> currentVersion then
                    None
                else
                    Some payload.Sessions
            with _ -> None

        let tryParseLegacy (json: string) =
            try
                let sessions = fromJsonString<LocalSessionStored list> json
                if isNull (box sessions) then None else Some sessions
            with _ -> None

        match tryGetItem storageKey with
        | Some json ->
            let sessions =
                match tryParsePayload json with
                | Some stored -> stored
                | None ->
                    match tryParseLegacy json with
                    | Some stored -> stored
                    | None -> []

            sessions |> List.choose fromStored
        | None -> []

    let save (sessions: LocalSession list) =
        let payload = { Version = currentVersion; Sessions = sessions |> List.map toStored }
        let json = toJsonString payload
        setItem storageKey json

    let upsert (session: LocalSession) =
        let sessions = load ()
        let updated =
            if sessions |> List.exists (fun s -> s.Id = session.Id) then
                sessions |> List.map (fun s -> if s.Id = session.Id then session else s)
            else
                session :: sessions
        save updated

    let markSynced (sessionId: Guid) =
        let sessions = load ()
        let updated =
            sessions
            |> List.map (fun s ->
                if s.Id = sessionId then { s with SyncedWithServer = true } else s)
        save updated

    let pending () : LocalSession list =
        load () |> List.filter (fun s -> not s.SyncedWithServer)

    let clear () =
        localStorage?removeItem(storageKey) |> ignore
