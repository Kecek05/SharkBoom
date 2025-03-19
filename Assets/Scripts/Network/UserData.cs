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

    public GameInfo userGamePreferences;
}


[Serializable]
public class GameInfo
{

    public Map map;
    public GameMode gameMode;
    public GameQueue gameQueue;

    public string ToMultiplayQueue()
    {
        return "";
    }
}
