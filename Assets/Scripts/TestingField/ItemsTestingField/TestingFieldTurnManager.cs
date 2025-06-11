using UnityEngine;

public class TestingFieldTurnManager : BaseTurnManager
{
    public override void HandleOnGameStateChanged(GameState newValue)
    {
        throw new System.NotImplementedException();
    }

    public override void HandleOnPlayableStateValueChanged(PlayableState previousValue, PlayableState newValue)
    {
        throw new System.NotImplementedException();
    }

    public override void HandleOnTimesUp()
    {
        throw new System.NotImplementedException();
    }

    public override void InitializeLocalStates(PlayableState playingState)
    {
        throw new System.NotImplementedException();
    }

    public override void PlayerJumped(PlayableState playableState)
    {
        throw new System.NotImplementedException();
    }

    public override void PlayerPlayed(PlayableState playerPlayingState)
    {
        throw new System.NotImplementedException();
    }

    public override void StartGame()
    {
        throw new System.NotImplementedException();
    }

    protected override void DelayChangeTurns(PlayableState playableState)
    {
        throw new System.NotImplementedException();
    }

    protected override void PlayerJumpedClient(PlayableState playableState)
    {
        throw new System.NotImplementedException();
    }

    protected override void SetPlayableStateServer(PlayableState newState)
    {
        throw new System.NotImplementedException();
    }
}
