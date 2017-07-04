using HoloToolkit.Unity;
using UnityEngine;

public class Speech : MonoBehaviour
{
    public SpatialUnderstandingCustomMesh SpatialUnderstandingMesh;

    private Material _swapMaterial;

    public void Start()
    {
        _swapMaterial = SpatialUnderstandingMesh.MeshMaterial;
    }

    public void ToggleMesh()
    {
        if (SpatialUnderstanding.Instance.ScanState != SpatialUnderstanding.ScanStates.Done) return;

        var otherMaterial = SpatialUnderstandingMesh.MeshMaterial;
        SpatialUnderstandingMesh.MeshMaterial = _swapMaterial;
        _swapMaterial = otherMaterial;
    }

}
