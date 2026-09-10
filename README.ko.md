# LizardBot 🦎

Raspberry Pi에서 상시 실행되며 Discord를 통해 개인 서버를 관리하기 위한 경량 관리 봇입니다.

## 🦎 프로젝트 소개

LizardBot은 **도마뱀 컴퍼니**(🎮 개발자가 참여 중인 게임 커뮤니티)에서 사용하는 Dedicated Server를 관리하기 위한 봇입니다.

Raspberry Pi를 관리 노드로 사용하며, 24시간 저전력으로 동작하면서 서버의 상태를 확인합니다. 필요할 때는 Wake-on-LAN을 통해 서버를 깨우거나 Discord를 통해 상태 정보를 제공합니다.

개인적인 서버 관리 목적으로 시작했지만, 비슷한 환경을 사용하는 사람도 쉽게 수정하여 사용할 수 있도록 설정값과 실제 운영 환경을 코드와 분리하는 것을 목표로 합니다.

## ✨ 주요 기능

현재 구현되어 있거나 개발 중인 주요 기능은 다음과 같습니다.

* 🔍 Discord `/status` 명령을 통한 서버 상태 확인
* ⚡ Discord `/wake` 명령을 통한 Wake-on-LAN
* 🔐 복수 Discord Role 기반 관리 명령 권한 제어
* 🔔 서버 Online / Offline 상태 변화 감지 및 Discord 알림
* 🌐 Web 서비스 상태 확인
* ⛏️ Minecraft 서버 상태 확인
  *(현재 다른 게임 서버는 운영을 종료했습니다. 😢)*
* 📊 Discord Embed 기반 서버 상태 Dashboard *(개발 예정)*
* ♻️ Dashboard 메시지 삭제 시 자동 복구 *(개발 예정)*
* ⏱️ 서버 Online 시점 및 Uptime 표시 *(개발 예정)*
* ⚙️ 관리자 Slash Command를 통한 Dashboard 표시 설정 *(개발 예정)*
* 💤 일정 시간 사용되지 않는 서버의 자동 절전 *(예정)*
* 🚀 GitHub Actions 기반 CI 및 Release 자동 생성
* 🔄 GitHub Release Polling 기반 Raspberry Pi 자동 업데이트 *(개발 예정)*
* 🛡️ 배포 중 자동 절전 방지 *(예정)*
* 🍓 Raspberry Pi 기반 상시 서버 관리

## 🏗️ 구성

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

Raspberry Pi는 관리 노드로서 상시 실행되며, 실제 서비스는 별도의 메인 서버인 **LizardServer**에서 동작합니다.

메인 서버가 절전 상태인 경우 Raspberry Pi가 Magic Packet을 전송하여 서버를 깨울 수 있습니다.

> 😥 메인 서버의 메인보드가 구형이라 완전 종료 상태에서는 Wake-on-LAN이 동작하지 않지만, 절전 상태에서는 정상적으로 동작합니다.

LizardBot은 관리 중인 서버의 상태를 주기적으로 확인하며, 현재는 서버의 Online / Offline 상태 변화가 감지되면 Discord에 알림을 전송합니다.

기본적으로 새로운 상태 메시지를 생성하는 대신 해당 메시지만 지속적으로 갱신하는 방식을 사용합니다.

그렇기 때문에 Dashboard 메시지의 ID는 별도의 런타임 상태로 저장하여 LizardBot이나 Raspberry Pi가 재시작되어도 기존 메시지를 다시 찾을 수 있도록 지정하며, 메시지가 삭제된 경우에만 새로운 메시지를 자동으로 생성합니다.

## 🎮 사용 예시

### 서버 상태 확인

```text
/status
```

예상 응답:

```text
🟢 LizardServer ONLINE

Web         ✅
Minecraft   ✅
```

LizardBot은 먼저 서버 자체가 응답 가능한 상태인지 확인한 뒤, 서버에 설정된 각 서비스의 상태를 확인합니다.

서버와 Health Check 항목은 설정 파일을 통해 관리하므로, 새로운 서버나 서비스를 추가하기 위해 코드를 직접 수정할 필요가 없습니다.

### 서버 깨우기

```text
/wake
```

서버가 절전 상태라면 Wake-on-LAN 패킷을 전송하고, 서버가 다시 응답할 때까지 상태를 확인합니다.

```text
💤 Server is sleeping.
Sending Wake-on-LAN...

🟢 Server is online.
```

`/wake`와 같은 관리 명령은 설정된 Discord Role을 가진 사용자만 사용할 수 있도록 제한할 예정입니다.

### 서버 Dashboard *(개발 예정)*

지정된 Discord 채널에 서버 상태를 표시하는 Embed 메시지 하나를 생성하고 지속적으로 갱신하는 기능을 추가할 예정입니다.

예:

```text
🦎 LizardBot Server Status

🟢 LizardServer

상태          ONLINE
Online Since  3시간 전

🌐 Web         ✅
⛏️ Minecraft   ✅

마지막 확인: 방금 전
```

상태가 변경될 때마다 새로운 메시지를 생성하는 대신 기존 Dashboard 메시지를 갱신하여, 상태 채널에 불필요한 메시지가 계속 쌓이지 않도록 하는 것을 목표로 합니다.

관리자가 Dashboard 메시지를 삭제한 경우에는 다음 상태 확인 시 새로운 Dashboard 메시지를 자동으로 생성하도록 구성할 예정입니다.

