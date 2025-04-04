
public class OwnershipHandler : IOwnershipHandler
{
    public bool IsReconnecting(UserData userData, ulong clientId)
    {
        if(NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.GetAuthIdByClientId(clientId) != null)
        {
            //Player already registered
            return true;
        } else
        {
            //New Player
            return false;
        }
    }

    public void HandleOwnership(ulong clientId, ulong newOwnerClientId)
    {
        
    }

    public void HandlePlayerObjectOwnership(ulong clientId, ulong newOwnerClientId)
    {
        
    }
}
