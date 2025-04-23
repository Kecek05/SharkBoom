using UnityEngine;

public class LocateOtherPlayer
{
    

    public static Vector2 GetOtherPlayerDirectionByPlayableState(PlayableState playableStateCalled)
    {
        BasePlayersPublicInfoManager playersPublicInfoManager = ServiceLocator.Get<BasePlayersPublicInfoManager>();

        


        return Vector2.down;
    }
}
