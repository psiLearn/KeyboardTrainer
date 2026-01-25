namespace KeyboardTrainer.Client.Pages

open System
open Elmish
open Fable.React
open Fable.React.Props
open KeyboardTrainer.Client.Components
open KeyboardTrainer.Shared
open KeyboardTrainer.Client

module Metrics =
    type Model = {
        LessonId: Guid option
        Sessions: SessionDto list
        IsLoading: bool
        Error: string option
        SelectedMetric: MetricType
        PendingLocalSessions: int
    }

    and MetricType =
        | AllTime
        | ByDifficulty
        | ByLesson
        | Trends

    type Msg =
        | LoadSessions of Guid
        | SessionsLoaded of SessionDto list
        | ChangeMetricView of MetricType
        | ApiError of string
        | ClearError
        | ClearLocalData
        | UpdatePendingCount of int
        | Refresh

    let init () =
        let pendingCount = LocalSessions.pending () |> List.length
        {
            LessonId = None
            Sessions = []
            IsLoading = false
            Error = None
            SelectedMetric = AllTime
            PendingLocalSessions = pendingCount
        }, Cmd.none

    let private loadSessionsCmd lessonId =
        Cmd.OfAsync.either ApiClient.getSessionsByLesson lessonId (function
            | Ok sessions -> SessionsLoaded sessions
            | Error error -> ApiError error) (fun ex -> ApiError ex.Message)

    let update msg model =
        match msg with
        | LoadSessions lessonId ->
            let pendingCount = LocalSessions.pending () |> List.length
            { model with IsLoading = true; Error = None; LessonId = Some lessonId; PendingLocalSessions = pendingCount }, loadSessionsCmd lessonId

        | SessionsLoaded sessions ->
            let pendingCount = LocalSessions.pending () |> List.length
            { model with Sessions = sessions; IsLoading = false; Error = None; PendingLocalSessions = pendingCount }, Cmd.none

        | ChangeMetricView metric ->
            { model with SelectedMetric = metric }, Cmd.none

        | ApiError error ->
            { model with Error = Some error; IsLoading = false }, Cmd.none

        | ClearError ->
            { model with Error = None }, Cmd.none

        | ClearLocalData ->
            LocalSessions.clear ()
            { model with PendingLocalSessions = 0 }, Cmd.none

        | UpdatePendingCount count ->
            { model with PendingLocalSessions = count }, Cmd.none

        | Refresh ->
            match model.LessonId with
            | Some lessonId ->
                let pendingCount = LocalSessions.pending () |> List.length
                { model with IsLoading = true; Error = None; PendingLocalSessions = pendingCount }, loadSessionsCmd lessonId
            | None -> model, Cmd.none

    let calculateStats (sessions: SessionDto list) =
        if List.isEmpty sessions then
            {| AvgWpm = 0; AvgCpm = 0; AvgAccuracy = 0.0; TotalSessions = 0; TotalErrors = 0 |}
        else
            let avgWpm = sessions |> List.map (fun s -> s.Wpm) |> List.average |> int
            let avgCpm = sessions |> List.map (fun s -> s.Cpm) |> List.average |> int
            let avgAccuracy = sessions |> List.map (fun s -> s.Accuracy) |> List.average
            let totalSessions = List.length sessions
            let totalErrors = sessions |> List.map (fun s -> s.ErrorCount) |> List.sum
            {| AvgWpm = avgWpm; AvgCpm = avgCpm; AvgAccuracy = avgAccuracy; TotalSessions = totalSessions; TotalErrors = totalErrors |}

    let view model dispatch =
        div [ ClassName "metrics-view" ] [
            h2 [ ClassName "page-title" ] [ str "Your Statistics" ]

            if model.IsLoading then
                LoadingSpinner.view (Some "Loading metrics...")
            else
                div [ ClassName "metrics-container" ] [
                    // Error message
                    if Option.isSome model.Error then
                        ErrorAlert.view
                            (Option.defaultValue "" model.Error)
                            (Some (fun () -> dispatch Refresh))
                            (Some (fun () -> dispatch ClearError))

                    if model.PendingLocalSessions > 0 then
                        div [ ClassName "local-sync-status" ] [
                            p [] [ str (sprintf "Pending local sessions: %d" model.PendingLocalSessions) ]
                            button [
                                ClassName "btn btn-secondary"
                                OnClick (fun _ -> dispatch ClearLocalData)
                            ] [ str "Clear local data" ]
                        ]

                    // Metric type selector
                    div [ ClassName "metric-selector" ] [
                        button [
                            ClassName (if model.SelectedMetric = AllTime then "btn btn-active" else "btn")
                            OnClick (fun _ -> dispatch (ChangeMetricView AllTime))
                        ] [ str "All Time" ]

                        button [
                            ClassName (if model.SelectedMetric = ByDifficulty then "btn btn-active" else "btn")
                            OnClick (fun _ -> dispatch (ChangeMetricView ByDifficulty))
                        ] [ str "By Difficulty" ]

                        button [
                            ClassName (if model.SelectedMetric = ByLesson then "btn btn-active" else "btn")
                            OnClick (fun _ -> dispatch (ChangeMetricView ByLesson))
                        ] [ str "By Lesson" ]

                        button [
                            ClassName (if model.SelectedMetric = Trends then "btn btn-active" else "btn")
                            OnClick (fun _ -> dispatch (ChangeMetricView Trends))
                        ] [ str "Trends" ]

                        button [
                            ClassName "btn btn-secondary"
                            OnClick (fun _ -> dispatch Refresh)
                        ] [ str "Refresh" ]
                    ]

                    // Stats summary
                    if not (List.isEmpty model.Sessions) then
                        let stats = calculateStats model.Sessions
                        
                        div [ ClassName "stats-summary" ] [
                            div [ ClassName "stat-card" ] [
                                h4 [] [ str "Average WPM" ]
                                p [ ClassName "stat-value" ] [ str (string stats.AvgWpm) ]
                                p [ ClassName "stat-unit" ] [ str "words per minute" ]
                            ]

                            div [ ClassName "stat-card" ] [
                                h4 [] [ str "Average CPM" ]
                                p [ ClassName "stat-value" ] [ str (string stats.AvgCpm) ]
                                p [ ClassName "stat-unit" ] [ str "characters per minute" ]
                            ]

                            div [ ClassName "stat-card" ] [
                                h4 [] [ str "Average Accuracy" ]
                                p [ ClassName "stat-value" ] [ str (sprintf "%.1f%%" stats.AvgAccuracy) ]
                                p [ ClassName "stat-unit" ] [ str "correct characters" ]
                            ]

                            div [ ClassName "stat-card" ] [
                                h4 [] [ str "Total Sessions" ]
                                p [ ClassName "stat-value" ] [ str (string stats.TotalSessions) ]
                                p [ ClassName "stat-unit" ] [ str "completed" ]
                            ]

                            div [ ClassName "stat-card" ] [
                                h4 [] [ str "Total Errors" ]
                                p [ ClassName "stat-value" ] [ str (string stats.TotalErrors) ]
                                p [ ClassName "stat-unit" ] [ str "across all sessions" ]
                            ]
                        ]

                        // Sessions table
                        div [ ClassName "sessions-table" ] [
                            h3 [] [ str "Recent Sessions" ]
                            
                            if List.isEmpty model.Sessions then
                                p [ ClassName "no-data" ] [ str "No sessions recorded yet" ]
                            else
                                table [] [
                                    thead [] [
                                        tr [] [
                                            th [] [ str "#" ]
                                            th [] [ str "Date" ]
                                            th [] [ str "WPM" ]
                                            th [] [ str "CPM" ]
                                            th [] [ str "Accuracy" ]
                                            th [] [ str "Errors" ]
                                        ]
                                    ]
                                    tbody [] [
                                        for (idx, session) in List.mapi (fun i s -> (i + 1, s)) (List.rev model.Sessions) do
                                            tr [] [
                                                td [] [ str (string idx) ]
                                                td [] [ str (session.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")) ]
                                                td [ ClassName "metric-wpm" ] [ str (string session.Wpm) ]
                                                td [ ClassName "metric-cpm" ] [ str (string session.Cpm) ]
                                                td [ ClassName "metric-accuracy" ] [ str (sprintf "%.1f%%" session.Accuracy) ]
                                                td [ ClassName "metric-errors" ] [ str (string session.ErrorCount) ]
                                            ]
                                    ]
                                ]
                        ]
                    else
                        div [ ClassName "no-sessions" ] [
                            h3 [] [ str "No Sessions Yet" ]
                            p [] [ str "Complete a typing lesson to see your statistics." ]
                        ]
                ]        ]
