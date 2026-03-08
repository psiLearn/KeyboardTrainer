namespace KeyboardTrainer.Client

open KeyboardTrainer.Shared
open KeyboardTrainer.Client.Pages

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
