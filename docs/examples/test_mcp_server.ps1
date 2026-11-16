# Test script for Windows Toolbox MCP Server
# Run this in PowerShell to test the MCP server

$mcpExe = "WindowsToolbox.MCP.exe"

Write-Host "Testing Windows Toolbox MCP Server..." -ForegroundColor Cyan
Write-Host ""

# Test 1: Initialize
Write-Host "Test 1: Initialize" -ForegroundColor Yellow
$init = '{"jsonrpc":"2.0","method":"initialize","params":{},"id":1}'
$result = $init | & $mcpExe | Select-Object -Last 1
Write-Host "Response: $result" -ForegroundColor Green
Write-Host ""

# Test 2: List tools
Write-Host "Test 2: List Tools" -ForegroundColor Yellow
$list = '{"jsonrpc":"2.0","method":"tools/list","params":{},"id":2}'
$result = $list | & $mcpExe | Select-Object -Last 1
Write-Host "Response (truncated): $($result.Substring(0, [Math]::Min(200, $result.Length)))..." -ForegroundColor Green
Write-Host ""

# Test 3: Format JSON (simple test)
Write-Host "Test 3: Format JSON" -ForegroundColor Yellow
$call = '{"jsonrpc":"2.0","method":"tools/call","params":{"name":"format-json","arguments":{"json_string":"{\"test\":true,\"value\":123}"}},"id":3}'
$result = $call | & $mcpExe | Select-Object -Last 1
Write-Host "Response: $result" -ForegroundColor Green
Write-Host ""

# Test 4: Generate hash
Write-Host "Test 4: Generate Hash" -ForegroundColor Yellow
$call = '{"jsonrpc":"2.0","method":"tools/call","params":{"name":"generate-hash","arguments":{"text":"Hello World","algorithms":["sha256","md5"]}},"id":4}'
$result = $call | & $mcpExe | Select-Object -Last 1
Write-Host "Response: $result" -ForegroundColor Green
Write-Host ""

# Test 5: Encode Base64
Write-Host "Test 5: Encode Base64" -ForegroundColor Yellow
$call = '{"jsonrpc":"2.0","method":"tools/call","params":{"name":"encode-base64","arguments":{"text":"Hello, World!"}},"id":5}'
$result = $call | & $mcpExe | Select-Object -Last 1
Write-Host "Response: $result" -ForegroundColor Green
Write-Host ""

Write-Host "All tests completed!" -ForegroundColor Cyan
