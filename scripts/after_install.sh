#!/bin/bash
echo "Installing dotnet service"

# Create systemd service
sudo cat /etc/systemd/system/srv-02.service <<EOL
[Unit]
Description=Dotnet S3 info service
After=network.target

[Service]
Type=simple
ExecStart=/usr/bin/dotnet /srv-02/bin/Release/net8.0/linux-x64/publish/srv02.dll
Restart=always
RestartSec=5
User=ubuntu
Environment=ASPNETCORE_ENVIRONMENT=Production
WorkingDirectory=/srv-02/bin/Release/net8.0/linux-x64/publish

[Install]
WantedBy=multi-user.target
EOL

systemctl daemon-reload
