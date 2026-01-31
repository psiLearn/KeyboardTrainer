param(
    [string]$BaseUrl = "http://localhost:5000"
)

$ErrorActionPreference = "Stop"
$supportsSkipHttpErrorCheck = $PSVersionTable.PSVersion.Major -ge 7

function Assert-True {
    param(
        [bool]$Condition,
        [string]$Message
    )
    if (-not $Condition) {
        throw $Message
    }
}

function Assert-Status {
    param(
        [int]$Expected,
        [int]$Actual,
        [string]$Message
    )
    if ($Expected -ne $Actual) {
        throw "$Message (expected $Expected, got $Actual)"
    }
}

function Invoke-WebRequestSafe {
    param(
        [string]$Method,
        [string]$Url,
        [string]$ContentType,
        [string]$Body
    )

    if ($supportsSkipHttpErrorCheck) {
        return Invoke-WebRequest `
            -Method $Method `
            -Uri $Url `
            -ContentType $ContentType `
            -Body $Body `
            -SkipHttpErrorCheck
    }

    try {
        return Invoke-WebRequest `
            -Method $Method `
            -Uri $Url `
            -ContentType $ContentType `
            -Body $Body `
            -ErrorAction Stop
    } catch {
        $response = $_.Exception.Response
        if (-not $response) {
            throw
        }

        $content = ""
        try {
            $stream = $response.GetResponseStream()
            if ($stream) {
                $reader = New-Object System.IO.StreamReader($stream)
                $content = $reader.ReadToEnd()
                $reader.Close()
            }
        } catch {
            $content = ""
        }

        [pscustomobject]@{
            StatusCode = [int]$response.StatusCode
            Content = $content
            BaseResponse = $response
        }
    }
}

function Invoke-Json {
    param(
        [string]$Method,
        [string]$Url,
        [object]$Body = $null
    )

    $payload = if ($null -ne $Body) { $Body | ConvertTo-Json -Depth 6 } else { $null }

    $response = Invoke-WebRequestSafe `
        -Method $Method `
        -Url $Url `
        -ContentType "application/json" `
        -Body $payload

    $statusCode = if ($null -ne $response.StatusCode) {
        [int]$response.StatusCode
    } elseif ($response.BaseResponse -and $response.BaseResponse.StatusCode) {
        [int]$response.BaseResponse.StatusCode
    } else {
        0
    }

    $content = $response.Content
    $json = $null
    if (-not [string]::IsNullOrWhiteSpace($content)) {
        try {
            $json = $content | ConvertFrom-Json -Depth 10
        } catch {
            $json = $null
        }
    }

    [pscustomobject]@{
        StatusCode = $statusCode
        Content = $content
        Json = $json
    }
}

Write-Host "API integration test against $BaseUrl"

Write-Host "Health check..."
$healthResponse = Invoke-WebRequestSafe -Method Get -Url "$BaseUrl/health" -ContentType "application/json" -Body $null
$healthStatus = if ($null -ne $healthResponse.StatusCode) {
    [int]$healthResponse.StatusCode
} elseif ($healthResponse.BaseResponse -and $healthResponse.BaseResponse.StatusCode) {
    [int]$healthResponse.BaseResponse.StatusCode
} else {
    0
}
Assert-Status 200 $healthStatus "Health check status"
Assert-True ($healthResponse.Content -eq "OK") "Health check failed: $($healthResponse.Content)"

Write-Host "Fetching lessons..."
$lessonsResponse = Invoke-Json -Method Get -Url "$BaseUrl/api/lessons"
Assert-Status 200 $lessonsResponse.StatusCode "GET /api/lessons"
$lessons = @($lessonsResponse.Json)
Assert-True ($lessons.Count -gt 0) "No lessons returned; ensure seed data exists."
$seedLessonId = $lessons[0].id
Write-Host "Using seeded lessonId: $seedLessonId"

Write-Host "Creating lesson..."
$stamp = Get-Date -Format "yyyyMMddHHmmss"
$newLesson = @{
    title = "Integration Test $stamp"
    difficulty = @{ case = "A1" }
    contentType = @{ case = "Words" }
    language = @{ case = "French" }
    textContent = "integration test content"
}
$createLessonResponse = Invoke-Json -Method Post -Url "$BaseUrl/api/lessons" -Body $newLesson
Assert-Status 201 $createLessonResponse.StatusCode "POST /api/lessons"
$createdLessonId = $createLessonResponse.Json.id
Assert-True (-not [string]::IsNullOrWhiteSpace($createdLessonId)) "Created lesson id missing"

Write-Host "Fetching created lesson..."
$getLessonResponse = Invoke-Json -Method Get -Url "$BaseUrl/api/lessons/$createdLessonId"
Assert-Status 200 $getLessonResponse.StatusCode "GET /api/lessons/{id}"
Assert-True ($getLessonResponse.Json.title -eq $newLesson.title) "Lesson title mismatch"

