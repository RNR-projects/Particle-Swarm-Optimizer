using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResultsDataDisplay : MonoBehaviour
{
    public Text particleBasicsDisplay;
    public Text particleSpecificsDisplay;

    [SerializeField] private SwarmOptimizer swarm;

    // Update is called once per frame
    void Update()
    {
        Camera.main.transform.position = new Vector3(Mathf.Clamp(Camera.main.transform.position.x,
                0, OptimizationParameterManager.Instance().GetRoadLength() * GlobalScaler.Instance().GetGlobalScale()),
            Mathf.Clamp(Camera.main.transform.position.y, GlobalScaler.Instance().GetGlobalScale(), 30f *
                GlobalScaler.Instance().GetGlobalScale()),
            Mathf.Clamp(Camera.main.transform.position.z, 0, OptimizationParameterManager.Instance().GetRoadWidth() *
                GlobalScaler.Instance().GetGlobalScale()));

        Cursor.visible = true;

        SolutionParticle bestParticle = swarm.GetBestParticle();
        if (bestParticle != null)
        {
            particleBasicsDisplay.text = "Iteration " + swarm.GetIterationsDone() + 
                                        "\nHeight: " + bestParticle.height + 
                                        "\nSpacing: " + bestParticle.spacing;

            particleSpecificsDisplay.text = "Average: " + bestParticle.averageIlluminance + 
                                            "\nMin: " + bestParticle.lowestIlluminanceAtAPoint + 
                                            "\nEfficiency:" + bestParticle.lightingEfficiency + 
                                            "\nOffset:" + (bestParticle.xOffset + 
                                            OptimizationParameterManager.Instance().GetRoadLength() % bestParticle.spacing);	
        }
    }
}
