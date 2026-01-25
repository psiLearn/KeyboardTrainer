namespace KeyboardTrainer.Client.Pages

open System
open Elmish
open Fable.React
open Fable.React.Props
open KeyboardTrainer.Client.Components
open KeyboardTrainer.Shared
open KeyboardTrainer.Client

module StartScreen =
    type Model = {
        Lessons: LessonDto list
        SelectedLesson: LessonDto option
        IsLoading: bool
        Error: string option
        FilterDifficulty: Difficulty option
    }

    type Msg =
        | LoadLessons
        | LessonsLoaded of LessonDto list
        | LessonSelected of LessonDto
        | FilterByDifficulty of Difficulty option
        | ApiError of string
        | ClearError
        | StartLesson

    let init () =
        {
            Lessons = []
            SelectedLesson = None
            IsLoading = true
            Error = None
            FilterDifficulty = None
        }, Cmd.ofMsg LoadLessons

    let private loadLessonsCmd () =
        Cmd.OfAsync.either ApiClient.getAllLessons () (function
            | Ok lessons -> LessonsLoaded lessons
            | Error error -> ApiError error) (fun ex -> ApiError ex.Message)

    let update msg model =
        match msg with
        | LoadLessons ->
            { model with IsLoading = true; Error = None }, loadLessonsCmd ()

        | LessonsLoaded lessons ->
            { model with Lessons = lessons; IsLoading = false; Error = None }, Cmd.none

        | LessonSelected lesson ->
            { model with SelectedLesson = Some lesson }, Cmd.none

        | FilterByDifficulty difficulty ->
            { model with FilterDifficulty = difficulty }, Cmd.none

        | ApiError error ->
            { model with Error = Some error; IsLoading = false }, Cmd.none

        | ClearError ->
            { model with Error = None }, Cmd.none

        | StartLesson ->
            model, Cmd.none

    let view model dispatch =
        div [ ClassName "start-screen" ] [
            h1 [ ClassName "title" ] [ str "Keyboard Trainer" ]
            p [ ClassName "subtitle" ] [ str "Improve your typing speed and accuracy" ]

            if model.IsLoading then
                LoadingSpinner.view (Some "Loading lessons...")
            else
                div [ ClassName "container" ] [
                    // Error message
                    if Option.isSome model.Error then
                        ErrorAlert.view
                            (Option.defaultValue "" model.Error)
                            (Some (fun () -> dispatch LoadLessons))
                            (Some (fun () -> dispatch ClearError))

                    // Filter controls
                    div [ ClassName "filter-section" ] [
                        h3 [] [ str "Filter by Difficulty:" ]
                        div [ ClassName "difficulty-buttons" ] [
                            button [
                                ClassName (if Option.isNone model.FilterDifficulty then "btn btn-active" else "btn")
                                OnClick (fun _ -> dispatch (FilterByDifficulty None))
                            ] [ str "All Levels" ]

                            button [
                                ClassName (if model.FilterDifficulty = Some Difficulty.A1 then "btn btn-active" else "btn")
                                OnClick (fun _ -> dispatch (FilterByDifficulty (Some Difficulty.A1)))
                            ] [ str "A1 (Beginner)" ]

                            button [
                                ClassName (if model.FilterDifficulty = Some Difficulty.A2 then "btn btn-active" else "btn")
                                OnClick (fun _ -> dispatch (FilterByDifficulty (Some Difficulty.A2)))
                            ] [ str "A2 (Elementary)" ]

                            button [
                                ClassName (if model.FilterDifficulty = Some Difficulty.B1 then "btn btn-active" else "btn")
                                OnClick (fun _ -> dispatch (FilterByDifficulty (Some Difficulty.B1)))
                            ] [ str "B1 (Intermediate)" ]

                            button [
                                ClassName (if model.FilterDifficulty = Some Difficulty.B2 then "btn btn-active" else "btn")
                                OnClick (fun _ -> dispatch (FilterByDifficulty (Some Difficulty.B2)))
                            ] [ str "B2 (Upper-Int)" ]

                            button [
                                ClassName (if model.FilterDifficulty = Some Difficulty.C1 then "btn btn-active" else "btn")
                                OnClick (fun _ -> dispatch (FilterByDifficulty (Some Difficulty.C1)))
                            ] [ str "C1 (Advanced)" ]
                        ]
                    ]

                    // Lessons list
                    div [ ClassName "lessons-section" ] [
                        h3 [] [ str $"Available Lessons ({model.Lessons.Length})" ]
                        
                        if List.isEmpty model.Lessons then
                            p [ ClassName "no-lessons" ] [ str "No lessons available" ]
                        else
                            let filteredLessons =
                                model.Lessons
                                |> List.filter (fun lesson ->
                                    match model.FilterDifficulty with
                                    | None -> true
                                    | Some difficulty -> lesson.Difficulty = difficulty
                                )

                            if List.isEmpty filteredLessons then
                                p [ ClassName "no-lessons" ] [ str "No lessons match the selected difficulty" ]
                            else
                                div [ ClassName "lessons-grid" ] [
                                    for lesson in filteredLessons do
                                        let isSelected = model.SelectedLesson |> Option.map (fun l -> l.Id = lesson.Id) |> Option.defaultValue false
                                        div [
                                            ClassName (if isSelected then "lesson-card lesson-card-selected" else "lesson-card")
                                            OnClick (fun _ -> dispatch (LessonSelected lesson))
                                        ] [
                                            h4 [ ClassName "lesson-title" ] [ str lesson.Title ]
                                            p [ ClassName "lesson-difficulty" ] [ str (sprintf "%A" lesson.Difficulty) ]
                                            p [ ClassName "lesson-type" ] [ str (sprintf "%A" lesson.ContentType) ]
                                            p [ ClassName "lesson-language" ] [ str (sprintf "%A" lesson.Language) ]
                                            p [ ClassName "lesson-content-preview" ] [ 
                                                str (if lesson.Content.Length > 50 then lesson.Content.[0..47] + "..." else lesson.Content)
                                            ]
                                        ]
                                ]

                        // Selected lesson details
                        match model.SelectedLesson with
                        | Some lesson ->
                            div [ ClassName "lesson-details" ] [
                                h3 [] [ str lesson.Title ]
                                p [ ClassName "detail-row" ] [
                                    strong [] [ str "Difficulty: " ]
                                    str (sprintf "%A" lesson.Difficulty)
                                ]
                                p [ ClassName "detail-row" ] [
                                    strong [] [ str "Type: " ]
                                    str (sprintf "%A" lesson.ContentType)
                                ]
                                p [ ClassName "detail-row" ] [
                                    strong [] [ str "Language: " ]
                                    str (sprintf "%A" lesson.Language)
                                ]
                                p [ ClassName "detail-row" ] [
                                    strong [] [ str "Content: " ]
                                    str lesson.Content
                                ]
                                p [ ClassName "detail-row" ] [
                                    strong [] [ str "Created: " ]
                                    str (lesson.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"))
                                ]
                                
                                button [
                                    ClassName "btn btn-primary btn-large"
                                    OnClick (fun _ -> dispatch StartLesson)
                                ] [ str "Start Typing" ]
                            ]
                        | None -> ()
                    ]
                ]
        ]
