using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceReferencesHub : MonoBehaviour
{
    public TextAsset azimuthFile, horizonAngleFile, solarGenerationFile, intensityFile;
    public GameObject roadPrefab, luminairePrefab, lightPointPrefab, structurePrefab;
    // Start is called before the first frame update
    void Awake()
    {
        IlluminanceCalculator.Instance().LoadIntensityFile(intensityFile);
        EnergyGenerationCalculator.Instance().LoadAzimuthFile(azimuthFile);
        EnergyGenerationCalculator.Instance().LoadHorizonAngleFile(horizonAngleFile);
        EnergyGenerationCalculator.Instance().LoadSolarGenerationFile(solarGenerationFile);
        RoadAndLuminaireCreator.Instance().RegisterLuminairePrefab(luminairePrefab);
        RoadAndLuminaireCreator.Instance().RegisterRoadPrefab(roadPrefab);
        IlluminationPointsCreator.Instance().RegisterIlluminationPoint(lightPointPrefab);
        StructureBuilder.Instance().RegisterStructurePrefab(structurePrefab);
    }
}
