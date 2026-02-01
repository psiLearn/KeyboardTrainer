param(
    [string]$BaseUrl = "http://localhost:5000",
    [string]$LessonsPath = (Join-Path $PSScriptRoot "..\\data\\lessons-beginner.json"),
    [switch]$Force
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Get-TitleValue {
    param([object]$Lesson)

    if ($null -eq $Lesson) { return $null }
    $props = $Lesson.PSObject.Properties.Name
    if ($props -contains "title") { return $Lesson.title }
    if ($props -contains "Title") { return $Lesson.Title }
    return $null
}

function Get-ExistingTitles {
    param([string]$ApiBaseUrl)

    $titles = @{}
    try {
        $existing = Invoke-RestMethod -Method Get -Uri "$ApiBaseUrl/api/lessons"
        if ($null -ne $existing) {
            foreach ($lesson in @($existing)) {
                $title = Get-TitleValue $lesson
                if ([string]::IsNullOrWhiteSpace($title)) { continue }
                $titles[$title.ToLowerInvariant()] = $true
            }
        }
    } catch {
        Write-Warning "Could not fetch existing lessons: $($_.Exception.Message)"
    }
    return $titles
}

$resolvedLessonsPath = Resolve-Path -Path $LessonsPath -ErrorAction Stop
$rawJson = Get-Content -Path $resolvedLessonsPath -Raw
$lessons = $rawJson | ConvertFrom-Json

if ($null -eq $lessons -or $lessons.Count -eq 0) {
    throw "No lessons found in $resolvedLessonsPath"
}

Write-Host "Seeding lessons from $resolvedLessonsPath to $BaseUrl" -ForegroundColor Cyan

$existingTitles = Get-ExistingTitles -ApiBaseUrl $BaseUrl
$created = 0
$skipped = 0
$failed = 0

foreach ($lesson in $lessons) {
    $title = Get-TitleValue $lesson
    if ([string]::IsNullOrWhiteSpace($title)) {
        Write-Warning "Skipping lesson with no title."
        $failed++
        continue
    }

    if (-not $Force -and $existingTitles.ContainsKey($title.ToLowerInvariant())) {
        Write-Host "Skipping existing lesson: $title" -ForegroundColor Yellow
        $skipped++
        continue
    }

    $payload = $lesson | ConvertTo-Json -Depth 6
    try {
        Invoke-RestMethod -Method Post -Uri "$BaseUrl/api/lessons" -ContentType "application/json" -Body $payload | Out-Null
        Write-Host "Created: $title" -ForegroundColor Green
        $created++
    } catch {
        $failed++
        $message = $_.Exception.Message
        $details = $_.ErrorDetails.Message
        if (-not [string]::IsNullOrWhiteSpace($details)) {
            $message = "$message - $details"
        }
        Write-Warning "Failed: $title - $message"
    }
}

Write-Host "Done. Created: $created, Skipped: $skipped, Failed: $failed" -ForegroundColor Cyan
