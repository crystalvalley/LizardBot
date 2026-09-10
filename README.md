# LizardBot 🦎

> 🇰🇷 [한국어 README](./README.ko.md)  
> This English README was translated from the original Korean version with AI assistance.

A lightweight management bot that runs continuously on a Raspberry Pi and manages personal servers through Discord.

## 🦎 Project Overview

LizardBot is a bot designed to manage the dedicated server used by **Lizard Company** (🎮 a gaming community that the developer is part of).

A Raspberry Pi acts as the management node, running 24/7 with low power consumption while monitoring server status. When needed, it can wake a server using Wake-on-LAN and provide status information through Discord.

Although this project started for personal server management, configuration values and environment-specific settings are kept separate from the source code so that people with similar setups can easily adapt it for their own use.

## ✨ Features

The following features are currently implemented or under development:

* 🔍 Check server status with the Discord `/status` command
* ⚡ Wake servers via Wake-on-LAN with the Discord `/wake` command
* 🔐 Administrative command permissions based on multiple Discord Roles
* 🔔 Detect server Online / Offline state changes and send Discord notifications
* 🌐 Web service status checks
* ⛏️ Minecraft server status checks  
  *(The other game servers have been retired for now. 😢)*
* 📊 Discord Embed-based server status Dashboard *(planned)*
* ♻️ Automatically recreate the Dashboard message if it is deleted *(planned)*
* ⏱️ Display server Online time and Uptime *(planned)*
* ⚙️ Configure Dashboard display options through administrator Slash Commands *(planned)*
* 💤 Automatically suspend servers after a period of inactivity *(planned)*
* 🚀 CI and automated Release generation using GitHub Actions
* 🔄 Automatic Raspberry Pi updates by polling GitHub Releases *(planned)*
* 🛡️ Prevent automatic suspend during deployments *(planned)*
* 🍓 Always-on server management powered by Raspberry Pi

## 🏗️ Architecture

```text
Discord
   │
   ├─ Slash Commands
   │    ├─ /ping
   │    ├─ /status
   │    └─ /wake
   │
   └─ Server Dashboard (Planned)
          ▲
          │
Raspberry Pi
   │
   ├─ LizardBot
   ├─ Server Status Monitor
   ├─ Discord Dashboard
   ├─ Wake-on-LAN
   └─ Server Management
          │
          ▼
      LizardServer
          │
          ├─ Web
          ├─ Minecraft
          └─ Docker Services
```

The Raspberry Pi runs continuously as the management node, while the actual services run on a separate main server called **LizardServer**.

When the main server is suspended, the Raspberry Pi can send a Magic Packet to wake it up.

> 😥 The main server uses an older motherboard, so Wake-on-LAN does not work from a full shutdown. It does, however, work normally while the server is suspended.

LizardBot periodically checks the status of managed servers. Currently, when an Online / Offline state change is detected, it sends a notification to Discord.

By default, instead of creating a new status message each time, the same message is continuously updated.

For this reason, the Dashboard Message ID is stored separately as runtime state so that LizardBot can find the existing message again even after LizardBot or the Raspberry Pi restarts. A new message is created automatically only if the existing Dashboard message has been deleted.

## 🎮 Usage Examples

### Check Server Status

```text
/status
```

Example response:

```text
🟢 LizardServer ONLINE

Web         ✅
Minecraft   ✅
```

LizardBot first checks whether the server itself is reachable, then checks the status of each service configured for that server.

Servers and Health Check entries are managed through configuration files, so adding a new server or service does not require modifying the source code.

### Wake a Server

```text
/wake
```

If the server is suspended, LizardBot sends a Wake-on-LAN packet and waits until the server becomes responsive again.

```text
💤 Server is sleeping.
Sending Wake-on-LAN...

🟢 Server is online.
```

Administrative commands such as `/wake` are restricted to users with one of the configured Discord Roles.

### Server Dashboard *(planned)*

LizardBot will create a single Embed message in a designated Discord channel to display server status and continuously update that message.

Example:

```text
🦎 LizardBot Server Status

🟢 LizardServer

Status        ONLINE
Online Since  3 hours ago

🌐 Web         ✅
⛏️ Minecraft   ✅

Last checked: just now
```

Instead of creating a new message every time the status changes, LizardBot will update the existing Dashboard message so the status channel does not fill up with unnecessary messages.

If an administrator deletes the Dashboard message, LizardBot will automatically create a new one during the next status check.

