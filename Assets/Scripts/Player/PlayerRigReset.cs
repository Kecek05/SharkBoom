using QFSW.QC;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerRigReset : DragListener
{
    [SerializeField] private BoneRenderer boneRenderes;

    [SerializeField] private Transform[] boneTransforms;
    [SerializeField] private Vector3[] boneInitialPosition;
    [SerializeField] private Quaternion[] boneInitialRotation;

    

    protected override void DoOnSpawn()
    {
        if (IsOwner)
        {
            Debug.Log("Testing Spawn");
            boneTransforms = boneRenderes.transforms;

            boneInitialPosition = new Vector3[boneTransforms.Length];
            boneInitialRotation = new Quaternion[boneTransforms.Length];

            for (int i = 0; i < boneTransforms.Length; i++) // Using the for because we need to save the initial position and rotation for all bones
            {
                boneInitialPosition[i] = boneTransforms[i].localPosition;
                boneInitialRotation[i] = boneTransforms[i].localRotation;
            }
        }
    }

    protected override void DoOnDragChange(float forcePercent, float andlePercent)
    {
        
        Debug.Log("Testing reset");
    }

    protected override void DoOnDragRelease()
    {
        Debug.Log("Testing release");
        ResetPose();
    }

    public void ResetPose()
    {
        Debug.Log("reseting pose");
        for (int i = 0; i < boneTransforms.Length; i++) // Reset the pose of all bones
        {
            boneTransforms[i].localPosition = boneInitialPosition[i]; 
            boneTransforms[i].localRotation = boneInitialRotation[i];
        }
    }

}
