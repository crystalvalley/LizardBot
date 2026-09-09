# LizardBot 🦎

Raspberry Pi에서 상시 실행되며 Discord를 통해 개인 서버를 관리하기 위한 경량 관리 봇입니다.

## 🦎 프로젝트 소개

LizardBot은 **도마뱀 컴퍼니**(🎮 개발자가 참여 중인 게임 커뮤니티)에서 사용하는 Dedicated Server를 관리하기 위한 봇입니다.

Raspberry Pi를 관리 노드로 사용하며, 24시간 저전력으로 동작하면서 서버의 상태를 확인합니다. 필요할 때는 Wake-on-LAN을 통해 서버를 깨우거나 Discord를 통해 상태 정보를 제공합니다.

개인적인 서버 관리 목적으로 시작했지만, 비슷한 환경을 사용하는 사람도 쉽게 수정하여 사용할 수 있도록 설정값과 실제 운영 환경을 코드와 분리하는 것을 목표로 합니다.

## ✨ 주요 기능

현재 구현하거나 계획 중인 기능은 다음과 같습니다.

* 🔍 Discord `/status` 명령을 통한 서버 상태 확인
* ⚡ Discord `/wake` 명령을 통한 Wake-on-LAN
* 🔐 Discord Role 기반 관리 명령 권한 제어
* 🔔 서버 Online / Offline 상태 변화 알림
* 🌐 Web 서비스 상태 확인
* ⛏️ Minecraft 서버 상태 확인
  *(현재 다른 게임 서버는 운영을 종료했습니다. 😢)*
* 💤 일정 시간 사용되지 않는 서버의 자동 절전
* 🚀 CI/CD 연동
* 🛡️ 배포 중 자동 절전 방지
* 🍓 Raspberry Pi 기반 상시 서버 관리

## 🏗️ 구성

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

Raspberry Pi는 관리 노드로서 상시 실행되며, 실제 서비스는 별도의 메인 서버인 **LizardServer**에서 동작합니다.

메인 서버가 절전 상태인 경우 Raspberry Pi가 Magic Packet을 전송하여 서버를 깨울 수 있습니다.

> 😥 메인 서버의 메인보드가 구형이라 완전 종료 상태에서는 Wake-on-LAN이 동작하지 않지만, 절전 상태에서는 정상적으로 동작합니다.

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

Discord Bot Token, 서버 IP, MAC 주소, Discord Guild/Role ID와 같은 운영 환경별 설정값은 소스 코드에 직접 저장하지 않습니다.

실제 운영 환경에서는 환경 변수 또는 별도의 비공개 설정 파일을 통해 값을 전달하는 것을 기본 원칙으로 합니다.

예:

```text
Discord__Token
Discord__GuildId
Discord__AdminRoleId

LizardServer__IpAddress
LizardServer__MacAddress
LizardServer__BroadcastAddress
```

이를 통해 공개 저장소에서도 개인 서버의 인증 정보나 민감한 운영 설정이 노출되지 않도록 구성합니다.

## 🚧 현재 상태

**개발 중**

현재 기본 프로젝트 구조 및 서버 관리 기능을 설계하고 있습니다.

초기 개발 목표는 다음과 같습니다.

1. Discord Bot 연결
2. `/status` 명령 구현
3. 서버 및 서비스 상태 확인
4. Wake-on-LAN 기능 구현
5. `/wake` 명령 구현
6. 서버 상태 변화 감시
7. 자동 절전 관리
8. CI/CD 연동

## 💡 개발 배경

소규모 커뮤니티이고 멤버들이 '현생'이라는 일일 퀘스트에 바빠서(🤣) 게임하는 시간이 많지 않다 보니, Dedicated Server를 항상 켜 두는 것은 전력 낭비라고 생각했습니다.

그래서 서버를 사용하지 않을 때는 절전 모드로 두고, 서버 관리자가 자리를 비우더라도 몇몇 관리자들이 필요할 때 직접 서버를 깨울 수 있는 방법이 필요했습니다.

전력 소모가 적은 Raspberry Pi를 항상 켜 두고 Discord를 서버 관리 인터페이스로 사용하면, 별도의 관리 프로그램 없이도 서버 상태 확인과 원격 기동을 편리하게 처리할 수 있다고 판단하여 이 프로젝트를 시작했습니다.

기능 자체는 단순한 기능들로 구성되어 있습니다. 그러니 처음 Discord Bot을 만들어 보거나, 초간단 서버 관리 도구를 만들어 보고 싶은 분들에게 작은 참고가 되었으면 합니다.

물론 요즘은 AI에게 물어보면 이런 코드쯤은 순식간에 만들어 주는 시대지만요. 🤖🤣

그래도 실제 환경에서 이것저것 연결하고 굴려 본 예제 하나쯤은 도움이 되지 않을까... 하는 마음입니다.😉

## 📄 License

MIT License
