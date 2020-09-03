using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyGenerationCalculator
{
	private static EnergyGenerationCalculator sharedInstance;

	private float[,] azimuth;
	private float[,] horizonAngle;
	private float[,] solarGeneration;

	private int[] monthlyHours = { 11, 11, 12, 13, 13, 13, 13, 13, 12, 12, 12, 11 };//where did we get this data again

	private float[] energyGeneration;

	public static EnergyGenerationCalculator Instance()
    {
		if (sharedInstance == null)
			sharedInstance = new EnergyGenerationCalculator();
		return sharedInstance;
    }

	private EnergyGenerationCalculator()
    {

    }

    public void CalculateEnergyGeneration(SolutionParticle particle)
    {
		this.InitializeEnergyGeneration(particle);

		this.ApplyShadowsOnSolar(particle);

		this.AssignEnergyGenerationToSolar(particle);

		this.CheckEnergyGeneration(particle);
    }
	/// <summary>
	/// initializes energy generation as assuming no shadows interfere
	/// </summary>
	private void InitializeEnergyGeneration(SolutionParticle particle)
	{
		this.energyGeneration = new float[particle.luminaireCount];

		for (int i = 0; i < particle.luminaireCount; i++)
			this.energyGeneration[i] = 0;

		for (int z = 0; z < 12; z++)
		{
			for (int sun = 0; sun < monthlyHours[z]; sun++)
			{
				float generation = solarGeneration[z, sun];
				if (z < 7)
				{
					if (z % 2 == 0)
						generation *= 31f;
					else if (z == 1)
						generation *= 28f;
					else
						generation *= 30f;
				}
				else
				{
					if (z % 2 == 1)
						generation *= 31f;
					else
						generation *= 30f;
				}
				for (int i = 0; i < particle.luminaireCount; i++)
					this.energyGeneration[i] += generation;
			}
		}
	}
	/// <summary>
	/// subtracts previously initialized values with lost generation due to shadows blocking
	/// </summary>
	private void ApplyShadowsOnSolar(SolutionParticle particle)
	{
		List<GameObject> lampList = RoadAndLuminaireCreator.Instance().GetAllCreatedLuminaires();
		OptimizationParameterManager parameters = OptimizationParameterManager.Instance();

		for (int x = 0; x < particle.luminaireCount; x++)
		{
			for (int z = 0; z < 12; z++)
			{
				for (int sun = 0; sun < monthlyHours[z]; sun++)
				{
					int percBlocked = 0;
					Vector3 sunPos = new Vector3(
						Mathf.Sin(Mathf.Deg2Rad * (azimuth[z, sun] - parameters.GetRoadDirection())) * 
									Mathf.Cos(Mathf.Deg2Rad * horizonAngle[z, sun]), 
						Mathf.Sin(Mathf.Deg2Rad * horizonAngle[z, sun]), 
						Mathf.Cos(Mathf.Deg2Rad * (azimuth[z, sun] - parameters.GetRoadDirection())) * 
									Mathf.Cos(Mathf.Deg2Rad * horizonAngle[z, sun]));

					Vector3 panelHalfSize = new Vector3(
						lampList[x].transform.GetChild(0).transform.lossyScale.x / 2f,
						lampList[x].transform.GetChild(0).transform.lossyScale.y / 2f,
						lampList[x].transform.GetChild(0).transform.lossyScale.z / 2f);

					if (Physics.BoxCast(lampList[x].transform.GetChild(0).transform.position, panelHalfSize, sunPos, Quaternion.Euler(0,0,0)))
					{	//split the panel into 4 quadrants and check again
						for (int t = 0; t < 2; t++)
						{
							for (int u = 0; u < 2; u++)
							{
								Vector3 panelQuadrantCenter = new Vector3(
									lampList[x].transform.GetChild(0).transform.position.x + Mathf.Pow(-1f, t) *
												lampList[x].transform.GetChild(0).transform.lossyScale.x / 4f,
									lampList[x].transform.GetChild(0).transform.position.y +
												lampList[x].transform.GetChild(0).transform.lossyScale.y / 2f,
									lampList[x].transform.GetChild(0).transform.position.z + Mathf.Pow(-1f, u) *
												lampList[x].transform.GetChild(0).transform.lossyScale.z / 4f);
								Vector3 panelQuadrantHalfSize = new Vector3(
									lampList[x].transform.GetChild(0).transform.lossyScale.x / 4f, 
									0, 
									lampList[x].transform.GetChild(0).transform.lossyScale.z / 4f);

								if (Physics.BoxCast(panelQuadrantCenter, panelQuadrantHalfSize, sunPos, Quaternion.identity))
								{	//if the quadrant is blocked by a building, split into a 5x5 grid each square blocked is 1%
									for (int m = 0; m < 5; m++)
										for (int n = 0; n < 5; n++)
										{
											Vector3 panelPercPos = new Vector3(
												lampList[x].transform.GetChild(0).transform.position.x + 
															Mathf.Pow(-1f, t) * lampList[x].transform.GetChild(0).transform.lossyScale.x / 4f - 
															lampList[x].transform.GetChild(0).transform.lossyScale.x / 4f + m * 
															lampList[x].transform.GetChild(0).transform.lossyScale.x / 10f, 
												lampList[x].transform.GetChild(0).transform.position.y + 
															lampList[x].transform.GetChild(0).transform.lossyScale.y / 2f, 
												lampList[x].transform.GetChild(0).transform.position.z + Mathf.Pow(-1f, u) * 
															lampList[x].transform.GetChild(0).transform.lossyScale.z / 4f - 
															lampList[x].transform.GetChild(0).transform.lossyScale.z / 4f + 
															n * lampList[x].transform.GetChild(0).transform.lossyScale.z / 10f);

											if (Physics.Raycast(panelPercPos, sunPos, Mathf.Infinity))
											{
												percBlocked++;
											}
										}
								}
							}
						}
					}
					//completely shaded panels can still generate 50% energy
					float generation = this.solarGeneration[z, sun] * (0.5f * percBlocked / 100f);
					if (z < 7)//multiply generation with the days in the month
					{
						if (z % 2 == 0)
							generation *= 31f;
						else if (z == 1)
							generation *= 28f;
						else
							generation *= 30f;
					}
					else
					{
						if (z % 2 == 1)
							generation *= 31f;
						else
							generation *= 30f;
					}
					this.energyGeneration[x] -= generation;
				}
			}
		}
	}
	/// <summary>
	/// assigns the calculated values to each of the solar panels
	/// </summary>
	/// <param name="particle"></param>
	private void AssignEnergyGenerationToSolar(SolutionParticle particle)
	{
		List<GameObject> lampList = RoadAndLuminaireCreator.Instance().GetAllCreatedLuminaires();

		for (int i = 0; i < particle.luminaireCount; i++)
		{
			lampList[i].GetComponentInChildren<AverageEnergyGeneration>().setScore(this.energyGeneration[i] * 0.607f);
			this.energyGeneration[i] = 0;
		}
	}
	/// <summary>
	/// check if the lowest energy generated meets the requirement and attempt to adjust the offset and go back from the top if not
	/// </summary>
	private void CheckEnergyGeneration(SolutionParticle particle)
	{
		OptimizationParameterManager parameters = OptimizationParameterManager.Instance();

		float margin = parameters.GetRoadLength() % particle.spacing / 2f;
		if (Mathf.Min(energyGeneration) < parameters.GetMinimumTargetEnergyGeneration())
		{
			if (particle.xOffset == 0)
			{
				particle.xOffset = -margin;
				ParticleSimulator.Instance().SimulateParticle(particle);
			}
			else if (particle.xOffset + 1f <= margin)
			{
				particle.xOffset++;
				ParticleSimulator.Instance().SimulateParticle(particle);
			}
			else
				particle.lowestEnergyGeneratedByALuminaire = Mathf.Min(energyGeneration);			
		}
		else
			particle.lowestEnergyGeneratedByALuminaire = Mathf.Min(energyGeneration);
	}

	public void LoadAzimuthFile(TextAsset file)
    {
		this.azimuth = DataParser.Instance().ParseFloatDataTable(file);
	}

	public void LoadHorizonAngleFile(TextAsset file)
    {
		this.horizonAngle = DataParser.Instance().ParseFloatDataTable(file);
	}

	public void LoadSolarGenerationFile(TextAsset file)
    {
		this.solarGeneration = DataParser.Instance().ParseFloatDataTable(file);
	}
}
