namespace KeyboardTrainer.Client.Pages

open System
open Browser.Dom
open Browser.Types
open Fable.Core.JsInterop
open Fable.React
open Fable.React.Props
open KeyboardTrainer.Client.Components
open KeyboardTrainer.Shared
open KeyboardTrainer.Client
open KeyboardTrainer.Client.Pages.TypingViewTypes
open KeyboardTrainer.Client.Pages.TypingViewState

module TypingViewView =
    let private tryShortcut model key =
        match model.TypingState, key with
        | NotStarted, "Enter"
        | NotStarted, " "
        | NotStarted, "Spacebar" -> Some StartTyping
        | InProgress, "Escape" -> Some CancelTyping
        | Completed, "Enter" -> Some SubmitSession
        | Completed, "r"
        | Completed, "R"
        | Completed, "Escape" -> Some ResetView
        | _ -> None

    let private lessonHeader model =
        [
            h2 [ ClassName "lesson-title" ] [ str model.Lesson.Title ]
            div [ ClassName "lesson-info" ] [
                span [ ClassName "difficulty" ] [ str (sprintf "%A" model.Lesson.Difficulty) ]
                span [ ClassName "separator" ] [ str " | " ]
                span [ ClassName "content-type" ] [ str (sprintf "%A" model.Lesson.ContentType) ]
            ]
        ]

    let private hiddenInput onInput =
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
            OnInput onInput
        ] []

    let private notStartedView model dispatch =
        if model.IsLoadingTargetContent then
            div [ ClassName "start-section" ] [ LoadingSpinner.view (Some "Preparing exercise...") ]
        else
            match model.TargetContentError with
            | Some error ->
                div [ ClassName "start-section" ] [
                    ErrorAlert.view error (Some (fun () -> dispatch LoadTargetContent)) None
                ]
            | None ->
                div [ ClassName "start-section" ] [
                    div [ ClassName "content-preview" ] [
                        h3 [] [ str "Text to Type:" ]
                        p [ ClassName "lesson-text" ] [ str model.TargetContent ]
                    ]
                    div [ ClassName "button-group" ] [
                        button [ ClassName "btn btn-primary btn-large"; OnClick (fun _ -> dispatch StartTyping) ] [ str "Start Typing" ]
                    ]
                ]

    let private keyboardHighlights settings model : KeyboardView.KeyHighlights option =
        if not settings.ShowKeyboard then
            None
        else
            let nextKey =
                if settings.HighlightNextKey && model.TypingState = InProgress && model.CurrentCharIndex < model.TargetContent.Length then
                    model.TargetContent.[model.CurrentCharIndex] |> KeyboardView.charToKey
                else
                    None
            let nextKeyFingerClass =
                if settings.EnableLetterColors then nextKey |> Option.bind KeyboardView.keyToFingerClass else None
            Some ({
                NextKey = nextKey
                NextKeyFingerClass = nextKeyFingerClass
                UseFingerColors = settings.EnableLetterColors
                LastKey = model.LastKey
                LastKeyIsError = model.LastKeyIsError
            } : KeyboardView.KeyHighlights)


    let private charClassName settings model index currentChar =
        let classes = ResizeArray<string>()
        classes.Add("char")
        if index < model.CurrentCharIndex then
            classes.Add(if Map.containsKey index model.Errors then "char-error" else "char-correct")
        elif index = model.CurrentCharIndex then
            classes.Add("char-current")
            if settings.EnableLetterColors then
                currentChar
                |> KeyboardView.charToKey
                |> Option.bind KeyboardView.keyToFingerClass
                |> Option.iter classes.Add
        else
            classes.Add("char-next")
        String.concat " " classes

    let private splitByWhitespaceSegments (text: string) =
        let segments = ResizeArray<bool * int * int>()
        let mutable index = 0

        while index < text.Length do
            let isWhitespace = Char.IsWhiteSpace text[index]
            let start = index
            while index < text.Length && Char.IsWhiteSpace text[index] = isWhitespace do
                index <- index + 1
            segments.Add(isWhitespace, start, index - start)

        segments |> Seq.toList

    let private inProgressView settings model dispatch =
        let wpm, _, accuracy, _ =
            match model.StartTime with
            | Some startTime -> calculateMetrics model.Lesson model.UserInput startTime (startTime.AddSeconds(float model.ElapsedSeconds)) model.Errors
            | None -> 0, 0, 100.0, 0

        let minutes = model.ElapsedSeconds / 60
        let seconds = model.ElapsedSeconds % 60
        let totalChars = if model.TargetContent.Length > 0 then model.TargetContent.Length else 1
        let lessonTextClass = if settings.EnableLetterColors then "lesson-text-display" else "lesson-text-display no-letter-colors"

        div [ ClassName "typing-section" ] [
            div [ ClassName "progress-bar" ] [
                div [ ClassName "progress-fill"; Style [ Width (sprintf "%.1f%%" ((double model.CurrentCharIndex / double totalChars) * 100.0)) ] ] []
            ]
            p [ ClassName "progress-text" ] [ str (sprintf "%d / %d characters" model.CurrentCharIndex model.TargetContent.Length) ]
            div [ ClassName "typing-area" ] [
                div [ ClassName lessonTextClass ] [
                    for isWhitespace, start, length in splitByWhitespaceSegments model.TargetContent do
                        if isWhitespace then
                            for offset in 0 .. length - 1 do
                                let index = start + offset
                                let currentChar = model.TargetContent.[index]
                                span [ ClassName (charClassName settings model index currentChar) ] [ str (string currentChar) ]
                        else
                            span [ ClassName "word-chunk" ] [
                                for offset in 0 .. length - 1 do
                                    let index = start + offset
                                    let currentChar = model.TargetContent.[index]
                                    span [ ClassName (charClassName settings model index currentChar) ] [ str (string currentChar) ]
                            ]
                ]
            ]
            match keyboardHighlights settings model with
            | Some highlights -> KeyboardView.view highlights
            | None -> ()
            div [ ClassName "typing-stats" ] [
                span [ ClassName "stat" ] [ strong [] [ str "Errors: " ]; str (string (Map.fold (fun acc _ count -> acc + count) 0 model.Errors)) ]
                span [ ClassName "stat" ] [ strong [] [ str "Time: " ]; str (sprintf "%02d:%02d" minutes seconds) ]
                span [ ClassName "stat" ] [ strong [] [ str "WPM: " ]; str (string wpm) ]
                span [ ClassName "stat" ] [ strong [] [ str "Accuracy: " ]; str (sprintf "%.1f%%" accuracy) ]
            ]
            button [ ClassName "btn btn-danger"; OnClick (fun _ -> dispatch CancelTyping) ] [ str "Cancel" ]
        ]

    let private completedView model dispatch =
        let wpm, cpm, accuracy, errorCount =
            match model.StartTime, model.EndTime with
            | Some startTime, Some endTime -> calculateMetrics model.Lesson model.UserInput startTime endTime model.Errors
            | _ -> 0, 0, 0.0, 0

        div [ ClassName "completion-section" ] [
            div [ ClassName "confetti" ] [ for index in 1 .. 12 do span [ Key (sprintf "confetti-%d" index) ] [] ]
            h3 [] [ str "Typing Complete!" ]
            match model.SubmitError with
            | Some error -> ErrorAlert.view error (Some (fun () -> dispatch SubmitSession)) (Some (fun () -> dispatch ClearSubmitError))
            | None -> ()
            div [ ClassName "results-summary" ] [
                div [ ClassName "result-stat" ] [ span [ ClassName "label" ] [ str "Words Per Minute:" ]; span [ ClassName "value" ] [ str (string wpm) ] ]
                div [ ClassName "result-stat" ] [ span [ ClassName "label" ] [ str "Characters Per Minute:" ]; span [ ClassName "value" ] [ str (string cpm) ] ]
                div [ ClassName "result-stat" ] [ span [ ClassName "label" ] [ str "Accuracy:" ]; span [ ClassName "value" ] [ str (sprintf "%.1f%%" accuracy) ] ]
                div [ ClassName "result-stat" ] [ span [ ClassName "label" ] [ str "Errors:" ]; span [ ClassName "value" ] [ str (string errorCount) ] ]
            ]
            div [ ClassName "button-group" ] [
                button [ ClassName "btn btn-primary"; OnClick (fun _ -> dispatch SubmitSession); Disabled model.IsSubmitting ] [ str (if model.IsSubmitting then "Submitting..." else "Submit Results") ]
                button [ ClassName "btn btn-secondary"; OnClick (fun _ -> dispatch ResetView) ] [ str "Try Again" ]
            ]
        ]

    let view (settings: UserSettings) model dispatch =
        let onKeyDown (ev: KeyboardEvent) =
            if not (ev.ctrlKey || ev.altKey || ev.metaKey) then
                match tryShortcut model ev.key with
                | Some msg ->
                    ev.preventDefault()
                    dispatch msg
                | None when model.TypingState = InProgress && ev.key = "Backspace" ->
                    ev.preventDefault()
                    dispatch Backspace
                | None when model.TypingState = InProgress && ev.key = "Enter" ->
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
            if not (isNull el) then el?focus() |> ignore

        div [ ClassName "typing-view"; OnClick focusInput; OnKeyDown onKeyDown ] [
            yield! lessonHeader model
            hiddenInput onInput
            match model.TypingState with
            | NotStarted -> notStartedView model dispatch
            | InProgress -> inProgressView settings model dispatch
            | Completed -> completedView model dispatch
        ]
