# test_server.ps1
# Script to verify the Aventura del Tesoro Perdido Prolog Server
# Requires the server to be running on localhost:5000

$BaseUrl = "http://localhost:5000"

function Test-Endpoint {
    param (
        [string]$Method,
        [string]$Uri,
        [hashtable]$Body = $null
    )
    
    $Url = "$BaseUrl$Uri"
    Write-Host "Testing $Method $Url..." -NoNewline
    
    try {
        if ($Method -eq "GET") {
            $Response = Invoke-RestMethod -Uri $Url -Method Get -ErrorAction Stop
        }
        elseif ($Method -eq "POST") {
            $JsonBody = $Body | ConvertTo-Json
            $Response = Invoke-RestMethod -Uri $Url -Method Post -Body $JsonBody -ContentType "application/json" -ErrorAction Stop
        }
        
        Write-Host " [OK]" -ForegroundColor Green
        # Write-Host ($Response | ConvertTo-Json -Depth 2)
        return $Response
    }
    catch {
        Write-Host " [FAILED]" -ForegroundColor Red
        Write-Host "Error: $($_.Exception.Message)"
        return $null
    }
}

Write-Host "=== Starting Server Verification ===" -ForegroundColor Cyan

# 1. Test Status (Initial)
$State = Test-Endpoint -Method "GET" -Uri "/estado"
if ($State) {
    Write-Host "Current Location: $($State.ubicacion)"
}

# 2. Test Restart (to ensure clean state)
Test-Endpoint -Method "GET" -Uri "/reiniciar"

# 3. Test Movement (should work if connected)
# Assuming 'templo' or 'puente' is connected to 'bosque' (initial). 
# Need to check paths first.
$Paths = Test-Endpoint -Method "GET" -Uri "/caminos"
if ($Paths.caminos) {
    Write-Host "Available paths: $($Paths.caminos -join ', ')"
    $FirstPath = $Paths.caminos[0]
    if ($FirstPath) {
        Test-Endpoint -Method "POST" -Uri "/mover" -Body @{ destino = $FirstPath }
    }
}

# 4. Test Objects in current location
Test-Endpoint -Method "GET" -Uri "/objetos_lugar"

# 5. Test Win Verification
Test-Endpoint -Method "GET" -Uri "/verifica_gane"

Write-Host "=== Verification Complete ===" -ForegroundColor Cyan
Write-Host "Note: If connection failed, ensure the Prolog server is running on port 5000." -ForegroundColor Yellow
