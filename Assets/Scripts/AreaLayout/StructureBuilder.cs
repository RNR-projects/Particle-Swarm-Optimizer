using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureBuilder
{
    private static StructureBuilder sharedInstance;
    private GameObject parentObject;
    public static StructureBuilder Instance()
    {
        if (sharedInstance == null)
            sharedInstance = new StructureBuilder();
        return sharedInstance;
    }

    private StructureBuilder()
    {
        this.structures = new List<GameObject>();
    }
    
    private GameObject structurePrefab;
    private List<GameObject> structures;

    public void CreateStructures()
    {
        List<Vector3> locations = BuildingLocationsManager.Instance().GetBuildingLocations();
        List<Vector3> sizes = BuildingLocationsManager.Instance().GetBuildingSizes();

        for (int i = 0; i < locations.Count; i++)
        {
            GameObject newBuilding = GameObject.Instantiate(structurePrefab, locations[i], Quaternion.Euler(0, 0, 0));
            newBuilding.transform.localScale = sizes[i];
            if (this.parentObject == null)
                this.parentObject = GameObject.Find("Swarm");
            newBuilding.transform.parent = this.parentObject.transform;

            structures.Add(newBuilding);
        }
    }

    public void ClearStructures()
    {
        for (int i = 0; i < structures.Count; i++)
        {
            GameObject.Destroy(structures[i]);
        }
        this.structures.Clear();
    }

    public void RegisterStructurePrefab(GameObject prefab)
    {
        this.structurePrefab = prefab;
    }
}
