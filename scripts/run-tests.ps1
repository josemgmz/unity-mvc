param(
    [ValidateSet("editmode", "playmode", "all")]
    [string]$Platform = "all",
    [string]$UnityPath,
    [string]$UnityVersion,
    [bool]$KillStaleUnityProcesses = $true,
    [string]$ProjectPath = (Join-Path (Join-Path $PSScriptRoot "..") "ValidationProject~\RunnerProject"),
    [string]$ResultsDir
)

$ErrorActionPreference = "Stop"

function Stop-StaleUnityProcesses {
    param([bool]$Enabled)

    if (-not $Enabled) {
        return
    }

    $targetProcessNames = @(
        "Unity",
        "UnityPackageManager",
        "Unity.Licensing.Client",
        "UnityCrashHandler64",
        "UnityShaderCompiler"
    )

    $running = Get-Process -ErrorAction SilentlyContinue |
        Where-Object { $targetProcessNames -contains $_.ProcessName }

    if (-not $running) {
        return
    }

    Write-Host "Stopping stale Unity processes before test execution..."
    $running | Select-Object ProcessName, Id | ForEach-Object {
        Write-Host " - $($_.ProcessName) [$($_.Id)]"
    }

    $running | Stop-Process -Force -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 2
}

function Resolve-UnityPath {
    param([string]$ConfiguredPath)

    if ($ConfiguredPath) {
        if (-not (Test-Path $ConfiguredPath)) {
            throw "Unity executable not found at '$ConfiguredPath'."
        }

        return $ConfiguredPath
    }

    if ($env:UNITY_PATH -and (Test-Path $env:UNITY_PATH)) {
        return $env:UNITY_PATH
    }

    $hubEditorsPath = "C:\Program Files\Unity\Hub\Editor"
    if (-not (Test-Path $hubEditorsPath)) {
        throw "Unity Hub editor path was not found and UNITY_PATH is not set."
    }

    $editor = Get-ChildItem $hubEditorsPath -Directory |
        Sort-Object Name -Descending |
        Select-Object -First 1

    if (-not $editor) {
        throw "No Unity editors were found under '$hubEditorsPath'."
    }

    $candidate = Join-Path $editor.FullName "Editor\Unity.exe"
    if (-not (Test-Path $candidate)) {
        throw "Unity executable not found under '$($editor.FullName)'."
    }

    return $candidate
}

function Resolve-UnityVersion {
    param(
        [string]$ConfiguredVersion,
        [string]$ResolvedUnityPath
    )

    if ($ConfiguredVersion) {
        return $ConfiguredVersion
    }

    $unityExecutable = Get-Item $ResolvedUnityPath
    $editorDirectory = $unityExecutable.Directory
    $versionDirectory = $editorDirectory.Parent

    if ($null -eq $versionDirectory -or [string]::IsNullOrWhiteSpace($versionDirectory.Name)) {
        throw "Unable to infer Unity version from '$ResolvedUnityPath'. Use -UnityVersion."
    }

    return $versionDirectory.Name
}

