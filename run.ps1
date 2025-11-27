# Run script for development: applies migrations and runs the app with Development environment
param(
    [switch]$SkipMigrations
)

$projDir = Join-Path -Path $PSScriptRoot -ChildPath "WebApplication5"
$projFile = "WebApplication5.csproj"

Write-Host "Setting ASPNETCORE_ENVIRONMENT=Development"
$env:ASPNETCORE_ENVIRONMENT = "Development"

Push-Location $projDir
try {
    if (-not $SkipMigrations) {
        Write-Host "Applying EF Core migrations..."
        dotnet ef database update --verbose
    }

    Write-Host "Starting application (dotnet run)..."
    dotnet run --project $projFile
}
finally {
    Pop-Location
}
