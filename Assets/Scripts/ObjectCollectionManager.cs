using System.Collections.Generic;
using HoloToolkit.Unity;
using UnityEngine;

public class ObjectCollectionManager : Singleton<ObjectCollectionManager>
{

    [Tooltip("A collection of square building prefabs to generate in the world.")]
    public List<GameObject> SquareBuildingPrefabs;

    [Tooltip("The desired size of square buildings in the world.")]
    public Vector3 SquareBuildingSize = new Vector3(.5f, .5f, .5f);

    [Tooltip("A collection of Wide building prefabs to generate in the world.")]
    public List<GameObject> WideBuildingPrefabs;

    [Tooltip("The desired size of wide buildings in the world.")]
    public Vector3 WideBuildingSize = new Vector3(1.0f, .5f, .5f);

    [Tooltip("A collection of tall building prefabs to generate in the world.")]
    public List<GameObject> TallBuildingPrefabs;

    [Tooltip("The desired size of tall buildings in the world.")]
    public Vector3 TallBuildingSize = new Vector3(.25f, .05f, .25f);

    [Tooltip("A collection of tree prefabs to generate in the world.")]
    public List<GameObject> TreePrefabs;

    [Tooltip("The desired size of trees in the world.")]
    public Vector3 TreeSize = new Vector3(.25f, .5f, .25f);

    [Tooltip("Will be calculated at runtime if is not preset.")]
    public float ScaleFactor;

    [Tooltip("Cowboy to Display.")]
    public GameObject Cowboy;

    [Tooltip("Cowboy desired Size.")]
    public Vector3 CowboySize;


    public List<GameObject> ActiveHolograms = new List<GameObject>();

    public void CreateSquareBuilding(int number, Vector3 positionCenter, Quaternion rotation)
    {
        CreateBuilding(SquareBuildingPrefabs[number], positionCenter, rotation, SquareBuildingSize);
    }

    public void CreateTallBuilding(int number, Vector3 positionCenter, Quaternion rotation)
    {
        CreateBuilding(TallBuildingPrefabs[number], positionCenter, rotation, TallBuildingSize);
    }

    public void CreateWideBuilding(int number, Vector3 positionCenter, Quaternion rotation)
    {
        CreateBuilding(WideBuildingPrefabs[number], positionCenter, rotation, WideBuildingSize);
    }

    private void CreateBuilding(GameObject buildingToCreate, Vector3 positionCenter, Quaternion rotation, Vector3 desiredSize)
    {
        // Stay center in the square but move down to the ground
        var position = positionCenter - new Vector3(0, desiredSize.y * .5f, 0);

        GameObject newObject = Instantiate(buildingToCreate, position, rotation);

        if (newObject != null)
        {
            // Set the parent of the new object the GameObject it was placed on
            newObject.transform.parent = gameObject.transform;

            newObject.transform.localScale = RescaleToSameScaleFactor(buildingToCreate);
            AddMeshColliderToAllChildren(newObject);
            ActiveHolograms.Add(newObject);
        }
    }

    public void CreateTree(int number, Vector3 positionCenter, Quaternion rotation)
    {
        // Stay center in the square but move down to the ground
        var position = positionCenter - new Vector3(0, TreeSize.y * .5f, 0);

        GameObject newObject = Instantiate(TreePrefabs[number], position, rotation);

        if (newObject != null)
        {
            // Set the parent of the new object the GameObject it was placed on
            newObject.transform.parent = gameObject.transform;

            newObject.transform.localScale = RescaleToSameScaleFactor(TreePrefabs[number]);
            newObject.AddComponent<MeshCollider>();
            ActiveHolograms.Add(newObject);
        }
    }

    public void CreateCowboy(Vector3 positionCenter, Quaternion rotation)
    {
        // Stay center in the square but move down to the ground
        var position = positionCenter - new Vector3(0, CowboySize.y * .5f, 0);

        GameObject newObject = Instantiate(Cowboy, position, rotation);

        if (newObject != null)
        {
            // Set the parent of the new object the GameObject it was placed on
            newObject.transform.parent = gameObject.transform;

            newObject.transform.localScale = RescaleToDesiredSizeProportional(Cowboy, CowboySize);

            newObject.AddComponent<Cowboy>();
            ActiveHolograms.Add(newObject);
        }
    }

    private void AddMeshColliderToAllChildren(GameObject obj)
    {
        for (int i = 0; i < obj.transform.childCount; i++)
        {
            obj.transform.GetChild(i).gameObject.AddComponent<MeshCollider>();
        }
    }

    private Vector3 RescaleToSameScaleFactor(GameObject objectToScale)
    {
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (ScaleFactor == 0f)
        {
            CalculateScaleFactor();
        }

        return objectToScale.transform.localScale * ScaleFactor;
    }

    private Vector3 RescaleToDesiredSizeProportional(GameObject objectToScale, Vector3 desiredSize)
    {
        float scaleFactor = CalcScaleFactorHelper(new List<GameObject> { objectToScale }, desiredSize);

        return objectToScale.transform.localScale * scaleFactor;
    }

    private Vector3 StretchToFit(GameObject obj, Vector3 desiredSize)
    {
        var curBounds = GetBoundsForAllChildren(obj).size;

        return new Vector3(desiredSize.x / curBounds.x / 2, desiredSize.y, desiredSize.z / curBounds.z / 2);
    }

    private void CalculateScaleFactor()
    {
        float maxScale = float.MaxValue;

        var ratio = CalcScaleFactorHelper(WideBuildingPrefabs, WideBuildingSize);
        if (ratio < maxScale)
        {
            maxScale = ratio;
        }

        ScaleFactor = maxScale;
    }

    private float CalcScaleFactorHelper(List<GameObject> objects, Vector3 desiredSize)
    {
        float maxScale = float.MaxValue;

        foreach (var obj in objects)
        {
            var curBounds = GetBoundsForAllChildren(obj).size;
            var difference = curBounds - desiredSize;

            float ratio;

            if (difference.x > difference.y && difference.x > difference.z)
            {
                ratio = desiredSize.x / curBounds.x;
            }
            else if (difference.y > difference.x && difference.y > difference.z)
            {
                ratio = desiredSize.y / curBounds.y;
            }
            else
            {
                ratio = desiredSize.z / curBounds.z;
            }

            if (ratio < maxScale)
            {
                maxScale = ratio;
            }
        }

        return maxScale;
    }

    private Bounds GetBoundsForAllChildren(GameObject findMyBounds)
    {
        Bounds result = new Bounds(Vector3.zero, Vector3.zero);

        foreach (var curRenderer in findMyBounds.GetComponentsInChildren<Renderer>())
        {
            if (result.extents == Vector3.zero)
            {
                result = curRenderer.bounds;
            }
            else
            {
                result.Encapsulate(curRenderer.bounds);
            }
        }

        return result;
    }
}
