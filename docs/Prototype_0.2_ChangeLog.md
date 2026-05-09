# Prototype 0.2 ChangeLog

## 변경 파일 목록

- `Assets/02_Scripts/Board/BoardManager.cs`
  - 보드 상태 변경 이벤트 `BoardStateChanged` 추가
  - 대표 출전 변경, 소환 가능 여부와 함께 UI 연결 지점 확장
  - 소환/머지 성공 및 실패 안내 이벤트 추가
- `Assets/02_Scripts/Board/BoardCell.cs`
  - 소환/머지 시 셀 pulse 피드백 추가
- `Assets/02_Scripts/Battle/BattleManager.cs`
  - 대표 유닛 기반 자동 공격 추가
  - 테스트 스테이지 자동 시작 옵션 추가
  - 적 스폰, 처치, 기지 도달 콜백 처리 추가
  - 처치 보상 골드 지급 연결
  - 공격 pulse 및 projectile-like UI 이펙트 추가
  - 공격 이펙트가 적에게 도착한 뒤 피해가 적용되도록 변경
- `Assets/02_Scripts/Core/DataManager.cs`
  - Unit/Enemy TextAsset JSON 로딩 진입점 추가
  - Preview Scene fallback 데이터 추가
  - `Resources/Data` 자동 로딩 추가
- `Assets/Resources/Data/UnitTable.json`
  - 런타임 자동 로딩용 유닛 데이터 추가
- `Assets/Resources/Data/EnemyTable.json`
  - 런타임 자동 로딩용 적 데이터 추가
- `Assets/02_Scripts/Battle/EnemyController.cs`
  - HP, 이동, 피격, 사망, 기지 도달 처리 추가
  - Preview용 HP Bar, 이름 라벨, 피격 반응 연결 지점 추가
- `Assets/02_Scripts/Battle/EnemySpawner.cs`
  - spawnPoint/targetPoint 기반 스폰 구조 추가
  - Enemy prefab 미연결 시 placeholder GameObject 생성
  - UI placeholder 적에 HP Bar와 이름 라벨 자동 생성
- `Assets/02_Scripts/Battle/EnemyData.cs`
  - Prototype 0.2용 적 런타임 데이터 구조 추가
- `Assets/02_Scripts/Battle/WaterUnitData.cs`
  - Prototype 0.2용 물 유닛 전투 데이터 구조 추가
- `Assets/02_Scripts/Stage/StageManager.cs`
  - Stage 1-1 테스트 진행도, baseHp, 클리어/실패 이벤트 추가
- `Assets/02_Scripts/UI/UIManager.cs`
  - 현재 스테이지, 남은 적 수, baseHp, 대표 유닛, 보드 상태 변경 UI 메서드 추가
  - 대표 물정령 Lv별 색상/크기/라벨 갱신 추가
- `Assets/03_Data/EnemyTable.json`
  - `ENEMY_001` 불꽃 병사 HP를 Prototype 0.2 기준 30으로 조정
- `Assets/03_Data/WaveTable.json`
  - `WAVE_1_01`을 적 10마리, 1.5초 간격으로 조정
- `README.md`
  - Prototype 0.2 기준 실행 및 테스트 방법 갱신
- `Assets/01_Scenes/MainScene.unity`
  - 기능 확인용 Prototype Preview Scene 추가
- `Assets/02_Scripts/Core/PrototypePreviewBootstrap.cs`
  - Play Mode에서 Canvas, HUD, 전투 필드, 6x4 보드, 버튼, 스폰 포인트를 자동 조립
  - 대표 물정령 placeholder, 공격 이펙트 root, DataManager 자동 연결
  - 재시작/초기화 버튼 추가
- `ProjectSettings/EditorBuildSettings.asset`
  - `MainScene`을 빌드 씬 목록에 등록

## 구현 기능 요약

- 6x4 머지 보드는 기존 구조를 유지했습니다.
- `MainScene`에서 최소 시각화 UI를 자동 생성해 Play Mode 테스트가 가능하게 했습니다.
- 대표 출전 유닛은 보드 내 최고 레벨, 동률 시 먼저 생성된 유닛 기준을 유지합니다.
- 대표 유닛이 없으면 자동 공격하지 않습니다.
- 대표 유닛이 있으면 현재 살아있는 적 중 목표 지점에 가장 가까운 적을 공격합니다.
- 공격 시 대표 물정령 placeholder가 pulse되고 파란 공격 이펙트가 적 방향으로 이동합니다.
- 공격 피해는 이펙트가 도착한 뒤 적용됩니다.
- 소환/머지 성공, 골드 부족, 보드 가득 참, 잘못된 머지 선택 안내가 표시됩니다.
- 테스트 스테이지 재시작과 저장 데이터 초기화가 Preview Scene에서 가능합니다.
- Prototype 0.2에서는 불꽃 병사 1종만 스폰합니다.
- 적은 spawnPoint에서 targetPoint 방향으로 이동합니다.
- 적 HP가 0 이하가 되면 처치 처리되고 골드 보상이 지급됩니다.
- 적이 targetPoint에 도달하면 baseHp가 감소하고 적은 제거됩니다.
- 모든 적이 처리되고 baseHp가 남아 있으면 클리어됩니다.
- baseHp가 0 이하가 되면 실패됩니다.

## 테스트 방법

1. Unity Editor에서 `Assets/01_Scenes/MainScene.unity`를 엽니다.
2. Play Mode를 실행합니다.
3. 하단 `소환` 버튼으로 Lv.1 물방울을 생성합니다.
4. 같은 레벨 물방울 2개를 클릭해 머지합니다.
5. 대표 유닛 생성 후 불꽃 병사가 자동 공격으로 제거되는지 확인합니다.
6. 모든 적 처리 시 클리어, baseHp 0 이하 시 실패를 확인합니다.

## 확인된 한계

- Unity 2022.3.62f3 batchmode 기준 스크립트 컴파일은 통과했습니다.
- Preview Scene은 포함되어 있지만 최종 Scene/Prefab 구조는 아직 아닙니다.
- `DataManager`는 `Resources/Data`의 Unit/Enemy JSON을 자동 로딩합니다. 실패 시 fallback 값을 사용합니다.
- 공격은 투사체 이동이 아니라 즉시 피해 처리입니다.
- 적이 기지에 도달한 경우 남은 적 수에서는 제거되지만 처치 보상은 지급되지 않습니다.

## 다음 작업 제안

- MainScene과 기본 Canvas/Prefab 생성
- JSON TextAsset을 Preview Scene에 실제 연결
- 실제 HP Bar prefab 연결
- Projectile 기반 공격 연출 연결
- Stage 1-1 클리어 후 다음 스테이지 진입 버튼 구현
- SaveData에 currentStage 및 전투 진행 상태 저장 범위 결정
