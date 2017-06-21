$targetNugetExe = ".nuget\nuget.exe"
if(!(Test-Path $targetNugetExe)){
    $sourceNugetExe = "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe"
    Invoke-WebRequest $sourceNugetExe -OutFile $targetNugetExe
}
Set-Alias nuget $targetNugetExe -Verbose
nuget pack BMSF.Utilities\BMSF.Utilities.csproj -IncludeReferencedProjects
nuget pack BMSF.Reactive.Utilities\BMSF.Reactive.Utilities.csproj -IncludeReferencedProjects
nuget pack BMSF.Reactive.Validation\BMSF.Reactive.Validation.csproj -IncludeReferencedProjects
nuget pack BMSF.WPF.Utilities\BMSF.WPF.Utilities.csproj -IncludeReferencedProjects
nuget pack BMSF.WPF.AutoCompleteControls\BMSF.WPF.AutoCompleteControls.csproj -IncludeReferencedProjects