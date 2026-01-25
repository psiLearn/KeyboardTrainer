namespace KeyboardTrainer.Client

open Elmish
open Elmish.React
open Fable.React
open Fable.React.Props
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
    }

    type Msg =
        | StartScreenMsg of StartScreen.Msg
        | TypingViewMsg of TypingView.Msg
        | MetricsMsg of Metrics.Msg
        | NavigateToStartScreen
        | NavigateToMetrics
        | NavigateToTypingView of LessonDto

    let init () =
        let startScreenModel, startScreenCmd = StartScreen.init()
        let metricsModel, metricsCmd = Metrics.init()
        
        {
            CurrentPage = StartScreen
            StartScreenModel = startScreenModel
            TypingViewModel = None
            MetricsModel = metricsModel
        },
        Cmd.batch [
            Cmd.map StartScreenMsg startScreenCmd
            Cmd.map MetricsMsg metricsCmd
        ]

    let update msg model =
        match msg with
        | StartScreenMsg startScreenMsg ->
            let startScreenModel, cmd = StartScreen.update startScreenMsg model.StartScreenModel
            
            match startScreenMsg with
            | StartScreen.Msg.StartLesson ->
                match model.StartScreenModel.SelectedLesson with
                | Some lesson ->
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
                | None -> { model with StartScreenModel = startScreenModel }, Cmd.map StartScreenMsg cmd
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
                    Cmd.ofMsg (MetricsMsg (Metrics.Msg.LoadSessions typingModel.Lesson.Id))

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
            { model with CurrentPage = Metrics },
            Cmd.none

        | NavigateToTypingView lesson ->
            let typingModel, typingCmd = TypingView.init lesson
            {
                model with
                    CurrentPage = TypingView lesson
                    TypingViewModel = Some typingModel
            },
            Cmd.map TypingViewMsg typingCmd

    let view model dispatch =
        div [ ClassName "app-container" ] [
            // Navigation bar
            nav [ ClassName "navbar" ] [
                div [ ClassName "nav-brand" ] [
                    h1 [ OnClick (fun _ -> dispatch NavigateToStartScreen) ] [ str "Keyboard Trainer" ]
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
                    StartScreen.view model.StartScreenModel (StartScreenMsg >> dispatch)

                | TypingView lesson ->
                    match model.TypingViewModel with
                    | Some typingModel ->
                        TypingView.view typingModel (TypingViewMsg >> dispatch)
                    | None -> div [] [ str "Loading..." ]

                | Metrics ->
                    Metrics.view model.MetricsModel (MetricsMsg >> dispatch)
            ]
            // Footer
            footer [ ClassName "footer" ] [
                p [] [ str "© 2024 Keyboard Trainer. Master your typing skills!" ]
            ]
        ]
