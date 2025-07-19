#!/bin/bash

DLL_PATH="/home/ubuntu/srv-02/bin/Release/net8.0/linux-x64/publish/S3FactsServer.dll"

cat > /etc/systemd/system/srv-02.service << EOL
[Unit]
Description=Dotnet S3 info service
After=network.target

[Service]
Type=simple
ExecStart=/usr/bin/dotnet $DLL_PATH
Restart=always
RestartSec=5
User=ubuntu
WorkingDirectory=/home/ubuntu/srv-02/bin/Release/net8.0/linux-x64/publish

[Install]
WantedBy=multi-user.target
EOL

systemctl daemon-reload
systemctl enable srv-02
systemctl start srv-02