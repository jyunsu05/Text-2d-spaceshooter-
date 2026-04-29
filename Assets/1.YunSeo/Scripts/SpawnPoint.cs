using UnityEngine;

// 씬에서 SpawnPoint_0~8 오브젝트에 붙이는 위치 마커 스크립트
// 스폰 로직은 SpawnManager가 담당하므로 이 스크립트는 비워둠
public class SpawnPoint : MonoBehaviour
{
    public int GetSpawnPointIndex()
    {
        string[] splitName = gameObject.name.Split('_');
        if (splitName.Length < 2) return -1;
        if (int.TryParse(splitName[splitName.Length - 1], out int index)) return index;
        return -1;
    }
}
