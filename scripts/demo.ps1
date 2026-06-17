<#
.SYNOPSIS
    End-to-end demo of the cross-agency child-rights flow.
.DESCRIPTION
    Drives the running stack (via the API gateway on :8080 by default) through a
    full scenario and prints what each step produced. Run the stack first with
    docker-compose (recommended, so RabbitMQ messaging works) or scripts/run-local.ps1.
.PARAMETER BaseUrl
    Gateway base URL. Defaults to http://localhost:8080.
#>

param(
    [string]$BaseUrl = "http://localhost:8080"
)

$ErrorActionPreference = "Stop"

# Deterministic demo identifiers seeded by the Education service.
$olehId   = "33333333-3333-3333-3333-333333331001"  # 12 unexcused absences (attendance red flag)
$mariiaId = "33333333-3333-3333-3333-333333331002"  # maths profile, strong in philology (profile change)
$class9   = "22222222-2222-2222-2222-222222220009"
$schoolId = "11111111-1111-1111-1111-111111111111"

function Show($title, $obj) {
    Write-Host ""
    Write-Host "=== $title ===" -ForegroundColor Cyan
    $obj | ConvertTo-Json -Depth 6
}

Write-Host "Child Rights Monitoring — end-to-end demo" -ForegroundColor Green
Write-Host "Gateway: $BaseUrl"

# 1. Education: pupil profile (attendance + grades).
$profile = Invoke-RestMethod "$BaseUrl/education/api/students/$olehId"
Show "1. Education profile (Oleh)" $profile

# 2. Analysis: run on-demand analysis for Oleh -> attendance red flag.
$run = Invoke-RestMethod -Method Post "$BaseUrl/analysis/api/analysis/students/$olehId/run"
Show "2. Analysis run (Oleh) -> red flags + recommendations" $run

# 3. Analysis: profiling recommendation for Mariia (change profile).
$runMariia = Invoke-RestMethod -Method Post "$BaseUrl/analysis/api/analysis/students/$mariiaId/run"
Show "3. Analysis run (Mariia) -> profiling recommendation" $runMariia

# 4. Medical: third recurring visit -> medical concern event -> medical red flag.
$visit = Invoke-RestMethod -Method Post "$BaseUrl/medical/api/medical/visits" -ContentType "application/json" -Body (@{
    studentId = $olehId; studentName = "Петренко Олег"; conditionCategory = "Респіраторні захворювання"
    date = (Get-Date -Format "yyyy-MM-dd"); note = "Повторне звернення"
} | ConvertTo-Json)
Show "4. Medical visit (3rd) -> concern raised" $visit

# 5. Juvenile Police: bullying report for the class -> class-level red flag.
$report = Invoke-RestMethod -Method Post "$BaseUrl/juvenile/api/juvenile/bullying-reports" -ContentType "application/json" -Body (@{
    classId = $class9; schoolId = $schoolId; severity = "Red"; summary = "Звернення щодо булінгу у 9-А"
} | ConvertTo-Json)
Show "5. Bullying report filed -> class-level signal" $report

Start-Sleep -Seconds 3  # allow asynchronous messaging to propagate

# 6. Analysis dashboard summary (aggregated KPIs).
$dashboard = Invoke-RestMethod "$BaseUrl/analysis/api/dashboard/summary"
Show "6. Dashboard summary" $dashboard

# 7. Notifications produced from the red flags.
$notifications = Invoke-RestMethod "$BaseUrl/notifications/api/notifications"
Show "7. Notifications dispatched" $notifications

# 8. Inter-agency referrals (Red flags escalate automatically).
$referrals = Invoke-RestMethod "$BaseUrl/notifications/api/referrals"
Show "8. Inter-agency referrals" $referrals

# 9. Social-services cases opened from referrals.
$cases = Invoke-RestMethod "$BaseUrl/social/api/social/cases"
Show "9. Social-services cases" $cases

Write-Host ""
Write-Host "Demo complete." -ForegroundColor Green
