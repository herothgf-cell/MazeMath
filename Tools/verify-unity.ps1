param(
  [string]$Unity = "C:\Program Files\Unity\Hub\Editor\2022.3.62f2\Editor\Unity.exe",
  [switch]$BuildWeb,
  [switch]$BuildAndroid
)
$ErrorActionPreference = "Stop"
$Project = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
if (!(Test-Path $Unity)) { throw "Unity 2022.3.62f2 executable not found: $Unity" }
$Results = Join-Path $Project "TestResults"
New-Item -ItemType Directory -Force $Results | Out-Null
function Invoke-Unity([string[]]$Arguments) {
  $Quoted = ($Arguments | ForEach-Object { '"' + $_.Replace('"','\"') + '"' }) -join ' '
  $Process = Start-Process -FilePath $Unity -ArgumentList $Quoted -PassThru -Wait
  if ($Process.ExitCode -ne 0) { throw "Unity exited with $($Process.ExitCode). See TestResults logs." }
}
foreach ($Suite in @("EditMode", "PlayMode")) {
  $XmlPath = Join-Path $Results "$Suite.xml"
  if (Test-Path $XmlPath) { Remove-Item $XmlPath }
  # Do not use -quit with -runTests: the test runner must finish before exiting.
  Invoke-Unity @("-batchmode","-nographics","-projectPath",$Project,"-runTests","-testPlatform",$Suite,"-testResults",$XmlPath,"-logFile",(Join-Path $Results "$Suite.log"))
  if (!(Test-Path $XmlPath)) { throw "$Suite did not produce a test result." }
  [xml]$Report = Get-Content -Raw $XmlPath
  $Run = $Report.'test-run'
  if ([int]$Run.total -le 0 -or [int]$Run.failed -gt 0 -or $Run.result -ne 'Passed') { throw "$Suite did not pass: $($Run.result), failed=$($Run.failed)" }
  Write-Host "$Suite passed: $($Run.passed)/$($Run.total)"
}
if ($BuildWeb) {
  Invoke-Unity @("-batchmode","-nographics","-quit","-projectPath",$Project,"-buildTarget","WebGL","-executeMethod","MazeMath.Editor.AdventureProjectTools.Web","-logFile",(Join-Path $Results "WebGL.log"))
  if (!(Test-Path (Join-Path $Project "Build\Web\index.html"))) { throw "WebGL output is missing." }
}
if ($BuildAndroid) {
  Invoke-Unity @("-batchmode","-nographics","-quit","-projectPath",$Project,"-buildTarget","Android","-executeMethod","MazeMath.Editor.AdventureProjectTools.Android","-logFile",(Join-Path $Results "Android.log"))
  if (!(Test-Path (Join-Path $Project "Build\Android\MazeMath.apk"))) { throw "Android output is missing." }
}
Write-Host "Requested verification finished. Build success is not a device playtest."
