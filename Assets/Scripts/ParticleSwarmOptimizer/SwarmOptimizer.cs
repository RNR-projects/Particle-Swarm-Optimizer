using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwarmOptimizer : MonoBehaviour {
    public int particleCount, iterations;
    private float[] index,
	tempAverages, tempMinimums, tempEfficiencies, tempDevAverage, tempDevMinimum, tempScore;
    private float c1 = 0.75f, c2 = 0.75f, r1, r2, bestSpacing, bestHeight;
    public float lowestAverageIlluminance, highestAverageilluminance, minimumIlluminance, roadLength, roadWidth, heightLimit, luminaireLimit, finalSpacing, finalHeight, minScore;
    private LightCalculator lightCalculator;
    public Text text;
	public GameObject road, lightPoint;
	private int currentIteration = 0;
	public Camera cam;
	private bool started = false, buildabl = false;
	public bool isBest, hBool1, hBool2, hBool3, sBool1, sBool2, sBool3, arrangement1 = true, arrangement2 = false, arrangement3 = false;
	public int longCalPoints;
	[HideInInspector] public float pointDistanceX, pointDistanceY, pointEdgeX, pointEdgeY;
	private int it, check;
	private int[] candidateIndex;
	public GridCreator grid;
	private List<SolutionParticle> particles;

	void Start() {
		grid = GameObject.Find ("Grid").GetComponent<GridCreator> ();
		isBest = false;
	}

	public void start () {
		check = 0;
		finalSpacing = 0;
		finalHeight = 0;

		currentIteration = 0;
		GameObject[] fin = GameObject.FindGameObjectsWithTag ("Finish");
		for (int i = 0; i < fin.Length; i++)
			Destroy (fin [i]);

		RoadAndLuminaireCreator.Instance().CreateRoad();

		IlluminationPointsCreator.Instance().InitializeIlluminationPoints();

        r1 = Random.value;
        r2 = Random.value;
        text = GetComponent<Text>();
        tempAverages = new float[particleCount];
        tempDevAverage = new float[particleCount];
        tempDevMinimum = new float[particleCount];
        tempEfficiencies = new float[particleCount];
        tempMinimums = new float[particleCount];
		tempScore = new float[particleCount];
		particles = ParticleGenerator.Instance().InitializeNewParticles(particleCount);
        StartCoroutine("BeginFirstIteration");
	}
	

	void Update () {
		cam.transform.position = new Vector3(Mathf.Clamp (cam.transform.position.x, 0, roadLength * GlobalScaler.Instance().GetGlobalScale()), 
			Mathf.Clamp (cam.transform.position.y, GlobalScaler.Instance().GetGlobalScale(), 30f * GlobalScaler.Instance().GetGlobalScale()), Mathf.Clamp (cam.transform.position.z, 0, roadWidth * GlobalScaler.Instance().GetGlobalScale()));
		if (cam.enabled && !started) {
			start ();
			started = true;
		}
		if (!cam.enabled)
			started = false;
		text.text = "Iteration " + currentIteration + "\nHeight: " + bestHeight + "\nSpacing: " + bestSpacing;
    }

	public void ClearData()
    {
        GameObject[] gameObject = GameObject.FindGameObjectsWithTag("Respawn");
		for (int i = 0; i < gameObject.Length; i++) {
			BoxCollider col = gameObject [i].GetComponent<BoxCollider> ();
			if (col != null)
				col.enabled = false;
			Destroy (gameObject [i]);
		}
    }

    IEnumerator BeginFirstIteration()
    {
        LightCalculator lightCalculator = this.transform.GetComponentInParent<LightCalculator>();
        for (int i = 0; i < particleCount; i++)
        {
            lightCalculator.spacing = particles[i].spacing;
            lightCalculator.height = particles[i].height;
			ClearData ();
            lightCalculator.CalcIlluminance(particles[i]);
			particles[i].spacing = lightCalculator.spacing;
			particles[i].isBuildable = lightCalculator.buildable;
            particles[i].averageIlluminance = (float)lightCalculator.averageIlluminance;
            particles[i].lowestIlluminanceAtAPoint = lightCalculator.minIlluminance;
			particles[i].lowestEnergyGeneratedByALuminaire = lightCalculator.score;
            particles[i].lightingEfficiency = lightCalculator.lightingEfficiency;
            if (particles[i].averageIlluminance >= lowestAverageIlluminance && particles[i].averageIlluminance <= highestAverageilluminance)
                particles[i].deviationFromAverageIlluminanceLimit = 0;
            else if (particles[i].averageIlluminance < lowestAverageIlluminance)
                particles[i].deviationFromAverageIlluminanceLimit = lowestAverageIlluminance - particles[i].averageIlluminance;
            else
                particles[i].deviationFromAverageIlluminanceLimit = particles[i].averageIlluminance - highestAverageilluminance;
            if (particles[i].lowestIlluminanceAtAPoint >= minimumIlluminance)
                particles[i].deviationFromMinimumIlluminanceAtAPoint = 0;
            else
                particles[i].deviationFromMinimumIlluminanceAtAPoint = minimumIlluminance - particles[i].lowestIlluminanceAtAPoint;
        }
        SetBest();
		isBest = true;
		lightCalculator.spacing = bestSpacing;
		lightCalculator.height = bestHeight;
		ClearData();
		lightCalculator.CalcIlluminance(particles[0]);
        StartCoroutine("AdjustData");
		yield return new WaitForSeconds (.01f);
    }

    IEnumerator AdjustData()
    {
        LightCalculator lightCalculator = this.transform.GetComponentInParent<LightCalculator>();
        for (int i = 0; i < iterations; i++)
        {
            for (int j = 0; j < particleCount; j++)
            {
				particles[j].spacingVelocity = CalcVelocity(particles[j].spacingVelocity, particles[j].closestSpacing, particles[j].spacing, bestSpacing);
				particles[j].heightVelocity = CalcVelocity(particles[j].heightVelocity, particles[j].closestHeight, particles[j].height, bestHeight);

				if (sBool1 && particles[j].spacingVelocity + particles[j].spacing < roadLength / (luminaireLimit - 1)) 
				{
					if (sBool2)
						particles[j].spacing = roadLength / (luminaireLimit - 1f);
					else
						particles[j].spacing = roadLength / (luminaireLimit - 1f) + .5f;
				} 
				else if (sBool3 && (particles[j].spacingVelocity + particles[j].spacing > roadLength / luminaireLimit || particles[j].spacingVelocity + particles[j].spacing <= 5)) 
				{
					if (particles[j].spacingVelocity + particles[j].spacing <= 5)
						particles[j].spacing = 6f;
					else if (sBool2)
						particles[j].spacing = roadLength / luminaireLimit;
					else
						particles[j].spacing = roadLength / luminaireLimit - .5f;
				} 
				else if (particles[j].spacingVelocity + particles[j].spacing > roadLength)
					particles[j].spacing = roadLength;
				else if (sBool2 && (particles[j].spacingVelocity + particles[j].spacing > roadLength / (luminaireLimit - 1f) || particles[j].spacingVelocity + particles[j].spacing < roadLength / luminaireLimit)) 
				{
					if (particles[j].spacingVelocity + particles[j].spacing > roadLength / (luminaireLimit - 1f))
						particles[j].spacing = roadLength / (luminaireLimit - 1f);
					else
						particles[j].spacing = roadLength / luminaireLimit;
				}
                else
					particles[j].spacing += particles[j].spacingVelocity;
				if (particles[j].heightVelocity + particles[j].height < 6f)
					particles[j].height = 6f;
				else if(hBool1 && particles[j].heightVelocity + particles[j].height > heightLimit) 
				{
					if(hBool2)
						particles[j].height = heightLimit;
					else
						particles[j].height = heightLimit - .5f;
				} 
				else if (hBool3 && particles[j].heightVelocity + particles[j].height < heightLimit) 
				{
					if(hBool2)
						particles[j].height = heightLimit;
					else
						particles[j].height = heightLimit + .5f;
				} 
				else if(hBool2)
					particles[j].height = heightLimit;
                else
                    particles[j].height += particles[j].heightVelocity;
            }
            for (int j = 0; j < particleCount; j++)
            {
                lightCalculator.spacing = particles[j].spacing;
                lightCalculator.height = particles[j].height;
				ClearData ();
                lightCalculator.CalcIlluminance(particles[j]);
				particles[j].spacing = lightCalculator.spacing;
				particles[j].isBuildable = lightCalculator.buildable;
                tempAverages[j] = (float)lightCalculator.averageIlluminance;
                tempMinimums[j] = lightCalculator.minIlluminance;
				tempScore [j] = lightCalculator.score;
                tempEfficiencies[j] = lightCalculator.lightingEfficiency;
                if (tempAverages[j] >= lowestAverageIlluminance && tempAverages[j] <= highestAverageilluminance)
                    tempDevAverage[j] = 0;
                else if (tempAverages[j] < lowestAverageIlluminance)
                    tempDevAverage[j] = lowestAverageIlluminance - tempAverages[j];
                else
                    tempDevAverage[j] = tempAverages[j] - highestAverageilluminance;
                if (tempMinimums[j] >= minimumIlluminance)
                    tempDevMinimum[j] = 0;
                else
                    tempDevMinimum[j] = minimumIlluminance - tempMinimums[j];
            }
            for (int j = 0; j < particleCount; j++)
            {
                if (particles[j].spacing >= 0 && particles[j].height >= 0)
                {
                    if (tempDevAverage[j] < particles[j].deviationFromAverageIlluminanceLimit)
                        AdjustOptimal(j);
                    else if (tempDevAverage[j] == particles[j].deviationFromAverageIlluminanceLimit)
                    {
						if (tempDevMinimum [j] < particles[j].deviationFromMinimumIlluminanceAtAPoint)
							AdjustOptimal (j);
						else if (tempDevMinimum [j] == particles[j].deviationFromMinimumIlluminanceAtAPoint) {
							if (tempEfficiencies [j] > particles[j].lightingEfficiency)
								AdjustOptimal (j);
							else if (tempEfficiencies [j] <= particles[j].lightingEfficiency && tempScore [j] < particles[j].lightingEfficiency)
								AdjustOptimal (j);
						}
                    }
                }
            }
			it = i;
            SetBest();
			ClearData();
			isBest = true;
			lightCalculator.spacing = bestSpacing;
			lightCalculator.height = bestHeight;
			lightCalculator.CalcIlluminance (particles[i]);
			if (check > 9) {
				StopCoroutine ("AdjustData");
				if (!buildabl)
					start ();
			}
			if (it + 1 == iterations) {
				if (!buildabl)
					start ();
			}
			currentIteration++;
			yield return new WaitForSeconds (.01f);
        }
    }

    private void AdjustOptimal(int index)
    {
        particles[index].closestSpacing = particles[index].spacing;
        particles[index].closestHeight = particles[index].height;
        particles[index].averageIlluminance = tempAverages[index];
        particles[index].lowestIlluminanceAtAPoint = tempMinimums[index];
        particles[index].lightingEfficiency = tempEfficiencies[index];
        particles[index].deviationFromAverageIlluminanceLimit = tempDevAverage[index];
        particles[index].deviationFromMinimumIlluminanceAtAPoint = tempDevMinimum[index];
		particles[index].lightingEfficiency = tempScore [index];
    }

    private void SetBest()
    {
        float[] bestCandidates1 = new float[particleCount];
        int[] foundIndex1 = new int[particleCount];
        int ind1 = 0;

        for (int i = 0; i < particleCount; i++)
        {
			if (particles[i].deviationFromAverageIlluminanceLimit == Mathf.Min(devAverage) && particles[i].isBuildable)
            {
                bestCandidates1[ind1] = particles[i].deviationFromMinimumIlluminanceAtAPoint;
                foundIndex1[ind1] = i;
                ind1++;
            }
        }
        float[] bestCandidates2 = new float[ind1];
        float[] bestCandidates3 = new float[ind1];
		float[] bestCandidates31 = new float[ind1];
		int[] foundIndex3 = new int[ind1];
        int[] foundIndex2 = new int[ind1];
        int ind2 = 0;
		int ind3 = 0;
        for (int i = 0; i < ind1; i++)
        {
            bestCandidates2[i] = bestCandidates1[i];
        }
        for (int i = 0; i < ind1; i++)
        {
            if (bestCandidates2[i] == Mathf.Min(bestCandidates2))
            {
                bestCandidates3[ind2] = particles[foundIndex1[i]].lightingEfficiency;
				bestCandidates31 [ind2] = particles[foundIndex1 [i]].lightingEfficiency;
                foundIndex2[ind2] = foundIndex1[i];
                ind2++;
            }
        }
		float[] bestCandidates4 = new float[ind2];
		for (int i = 0; i < ind2; i++) {
			if (bestCandidates31 [i] >= minScore) {
				bestCandidates4 [ind3] = particles[foundIndex2 [i]].lightingEfficiency;
				foundIndex3 [ind3] = foundIndex2 [i];
				ind3++;
			}
		}
		int index = 0;
		if (ind3 == 0) {
			if (ind2 == 0) {
				if (ind1 == 0)
					index = System.Array.IndexOf (devAverage, Mathf.Min (devAverage));
				else
					index = foundIndex1 [System.Array.IndexOf (bestCandidates2, Mathf.Min (bestCandidates2))];
			}
			else
			index = foundIndex2 [System.Array.IndexOf (bestCandidates3, Mathf.Max (bestCandidates3))];
		}
		else
			index = foundIndex3[System.Array.IndexOf(bestCandidates4, Mathf.Max(bestCandidates4))];
        bestSpacing = particles[index].spacing;
        bestHeight = particles[index].height;
		buildabl = particles[index].isBuildable;
		if (it > Mathf.FloorToInt (iterations / 2.5f)) {
			if (finalSpacing <= bestSpacing + .5f && finalSpacing >= bestSpacing - .5f && finalHeight <= bestHeight + .5f && finalHeight >= bestHeight - .5f)
				check++;
			else {
				check = 0;
				finalSpacing = bestSpacing;
				finalHeight = bestHeight;
			}
		}
        particles[index].spacingVelocity = 0;
        particles[index].heightVelocity = 0;
    }
    
    private float CalcVelocity(float velocity,float closest, float position, float best){
        return velocity + c1 * r1 * (closest - position) + c2 * r2 * (best - position);
    }
}
