# 모바일 웹 시험 빌드

대상: Unity **2022.3.62f2** / main / Adventure.unity.
기존 Cute Block UI, 문제 정답 자동 닫기, 진행/제작/인챈트/저장 규칙을 재사용한다.

## 현재 상태를 구분하기

이 변경은 모바일 웹용 **빌드 프로필과 HTML 실행 페이지**다. HTML만 있다고 Unity 게임이 빌드된 것은 아니다.
작성 환경에는 Unity Editor/WebGL Build Support가 없으며 사용자 PC 원격 실행 연결도 없다.
실제 WebAssembly 게임 빌드, 실제 iPhone Safari/Android Chrome 플레이, 인터넷 공개 배포는 아직 실행하지 않았다.
Unity 2022.3은 모바일 브라우저를 공식 지원하지 않는다. 기기별 시험 실행용이며 모든 휴대폰 동작을 보장하지 않는다.
버전 업그레이드는 하지 않았다. Windows/macOS용 기존 WebGL과 Android APK 빌드 메뉴도 유지한다.

## 가장 간단한 빌드

1. Unity에서 Play를 종료하고 로컬 변경을 보관한다. 저장소에서 `git pull --ff-only origin main`.
2. Unity Hub의 2022.3.62f2에 **WebGL Build Support** 모듈을 설치한다. Editor 라이선스 활성화는 사용자 PC에서 정상적으로 완료돼 있어야 한다.
3. 프로젝트를 열고 컴파일 오류가 없는지 확인한다.
4. **MazeMath > Build > Mobile Web (Experimental)**을 실행한다.
5. Console에 성공 메시지가 나온 뒤 `Build/MobileWeb/index.html`과 `Build/MobileWeb/Build/` 안의 실제 게임 파일이 생겼는지 확인한다.

Setup Project, 기존 씬 재생성, 저장 초기화는 필요 없다.
빌드 시 임시 프로필을 적용하고 완료/실패 후 설정을 원래대로 복원한다.
기존 MobileWeb 산출물이 있으면 새 빌드 성공 후 `MobileWeb-backup-*` 폴더로 보존한다.
실패한 임시 빌드는 `Build/.MobileWeb-*`에 남긴다. 성공 빌드인 것처럼 사용하지 않는다.

## 휴대폰에서 바로 시험하기 — 같은 Wi-Fi

Python 3이 설치된 PC에서 프로젝트 루트 기준:

```powershell
py Tools/serve_mobile_web.py
```

macOS/Linux는 `python3 Tools/serve_mobile_web.py`.
명령창에 표시되는 **Phone on the same Wi-Fi** 주소를 휴대폰 Safari 또는 Chrome에 입력한다.
예시 주소 형식: `http://192.168.0.10:8000/`. 실제 숫자는 사용자 PC 주소로 달라진다.
**휴대폰에서 localhost를 입력하지 않는다.** localhost는 그 휴대폰 자신을 뜻한다.
명령창을 닫으면 서버도 종료된다. 종료는 Ctrl+C.

서버는 Build/MobileWeb 폴더만 제공하며 디렉터리 목록/숨김 파일/외부 경로는 제공하지 않는다.
가정의 신뢰할 수 있는 Wi-Fi에서만 사용한다. 공유기 포트포워딩이나 인터넷 공개용 서버로 쓰지 않는다.
접속이 안 되면 PC와 휴대폰의 동일 Wi-Fi 여부, 게스트 Wi-Fi 기기 격리, Windows 개인 네트워크 방화벽 허용을 확인한다.
회사 보안 정책이나 공용 네트워크 제한을 우회하지 않는다.

## 배치 빌드 대안 (Windows)

Unity에서 이 프로젝트를 닫고 PowerShell에서:

```powershell
.\Tools\build-mobile-web.ps1
```

Editor가 다른 드라이브에 있으면:

```powershell
.\Tools\build-mobile-web.ps1 -UnityPath 'D:\Unity\2022.3.62f2\Editor\Unity.exe'
```

로그: `TestResults/mobile-web-build.log`. 성공 시 `Build/MobileWeb.zip`도 생성한다.
PowerShell 실행 정책으로 막히면 조직 정책을 바꾸지 말고 Unity의 메뉴 빌드를 사용한다.
스크립트는 이전 산출물이 있는 것만으로 새 빌드를 성공 처리하지 않는다.
배치 PowerShell 및 Unity 호출은 작성 환경에서 실행하지 못했다.

