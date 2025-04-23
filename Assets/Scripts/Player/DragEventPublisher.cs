using System;
using Unity.Netcode;

public class DragEventPublisher : NetworkBehaviour
{
    public static event Action OnOwnerInitialized;
    public static event Action<float, float> OnDragChanged;
    public static event Action OnDragReleased;
    public static event Action OnTurnEnded;

    public void InitializeOwner()
    {
        if (!IsOwner) return;
        OnOwnerInitialized?.Invoke();
    }

    public void HandleDrag(float forcePercent, float angle)
    {
        if (!IsOwner) return;
        OnDragChanged?.Invoke(forcePercent, angle);
    }

    public void HandleStateChange(PlayerState state)
    {
        if (!IsOwner) return;
        if (state == PlayerState.DragReleaseItem || state == PlayerState.DragReleaseJump)
            OnDragReleased?.Invoke();
        else if (state == PlayerState.MyTurnEnded)
            OnTurnEnded?.Invoke();
    }
}
