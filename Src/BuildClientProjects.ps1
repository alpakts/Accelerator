$Global:ErrorActionPreference = "Stop"

Write-Host "Ensure 'NPM' is installed"
$npmCommand = (get-command npm.cmd -ErrorAction SilentlyContinue).Source;
if ($null -eq $npmCommand) {
    throw "NPM need to be installed and in path"
}

Write-Host "Ensure 'yarn' is installed"
$yarnCommand = (get-command yarn.cmd -ErrorAction SilentlyContinue).Source
if ($null -eq $yarnCommand) {
    Write-host "Installing yarn"
    &$npmCommand install -g yarn
    $yarnCommand = (get-command yarn.cmd).Source
}

Write-Host "building extensions module..."
Push-Location "$Script:PSScriptRoot"
&$yarnCommand install
&$yarnCommand run prod
Pop-Location
if ($LASTEXITCODE -ne 0){
    Write-Error "Build error";
    return;
}
Write-Host "building email project..."
Push-Location "$Script:PSScriptRoot\Litium.Accelerator.Email"
&$yarnCommand install
&$yarnCommand run prod
Pop-Location


Write-Host "building mvc project..."
Push-Location "$Script:PSScriptRoot\Litium.Accelerator.Mvc"
&$yarnCommand install --check-files
&$yarnCommand run prod
Pop-Location
if ($LASTEXITCODE -ne 0){
    Write-Error "Build error";
    return;
}