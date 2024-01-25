using System.Collections.Generic;
using UnityEngine;

public class TargetFeedback : MonoBehaviour
{
    private MeshRenderer[] meshRenderers;

    public Material TransparentWhiteMaterial;

    public Material TransparentGreenMaterial;

    private Dictionary<Colors, Material> materials = new Dictionary<Colors, Material>();

    public enum Colors
    {
        White,
        Green
    }

    // Use this for initialization
    void Awake ()
    {
        this.materials.Add(Colors.White, TransparentWhiteMaterial);
        this.materials.Add(Colors.Green, TransparentGreenMaterial);

        meshRenderers = this.GetComponentsInChildren<MeshRenderer>();
    }
    
    public void SetMaterial(Colors material)
    {
        foreach (var meshRenderer in this.meshRenderers)
        {
            meshRenderer.material = this.materials[material];
        }
    }
}
