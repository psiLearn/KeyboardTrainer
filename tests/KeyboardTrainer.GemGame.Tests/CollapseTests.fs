namespace KeyboardTrainer.GemGame.Tests

open System
open Xunit
open KeyboardTrainer.GemGame.GameLogic
open KeyboardTrainer.GemGame.GameTypes

module CollapseTests =

    [<Fact>]
    let ``collapseConnectedGroup respects blocked cell`` () =
        // Build a 3x3 board with connected blue across most cells except a red center
        // Board is list of columns, each column is list of rows (row 0 is top)
        let blue = Blue
        let red = Red
        let board = [
            [ blue; blue; blue ]
            [ blue; red;  blue ]
            [ blue; blue; blue ]
        ]

        // Start collapsing from (0,1) which is blue; block (1,1) (the red center)
        let rows = 3
        let columns = 3
        let startColumn, startRow = 0, 1
        let blockedColumn, blockedRow = 1, 1
        let seed = 42

        let collapsed, nextBoard, _nextSeed = collapseConnectedGroup rows columns board startColumn startRow blockedColumn blockedRow seed

        // Blocked cell is red so it wouldn't be part of the blue group anyway; expect the full peripheral blue group
        Assert.True(Set.contains (0,1) collapsed)
        // Ensure blocked cell is not in the collapsed set
        Assert.False(Set.contains (blockedColumn, blockedRow) collapsed)

    [<Fact>]
    let ``collapseConnectedGroup excludes explicitly blocked neighbor`` () =
        // 2x2 board where all cells are blue
        let b = Blue
        let board = [ [ b; b ]; [ b; b ] ]
        let rows = 2
        let columns = 2
        // Start at (0,0) and block (1,0) so expansion to the right is prevented
        let collapsed, _nextBoard, _ = collapseConnectedGroup rows columns board 0 0 1 0 7

        // Blocked neighbor must not be part of the collapsed set
        Assert.True(Set.contains (0,0) collapsed)
        Assert.False(Set.contains (1,0) collapsed)
