namespace KeyboardTrainer.Client.Components

open Fable.React
open Fable.React.Props
open KeyboardTrainer.Client

module ErrorAlert =
    let view (error: AppError) (onRetry: (unit -> unit) option) (onDismiss: (unit -> unit) option) =
        let message = AppError.toMessage error
        div [ ClassName "alert alert-danger" ] [
            match onDismiss with
            | Some dismiss ->
                button [
                    ClassName "alert-close"
                    OnClick (fun _ -> dismiss ())
                ] [ str "x" ]
            | None -> ()

            match error with
            | Validation errors ->
                p [ ClassName "alert-message" ] [ str "Please fix the following:" ]
                ul [ ClassName "alert-list" ] [
                    for index, err in errors |> List.indexed do
                        li [ Key (sprintf "%s-%d" err.Field index) ] [ str (sprintf "%s: %s" err.Field err.Message) ]
                ]
            | _ ->
                p [ ClassName "alert-message" ] [ str message ]

            match onRetry with
            | Some retry ->
                button [
                    ClassName "btn btn-secondary"
                    OnClick (fun _ -> retry ())
                ] [ str "Retry" ]
            | None -> ()
        ]
