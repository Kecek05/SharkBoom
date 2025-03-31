using UnityEngine;

public class SpawnPointComponent : MonoBehaviour
{
    
    void Start()
    {
        ServiceLocator.Get<BasePlayersPublicInfoManager>().SetRandomSpawnPoint(transform);
    }
}
