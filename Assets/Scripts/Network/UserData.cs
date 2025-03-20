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

    public string userName;
    public string userAuthId;

    public int userPearls;

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
