namespace KeyboardTrainer.Client

open Elmish
open Elmish.React

module Main =
    Program.mkProgram App.init App.update App.view
    |> Program.withReactBatched "app"
    |> Program.run
