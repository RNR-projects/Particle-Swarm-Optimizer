using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LuminairePositionsCalculator
{
	private static LuminairePositionsCalculator sharedInstance;

	public static LuminairePositionsCalculator Instance()
    {
		if (sharedInstance == null)
			sharedInstance = new LuminairePositionsCalculator();
		return sharedInstance;
    }

	private LuminairePositionsCalculator()
    {
		this.RevNegOffsets = new List<float>();
		this.RevPosOffsets = new List<float>();
		this.pairedNegOffsets = new List<float>();
		this.pairedPosOffsets = new List<float>();
		this.OGNegOffsets = new List<float>();
		this.OGPosOffsets = new List<float>();
	}

	/// <summary>
	/// offsets needed to avoid structures in the X direction
	/// </summary>
	private List<float> pairedPosOffsets, pairedNegOffsets;
	/// <summary>
	/// offsets needed to avoid structures in the X direction in the non reversed state
	/// </summary>
	private List<float> OGPosOffsets, OGNegOffsets;
	/// <summary>
	/// offsets needed to avoid structures in the X direction in the reversed state
	/// </summary>
	private List<float> RevPosOffsets, RevNegOffsets;

	public void CalculateFinalLuminairePositions(SolutionParticle particle)
	{
		//check if there is a need to change
		if (this.CheckLuminaireSetLocations(particle.spacing, particle.xOffset, particle.luminaireCount, false))
		{
			this.GetOffsetsToAvoidStructures(particle);
			
			if (!particle.isReversed)//only is true at this point if just reversed order is satisfactory already
			{
				this.AttemptToFixSpacing(particle);
			}
		}

		this.ClearCalculationData();
	}

	private void GetOffsetsToAvoidStructures(SolutionParticle particle)
	{
		float scale = GlobalScaler.Instance().GetGlobalScale();
		OptimizationParameterManager parameters = OptimizationParameterManager.Instance();

		if (parameters.GetLuminaireArrangement() == OptimizationParameterManager.LuminaireArrangementSettings.Paired)
		{
			for (int i = 0; i < particle.luminaireCount; i++)
			{
				float luminaireXPos = particle.spacing * Mathf.Floor(i / 2f) + (parameters.GetRoadLength() % particle.spacing) / 2f * scale;
				if (this.CheckLuminaireFoundation(luminaireXPos, i, false))
				{
					float luminareYPos = parameters.GetRoadWidth() * (i % 2) * scale;

					Collider[] structure = Physics.OverlapBox(new Vector3(luminaireXPos, 0, luminareYPos),
						new Vector3(0.25f * scale, 2f * scale, 0.25f * scale), Quaternion.Euler(0, 0, 0));

					float structureCenter = structure[0].transform.position.x;
					float structureHalfSize = structure[0].transform.lossyScale.x / 2f;

					this.pairedPosOffsets.Add((structureCenter + structureHalfSize) / scale -
						(particle.spacing * Mathf.Floor(i / 2f) + (parameters.GetRoadLength() % particle.spacing) / 2f) + 0.5f);
					this.pairedNegOffsets.Add((structureCenter - structureHalfSize) / scale -
						(particle.spacing * Mathf.Floor(i / 2f) + (parameters.GetRoadLength() % particle.spacing) / 2f) - 0.5f);
				}
				else
				{
					this.pairedPosOffsets.Add(0);
					this.pairedNegOffsets.Add(0);
				}
			}
		}
		else
		{
			if (this.CheckLuminaireSetLocations(particle.spacing, 0, particle.luminaireCount, true))
			{
				for (int i = 0; i < particle.luminaireCount; i++)
				{
					//get offsets to avoid structures in the reversed position
					float luminaireXPos = particle.spacing * i + (parameters.GetRoadLength() % particle.spacing) / 2f * scale;
					if (this.CheckLuminaireFoundation(luminaireXPos, i, true))
					{
						float luminaireYPos;
						if (parameters.GetLuminaireArrangement() == OptimizationParameterManager.LuminaireArrangementSettings.Alternating)
							luminaireYPos = parameters.GetRoadWidth() * ((i + 1) % 2) * scale;
						else
							luminaireYPos = parameters.GetRoadWidth() * scale;

						Collider[] structure = Physics.OverlapBox(new Vector3(luminaireXPos, 0, luminaireYPos),
											new Vector3(0.25f * scale, 2f * scale, 0.25f * scale), Quaternion.Euler(0, 0, 0));

						float structureCenter = structure[0].transform.position.x;
						float structureHalfSize = structure[0].transform.lossyScale.x / 2f;

						this.RevPosOffsets.Add((structureCenter + structureHalfSize) / scale -
							(particle.spacing * i + (parameters.GetRoadLength() % particle.spacing) / 2f) + 0.5f);
						this.RevNegOffsets.Add((structureCenter - structureHalfSize) / scale -
							(particle.spacing * i + (parameters.GetRoadLength() % particle.spacing) / 2f) - 0.5f);
					}
					else
					{
						this.RevPosOffsets.Add(0);
						this.RevNegOffsets.Add(0);
					}

					//get offsets to avoid structures in the original position
					if (this.CheckLuminaireFoundation(luminaireXPos, i, false))
					{
						float luminaireYPos;
						if (parameters.GetLuminaireArrangement() == OptimizationParameterManager.LuminaireArrangementSettings.Alternating)
							luminaireYPos = parameters.GetRoadWidth() * (i % 2) * scale;
						else
							luminaireYPos = 0;

						Collider[] structure = Physics.OverlapBox(new Vector3(luminaireXPos, 0, luminaireYPos),
											new Vector3(0.25f * scale, 2f * scale, 0.25f * scale), Quaternion.Euler(0, 0, 0));

						float structureCenter = structure[0].transform.position.x;
						float structureHalfSize = structure[0].transform.lossyScale.x / 2f;

						this.OGPosOffsets.Add((structureCenter + structureHalfSize) / scale -
							(particle.spacing * i + (parameters.GetRoadLength() % particle.spacing) / 2f) + 0.5f);
						this.OGNegOffsets.Add((structureCenter - structureHalfSize) / scale -
							(particle.spacing * i + (parameters.GetRoadLength() % particle.spacing) / 2f) - 0.5f);
					}
					else
					{
						this.OGPosOffsets.Add(0);
						this.OGNegOffsets.Add(0);
					}
				}
			}
			else
			{
				particle.isReversed = true;
			}
		}
	}
	/// <summary>
	/// returns true if any of the luminaires is blocked by a collider
	/// </summary>
	private bool CheckLuminaireSetLocations(float spacing, float XOffset, int luminaireCount, bool isReversedPositions)
	{
		OptimizationParameterManager parameters = OptimizationParameterManager.Instance();
		float scale = GlobalScaler.Instance().GetGlobalScale();

		for (int i = 0; i < luminaireCount; i++)
		{
			float luminaireXPos;
			if (parameters.GetLuminaireArrangement() == OptimizationParameterManager.LuminaireArrangementSettings.Paired)
			{
				luminaireXPos = (spacing * Mathf.Floor(i / 2f) + parameters.GetRoadLength() % spacing / 2f  + XOffset) * scale;
			}
			else
			{
				luminaireXPos = (spacing * i + parameters.GetRoadLength() % spacing / 2f + XOffset) * scale;
			}
			if (this.CheckLuminaireFoundation(luminaireXPos, i, isReversedPositions))
				return true;
		}

		return false;
	}
	///<summary>returns true if there is a collider in the way</summary>
	private bool CheckLuminaireFoundation(float XPos, int index, bool isReversedPosition)
	{
		OptimizationParameterManager parameters = OptimizationParameterManager.Instance();
		float scale = GlobalScaler.Instance().GetGlobalScale();
		OptimizationParameterManager.LuminaireArrangementSettings arrangement = parameters.GetLuminaireArrangement();

		switch (arrangement)
		{
			case OptimizationParameterManager.LuminaireArrangementSettings.Alternating:
				int reversedEffect = 0;
				if (isReversedPosition)
					reversedEffect = 1;
				if (Physics.CheckBox(new Vector3(XPos, 0, parameters.GetRoadWidth() * ((index + reversedEffect) % 2) * scale),
										new Vector3(0.25f * scale, 2f * scale, 0.25f * scale), Quaternion.Euler(0, 0, 0)))
					return true;
				else
					return false;
			case OptimizationParameterManager.LuminaireArrangementSettings.OneSided:
				float YPos = 0;
				if (isReversedPosition)
					YPos = parameters.GetRoadWidth() * scale;
				if (Physics.CheckBox(new Vector3(XPos, 0, YPos),
										new Vector3(0.25f * scale, 2f * scale, 0.25f * scale), Quaternion.Euler(0, 0, 0)))
					return true;
				else
					return false;
			case OptimizationParameterManager.LuminaireArrangementSettings.Paired:
				if (Physics.CheckBox(new Vector3(XPos, 0, parameters.GetRoadWidth() * (index % 2) * scale),
										new Vector3(0.25f * scale, 2f * scale, 0.25f * scale), Quaternion.Euler(0, 0, 0)))
					return true;
				else
					return false;
			default:
				return false;
		}
	}
	/// <summary>
	/// tries the previously taken offsets and maybe adjust spacing to fix it, otherwise sets the particle's isBuildable to false
	/// </summary>
	private void AttemptToFixSpacing(SolutionParticle particle)
	{
		OptimizationParameterManager parameters = OptimizationParameterManager.Instance();


		if (parameters.GetLuminaireArrangement() == OptimizationParameterManager.LuminaireArrangementSettings.Paired)
		{
			float deltaSpaceLimit = parameters.GetRoadLength() / (particle.luminaireCount / 2) + 0.25f - particle.spacing;
			float originalFurthestXPos = particle.spacing * Mathf.Floor((particle.luminaireCount - 1f) / 2f) +
					parameters.GetRoadLength() % particle.spacing / 2f;


			if ((originalFurthestXPos + Mathf.Max(this.pairedPosOffsets.ToArray())) > parameters.GetRoadLength())
			{   //get overshoot value, divide it to spacing change needed to prevent it, then reduce spacing further to attempt to fix the main issue
				float deltaSpacing = -(originalFurthestXPos + Mathf.Max(this.pairedPosOffsets.ToArray())
					- parameters.GetRoadLength() + 0.25f) / Mathf.Floor((particle.luminaireCount - 1) / 2f) - 0.5f;
				if (deltaSpacing >= deltaSpaceLimit)
				{
					float trialSpacing = particle.spacing + deltaSpacing;
					//adjust offset to take into account spacing changes and the resulting margin changes as well
					float trialOffset = Mathf.Max(this.pairedPosOffsets.ToArray()) -
						deltaSpacing * Mathf.Floor(this.pairedPosOffsets.IndexOf(Mathf.Max(this.pairedPosOffsets.ToArray())) / 2f) +
						deltaSpacing * Mathf.Floor((particle.luminaireCount - 1) / 2) / 2f;

					if (this.CheckLuminaireSetLocations(trialSpacing, trialOffset, particle.luminaireCount, false))
					{
						this.TryPairedNegOffsets(particle);
					}
					else
					{
						particle.spacing += deltaSpacing;
						particle.xOffset += trialOffset;
					}
				}
				else
				{
					this.TryPairedNegOffsets(particle);
				}
			}
			else
			{
				if (this.CheckLuminaireSetLocations(particle.spacing, Mathf.Max(this.pairedPosOffsets.ToArray()), particle.luminaireCount, false))
				{
					this.TryPairedNegOffsets(particle);
				}
				else
				{
					particle.xOffset += Mathf.Max(this.pairedPosOffsets.ToArray());
				}
			}
		}
		else
        {	//same formulas are being used with alternating and one sided arrangements
			float deltaSpaceLimit = parameters.GetRoadLength() / particle.luminaireCount + 0.25f - particle.spacing;
			float originalFurthestXPos = particle.spacing * (particle.luminaireCount - 1f) + 
												parameters.GetRoadLength() % particle.spacing / 2f;

			if ((originalFurthestXPos + Mathf.Max(this.OGPosOffsets.ToArray())) > parameters.GetRoadLength())
			{   //get overshoot value, divide it to spacing change needed to prevent it, then reduce spacing further to attempt to fix the main issue
				float deltaSpacing = -(originalFurthestXPos + Mathf.Max(this.OGPosOffsets.ToArray())
					- parameters.GetRoadLength() + 0.25f) / (particle.luminaireCount - 1f) - 0.5f;
				if (deltaSpacing >= deltaSpaceLimit)
				{
					float trialSpacing = particle.spacing + deltaSpacing;
					//adjust offset to take into account spacing changes and the resulting margin changes as well
					float trialOffset = Mathf.Max(this.OGPosOffsets.ToArray()) -
						deltaSpacing * this.OGPosOffsets.IndexOf(Mathf.Max(this.OGPosOffsets.ToArray())) +
						deltaSpacing * (particle.luminaireCount - 1f) / 2f;

					if (this.CheckLuminaireSetLocations(trialSpacing, trialOffset, particle.luminaireCount, false))
					{
						this.TryOGNegOffsets(particle);
					}
					else
					{
						particle.spacing += deltaSpacing;
						particle.xOffset += trialOffset;
					}
				}
				else
				{
					this.TryOGNegOffsets(particle);
				}
			}
			else
			{
				if (this.CheckLuminaireSetLocations(particle.spacing, Mathf.Max(this.OGPosOffsets.ToArray()), particle.luminaireCount, false))
				{
					this.TryOGNegOffsets(particle);
				}
				else
				{
					particle.xOffset += Mathf.Max(this.OGPosOffsets.ToArray());
				}
			}
		}
		

	}

	private void TryPairedNegOffsets(SolutionParticle particle)
	{
		OptimizationParameterManager parameters = OptimizationParameterManager.Instance();

		float deltaSpaceLimit = parameters.GetRoadLength() / (particle.luminaireCount / 2) + 0.25f - particle.spacing;

		if ((parameters.GetRoadLength() % particle.spacing / 2f + Mathf.Min(this.pairedNegOffsets.ToArray())) < 0)
		{
			float deltaSpacing = -(parameters.GetRoadLength() % particle.spacing / 2f + Mathf.Min(this.pairedNegOffsets.ToArray()) + 0.25f) / 
				Mathf.Floor((particle.luminaireCount - 1) / 2f) - 0.5f;
			if (deltaSpacing >= deltaSpaceLimit)
			{
				float trialSpacing = particle.spacing + deltaSpacing;
				//adjust offset to take into account spacing changes and the resulting margin changes as well
				float trialOffset = Mathf.Min(this.pairedNegOffsets.ToArray()) -
					deltaSpacing * Mathf.Floor(this.pairedNegOffsets.IndexOf(Mathf.Min(this.pairedNegOffsets.ToArray())) / 2f) +
					deltaSpacing * Mathf.Floor((particle.luminaireCount - 1) / 2) / 2f;
				
				if (this.CheckLuminaireSetLocations(trialSpacing, trialOffset, particle.luminaireCount, false))
                {
					particle.isBuildable = false;
                }
				else
                {
					particle.spacing += deltaSpacing;
					particle.xOffset += trialOffset;
                }
			}
			else
            {
				particle.isBuildable = false;
            }
		}
		else
		{
			if (this.CheckLuminaireSetLocations(particle.spacing, Mathf.Min(this.pairedNegOffsets.ToArray()), particle.luminaireCount, false))
            {
				particle.isBuildable = false;
            }
			else
			{
				particle.xOffset += Mathf.Min(this.pairedNegOffsets.ToArray());
			}
		}
	}

	private void TryOGNegOffsets(SolutionParticle particle)
    {
		OptimizationParameterManager parameters = OptimizationParameterManager.Instance();

		float deltaSpaceLimit = parameters.GetRoadLength() / particle.luminaireCount + 0.25f - particle.spacing;

		if ((parameters.GetRoadLength() % particle.spacing / 2f + Mathf.Min(this.OGNegOffsets.ToArray())) < 0)
		{
			float deltaSpacing = -(parameters.GetRoadLength() % particle.spacing / 2f + Mathf.Min(this.OGNegOffsets.ToArray()) + 0.25f) /
									(particle.luminaireCount - 1f) - 0.5f;
			if (deltaSpacing >= deltaSpaceLimit)
			{
				float trialSpacing = particle.spacing + deltaSpacing;
				//adjust offset to take into account spacing changes and the resulting margin changes as well
				float trialOffset = Mathf.Min(this.OGNegOffsets.ToArray()) -
					deltaSpacing * this.OGNegOffsets.IndexOf(Mathf.Min(this.OGNegOffsets.ToArray())) +
					deltaSpacing * (particle.luminaireCount - 1f) / 2f;

				if (this.CheckLuminaireSetLocations(trialSpacing, trialOffset, particle.luminaireCount, false))
				{
					this.TryRevPosOffsets(particle);
				}
				else
				{
					particle.spacing += deltaSpacing;
					particle.xOffset += trialOffset;
				}
			}
			else
			{
				this.TryRevPosOffsets(particle);
			}
		}
		else
		{
			if (this.CheckLuminaireSetLocations(particle.spacing, Mathf.Min(this.OGNegOffsets.ToArray()), particle.luminaireCount, false))
			{
				this.TryRevPosOffsets(particle);
			}
			else
			{
				particle.xOffset += Mathf.Min(this.OGNegOffsets.ToArray());
			}
		}
	}

	private void TryRevPosOffsets(SolutionParticle particle)
    {
		OptimizationParameterManager parameters = OptimizationParameterManager.Instance();

		//same formulas are being used with alternating and one sided arrangements
		float deltaSpaceLimit = parameters.GetRoadLength() / particle.luminaireCount + 0.25f - particle.spacing;
		float originalFurthestXPos = particle.spacing * (particle.luminaireCount - 1f) +
											parameters.GetRoadLength() % particle.spacing / 2f;

		if ((originalFurthestXPos + Mathf.Max(this.RevPosOffsets.ToArray())) > parameters.GetRoadLength())
		{   //get overshoot value, divide it to spacing change needed to prevent it, then reduce spacing further to attempt to fix the main issue
			float deltaSpacing = -(originalFurthestXPos + Mathf.Max(this.RevPosOffsets.ToArray())
				- parameters.GetRoadLength() + 0.25f) / (particle.luminaireCount - 1f) - 0.5f;
			if (deltaSpacing >= deltaSpaceLimit)
			{
				float trialSpacing = particle.spacing + deltaSpacing;
				//adjust offset to take into account spacing changes and the resulting margin changes as well
				float trialOffset = Mathf.Max(this.RevPosOffsets.ToArray()) -
					deltaSpacing * this.RevPosOffsets.IndexOf(Mathf.Max(this.RevPosOffsets.ToArray())) +
					deltaSpacing * (particle.luminaireCount - 1f) / 2f;

				if (this.CheckLuminaireSetLocations(trialSpacing, trialOffset, particle.luminaireCount, true))
				{
					this.TryRevNegOffsets(particle);
				}
				else
				{
					particle.spacing += deltaSpacing;
					particle.xOffset += trialOffset;
					particle.isReversed = true;
				}
			}
			else
			{
				this.TryRevNegOffsets(particle);
			}
		}
		else
		{
			if (this.CheckLuminaireSetLocations(particle.spacing, Mathf.Max(this.RevPosOffsets.ToArray()), particle.luminaireCount, true))
			{
				this.TryRevNegOffsets(particle);
			}
			else
			{
				particle.xOffset += Mathf.Max(this.RevPosOffsets.ToArray());
				particle.isReversed = true;
			}
		}
	}

	private void TryRevNegOffsets(SolutionParticle particle)
    {
		OptimizationParameterManager parameters = OptimizationParameterManager.Instance();

		float deltaSpaceLimit = parameters.GetRoadLength() / particle.luminaireCount + 0.25f - particle.spacing;

		if ((parameters.GetRoadLength() % particle.spacing / 2f + Mathf.Min(this.RevNegOffsets.ToArray())) < 0)
		{
			float deltaSpacing = -(parameters.GetRoadLength() % particle.spacing / 2f + Mathf.Min(this.RevNegOffsets.ToArray()) + 0.25f) /
									(particle.luminaireCount - 1f) - 0.5f;
			if (deltaSpacing >= deltaSpaceLimit)
			{
				float trialSpacing = particle.spacing + deltaSpacing;
				//adjust offset to take into account spacing changes and the resulting margin changes as well
				float trialOffset = Mathf.Min(this.RevNegOffsets.ToArray()) -
					deltaSpacing * this.RevNegOffsets.IndexOf(Mathf.Min(this.RevNegOffsets.ToArray())) +
					deltaSpacing * (particle.luminaireCount - 1f) / 2f;

				if (this.CheckLuminaireSetLocations(trialSpacing, trialOffset, particle.luminaireCount, true))
				{
					particle.isBuildable = false;
				}
				else
				{
					particle.spacing += deltaSpacing;
					particle.xOffset += trialOffset;
					particle.isReversed = true;
				}
			}
			else
			{
				particle.isBuildable = false;
			}
		}
		else
		{
			if (this.CheckLuminaireSetLocations(particle.spacing, Mathf.Min(this.RevNegOffsets.ToArray()), particle.luminaireCount, true))
			{
				particle.isBuildable = false;
			}
			else
			{
				particle.xOffset += Mathf.Min(this.RevNegOffsets.ToArray());
				particle.isReversed = true;
			}
		}
	}

	private void ClearCalculationData()
    {
		this.OGNegOffsets.Clear();
		this.OGPosOffsets.Clear();
		this.RevPosOffsets.Clear();
		this.RevNegOffsets.Clear();
		this.pairedPosOffsets.Clear();
		this.pairedNegOffsets.Clear();
    }
}
