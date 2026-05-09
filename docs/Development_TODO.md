# Development TODO

## High

- MainScene 구성
  - 완료 기준: Unity에서 Play Mode 진입 시 보드, 소환 버튼, 대표 유닛 텍스트, 전투 영역이 표시된다.
- BoardCell prefab 생성
  - 완료 기준: 24칸이 B01~B24로 표시되고 클릭 이벤트가 정상 동작한다.
- Enemy prefab 생성
  - 완료 기준: 불꽃 병사가 눈에 보이는 placeholder 이미지와 HP Bar를 가진다.
- JSON 데이터 로더 구현
  - 완료 기준: UnitTable, EnemyTable, WaveTable 값을 코드 임시값 대신 런타임에서 사용한다.
  - 현재 상태: UnitTable, EnemyTable은 `Resources/Data` 자동 로딩 완료. WaveTable 로딩은 남음.
- Unity 컴파일 검증
  - 완료 기준: Console에 compile error가 없다.
  - 현재 상태: Unity 2022.3.62f3 batchmode 컴파일 통과.
- Prototype 0.2 플레이 테스트
  - 완료 기준: 소환, 머지, 대표 갱신, 스폰, 자동 공격, 처치, 클리어, 실패가 한 씬에서 확인된다.

## Medium

- Projectile 기반 공격 처리
  - 완료 기준: 대표 유닛 공격 시 placeholder 투사체가 적에게 이동한 뒤 피해를 준다.
- BattleHUD 정리
  - 완료 기준: 현재 스테이지, 남은 적 수, baseHp, 골드, 대표 Lv.이 모바일 세로 화면에서 읽기 좋게 표시된다.
- Stage 1-1 밸런싱
  - 완료 기준: Lv.1~Lv.2 대표 유닛으로 기본 루프를 확인할 수 있고, 실패/클리어 모두 재현 가능하다.
- 저장 범위 확장
  - 완료 기준: currentStageId, highestClearedStageId, boardState, gold가 재실행 후 복원된다.
- 간단 튜토리얼
  - 완료 기준: 첫 소환, 첫 머지, 대표 출전 안내가 전투 흐름을 크게 막지 않고 표시된다.

## Low

- 기본 사운드 연결
  - 완료 기준: 소환, 머지, 공격, 처치에 placeholder SFX가 연결된다.
- 기본 이펙트 연결
  - 완료 기준: 머지와 적 처치 시 간단한 particle 또는 UI feedback이 표시된다.
- Stage 1-2~1-10 데이터 확장
  - 완료 기준: StageTable/WaveTable 기준으로 10개 스테이지가 순차 진행된다.
- 보스 placeholder 구현
  - 완료 기준: Stage 1-10에서 일반 적과 구분되는 보스 1종이 등장한다.
- Android 빌드 설정
  - 완료 기준: 세로 화면 APK 빌드가 생성된다.
