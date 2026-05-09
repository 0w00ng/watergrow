# WaterGrow Prototype

물 키우기 MVP 개발용 Unity 2D 모바일 캐주얼 머지 디펜스 프로젝트입니다.

## 현재 구현 범위: Prototype 0.2

- Unity `Assets` 기준 폴더 구조
- 6x4 머지 보드 핵심 스크립트
- Lv.1 물방울 소환
- 동일 레벨 유닛 2개 클릭 머지
- Lv.5 제한
- 최고 등급 및 먼저 생성된 순서 기준 대표 출전 유닛 계산
- 대표 출전 유닛 변경 이벤트
- 대표 유닛 기반 단일 타겟 자동 공격
- 대표 유닛 공격 시 물정령 pulse와 짧은 파란색 공격 이펙트 표시
- 공격 이펙트 도착 후 피해 적용
- 소환/머지 성공 및 실패 안내 문구
- 소환/머지 시 보드 셀 pulse 피드백
- Prototype 0.2 테스트 스테이지
  - Stage 1-1
  - 불꽃 병사 10마리
  - 1.5초 간격 스폰
  - baseHp 5
- 불꽃 병사 이동, 피격, 사망, 기지 도달 처리
- 처치 보상 골드 지급
- PlayerPrefs 기반 기본 저장 구조
- UI 연결용 null-safe 메서드
- `DataManager` 기반 Unit/Enemy TextAsset JSON 로딩 진입점
- MVP 데이터 테이블 JSON 초안

## Unity에서 열기

1. Unity Hub에서 이 폴더를 프로젝트로 추가합니다.
2. Unity 2022.3 LTS 계열로 엽니다.
3. `Assets/01_Scenes/MainScene.unity`를 엽니다.
4. Play Mode를 실행합니다.

## Prototype Preview Scene

`MainScene`에는 `PrototypePreviewBootstrap` 오브젝트가 포함되어 있습니다. 이 Bootstrap은 Play Mode에서 아래 오브젝트를 자동 생성하고 연결합니다.

- `PrototypePreviewCanvas`
  - 상단 HUD: Stage, 남은 적 수, Base HP, Gold
  - 중앙 BattleField: 물 유닛 placeholder, 불꽃 병사 spawn/base marker, 안내 문구
  - 물정령 placeholder: 파란색 대표 유닛 표시, 공격 시 pulse
  - 대표 물정령 Lv에 따른 색상/크기/라벨 변경
  - 불꽃 병사 placeholder: 주황색 UI 오브젝트, 이름 라벨, HP Bar, 피격 반응
  - 공격 이펙트: 파란색 projectile-like UI placeholder
  - 하단 MergeBoardPanel: 6x4 GridLayoutGroup 보드
  - 하단 버튼: 소환, 저장
- `Systems`
   - `DataManager`
   - `GameManager`
   - `SaveManager`
   - `BoardManager`
   - `UIManager`
   - `BattleManager`
   - `StageManager`
   - `EnemySpawner`

Preview Scene은 최종 디자인이 아니라 기능 확인용입니다. 아트 리소스 없이 Unity 기본 UI Image/Text와 런타임 placeholder만 사용합니다.

수동 Scene을 만들 경우 필요한 연결은 다음과 같습니다.

1. Canvas 하위에 6x4 GridLayoutGroup 보드를 만들고 `BoardCell` 또는 `cellPrefab`을 `BoardManager`에 연결합니다.
2. `EnemySpawner`에 `spawnPoint`, `targetPoint`, `enemyRoot`를 연결합니다.
3. `UIManager`에 HUD Text와 소환 버튼을 연결합니다.
4. `BattleManager`에 `BoardManager`, `EnemySpawner`, `StageManager`, `UIManager`를 연결합니다.
5. Text/Button UI는 선택 연결입니다. 연결하지 않아도 NullReferenceException이 나지 않도록 작성되어 있습니다.

## Prototype 0.2 테스트 순서

1. Play Mode를 시작합니다.
2. 소환 버튼으로 Lv.1 물방울을 생성합니다.
3. 소환된 칸의 pulse와 안내 문구를 확인합니다.
4. 같은 레벨 물방울 2개를 차례로 클릭해 머지합니다.
5. 대표 출전 텍스트, 중앙 물정령 색/크기/라벨 갱신을 확인합니다.
6. `BattleManager.autoStartTestStage`가 켜져 있으면 테스트 스테이지가 자동 시작됩니다.
7. 불꽃 병사가 스폰되고 왼쪽 목표 지점으로 이동하는지 확인합니다.
8. 대표 유닛이 있으면 가장 목표 지점에 가까운 적을 자동 공격합니다.
9. 파란 공격 이펙트가 적에게 도착한 뒤 HP가 감소하는지 확인합니다.
10. 적 처치 시 골드가 증가하고 남은 적 수가 감소하는지 확인합니다.
11. 모든 적이 처리되고 baseHp가 남아 있으면 스테이지 클리어 메시지가 표시됩니다.
12. baseHp가 0이 되면 스테이지 실패 메시지가 표시됩니다.

## 현재 한계

- 기능 확인용 Unity Scene과 런타임 Canvas 생성기는 포함되어 있지만, 최종 수동 제작 Scene/Prefab은 아직 없습니다.
- 적과 투사체 아트는 없습니다. `EnemySpawner`는 prefab이 없으면 HP Bar가 붙은 placeholder GameObject를 생성합니다.
- 전투 데이터는 `DataManager` TextAsset 연결 시 JSON을 읽을 수 있습니다. Preview Scene은 미연결 상태라 fallback 값을 사용합니다.
- 자동 공격은 단일 타겟이며, Preview에서는 UI 공격 이펙트 도착 후 피해가 적용됩니다.
- 공격 이펙트는 UI placeholder이며 실제 투사체 판정은 아직 아닙니다.
- Android APK 빌드 설정은 아직 없습니다.
