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

    type GameConfig = {
        Rows: int
        Columns: int
        TickMs: int
        DurationSeconds: int
        TargetScore: int
        Lives: int
        ShowLettersInGems: bool
        MoveRows: bool
    }

    type CompletionPayload = {
        LessonId: Guid option
        Score: int
        Hits: int
        Misses: int
        DurationSeconds: int
        PerKeyErrors: Map<int, int>
    }

    type HighScore = {
        Score: int
        Hits: int
        Misses: int
        PlayedAt: string
    }

    type CollapsePlan = {
        Side: Side
        TargetColumn: int
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
        Score: int
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
        | ToggleSettings
        | SetTickMs of int
        | ApplyCollapse
        | Restart
        | CompletionPosted
