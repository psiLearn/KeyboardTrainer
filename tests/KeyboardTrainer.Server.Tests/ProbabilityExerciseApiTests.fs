namespace KeyboardTrainer.Server.Tests

open System
open System.IO
open System.Text
open System.Threading.Tasks
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Xunit
open Giraffe
open KeyboardTrainer.Server

module ProbabilityExerciseApiTests =
    let private serviceProvider =
        let services = ServiceCollection()
        services.AddGiraffe() |> ignore
        services.BuildServiceProvider()

    let private executeJsonPost (handler: HttpHandler) (body: string) =
        let ctx = DefaultHttpContext()
        ctx.RequestServices <- serviceProvider
        ctx.Request.Method <- "POST"
        ctx.Request.ContentType <- "application/json"
        ctx.Request.Body <- new MemoryStream(Encoding.UTF8.GetBytes(body))

        let responseStream = new MemoryStream()
        ctx.Response.Body <- responseStream

        let next: HttpFunc = fun httpContext -> Task.FromResult(Some httpContext)
        handler next ctx |> Async.AwaitTask |> Async.RunSynchronously |> ignore

        responseStream.Position <- 0L
        use reader = new StreamReader(responseStream, Encoding.UTF8)
        let responseBody = reader.ReadToEnd()
        ctx.Response.StatusCode, responseBody

    [<Fact>]
    let ``postProbabilityExercise returns generated content for valid payload`` () =
        let payload =
            """{"content":"{\"kind\":\"unigram-v1\",\"generatedLength\":12,\"wordLength\":4,\"weights\":{\"q\":0.5,\"w\":0.5}}"}"""

        let statusCode, responseBody = executeJsonPost LessonHandler.postProbabilityExercise payload

        Assert.Equal(200, statusCode)
        Assert.Contains("content", responseBody, StringComparison.OrdinalIgnoreCase)

    [<Fact>]
    let ``postProbabilityExercise returns 400 for missing content`` () =
        let payload = """{"content":""}"""

        let statusCode, responseBody = executeJsonPost LessonHandler.postProbabilityExercise payload

        Assert.Equal(400, statusCode)
        Assert.Contains("Validation failed", responseBody)

    [<Fact>]
    let ``postProbabilityExercise returns 400 for invalid probability keys`` () =
        let payload =
            """{"content":"{\"kind\":\"unigram-v1\",\"generatedLength\":8,\"wordLength\":4,\"weights\":{\"ab\":1.0}}"}"""

        let statusCode, responseBody = executeJsonPost LessonHandler.postProbabilityExercise payload

        Assert.Equal(400, statusCode)
        Assert.Contains("Invalid weight key", responseBody)
