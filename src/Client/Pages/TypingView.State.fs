namespace KeyboardTrainer.Client.Pages

open System
open Elmish
open KeyboardTrainer.Shared
open KeyboardTrainer.Client
open KeyboardTrainer.Client.Components
open KeyboardTrainer.Client.Pages.TypingViewTypes

module TypingViewState =
    let calculateMetrics (_lesson: LessonDto) (input: string) (startTime: DateTime) (endTime: DateTime) (errors: Map<int, int>) =
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

    let private loadTargetContentCmd (lesson: LessonDto) =
        Cmd.OfAsync.either ApiClient.getLessonExercise lesson.Id (function
            | Ok exercise -> TargetContentLoaded exercise.Content
            | Error error -> TargetContentLoadFailed error) (fun ex -> TargetContentLoadFailed (AppError.fromException ex))

    let init lesson =
        let isProbability = lesson.ContentType = ContentType.Probability
        {
            Lesson = lesson
            TargetContent = if isProbability then "" else lesson.Content
            IsLoadingTargetContent = isProbability
            TargetContentError = None
            TypingState = NotStarted
            StartTime = None
            EndTime = None
            UserInput = ""
            CurrentCharIndex = 0
            Errors = Map.empty
            IsSubmitting = false
            SubmitError = None
            PendingLocalSessionId = None
            ElapsedSeconds = 0
            LastKey = None
            LastKeyIsError = None
        }, (if isProbability then loadTargetContentCmd lesson else Cmd.none)

    let private tickCmd =
        Cmd.OfAsync.perform (fun () -> async {
            do! Async.Sleep 1000
            return ()
        }) () (fun () -> Tick)

    let private submitSessionCmd (dto: SessionCreateDto) =
        Cmd.OfAsync.either ApiClient.createSession dto (function
            | Ok session -> SessionSubmitted session
            | Error error -> SubmitError error) (fun ex -> SubmitError (AppError.fromException ex))

    let private startTyping model =
        if model.IsLoadingTargetContent || String.IsNullOrWhiteSpace model.TargetContent then
            model, Cmd.none
        elif model.TypingState = InProgress then
            model, Cmd.none
        else
            { model with
                TypingState = InProgress
                StartTime = Some DateTime.Now
                UserInput = ""
                CurrentCharIndex = 0
                Errors = Map.empty
                ElapsedSeconds = 0
                LastKey = None
                LastKeyIsError = None
            }, tickCmd

    let private applyCharacter char model =
        if model.TypingState <> InProgress || model.CurrentCharIndex >= model.TargetContent.Length then
            model, Cmd.none
        else
            let expectedChar = model.TargetContent.[model.CurrentCharIndex]
            let newErrors =
                if expectedChar <> char then
                    Map.change model.CurrentCharIndex (function Some count -> Some (count + 1) | None -> Some 1) model.Errors
                else
                    model.Errors

            let newInput = model.UserInput + string char
            let newIndex = model.CurrentCharIndex + 1
            let lastKey = KeyboardView.charToKey char
            let lastKeyIsError = lastKey |> Option.map (fun _ -> expectedChar <> char)

            if newIndex >= model.TargetContent.Length then
                let endTime =
                    match model.StartTime with
                    | Some startTime -> startTime.AddSeconds(float model.ElapsedSeconds)
                    | None -> DateTime.Now

                { model with
                    UserInput = newInput
                    CurrentCharIndex = newIndex
                    Errors = newErrors
                    TypingState = Completed
                    EndTime = Some endTime
                    LastKey = lastKey
                    LastKeyIsError = lastKeyIsError
                }, Cmd.none
            else
                { model with
                    UserInput = newInput
                    CurrentCharIndex = newIndex
                    Errors = newErrors
                    LastKey = lastKey
                    LastKeyIsError = lastKeyIsError
                }, Cmd.none

    let private applyBackspace model =
        if model.TypingState <> InProgress || model.CurrentCharIndex <= 0 then
            model, Cmd.none
        else
            let newIndex = model.CurrentCharIndex - 1
            let newInput = if newIndex > 0 then model.UserInput.[0..newIndex - 1] else ""
            { model with
                UserInput = newInput
                CurrentCharIndex = newIndex
                Errors = Map.remove newIndex model.Errors
                LastKey = None
                LastKeyIsError = None
            }, Cmd.none

    let private submitSession model =
        if model.IsSubmitting then
            model, Cmd.none
        else
            match model.StartTime, model.EndTime with
            | Some startTime, Some endTime ->
                let wpm, cpm, accuracy, errorCount =
                    calculateMetrics model.Lesson model.UserInput startTime endTime model.Errors
                let localSessionId = model.PendingLocalSessionId |> Option.defaultValue (Guid.NewGuid())
                let sessionDto: SessionCreateDto = {
                    ClientSessionId = localSessionId
                    LessonId = model.Lesson.Id
                    Wpm = wpm
                    Cpm = cpm
                    Accuracy = accuracy
                    ErrorCount = errorCount
                    PerKeyErrors = model.Errors
                }
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

    let update msg model =
        match msg with
        | LoadTargetContent ->
            if model.Lesson.ContentType = ContentType.Probability then
                { model with IsLoadingTargetContent = true; TargetContentError = None }, loadTargetContentCmd model.Lesson
            else
                { model with IsLoadingTargetContent = false; TargetContentError = None; TargetContent = model.Lesson.Content }, Cmd.none

        | TargetContentLoaded content ->
            { model with TargetContent = content; IsLoadingTargetContent = false; TargetContentError = None }, Cmd.none

        | TargetContentLoadFailed error ->
            { model with IsLoadingTargetContent = false; TargetContentError = Some error }, Cmd.none

        | StartTyping -> startTyping model
        | CharacterTyped char -> applyCharacter char model
        | Backspace -> applyBackspace model

        | Tick ->
            if model.TypingState = InProgress then
                { model with ElapsedSeconds = model.ElapsedSeconds + 1 }, tickCmd
            else
                model, Cmd.none

        | SubmitSession -> submitSession model

        | SessionSubmitted _ ->
            model.PendingLocalSessionId |> Option.iter LocalSessions.markSynced
            { model with IsSubmitting = false; SubmitError = None; PendingLocalSessionId = None }, Cmd.none

        | SubmitError error ->
            { model with IsSubmitting = false; SubmitError = Some error }, Cmd.none

        | ClearSubmitError ->
            { model with SubmitError = None }, Cmd.none

        | ResetView
        | CancelTyping ->
            init model.Lesson
