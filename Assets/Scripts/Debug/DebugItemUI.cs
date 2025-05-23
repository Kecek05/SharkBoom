using TMPro;
using UnityEngine;

public class DebugItemUI : MonoBehaviour
{
    [SerializeField] private BaseItemThrowable item;
    [SerializeField] private FollowTransformComponent followTransformComponent;
    [SerializeField] private LifetimeTriggerItemComponent lifetimeTriggerItemComponent;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private GameObject colliterGameObject;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI rbMode;
    [SerializeField] private TextMeshProUGUI followTransformActive;
    [SerializeField] private TextMeshProUGUI followTransformName;
    [SerializeField] private TextMeshProUGUI collisionLayer;
    [SerializeField] private TextMeshProUGUI lifetimeActive;
    [SerializeField] private TextMeshProUGUI itemReleased;
    [SerializeField] private TextMeshProUGUI rbVelocity;

    private void LateUpdate()
    {
        rbMode.text = "RB Mode: " + rb.bodyType.ToString();
        followTransformActive.text = "Follow Transform Active: " + followTransformComponent.IsActive.ToString();
        followTransformName.text = "Follow Transform Name: " + followTransformComponent.TargetTransform.ToString();
        collisionLayer.text = "Collision Layer: " + colliterGameObject.layer.ToString();
        //lifetimeActive.text = "Lifetime Active: " + lifetimeTriggerItemComponent.isActive.ToString();
        itemReleased.text = "Item Released: " + item.IsItemReleased.ToString();
        rbVelocity.text = "RB Velocity: " + rb.linearVelocity.ToString("F2");
    }
}
