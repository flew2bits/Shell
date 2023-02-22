$AppName = Split-Path -Path (Get-Location) -Leaf

Rename-Item Shell.sln "${AppName}.sln"
Rename-Item Shell\Shell.csproj "${AppName}.csproj"
Rename-Item Shell $AppName

Get-ChildItem -Recurse -Include "*.cs","*.sln","*.csproj","*.cshtml" | ForEach-Object { $name = $_.FullName; (Get-Content $name) | ForEach-Object { $_ -replace "Shell","${AppName}" } | Set-Content $name }