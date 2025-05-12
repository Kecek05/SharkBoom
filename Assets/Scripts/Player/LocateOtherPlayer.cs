using UnityEngine;

public class LocateOtherPlayer
{
    

    public static Vector2 GetOtherPlayerDirectionNormalizedByPlayableState(PlayableState playableStateCalled)
    {
        BasePlayersPublicInfoManager playersPublicInfoManager = ServiceLocator.Get<BasePlayersPublicInfoManager>();

        Transform sender = playersPublicInfoManager.GetPlayerObjectByPlayableState(playableStateCalled).transform;

        Transform reciever = playersPublicInfoManager.GetOtherPlayerByMyPlayableState(playableStateCalled).transform;

        if (reciever == null)
        {
            Debug.LogWarning("Not found the other player");
            return Vector2.zero;
        }

        Vector2 recieverDirection = reciever.position - sender.position;
        recieverDirection.Normalize();
        return recieverDirection;

    }

    public static bool OtherPlayerIsOnMyRight(PlayableState playableStateCalled)
    {
        Vector2 otherPlayerDirection = GetOtherPlayerDirectionNormalizedByPlayableState(playableStateCalled);
        return otherPlayerDirection.x > 0;
    }
}
