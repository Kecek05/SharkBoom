
public class FollowSelectedSocketComponent : FollowTransformComponent
{
    public void HandleOnPlayerSpawnItemOnHandOnItemSocketSelected(ItemSocket selectedSocket)
    {
        SetTarget(selectedSocket.TrajectoryTransform.transform);
        EnableComponent();
    }
}
