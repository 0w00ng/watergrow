# WaterGrow Prototype

물 키우기 MVP 개발용 Unity 2D 모바일 캐주얼 머지 디펜스 프로젝트입니다.

## 현재 구현 범위

Prototype 0.2 기준으로 아래 기능을 포함합니다.

- 모바일 세로 화면 기준 `MainScene`
- 상단 HUD: Stage, Enemies, Base HP, Gold
- 중앙 전투 필드: 물 정령 placeholder, 불꽃 병사 placeholder, base/spawn marker
- 하단 6x4 머지 보드
- Lv.1 물 유닛 소환
- 같은 레벨 유닛 2개 클릭 머지
- Lv.5 제한
- 최고 레벨, 먼저 생성된 순서 기준 대표 출전 유닛 계산
- 대표 유닛 기반 자동 단일 타겟 공격
- Stage 1-1 테스트 전투
  - 불꽃 병사 10마리
  - 1.5초 간격 스폰
  - baseHp 5
- 적 이동, 피격, HP 감소, 사망, base 도달 처리
- 처치 시 Gold 지급
- PlayerPrefs 기반 기본 저장 구조
- JSON 기반 Unit/Enemy 데이터 로딩

## Unity에서 실행

1. Unity Hub에서 `C:\Users\k2417\watergrow` 폴더를 프로젝트로 엽니다.
2. Unity 버전은 `2022.3.62f3` 기준입니다.
3. `Assets/01_Scenes/MainScene.unity`를 엽니다.
4. Scene/Game View에 Preview UI가 보여야 합니다.
5. Play 버튼을 누릅니다.
6. `Summon` 버튼으로 물 유닛을 만들고, 같은 레벨 유닛 2개를 클릭해서 머지합니다.

## 메인씬이 비어 보일 때

이 프로젝트는 Preview UI를 코드로 생성합니다. 현재 `MainScene`에는 생성된 Preview 오브젝트가 저장되어 있지만, 씬이 비어 보이거나 오브젝트 연결이 깨졌다면 아래 메뉴를 실행하세요.

`WaterGrow > Rebuild Prototype Preview Scene`

이 메뉴는 다음 오브젝트를 다시 생성하고 `MainScene`에 저장합니다.

- `Main Camera`
- `EventSystem`
- `PrototypePreviewCanvas`
- `Systems`
- `BattleField`
- `MergeBoardPanel`
- `SummonButton`
- `RestartButton`
- `ResetButton`

## 씬 오브젝트 구성

- `PrototypePreviewBootstrap`
  - Preview Scene을 생성하는 bootstrap 오브젝트입니다.
- `Systems`
  - `DataManager`
  - `SaveManager`
  - `BoardManager`
  - `UIManager`
  - `StageManager`
  - `EnemySpawner`
  - `BattleManager`
  - `GameManager`
- `PrototypePreviewCanvas`
  - HUD, 전투 필드, 머지 보드, 버튼을 포함합니다.
- `MergeBoardPanel`
  - `GridLayoutGroup` 기반 6x4 보드입니다.
- `BattleField`
  - 물 유닛과 불꽃 병사 placeholder가 표시되는 영역입니다.

## 테스트 방법

1. Play Mode를 시작합니다.
2. `Summon`을 눌러 Lv.1 유닛을 생성합니다.
3. 같은 레벨 유닛 2개를 순서대로 클릭해서 머지합니다.
4. 대표 출전 유닛 레벨이 갱신되는지 확인합니다.
5. 불꽃 병사가 오른쪽에서 왼쪽으로 이동하는지 확인합니다.
6. 대표 유닛이 있으면 자동 공격으로 적 HP가 줄어드는지 확인합니다.
7. 적을 모두 처치하면 클리어 메시지가 표시되는지 확인합니다.
8. 적이 base에 도달하면 Base HP가 감소하는지 확인합니다.
9. `Restart`로 테스트 스테이지를 다시 시작합니다.
10. `Reset`으로 저장 데이터와 보드를 초기화합니다.

## 현재 한계

- 아트, 사운드, 이펙트는 최종 리소스가 아니라 UI placeholder입니다.
- 자동 공격은 단일 타겟만 지원합니다.
- Prototype 0.2는 일반 적 1종 테스트 스테이지만 포함합니다.
- Android APK 빌드 설정은 아직 MVP 단계에서 정리해야 합니다.
