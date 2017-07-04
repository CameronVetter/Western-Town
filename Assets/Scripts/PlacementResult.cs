using HoloToolkit.Unity;
using UnityEngine;

public class PlacementResult
{
    public PlacementResult(SpatialUnderstandingDllObjectPlacement.ObjectPlacementResult result, Vector3 dimensions, ObjectType objType)
    {
        _result = result;
        Dimensions = dimensions;
        ObjType = objType;
    }

    public Vector3 Position { get { return _result.Position; } }
    public Vector3 Normal { get { return _result.Forward; } }
    public Vector3 Dimensions { get; private set; }
    public ObjectType ObjType { get; private set; }

    private readonly SpatialUnderstandingDllObjectPlacement.ObjectPlacementResult _result;
}