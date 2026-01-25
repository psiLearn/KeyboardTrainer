namespace KeyboardTrainer.Client.Pages

open System
open Browser.Dom
open Browser.Types
open Elmish
open Fable.Core.JsInterop
open Fable.React
open Fable.React.Props
open KeyboardTrainer.Client.Components
open KeyboardTrainer.Shared
open KeyboardTrainer.Client

module TypingView =
    type TypingState =
        | NotStarted
        | InProgress
        | Completed

    type Model = {
        Lesson: LessonDto
        TypingState: TypingState
        StartTime: DateTime option
        EndTime: DateTime option
        UserInput: string
        CurrentCharIndex: int
        Errors: Map<int, int>
        IsSubmitting: bool
        SubmitError: string option
        PendingLocalSessionId: Guid option
    }

    type Msg =
        | StartTyping
        | CharacterTyped of char
        | Backspace
        | SubmitSession
        | SessionSubmitted of SessionDto
        | SubmitError of string
        | ClearSubmitError
        | ResetView
        | CancelTyping

    let calculateMetrics (lesson: LessonDto) (input: string) (startTime: DateTime) (endTime: DateTime) (errors: Map<int, int>) =
        let duration = (endTime - startTime).TotalSeconds
        let words = (double input.Length) / 5.0
        let wpm = if duration > 0.0 then int (words / (duration / 60.0)) else 0
        let cpm = if duration > 0.0 then int ((double input.Length) / (duration / 60.0)) else 0
        let totalErrors = Map.fold (fun acc _ count -> acc + count) 0 errors
        let accuracy = 
            if input.Length = 0 then 100.0
            else
                let correctChars = input.Length - totalErrors
                ((double correctChars) / (double input.Length)) * 100.0

        wpm, cpm, accuracy, totalErrors

    let init lesson =
        {
            Lesson = lesson
            TypingState = NotStarted
            StartTime = None
            EndTime = None
            UserInput = ""
            CurrentCharIndex = 0
            Errors = Map.empty
            IsSubmitting = false
            SubmitError = None
            PendingLocalSessionId = None
        }, Cmd.none

    let private submitSessionCmd (dto: SessionCreateDto) =
        Cmd.OfAsync.either ApiClient.createSession dto (function
            | Ok session -> SessionSubmitted session
            | Error error -> SubmitError error) (fun ex -> SubmitError ex.Message)

    let update msg model =
        match msg with
        | StartTyping ->
            { model with 
                TypingState = InProgress
                StartTime = Some DateTime.Now
                UserInput = ""
                CurrentCharIndex = 0
                Errors = Map.empty
            }, Cmd.none

        | CharacterTyped char ->
            if model.TypingState = InProgress && model.CurrentCharIndex < model.Lesson.Content.Length then
                let expectedChar = model.Lesson.Content.[model.CurrentCharIndex]
                let newErrors = 
                    if expectedChar <> char then
                        Map.change model.CurrentCharIndex (fun existing ->
                            match existing with
                            | Some count -> Some (count + 1)
                            | None -> Some 1) model.Errors
                    else
                        model.Errors
                
                let newInput = model.UserInput + string char
                let newIndex = model.CurrentCharIndex + 1
                
                if newIndex >= model.Lesson.Content.Length then
                    { model with 
                        UserInput = newInput
                        CurrentCharIndex = newIndex
                        Errors = newErrors
                        TypingState = Completed
                        EndTime = Some DateTime.Now
                    }, Cmd.none
                else
                    { model with 
                        UserInput = newInput
                        CurrentCharIndex = newIndex
                        Errors = newErrors
                    }, Cmd.none
            else
                model, Cmd.none

        | Backspace ->
            if model.TypingState = InProgress && model.CurrentCharIndex > 0 then
                let newIndex = model.CurrentCharIndex - 1
                let newInput = 
                    if newIndex > 0 then
                        model.UserInput.[0..newIndex - 1]
                    else
                        ""
                let newErrors = Map.remove newIndex model.Errors
                
                { model with 
                    UserInput = newInput
                    CurrentCharIndex = newIndex
                    Errors = newErrors
                }, Cmd.none
            else
                model, Cmd.none

        | SubmitSession ->
            match (model.StartTime, model.EndTime) with
            | (Some startTime, Some endTime) ->
                let wpm, cpm, accuracy, errorCount = calculateMetrics model.Lesson model.UserInput startTime endTime model.Errors
                
                let sessionDto: SessionCreateDto = {
                    LessonId = model.Lesson.Id
                    Wpm = wpm
                    Cpm = cpm
                    Accuracy = accuracy
                    ErrorCount = errorCount
                    PerKeyErrors = model.Errors
                }

                let localSessionId =
                    match model.PendingLocalSessionId with
                    | Some id -> id
                    | None -> Guid.NewGuid()

                let localSession: LocalSessions.LocalSession = {
                    Id = localSessionId
                    LessonId = model.Lesson.Id
                    Wpm = wpm
                    Cpm = cpm
                    Accuracy = accuracy
                    ErrorCount = errorCount
                    PerKeyErrors = model.Errors
                    CreatedAt = DateTime.UtcNow
                    SyncedWithServer = false
                }

                LocalSessions.upsert localSession
                
                { model with IsSubmitting = true; SubmitError = None; PendingLocalSessionId = Some localSessionId }, submitSessionCmd sessionDto

            | _ -> model, Cmd.none

        | SessionSubmitted session ->
            match model.PendingLocalSessionId with
            | Some localSessionId ->
                LocalSessions.markSynced localSessionId
            | None -> ()

            { model with IsSubmitting = false; SubmitError = None; PendingLocalSessionId = None }, Cmd.none

        | SubmitError error ->
            { model with IsSubmitting = false; SubmitError = Some error }, Cmd.none

        | ClearSubmitError ->
            { model with SubmitError = None }, Cmd.none

        | ResetView ->
            init model.Lesson

        | CancelTyping ->
            init model.Lesson

    let view model dispatch =
        let onKeyDown (ev: KeyboardEvent) =
            if model.TypingState = InProgress then
                if ev.ctrlKey || ev.altKey || ev.metaKey then
                    ()
                else
                    match ev.key with
                    | "Backspace" ->
                        ev.preventDefault()
                        dispatch Backspace
                    | "Enter" ->
                        ev.preventDefault()
                        dispatch (CharacterTyped '\n')
                    | _ -> ()

        let onInput (ev: Event) =
            let value: string = ev.target?value
            if not (String.IsNullOrEmpty value) then
                if model.TypingState = InProgress then
                    value |> Seq.iter (fun c -> dispatch (CharacterTyped c))
                ev.target?value <- ""

        let focusInput (_: MouseEvent) =
            let el = document.getElementById("typing-input")
            if not (isNull el) then
                el?focus() |> ignore

        div [
            ClassName "typing-view"
            OnClick focusInput
        ] [
            h2 [ ClassName "lesson-title" ] [ str model.Lesson.Title ]

            textarea [
                Id "typing-input"
                ClassName "typing-input"
                AutoFocus true
                HTMLAttr.Custom ("aria-label", "Typing input")
                SpellCheck false
                Style [
                    Position PositionOptions.Absolute
                    Left "-9999px"
                    Width "1px"
                    Height "1px"
                    Opacity 0.0
                ]
                OnKeyDown onKeyDown
                OnInput onInput
            ] []

            div [ ClassName "lesson-info" ] [
                span [ ClassName "difficulty" ] [ str (sprintf "%A" model.Lesson.Difficulty) ]
                span [ ClassName "separator" ] [ str " | " ]
                span [ ClassName "content-type" ] [ str (sprintf "%A" model.Lesson.ContentType) ]
            ]

            match model.TypingState with
            | NotStarted ->
                div [ ClassName "start-section" ] [
                    div [ ClassName "content-preview" ] [
                        h3 [] [ str "Text to Type:" ]
                        p [ ClassName "lesson-text" ] [ str model.Lesson.Content ]
                    ]
                    
                    div [ ClassName "button-group" ] [
                        button [
                            ClassName "btn btn-primary btn-large"
                            OnClick (fun _ -> dispatch StartTyping)
                        ] [ str "Start Typing" ]
                    ]
                ]

            | InProgress ->
                div [ ClassName "typing-section" ] [
                    div [ ClassName "progress-bar" ] [
                        div [
                            ClassName "progress-fill"
                            Style [ Width (sprintf "%.1f%%" ((double model.CurrentCharIndex / double model.Lesson.Content.Length) * 100.0)) ]
                        ] []
                    ]
                    p [ ClassName "progress-text" ] [ 
                        str (sprintf "%d / %d characters" model.CurrentCharIndex model.Lesson.Content.Length)
                    ]

                    div [ ClassName "typing-area" ] [
                        div [ ClassName "lesson-text-display" ] [
                            // Display lesson text with character-by-character highlighting
                            for i in 0 .. model.Lesson.Content.Length - 1 do
                                let char = model.Lesson.Content.[i]
                                let className = 
                                    if i < model.CurrentCharIndex then
                                        if Map.containsKey i model.Errors then
                                            "char char-error"
                                        else
                                            "char char-correct"
                                    elif i = model.CurrentCharIndex then
                                        "char char-current"
                                    else
                                        "char char-next"
                                
                                span [ ClassName className ] [ str (string char) ]
                        ]
                    ]

                    div [ ClassName "typing-stats" ] [
                        span [ ClassName "stat" ] [
                            strong [] [ str "Errors: " ]
                            str (string (Map.fold (fun acc _ count -> acc + count) 0 model.Errors))
                        ]
                    ]

                    button [
                        ClassName "btn btn-danger"
                        OnClick (fun _ -> dispatch CancelTyping)
                    ] [ str "Cancel" ]
                ]

            | Completed ->
                let wpm, cpm, accuracy, errorCount = 
                    match (model.StartTime, model.EndTime) with
                    | (Some startTime, Some endTime) ->
                        calculateMetrics model.Lesson model.UserInput startTime endTime model.Errors
                    | _ -> 0, 0, 0.0, 0

                div [ ClassName "completion-section" ] [
                    h3 [] [ str "Typing Complete!" ]

                    if Option.isSome model.SubmitError then
                        ErrorAlert.view
                            (Option.defaultValue "" model.SubmitError)
                            (Some (fun () -> dispatch SubmitSession))
                            (Some (fun () -> dispatch ClearSubmitError))

                    div [ ClassName "results-summary" ] [
                        div [ ClassName "result-stat" ] [
                            span [ ClassName "label" ] [ str "Words Per Minute:" ]
                            span [ ClassName "value" ] [ str (string wpm) ]
                        ]
                        div [ ClassName "result-stat" ] [
                            span [ ClassName "label" ] [ str "Characters Per Minute:" ]
                            span [ ClassName "value" ] [ str (string cpm) ]
                        ]
                        div [ ClassName "result-stat" ] [
                            span [ ClassName "label" ] [ str "Accuracy:" ]
                            span [ ClassName "value" ] [ str (sprintf "%.1f%%" accuracy) ]
                        ]
                        div [ ClassName "result-stat" ] [
                            span [ ClassName "label" ] [ str "Errors:" ]
                            span [ ClassName "value" ] [ str (string errorCount) ]
                        ]
                    ]

                    div [ ClassName "button-group" ] [
                        button [
                            ClassName "btn btn-primary"
                            OnClick (fun _ -> dispatch SubmitSession)
                            Disabled model.IsSubmitting
                        ] [ str (if model.IsSubmitting then "Submitting..." else "Submit Results") ]

                        button [
                            ClassName "btn btn-secondary"
                            OnClick (fun _ -> dispatch ResetView)
                        ] [ str "Try Again" ]
                    ]
                ]
        ]
