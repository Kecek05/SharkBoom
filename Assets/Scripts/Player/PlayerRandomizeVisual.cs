using UnityEngine;

public class PlayerRandomizeVisual : MonoBehaviour
{
    [SerializeField] private Mesh sharkMesh;
    [SerializeField] private Material sharkMaterial;

    [SerializeField] private Mesh orcaMesh;
    [SerializeField] private Material orcaMaterial;

    public Mesh SharkMesh => sharkMesh;
    public Material SharkMaterial => sharkMaterial;
    public Mesh OrcaMesh => orcaMesh;
    public Material OrcaMaterial => orcaMaterial;
}
