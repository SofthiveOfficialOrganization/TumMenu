# # Start-All.ps1
# Write-Host "Starting Location API silently..."
# # Starts the Location API without showing a terminal window
# Start-Process dotnet "run --project Microservices/Location/WebAPI/WebAPI.csproj --launch-profile http" -WindowStyle Hidden

# Write-Host "Starting Main Application..."
# # Starts the main app and keeps the terminal open to see logs
# dotnet watch --project WebUI/WebUI.csproj


# Start-All.ps1

$webUiUrls = "https://0.0.0.0:7019;http://0.0.0.0:5000"
$locationApiUrls = "http://localhost:5197"

Write-Host "Starting Location API silently..."
Start-Process -FilePath "dotnet" `
  -ArgumentList @(
    "run",
    "--project", "Microservices/Location/WebAPI/WebAPI.csproj",
    "--launch-profile", "http",
    "--urls", $locationApiUrls
  ) `
  -WindowStyle Hidden

Write-Host "Starting Main Application..."
$env:ASPNETCORE_URLS = $webUiUrls
dotnet watch --project WebUI/WebUI.csproj --launch-profile https --urls $webUiUrls