## 🛠️ Tech Stack

* C#
* ASP.NET Core
* Discord.Net
* Raspberry Pi OS
* Wake-on-LAN
* systemd
* Docker
* GitHub Actions

## ⚙️ Configuration

LizardBot keeps authentication information and managed-server configuration separate from the source code.

The current Raspberry Pi deployment uses the following structure:

```text
/etc/lizardbot/
├─ lizardbot.env
│  └─ Environment-specific settings such as the Discord Bot Token
│
└─ servers.json
   └─ Managed servers and Health Check configuration
```

After the Dashboard feature is implemented, runtime state that must be preserved by LizardBot itself will be stored separately under `/var/lib/lizardbot/`.

```text
/var/lib/lizardbot/
└─ dashboard.json
   └─ Runtime state such as the Dashboard Message ID
```

### Environment Variables

Sensitive values such as the Discord Bot Token are not stored directly in source code or public configuration files.

Example:

```text
Discord__Token
Discord__GuildId
Discord__StatusChannelId

Discord__AdminRoleIds__0
Discord__AdminRoleIds__1
```

Multiple administrator Roles can be configured. Users with at least one of the configured Roles can use administrative commands such as `/wake`.

In development environments, .NET User Secrets can be used.

### Server Configuration

Managed servers are defined as a list in `servers.json`.

Example:

```json
{
  "ServerManagement": {
    "Servers": [
      {
        "Id": "lizard",
        "Name": "LizardServer",
        "Address": "192.168.x.x",
        "MacAddress": "AA:BB:CC:DD:EE:FF",
        "BroadcastAddress": "192.168.x.255",
        "HealthChecks": [
          {
            "Name": "Web",
            "Type": "Tcp",
            "Port": 443
          },
          {
            "Name": "Minecraft",
            "Type": "Tcp",
            "Port": 25565
          }
        ]
      }
    ]
  }
}
```

Servers and Health Check entries are configuration-driven, so new servers or services can be added without modifying the source code.

In production, `servers.json` is loaded through ASP.NET Core Configuration. Changes made to the file while LizardBot is running can be reflected without restarting the application.

### Runtime State *(Dashboard planned)*

Values that LizardBot creates or changes at runtime, such as the Dashboard Message ID, will be stored separately under `/var/lib/lizardbot/` rather than mixed with configuration files.

This allows LizardBot to find and continue using the existing Dashboard message even after LizardBot or the Raspberry Pi restarts.

## 🚧 Current Status

The core server-management features have been implemented and LizardBot is currently running on an actual Raspberry Pi.

### ✅ Implemented

1. Discord Bot connection and `/ping`
2. Multiple managed server configuration
3. Server and service status checks through `/status`
4. Ping and TCP-based Health Checks
5. Wake-on-LAN
6. Remote server wake through `/wake`
7. Administrator permissions based on multiple Discord Roles
8. Server Online / Offline state-change detection
9. Discord state-change notifications
10. Linux ARM64 deployment for Raspberry Pi
11. Automatic LizardBot startup using systemd
12. CI using GitHub Actions
13. Automatic Raspberry Pi Release generation from Git Tags

### 🛠️ In Development

* Discord Embed-based server Dashboard
* Persistent Dashboard Message ID storage
* Automatic recovery when the Dashboard message is deleted
* Server Online time and Uptime display
* Dashboard display configuration through administrator Slash Commands
* Automatic Raspberry Pi updates by polling GitHub Releases

### 🧪 Planned

* Detailed Minecraft status and player information
* Server idle detection and automatic suspend
* Prevent automatic suspend during deployments

## 💡 Why I Made This

This is a small community, and the members are usually busy with the daily quest known as **real life** (🤣), so we do not actually spend that much time gaming. Keeping a dedicated server running 24/7 felt like a waste of electricity.

So I wanted the server to stay suspended while it was not in use, while still allowing a few administrators to wake it up when needed even if the main server administrator was away.

I figured that keeping a low-power Raspberry Pi online and using Discord as the server-management interface would provide a convenient way to check server status and wake the server remotely without requiring a separate management application.

The features themselves are intentionally simple. Hopefully, this project can serve as a small reference for anyone building their first Discord Bot or experimenting with a very simple server-management tool.

Of course, these days you can ask an AI and get code like this generated in seconds. 🤖🤣

Still, I thought a real-world example that has actually been wired together and put into operation might be useful to someone. 😉

## 📄 License

MIT License
