param(
    [Parameter(Mandatory)]
    [ValidateSet('Debug','Release')]
    [System.String]$Target,
    
    [Parameter(Mandatory)]
    [System.String]$TargetPath,
    
    [Parameter(Mandatory)]
    [System.String]$TargetAssembly,

    [Parameter(Mandatory)]
    [System.String]$ValheimPath,

    [Parameter(Mandatory)]
    [System.String]$ProjectPath,
    
    [System.String]$DeployPath
)

# Make sure Get-Location is the script path
Push-Location -Path (Split-Path -Parent $MyInvocation.MyCommand.Path)

# Test preliminaries
("$TargetPath",
 "$ValheimPath",
 "$(Get-Location)\..\libraries"
) | ForEach-Object {
    if (!(Test-Path "$_")) {
        Write-Error -ErrorAction Stop -Message "$_ folder is missing"
    }
}

# Plugin name without ".dll"
$name = "$TargetAssembly" -Replace('.dll')

# Important paths
$dataSourcePath = "$ProjectPath\data"

# Create the mdb file if pdb exists
$pdb = "$TargetPath\$name.pdb"

if (Test-Path -Path "$pdb") {
    Write-Host "Create mdb file for plugin $name"
    Invoke-Expression "& `"$(Get-Location)\..\libraries\Debug\pdb2mdb.exe`" `"$TargetPath\$TargetAssembly`""
}

Write-Host "Publishing for $Target from $TargetPath"

if ($Target.Equals("Debug")) {
    if ($DeployPath.Equals("")) {
        $DeployPath = "$ValheimPath\BepInEx\plugins"
    }
    
    # Final debug install folder:
    # Valheim/BepInEx/plugins/ValheimCompletionist/
    $plug = New-Item -Type Directory -Path "$DeployPath\$name" -Force

    Write-Host "Copy $TargetAssembly to $plug"
    Copy-Item -Path "$TargetPath\$name.dll" -Destination "$plug" -Force

    if (Test-Path "$TargetPath\$name.pdb") {
        Copy-Item -Path "$TargetPath\$name.pdb" -Destination "$plug" -Force
    }

    if (Test-Path "$TargetPath\$name.dll.mdb") {
        Copy-Item -Path "$TargetPath\$name.dll.mdb" -Destination "$plug" -Force
    }

    # Copy data folder
    if (Test-Path "$dataSourcePath") {
        Write-Host "Copy data folder to $plug\data"
        Copy-Item -Path "$dataSourcePath" -Destination "$plug\data" -Recurse -Force
    }
    else {
        Write-Warning "Data folder not found: $dataSourcePath"
    }
}

if ($Target.Equals("Release")) {
    Write-Host "Packaging for ThunderStore..."

    $Package = "Package"
    $PackagePath = "$ProjectPath\$Package"

    # Clean old package plugins folder first
    if (Test-Path "$PackagePath\plugins\$name") {
        Remove-Item "$PackagePath\plugins\$name" -Recurse -Force
    }

    # Thunderstore package structure:
    # Package/plugins/ValheimCompletionist/ValheimCompletionist.dll
    # Package/plugins/ValheimCompletionist/data/items.csv
    $pluginPackagePath = "$PackagePath\plugins\$name"

    New-Item -Type Directory -Path "$pluginPackagePath" -Force

    Write-Host "Copy $TargetAssembly to $pluginPackagePath"
    Copy-Item -Path "$TargetPath\$TargetAssembly" -Destination "$pluginPackagePath\$TargetAssembly" -Force

    # Copy data folder into release package
    if (Test-Path "$dataSourcePath") {
        Write-Host "Copy data folder to $pluginPackagePath\data"
        Copy-Item -Path "$dataSourcePath" -Destination "$pluginPackagePath\data" -Recurse -Force
    }
    else {
        Write-Warning "Data folder not found: $dataSourcePath"
    }

    # Copy README
    Copy-Item -Path "$ProjectPath\README.md" -Destination "$PackagePath\README.md" -Force

    # Remove old zip if it exists
    if (Test-Path "$TargetPath\$name.zip") {
        Remove-Item "$TargetPath\$name.zip" -Force
    }

    Compress-Archive -Path "$PackagePath\*" -DestinationPath "$TargetPath\$name.zip" -Force

    Write-Host "Created package: $TargetPath\$name.zip"
}

Pop-Location