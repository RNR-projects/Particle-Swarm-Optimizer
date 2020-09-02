using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureBuilder : MonoBehaviour
{
    [SerializeField] private GameObject structurePrefab;
    private List<GameObject> structures;

    private void Awake()
    {
        this.structures = new List<GameObject>();
    }
    public void CreateStructures()
    {
        List<Vector3> locations = BuildingLocationsManager.Instance().GetBuildingLocations();
        List<Vector3> sizes = BuildingLocationsManager.Instance().GetBuildingSizes();

        for (int i = 0; i < locations.Count; i++)
        {
            GameObject newBuilding = Instantiate(structurePrefab, locations[i], Quaternion.Euler(0, 0, 0));
            newBuilding.transform.localScale = sizes[i];
            structures.Add(newBuilding);
        }
    }

    public void ClearStructures()
    {
        foreach (GameObject structure in this.structures)
        {
            Destroy(structure);
        }
        this.structures.Clear();
    }
}
