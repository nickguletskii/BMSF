$targetNugetExe = ".nuget\nuget.exe"
if(!(Test-Path $targetNugetExe)){
    $sourceNugetExe = "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe"
    Invoke-WebRequest $sourceNugetExe -OutFile $targetNugetExe
}
$buildParameters = "/property:Configuration=Release;BuildInParallel=true;WarningLevel=0 /maxcpucount"
$buildProcess = Invoke-MsBuild -Path BMSF.sln -MsBuildParameters $buildParameters -Use32BitMsBuild -PassThru -ShowBuildWindow

$maximumRuntimeSeconds = 30
try
{
    $buildProcess | Wait-Process -Timeout $maximumRuntimeSeconds -ErrorAction Stop
    Write-Host "Process successfully completed within timeout."
}
catch
{
    Write-Warning -Message 'Process exceeded timeout, will be killed now.'
    $buildProcess | Stop-Process -Force
}
$BuildLogDirectoryPath = [System.IO.Path]::GetFullPath($env:Temp)
$buildLogFilePath = (Join-Path -Path $BuildLogDirectoryPath -ChildPath "BMSF.sln") + ".msbuild.log"
$buildSucceeded = (((Select-String -Path $buildLogFilePath -Pattern "Build FAILED." -SimpleMatch) -eq $null) -and $buildProcess.ExitCode -eq 0)

if ($buildSucceeded -ne $true)
{
    Write-Error "Build failed.";
    exit 1;
}
mkdir -Force pkg
Push-Location
try {
    Set-Location pkg
    ..\.nuget\nuget pack -Symbols ..\BMSF.Utilities\BMSF.Utilities.csproj -IncludeReferencedProjects -Prop Configuration=Release
    ..\.nuget\nuget pack -Symbols ..\BMSF.Reactive.Utilities\BMSF.Reactive.Utilities.csproj -IncludeReferencedProjects -Prop Configuration=Release
    ..\.nuget\nuget pack -Symbols ..\BMSF.Reactive.Validation\BMSF.Reactive.Validation.csproj -IncludeReferencedProjects -Prop Configuration=Release
    ..\.nuget\nuget pack -Symbols ..\BMSF.WPF.Utilities\BMSF.WPF.Utilities.csproj -IncludeReferencedProjects -Prop Configuration=Release
    ..\.nuget\nuget pack -Symbols ..\BMSF.WPF.AutoCompleteControls\BMSF.WPF.AutoCompleteControls.csproj -IncludeReferencedProjects -Prop Configuration=Release
} finally {
    Pop-Location
}