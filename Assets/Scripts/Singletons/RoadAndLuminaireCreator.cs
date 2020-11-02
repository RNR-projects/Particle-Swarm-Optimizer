using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadAndLuminaireCreator
{
    private static RoadAndLuminaireCreator sharedInstance;

    private GameObject roadPrefab;
    private GameObject luminairePrefab;
    private GameObject instantiatedRoad;
    private List<GameObject> instantiatedLuminaires;
    private List<float> luminaireXCoords;
    private List<float> luminaireYCoords;
    private GameObject parentObject;

    public static RoadAndLuminaireCreator Instance()
    {
        if (sharedInstance == null)
            sharedInstance = new RoadAndLuminaireCreator();
        return sharedInstance;
    }

    private RoadAndLuminaireCreator()
    {
        instantiatedLuminaires = new List<GameObject>();
        luminaireXCoords = new List<float>();
        luminaireYCoords = new List<float>();
    }

    public void RegisterRoadPrefab(GameObject prefab)
    {
        this.roadPrefab = prefab;
    }

    public void RegisterLuminairePrefab(GameObject prefab)
    {
        this.luminairePrefab = prefab;
    }

    public void CreateRoad()
    {
        if (instantiatedRoad == null)
        {
            float scale = GlobalScaler.Instance().GetGlobalScale();
            float roadLength = OptimizationParameterManager.Instance().GetRoadLength();
            float roadWidth = OptimizationParameterManager.Instance().GetRoadWidth();

            GameObject road = GameObject.Instantiate(this.roadPrefab, new Vector3(roadLength / 2f * scale, -scale,
                roadWidth / 2f * scale), Quaternion.Euler(0, 0, 0));
            road.transform.localScale = new Vector3(roadLength / 10f * scale, scale, roadWidth / 10f * scale);
            if (this.parentObject == null)
                this.parentObject = GameObject.Find("Swarm");
            road.transform.parent = this.parentObject.transform;

            this.instantiatedRoad = road;
        }
        else
        {
            float scale = GlobalScaler.Instance().GetGlobalScale();
            float roadLength = OptimizationParameterManager.Instance().GetRoadLength();
            float roadWidth = OptimizationParameterManager.Instance().GetRoadWidth();

            instantiatedRoad.SetActive(true);
            instantiatedRoad.transform.position = new Vector3(roadLength / 2f * scale, -scale,
                roadWidth / 2f * scale);
            instantiatedRoad.transform.localScale = new Vector3(roadLength / 10f * scale, scale, roadWidth / 10f * scale);
        }
    }

    public void CreateLuminaires(SolutionParticle particle)
    {
        OptimizationParameterManager parameters = OptimizationParameterManager.Instance();
        float scale = GlobalScaler.Instance().GetGlobalScale();

        for (int i = 0; i < particle.luminaireCount; i++)
        {
            if (parameters.GetLuminaireArrangement() == OptimizationParameterManager.LuminaireArrangementSettings.Paired)
                this.luminaireXCoords.Add(particle.spacing * Mathf.Floor(i / 2) + (parameters.GetRoadLength() % particle.spacing) / 2f 
                    + particle.xOffset);
            else
                this.luminaireXCoords.Add(particle.spacing * i + (parameters.GetRoadLength() % particle.spacing) / 2f + particle.xOffset);
            if (parameters.GetLuminaireArrangement() == OptimizationParameterManager.LuminaireArrangementSettings.OneSided)
            {
                if (particle.isReversed)
                    this.luminaireYCoords.Add(parameters.GetRoadWidth());
                else
                    this.luminaireYCoords.Add(0);                
            }
            else
            {
                if (!particle.isReversed)
                {
                    if (i % 2 == 0)
                        this.luminaireYCoords.Add(0);
                    else
                        this.luminaireYCoords.Add(parameters.GetRoadWidth());
                }
                else
                {
                    if (i % 2 == 0)
                        this.luminaireYCoords.Add(parameters.GetRoadWidth());
                    else
                        this.luminaireYCoords.Add(0);
                }
            }

            if (instantiatedLuminaires.Count <= i)
            {
                GameObject lampPost = GameObject.Instantiate(this.luminairePrefab, new Vector3(this.luminaireXCoords[i] * scale,
                    (particle.height / 2f - 1f) * scale, this.luminaireYCoords[i] * scale),
                    Quaternion.Euler(0, 0, 0));
                lampPost.transform.localScale = new Vector3(.25f * scale, particle.height / 2f * scale, .25f * scale);
                if (this.parentObject == null)
                    this.parentObject = GameObject.Find("Swarm");
                lampPost.transform.parent = this.parentObject.transform;

                this.instantiatedLuminaires.Add(lampPost);
            }
            else
            {
                instantiatedLuminaires[i].SetActive(true);
                instantiatedLuminaires[i].transform.position = new Vector3(this.luminaireXCoords[i] * scale,
                    (particle.height / 2f - 1f) * scale, this.luminaireYCoords[i] * scale);
                instantiatedLuminaires[i].transform.localScale = new Vector3(.25f * scale, particle.height / 2f * scale, .25f * scale);
            }
        }
    }

    public List<GameObject> GetAllCreatedLuminaires()
    {
        return this.instantiatedLuminaires;
    }

    public List<float> GetLuminaireXPositions()
    {
        return this.luminaireXCoords;
    }
    
    public List<float> GetLuminaireYPositions()
    {
        return this.luminaireYCoords;
    }

    public void ClearLuminaires()
    {
        for (int i = 0; i < luminaireXCoords.Count; i++)
        {
            //GameObject.Destroy(instantiatedLuminaires[i]);
            instantiatedLuminaires[i].SetActive(false);
        }
        //this.instantiatedLuminaires.Clear();
        this.luminaireXCoords.Clear();
        this.luminaireYCoords.Clear();
    }

    public void ClearRoad()
    {
        //GameObject.Destroy(this.instantiatedRoad);
        //this.instantiatedRoad = null;
        if (instantiatedRoad != null)
            instantiatedRoad.SetActive(false);
    }
}
