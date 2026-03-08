namespace KeyboardTrainer.Server

open System
open Giraffe
open KeyboardTrainer.Shared
open KeyboardTrainer.Server.LessonHandlerCommon

module LessonHandlerCrud =
    let getAllLessons: HttpHandler =
        fun next ctx ->
            task {
                try
                    let! lessons = LessonRepository.getAllLessons() |> Async.StartAsTask
                    return! json (lessons |> List.map toLessonDto) next ctx
                with ex ->
                    ctx.SetStatusCode 500
                    let error = apiError 500 "Failed to retrieve lessons" (Some [ { Field = "server"; Message = ex.Message } ])
                    return! json error next ctx
            }

    let getLessonById (id: Guid): HttpHandler =
        fun next ctx ->
            task {
                try
                    let! lesson = LessonRepository.getLessonById id |> Async.StartAsTask
                    match lesson with
                    | Some value -> return! json (toLessonDto value) next ctx
                    | None ->
                        ctx.SetStatusCode 404
                        return! json (apiError 404 $"Lesson with id {id} not found" None) next ctx
                with ex ->
                    ctx.SetStatusCode 500
                    let error = apiError 500 "Failed to retrieve lesson" (Some [ { Field = "server"; Message = ex.Message } ])
                    return! json error next ctx
            }

    let postLesson: HttpHandler =
        fun next ctx ->
            task {
                try
                    let! dto = ctx.BindJsonAsync<LessonCreateDto>()
                    let errors = validateLessonCreateDto dto
                    if not (List.isEmpty errors) then
                        ctx.SetStatusCode 400
                        return! json (apiError 400 "Validation failed" (Some errors)) next ctx
                    else
                        let! lesson = LessonRepository.createLesson dto |> Async.StartAsTask
                        ctx.SetStatusCode 201
                        ctx.SetHttpHeader("Location", $"/api/lessons/{lesson.Id}")
                        return! json (toLessonDto lesson) next ctx
                with
                | :? System.Text.Json.JsonException as ex ->
                    ctx.SetStatusCode 400
                    return! json (apiError 400 "Invalid request body" (Some [ { Field = "body"; Message = ex.Message } ])) next ctx
                | ex ->
                    ctx.SetStatusCode 500
                    return! json (apiError 500 "Failed to create lesson" (Some [ { Field = "server"; Message = ex.Message } ])) next ctx
            }

    let putLesson (id: Guid): HttpHandler =
        fun next ctx ->
            task {
                try
                    let! dto = ctx.BindJsonAsync<LessonCreateDto>()
                    let errors = validateLessonCreateDto dto
                    if not (List.isEmpty errors) then
                        ctx.SetStatusCode 400
                        return! json (apiError 400 "Validation failed" (Some errors)) next ctx
                    else
                        let! lesson = LessonRepository.updateLesson id dto |> Async.StartAsTask
                        match lesson with
                        | Some value -> return! json (toLessonDto value) next ctx
                        | None ->
                            ctx.SetStatusCode 404
                            return! json (apiError 404 $"Lesson with id {id} not found" None) next ctx
                with
                | :? System.Text.Json.JsonException as ex ->
                    ctx.SetStatusCode 400
                    return! json (apiError 400 "Invalid request body" (Some [ { Field = "body"; Message = ex.Message } ])) next ctx
                | ex ->
                    ctx.SetStatusCode 500
                    return! json (apiError 500 "Failed to update lesson" (Some [ { Field = "server"; Message = ex.Message } ])) next ctx
            }

    let deleteLesson (id: Guid): HttpHandler =
        fun next ctx ->
            task {
                try
                    let! deleted = LessonRepository.deleteLesson id |> Async.StartAsTask
                    if deleted then
                        ctx.SetStatusCode 204
                        return! next ctx
                    else
                        ctx.SetStatusCode 404
                        return! json (apiError 404 $"Lesson with id {id} not found" None) next ctx
                with ex ->
                    ctx.SetStatusCode 500
                    return! json (apiError 500 "Failed to delete lesson" (Some [ { Field = "server"; Message = ex.Message } ])) next ctx
            }
