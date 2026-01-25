namespace KeyboardTrainer.Client

open System
open Elmish
open KeyboardTrainer.Shared

module SessionSync =
    type State = {
        Attempts: Map<Guid, int>
        RetryAt: Map<Guid, DateTime>
        LastError: string option
        LastErrorAt: DateTime option
    }

    let init =
        {
            Attempts = Map.empty
            RetryAt = Map.empty
            LastError = None
            LastErrorAt = None
        }

    let private syncLocalSession (local: LocalSessions.LocalSession) =
        async {
            let dto: SessionCreateDto = {
                LessonId = local.LessonId
                Wpm = local.Wpm
                Cpm = local.Cpm
                Accuracy = local.Accuracy
                ErrorCount = local.ErrorCount
                PerKeyErrors = local.PerKeyErrors
            }

            let! result = ApiClient.createSession dto
            match result with
            | Ok _ -> return Ok local.Id
            | Error err -> return Error err
        }

    let pendingForSync (now: DateTime) (state: State) (pending: LocalSessions.LocalSession list) =
        pending
        |> List.filter (fun session ->
            match Map.tryFind session.Id state.RetryAt with
            | Some retryAt -> retryAt <= now
            | None -> true)

    let syncPendingCmd (pending: LocalSessions.LocalSession list) (onSuccess: Guid -> 'msg) (onFailure: Guid * string -> 'msg) =
        if List.isEmpty pending then
            Cmd.none
        else
            pending
            |> List.map (fun local ->
                Cmd.OfAsync.either syncLocalSession local
                    (function
                        | Ok localId -> onSuccess localId
                        | Error err -> onFailure (local.Id, err))
                    (fun ex -> onFailure (local.Id, ex.Message)))
            |> Cmd.batch

    let markSynced (state: State) (localId: Guid) =
        { state with
            Attempts = state.Attempts |> Map.remove localId
            RetryAt = state.RetryAt |> Map.remove localId }

    let recordFailure (now: DateTime) (state: State) (localId: Guid) (error: string) =
        let nextAttempt =
            match Map.tryFind localId state.Attempts with
            | Some count -> count + 1
            | None -> 1

        let backoffSeconds = min 60 (pown 2 (min 5 nextAttempt))
        let nextRetryAt = now.AddSeconds(float backoffSeconds)

        let nextState =
            { state with
                Attempts = state.Attempts |> Map.add localId nextAttempt
                RetryAt = state.RetryAt |> Map.add localId nextRetryAt
                LastError = Some error
                LastErrorAt = Some now }

        nextState, backoffSeconds * 1000
