namespace KeyboardTrainer.GemGame

open System
#if FABLE_COMPILER
open Fable.Core
open Fable.Core.JsInterop
#else
open System.Text.Json
#endif
open KeyboardTrainer.GemGame.GameTypes

module GameLogic =
    let private defaultConfig = {
        Rows = 15
        Columns = 15
        TickMs = 850
        DurationSeconds = 75
        TargetScore = 500
        Lives = 10
        ShowLettersInGems = false
        MoveRows = true
    }

    let private nextSeed seed =
        ((seed * 1103515245) + 12345) &&& 0x7fffffff

    let private randomColor seed =
        let seed' = nextSeed seed
        let bucket = (seed' / 65536) % 100
        let color =
            if bucket < 19 then Blue
            elif bucket < 42 then Green
            elif bucket < 69 then Red
            else Yellow
        color, seed'

    let private createColumn rows seed =
        let mutable current = seed
        let colors = ResizeArray<GemColor>()
        for _ in 1 .. rows do
            let color, next = randomColor current
            colors.Add color
            current <- next
        colors |> Seq.toList, current

    let private createBoard rows columns seed =
        let mutable current = seed
        let board = ResizeArray<GemColor list>()
        for _ in 1 .. columns do
            let column, next = createColumn rows current
            board.Add column
            current <- next
        board |> Seq.toList, current

    let private normalizePositive minValue maxValue fallback value =
        if value < minValue || value > maxValue then fallback else value

    let clampTickMs value =
        max 300 (min 2000 value)

    let private parseQueryString (search: string) =
        if String.IsNullOrWhiteSpace search then Map.empty
        else
            search.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries)
            |> Array.choose (fun segment ->
                let parts = segment.Split('=', 2)
                if parts.Length = 2 then
                    Some (parts[0], Uri.UnescapeDataString parts[1])
                elif parts.Length = 1 then
                    Some (parts[0], "")
                else
                    None)
            |> Map.ofArray

    let private parseConfigFromQuery (search: string) =
        let query = parseQueryString search
        let lessonId =
            query
            |> Map.tryFind "lessonId"
            |> Option.bind (fun raw ->
                match Guid.TryParse raw with
                | true, parsed -> Some parsed
                | _ -> None)

        let config =
            query
            |> Map.tryFind "config"
            |> Option.bind (fun raw ->
                try
                    #if FABLE_COMPILER
                    let parsed: obj = emitJsExpr raw "JSON.parse($0)"
                    let tryGetInt (name: string) =
                        let value: obj = parsed?(name)
                        if isNull value || obj.ReferenceEquals(value, JS.undefined) then None
                        else
                            match value with
                            | :? int as number -> Some number
                            | :? float as number -> Some (int number)
                            | :? string as text ->
                                match Int32.TryParse text with
                                | true, number -> Some number
                                | _ -> None
                            | _ -> None
                    let tryGetBool (name: string) =
                        let value: obj = parsed?(name)
                        if isNull value || obj.ReferenceEquals(value, JS.undefined) then None
                        else
                            match value with
                            | :? bool as flag -> Some flag
                            | :? int as number -> Some (number <> 0)
                            | :? float as number -> Some (number <> 0.0)
                            | :? string as text ->
                                match Boolean.TryParse text with
                                | true, flag -> Some flag
                                | _ -> None
                            | _ -> None
                    #else
                    use parsed = JsonDocument.Parse(raw)
                    let root = parsed.RootElement
                    let tryGetInt (name: string) =
                        let mutable node = Unchecked.defaultof<JsonElement>
                        if root.TryGetProperty(name, &node) then
                            if node.ValueKind = JsonValueKind.Number then
                                match node.TryGetInt32() with
                                | true, value -> Some value
                                | _ -> None
                            elif node.ValueKind = JsonValueKind.String then
                                match Int32.TryParse(node.GetString()) with
                                | true, value -> Some value
                                | _ -> None
                            else
                                None
                        else
                            None
                    let tryGetBool (name: string) =
                        let mutable node = Unchecked.defaultof<JsonElement>
                        if root.TryGetProperty(name, &node) then
                            match node.ValueKind with
                            | JsonValueKind.True -> Some true
                            | JsonValueKind.False -> Some false
                            | JsonValueKind.String ->
                                match Boolean.TryParse(node.GetString()) with
                                | true, flag -> Some flag
                                | _ -> None
                            | JsonValueKind.Number ->
                                match node.TryGetInt32() with
                                | true, value -> Some (value <> 0)
                                | _ -> None
                            | _ -> None
                        else
                            None
                    #endif

                    let rows = tryGetInt "rows" |> Option.defaultValue defaultConfig.Rows
                    let columns = tryGetInt "columns" |> Option.defaultValue defaultConfig.Columns
                    let tickMs = tryGetInt "tickMs" |> Option.defaultValue defaultConfig.TickMs
                    let durationSeconds = tryGetInt "durationSeconds" |> Option.defaultValue defaultConfig.DurationSeconds
                    let targetScore = tryGetInt "targetScore" |> Option.defaultValue defaultConfig.TargetScore
                    let lives = tryGetInt "lives" |> Option.defaultValue defaultConfig.Lives
                    let showLettersInGems = tryGetBool "showLettersInGems" |> Option.defaultValue defaultConfig.ShowLettersInGems
                    let moveRows = tryGetBool "moveRows" |> Option.defaultValue defaultConfig.MoveRows

                    Some {
                        Rows = normalizePositive 4 20 defaultConfig.Rows rows
                        Columns = normalizePositive 5 20 defaultConfig.Columns columns
                        TickMs = clampTickMs tickMs
                        DurationSeconds = normalizePositive 20 300 defaultConfig.DurationSeconds durationSeconds
                        TargetScore = normalizePositive 50 5000 defaultConfig.TargetScore targetScore
                        Lives = normalizePositive 1 25 defaultConfig.Lives lives
                        ShowLettersInGems = showLettersInGems
                        MoveRows = moveRows
                    }
                with _ ->
                    None)
            |> Option.defaultValue defaultConfig

        lessonId, config

    let private sideToLabel side =
        match side with
        | Left -> "left"
        | Right -> "right"
        | Up -> "up"
        | Down -> "down"

    let colorToLabel color =
        match color with
        | Blue -> "blue"
        | Green -> "green"
        | Red -> "red"
        | Yellow -> "yellow"

    let colorToCss color =
        match color with
        | Blue -> "gem-blue"
        | Green -> "gem-green"
        | Red -> "gem-red"
        | Yellow -> "gem-yellow"

    let keyForSideColor side color =
        match side, color with
        | Left, Blue -> "a"
        | Left, Green -> "s"
        | Left, Red -> "d"
        | Left, Yellow -> "f"
        | Right, Blue -> "ö"
        | Right, Green -> "l"
        | Right, Red -> "k"
        | Right, Yellow -> "j"
        | Up, Blue -> "q/p"
        | Up, Green -> "w/o"
        | Up, Red -> "e/i"
        | Up, Yellow -> "r/u"
        | Down, Blue -> "y/-"
        | Down, Green -> "x/."
        | Down, Red -> "c/,"
        | Down, Yellow -> "v/m"

    let private targetPosition side model =
        match side with
        | Left -> model.PlayerColumn - 1, model.PlayerRow
        | Right -> model.PlayerColumn + 1, model.PlayerRow
        | Up -> model.PlayerColumn, model.PlayerRow - 1
        | Down -> model.PlayerColumn, model.PlayerRow + 1

    let private isValidPosition (column, row) model =
        column >= 0
        && column < model.Config.Columns
        && row >= 0
        && row < model.Config.Rows

    let private positionColor (column, row) model =
        model.Board |> List.item column |> List.item row

    let expectedKeyText model =
        let sideText side =
            let position = targetPosition side model
            if isValidPosition position model then
                let color = positionColor position model
                sprintf "%s %s: %s" (sideToLabel side) (colorToLabel color) (keyForSideColor side color)
            else
                sprintf "%s edge" (sideToLabel side)

        sprintf "%s | %s | %s | %s" (sideText Left) (sideText Right) (sideText Up) (sideText Down)

    let private colorAt board column row =
        board |> List.item column |> List.item row

    let private neighbors columns rows (column, row) =
        [
            column - 1, row
            column + 1, row
            column, row - 1
            column, row + 1
        ]
        |> List.filter (fun (nextColumn, nextRow) ->
            nextColumn >= 0
            && nextColumn < columns
            && nextRow >= 0
            && nextRow < rows)

    let private connectedSameColorGroup rows columns board blockedCells startColumn startRow color =
        let rec visit frontier visited =
            match frontier with
            | [] -> visited
            | position :: rest ->
                if Set.contains position visited || Set.contains position blockedCells then
                    visit rest visited
                else
                    let column, row = position
                    if colorAt board column row <> color then
                        visit rest visited
                    else
                        let next =
                            neighbors columns rows position
                            |> List.filter (fun candidate -> not (Set.contains candidate visited))
                        visit (next @ rest) (Set.add position visited)

        visit [ startColumn, startRow ] Set.empty

    let private generateColors count seed =
        let mutable current = seed
        let mutable generated = []
        for _ in 1 .. count do
            let color, next = randomColor current
            generated <- color :: generated
            current <- next
        List.rev generated, current

    let private collapseConnectedGroup rows columns board startColumn startRow playerColumn playerRow seed =
        let targetColor = colorAt board startColumn startRow
        let blockedCells = Set.singleton (playerColumn, playerRow)
        let collapsed = connectedSameColorGroup rows columns board blockedCells startColumn startRow targetColor
        let mutable current = seed

        let nextBoard =
            board
            |> List.mapi (fun columnIndex column ->
                let collapsedRows =
                    collapsed
                    |> Set.toList
                    |> List.choose (fun (groupColumn, groupRow) ->
                        if groupColumn = columnIndex then Some groupRow else None)

                match collapsedRows with
                | [] -> column
                | _ ->
                    let collapsedRowSet = Set.ofList collapsedRows
                    let keep =
                        column
                        |> List.indexed
                        |> List.filter (fun (rowIndex, _) -> not (Set.contains rowIndex collapsedRowSet))
                        |> List.map snd
                    let newColors, nextSeed = generateColors collapsedRows.Length current
                    current <- nextSeed
                    keep @ newColors)

        collapsed, nextBoard, current

    let private collapsePoints collapsedCount =
        if collapsedCount <= 1 then 1
        elif collapsedCount >= 30 then Int32.MaxValue
        else pown 2 (collapsedCount - 1)

    let private shiftColumn rows column seed =
        let color, next = randomColor seed
        let shifted = color :: (column |> List.take (rows - 1))
        shifted, next

    let private shiftBoard rows board seed =
        let mutable current = seed
        let shifted =
            board
            |> List.map (fun column ->
                let nextColumn, nextSeed = shiftColumn rows column current
                current <- nextSeed
                nextColumn)
        shifted, current

    let private registerMiss keyCode model =
        { model with
            Misses = model.Misses + 1
            Combo = 0
            Lives = max 0 (model.Lives - 1)
            PerKeyErrors = Map.change keyCode (function Some count -> Some (count + 1) | None -> Some 1) model.PerKeyErrors
            LastHint = "Miss. Match side and color of the highlighted bottom gem." }

    let private keyCodeForSideColor side color =
        match side, color with
        | Left, Blue -> int 'A'
        | Left, Green -> int 'S'
        | Left, Red -> int 'D'
        | Left, Yellow -> int 'F'
        | Right, Blue -> int 'Ö'
        | Right, Green -> int 'L'
        | Right, Red -> int 'K'
        | Right, Yellow -> int 'J'
        | Up, Blue -> int 'Q'
        | Up, Green -> int 'W'
        | Up, Red -> int 'E'
        | Up, Yellow -> int 'R'
        | Down, Blue -> int 'Y'
        | Down, Green -> int 'X'
        | Down, Red -> int 'C'
        | Down, Yellow -> int 'V'

    let private normalizeKey (key: string) =
        if String.IsNullOrWhiteSpace key then ""
        else key.Trim().ToLowerInvariant()

    let private keyToSideAndColor (raw: string) =
        match normalizeKey raw with
        | "a" -> Some (Left, Blue)
        | "s" -> Some (Left, Green)
        | "d" -> Some (Left, Red)
        | "f" -> Some (Left, Yellow)
        | "j" -> Some (Right, Yellow)
        | "l" -> Some (Right, Green)
        | "k" -> Some (Right, Red)
        | "ö"
        | ";" -> Some (Right, Blue)
        | "q"
        | "p" -> Some (Up, Blue)
        | "w"
        | "o" -> Some (Up, Green)
        | "e"
        | "i" -> Some (Up, Red)
        | "r"
        | "u" -> Some (Up, Yellow)
        | "y"
        | "-" -> Some (Down, Blue)
        | "x"
        | "." -> Some (Down, Green)
        | "c"
        | "," -> Some (Down, Red)
        | "v"
        | "m" -> Some (Down, Yellow)
        | _ -> None

    let private finishIfNeeded model =
        if model.Lives <= 0 || model.RemainingMs <= 0 || model.Score >= model.Config.TargetScore then
            { model with IsRunning = false; IsFinished = true; FinishDelayMs = 3000 }
        else
            model

    let createModel (search: string) =
        let lessonId, config = parseConfigFromQuery search
        let initialSeed = int DateTime.UtcNow.Ticks
        let board, seed1 = createBoard config.Rows config.Columns initialSeed
        {
            Config = config
            LessonId = lessonId
            Board = board
            PlayerColumn = config.Columns / 2
            PlayerRow = config.Rows / 2
            Seed = seed1
            Score = 0
            Combo = 0
            Lives = config.Lives
            Hits = 0
            Misses = 0
            RemainingMs = config.DurationSeconds * 1000
            IsRunning = true
            IsFinished = false
            CompletionSent = false
            ShowLettersInGems = config.ShowLettersInGems
            IsCollapsing = false
            CollapsingCells = Set.empty
            PendingCollapse = None
            TickVersion = 0
            LastHint = "Move into the neighboring gem by pressing its side and color key."
            PerKeyErrors = Map.empty
            FinishDelayMs = 0
            HighScores = []
            ScoreRecorded = false
            SettingsExpanded = false
            SoundsEnabled = true
        }

    let handleKeyPress key model =
        if not model.IsRunning || model.IsFinished || model.IsCollapsing then
            model
        else
            match keyToSideAndColor key with
            | None -> model
            | Some (side, color) ->
                let columnIndex, rowIndex = targetPosition side model

                if not (isValidPosition (columnIndex, rowIndex) model) then
                    registerMiss (keyCodeForSideColor side color) { model with LastHint = "No column on that side." }
                    |> finishIfNeeded
                elif color = positionColor (columnIndex, rowIndex) model then
                    let collapsedCells, updatedBoard, nextSeed =
                        collapseConnectedGroup model.Config.Rows model.Config.Columns model.Board columnIndex rowIndex model.PlayerColumn model.PlayerRow model.Seed
                    let collapsedCount = Set.count collapsedCells
                    let points = collapsePoints collapsedCount
                    let plan = {
                        Side = side
                        TargetColumn = columnIndex
                        CollapsedCells = collapsedCells
                        CollapsedCount = collapsedCount
                        Points = points
                        NextBoard = updatedBoard
                        NextSeed = nextSeed
                    }
                    {
                        model with
                            IsCollapsing = true
                            CollapsingCells = collapsedCells
                            PendingCollapse = Some plan
                            LastHint = sprintf "Collapsing %d connected %s gems..." collapsedCount (colorToLabel color)
                    }
                else
                    registerMiss (keyCodeForSideColor side color) model |> finishIfNeeded

    let applyPendingCollapse model =
        match model.PendingCollapse with
        | None ->
            { model with IsCollapsing = false; CollapsingCells = Set.empty }
        | Some plan ->
            let combo = model.Combo + 1
            {
                model with
                    Board = plan.NextBoard
                    PlayerColumn = plan.TargetColumn
                    PlayerRow =
                        match plan.Side with
                        | Left
                        | Right -> model.PlayerRow
                        | Up -> model.PlayerRow - 1
                        | Down -> model.PlayerRow + 1
                    Seed = plan.NextSeed
                    Score = model.Score + plan.Points
                    Combo = combo
                    Hits = model.Hits + 1
                    IsCollapsing = false
                    CollapsingCells = Set.empty
                    PendingCollapse = None
                    LastHint = sprintf "Moved %s. Collapsed %d gems! +%d points." (sideToLabel plan.Side) plan.CollapsedCount plan.Points
            }
            |> finishIfNeeded

    let handleTick model =
        if not model.IsRunning || model.IsFinished || model.IsCollapsing then
            model
        else
            let next =
                if model.Config.MoveRows then
                    let shiftedBoard, seed1 = shiftBoard model.Config.Rows model.Board model.Seed
                    { model with
                        Board = shiftedBoard
                        Seed = seed1
                        RemainingMs = max 0 (model.RemainingMs - model.Config.TickMs) }
                else
                    { model with
                        RemainingMs = max 0 (model.RemainingMs - model.Config.TickMs) }
            finishIfNeeded next

    let toCompletionPayload model =
        let elapsedSeconds =
            max 1 (model.Config.DurationSeconds - (model.RemainingMs / 1000))
        {
            LessonId = model.LessonId
            Score = model.Score
            Hits = model.Hits
            Misses = model.Misses
            DurationSeconds = elapsedSeconds
            PerKeyErrors = model.PerKeyErrors
        }
