$port = 5186

# Kill any process on port 5186
$conn = Get-NetTCPConnection -LocalPort $port -ErrorAction SilentlyContinue | Select-Object -First 1
if ($conn) {
    Stop-Process -Id $conn.OwningProcess -Force
    Write-Host "Killed process on port $port (PID $($conn.OwningProcess))"
}

# Kill stale EventPulse.API processes
Get-Process -Name "EventPulse.API" -ErrorAction SilentlyContinue | ForEach-Object {
    Stop-Process -Id $_.Id -Force
    Write-Host "Killed stale EventPulse.API process (PID $($_.Id))"
}

dotnet run