## 🛠️ 기술 스택

* C#
* ASP.NET Core
* Discord.Net
* Raspberry Pi OS
* Wake-on-LAN
* systemd
* Docker
* GitHub Actions

## ⚙️ 설정

LizardBot은 인증 정보와 관리 대상 서버 설정을 소스 코드에서 분리하여 관리합니다.

현재 Raspberry Pi 운영 환경에서는 다음과 같은 구조를 사용합니다.

```text
/etc/lizardbot/
├─ lizardbot.env
│  └─ Discord Bot Token 등 환경별 설정
│
└─ servers.json
   └─ 관리할 서버 및 Health Check 설정
```

Dashboard 기능 구현 후에는 LizardBot 자체가 유지해야 하는 런타임 상태를 `/var/lib/lizardbot/`에 별도로 저장할 예정입니다.

```text
/var/lib/lizardbot/
└─ dashboard.json
   └─ Dashboard Message ID 등 Bot이 유지해야 하는 런타임 상태
```

### 환경 변수

Discord Bot Token과 같은 민감한 값은 소스 코드나 공개 설정 파일에 직접 저장하지 않습니다.

예:

```text
Discord__Token
Discord__GuildId
Discord__StatusChannelId

Discord__AdminRoleIds__0
Discord__AdminRoleIds__1
```

여러 개의 관리자 Role을 지정할 수 있으며, 설정된 Role 중 하나 이상을 가진 사용자는 `/wake`와 같은 관리 명령을 사용할 수 있습니다.

개발 환경에서는 .NET User Secrets를 사용할 수 있습니다.

### 서버 설정

관리할 서버는 `servers.json`에서 리스트 형태로 정의합니다.

예:

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

서버 및 Health Check 항목은 설정 기반으로 관리되므로 새로운 서버나 서비스를 추가하기 위해 소스 코드를 수정할 필요가 없습니다.

운영 환경의 `servers.json`은 ASP.NET Core Configuration을 통해 읽으며, 실행 중 설정 파일이 변경될 경우에도 변경된 서버 설정을 반영할 수 있도록 구성되어 있습니다.

### 런타임 상태 *(Dashboard 개발 예정)*

Dashboard Message ID처럼 LizardBot이 실행 중 직접 생성하거나 변경해야 하는 값은 설정 파일과 분리하여 `/var/lib/lizardbot/`에 저장할 예정입니다.

이를 통해 LizardBot이나 Raspberry Pi가 재시작되더라도 기존 Dashboard 메시지를 다시 찾아 계속 사용할 수 있도록 구성할 계획입니다.

## 🚧 현재 상태

현재 기본적인 서버 관리 기능은 구현되어 있으며 Raspberry Pi에서 실제로 운영 중입니다.

### ✅ 구현 완료

1. Discord Bot 연결 및 `/ping`
2. 다중 서버 설정
3. `/status`를 통한 서버 및 서비스 상태 확인
4. Ping 및 TCP 기반 Health Check
5. Wake-on-LAN
6. `/wake`를 통한 원격 서버 기동
7. 복수 Discord Role 기반 관리자 권한 제어
8. 서버 Online / Offline 상태 변화 감지
9. Discord 상태 변화 알림
10. Raspberry Pi용 Linux ARM64 배포
11. systemd 기반 LizardBot 자동 실행
12. GitHub Actions 기반 CI
13. Git Tag 기반 Raspberry Pi용 Release 자동 생성

### 🛠️ 개발 중

* Discord Embed 기반 서버 Dashboard
* Dashboard Message ID 영속화
* Dashboard 메시지 삭제 시 자동 복구
* 서버 Online 시점 및 Uptime 표시
* 관리자 Slash Command를 통한 Dashboard 표시 설정
* GitHub Release Polling 기반 Raspberry Pi 자동 업데이트

### 🧪 예정

* Minecraft 상세 상태 및 접속자 확인
* 서버 유휴 상태 감지 및 자동 절전
* 배포 중 자동 절전 방지

## 💡 개발 배경

소규모 커뮤니티이고 멤버들이 '현생'이라는 일일 퀘스트에 바빠서(🤣) 게임하는 시간이 많지 않다 보니, Dedicated Server를 항상 켜 두는 것은 전력 낭비라고 생각했습니다.

그래서 서버를 사용하지 않을 때는 절전 모드로 두고, 서버 관리자가 자리를 비우더라도 몇몇 관리자들이 필요할 때 직접 서버를 깨울 수 있는 방법이 필요했습니다.

전력 소모가 적은 Raspberry Pi를 항상 켜 두고 Discord를 서버 관리 인터페이스로 사용하면, 별도의 관리 프로그램 없이도 서버 상태 확인과 원격 기동을 편리하게 처리할 수 있다고 판단하여 이 프로젝트를 시작했습니다.

기능 자체는 단순한 기능들로 구성되어 있습니다. 그러니 처음 Discord Bot을 만들어 보거나, 초간단 서버 관리 도구를 만들어 보고 싶은 분들에게 작은 참고가 되었으면 합니다.

물론 요즘은 AI에게 물어보면 이런 코드쯤은 순식간에 만들어 주는 시대지만요. 🤖🤣

그래도 실제 환경에서 이것저것 연결하고 굴려 본 예제 하나쯤은 도움이 되지 않을까... 하는 마음입니다. 😉

## 📄 License

MIT License