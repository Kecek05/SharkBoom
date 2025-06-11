using Unity.Netcode;
using UnityEngine;

public class ItemTestingNotNetworked : MonoBehaviour
{
    [SerializeField] private Rigidbody rigidbody;
    
    public void ItemReleased(ItemLauncherData itemLauncherData)
    {
       // UpdateOnRelease(itemLauncherData);

        //followTransformComponent.DisableComponent();
        //turnManager = ServiceLocator.Get<BaseTurnManager>();
        rigidbody.AddForce(itemLauncherData.dragDirection * itemLauncherData.dragForce, ForceMode.Impulse);
    }
}
