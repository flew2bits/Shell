$AppName = Split-Path -Path (Get-Location) -Leaf
$LowerAppName = $AppName.ToLower().Trim().Replace(" ", "");

docker build -f $AppName\Dockerfile -t flew2bits/${LowerAppName}:latest .

mkdir c:\temp\$LowerAppName\postgres -f

dotnet dev-certs https -ep c:\temp\$LowerAppName\localhost.pfx -p SECURE

docker compose -f $AppName\docker-compose.yml up -d
