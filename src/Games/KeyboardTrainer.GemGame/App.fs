namespace KeyboardTrainer.GemGame

open System
open Elmish
open Browser.Dom
open Browser.Types
open Fable.Core
open Fable.Core.JsInterop
open Fable.React
open Fable.React.Props
open KeyboardTrainer.GemGame.GameTypes
open KeyboardTrainer.GemGame.GameLogic

module App =
    let private collapseAnimationMs = 260
    let private highScoreLimit = 8

    [<Emit("""
        (function(kind) {
            try {
                var AudioContext = window.AudioContext || window.webkitAudioContext;
                if (!AudioContext) return;
                var ctx = window.__gemGameAudioContext || new AudioContext();
                window.__gemGameAudioContext = ctx;
                if (ctx.state === "suspended") ctx.resume();
                var now = ctx.currentTime;
                var gain = ctx.createGain();
                var osc = ctx.createOscillator();
                var second = ctx.createOscillator();
                var config = {
                    hit: [660, 990, 0.11, 0.055],
                    miss: [180, 120, 0.14, 0.045],
                    finish: [523, 1046, 0.36, 0.07]
                }[kind] || [440, 660, 0.1, 0.05];
                osc.type = kind === "miss" ? "sawtooth" : "triangle";
                second.type = "sine";
                osc.frequency.setValueAtTime(config[0], now);
                second.frequency.setValueAtTime(config[1], now + (kind === "finish" ? 0.08 : 0.02));
                gain.gain.setValueAtTime(0.0001, now);
                gain.gain.exponentialRampToValueAtTime(config[3], now + 0.02);
                gain.gain.exponentialRampToValueAtTime(0.0001, now + config[2]);
                osc.connect(gain);
                second.connect(gain);
                gain.connect(ctx.destination);
                osc.start(now);
                second.start(now + (kind === "finish" ? 0.08 : 0.02));
                osc.stop(now + config[2]);
                second.stop(now + config[2] + 0.05);
            } catch (_) {}
        })($0)
    """)>]
    let private playGameSoundImpl (kind: string) : unit = jsNative

    [<Emit("""
        (function(key) {
            try { return window.localStorage.getItem(key) || ""; }
            catch (_) { return ""; }
        })($0)
    """)>]
    let private loadHighScoresRaw (key: string) : string = jsNative

    [<Emit("""
        (function(key, value) {
            try { window.localStorage.setItem(key, value); }
            catch (_) {}
        })($0, $1)
    """)>]
    let private saveHighScoresRaw (key: string) (value: string) : unit = jsNative

    let private tickCmd (delayMs: int) tickVersion =
        Cmd.OfAsync.perform (fun () -> async {
            do! Async.Sleep delayMs
            return ()
        }) () (fun () -> Tick tickVersion)

    let private collapseAnimationCmd () =
        Cmd.OfAsync.perform (fun () -> async {
            do! Async.Sleep collapseAnimationMs
            return ()
        }) () (fun () -> ApplyCollapse)

    let private ofSub (sub: (Msg -> unit) -> unit) : Cmd<Msg> =
        [ sub ]

    let private playSoundCmd enabled kind =
        if enabled then
            [ fun _ -> playGameSoundImpl kind ]
        else
            Cmd.none

    let private highScoreStorageKey lessonId =
        lessonId
        |> Option.map (fun id -> sprintf "gem-game-high-scores:%O" id)
        |> Option.defaultValue "gem-game-high-scores:standalone"

    let private serializeHighScore (score: HighScore) =
        sprintf "%d|%d|%d|%d|%s" score.Score score.LevelScore score.Hits score.Misses score.PlayedAt

    let private deserializeHighScore (raw: string) : HighScore option =
        match raw.Split('|') with
        | [| gameScore; levelScore; hits; misses; playedAt |] ->
            match Int32.TryParse gameScore, Int32.TryParse levelScore, Int32.TryParse hits, Int32.TryParse misses with
            | (true, parsedGameScore), (true, parsedLevelScore), (true, parsedHits), (true, parsedMisses) ->
                Some {
                    Score = parsedGameScore
                    LevelScore = parsedLevelScore
                    Hits = parsedHits
                    Misses = parsedMisses
                    PlayedAt = playedAt
                }
            | _ -> None
        | [| score; hits; misses; playedAt |] ->
            match Int32.TryParse score, Int32.TryParse hits, Int32.TryParse misses with
            | (true, parsedScore), (true, parsedHits), (true, parsedMisses) ->
                Some {
                    Score = parsedScore
                    LevelScore = parsedScore
                    Hits = parsedHits
                    Misses = parsedMisses
                    PlayedAt = playedAt
                }
            | _ -> None
        | _ -> None

    let private sortHighScores (scores: HighScore list) =
        scores
        |> List.sortBy (fun score -> -score.Score, -score.Hits, score.Misses, score.PlayedAt)
        |> List.truncate highScoreLimit

    let private loadHighScores lessonId =
        let key = highScoreStorageKey lessonId
        let raw = loadHighScoresRaw key
        if String.IsNullOrWhiteSpace raw then
            []
        else
            raw.Split(';', StringSplitOptions.RemoveEmptyEntries)
            |> Array.choose deserializeHighScore
            |> Array.toList
            |> sortHighScores

    let private saveHighScoresCmd lessonId (scores: HighScore list) =
        let key = highScoreStorageKey lessonId
        let payload =
            scores
            |> List.map serializeHighScore
            |> String.concat ";"
        [ fun _ -> saveHighScoresRaw key payload ]

    let private addHighScoreIfNeeded (model: Model) =
        if model.IsFinished && not model.ScoreRecorded then
            let entry: HighScore = {
                Score = model.GameScore
                LevelScore = model.LevelScore
                Hits = model.Hits
                Misses = model.Misses
                PlayedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm")
            }
            let highScores = sortHighScores (entry :: model.HighScores)
            { model with HighScores = highScores; ScoreRecorded = true },
            saveHighScoresCmd model.LessonId highScores
        else
            model, Cmd.none

    let private isRestartKey (key: string) (code: string) =
        key = "r"
        || key = "R"
        || key = " "
        || key = "Spacebar"
        || code = "Space"

    let private registerGlobalKeys (dispatch: Msg -> unit) =
        let onKeyDown (ev: Event) =
            let keyEv = ev :?> KeyboardEvent
            if not (keyEv.ctrlKey || keyEv.altKey || keyEv.metaKey) && isRestartKey keyEv.key keyEv.code then
                keyEv.preventDefault()
                dispatch Restart
            else
                dispatch (KeyPressed keyEv.key)
        window.addEventListener("keydown", onKeyDown)

    let private postCompletionCmd (payload: CompletionPayload) =
        Cmd.OfAsync.perform (fun () -> async {
            let lessonId = payload.LessonId |> Option.map string |> Option.defaultValue ""
            let perKeyErrors =
                payload.PerKeyErrors
                |> Map.toSeq
                |> Seq.map (fun (key, value) -> sprintf "%d:%d" key value)
                |> String.concat ","
            let message =
                sprintf
                    "gem-game-completed|%s|%d|%d|%d|%d|%s"
                    lessonId
                    payload.Score
                    payload.Hits
                    payload.Misses
                    payload.DurationSeconds
                    perKeyErrors
            window.parent.postMessage(message, "*")
        }) () (fun () -> CompletionPosted)

    let init () =
        let baseModel = createModel window.location.search
        let model = { baseModel with HighScores = loadHighScores baseModel.LessonId }
        model,
        Cmd.batch [
            tickCmd model.Config.TickMs model.TickVersion
            ofSub registerGlobalKeys
        ]

    let update msg model =
        match msg with
        | Tick tickVersion ->
            if tickVersion <> model.TickVersion then
                model, Cmd.none
            else
                let next =
                    if model.IsFinished then
                        // Game is finished, handle countdown to auto-restart
                        let newDelayMs = max 0 (model.FinishDelayMs - model.Config.TickMs)
                        { model with FinishDelayMs = newDelayMs }
                    else
                        // Game is running, process normal tick
                        handleTick model
                let next, highScoreCmd = addHighScoreIfNeeded next
                
                let cmd =
                    if next.IsFinished && not next.CompletionSent then
                        Cmd.batch [
                            highScoreCmd
                            postCompletionCmd (toCompletionPayload next)
                            if not model.IsFinished then playSoundCmd next.SoundsEnabled "finish"
                        ]
                    elif next.IsFinished && next.FinishDelayMs = 0 then
                        Cmd.batch [ highScoreCmd; Cmd.ofMsg Restart ]
                    elif next.IsRunning || next.IsFinished then
                        Cmd.batch [ highScoreCmd; tickCmd next.Config.TickMs next.TickVersion ]
                    else
                        highScoreCmd
                next, cmd

        | KeyPressed key ->
            if model.IsFinished then
                model, Cmd.none
            else
                let next = handleKeyPress key model
                let next, highScoreCmd = addHighScoreIfNeeded next
                let soundCmd =
                    if next.IsFinished && not model.IsFinished then
                        playSoundCmd next.SoundsEnabled "finish"
                    elif next.Misses > model.Misses then
                        playSoundCmd next.SoundsEnabled "miss"
                    elif next.IsCollapsing && not model.IsCollapsing then
                        playSoundCmd next.SoundsEnabled "hit"
                    else
                        Cmd.none
                let cmd =
                    if next.IsCollapsing && Option.isSome next.PendingCollapse && not model.IsCollapsing then
                        Cmd.batch [ highScoreCmd; soundCmd; collapseAnimationCmd () ]
                    elif next.IsFinished && not next.CompletionSent then
                        Cmd.batch [ highScoreCmd; soundCmd; postCompletionCmd (toCompletionPayload next) ]
                    else
                        Cmd.batch [ highScoreCmd; soundCmd ]
                next, cmd

        | ApplyCollapse ->
            let next = applyPendingCollapse model
            let next, highScoreCmd = addHighScoreIfNeeded next
            let cmd =
                if next.IsFinished && not next.CompletionSent then
                    Cmd.batch [
                        highScoreCmd
                        playSoundCmd next.SoundsEnabled "finish"
                        postCompletionCmd (toCompletionPayload next)
                    ]
                else
                    highScoreCmd
            next, cmd

        | Restart ->
            let fresh = createModel window.location.search
            let keptConfig =
                { fresh.Config with
                    TickMs = model.Config.TickMs
                    ShowLettersInGems = model.Config.ShowLettersInGems
                    MoveRows = model.Config.MoveRows
                    ScoreMode = model.Config.ScoreMode }
            let next =
                { fresh with
                    Config = keptConfig
                    ShowLettersInGems = keptConfig.ShowLettersInGems
                    TickVersion = model.TickVersion + 1
                    GameScore = model.GameScore
                    HighScores = model.HighScores
                    SettingsExpanded = model.SettingsExpanded
                    SoundsEnabled = model.SoundsEnabled }
            next, tickCmd next.Config.TickMs next.TickVersion

        | SetShowLettersInGems showLetters ->
            let next =
                { model with
                    Config = { model.Config with ShowLettersInGems = showLetters }
                    ShowLettersInGems = showLetters }
            next, Cmd.none

        | SetMoveRows moveRows ->
            let next = {
                model with
                    Config = { model.Config with MoveRows = moveRows }
                    TickVersion = model.TickVersion + 1
                    LastHint = if moveRows then "Rows will move." else "Rows will stay still."
            }
            let cmd =
                if next.IsRunning && not next.IsFinished then
                    tickCmd next.Config.TickMs next.TickVersion
                else
                    Cmd.none
            next, cmd

        | SetSoundsEnabled soundsEnabled ->
            { model with
                SoundsEnabled = soundsEnabled
                LastHint = if soundsEnabled then "Sounds are on." else "Sounds are muted." },
            if soundsEnabled then playSoundCmd true "hit" else Cmd.none

        | SetScoreMode scoreMode ->
            { model with
                Config = { model.Config with ScoreMode = scoreMode }
                LastHint = sprintf "Point calculation set to %s." (scoreModeToLabel scoreMode) },
            Cmd.none

        | ToggleSettings ->
            { model with SettingsExpanded = not model.SettingsExpanded }, Cmd.none

        | SetTickMs tickMs ->
            let nextTickMs = clampTickMs tickMs
            let next = {
                model with
                    Config = { model.Config with TickMs = nextTickMs }
                    TickVersion = model.TickVersion + 1
                    LastHint = sprintf "Row movement interval set to %d ms." nextTickMs
            }
            let cmd =
                if next.IsRunning && not next.IsFinished then
                    tickCmd next.Config.TickMs next.TickVersion
                else
                    Cmd.none
            next, cmd

        | CompletionPosted ->
            { model with CompletionSent = true }, Cmd.none

    let private isNeighborTarget columnIndex model =
        false

    let private boardCellClass rowIndex columnIndex model color =
        let classes = ResizeArray<string>()
        classes.Add("gem-cell")
        classes.Add(colorToCss color)
        if Set.contains (columnIndex, rowIndex) model.CollapsingCells then
            classes.Add("collapsing-gem")
        if rowIndex = model.PlayerRow && columnIndex = model.PlayerColumn then
            classes.Add("player-cell-on-board")
        elif
            (rowIndex = model.PlayerRow && (columnIndex = model.PlayerColumn - 1 || columnIndex = model.PlayerColumn + 1))
            || (columnIndex = model.PlayerColumn && (rowIndex = model.PlayerRow - 1 || rowIndex = model.PlayerRow + 1))
        then
            classes.Add("active-target")
        String.concat " " classes

    let private toMinutesSeconds remainingMs =
        let total = max 0 (remainingMs / 1000)
        total / 60, total % 60

    let private sideForPosition rowIndex columnIndex model =
        if rowIndex < model.PlayerRow && columnIndex = model.PlayerColumn then Some Up
        elif rowIndex > model.PlayerRow && columnIndex = model.PlayerColumn then Some Down
        elif columnIndex < model.PlayerColumn then Some Left
        elif columnIndex > model.PlayerColumn then Some Right
        else None

    let private gemLetter rowIndex columnIndex model color =
        if model.ShowLettersInGems && not (rowIndex = model.PlayerRow && columnIndex = model.PlayerColumn) then
            sideForPosition rowIndex columnIndex model
            |> Option.map (fun side -> keyForSideColor side color)
        else
            None

    let private parseIntOr fallback (raw: string) =
        match Int32.TryParse raw with
        | true, value -> value
        | _ -> fallback

    let private dispatchTickMsFromEvent model dispatch (ev: Event) =
        let target: obj = ev.target
        let raw: string = target?value
        dispatch (SetTickMs (parseIntOr model.Config.TickMs raw))

    let private dispatchScoreModeFromEvent dispatch (ev: Event) =
        let target: obj = ev.target
        let raw: string = target?value
        raw
        |> parseScoreMode
        |> Option.iter (fun scoreMode -> dispatch (SetScoreMode scoreMode))

    let private highScoreRows (scores: HighScore list) =
        match scores with
        | [] ->
            [
                tr [] [
                    td [ ColSpan 5; ClassName "empty-high-score" ] [
                        str "Finish a round to record a score."
                    ]
                ]
            ]
        | _ ->
            scores
            |> List.mapi (fun index score ->
                tr [ Key (sprintf "high-score-%d" index) ] [
                    td [] [ str (sprintf "%d" (index + 1)) ]
                    td [] [ str (string score.Score) ]
                    td [] [ str (string score.LevelScore) ]
                    td [] [ str (sprintf "%d/%d" score.Hits score.Misses) ]
                    td [] [ str score.PlayedAt ]
                ])

    let view model dispatch =
        let minutes, seconds = toMinutesSeconds model.RemainingMs
        let boardRows =
            [ for row in 0 .. model.Config.Rows - 1 ->
                tr [ Key (sprintf "row-%d" row) ] [
                    for column in 0 .. model.Config.Columns - 1 do
                        let color = model.Board[column][row]
                        td [ ClassName (boardCellClass row column model color) ] [
                            if row = model.PlayerRow && column = model.PlayerColumn then
                                span [ ClassName "player-gem" ] [ str "●" ]
                            else
                                match gemLetter row column model color with
                                | Some letter ->
                                    span [ ClassName "gem-letter" ] [ str letter ]
                                | None -> ()
                        ]
                ] ]

        div [ ClassName "gem-game-root"; TabIndex 0; AutoFocus true ] [
            div [ ClassName "gem-game-header" ] [
                h1 [] [ str "Gem Color Rush" ]
                p [ ClassName "hint" ] [ str (expectedKeyText model) ]
            ]

            div [ ClassName "gem-game-stats" ] [
                div [ ClassName "stat-card" ] [ span [] [ str "Level Score" ]; strong [] [ str (string model.LevelScore) ] ]
                div [ ClassName "stat-card" ] [ span [] [ str "Game Score" ]; strong [] [ str (string model.GameScore) ] ]
                div [ ClassName "stat-card" ] [ span [] [ str "Lives" ]; strong [] [ str (string model.Lives) ] ]
                div [ ClassName "stat-card" ] [ span [] [ str "Combo" ]; strong [] [ str (string model.Combo) ] ]
                div [ ClassName "stat-card" ] [ span [] [ str "Hits / Misses" ]; strong [] [ str (sprintf "%d / %d" model.Hits model.Misses) ] ]
                div [ ClassName "stat-card" ] [ span [] [ str "Time" ]; strong [] [ str (sprintf "%02d:%02d" minutes seconds) ] ]
                div [ ClassName "stat-card" ] [ span [] [ str "Level Target" ]; strong [] [ str (string model.Config.TargetScore) ] ]
                div [ ClassName "stat-card" ] [ span [] [ str "Points" ]; strong [] [ str (scoreModeToLabel model.Config.ScoreMode) ] ]
            ]

            div [ ClassName "gem-game-play-area" ] [
                div [ ClassName "gem-game-board-wrapper" ] [
                    table [ ClassName "gem-game-board"; CellPadding 0; CellSpacing 0 ] [
                        tbody [] boardRows
                    ]
                ]

                aside [ ClassName "gem-game-high-scores" ] [
                    h2 [] [ str "High Scores" ]
                    p [ ClassName "current-score-label" ] [
                        str (sprintf "Game: %d | Level: %d" model.GameScore model.LevelScore)
                    ]
                    table [ ClassName "high-score-table"; CellPadding 0; CellSpacing 0 ] [
                        thead [] [
                            tr [] [
                                th [] [ str "#" ]
                                th [] [ str "Game" ]
                                th [] [ str "Level" ]
                                th [] [ str "H/M" ]
                                th [] [ str "When" ]
                            ]
                        ]
                        tbody [] (highScoreRows model.HighScores)
                    ]
                ]
            ]

            div [ ClassName (if model.SettingsExpanded then "gem-game-settings expanded" else "gem-game-settings collapsed") ] [
                button [
                    Type "button"
                    ClassName "settings-toggle"
                    OnClick (fun _ -> dispatch ToggleSettings)
                    AriaExpanded model.SettingsExpanded
                ] [
                    span [] [ str "Game Settings" ]
                    strong [] [ str (if model.SettingsExpanded then "Hide" else "Show") ]
                ]

                if model.SettingsExpanded then
                    div [ ClassName "settings-body" ] [
                        label [ ClassName "settings-check" ] [
                            input [
                                Type "checkbox"
                                Checked model.ShowLettersInGems
                                OnChange (fun ev ->
                                    let isChecked: bool = ev.target?``checked``
                                    dispatch (SetShowLettersInGems isChecked))
                            ]
                            span [] [ str "Show letters inside colored gems" ]
                        ]
                        label [ ClassName "settings-check" ] [
                            input [
                                Type "checkbox"
                                Checked model.Config.MoveRows
                                OnChange (fun ev ->
                                    let isChecked: bool = ev.target?``checked``
                                    dispatch (SetMoveRows isChecked))
                            ]
                            span [] [ str "Move rows" ]
                        ]
                        label [ ClassName "settings-check" ] [
                            input [
                                Type "checkbox"
                                Checked model.SoundsEnabled
                                OnChange (fun ev ->
                                    let isChecked: bool = ev.target?``checked``
                                    dispatch (SetSoundsEnabled isChecked))
                            ]
                            span [] [ str "Sounds" ]
                        ]
                        label [ ClassName "settings-range" ] [
                            span [] [ str (sprintf "Row movement interval: %d ms" model.Config.TickMs) ]
                            input [
                                Type "range"
                                Min "300"
                                Max "2000"
                                Step "50"
                                Value (string model.Config.TickMs)
                                OnInput (dispatchTickMsFromEvent model dispatch)
                                OnChange (dispatchTickMsFromEvent model dispatch)
                            ]
                        ]
                        label [ ClassName "settings-field" ] [
                            span [] [ str "Point calculation" ]
                            select [
                                Value (scoreModeToValue model.Config.ScoreMode)
                                OnChange (dispatchScoreModeFromEvent dispatch)
                            ] [
                                option [ Value "exponential" ] [ str "2^(n-1)" ]
                                option [ Value "square" ] [ str "n^2" ]
                            ]
                        ]
                    ]
            ]

            div [ ClassName "gem-game-controls" ] [
                p [] [ str "Left side: a blue, s green, d red, f yellow" ]
                p [] [ str "Right side: ö blue, l green, k red, j yellow" ]
                p [] [ str "Up: q/p blue, w/o green, e/i red, r/u yellow" ]
                p [] [ str "Down: y/- blue, x/. green, c/, red, v/m yellow" ]
                p [ ClassName "last-hint" ] [ str model.LastHint ]
            ]

            if model.IsFinished then
                div [ ClassName "gem-game-finish" ] [
                    div [ ClassName "confetti-game" ] [ 
                        for index in 1 .. 24 do 
                            span [ Key (sprintf "game-confetti-%d" index) ] [] 
                    ]
                    h2 [ ClassName "finish-heading" ] [ str "🎉 Round Complete! 🎉" ]
                    p [ ClassName "final-score" ] [ str (sprintf "Level Score: %d" model.LevelScore) ]
                    p [ ClassName "final-score" ] [ str (sprintf "Game Score: %d" model.GameScore) ]
                    if model.HighScores |> List.tryHead |> Option.exists (fun score -> score.Score = model.GameScore && score.LevelScore = model.LevelScore) then
                        p [ ClassName "score-message high-score-message" ] [ str "New score added to the high-score table!" ]
                    p [ ClassName "score-message" ] [ 
                        str (if model.LevelScore >= model.Config.TargetScore 
                             then "🏆 Target Reached! Excellent!"
                             else sprintf "Keep going! You need %d more level points to reach the target." (model.Config.TargetScore - model.LevelScore))
                    ]
                    if model.FinishDelayMs > 0 then
                        p [ ClassName "auto-restart-message" ] [ 
                            str (sprintf "Starting new round in %d seconds..." (model.FinishDelayMs / 1000 + 1))
                        ]
                    else
                        ()
                    button [ ClassName "restart-btn"; OnClick (fun _ -> dispatch Restart) ] [ str "Play Again (R or Space)" ]
                ]
        ]
