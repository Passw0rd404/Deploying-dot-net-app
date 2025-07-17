#!/bin/bash

# Create systemd service file with correct path
cat > /etc/systemd/system/srv-02.service << EOL
[Unit]
Description=Dotnet S3 info service
After=network.target

[Service]
Type=notify
ExecStart=/usr/bin/dotnet $DLL_PATH
Restart=always
RestartSec=5
SyslogIdentifier=srv-02
User=ubuntu
Environment=DOTNET_CLI_HOME=/tmp
Environment=ASPNETCORE_ENVIRONMENT=Production
WorkingDirectory=/home/ubuntu/srv-02/bin/Release/netcoreapp6/linux-x64/publish

[Install]
WantedBy=multi-user.target
EOL

# Reload systemd and start service
echo "Starting service..."
sudo systemctl daemon-reload