using QFSW.QC;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerRigReset : NetworkBehaviour
{
    [SerializeField] private BoneRenderer boneRenderes;

    [SerializeField] private Transform[] boneTransforms;
    [SerializeField] private Vector3[] boneInitialPosition;
    [SerializeField] private Quaternion[] boneInitialRotation;

    public override void OnNetworkSpawn()
    {
        if(IsOwner)
        {
            boneTransforms = boneRenderes.transforms;

            boneInitialPosition = new Vector3[boneTransforms.Length];
            boneInitialRotation = new Quaternion[boneTransforms.Length];

            for (int i = 0; i < boneTransforms.Length; i++)
            {
                boneInitialPosition[i] = boneTransforms[i].localPosition;
                boneInitialRotation[i] = boneTransforms[i].localRotation;
            }
        }
    }

    [Command("player-ResetRig", MonoTargetType.All)]
    public void ResetPose()
    {
        Debug.Log("Testing reset rig");

        for (int i = 0; i < boneTransforms.Length; i++)
        {
            boneTransforms[i].localPosition = boneInitialPosition[i];
            boneTransforms[i].localRotation = boneInitialRotation[i];
        }
    }

    [ClientRpc]
    public void ResetPoseClientRpc()
    {
        ResetPose();
    }
}
