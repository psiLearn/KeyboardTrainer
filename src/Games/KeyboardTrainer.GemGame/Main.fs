namespace KeyboardTrainer.GemGame

open Elmish
open Elmish.React

module Main =
    Program.mkProgram App.init App.update App.view
    |> Program.withReactBatched "gem-game-app"
    |> Program.run