Write-Host "Updating lesson..."
$updateLesson = @{
    title = "$($newLesson.title) Updated"
    difficulty = @{ case = "A2" }
    contentType = @{ case = "Words" }
    language = @{ case = "French" }
    textContent = "integration test content updated"
}
$updateLessonResponse = Invoke-Json -Method Put -Url "$BaseUrl/api/lessons/$createdLessonId" -Body $updateLesson
Assert-Status 200 $updateLessonResponse.StatusCode "PUT /api/lessons/{id}"
Assert-True ($updateLessonResponse.Json.title -eq $updateLesson.title) "Updated lesson title mismatch"

Write-Host "Validating lesson create errors..."
$invalidLesson = @{
    title = ""
    difficulty = @{ case = "A1" }
    contentType = @{ case = "Words" }
    language = @{ case = "French" }
    textContent = ""
}
$invalidLessonResponse = Invoke-Json -Method Post -Url "$BaseUrl/api/lessons" -Body $invalidLesson
Assert-Status 400 $invalidLessonResponse.StatusCode "POST /api/lessons validation"
Assert-True ($null -ne $invalidLessonResponse.Json.errors) "Lesson validation errors missing"

Write-Host "Creating session with validation errors..."
$invalidSession = @{
    clientSessionId = [guid]::Empty.ToString()
    lessonId = [guid]::Empty.ToString()
    wpm = -1
    cpm = -1
    accuracy = 120.0
    errorCount = -1
    perKeyErrors = @{ "0" = 1 }
}
$invalidSessionResponse = Invoke-Json -Method Post -Url "$BaseUrl/api/sessions" -Body $invalidSession
Assert-Status 400 $invalidSessionResponse.StatusCode "POST /api/sessions validation"
Assert-True ($null -ne $invalidSessionResponse.Json.errors) "Session validation errors missing"

Write-Host "Creating session with missing lesson..."
$missingLessonSession = @{
    clientSessionId = [guid]::NewGuid().ToString()
    lessonId = [guid]::NewGuid().ToString()
    wpm = 10
    cpm = 50
    accuracy = 99.0
    errorCount = 0
    perKeyErrors = @{}
}
$missingLessonResponse = Invoke-Json -Method Post -Url "$BaseUrl/api/sessions" -Body $missingLessonSession
Assert-Status 400 $missingLessonResponse.StatusCode "POST /api/sessions missing lesson"

Write-Host "Creating session..."
$sessionId = [guid]::NewGuid().ToString()
$sessionPayload = @{
    clientSessionId = $sessionId
    lessonId = $seedLessonId
    wpm = 42
    cpm = 210
    accuracy = 98.5
    errorCount = 1
    perKeyErrors = @{ "0" = 1 }
}
$sessionResponse = Invoke-Json -Method Post -Url "$BaseUrl/api/sessions" -Body $sessionPayload
Assert-Status 201 $sessionResponse.StatusCode "POST /api/sessions"
$createdSessionId = $sessionResponse.Json.id
Assert-True (-not [string]::IsNullOrWhiteSpace($createdSessionId)) "Created session id missing"

Write-Host "Checking session idempotency..."
$idempotentResponse = Invoke-Json -Method Post -Url "$BaseUrl/api/sessions" -Body $sessionPayload
Assert-Status 201 $idempotentResponse.StatusCode "POST /api/sessions idempotent"
Assert-True ($idempotentResponse.Json.id -eq $createdSessionId) "Idempotent session id mismatch"

Write-Host "Checking session conflict..."
$conflictPayload = $sessionPayload.Clone()
$conflictPayload.lessonId = $createdLessonId
$conflictResponse = Invoke-Json -Method Post -Url "$BaseUrl/api/sessions" -Body $conflictPayload
Assert-Status 409 $conflictResponse.StatusCode "POST /api/sessions conflict"

Write-Host "Fetching sessions for lesson..."
$sessionsResponse = Invoke-Json -Method Get -Url "$BaseUrl/api/lessons/$seedLessonId/sessions"
Assert-Status 200 $sessionsResponse.StatusCode "GET /api/lessons/{lessonId}/sessions"
$sessions = @($sessionsResponse.Json)
Assert-True (($sessions | Where-Object { $_.id -eq $createdSessionId }).Count -ge 1) "Created session not found in lesson sessions"

Write-Host "Fetching last session..."
$lastSessionResponse = Invoke-Json -Method Get -Url "$BaseUrl/api/sessions/last"
Assert-Status 200 $lastSessionResponse.StatusCode "GET /api/sessions/last"

Write-Host "Deleting lesson..."
$deleteLessonResponse = Invoke-Json -Method Delete -Url "$BaseUrl/api/lessons/$createdLessonId"
Assert-Status 204 $deleteLessonResponse.StatusCode "DELETE /api/lessons/{id}"

Write-Host "Integration test complete."