function Ensure-ValidationProject {
    param(
        [string]$ResolvedProjectPath,
        [string]$ResolvedPackagePath,
        [string]$ResolvedUnityVersion
    )

    New-Item -ItemType Directory -Force -Path $ResolvedProjectPath | Out-Null
    New-Item -ItemType Directory -Force -Path (Join-Path $ResolvedProjectPath "Assets") | Out-Null
    New-Item -ItemType Directory -Force -Path (Join-Path $ResolvedProjectPath "Packages") | Out-Null
    New-Item -ItemType Directory -Force -Path (Join-Path $ResolvedProjectPath "ProjectSettings") | Out-Null

    $packagePathForManifest = $ResolvedPackagePath.Replace("\", "/")
    $manifest = @"
{
  "dependencies": {
    "com.unity.test-framework": "1.5.1",
    "com.unity.ugui": "2.0.0",
    "com.unity.modules.physics": "1.0.0",
    "com.unity.modules.physics2d": "1.0.0",
    "com.unitymvc.core": "file:$packagePathForManifest"
  },
  "testables": [
    "com.unitymvc.core"
  ]
}
"@

    $projectVersion = @"
m_EditorVersion: $ResolvedUnityVersion
m_EditorVersionWithRevision: $ResolvedUnityVersion
"@

    Set-Content (Join-Path $ResolvedProjectPath "Packages\manifest.json") $manifest
    Set-Content (Join-Path $ResolvedProjectPath "ProjectSettings\ProjectVersion.txt") $projectVersion
}

function Test-IsNestedPath {
    param(
        [string]$ParentPath,
        [string]$CandidatePath
    )

    $resolvedParentPath = [System.IO.Path]::GetFullPath($ParentPath).TrimEnd('\', '/')
    $resolvedCandidatePath = [System.IO.Path]::GetFullPath($CandidatePath).TrimEnd('\', '/')

    return $resolvedCandidatePath.StartsWith("$resolvedParentPath\", [System.StringComparison]::OrdinalIgnoreCase) `
        -or $resolvedCandidatePath.Equals($resolvedParentPath, [System.StringComparison]::OrdinalIgnoreCase)
}

function Resolve-ResultsDirectory {
    param(
        [string]$ConfiguredResultsDir,
        [string]$ResolvedPackagePath,
        [string]$ResolvedProjectPath
    )

    $defaultResultsDir = Join-Path $ResolvedPackagePath ("TestResults~\" + (Get-Date -Format "yyyyMMdd-HHmmss"))
    $requestedResultsDir = if ([string]::IsNullOrWhiteSpace($ConfiguredResultsDir)) {
        $defaultResultsDir
    } else {
        [System.IO.Path]::GetFullPath($ConfiguredResultsDir)
    }

    if (Test-IsNestedPath -ParentPath $ResolvedProjectPath -CandidatePath $requestedResultsDir) {
        Write-Warning "Results directory '$requestedResultsDir' is inside the validation project. Redirecting to '$defaultResultsDir' to avoid Unity import loops."
        return $defaultResultsDir
    }

    if (Test-IsNestedPath -ParentPath $ResolvedPackagePath -CandidatePath $requestedResultsDir) {
        $resolvedPackageRoot = [System.IO.Path]::GetFullPath($ResolvedPackagePath).TrimEnd('\', '/')
        $resolvedResultsPath = [System.IO.Path]::GetFullPath($requestedResultsDir).TrimEnd('\', '/')
        $relativePath = $resolvedResultsPath.Substring($resolvedPackageRoot.Length).TrimStart('\', '/')
        $topLevelSegment = if ([string]::IsNullOrWhiteSpace($relativePath)) {
            ""
        } else {
            $relativePath -split '[\\/]'
        }
        if ($topLevelSegment -is [System.Array]) {
            $topLevelSegment = $topLevelSegment[0]
        }

        if ([string]::IsNullOrWhiteSpace($topLevelSegment) -or -not $topLevelSegment.EndsWith("~")) {
            Write-Warning "Results directory '$requestedResultsDir' is inside the package without a '~' top-level folder. Redirecting to '$defaultResultsDir' to avoid package import loops."
            return $defaultResultsDir
        }
    }

    return $requestedResultsDir
}

function Invoke-UnityTests {
    param(
        [string]$ResolvedUnityPath,
        [string]$ResolvedProjectPath,
        [string]$ResolvedResultsDir,
        [string]$TestPlatform
    )

    $resultsPath = Join-Path $ResolvedResultsDir "$TestPlatform-results.xml"
    $logPath = Join-Path $ResolvedResultsDir "$TestPlatform.log"
    $unityArguments = @(
        "-batchmode",
        "-projectPath", $ResolvedProjectPath,
        "-runTests",
        "-testPlatform", $TestPlatform,
        "-testResults", $resultsPath,
        "-logFile", $logPath
    )

    Write-Host "Running $TestPlatform tests..."
    $unityProcess = Start-Process `
        -FilePath $ResolvedUnityPath `
        -ArgumentList $unityArguments `
        -NoNewWindow `
        -Wait `
        -PassThru

    $unityExitCode = if ($null -ne $unityProcess) { $unityProcess.ExitCode } else { -1 }
    if ($unityExitCode -ne 0) {
        throw "Unity exited with code $unityExitCode while running $TestPlatform tests. See '$logPath'."
    }

    if (-not (Test-Path $resultsPath)) {
        throw "Unity finished without producing '$resultsPath'. See '$logPath'."
    }

    Write-Host "Completed $TestPlatform tests. Results: $resultsPath"
}

$resolvedUnityPath = Resolve-UnityPath -ConfiguredPath $UnityPath
$resolvedUnityVersion = Resolve-UnityVersion -ConfiguredVersion $UnityVersion -ResolvedUnityPath $resolvedUnityPath
$resolvedPackagePath = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot ".."))
$resolvedProjectPath = [System.IO.Path]::GetFullPath($ProjectPath)
Stop-StaleUnityProcesses -Enabled $KillStaleUnityProcesses
$resolvedResultsDir = Resolve-ResultsDirectory `
    -ConfiguredResultsDir $ResultsDir `
    -ResolvedPackagePath $resolvedPackagePath `
    -ResolvedProjectPath $resolvedProjectPath

Write-Host "Unity executable: $resolvedUnityPath"
Write-Host "Unity version: $resolvedUnityVersion"
Write-Host "Validation project: $resolvedProjectPath"
Write-Host "Results directory: $resolvedResultsDir"

New-Item -ItemType Directory -Force -Path $resolvedResultsDir | Out-Null
Ensure-ValidationProject `
    -ResolvedProjectPath $resolvedProjectPath `
    -ResolvedPackagePath $resolvedPackagePath `
    -ResolvedUnityVersion $resolvedUnityVersion

$platforms = if ($Platform -eq "all") { @("editmode", "playmode") } else { @($Platform) }

foreach ($testPlatform in $platforms) {
    Invoke-UnityTests `
        -ResolvedUnityPath $resolvedUnityPath `
        -ResolvedProjectPath $resolvedProjectPath `
        -ResolvedResultsDir $resolvedResultsDir `
        -TestPlatform $testPlatform
}

Write-Host "All requested test runs completed successfully."
