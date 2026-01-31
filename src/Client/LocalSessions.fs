namespace KeyboardTrainer.Client

open System
open Browser.Dom
open Fable.Core
open Fable.Core.JsInterop

module LocalSessions =
    [<Emit("window.localStorage")>]
    let private localStorage: obj = jsNative

    let private storageKey = "keyboard-trainer-sessions"
    let private currentVersion = 1
    let private maxStoredSessions = 1000

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
        Sessions: LocalSessionStored array
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
        JS.JSON.stringify value

    let private fromJsonString<'T> (json: string) : 'T =
        JS.JSON.parse json |> unbox<'T>

    let private tryGetProp (names: string list) (value: obj) =
        if isNull value then None
        else
            names
            |> List.tryPick (fun name ->
                let prop: obj = value?(name)
                if isNull prop then None else Some prop)

    let private tryGetIntProp (names: string list) (value: obj) =
        match tryGetProp names value with
        | Some (:? int as number) -> Some number
        | Some (:? float as number) -> Some (int number)
        | Some (:? string as text) ->
            match Int32.TryParse text with
            | true, parsed -> Some parsed
            | _ -> None
        | _ -> None

    let private tryParseStoredArray (value: obj) =
        try
            Some (unbox<LocalSessionStored array> value)
        with _ ->
            try
                let list = unbox<LocalSessionStored list> value
                Some (List.toArray list)
            with _ ->
                try
                    let raw: obj array = unbox value
                    raw
                    |> Array.choose (fun item ->
                        try Some (unbox<LocalSessionStored> item) with _ -> None)
                    |> Some
                with _ ->
                    None

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
                let payloadObj = fromJsonString<obj> json
                match tryGetIntProp [ "Version"; "version" ] payloadObj with
                | Some version when version = currentVersion ->
                    match tryGetProp [ "Sessions"; "sessions" ] payloadObj with
                    | Some sessionsValue -> tryParseStoredArray sessionsValue
                    | None -> None
                | _ -> None
            with _ -> None

        let tryParseLegacy (json: string) =
            try
                let sessionsObj = fromJsonString<obj> json
                tryParseStoredArray sessionsObj
            with _ -> None

        match tryGetItem storageKey with
        | Some json ->
            let sessions =
                match tryParsePayload json with
                | Some stored -> stored
                | None ->
                    match tryParseLegacy json with
                    | Some stored -> stored
                    | None -> [||]

            sessions |> Array.toList |> List.choose fromStored
        | None -> []

    let private normalize (sessions: LocalSession list) =
        let sorted = sessions |> List.sortByDescending (fun session -> session.CreatedAt)
        let unsynced, synced = sorted |> List.partition (fun session -> not session.SyncedWithServer)
        let retainedUnsynced =
            if unsynced.Length > maxStoredSessions then
                console.warn($"LocalSessions: {unsynced.Length} unsynced sessions exceed retention cap of {maxStoredSessions}. Keeping all unsynced.")
                unsynced
            else
                unsynced
        let remaining = maxStoredSessions - retainedUnsynced.Length
        let retainedSynced =
            if remaining > 0 then
                synced |> List.truncate remaining
            else
                []
        retainedUnsynced @ retainedSynced

    let save (sessions: LocalSession list) =
        let normalized = normalize sessions
        let payload = { Version = currentVersion; Sessions = normalized |> List.map toStored |> List.toArray }
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
