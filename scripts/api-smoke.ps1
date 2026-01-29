param(
    [string]$BaseUrl = "http://localhost:5000"
)

$ErrorActionPreference = "Stop"

Write-Host "API smoke test against $BaseUrl"

Write-Host "Health check..."
$health = Invoke-RestMethod "$BaseUrl/health"
if ($health -ne "OK") {
    throw "Health check failed: $health"
}

Write-Host "Fetching lessons..."
$lessons = Invoke-RestMethod "$BaseUrl/api/lessons"
$lessonList = @($lessons)
if ($lessonList.Count -lt 1) {
    throw "No lessons returned; ensure seed data exists."
}

$lessonId = $lessonList[0].id
Write-Host "Using lessonId: $lessonId"

Write-Host "Creating session..."
$sessionId = [guid]::NewGuid().ToString()
$body = @{
    clientSessionId = $sessionId
    lessonId = $lessonId
    wpm = 42
    cpm = 210
    accuracy = 98.5
    errorCount = 1
    perKeyErrors = @{ "0" = 1 }
} | ConvertTo-Json -Depth 5

try {
    $session = Invoke-RestMethod -Method Post -ContentType "application/json" -Body $body "$BaseUrl/api/sessions"
    Write-Host "Created session: $($session.id)"
} catch {
    if ($_.Exception.Response -and $_.Exception.Response.StatusCode.value__ -eq 409) {
        Write-Host "Session already exists (409)."
    } else {
        throw
    }
}

Write-Host "Fetching sessions for lesson..."
$sessions = Invoke-RestMethod "$BaseUrl/api/lessons/$lessonId/sessions"
$sessionsList = @($sessions)
Write-Host "Sessions returned: $($sessionsList.Count)"

Write-Host "Smoke test complete."
