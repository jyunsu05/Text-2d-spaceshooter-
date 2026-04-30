using UnityEngine;

// ──────────────────────────────────────────────────────────────
// SpawnPoint
// 씬에 있는 SpawnPoint_0 ~ SpawnPoint_8 오브젝트에 적용하는 위치 마커 스크립트
//
// 구현된 기능:
//   1. 오브젝트 이름(예: SpawnPoint_3)에서 스폰 인덱스 자동 파싱
//   2. 실제 소환 로직은 SpawnManager가 전담하므로
//      이 스크립트는 위치 정보를 제공하는 마커 역할만 수행
// ──────────────────────────────────────────────────────────────
public class SpawnPoint : MonoBehaviour
{
    // ──────────────────────────────────────────────────────────────
    // 오브젝트 이름에서 스폰 포인트 인덱스 추출
    // - 이름 형식: "SpawnPoint_N" (N = 0~8)
    // - SpawnManager에서 어떤 그룹에 속하는지 판단할 때 사용
    // - 파싱 실패 시 -1 반환
    // ──────────────────────────────────────────────────────────────

    public int GetSpawnPointIndex()
    {
        // 이름을 '_' 기준으로 스플릿
        string[] splitName = gameObject.name.Split('_');

        // 이름 형식이 잘못된 경우 -1 반환
        if (splitName.Length < 2) return -1;

        // 마지막 토큰을 정수로 뒤환 시도, 실패 시 -1 반환
        if (int.TryParse(splitName[splitName.Length - 1], out int index)) return index;
        return -1;
    }
}
