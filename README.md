# WaterGrow Prototype

물 키우기 MVP 개발용 Unity 2D 프로젝트 초안입니다.

## 현재 포함 범위

- `Assets` 기준 폴더 구조
- 6x4 머지 보드용 핵심 스크립트
- Lv.1 소환, 동일 레벨 2개 클릭 머지, Lv.5 제한
- 최고 등급 및 먼저 생성된 순서 기준 대표 출전 유닛 계산
- PlayerPrefs 기반 기본 저장 구조
- MVP 데이터 테이블 JSON 초안

## Unity에서 열기

1. Unity Hub에서 이 폴더를 프로젝트로 추가합니다.
2. Unity 2022.3 LTS 계열로 엽니다.
3. `MainScene`을 만든 뒤 Canvas, GridLayoutGroup, 24칸 Cell 또는 Cell prefab을 배치합니다.
4. `GameManager`, `SaveManager`, `BoardManager`, `UIManager`, `BattleManager`를 씬 오브젝트에 연결합니다.

## Prototype 0.1 조작

- 소환 버튼: 빈 칸에 Lv.1 물방울 생성
- 같은 Lv. 유닛 두 개를 차례로 클릭: 두 번째 칸에 Lv.+1 유닛 생성
- 최고 Lv. 유닛: 대표 출전 유닛으로 자동 선택
- 같은 최고 Lv.이 여러 개일 때: 먼저 생성된 유닛이 대표

