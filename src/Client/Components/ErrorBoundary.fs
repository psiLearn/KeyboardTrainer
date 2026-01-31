namespace KeyboardTrainer.Client.Components

open Browser.Dom
open Fable.React
open Fable.React.Props
open KeyboardTrainer.Client

type ErrorBoundaryState = {
    HasError: bool
    ErrorMessage: string option
}

type ErrorBoundaryProps = {
    Children: ReactElement list
}

type ErrorBoundary(props) =
    inherit Component<ErrorBoundaryProps, ErrorBoundaryState>(props)

    do
        base.setInitState({ HasError = false; ErrorMessage = None })

    override this.componentDidCatch(error, _info) =
        let message = if isNull error then "Unknown error" else error.Message
        this.setState(fun _ _ -> { HasError = true; ErrorMessage = Some message })

    override this.render() =
        if this.state.HasError then
            let message = this.state.ErrorMessage |> Option.defaultValue "Unknown error"
            div [ ClassName "error-boundary" ] [
                h2 [] [ str "Something went wrong." ]
                ErrorAlert.view (AppError.Unknown message) None (Some (fun () -> window.location.reload()))
            ]
        else
            div [] this.props.Children

module ErrorBoundary =
    let view (children: ReactElement list) =
        ofType<ErrorBoundary, ErrorBoundaryProps, ErrorBoundaryState> { Children = children } []
