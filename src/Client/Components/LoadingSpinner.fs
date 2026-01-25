namespace KeyboardTrainer.Client.Components

open Fable.React
open Fable.React.Props

module LoadingSpinner =
    let view (message: string option) =
        div [ ClassName "loading-spinner" ] [
            div [ ClassName "spinner-border" ] []
            match message with
            | Some text -> p [ ClassName "loading-text" ] [ str text ]
            | None -> ()
        ]
