$body = Get-Content 'test_user.json' -Raw
try {
    $response = Invoke-WebRequest -Uri 'http://localhost:5005/api/Usuarios' -Method POST -ContentType 'application/json' -Body $body
    Write-Host "Status: $($response.StatusCode)"
    Write-Host "Body: $($response.Content)"
}
catch {
    Write-Host "Error Status: $($_.Exception.Response.StatusCode.value__)"
    $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
    $responseBody = $reader.ReadToEnd()
    Write-Host "Full Response:"
    Write-Host $responseBody
}
