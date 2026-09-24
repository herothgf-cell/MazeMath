param([string]$UnityPath = '')
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$version = '2022.3.62f2'
if (!$UnityPath) {
    $UnityPath = Join-Path ${env:ProgramFiles} "Unity\Hub\Editor\$version\Editor\Unity.exe"
}
if (!(Test-Path -LiteralPath $UnityPath -PathType Leaf)) {
    throw "Unity $version not found. Pass -UnityPath 'D:\...\Editor\Unity.exe', or use MazeMath > Build > Mobile Web (Experimental)."
}
$versionFile = Join-Path $root 'ProjectSettings/ProjectVersion.txt'
if (!(Test-Path $versionFile) -or !(Select-String -Path $versionFile -SimpleMatch "m_EditorVersion: $version" -Quiet)) {
    throw "Project is not pinned to Unity $version. Pull main and open the correct repository."
}
$logs = Join-Path $root 'TestResults'
New-Item -ItemType Directory -Force -Path $logs | Out-Null
$log = Join-Path $logs 'mobile-web-build.log'
Write-Host 'Close this project in Unity before running the batch build. No licence details are collected by this script.'
$argsList = @('-batchmode', '-quit', '-projectPath', ('"' + $root + '"'), '-buildTarget', 'WebGL',
    '-executeMethod', 'MazeMath.Editor.Build.BuildMobileWeb.PerformBuild', '-logFile', ('"' + $log + '"'))
$startedAt = [DateTime]::UtcNow
$process = Start-Process -FilePath $UnityPath -ArgumentList $argsList -PassThru -Wait
if ($process.ExitCode -ne 0) { throw "Unity build failed (exit $($process.ExitCode)). Log: $log" }
$output = Join-Path $root 'Build/MobileWeb'
foreach ($relative in @('index.html', 'mobile-web.js', 'mobile-web.css', 'build-info.json')) {
    if (!(Test-Path (Join-Path $output $relative) -PathType Leaf)) { throw "Missing output $relative. See $log" }
}
if ((Get-Item (Join-Path $output 'build-info.json')).LastWriteTimeUtc -lt $startedAt) { throw 'No fresh build marker. Old output is not considered a successful build.' }
if (!(Get-ChildItem (Join-Path $output 'Build') -Filter '*.wasm*' -File)) { throw "Missing WebAssembly output. See $log" }
Compress-Archive -Path (Join-Path $output '*') -DestinationPath ($output + '.zip') -Force
Write-Host "Output: $output"
Write-Host "Archive: $output.zip"
Write-Host 'LAN test: py Tools/serve_mobile_web.py'
Write-Host 'Build completion does not guarantee mobile Safari/Chrome compatibility; test on real devices.'
