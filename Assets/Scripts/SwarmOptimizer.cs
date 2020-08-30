using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwarmOptimizer : MonoBehaviour {
    public int particles, iterations;
    private float[] spacing, height, Xvelocity, Yvelocity, Averages, Minimums, Efficiencies, devAverage, devMinimum, index,
	tempAverages, tempMinimums, tempEfficiencies, tempDevAverage, tempDevMinimum, closestSpacing, closestHeight, Score, tempScore;
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
	[HideInInspector] public float[] lumPointsX, lumPointsY, pointIlluminance;
	private int lumIndex, it, check, inx;
	private int[] candidateIndex;
	public GameObject[] calPoints;
	public bool[] canBuild;
	public GridCreator grid;

	void Start() {
		grid = GameObject.Find ("Grid").GetComponent<GridCreator> ();
		isBest = false;
	}

	public void start () {
		check = 0;
		finalSpacing = 0;
		finalHeight = 0;
		longCalPoints = (int) 15;
		pointDistanceX = roadLength / longCalPoints;
		pointDistanceY = roadWidth / 3;
		pointEdgeX = pointDistanceX / 2;
		pointEdgeY = pointDistanceY / 2;
		lumPointsX = new float[longCalPoints * 3];
		lumPointsY = new float[longCalPoints * 3];
		pointIlluminance = new float[longCalPoints * 3];
		for (int i = 0; i < 3; i++) 
		{
			for (int j = 0; j < longCalPoints; j++) 
			{
				lumIndex = i * longCalPoints + j;
				lumPointsX [lumIndex] = pointEdgeX + j * pointDistanceX;
				lumPointsY [lumIndex] = pointEdgeY + i * pointDistanceY;
			}
		}
		if (luminaireLimit < 1f)
			luminaireLimit = 1f;
		if (heightLimit < 6f)
			heightLimit = 6f;
		currentIteration = 0;
		GameObject[] fin = GameObject.FindGameObjectsWithTag ("Finish");
		for (int i = 0; i < fin.Length; i++)
			Destroy (fin [i]);
		GameObject Road = Instantiate(road, new Vector3(roadLength / 2f * GlobalScaler.Instance().GetGlobalScale(), -GlobalScaler.Instance().GetGlobalScale(), roadWidth / 2f * GlobalScaler.Instance().GetGlobalScale()), new Quaternion(0, 0, 0, 0));
		Road.transform.localScale = new Vector3(roadLength / 10f * GlobalScaler.Instance().GetGlobalScale(), GlobalScaler.Instance().GetGlobalScale(), roadWidth / 10f * GlobalScaler.Instance().GetGlobalScale());
		calPoints = new GameObject[longCalPoints * 3];
		for (int i = 0; i < longCalPoints * 3; i++) {
			calPoints [i] = Instantiate (lightPoint, new Vector3 (lumPointsX [i] * GlobalScaler.Instance().GetGlobalScale(), 0, lumPointsY [i] * GlobalScaler.Instance().GetGlobalScale()), new Quaternion (0, 0, 0, 0));
			calPoints[i].transform.localScale = new Vector3 (.02f * GlobalScaler.Instance().GetGlobalScale(), .02f * GlobalScaler.Instance().GetGlobalScale(), .15f * GlobalScaler.Instance().GetGlobalScale());
		}
        r1 = Random.value;
        r2 = Random.value;
        text = GetComponent<Text>();
        Xvelocity = new float[particles];
        Yvelocity = new float[particles];
        LightCalculator lightCalculator = this.transform.GetComponentInParent<LightCalculator>();
		lightCalculator.roadWidth = roadWidth;
        spacing = new float[particles];
        height = new float[particles];
        closestSpacing = new float[particles];
        closestHeight = new float[particles];
        Averages = new float[particles];
        Minimums = new float[particles];
        devAverage = new float[particles];
        devMinimum = new float[particles];
        Efficiencies = new float[particles];
        tempAverages = new float[particles];
        tempDevAverage = new float[particles];
        tempDevMinimum = new float[particles];
        tempEfficiencies = new float[particles];
        tempMinimums = new float[particles];
		Score = new float[particles];
		tempScore = new float[particles];
		canBuild = new bool[particles];
		for (int i = 0; i < particles; i++)
        {
            Xvelocity[i] = 0;
            Yvelocity[i] = 0;
			if (sBool1)
				spacing [i] = (float)Random.Range (roadLength / (luminaireLimit - 1f), roadLength);
			else if (sBool2) 
				spacing [i] = (float)Random.Range (roadLength / luminaireLimit, roadLength / (luminaireLimit - 1f));
			else if (sBool3)
				spacing [i] = (float)Random.Range (5f, roadLength / luminaireLimit);
			if (hBool1)
				height [i] = (float)Random.Range (6f, heightLimit);
			else if (hBool2)
				height [i] = heightLimit;
			else if (hBool3)
				height[i] = (float)Random.Range(heightLimit, 20f);
            closestSpacing[i] = spacing[i];
            closestHeight[i] = height[i];
        }
        StartCoroutine("ActivateSwarm1");
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

    IEnumerator ActivateSwarm1()
    {
        LightCalculator lightCalculator = this.transform.GetComponentInParent<LightCalculator>();
        for (int i = 0; i < particles; i++)
        {
            lightCalculator.spacing = spacing[i];
            lightCalculator.height = height[i];
			ClearData ();
            lightCalculator.CalcIlluminance();
			spacing [i] = lightCalculator.spacing;
			canBuild [i] = lightCalculator.buildable;
            Averages[i] = (float)lightCalculator.averageIlluminance;
            Minimums[i] = lightCalculator.minIlluminance;
			Score [i] = lightCalculator.score;
            Efficiencies[i] = lightCalculator.lightingEfficiency;
            if (Averages[i] >= lowestAverageIlluminance && Averages[i] <= highestAverageilluminance)
                devAverage[i] = 0;
            else if (Averages[i] < lowestAverageIlluminance)
                devAverage[i] = lowestAverageIlluminance - Averages[i];
            else
                devAverage[i] = Averages[i] - highestAverageilluminance;
            if (Minimums[i] >= minimumIlluminance)
                devMinimum[i] = 0;
            else
                devMinimum[i] = minimumIlluminance - Minimums[i];
        }
        SetBest();
		isBest = true;
		lightCalculator.spacing = bestSpacing;
		lightCalculator.height = bestHeight;
		ClearData();
		lightCalculator.CalcIlluminance();
        StartCoroutine("AdjustData");
		yield return new WaitForSeconds (.01f);
    }

    IEnumerator AdjustData()
    {
        LightCalculator lightCalculator = this.transform.GetComponentInParent<LightCalculator>();
        for (int i = 0; i < iterations; i++)
        {
            for (int j = 0; j < particles; j++)
            {
				Xvelocity[j] = CalcVelocity(Xvelocity[j], closestSpacing[j], spacing[j], bestSpacing);
				Yvelocity[j] = CalcVelocity(Yvelocity[j], closestHeight[j], height[j], bestHeight);

				if (sBool1 && Xvelocity [j] + spacing [j] < roadLength / (luminaireLimit - 1)) 
				{
					if (sBool2)
						spacing [j] = roadLength / (luminaireLimit - 1f);
					else
						spacing [j] = roadLength / (luminaireLimit - 1f) + .5f;
				} 
				else if (sBool3 && (Xvelocity [j] + spacing [j] > roadLength / luminaireLimit || Xvelocity[j] + spacing[j] <= 5)) 
				{
					if (Xvelocity [j] + spacing [j] <= 5)
						spacing [j] = 6f;
					else if (sBool2)
						spacing [j] = roadLength / luminaireLimit;
					else
						spacing [j] = roadLength / luminaireLimit - .5f;
				} 
				else if (Xvelocity [j] + spacing [j] > roadLength)
					spacing [j] = roadLength;
				else if (sBool2 && (Xvelocity [j] + spacing [j] > roadLength / (luminaireLimit - 1f) || Xvelocity [j] + spacing [j] < roadLength / luminaireLimit)) 
				{
					if (Xvelocity [j] + spacing [j] > roadLength / (luminaireLimit - 1f))
						spacing [j] = roadLength / (luminaireLimit - 1f);
					else
						spacing [j] = roadLength / luminaireLimit;
				}
                else
					spacing[j] += Xvelocity[j];
				if (Yvelocity[j] + height[j] < 6f)
					height[j] = 6f;
				else if(hBool1 && Yvelocity[j] + height[j] > heightLimit) 
				{
					if(hBool2)
						height[j] = heightLimit;
					else
						height[j] = heightLimit - .5f;
				} 
				else if (hBool3 && Yvelocity[j] + height[j] < heightLimit) 
				{
					if(hBool2)
						height[j] = heightLimit;
					else
						height[j] = heightLimit + .5f;
				} 
				else if(hBool2)
					height[j] = heightLimit;
                else
                    height[j] += Yvelocity[j];
            }
            for (int j = 0; j < particles; j++)
            {
                lightCalculator.spacing = spacing[j];
                lightCalculator.height = height[j];
				ClearData ();
                lightCalculator.CalcIlluminance();
				spacing [j] = lightCalculator.spacing;
				canBuild [j] = lightCalculator.buildable;
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
            for (int j = 0; j < particles; j++)
            {
                if (spacing[j] >= 0 && height[j] >= 0)
                {
                    if (tempDevAverage[j] < devAverage[j])
                        AdjustOptimal(j);
                    else if (tempDevAverage[j] == devAverage[j])
                    {
						if (tempDevMinimum [j] < devMinimum [j])
							AdjustOptimal (j);
						else if (tempDevMinimum [j] == devMinimum [j]) {
							if (tempEfficiencies [j] > Efficiencies [j])
								AdjustOptimal (j);
							else if (tempEfficiencies [j] <= Efficiencies [j] && tempScore [j] < Score [j])
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
			lightCalculator.CalcIlluminance ();
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
        closestSpacing[index] = spacing[index];
        closestHeight[index] = height[index];
        Averages[index] = tempAverages[index];
        Minimums[index] = tempMinimums[index];
        Efficiencies[index] = tempEfficiencies[index];
        devAverage[index] = tempDevAverage[index];
        devMinimum[index] = tempDevMinimum[index];
		Score [index] = tempScore [index];
    }

    private void SetBest()
    {
        float[] bestCandidates1 = new float[particles];
        int[] foundIndex1 = new int[particles];
        int ind1 = 0;
        for (int i = 0; i < particles; i++)
        {
			if (devAverage[i] == Mathf.Min(devAverage) && canBuild[i])
            {
                bestCandidates1[ind1] = devMinimum[i];
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
                bestCandidates3[ind2] = Efficiencies[foundIndex1[i]];
				bestCandidates31 [ind2] = Score [foundIndex1 [i]];
                foundIndex2[ind2] = foundIndex1[i];
                ind2++;
            }
        }
		float[] bestCandidates4 = new float[ind2];
		for (int i = 0; i < ind2; i++) {
			if (bestCandidates31 [i] >= minScore) {
				bestCandidates4 [ind3] = Efficiencies [foundIndex2 [i]];
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
        bestSpacing = spacing[index];
        bestHeight = height[index];
		buildabl = canBuild [index];
		if (it > Mathf.FloorToInt (iterations / 2.5f)) {
			if (finalSpacing <= bestSpacing + .5f && finalSpacing >= bestSpacing - .5f && finalHeight <= bestHeight + .5f && finalHeight >= bestHeight - .5f)
				check++;
			else {
				check = 0;
				finalSpacing = bestSpacing;
				finalHeight = bestHeight;
			}
		}
        Xvelocity[index] = 0;
        Yvelocity[index] = 0;
    }
    
    private float CalcVelocity(float velocity,float closest, float position, float best){
        return velocity + c1 * r1 * (closest - position) + c2 * r2 * (best - position);
    }
}
