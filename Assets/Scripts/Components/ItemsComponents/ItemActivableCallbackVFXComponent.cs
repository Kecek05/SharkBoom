using UnityEngine;

public class ItemActivableCallbackVFXComponent : MonoBehaviour
{
    [SerializeField] private BaseItemThrowableActivable ItemThrowable;
    [SerializeField] private ParticleSystem particleSystemOnActivated;

    private void OnEnable()
    {
        ItemThrowable.OnItemActivated += BaseItemThrowableActivable_OnItemActivated;
    }

    private void BaseItemThrowableActivable_OnItemActivated()
    {
        particleSystemOnActivated.Play();
    }

    private void OnDisable()
    {
        ItemThrowable.OnItemActivated -= BaseItemThrowableActivable_OnItemActivated;
    }
}
