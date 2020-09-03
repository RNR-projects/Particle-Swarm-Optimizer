using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleGenerator
{
	private static ParticleGenerator sharedInstance;

	public static ParticleGenerator Instance()
    {
		if (sharedInstance == null)
			sharedInstance = new ParticleGenerator();
		return sharedInstance;
    }

	private ParticleGenerator()
    {

    }

	public List<SolutionParticle> InitializeNewParticles(int particleCount)
    {
		OptimizationParameterManager parameters = OptimizationParameterManager.Instance();
		List<SolutionParticle> particles = new List<SolutionParticle>();

		for (int i = 0; i < particleCount; i++)
        {
			SolutionParticle newParticle = new SolutionParticle();
			if (parameters.GetLessThanCountPivot())
			{
				if (parameters.GetEqualToCountPivot())
				{
					newParticle.spacing = Random.Range(Mathf.Max(6f, parameters.GetRoadLength() /
						(parameters.GetLuminaireCountPivot() - 1f)),
						parameters.GetRoadLength());
				}
				else
                {
					newParticle.spacing = Random.Range(Mathf.Max(6f, parameters.GetRoadLength() /
						(parameters.GetLuminaireCountPivot() - 2f)),
						parameters.GetRoadLength());
				}
			}
			else if (parameters.GetGreaterThanCountPivot())
            {
				if (parameters.GetEqualToCountPivot())
                {
					newParticle.spacing = Random.Range(6f, parameters.GetRoadLength() /
						(parameters.GetLuminaireCountPivot() - 1f));
				}
				else
                {
					newParticle.spacing = Random.Range(6f, parameters.GetRoadLength() /
						(parameters.GetLuminaireCountPivot()));
				}
            }
			else
				newParticle.spacing = Random.Range(parameters.GetRoadLength() / 
					parameters.GetLuminaireCountPivot(), 
					parameters.GetRoadLength() / 
					(parameters.GetLuminaireCountPivot() - 1f));
			if (parameters.GetLessThanHeightPivot())
			{
				if (parameters.GetEqualToHeightPivot())
					newParticle.height = Random.Range(6f, parameters.GetLuminaireHeightPivot());
				else
					newParticle.height = Random.Range(6f, parameters.GetLuminaireHeightPivot() - 0.1f);
			}
			else if (parameters.GetGreaterThanHeightPivot())
            {
				if (parameters.GetEqualToHeightPivot())
					newParticle.height = Random.Range(Mathf.Max(6f, parameters.GetLuminaireHeightPivot()), 20f);
				else
					newParticle.height = Random.Range(Mathf.Max(6f, parameters.GetLuminaireHeightPivot() + 0.1f), 20f);
            }
			else
				newParticle.height = Mathf.Clamp(parameters.GetLuminaireHeightPivot(), 6f, 20f);

			newParticle.luminaireCount = Mathf.FloorToInt(parameters.GetRoadLength() / newParticle.spacing) + 1;
			if (parameters.GetLuminaireArrangement() == OptimizationParameterManager.LuminaireArrangementSettings.Paired)
				newParticle.luminaireCount *= 2;
			newParticle.isBuildable = true;
			newParticle.isReversed = false;
			newParticle.xOffset = 0;

			particles.Add(newParticle);
		}

		return particles;
    }

	public List<SolutionParticle> CopyParticles(List<SolutionParticle> particles)
    {
		List<SolutionParticle> copiedList = new List<SolutionParticle>();

		foreach(SolutionParticle particle in particles)
        {
			SolutionParticle newParticle = new SolutionParticle();

			newParticle.spacing = particle.spacing;
			newParticle.height = particle.height;
			newParticle.luminaireCount = particle.luminaireCount;
			newParticle.isBuildable = particle.isBuildable;
			newParticle.isReversed = particle.isReversed;
			newParticle.xOffset = particle.xOffset;

			copiedList.Add(newParticle);
        }

		return copiedList;
    }

	public SolutionParticle CopyParticle(SolutionParticle particle)
	{

		SolutionParticle newParticle = new SolutionParticle();

		newParticle.spacing = particle.spacing;
		newParticle.height = particle.height;
		newParticle.luminaireCount = particle.luminaireCount;
		newParticle.isBuildable = particle.isBuildable;
		newParticle.isReversed = particle.isReversed;
		newParticle.xOffset = particle.xOffset;

		return newParticle;
	}
}
