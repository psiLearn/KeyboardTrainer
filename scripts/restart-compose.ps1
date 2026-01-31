param(
    [string]$EnvFile = ".env.docker.dev",
    [string]$ProjectName,
    [switch]$SkipClientBuild,
    [switch]$SkipServerBuild
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
Set-Location $repoRoot

function Invoke-Checked {
    param(
        [string]$Label,
        [scriptblock]$Command
    )

    & $Command
    if ($LASTEXITCODE -ne 0) {
        throw "$Label failed with exit code $LASTEXITCODE."
    }
}

function Get-ComposeProjectName {
    param(
        [string]$EnvFilePath,
        [string]$DefaultName
    )

    if ($EnvFilePath -and (Test-Path $EnvFilePath)) {
        $line = Get-Content $EnvFilePath | Where-Object { $_ -match '^\s*COMPOSE_PROJECT_NAME\s*=' } | Select-Object -First 1
        if ($line) {
            $value = ($line -split "=", 2)[1].Trim()
            if ($value) {
                return $value
            }
        }
    }

    if ($env:COMPOSE_PROJECT_NAME) {
        return $env:COMPOSE_PROJECT_NAME
    }

    return $DefaultName
}

$defaultProjectName = Split-Path -Leaf $repoRoot
$resolvedProjectName = if ($ProjectName) { $ProjectName } else { Get-ComposeProjectName -EnvFilePath $EnvFile -DefaultName $defaultProjectName }
$previousComposeProjectName = $env:COMPOSE_PROJECT_NAME
$env:COMPOSE_PROJECT_NAME = $resolvedProjectName

try {
    Write-Host "🛑 Stopping docker-compose stack ($resolvedProjectName)..."
    Invoke-Checked "docker-compose down" { docker-compose --env-file $EnvFile down }

    if (-not $SkipClientBuild) {
        Write-Host "📦 Installing client dependencies..."
        if (Test-Path "package-lock.json") {
            Invoke-Checked "npm ci" { npm ci }
        } else {
            Invoke-Checked "npm install" { npm install }
        }

        Write-Host "🧱 Building client assets..."
        Invoke-Checked "npm run build:client" { npm run build:client }
    }

    if (-not $SkipServerBuild) {
        Write-Host "🧱 Building server project..."
        Invoke-Checked "dotnet build server" { dotnet build src/Server/KeyboardTrainer.Server.fsproj }
    }

    Write-Host "🚀 Starting docker-compose stack with rebuild..."
    Invoke-Checked "docker-compose up" { docker-compose --env-file $EnvFile up -d --build }

    Write-Host "✅ Done."
}
finally {
    if ($null -ne $previousComposeProjectName -and $previousComposeProjectName -ne "") {
        $env:COMPOSE_PROJECT_NAME = $previousComposeProjectName
    } else {
        Remove-Item Env:COMPOSE_PROJECT_NAME -ErrorAction SilentlyContinue
    }
}
