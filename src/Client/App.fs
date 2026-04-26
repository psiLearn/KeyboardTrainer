namespace KeyboardTrainer.Client

open System
open Elmish
open Elmish.React
open Browser.Dom
open Browser.Types
open Fable.React
open Fable.React.Props
open KeyboardTrainer.Client.Components
open KeyboardTrainer.Client.Pages
open KeyboardTrainer.Shared

module App =
    let private delayCmd (delayMs: int) (msg: Msg) =
        Cmd.OfAsync.perform (fun () -> async {
            do! Async.Sleep delayMs
            return ()
        }) () (fun () -> msg)

    let private ofSub (sub: (Msg -> unit) -> unit) : Cmd<Msg> =
        [ sub ]

    let private registerConnectivity (dispatch: Msg -> unit) =
        let onOnline (_: Event) = dispatch SyncPendingSessions
        let onVisibility (_: Event) =
            if document.visibilityState = "visible" then
                dispatch SyncPendingSessions
        window.addEventListener("online", onOnline)
        window.addEventListener("visibilitychange", onVisibility)

    let init () =
        let startScreenModel, startScreenCmd = StartScreen.init()
        let metricsModel, metricsCmd = Metrics.init()
        let settings = UserSettings.load()
        
        {
            CurrentPage = StartScreen
            StartScreenModel = startScreenModel
            TypingViewModel = None
            MetricsModel = metricsModel
            GameSubmitError = None
            GameStatusMessage = None
            GameSessionPending = false
            SyncState = SessionSync.init
            Settings = settings
        },
        Cmd.batch [
            Cmd.map StartScreenMsg startScreenCmd
            Cmd.map MetricsMsg metricsCmd
            Cmd.ofMsg SyncPendingSessions
            ofSub registerConnectivity
        ]

    let update msg model =
        match msg with
        | StartScreenMsg startScreenMsg ->
            let startScreenModel, cmd = StartScreen.update startScreenMsg model.StartScreenModel
            
            match startScreenMsg with
            | StartScreen.Msg.StartLesson lesson ->
                match lesson.ContentType with
                | ContentType.GemGame ->
                    // Route to game view for gem game content
                    {
                        model with
                            CurrentPage = GameView lesson
                            StartScreenModel = startScreenModel
                            GameStatusMessage = Some "Loading game..."
                    },
                    Cmd.batch [
                        Cmd.map StartScreenMsg cmd
                    ]
                | _ ->
                    // Route to typing view for other content types (Words, Sentences, Probability)
                    let typingModel, typingCmd = TypingView.init lesson
                    {
                        model with
                            CurrentPage = TypingView lesson
                            StartScreenModel = startScreenModel
                            TypingViewModel = Some typingModel
                    },
                    Cmd.batch [
                        Cmd.map StartScreenMsg cmd
                        Cmd.map TypingViewMsg typingCmd
                    ]
            | _ ->
                { model with StartScreenModel = startScreenModel },
                Cmd.map StartScreenMsg cmd

        | TypingViewMsg typingMsg ->
            match model.TypingViewModel with
            | Some typingModel ->
                let typingModel, cmd = TypingView.update typingMsg typingModel
                
                match typingMsg with
                | TypingView.Msg.ResetView ->
                    let newTypingModel, newCmd = TypingView.init typingModel.Lesson
                    {
                        model with
                            CurrentPage = TypingView typingModel.Lesson
                            TypingViewModel = Some newTypingModel
                    },
                    Cmd.map TypingViewMsg newCmd

                | TypingView.Msg.CancelTyping ->
                    {
                        model with
                            CurrentPage = StartScreen
                            TypingViewModel = None
                    },
                    Cmd.none

                | TypingView.Msg.SessionSubmitted _ ->
                    {
                        model with
                            CurrentPage = StartScreen
                            TypingViewModel = None
                    },
                    Cmd.batch [
                        Cmd.ofMsg (MetricsMsg (Metrics.Msg.LoadSessions typingModel.Lesson.Id))
                        Cmd.ofMsg SyncPendingSessions
                    ]

                | _ ->
                    {
                        model with
                            TypingViewModel = Some typingModel
                    },
                    Cmd.map TypingViewMsg cmd
            | None -> model, Cmd.none

        | MetricsMsg metricsMsg ->
            let metricsModel, cmd = Metrics.update metricsMsg model.MetricsModel
            { model with MetricsModel = metricsModel },
            Cmd.map MetricsMsg cmd

        | NavigateToStartScreen ->
            { model with CurrentPage = StartScreen },
            Cmd.ofMsg (StartScreenMsg StartScreen.Msg.LoadLessons)

        | NavigateToMetrics ->
            let pendingCount = LocalSessions.pending () |> List.length
            let metricsModel =
                { model.MetricsModel with
                    PendingLocalSessions = pendingCount
                    LastSyncError = model.SyncState.LastError
                    LastSyncErrorAt = model.SyncState.LastErrorAt }
            { model with CurrentPage = Metrics; MetricsModel = metricsModel },
            Cmd.none

        | NavigateToTypingView lesson ->
            let typingModel, typingCmd = TypingView.init lesson
            {
                model with
                    CurrentPage = TypingView lesson
                    TypingViewModel = Some typingModel
            },
            Cmd.map TypingViewMsg typingCmd

        | NavigateToGameView lesson ->
            { model with CurrentPage = GameView lesson },
            Cmd.none

        | GameMessageReceived msg ->
            // Handle messages from the embedded game iframe
            model, Cmd.none

        | GameSessionSubmitted (sessionId, session) ->
            { model with GameSessionPending = false; GameStatusMessage = Some "Session saved" },
            Cmd.batch [
                Cmd.ofMsg (MetricsMsg (Metrics.Msg.LoadSessions (Guid.Empty)))
                Cmd.ofMsg SyncPendingSessions
            ]

        | GameSessionSubmitFailed (sessionId, error) ->
            { model with GameSessionPending = false; GameSubmitError = Some error },
            Cmd.none

        | SyncPendingSessions ->
            let now = System.DateTime.UtcNow
            let pending = LocalSessions.pending ()
            let eligible = SessionSync.pendingForSync now model.SyncState pending
            let cmd =
                SessionSync.syncPendingCmd eligible PendingSessionSynced PendingSessionSyncFailed
            model, cmd

        | PendingSessionSynced localId ->
            LocalSessions.markSynced localId
            let pendingCount = LocalSessions.pending () |> List.length
            let nextSyncState = SessionSync.markSynced model.SyncState localId
            let nextModel = { model with SyncState = nextSyncState }
            nextModel,
            Cmd.batch [
                Cmd.ofMsg (MetricsMsg (Metrics.Msg.UpdatePendingCount pendingCount))
                Cmd.ofMsg (MetricsMsg (Metrics.Msg.UpdateSyncStatus (None, None)))
            ]

        | PendingSessionSyncFailed (sessionId, error) ->
            let pendingCount = LocalSessions.pending () |> List.length
            let now = System.DateTime.UtcNow
            let nextSyncState, delayMs = SessionSync.recordFailure now model.SyncState sessionId error
            let nextModel = { model with SyncState = nextSyncState }
            nextModel,
            Cmd.batch [
                Cmd.ofMsg (MetricsMsg (Metrics.Msg.UpdatePendingCount pendingCount))
                Cmd.ofMsg (MetricsMsg (Metrics.Msg.UpdateSyncStatus (Some error, Some now)))
                delayCmd delayMs SyncPendingSessions
            ]

        | UpdateSettings settings ->
            UserSettings.save settings
            { model with Settings = settings }, Cmd.none

    let view model dispatch =
        ErrorBoundary.view [
            let isGameMode =
                match model.CurrentPage with
                | GameView _ -> true
                | _ -> false
            let appClass =
                [
                    "app-container"
                    if model.Settings.ColorBlindPalette then "color-blind"
                    if isGameMode then "game-mode"
                ]
                |> String.concat " "
            div [ ClassName appClass ] [
                // Navigation bar
                nav [ ClassName "navbar" ] [
                    div [ ClassName "nav-brand" ] [
                        button [
                            ClassName "brand-button"
                            OnClick (fun _ -> dispatch NavigateToStartScreen)
                        ] [
                            img [
                                ClassName "brand-logo"
                                Src "/logoSmall.svg"
                                Alt "Keyboard Trainer logo"
                            ]
                            span [ ClassName "brand-text" ] [ str "Keyboard Trainer" ]
                        ]
                    ]
                    ul [ ClassName "nav-links" ] [
                        li [] [
                            button [
                                ClassName (if model.CurrentPage = StartScreen then "nav-link active" else "nav-link")
                                OnClick (fun _ -> dispatch NavigateToStartScreen)
                            ] [ str "Start" ]
                        ]
                        li [] [
                            button [
                                ClassName (if model.CurrentPage = Metrics then "nav-link active" else "nav-link")
                                OnClick (fun _ -> dispatch NavigateToMetrics)
                            ] [ str "Statistics" ]
                        ]
                    ]
                ]

                // Main content area
                main [ ClassName (if isGameMode then "main-content game-main-content" else "main-content") ] [
                    match model.CurrentPage with
                    | StartScreen ->
                        let pendingCount = LocalSessions.pending () |> List.length
                        StartScreen.view
                            model.StartScreenModel
                            pendingCount
                            model.SyncState.LastError
                            model.SyncState.LastErrorAt
                            model.Settings
                            (UpdateSettings >> dispatch)
                            (StartScreenMsg >> dispatch)

                    | TypingView lesson ->
                        match model.TypingViewModel with
                        | Some typingModel ->
                            TypingView.view model.Settings typingModel (TypingViewMsg >> dispatch)
                        | None -> div [] [ str "Loading..." ]

                    | GameView lesson ->
                        let iframeSource =
                            let encodedConfig = Uri.EscapeDataString lesson.Content
                            $"/games/gem-game/index.html?lessonId={lesson.Id}&config={encodedConfig}"

                        div [ ClassName "game-view" ] [
                            match model.GameSubmitError with
                            | Some error ->
                                div [ ClassName "game-error alert alert-danger" ] [
                                    p [] [ str (AppError.toMessage error) ]
                                    button [
                                        ClassName "btn btn-sm"
                                        OnClick (fun _ -> dispatch NavigateToStartScreen)
                                    ] [ str "Back to Lessons" ]
                                ]
                            | None -> ()

                            iframe [
                                Src iframeSource
                                ClassName "game-frame"
                                Title "Gem Game Exercise"
                                HTMLAttr.Custom ("allow", "fullscreen")
                            ] []
                        ]

                    | Metrics ->
                        Metrics.view model.MetricsModel (MetricsMsg >> dispatch)
                ]
                if not isGameMode then
                    footer [ ClassName "footer" ] [
                        p [] [ str "© 2024 Keyboard Trainer. Master your typing skills!" ]
                    ]
            ]
        ]
