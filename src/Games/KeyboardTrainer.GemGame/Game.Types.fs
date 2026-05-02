namespace KeyboardTrainer.GemGame

open System

module GameTypes =
    type Side =
        | Left
        | Right
        | Up
        | Down

    type GemColor =
        | Blue
        | Green
        | Red
        | Yellow

    type ScoreMode =
        | Exponential
        | Square

    type GameConfig = {
        Rows: int
        Columns: int
        TickMs: int
        DurationSeconds: int
        TargetScore: int
        Lives: int
        ShowLettersInGems: bool
        MoveRows: bool
        ScoreMode: ScoreMode
    }

    type CompletionPayload = {
        LessonId: Guid option
        Score: int
        GameScore: int
        Hits: int
        Misses: int
        DurationSeconds: int
        PerKeyErrors: Map<int, int>
    }

    type HighScore = {
        Score: int
        LevelScore: int
        Hits: int
        Misses: int
        PlayedAt: string
    }

    type CollapsePlan = {
        Side: Side
        TargetColumn: int
        TargetRow: int
        CollapsedCells: Set<int * int>
        CollapsedCount: int
        Points: int
        NextBoard: GemColor list list
        NextSeed: int
    }

    type Model = {
        Config: GameConfig
        LessonId: Guid option
        Board: GemColor list list
        PlayerColumn: int
        PlayerRow: int
        Seed: int
        LevelScore: int
        GameScore: int
        Combo: int
        Lives: int
        Hits: int
        Misses: int
        RemainingMs: int
        IsRunning: bool
        IsFinished: bool
        CompletionSent: bool
        ShowLettersInGems: bool
        IsCollapsing: bool
        CollapsingCells: Set<int * int>
        PendingCollapse: CollapsePlan option
        TickVersion: int
        LastHint: string
        PerKeyErrors: Map<int, int>
        FinishDelayMs: int
        HighScores: HighScore list
        ScoreRecorded: bool
        SettingsExpanded: bool
        SoundsEnabled: bool
    }

    type Msg =
        | Tick of int
        | KeyPressed of string
        | SetShowLettersInGems of bool
        | SetMoveRows of bool
        | SetSoundsEnabled of bool
        | SetScoreMode of ScoreMode
        | ToggleSettings
        | SetTickMs of int
        | ApplyCollapse
        | Restart
        | CompletionPosted
