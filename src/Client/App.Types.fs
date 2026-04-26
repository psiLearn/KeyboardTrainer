namespace KeyboardTrainer.Client

open KeyboardTrainer.Shared
open KeyboardTrainer.Client.Pages

type Page =
    | StartScreen
    | TypingView of LessonDto
    | GameView of LessonDto
    | Metrics

type Model = {
    CurrentPage: Page
    StartScreenModel: StartScreen.Model
    TypingViewModel: TypingView.Model option
    MetricsModel: Metrics.Model
    GameSubmitError: AppError option
    GameStatusMessage: string option
    GameSessionPending: bool
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
    | NavigateToGameView of LessonDto
    | GameMessageReceived of Browser.Types.MessageEvent
    | GameSessionSubmitted of System.Guid * SessionDto
    | GameSessionSubmitFailed of System.Guid * AppError
    | SyncPendingSessions
    | PendingSessionSynced of System.Guid
    | PendingSessionSyncFailed of System.Guid * string
    | UpdateSettings of UserSettings
