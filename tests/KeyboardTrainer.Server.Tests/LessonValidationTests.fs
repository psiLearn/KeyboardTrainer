namespace KeyboardTrainer.Server.Tests

open System
open Xunit
open KeyboardTrainer.Shared
open KeyboardTrainer.Server

module LessonValidationTests =
    let private validLesson: LessonCreateDto =
        {
            Title = "Lesson title"
            Difficulty = Difficulty.A1
            ContentType = ContentType.Words
            Language = Language.French
            TextContent = "some content"
            Tags = None
        }

    [<Fact>]
    let ``validateLessonCreateDto flags empty title`` () =
        let dto = { validLesson with Title = "" }
        let errors = LessonHandler.validateLessonCreateDto dto
        Assert.True(errors |> List.exists (fun err -> err.Field = "title"))

    [<Fact>]
    let ``validateLessonCreateDto flags long title`` () =
        let dto = { validLesson with Title = String('a', 101) }
        let errors = LessonHandler.validateLessonCreateDto dto
        Assert.True(errors |> List.exists (fun err -> err.Field = "title"))

    [<Fact>]
    let ``validateLessonCreateDto flags empty content`` () =
        let dto = { validLesson with TextContent = "" }
        let errors = LessonHandler.validateLessonCreateDto dto
        Assert.True(errors |> List.exists (fun err -> err.Field = "textContent"))

    [<Fact>]
    let ``validateLessonCreateDto flags long content`` () =
        let dto = { validLesson with TextContent = String('b', 5001) }
        let errors = LessonHandler.validateLessonCreateDto dto
        Assert.True(errors |> List.exists (fun err -> err.Field = "textContent"))

    [<Fact>]
    let ``validateLessonCreateDto accepts valid dto`` () =
        let errors = LessonHandler.validateLessonCreateDto validLesson
        Assert.Empty(errors)
