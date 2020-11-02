using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResultsDataDisplay : MonoBehaviour
{
    [SerializeField] private Text particleBasicsDisplay;
    [SerializeField] private Text particleSpecificsDisplay;

    [SerializeField] private Image progressBar;
    [SerializeField] private Text progressBarText;
    [SerializeField] private GameObject progressBarHolder;

    [SerializeField] private SwarmOptimizer swarm;

    private void OnDisable()
    {
        this.progressBarHolder.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        Camera.main.transform.position = new Vector3(Mathf.Clamp(Camera.main.transform.position.x,
                0, OptimizationParameterManager.Instance().GetRoadLength() * GlobalScaler.Instance().GetGlobalScale()),
            Mathf.Clamp(Camera.main.transform.position.y, GlobalScaler.Instance().GetGlobalScale(), 30f *
                GlobalScaler.Instance().GetGlobalScale()),
            Mathf.Clamp(Camera.main.transform.position.z, 0, OptimizationParameterManager.Instance().GetRoadWidth() *
                GlobalScaler.Instance().GetGlobalScale()));

        SolutionParticle bestParticle = swarm.GetBestParticle();

        if (progressBarHolder.activeSelf)
        {
            progressBar.fillAmount = (float)swarm.GetIterationsDone() / (float)OptimizationParameterManager.ITERATIONLIMIT;
            progressBarText.text = swarm.GetIterationsDone() + " / " + OptimizationParameterManager.ITERATIONLIMIT;
            if (swarm.optimizationIsDone)
            {
                progressBarText.text = "FINISHED";
                //ParticleSimulator.Instance().RepeatSimulatedParticle(bestParticle);
                StartCoroutine(this.RemoveBar());
            }

            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = true;

            particleBasicsDisplay.text = "Iteration " + swarm.GetIterationsDone() +
                                    "\nHeight: " + bestParticle.height +
                                    "\nSpacing: " + bestParticle.spacing;

            particleSpecificsDisplay.text = "Average: " + bestParticle.averageIlluminance +
                                            "\nMin: " + bestParticle.lowestIlluminanceAtAPoint +
                                            "\nEfficiency:" + bestParticle.lightingEfficiency +
                                            "\nOffset:" + (bestParticle.xOffset +
                                            OptimizationParameterManager.Instance().GetRoadLength() % bestParticle.spacing / 2f);
        }
    }

    private IEnumerator RemoveBar()
    {
        yield return new WaitForSeconds(0.1f);
        progressBarHolder.SetActive(false);
    }
}