## 모바일 페이지에서 달라지는 부분

- 가로 화면 플레이. 세로로 돌리면 가로 전환 안내와 게임 일시정지 요청.
- iPhone/iPad(데스크톱 모드 포함)/Android 터치 기능을 감지해 화면 버튼 표시.
- 모바일 HUD 논리 해상도 900x500으로 조정해 작은 휴대폰에서 과도한 UI 축소를 줄임.
- 기본 '선명하게': 픽셀 비율 최대 2 및 약 240만 렌더 픽셀 제한. '가볍게' 모드도 시작 전 선택 가능.
- 모바일 목표 30fps. 성능 측정 결과가 아니라 설정값이다.
- 노치/홈 표시줄 여백, 캔버스에서의 스크롤/길게 누르기 메뉴 방지.
- 탭 전환/화면 회전 시 입력 해제와 저장 시도. 돌아오면 자동 이동하지 않고 사용자가 재개.
- 전체화면 버튼은 지원되는 경우에만 선택적으로 사용. 지원되지 않아도 일반 브라우저 화면으로 동작.
- 로딩 진행률, WebGL2/WebAssembly 미지원, 로더 네트워크 실패, 그래픽 컨텍스트 손실을 명시적으로 표시.
- Gzip + Unity JavaScript decompression fallback. 멀티스레드는 끄므로 COOP/COEP 헤더 설정을 요구하지 않음.

한글 웹 게임 폰트는 기존 규칙을 따른다: `Resources/MazeMathKorean` 또는 Inspector에 지정된 한글 Font 에셋이 없으면 게임 내부 UI는 영문 fallback일 수 있다. HTML 시작 화면은 시스템 한글 폰트를 사용한다. 이 패치에는 폰트 파일을 포함하지 않는다.

## 다른 장소에서도 접속하려면

`Build/MobileWeb`의 **내용 전체**를 HTTPS 정적 호스팅에 올려야 한다.
Unity 소스 Assets 폴더, 템플릿 index.html만, ZIP 자체를 올려서는 실행되지 않는다.
index.html / mobile-web.js / mobile-web.css / Build / 필요한 StreamingAssets의 상대 경로를 유지한다.
압축 fallback 파일(.unityweb)에 잘못된 Content-Encoding을 붙이지 않는다.
실제 .wasm 파일을 제공하는 구성에서는 application/wasm MIME을 사용한다.
이 변경은 호스팅 서비스 생성이나 공개 배포를 자동으로 수행하지 않는다.

## 저장 주의

이어하기는 브라우저 사이트별 로컬 저장소다. PC/Android 앱/다른 URL과 자동 공유하지 않는다.
같은 게임이라도 PC IP나 포트 또는 공개 호스팅 주소가 달라지면 별도 저장 기록으로 취급될 수 있다.
시크릿 모드, 사이트 데이터 삭제, 브라우저 강제 종료/운영체제 메모리 정리로 저장이 보존되지 않을 수 있다.
종료 직전 저장에만 의존하지 말고 체크포인트를 사용한다. 저장소 사전 검사 통과는 IndexedDB 영속 저장을 보장하지 않는다.

## 검증 범위

- `node --test Tools/WebTests/mobile-web.test.cjs`: 터치 판별, 해상도 예산, 전체화면 미지원 처리, 템플릿 연결, 로더 실패 처리.
- `python -m unittest discover -s Tools/WebTests -p 'test_*.py'`: 빈/부분 빌드 거부, 서버 파일/MIME 검사.
- `.github/workflows/mobile-web-template-tests.yml`은 **위 검사만** 수행한다. Unity 빌드 워크플로가 아니다.
- 기존 core-tests의 C# 도메인/소스 구문 검사와 Unity Editor 컴파일/PlayMode/IL2CPP 빌드는 다른 검증이다.
- 실제 기기에서 이동+점프 동시 터치, 사다리, 주관식/객관식 후 닫힘, 제작/인챈트, 회전, 백그라운드 후 재개, 새로고침 후 이어하기, 챕터 완주를 확인해야 한다.

## Unity 공식 참고

- https://docs.unity3d.com/2022.3/Documentation/Manual/webgl-browsercompatibility.html
- https://docs.unity3d.com/2022.3/Documentation/Manual/webgl-templates.html
- https://docs.unity3d.com/2022.3/Documentation/Manual/webgl-canvas-size.html
- https://docs.unity3d.com/2022.3/Documentation/Manual/webgl-deploying.html
