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

try {
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
    $name = "$TargetAssembly" -Replace('\.dll$', '')

    # Important source paths
    $dataSourcePath = Join-Path $ProjectPath "data"

    function Copy-CsvDataFiles {
        param(
            [Parameter(Mandatory)]
            [System.String]$SourceDataPath,

            [Parameter(Mandatory)]
            [System.String[]]$TargetDataPaths
        )

        if (!(Test-Path "$SourceDataPath")) {
            Write-Warning "Data folder not found: $SourceDataPath"
            return
        }

        $csvFiles = Get-ChildItem -Path "$SourceDataPath" -Filter "*.csv" -File

        if ($csvFiles.Count -eq 0) {
            Write-Warning "No CSV files found in data folder: $SourceDataPath"
            return
        }

        foreach ($targetDataPath in $TargetDataPaths) {
            New-Item -Type Directory -Path "$targetDataPath" -Force | Out-Null

            Write-Host "Copy CSV data files from $SourceDataPath to $targetDataPath"

            foreach ($csvFile in $csvFiles) {
                Copy-Item -Path $csvFile.FullName -Destination "$targetDataPath\$($csvFile.Name)" -Force
                Write-Host " - $($csvFile.Name)"
            }
        }
    }

    # Create the mdb file if pdb exists
    $pdb = Join-Path $TargetPath "$name.pdb"

    if (Test-Path -Path "$pdb") {
        Write-Host "Create mdb file for plugin $name"
        Invoke-Expression "& `"$(Get-Location)\..\libraries\Debug\pdb2mdb.exe`" `"$TargetPath\$TargetAssembly`""
    }

    Write-Host "Publishing for $Target from $TargetPath"

    if ($Target.Equals("Debug")) {
        if ([string]::IsNullOrWhiteSpace($DeployPath)) {
            $DeployPath = "$ValheimPath\BepInEx\plugins"
        }

        # Final debug plugin install folder:
        # Valheim/BepInEx/plugins/ValheimCompletionist/
        $pluginDeployPath = Join-Path $DeployPath $name

        New-Item -Type Directory -Path "$pluginDeployPath" -Force | Out-Null

        Write-Host "Copy $TargetAssembly to $pluginDeployPath"
        Copy-Item -Path "$TargetPath\$name.dll" -Destination "$pluginDeployPath" -Force

        if (Test-Path "$TargetPath\$name.pdb") {
            Copy-Item -Path "$TargetPath\$name.pdb" -Destination "$pluginDeployPath" -Force
        }

        if (Test-Path "$TargetPath\$name.dll.mdb") {
            Copy-Item -Path "$TargetPath\$name.dll.mdb" -Destination "$pluginDeployPath" -Force
        }

        # Write CSV data to BOTH locations during debug:
        #
        # 1. Plugin defaults:
        #    Valheim/BepInEx/plugins/ValheimCompletionist/data/
        #
        # 2. Runtime/user-editable config copy:
        #    Valheim/BepInEx/config/ValheimCompletionist/data/
        $pluginDataTargetPath = Join-Path $pluginDeployPath "data"
        $configDataTargetPath = Join-Path (Join-Path (Join-Path $ValheimPath "BepInEx") "config") "$name\data"

        Copy-CsvDataFiles `
            -SourceDataPath "$dataSourcePath" `
            -TargetDataPaths @(
                "$pluginDataTargetPath",
                "$configDataTargetPath"
            )
    }

    if ($Target.Equals("Release")) {
        Write-Host "Packaging for Thunderstore..."

        $Package = "Package"
        $PackagePath = Join-Path $ProjectPath $Package

        # Clean old package plugin folder
        if (Test-Path "$PackagePath\plugins\$name") {
            Remove-Item "$PackagePath\plugins\$name" -Recurse -Force
        }

        # Clean old package config folder
        if (Test-Path "$PackagePath\config\$name") {
            Remove-Item "$PackagePath\config\$name" -Recurse -Force
        }

        # Thunderstore plugin structure:
        # Package/plugins/ValheimCompletionist/ValheimCompletionist.dll
        $pluginPackagePath = "$PackagePath\plugins\$name"

        New-Item -Type Directory -Path "$pluginPackagePath" -Force | Out-Null

        Write-Host "Copy $TargetAssembly to $pluginPackagePath"
        Copy-Item -Path "$TargetPath\$TargetAssembly" -Destination "$pluginPackagePath\$TargetAssembly" -Force

        # Write CSV data to BOTH package locations:
        #
        # 1. Plugin defaults:
        #    Package/plugins/ValheimCompletionist/data/
        #
        # 2. Config copy:
        #    Package/config/ValheimCompletionist/data/
        #
        # Note:
        # Shipping plugin/data is the important one for defaults.
        # Shipping config/data is optional, but included because you requested both.
        $pluginDataPackagePath = "$pluginPackagePath\data"
        $configDataPackagePath = "$PackagePath\config\$name\data"

        Copy-CsvDataFiles `
            -SourceDataPath "$dataSourcePath" `
            -TargetDataPaths @(
                "$pluginDataPackagePath",
                "$configDataPackagePath"
            )

        # Copy README if it exists
        if (Test-Path "$ProjectPath\README.md") {
            Copy-Item -Path "$ProjectPath\README.md" -Destination "$PackagePath\README.md" -Force
        }
        else {
            Write-Warning "README.md not found: $ProjectPath\README.md"
        }

        # Remove old zip if it exists
        if (Test-Path "$TargetPath\$name.zip") {
            Remove-Item "$TargetPath\$name.zip" -Force
        }

        Compress-Archive -Path "$PackagePath\*" -DestinationPath "$TargetPath\$name.zip" -Force

        Write-Host "Created package: $TargetPath\$name.zip"
    }
}
finally {
    Pop-Location
}
