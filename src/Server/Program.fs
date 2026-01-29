namespace KeyboardTrainer.Server

open System
open System.IO
open Giraffe
open Saturn
open KeyboardTrainer.Shared

module Program =
    let webApp: HttpHandler =
        choose [
            // Health check endpoint
            GET >=> route "/health" >=> text "OK"
            
            // Lesson endpoints
            GET >=> route "/api/lessons" >=> LessonHandler.getAllLessons
            GET >=> routef "/api/lessons/%O" LessonHandler.getLessonById
            POST >=> route "/api/lessons" >=> LessonHandler.postLesson
            PUT >=> routef "/api/lessons/%O" LessonHandler.putLesson
            DELETE >=> routef "/api/lessons/%O" LessonHandler.deleteLesson
            
            // Session endpoints
            POST >=> route "/api/sessions" >=> SessionHandler.postSession
            GET >=> routef "/api/lessons/%O/sessions" SessionHandler.getSessionsByLesson
            GET >=> route "/api/sessions/last" >=> SessionHandler.getLastSession
            
            // 404 handler
            RequestErrors.NOT_FOUND "Not Found"
        ]

    let app =
        application {
            url "http://0.0.0.0:5000"
            use_router webApp
            memory_cache
            use_static "public"
            use_gzip
        }

    [<EntryPoint>]
    let main _ =
        let port = 
            System.Environment.GetEnvironmentVariable "PORT"
            |> function
            | null | "" -> 5000
            | p -> int p
        
        printfn "Starting Keyboard Trainer server on port %d..." port
        
        // Run migrations on startup
        async {
            try
                printfn "Running database migrations..."
                let! _ = DbContext.runMigrations()
                
                // Health check
                let! healthOk = DbContext.healthCheck()
                if healthOk then
                    printfn "Database is ready"
                else
                    printfn "WARNING: Database health check failed"
            with
            | ex ->
                printfn "ERROR: Failed to initialize database: %s" ex.Message
                return raise ex
        }
        |> Async.RunSynchronously
        
        run app
        0
