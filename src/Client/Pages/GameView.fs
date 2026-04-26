namespace KeyboardTrainer.Client.Pages

open System
open Elmish
open Fable.React
open Fable.React.Props
open KeyboardTrainer.Shared
open KeyboardTrainer.Client
open Browser.Types

module GameView =
    type Model = {
        Lesson: LessonDto
        IsSubmitting: bool
        SubmitError: AppError option
        GameStarted: bool
    }

    type Msg =
        | GameStarted
        | GameCompleted of SessionDto
        | GameError of string
        | SubmitSession of SessionDto
        | SessionSubmitted
        | SubmitError of AppError
        | CancelGame
        | ResetGame

    let init lesson =
        {
            Lesson = lesson
            IsSubmitting = false
            SubmitError = None
            GameStarted = false
        }, Cmd.none

    let update msg model =
        match msg with
        | GameStarted ->
            { model with GameStarted = true; SubmitError = None }, Cmd.none

        | GameCompleted session ->
            // Game finished with results
            { model with GameStarted = false }, Cmd.none

        | GameError error ->
            { model with GameStarted = false; SubmitError = Some (AppError.Unknown error) }, Cmd.none

        | SubmitSession session ->
            { model with IsSubmitting = true; SubmitError = None }, Cmd.none

        | SessionSubmitted ->
            { model with IsSubmitting = false }, Cmd.none

        | SubmitError error ->
            { model with IsSubmitting = false; SubmitError = Some error }, Cmd.none

        | CancelGame ->
            model, Cmd.none

        | ResetGame ->
            init model.Lesson

    let view model dispatch =
        let iframeSource =
            let encodedConfig = Uri.EscapeDataString model.Lesson.Content
            $"/games/gem-game/index.html?lessonId={model.Lesson.Id}&config={encodedConfig}"

        div [ ClassName "game-view-container" ] [
            // Header with lesson info and back button
            div [ ClassName "game-header" ] [
                h2 [ ClassName "game-title" ] [ str model.Lesson.Title ]
                div [ ClassName "game-info" ] [
                    span [ ClassName "difficulty" ] [ str (sprintf "Difficulty: %A" model.Lesson.Difficulty) ]
                    span [ ClassName "separator" ] [ str " | " ]
                    span [ ClassName "type" ] [ str (sprintf "Type: %A" model.Lesson.ContentType) ]
                ]
            ]

            // Error display
            match model.SubmitError with
            | Some error ->
                div [ ClassName "alert alert-danger game-error" ] [
                    p [] [ str (AppError.toMessage error) ]
                ]
            | None -> ()

            // Game container (iframe or embedded)
            div [ 
                ClassName "game-content"
                Data ("lesson-id", model.Lesson.Id.ToString())
                Data ("difficulty", model.Lesson.Difficulty.ToString())
            ] [
                iframe [
                    Src iframeSource
                    ClassName "game-frame"
                    Id "game-frame"
                    HTMLAttr.Custom ("sandbox", "allow-same-origin allow-scripts")
                ] []
            ]

            // Game controls
            div [ ClassName "game-controls" ] [
                button [
                    ClassName "btn btn-danger"
                    OnClick (fun _ -> dispatch CancelGame)
                    Disabled model.IsSubmitting
                ] [ str "Cancel Game" ]
            ]
        ]
