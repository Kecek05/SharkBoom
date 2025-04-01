using System;

public enum Map
{
    Default,
}

public enum GameMode
{
    Default,
}

public enum GameQueue
{
    Ranked,
    UnRanked,
}


[Serializable]
public class UserData
{
    //need to be public for the Json payload

    public string userName;
    public string userAuthId;

    public int userPearls; 

    public void SetUserPearls(int userPearls) => this.userPearls = userPearls;

    public GameInfo userGamePreferences = new();
}


[Serializable]
public class GameInfo
{

    public Map map;
    public GameMode gameMode;
    public GameQueue gameQueue;

    public string ToMultiplayQueue()
    {
        return gameQueue switch
        {
            GameQueue.Ranked => "solo-ranked-queue",
            GameQueue.UnRanked => "solo-unranked-queue",
            _ => "solo-ranked-queue",
        };
    }
}

[Serializable]
public class MatchmakingPlayerData
{
    public int pearls;
}
