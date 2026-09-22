# MazeMath — 모모의 숫자 미로

**Unity 2022.3.62f2 / main / Native development**

블록형 UI, 직접 이동하는 3층 폐광, 숫자 문제, 환경 퍼즐, 로봇 장비 제작·인챈트, 골렘 보스를 연결한 첫 챕터 구현입니다. UI는 이전 `yeonjun_block_ui_v1.html` 시안의 목표 카드·하트·경험치·6칸 퀵슬롯 구성을 참고했습니다.

## Unity에서 실행

```bash
git pull --ff-only origin main
```

Unity **2022.3.62f2**로 프로젝트를 열고 **MazeMath > Play Adventure**를 선택합니다.

또는 **`Assets/MazeMath/Scenes/Adventure.unity`**를 더블클릭하고 Play를 누릅니다. 첫 실행은 **새 탐험 시작 / New adventure**입니다.

**이번 씬과 `.meta`는 저장소에 포함되어 있습니다. Setup Project를 먼저 누를 필요가 없습니다.** 씬은 런타임에 월드와 UI를 생성하므로 편집 모드에는 시작 오브젝트만 보일 수 있습니다.

기존 `Bootstrap` / `Gameplay`는 이전 기능 데모입니다. 새 챕터는 `Adventure`에서 실행하세요. 기존 씬을 유지하려고 파일을 지우거나 새 프로젝트를 만들 필요는 없습니다.

## 플레이

방향키/WASD 이동·사다리, Space 점프, E 조사, M 지도, B 가방, Esc 일시정지. 화면 아래 버튼으로도 모두 조작합니다.

숫자 장치 → 이전 작업대로 복귀 → 곡괭이 팔 제작 → 균열벽 → 상자/저울 다리 → 사다리 → 거울/발판 → 위층 골렘의 세 보호막 → 마지막 대화로 챕터 완료.

- 장비 5종 / 장비별 인챈트 2종 / 안내 제작과 3×3 제작.
- 현재 위치·장비·재료·XP·해금·미완료 숫자 문제 저장/이어하기.
- 지도는 방문한 방을 표시하며 순간이동하지 않습니다.
- 첫 챕터의 지형은 직접 설계한 고정 맵이고, 새 탐험의 숫자 문제·발판 순서는 시드로 달라집니다.

## 빌드와 검증

`MazeMath > Build > WebGL` → `Build/Web/`

`MazeMath > Build > Android APK` → `Build/Android/MazeMath.apk`

빌드 메뉴는 Adventure만 빌드하고 기존 씬을 재생성하지 않습니다. 필요한 WebGL/Android Build Support가 Unity Hub에 설치되어 있어야 합니다. Android 산출물은 로컬 테스트용 debug APK이며 스토어 출시물이 아닙니다.

Unity를 닫고 PowerShell에서:

```powershell
.\Tools\verify-unity.ps1
.\Tools\verify-unity.ps1 -BuildWeb -BuildAndroid
```

실제 GitHub Actions 순수 C# 및 구문 검사 **42/42 통과** 기록: [검증 기록](Docs/Development/ADVENTURE_VERIFICATION.md).

**Unity Editor 화면 실행·Unity EditMode/PlayMode·WebGL/APK 빌드·실기기 플레이는 이 개발 환경에서 실행하지 못했습니다.** C# 자동 검사 성공을 Unity 빌드 성공으로 간주하지 마세요. 실행 스크립트는 결과 XML과 산출물 존재 여부를 확인합니다.

한글 프로젝트/OS 폰트를 사용할 수 없으면 영문 LegacyRuntime UI로 전환합니다. WebGL 한글 고정 방법은 실행 안내서의 폰트 항목을 참고하세요.

## 문서

- [실행·조작·제작·저장·빌드 안내](Docs/Development/ADVENTURE_PLAY_GUIDE.md)
- [현재 개발 상태](Docs/Development/NATIVE_PROGRESS.md)
- [실행 기록과 설계 결정](Docs/Development/ADVENTURE_EXECUTION.md)
- [미로 시스템 설계](Docs/Design/01_MAZE_SYSTEM_DESIGN.md)
- [제작·인챈트 설계](Docs/Design/02_CRAFTING_ENCHANT_DESIGN.md)
- [문제·환경 퍼즐 설계](Docs/Design/03_PUZZLE_QUESTION_SYSTEM_DESIGN.md)

이전 `MAZE_FOUNDATION_STATUS.md`와 foundation 계획은 초기 작업 기록이며, 현재 실행 진입점은 위의 Adventure 씬입니다.
