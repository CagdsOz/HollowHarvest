param([string]$UnityData = 'C:/Program Files/Unity/Hub/Editor/6000.5.5f1/Editor/Data')
$ErrorActionPreference = 'Stop'
Push-Location (Split-Path $PSScriptRoot -Parent)
try {
    $output = 'Temp/FarmValidation'
    New-Item -ItemType Directory -Force $output | Out-Null
    $dotnet = Join-Path $UnityData 'NetCoreRuntime/dotnet.exe'
    $compiler = Get-ChildItem "$UnityData/DotNetSdk/sdk/*/Roslyn/bincore/csc.dll" | Select-Object -First 1
    if (!$compiler) { throw 'Unity bundled C# compiler not found.' }
    $framework = @(Get-ChildItem "$UnityData/NetStandard/ref/2.1.0" -Filter *.dll)
    $engine = @(Get-ChildItem "$UnityData/Managed/UnityEngine" -Filter *.dll)
    $packages = @('Unity.InputSystem.dll', 'Unity.RenderPipelines.Universal.Runtime.dll',
        'Unity.RenderPipelines.Universal.2D.Runtime.dll', 'Unity.RenderPipelines.Core.Runtime.dll', 'UnityEngine.UI.dll')
    $packageFiles = @(foreach ($name in $packages) {
        Get-Item -LiteralPath "Library/ScriptAssemblies/$name"
    })
    function Compile([string]$name, [string[]]$arguments) {
        $response = "$output/$name.rsp"
        $arguments | Set-Content -Encoding utf8 $response
        & $dotnet $compiler.FullName "@$response"
        if ($LASTEXITCODE -ne 0) { throw "$name compilation failed." }
    }
    function References($files) {
        $files | ForEach-Object { '-r:"' + $_.FullName + '"' }
    }
    $runtime = @('-nologo', '-target:library', '-langversion:9.0', '-nowarn:0649', "-out:$output/HollowHarvest.Runtime.dll")
    $runtime += References ($framework + @($engine | Where-Object Name -NotLike 'UnityEditor*') + $packageFiles)
    $runtime += Get-ChildItem Assets/Scripts -Filter *.cs | ForEach-Object { '"' + $_.FullName + '"' }
    Compile 'runtime' $runtime
    $editor = @('-nologo', '-target:library', '-langversion:9.0', '-nowarn:0649', "-out:$output/HollowHarvest.Editor.dll", "-r:$output/HollowHarvest.Runtime.dll")
    $editor += References ($framework + $engine + $packageFiles)
    $editor += Get-ChildItem Assets/Editor -Filter *.cs | ForEach-Object { '"' + $_.FullName + '"' }
    Compile 'editor' $editor
    $checks = @('-nologo', '-target:exe', "-out:$output/FarmLogicChecks.dll", "-r:$output/HollowHarvest.Runtime.dll")
    $checks += References $framework
    foreach ($name in @('UnityEngine.CoreModule.dll', 'UnityEngine.Physics2DModule.dll')) {
        $file = Get-Item -LiteralPath "$UnityData/Managed/UnityEngine/$name"
        $checks += References @($file)
        Copy-Item -LiteralPath $file.FullName -Destination $output -Force
    }
    $checks += 'Tools/Tests/FarmLogicChecks.cs'
    Compile 'logic' $checks
    '{"runtimeOptions":{"tfm":"net8.0","framework":{"name":"Microsoft.NETCore.App","version":"8.0.0"}}}' |
        Set-Content "$output/FarmLogicChecks.runtimeconfig.json"
    & $dotnet "$output/FarmLogicChecks.dll"
    if ($LASTEXITCODE -ne 0) { throw 'Farm logic checks failed.' }
    Write-Output 'PASS: runtime and Editor compilation. Inspector-assigned field warnings suppressed.'
} finally {
    Pop-Location
}
