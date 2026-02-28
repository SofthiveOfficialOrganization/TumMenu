# Start-All.ps1
Write-Host "Starting Location API silently..."
# Starts the Location API without showing a terminal window
Start-Process dotnet "run --project Microservices/Location/WebAPI/WebAPI.csproj --launch-profile http" -WindowStyle Hidden

Write-Host "Starting Main Application..."
# Starts the main app and keeps the terminal open to see logs
dotnet watch --project WebUI/WebUI.csproj
