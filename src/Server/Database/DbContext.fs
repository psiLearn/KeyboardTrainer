namespace KeyboardTrainer.Server

open Npgsql
open System
open System.Data
open Dapper

module DbContext =
    /// Get database connection string from environment or use default
    let getConnectionString () =
        System.Environment.GetEnvironmentVariable "DATABASE_URL"
        |> function
            | null | "" -> "Server=localhost;Database=keyboardtrainer;User Id=trainer;Password=trainer123"
            | connStr -> connStr

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
                
                let migration1 = 
                    System.IO.File.ReadAllText(
                        "./src/Server/Database/Migrations/001_CreateLessonsAndSessionsTables.sql"
                    )
                let! _ = conn.ExecuteAsync(migration1) |> Async.AwaitTask
                printfn "[DB] ✓ Migration 001 completed"
                
                let migration2 = 
                    System.IO.File.ReadAllText(
                        "./src/Server/Database/Migrations/002_SeedFrenchLessons.sql"
                    )
                let! _ = conn.ExecuteAsync(migration2) |> Async.AwaitTask
                printfn "[DB] ✓ Migration 002 (seed data) completed"
                
                printfn "[DB] All migrations completed successfully"
            with
            | ex ->
                printfn "[DB] Warning: Migration may have already been applied: %s" ex.Message
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
