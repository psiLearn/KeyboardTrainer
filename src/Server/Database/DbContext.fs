namespace KeyboardTrainer.Server

open Npgsql
open System
open System.Data
open System.IO
open Dapper

module DbContext =
    /// Get database connection string from environment or use default
    let getConnectionString () =
        let getEnv name defaultValue =
            System.Environment.GetEnvironmentVariable name
            |> function
                | null | "" -> defaultValue
                | value -> value

        let hasExplicitDbEnv () =
            [ "DB_HOST"; "DB_PORT"; "DB_NAME"; "DB_USER"; "DB_PASSWORD" ]
            |> List.exists (fun name ->
                match System.Environment.GetEnvironmentVariable name with
                | null | "" -> false
                | _ -> true)

        let buildFromEnv () =
            let host = getEnv "DB_HOST" "localhost"
            let port = getEnv "DB_PORT" "5434"
            let database = getEnv "DB_NAME" "keyboardtrainer"
            let user = getEnv "DB_USER" "trainer"
            let password = getEnv "DB_PASSWORD" "trainer123"
            $"Host={host};Port={port};Database={database};Username={user};Password={password}"

        match hasExplicitDbEnv (), System.Environment.GetEnvironmentVariable "DATABASE_URL" with
        | true, _ -> buildFromEnv ()
        | false, null | false, "" -> buildFromEnv ()
        | false, connStr -> connStr

    /// Create a new database connection
    let createConnection () : IDbConnection =
        new NpgsqlConnection(getConnectionString()) :> IDbConnection

    /// Initialize Dapper type handlers for custom types
    let initializeDapper () =
        // Register type handlers if needed
        ()

    /// Run migrations on startup
    let runMigrations () =
        async {
            use conn = createConnection()
            conn.Open()
            
            try
                // Read and execute migration files
                printfn "[DB] Running migrations..."

                let baseDir = AppContext.BaseDirectory
                let candidates =
                    [
                        Path.Combine(baseDir, "Database", "Migrations")
                        Path.Combine(baseDir, "Migrations")
                        Path.Combine(Environment.CurrentDirectory, "src", "Server", "Database", "Migrations")
                    ]

                let migrationsDir =
                    candidates
                    |> List.tryFind Directory.Exists

                match migrationsDir with
                | None ->
                    let message = "[DB] Migration failed: No migrations directory found in expected locations."
                    printfn "%s" message
                    return raise (InvalidOperationException message)
                | Some dir ->
                    let files =
                        Directory.GetFiles(dir, "*.sql")
                        |> Array.sort

                    if files.Length = 0 then
                        let message = $"[DB] Migration failed: No migration files found in {dir}"
                        printfn "%s" message
                        return raise (InvalidOperationException message)
                    else
                        for filePath in files do
                            let migrationSql = File.ReadAllText(filePath)
                            let! _ = conn.ExecuteAsync(migrationSql) |> Async.AwaitTask
                            printfn "[DB] ✓ Migration %s completed" (Path.GetFileName(filePath))

                        printfn "[DB] All migrations completed successfully"
            with
            | ex ->
                printfn "[DB] Migration failed: %s" ex.Message
                return raise ex
        }
    
    /// Check if database is accessible
    let healthCheck () =
        async {
            try
                use conn = createConnection()
                conn.Open()
                let! _ = conn.ExecuteAsync("SELECT 1") |> Async.AwaitTask
                return true
            with _ ->
                return false
        }
