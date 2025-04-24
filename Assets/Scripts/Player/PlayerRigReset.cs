using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerRigReset : DragListener, IInitializeOnwer, IDetectEndedTurn
{
    [SerializeField] private BoneRenderer boneRenderes;

    [SerializeField] private Transform[] boneTransforms;
    [SerializeField] private Vector3[] boneInitialPosition;
    [SerializeField] private Quaternion[] boneInitialRotation;

    public void DoOnInitializeOnwer()
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


    public void ResetPose()
    {
        Debug.Log("reseting pose");
        for (int i = 0; i < boneTransforms.Length; i++) // Reset the pose of all bones
        {
            boneTransforms[i].localPosition = boneInitialPosition[i];
            boneTransforms[i].localRotation = boneInitialRotation[i];
        }
    }


    public void DoOnEndedTurn()
    {
        ResetPose();
    }

   
    //protected override void DoOnInitializeOnwer()
    //{
    //    Debug.Log("Testing Spawn");
    //    boneTransforms = boneRenderes.transforms;

    //    boneInitialPosition = new Vector3[boneTransforms.Length];
    //    boneInitialRotation = new Quaternion[boneTransforms.Length];

    //    for (int i = 0; i < boneTransforms.Length; i++) // Using the for because we need to save the initial position and rotation for all bones
    //    {
    //        boneInitialPosition[i] = boneTransforms[i].localPosition;
    //        boneInitialRotation[i] = boneTransforms[i].localRotation;
    //    }
    //}

    //protected override void DoOnDragChange(float forcePercent, float andlePercent)
    //{
    //    Debug.Log("Testing reset");
    //}

    //protected override void DoOnDragRelease()
    //{
    //    Debug.Log("Testing release");
    //}


    //protected override void DoOnEndedTurn()
    //{
    //    ResetPose();
    //}
}
