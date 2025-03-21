# Requires -RunAsAdministrator

$ErrorActionPreference = "Stop"

Write-Host "====================================================="
Write-Host "Configuring PostgreSQL for TCP/IP Connections"
Write-Host "====================================================="
Write-Host ""

$PGHOME = "C:\Program Files\PostgreSQL\14"
$PGDATA = "$PGHOME\data"
$PGCONF = "$PGDATA\postgresql.conf"
$PGHBA = "$PGDATA\pg_hba.conf"

# 1. Stop PostgreSQL Service
Write-Host "1. Stopping PostgreSQL service..."
Stop-Service postgresql-x64-14 -Force
Start-Sleep -Seconds 5

# 2. Backup and Update postgresql.conf
Write-Host "`n2. Updating postgresql.conf..."
$timestamp = Get-Date -Format "yyyyMMddHHmmss"
Copy-Item $PGCONF "$PGCONF.backup.$timestamp"

$config = Get-Content $PGCONF
$config = $config -replace "^#?listen_addresses.*=.*$", "listen_addresses = '*'"
Set-Content $PGCONF $config

# 3. Backup and Update pg_hba.conf
Write-Host "`n3. Updating pg_hba.conf..."
Copy-Item $PGHBA "$PGHBA.backup.$timestamp"

$hbaConfig = @"
# TYPE  DATABASE        USER            ADDRESS                 METHOD
local   all            all                                     scram-sha-256
host    all            all             127.0.0.1/32            scram-sha-256
host    all            all             ::1/128                 scram-sha-256
host    all            all             0.0.0.0/0               scram-sha-256
"@

Set-Content $PGHBA $hbaConfig

# 4. Start PostgreSQL Service
Write-Host "`n4. Starting PostgreSQL service..."
Start-Service postgresql-x64-14
Start-Sleep -Seconds 5

# 5. Test Connection
Write-Host "`n5. Testing connection..."
$env:PGPASSWORD = "root@rsi"
& "$PGHOME\bin\psql.exe" -U postgres -c "\conninfo"

if ($LASTEXITCODE -eq 0) {
    Write-Host "`nPostgreSQL is now configured and accepting connections."
} else {
    Write-Host "`nFailed to connect to PostgreSQL."
    Write-Host "Please check the configuration and try again."
}

Write-Host "`nConfiguration completed. Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown") 