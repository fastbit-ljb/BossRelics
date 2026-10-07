$ErrorActionPreference = 'Stop'

$project = Split-Path -Parent $PSScriptRoot
$mods = 'D:\L\Game\Steam\steamapps\common\Slay the Spire 2\mods'
$log = Join-Path $project 'install.log'

while (Get-Process -Name 'SlayTheSpire2' -ErrorAction SilentlyContinue) {
    Start-Sleep -Seconds 1
}

New-Item -ItemType Directory -Path $mods -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $project 'bin\Release\net9.0\BossRelics.dll') -Destination (Join-Path $mods 'BossRelics.dll') -Force
Copy-Item -LiteralPath (Join-Path $project 'build\BossRelics.pck') -Destination (Join-Path $mods 'BossRelics.pck') -Force
Copy-Item -LiteralPath (Join-Path $project 'BossRelics.json') -Destination (Join-Path $mods 'BossRelics.json') -Force

"$(Get-Date -Format o) Installed BossRelics after game exit." | Set-Content -LiteralPath $log -Encoding UTF8
