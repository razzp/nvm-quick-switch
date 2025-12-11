function Exit-WithRollBack {
    param(
        [string]$Message = "Aborted."
    )

    Write-Host $Message -ForegroundColor Red

    # Roll back the VERSION file.
    Set-Content -Path $versionPath -Value $version.toString()

    exit
}

function Get-ISCCPath {
    # Sniffing the registry is the most reliable way of finding ISCC.exe,
    # since a PATH environment variable isn't created during install.
    $keys = @(
        'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Inno Setup 6_is1',
        'HKLM:\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Inno Setup 6_is1',
        'HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\Inno Setup 6_is1',
        'HKLM:\SOFTWARE\Inno Setup\Inno Setup 6',
        'HKLM:\SOFTWARE\WOW6432Node\Inno Setup\Inno Setup 6'
    )

    foreach ($key in $keys) {
        $item = Get-ItemProperty $key -ErrorAction Ignore

        if ($item) {
            $path = $null

            if ($item.InstallLocation) {
                $path = $item.InstallLocation
            }
            elseif ($item.AppPath) {
                $path = $item.AppPath
            }

            if ($path) {
                $executable = Join-Path $path 'ISCC.exe'

                if (Test-Path $executable) {
                    return $executable
                }
            }
        }
    }

    throw "ISCC.exe could not be located."
}

function Get-NewVersion {
    param(
        [Version]$CurrentVersion
    )

    $nextMajor = New-Object Version ($CurrentVersion.Major + 1), 0, 0
    $nextMinor = New-Object Version $CurrentVersion.Major, ($CurrentVersion.Minor + 1), 0
    $nextPatch = New-Object Version $CurrentVersion.Major, $CurrentVersion.Minor, ($CurrentVersion.Build + 1)

    $optionMajor = "Major: $nextMajor"
    $optionMinor = "Minor: $nextMinor"
    $optionPatch = "Patch: $nextPatch"
    $optionAbort = "Fuck this!"

    $options = @($optionMajor, $optionMinor, $optionPatch, $optionAbort)
    $choice = Show-Menu -Title "Choose a new version:" -Options $options

    if ($choice -eq $optionAbort) {
        throw "Aborted by user."
    }

    switch ($choice) {
        $optionMajor { return $nextMajor }
        $optionMinor { return $nextMinor }
        $optionPatch { return $nextPatch }
        default { throw "Unexpected choice: $choice" }
    }
}

function Get-ReleaseUrl {
    param(
        [string]$ReleaseTag,
        [string]$ReleaseTitle
    )

    $repoUrl = git config --get remote.origin.url

    if ($repoUrl -match "git@github\.com:(.+)/(.+)\.git") {
        $owner = $matches[1]
        $repoName = $matches[2]
    }
    elseif ($repoUrl -match "https://github\.com/(.+)/(.+)\.git") {
        $owner = $matches[1]
        $repoName = $matches[2]
    }
    else {
        throw "Could not parse repository URL ($repoUrl)."
    }

    $body = Get-Content (Join-Path $PSScriptRoot "\templates\release-notes.md") -Raw

    $body = $body -replace "{{version}}", $newVersion.toString()

    $encodedTag = [System.Net.WebUtility]::UrlEncode($ReleaseTag)
    $encodedTitle = [System.Net.WebUtility]::UrlEncode($ReleaseTitle)
    $encodedBody = [System.Net.WebUtility]::UrlEncode($body)

    return "https://github.com/$owner/$repoName/releases/new?tag=$encodedTag&title=$encodedTitle&body=$encodedBody"
}

function Invoke-GitTagAndPush {
    param(
        [string]$BranchName = "main",
        [string]$ReleaseTag,
        [bool]$CheckTags = $true,
        [bool]$CheckUncommitted = $true,
        [bool]$DryRun = $false
    )

    # Ensure we're in a git repo.
    $isGitRepo = git rev-parse --is-inside-work-tree 2>$null;
    
    if (-not $isGitRepo) {
        throw "This folder is not a git repository."
    }

    # Ensure we're on the correct branch.
    $currentBranch = git rev-parse --abbrev-ref HEAD

    if ($currentBranch -ne $BranchName) {
        throw "Current branch is '$currentBranch'. Please switch to '$branchName' first."
    }

    if ($CheckUncommitted) {
        # Ensure there aren't any uncommitted changes.
        $changes = git status --porcelain

        if ($changes) {
            if (-not (Show-YesNoQuestion -Question "There are uncommitted changes. Continue?")) {
                $shortHash = git rev-parse --short HEAD

                throw "Please commit first."
            }
        }
    }

    if ($CheckTags) {
        # Check if there are any existing tags on the current commit.
        $tags = git tag --points-at HEAD

        if ($tags) {
            Write-Warning "Current commit already has the following tag(s):"
            Write-Host $tags -Separator ", "

            if (-not (Show-YesNoQuestion -Question "Continue?")) {
                $shortHash = git rev-parse --short HEAD

                throw "Current commit ($shortHash) already has one or more tags."
            }
        }
    }

    if (-not $DryRun) {
        $createTagOutput = git tag $ReleaseTag 2>&1

        if ($LASTEXITCODE -ne 0) {
            throw "Failed to create tag: $createTagOutput"
        }

        $pushTagOutput = git push origin $ReleaseTag 2>&1

        if ($LASTEXITCODE -ne 0) {
            throw "Failed to push tag: $pushTagOutput"
        }
    }
}

function Show-Menu {
    param(
        [string]$Title,
        [string[]]$Options
    )

    $index = 0
    $cursorTop = [Console]::CursorTop

    while ($true) {
        [Console]::SetCursorPosition(0, $cursorTop)

        for ($row = $cursorTop; $row -lt [Console]::WindowHeight; $row++) {
            [Console]::SetCursorPosition(0, $row)
            [Console]::Write(" " * ([Console]::WindowWidth))
        }

        [Console]::SetCursorPosition(0, $cursorTop)

        Write-Host $Title
        Write-Host "Use $([char]0x2191)/$([char]0x2193) to move, Enter to select"
        Write-Host ""

        for ($i = 0; $i -lt $Options.Count; $i++) {
            if ($i -eq $index) {
                Write-Host "> $($Options[$i])" -ForegroundColor Cyan
            }
            else {
                Write-Host "  $($Options[$i])"
            }
        }

        $key = [System.Console]::ReadKey($true)

        switch ($key.Key) {
            "UpArrow" { if ($index -gt 0) { $index-- } }
            "DownArrow" { if ($index -lt $Options.Count - 1) { $index++ } }
            "Enter" { return $Options[$index] }
        }
    }
}

function Show-YesNoQuestion {
    param(
        [string]$Question,
        [bool]$DefaultYes = $true
    )

    $defaultPrompt = if ($DefaultYes) { "[Y/n]" } else { "[y/N]" }
    $response = Read-Host "$Question $defaultPrompt"

    if ([string]::IsNullOrWhiteSpace($response)) {
        return $DefaultYes
    }

    switch ($response.ToLower()) {
        'y' { return $true }
        'yes' { return $true }
        'n' { return $false }
        'no' { return $false }
        default { throw "Please answer yes or no." }
    }
}