namespace KeyboardTrainer.Server

open System
open Giraffe
open KeyboardTrainer.Shared
open KeyboardTrainer.Server.LessonHandlerCommon

module LessonHandlerExercise =
    let getLessonExercise (id: Guid): HttpHandler =
        fun next ctx ->
            task {
                try
                    let! lesson = LessonRepository.getLessonById id |> Async.StartAsTask
                    match lesson with
                    | None ->
                        ctx.SetStatusCode 404
                        return! json (apiError 404 $"Lesson with id {id} not found" None) next ctx
                    | Some value ->
                        let resolved =
                            match value.ContentType with
                            | ContentType.Probability -> ProbabilityExerciseGenerator.generateFromProbabilityJson value.Content None None
                            | _ -> Ok value.Content

                        match resolved with
                        | Ok content ->
                            let response: ExerciseDto = { Content = content }
                            return! json response next ctx
                        | Error message ->
                            ctx.SetStatusCode 400
                            return! json (apiError 400 "Failed to generate exercise" (Some [ { Field = "content"; Message = message } ])) next ctx
                with ex ->
                    ctx.SetStatusCode 500
                    return! json (apiError 500 "Failed to resolve lesson exercise" (Some [ { Field = "server"; Message = ex.Message } ])) next ctx
            }

    let postProbabilityExercise: HttpHandler =
        fun next ctx ->
            task {
                try
                    let! dto = ctx.BindJsonAsync<ProbabilityExerciseRequestDto>()
                    if String.IsNullOrWhiteSpace dto.Content then
                        ctx.SetStatusCode 400
                        return! json (apiError 400 "Validation failed" (Some [ { Field = "content"; Message = "Content is required" } ])) next ctx
                    else
                        match ProbabilityExerciseGenerator.generateFromProbabilityJson dto.Content dto.GeneratedLength dto.WordLength with
                        | Ok content ->
                            let response: ExerciseDto = { Content = content }
                            return! json response next ctx
                        | Error message ->
                            ctx.SetStatusCode 400
                            return! json (apiError 400 "Failed to generate exercise" (Some [ { Field = "content"; Message = message } ])) next ctx
                with
                | :? System.Text.Json.JsonException as ex ->
                    ctx.SetStatusCode 400
                    return! json (apiError 400 "Invalid request body" (Some [ { Field = "body"; Message = ex.Message } ])) next ctx
                | ex ->
                    ctx.SetStatusCode 500
                    return! json (apiError 500 "Failed to generate probability exercise" (Some [ { Field = "server"; Message = ex.Message } ])) next ctx
            }
