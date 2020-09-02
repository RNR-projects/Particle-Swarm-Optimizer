using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LightCalculator : MonoBehaviour
{
    private int longCalPoints;

	private float averageIlluminance;

	private float lampLumen;
	private float luminairePower;

	private int[,] intensityTable;
	[SerializeField] private TextAsset intensityTableFile;

	private List<float> pointIlluminances;

	void Awake()
	{
		this.lampLumen = OptimizationParameterManager.LAMPLUMEN;
		this.luminairePower = OptimizationParameterManager.LUMINAIREPOWER;
		this.longCalPoints = OptimizationParameterManager.LONGCALPOINTS;

		this.intensityTable = DataParser.Instance().ParseIntDataTable(intensityTableFile);
	}

    void FixedUpdate()
    {
		//text.text = "Average: " + averageIlluminance + 
					//"\nMin: " + minIlluminance + 
					//"\nEfficiency:" + lightingEfficiency + 
					//"\nOffset:" + (xOffset + swarm.roadLength % particle.spacing);		
    }

    public void CalcIlluminance(SolutionParticle particle)
	{
		float roadWidth = OptimizationParameterManager.Instance().GetRoadWidth();

		this.CalculateLuminaireIlluminanceEffect(particle);

		this.GetAverageIlluminance();
		particle.averageIlluminance = this.averageIlluminance;
		particle.lightingEfficiency = this.averageIlluminance * particle.spacing * roadWidth / this.luminairePower;

		this.AssignIlluminationValues(particle);

		this.ResetIllumination();
	}
	/// <summary>
	/// requires the candela table to be loaded
	/// </summary>
	private void CalculateLuminaireIlluminanceEffect(SolutionParticle particle)
    {
		List<float> luminaireXPos = RoadAndLuminaireCreator.Instance().GetLuminaireXPositions();
		List<float> luminaireYPos = RoadAndLuminaireCreator.Instance().GetLuminaireYPositions();

		List<float> illuminancePointXPos = IlluminationPointsCreator.Instance().GetIlluminationPointXCoords();
		List<float> illuminancePointYPos = IlluminationPointsCreator.Instance().GetIlluminationPointYCoords();

		for (int i = 0; i < particle.luminaireCount; i++)
		{
			for (int j = 0; j < longCalPoints * 3; j++)
			{
				float C = Mathf.Rad2Deg * Mathf.Atan(Mathf.Abs(illuminancePointYPos[j] - luminaireYPos[i]) / Mathf.Abs(illuminancePointXPos[j] - luminaireXPos[i]));
				//get the angle between the point and the luminaire's base, obviously needs reworking as the light does not come from there
				//opposite is perpendicular axis to the road, adjacent is parallel axis
				float Y = Mathf.Rad2Deg * Mathf.Atan(Mathf.Sqrt(Mathf.Pow(illuminancePointXPos[j] - luminaireXPos[i], 2) +
										Mathf.Pow(illuminancePointYPos[j] - luminaireYPos[i], 2)) / particle.height);
				//angle between the light source and the point also needs reworking, opposite is the ground, adjacent is the height
				float illuminancePoint = Mathf.Lerp(Mathf.Lerp(intensityTable[Mathf.FloorToInt(C / 2.5f), Mathf.FloorToInt(Y / 2.5f)],
					intensityTable[Mathf.FloorToInt(C / 2.5f) + 1, Mathf.FloorToInt(Y / 2.5f)], (C % 2.5f) / 2.5f),
					Mathf.Lerp(intensityTable[Mathf.FloorToInt(C / 2.5f), Mathf.FloorToInt(Y / 2.5f) + 1],
						intensityTable[Mathf.FloorToInt(C / 2.5f) + 1, Mathf.FloorToInt(Y / 2.5f) + 1],
						C % 2.5f / 2.5f), Y % 2.5f / 2.5f);
				//apply linear interpolation to get the illumination value at the point regardless of what angle it is
				pointIlluminances[j] += (lampLumen * .85f / Mathf.Pow(particle.height, 2))
					* (illuminancePoint / lampLumen * Mathf.Pow(Mathf.Cos(Mathf.Deg2Rad * Y), 3));
			}
		}
	}
	/// <summary>
	/// applies the calculated values onto the illuminance points
	/// </summary>
	private void AssignIlluminationValues(SolutionParticle particle)
    {
		float minIlluminance = Mathf.Min(this.pointIlluminances.ToArray());
		particle.lowestIlluminanceAtAPoint = minIlluminance;
		List<GameObject> points = IlluminationPointsCreator.Instance().GetIlluminationPoints();

		for (int i = 0; i < longCalPoints * 3; i++)
		{
			GameObject point = points[i];
			point.GetComponent<LightLevel>().SetIlluminance(this.pointIlluminances[i]);
			if (this.pointIlluminances[i] < minIlluminance + (this.averageIlluminance - minIlluminance) / 2)
				point.GetComponent<Renderer>().material.color = Color.red;
			else if (this.pointIlluminances[i] >= minIlluminance + (this.averageIlluminance - minIlluminance) / 2 && this.pointIlluminances[i] < this.averageIlluminance)
				point.GetComponent<Renderer>().material.color = Color.yellow;
			else if (this.pointIlluminances[i] >= this.averageIlluminance && this.pointIlluminances[i] < this.averageIlluminance * 2)
				point.GetComponent<Renderer>().material.color = Color.green;
			else if (this.pointIlluminances[i] >= this.averageIlluminance * 2)
				point.GetComponent<Renderer>().material.color = Color.blue;
		}
	}

	private void GetAverageIlluminance()
    {
		float totalIlluminance = 0;
		for (int i = 0; i < longCalPoints * 3; i++)
		{
			float illuminance = this.pointIlluminances[i];
			totalIlluminance += illuminance;			
		}
		this.averageIlluminance = totalIlluminance / (longCalPoints * 3);
	}

	private void ResetIllumination()
    {
		this.pointIlluminances.Clear();
	}
}
