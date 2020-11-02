using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IlluminationPointsCreator
{
    private static IlluminationPointsCreator sharedInstance;

    private GameObject illuminationPointPrefab;
    private List<GameObject> instantiatedObjects;
    private List<float> illuminationPointXCoords;
    private List<float> illuminationPointYCoords;
    private GameObject parentObject;
    public static IlluminationPointsCreator Instance()
    {
        if (sharedInstance == null)
            sharedInstance = new IlluminationPointsCreator();
        return sharedInstance;
    }

    private IlluminationPointsCreator()
    {
        this.illuminationPointXCoords = new List<float>();
        this.illuminationPointYCoords = new List<float>();
        this.instantiatedObjects = new List<GameObject>();   
    }

    public void RegisterIlluminationPoint(GameObject prefab)
    {
        this.illuminationPointPrefab = prefab;
    }

    private void CreateIlluminationPoint(Vector3 location)
    {
        GameObject gameObject = GameObject.Instantiate(this.illuminationPointPrefab, location, Quaternion.Euler(0, 0, 0));
        gameObject.transform.localScale = new Vector3(.02f * GlobalScaler.Instance().GetGlobalScale(), 
                                                        .02f * GlobalScaler.Instance().GetGlobalScale(), 
                                                        .15f * GlobalScaler.Instance().GetGlobalScale());
        if (this.parentObject == null)
            this.parentObject = GameObject.Find("Swarm");
        gameObject.transform.parent = this.parentObject.transform;

        this.instantiatedObjects.Add(gameObject);
    }

    public void InitializeIlluminationPoints()
    {
        int longCalPoints = OptimizationParameterManager.LONGCALPOINTS;
        float pointDistanceX = OptimizationParameterManager.Instance().GetRoadLength() / longCalPoints;
        float pointDistanceY = OptimizationParameterManager.Instance().GetRoadWidth() / 3;
        float pointEdgeX = pointDistanceX / 2;
        float pointEdgeY = pointDistanceY / 2;

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < longCalPoints; j++)
            {
                this.illuminationPointXCoords.Add(pointEdgeX + j * pointDistanceX);
                this.illuminationPointYCoords.Add(pointEdgeY + i * pointDistanceY);

                Vector3 location = new Vector3(this.illuminationPointXCoords[i * longCalPoints + j] * GlobalScaler.Instance().GetGlobalScale(),
                                                0,
                                                this.illuminationPointYCoords[i * longCalPoints + j] * GlobalScaler.Instance().GetGlobalScale());

                this.CreateIlluminationPoint(location);
            }
        }
    }

    public void ClearIlluminationPoints()
    {
        for (int i = 0; i < instantiatedObjects.Count; i++)
            GameObject.Destroy(instantiatedObjects[i]);
        this.instantiatedObjects.Clear();
        this.illuminationPointXCoords.Clear();
        this.illuminationPointYCoords.Clear();
    }

    public List<float> GetIlluminationPointXCoords()
    {
        return this.illuminationPointXCoords;
    }

    public List<float> GetIlluminationPointYCoords()
    {
        return this.illuminationPointYCoords;
    }

    public List<GameObject> GetIlluminationPoints()
    {
        return this.instantiatedObjects;
    }
}
