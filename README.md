# LizardBot 🦎

> 🇰🇷 [한국어 README](./README.ko.md)
> This English README was translated from the original Korean version with AI assistance.

A lightweight management bot that runs continuously on a Raspberry Pi and manages a personal server through Discord.

## 🦎 About the Project

LizardBot is a bot designed to manage the Dedicated Server used by **Lizard Company** (🎮 a gaming community that the developer is part of).

A Raspberry Pi acts as the management node, running 24/7 with low power consumption while monitoring the server. When needed, it can wake the server using Wake-on-LAN and provide server status information through Discord.

Although the project started as a personal server management tool, configuration values and environment-specific settings are kept separate from the source code so that people with similar setups can easily adapt it for their own use.

## ✨ Features

The following features are currently implemented or planned:

* 🔍 Check server status with the Discord `/status` command
* ⚡ Wake the server via Wake-on-LAN with the Discord `/wake` command
* 🔐 Role-based permissions for administrative Discord commands
* 🔔 Notifications when the server goes Online / Offline
* 🌐 Web service health checks
* ⛏️ Minecraft server status monitoring
  *(The other game servers have unfortunately been retired. 😢)*
* 💤 Automatic suspend when the server has been idle for a certain period
* 🚀 CI/CD integration
* 🛡️ Prevent automatic suspend during deployment
* 🍓 Always-on server management powered by Raspberry Pi

## 🏗️ Architecture

```text
Discord
   │
   ▼
Raspberry Pi
   │
   ├─ LizardBot
   ├─ Server Status Monitor
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

> 😥 The main server uses an older motherboard, so Wake-on-LAN does not work from a full shutdown. It does, however, work normally when the server is suspended.

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

### Wake the Server

```text
/wake
```

If the server is suspended, LizardBot sends a Wake-on-LAN packet and waits until the server becomes responsive again.

```text
💤 Server is sleeping.
Sending Wake-on-LAN...

🟢 Server is online.
```

Administrative commands such as `/wake` are intended to be restricted to users with configured Discord Roles.

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

Environment-specific values such as the Discord Bot Token, server IP address, MAC address, and Discord Guild/Role IDs are not stored directly in the source code.

In production, these values are provided through environment variables or separate private configuration files.

Example:

```text
Discord__Token
Discord__GuildId
Discord__AdminRoleId

LizardServer__IpAddress
LizardServer__MacAddress
LizardServer__BroadcastAddress
```

This allows the repository to remain public without exposing authentication credentials or sensitive details about the actual server environment.

## 🚧 Current Status

**Work in Progress**

The basic project structure and server management functionality are currently being designed and implemented.

Initial development goals:

1. Connect the Discord Bot
2. Implement the `/status` command
3. Check server and service status
4. Implement Wake-on-LAN
5. Implement the `/wake` command
6. Monitor server status changes
7. Add automatic suspend management
8. Integrate CI/CD

## 💡 Why I Made This

This is a small community, and its members are usually busy grinding through the daily quest known as **real life** (🤣), so we do not actually spend that much time gaming. Keeping a Dedicated Server running 24/7 felt like a waste of electricity.

Instead, I wanted the server to remain suspended while nobody was using it, while still allowing a few trusted administrators to wake it remotely whenever needed—even when the main server administrator was away.

I figured that keeping a low-power Raspberry Pi online and using Discord as the server management interface would provide a convenient way to check server status and wake the server remotely without requiring a separate management application.

The features themselves are intentionally simple. Hopefully, this project can serve as a small reference for anyone building their first Discord Bot or experimenting with a very simple server management tool.

Of course, these days you can ask an AI to generate code like this in seconds. 🤖🤣

Still, I figured a real-world example that has actually been wired together, deployed, and used might be useful to someone. 😉

## 📄 License

MIT License
