namespace KeyboardTrainer.Client.Pages

open KeyboardTrainer.Client
open KeyboardTrainer.Client.Pages.TypingViewTypes

module TypingView =
    type TypingState = TypingViewTypes.TypingState
    type Model = TypingViewTypes.Model
    type Msg = TypingViewTypes.Msg

    let init = TypingViewState.init
    let update = TypingViewState.update
    let view = TypingViewView.view
