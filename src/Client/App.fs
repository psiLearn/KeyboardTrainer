namespace KeyboardTrainer.Client

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
    type Page =
        | StartScreen
        | TypingView of LessonDto
        | Metrics

    type Model = {
        CurrentPage: Page
        StartScreenModel: StartScreen.Model
        TypingViewModel: TypingView.Model option
        MetricsModel: Metrics.Model
        SyncState: SessionSync.State
        Settings: UserSettings
    }

    type Msg =
        | StartScreenMsg of StartScreen.Msg
        | TypingViewMsg of TypingView.Msg
        | MetricsMsg of Metrics.Msg
        | NavigateToStartScreen
        | NavigateToMetrics
        | NavigateToTypingView of LessonDto
        | SyncPendingSessions
        | PendingSessionSynced of System.Guid
        | PendingSessionSyncFailed of System.Guid * string
        | UpdateSettings of UserSettings

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
            let appClass =
                if model.Settings.ColorBlindPalette then "app-container color-blind"
                else "app-container"
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
                main [ ClassName "main-content" ] [
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

                    | Metrics ->
                        Metrics.view model.MetricsModel (MetricsMsg >> dispatch)
                ]
                // Footer
                footer [ ClassName "footer" ] [
                    p [] [ str "© 2024 Keyboard Trainer. Master your typing skills!" ]
                ]
            ]
        ]
