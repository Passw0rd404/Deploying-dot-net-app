# srv-02: One-Click .NET Deployment

🚀 **Deploy .NET 6.0 service to AWS EC2 in under 2 minutes**

## Usage

1. **AWS Console** → **EC2** → **Launch Templates** → **Create**
2. **AMI**: Ubuntu 20.04 or 22.04 LTS
3. **User Data**: Copy script below
4. **Launch Instance** ✨

Your service will be running automatically on boot.

## 📋 Launch Template Script

```bash
#!/bin/bash

# Exit on any error
set -e

# Update package list
apt update

# Install .NET 6.0
echo "Installing .NET 6.0..."
apt install -y aspnetcore-runtime-6.0
apt install -y dotnet-sdk-6.0

# Install git
echo "Installing git"
apt install -y git

# Clone repository
cd /home/ubuntu
echo "Cloning repository..."
sudo -u ubuntu git clone https://github.com/Passw0rd404/srv-02.git

# Change to project directory
cd srv-02

# Set DOTNET_CLI_HOME environment variable
echo "Setting up .NET environment..."
echo 'DOTNET_CLI_HOME=/tmp' >> /etc/environment
export DOTNET_CLI_HOME=/tmp

# Build and publish the .NET application
echo "Building .NET application..."
sudo -u ubuntu dotnet publish -c Release --self-contained=false --runtime linux-x64

# Find the actual DLL path (handle naming inconsistencies)
DLL_PATH=/home/ubuntu/srv-02/bin/Release/netcoreapp6/linux-x64/publish/srv02.dll
if [ -z "$DLL_PATH" ]; then
    echo "Error: Could not find the application DLL"
    exit 1
fi

echo "Found DLL at: $DLL_PATH"

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
systemctl daemon-reload
systemctl enable srv-02
systemctl start srv-02

# Check service status
echo "Service status:"
systemctl status srv-02 --no-pager

echo "Deployment completed!"
```

## ⚙️ Requirements

| Component | Requirement |
|-----------|-------------|
| **OS** | Ubuntu 20.04 / 22.04 LTS |
| **Instance** | t3.micro+ (1GB+ RAM) |
| **Network** | Internet access required |
| **Ports** | Configure security group as needed |

## 🔧 Service Commands

```bash
# Status & Logs
sudo systemctl status srv-02
sudo journalctl -u srv-02 -f

# Control
sudo systemctl restart srv-02
sudo systemctl stop srv-02
sudo systemctl start srv-02
```

## 🐛 Quick Debug

**Service won't start?**
```bash
# Check logs
sudo journalctl -u srv-02 --no-pager -n 20

# Test manually
cd /home/ubuntu/srv-02/publish
sudo -u ubuntu dotnet srv02.dll
```

**Files missing?**
```bash
# Verify deployment
ls -la /home/ubuntu/srv-02/publish/srv02.dll
```

## 💡 Tips

- **Testing**: Use `t3.micro` instances
- **Production**: Use `t3.small` or larger
- **Updates**: Terminate instance and launch new one with latest code
- **Logs**: Service logs to systemd journal by default

---
⭐ **Works out-of-the-box** • **Ubuntu 20/22 only** • **No configuration needed**

## 🙏 Acknowledgements

- 📚 [aws-DevOps-90 course](https://cloudnativebasecamp.com/courses/aws-devops-90/) – for helpful guidance 
