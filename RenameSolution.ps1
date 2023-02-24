$AppName = Split-Path -Path (Get-Location) -Leaf
$AppNameSquished = $AppName.Trim().Replace(" ", "").Replace("-", "_")
$LowerAppName = $AppNameSquished.ToLower()

Rename-Item Shell.sln "${AppName}.sln"
Rename-Item Shell\Shell.csproj "${AppName}.csproj"
Rename-Item Shell $AppName

Get-ChildItem -Recurse -Include "*.sln","*.csproj","Dockerfile" | ForEach-Object { $name = $_.FullName; (Get-Content $name) | ForEach-Object { $_ -replace "Shell","${AppName}" } | Set-Content $name }
Get-ChildItem -Recurse -Include "*.cs","*.cshtml" | ForEach-Object { $name = $_.FullName; (Get-Content $name) | ForEach-Object { $_ -replace "Shell","${AppNameSquished}" } | Set-Content $name }
Rename-Item $AppName\docker-compose.yml docker-compose.yml.bak
Get-Content ${AppName}\docker-compose.yml.bak | ForEach-Object { $_ -replace "shell",$LowerAppName} | Set-Content $AppName\docker-compose.yml
Remove-Item $AppName\docker-compose.yml.bak
