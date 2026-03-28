namespace KeyboardTrainer.Server

open System
open System.IO
open Giraffe
open Saturn
open KeyboardTrainer.Shared

module Program =
    let private parseDotEnvLine (line: string) =
        let trimmed = line.Trim()
        if String.IsNullOrWhiteSpace trimmed || trimmed.StartsWith "#" then
            None
        else
            match trimmed.IndexOf '=' with
            | -1 -> None
            | idx ->
                let key = trimmed.Substring(0, idx).Trim()
                let value =
                    trimmed.Substring(idx + 1).Trim()
                    |> fun v -> v.Trim('"')

                if String.IsNullOrWhiteSpace key then None
                else Some(key, value)

    let private loadDevelopmentEnv () =
        let envName = System.Environment.GetEnvironmentVariable "ASPNETCORE_ENVIRONMENT"
        let shouldLoad =
            String.IsNullOrWhiteSpace envName
            || envName.Equals("Development", StringComparison.OrdinalIgnoreCase)

        let envFile = ".env.development"

        if shouldLoad && File.Exists envFile then
            File.ReadLines envFile
            |> Seq.choose parseDotEnvLine
            |> Seq.iter (fun (key, value) ->
                if String.IsNullOrWhiteSpace (System.Environment.GetEnvironmentVariable key) then
                    System.Environment.SetEnvironmentVariable(key, value))

    let webApp: HttpHandler =
        choose [
            // Health check endpoint
            GET >=> route "/health" >=> text "OK"
            
            // Lesson endpoints
            GET >=> route "/api/lessons" >=> LessonHandlerCrud.getAllLessons
            GET >=> routef "/api/lessons/%O" LessonHandlerCrud.getLessonById
            GET >=> routef "/api/lessons/%O/exercise" LessonHandlerExercise.getLessonExercise
            POST >=> route "/api/lessons" >=> LessonHandlerCrud.postLesson
            PUT >=> routef "/api/lessons/%O" LessonHandlerCrud.putLesson
            DELETE >=> routef "/api/lessons/%O" LessonHandlerCrud.deleteLesson

            // Exercise generation endpoints
            POST >=> route "/api/exercises/probability" >=> LessonHandlerExercise.postProbabilityExercise
            
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
        loadDevelopmentEnv ()

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
