using Sortify;
using UnityEngine;

public abstract class BaseItemComponent : MonoBehaviour
{
    //[BetterHeader("Base Item Component Settings")]
    protected bool isEnabled;

    public bool IsEnabled => isEnabled;

    /// <summary>
    /// Startup logic for the component. This is called when the component is enabled.
    /// </summary>
    protected abstract void OnEnableComponent();


    /// <summary>
    /// Start logic for the component. Call this to do the logic of the component - MUST BE ENABLED!
    /// </summary>
    public virtual void StartComponentLogic()
    {
        if (!isEnabled) return; //if the component is not enabled, do nothing

        DoComponentLogic();
    }

    /// <summary>
    /// Main logic for the component. 
    /// </summary>
    protected abstract void DoComponentLogic();

    public virtual void EnableComponent()
    {
        isEnabled = true;
        OnEnableComponent();
    }

    public virtual void DisableComponent()
    {
        isEnabled = false;
        OnDisableComponent();
    }


    /// <summary>
    /// Shutdown logic for the component. This is called when the component is disabled.
    /// </summary>
    protected abstract void OnDisableComponent();
}
