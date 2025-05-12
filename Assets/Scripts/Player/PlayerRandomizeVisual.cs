using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerRandomizeVisual : NetworkBehaviour
{
    [SerializeField] private Mesh sharkMesh;
    [SerializeField] private Material sharkMaterial;

    [SerializeField] private Mesh orcaMesh;
    [SerializeField] private Material orcaMaterial;

    [SerializeField] private SkinnedMeshRenderer meshRenderer;

    private NetworkVariable<PlayerVisualType> playerVisualType = new NetworkVariable<PlayerVisualType>(PlayerVisualType.Shark);


    public override void OnNetworkSpawn()
    {
        playerVisualType.OnValueChanged += HandleOnVisualChanged;
        HandleOnVisualChanged(PlayerVisualType.None, playerVisualType.Value);
    }

    private void HandleOnVisualChanged(PlayerVisualType previousValue, PlayerVisualType newValue)
    {
        ApplyVisual(newValue);
    }

    public void SetVisualNetworked(PlayerVisualType _type)
    {
        if (!IsServer) return;

        playerVisualType.Value = _type;
        SetVisualClientRpc(_type);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SetVisualClientRpc(PlayerVisualType type)
    {
        if (!IsOwner) return;

        ApplyVisual(type);
    }

    private void ApplyVisual(PlayerVisualType type)
    {
        if (meshRenderer == null)
        {
            return;
        }

        if (type == PlayerVisualType.Orca)
        {
            meshRenderer.sharedMesh = orcaMesh;
            meshRenderer.material = orcaMaterial;
        }
        else if(type == PlayerVisualType.Shark)
        {
            meshRenderer.sharedMesh = sharkMesh;
            meshRenderer.material = sharkMaterial;
        }

        meshRenderer.updateWhenOffscreen = true;
    }

    public override void OnNetworkDespawn()
    {
        playerVisualType.OnValueChanged -= HandleOnVisualChanged;
    }
}

public enum PlayerVisualType : byte
{
    None,
    Orca,
    Shark
}
