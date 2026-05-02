namespace KeyboardTrainer.GemGame.Tests

open Xunit
open KeyboardTrainer.GemGame.GameLogic
open KeyboardTrainer.GemGame.GameTypes

module ShiftBoardTests =

    [<Fact>]
    let ``shiftBoard rotates each column downward preserving seed`` () =
        // Column representation: top..bottom
        // For a column [a;b;c] shifting should produce [c;a;b]
        let a,b,c = Blue, Green, Red
        let board = [ [ a; b; c ]; [ a; b; c ] ]
        let seed = 123

        let shifted, returnedSeed = shiftBoard board seed

        // both columns should have rotated so bottom moved to top
        Assert.Equal<GemColor list>([c; a; b], shifted.[0])
        Assert.Equal<GemColor list>([c; a; b], shifted.[1])
        // seed is returned unchanged by current implementation
        Assert.Equal(seed, returnedSeed)
