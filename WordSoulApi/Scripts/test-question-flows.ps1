$ErrorActionPreference = 'Stop'

$apiRoot = Split-Path -Parent $PSScriptRoot
$repositoryRoot = Split-Path -Parent $apiRoot

Push-Location $apiRoot
try {
    dotnet build WordSoulApi.sln --no-restore
    if ($LASTEXITCODE -ne 0) { throw 'Backend build failed.' }

    dotnet test WordSoul.Tests\WordSoul.Tests.csproj `
        --no-build `
        --no-restore `
        --filter 'Suite=MandatoryQuestionFlow'
    if ($LASTEXITCODE -ne 0) { throw 'Mandatory unit tests failed.' }

    dotnet test WordSoul.IntegrationTests\WordSoul.IntegrationTests.csproj `
        --no-build `
        --no-restore `
        --filter 'Suite=MandatoryQuestionFlow'
    if ($LASTEXITCODE -ne 0) { throw 'Mandatory integration tests failed.' }
}
finally {
    Pop-Location
}

Push-Location (Join-Path $repositoryRoot 'wordsoul-app')
try {
    $webTypeCheck = Start-Process `
        -FilePath 'node.exe' `
        -ArgumentList @('node_modules/typescript/bin/tsc', '-b') `
        -NoNewWindow `
        -Wait `
        -PassThru
    if ($webTypeCheck.ExitCode -ne 0) { throw 'Web type-check failed.' }

    $webBuild = Start-Process `
        -FilePath 'node.exe' `
        -ArgumentList @('node_modules/vite/bin/vite.js', 'build') `
        -NoNewWindow `
        -Wait `
        -PassThru
    if ($webBuild.ExitCode -ne 0) { throw 'Web build failed.' }

    $webTests = Start-Process `
        -FilePath 'node.exe' `
        -ArgumentList @('node_modules/vitest/vitest.mjs', 'run') `
        -NoNewWindow `
        -Wait `
        -PassThru
    if ($webTests.ExitCode -ne 0) { throw 'Web tests failed.' }
}
finally {
    Pop-Location
}

Push-Location (Join-Path $repositoryRoot 'wordsoul-mobile')
try {
    $mobileTypeCheck = Start-Process `
        -FilePath 'node.exe' `
        -ArgumentList @('node_modules/typescript/bin/tsc', '--noEmit') `
        -NoNewWindow `
        -Wait `
        -PassThru
    if ($mobileTypeCheck.ExitCode -ne 0) {
        throw 'Mobile type-check failed.'
    }
}
finally {
    Pop-Location
}
