namespace KeyboardTrainer.Server.Tests

open System
open Xunit
open KeyboardTrainer.Shared
open KeyboardTrainer.Server

module SessionValidationTests =
    let private validSession: SessionCreateDto =
        {
            ClientSessionId = Guid.NewGuid()
            LessonId = Guid.NewGuid()
            Wpm = 60
            Cpm = 300
            Accuracy = 98.0
            ErrorCount = 1
            PerKeyErrors = Map.empty
        }

    [<Fact>]
    let ``validateSessionCreateDto flags empty lesson id`` () =
        let dto = { validSession with LessonId = Guid.Empty }
        let errors = SessionHandler.validateSessionCreateDto dto
        Assert.True(errors |> List.exists (fun err -> err.Field = "lessonId"))

    [<Fact>]
    let ``validateSessionCreateDto flags empty client session id`` () =
        let dto = { validSession with ClientSessionId = Guid.Empty }
        let errors = SessionHandler.validateSessionCreateDto dto
        Assert.True(errors |> List.exists (fun err -> err.Field = "clientSessionId"))

    [<Fact>]
    let ``validateSessionCreateDto flags negative accuracy`` () =
        let dto = { validSession with Accuracy = -1.0 }
        let errors = SessionHandler.validateSessionCreateDto dto
        Assert.True(errors |> List.exists (fun err -> err.Field = "accuracy"))

    [<Fact>]
    let ``validateSessionCreateDto flags accuracy over 100`` () =
        let dto = { validSession with Accuracy = 120.0 }
        let errors = SessionHandler.validateSessionCreateDto dto
        Assert.True(errors |> List.exists (fun err -> err.Field = "accuracy"))

    [<Fact>]
    let ``validateSessionCreateDto flags negative wpm`` () =
        let dto = { validSession with Wpm = -1 }
        let errors = SessionHandler.validateSessionCreateDto dto
        Assert.True(errors |> List.exists (fun err -> err.Field = "wpm"))

    [<Fact>]
    let ``validateSessionCreateDto flags negative cpm`` () =
        let dto = { validSession with Cpm = -1 }
        let errors = SessionHandler.validateSessionCreateDto dto
        Assert.True(errors |> List.exists (fun err -> err.Field = "cpm"))

    [<Fact>]
    let ``validateSessionCreateDto flags negative error count`` () =
        let dto = { validSession with ErrorCount = -1 }
        let errors = SessionHandler.validateSessionCreateDto dto
        Assert.True(errors |> List.exists (fun err -> err.Field = "errorCount"))

    [<Fact>]
    let ``validateSessionCreateDto accepts valid dto`` () =
        let errors = SessionHandler.validateSessionCreateDto validSession
        Assert.Empty(errors)
