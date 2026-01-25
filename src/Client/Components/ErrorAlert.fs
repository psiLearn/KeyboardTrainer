namespace KeyboardTrainer.Client.Components

open Fable.React
open Fable.React.Props

module ErrorAlert =
    let view (message: string) (onRetry: (unit -> unit) option) (onDismiss: (unit -> unit) option) =
        div [ ClassName "alert alert-danger" ] [
            match onDismiss with
            | Some dismiss ->
                button [
                    ClassName "alert-close"
                    OnClick (fun _ -> dismiss ())
                ] [ str "x" ]
            | None -> ()

            p [ ClassName "alert-message" ] [ str message ]

            match onRetry with
            | Some retry ->
                button [
                    ClassName "btn btn-secondary"
                    OnClick (fun _ -> retry ())
                ] [ str "Retry" ]
            | None -> ()
        ]
