namespace KeyboardTrainer.Client.Pages

open System
open KeyboardTrainer.Shared
open KeyboardTrainer.Client

module TypingViewTypes =
    type TypingState =
        | NotStarted
        | InProgress
        | Completed

    type Model = {
        Lesson: LessonDto
        TargetContent: string
        IsLoadingTargetContent: bool
        TargetContentError: AppError option
        TypingState: TypingState
        StartTime: DateTime option
        EndTime: DateTime option
        UserInput: string
        CurrentCharIndex: int
        Errors: Map<int, int>
        IsSubmitting: bool
        SubmitError: AppError option
        PendingLocalSessionId: Guid option
        ElapsedSeconds: int
        LastKey: string option
        LastKeyIsError: bool option
    }

    type Msg =
        | LoadTargetContent
        | TargetContentLoaded of string
        | TargetContentLoadFailed of AppError
        | StartTyping
        | CharacterTyped of char
        | Backspace
        | Tick
        | SubmitSession
        | SessionSubmitted of SessionDto
        | SubmitError of AppError
        | ClearSubmitError
        | ResetView
        | CancelTyping
