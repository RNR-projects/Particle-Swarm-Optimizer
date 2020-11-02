using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwarmOptimizer : MonoBehaviour {
	private int particleCount;
	private int iterationsToRun;

	private float c1 = 0.75f;
	private float c2 = 0.75f;
	private float r1;
	private float r2;

	private float bestSpacing;
	private float bestHeight;
	private SolutionParticle bestParticle;
	private bool bestIsBuildable;

	private float allTimeBestSpacing;
	private float allTimeBestHeight;

	private int currentIteration;
	private int consecutivelyBestCount;

	private List<float> devFromAverage;
	private List<float> devFromMinimum;
	private List<float> trialDevFromAverage;
	private List<float> trialDevFromMinimum;

	private List<float> spacingVelocity;
	private List<float> heightVelocity;

	private List<float> personalBestSpacing;
	private List<float> personalBestHeight;

	public bool optimizationIsDone;

	void Awake() {
		this.particleCount = OptimizationParameterManager.PARTICLECOUNT;
		this.iterationsToRun = OptimizationParameterManager.ITERATIONLIMIT;

		this.devFromAverage = new List<float>();
		this.devFromMinimum = new List<float>();
		this.trialDevFromAverage = new List<float>();
		this.trialDevFromMinimum = new List<float>();
		this.spacingVelocity = new List<float>();
		this.heightVelocity = new List<float>();
		this.personalBestHeight = new List<float>();
		this.personalBestSpacing = new List<float>();
		this.currentIteration = 0;
	}

	public void InitializeOptimization()
	{
		this.optimizationIsDone = false;
		this.r1 = Random.value;
		this.r2 = Random.value;

		List<SolutionParticle> particles = ParticleGenerator.Instance().InitializeNewParticles(this.particleCount);

		for (int i = 0; i < this.particleCount; i++)
		{
			this.spacingVelocity.Add(0);
			this.heightVelocity.Add(0);
			this.personalBestHeight.Add(particles[i].height);
			this.personalBestSpacing.Add(particles[i].spacing);
			this.trialDevFromAverage.Add(0);
			this.trialDevFromMinimum.Add(0);
		}

		StartCoroutine(this.BeginFirstIteration(particles));
	}

    IEnumerator BeginFirstIteration(List<SolutionParticle> particles)
    {
		OptimizationParameterManager parameters = OptimizationParameterManager.Instance();

        for (int i = 0; i < particleCount; i++)
        {
			ParticleSimulator.Instance().SimulateParticle(particles[i]);

			yield return new WaitForSeconds(0.1f);

			if (particles[i].averageIlluminance >= parameters.GetMinimumAverageIlluminance() &&
				particles[i].averageIlluminance <= parameters.GetMaximumAverageIlluminance())
			{
				this.devFromAverage.Add(0);
			}
			else if (particles[i].averageIlluminance < parameters.GetMinimumAverageIlluminance())
			{
				this.devFromAverage.Add(parameters.GetMinimumAverageIlluminance() - particles[i].averageIlluminance);
			}
			else
			{
				this.devFromAverage.Add(particles[i].averageIlluminance - parameters.GetMaximumAverageIlluminance());
			}
			if (particles[i].lowestIlluminanceAtAPoint >= parameters.GetMinimumIlluminanceAtAnyPoint())
			{
				this.devFromMinimum.Add(0);
			}
			else
			{
				this.devFromMinimum.Add(parameters.GetMinimumIlluminanceAtAnyPoint() - particles[i].lowestIlluminanceAtAPoint);
			}
        }

        this.SetBest(particles);

		//yield return new WaitForSeconds(.1f);

		StartCoroutine(this.OptimizeSwarm(particles));	
    }

    IEnumerator OptimizeSwarm(List<SolutionParticle> particles)
    {
		OptimizationParameterManager parameters = OptimizationParameterManager.Instance();
        for (int i = 0; i < iterationsToRun; i++)
        {
			RoadAndLuminaireCreator.Instance().ClearLuminaires();

			List<SolutionParticle> trialParticles = this.AdjustParticles(particles);
			//check if the new parameters of the particles is better than the old ones
			//this.RunIteration(trialParticles);
			
			for (int j = 0; j < particleCount; j++)
			{
				particles[j].xOffset = 0;

				ParticleSimulator.Instance().SimulateParticle(particles[j]);
				
				yield return new WaitForSeconds(0.1f);
				
				if (particles[j].averageIlluminance >= parameters.GetMinimumAverageIlluminance() &&
							particles[j].averageIlluminance <= parameters.GetMaximumAverageIlluminance())
					trialDevFromAverage[j] = 0;
				else if (particles[j].averageIlluminance < parameters.GetMinimumAverageIlluminance())
					trialDevFromAverage[j] = parameters.GetMinimumAverageIlluminance() - particles[j].averageIlluminance;
				else
					trialDevFromAverage[j] = particles[j].averageIlluminance - parameters.GetMaximumAverageIlluminance();
				if (particles[j].lowestIlluminanceAtAPoint >= parameters.GetMinimumIlluminanceAtAnyPoint())
					trialDevFromMinimum[j] = 0;
				else
					trialDevFromMinimum[j] = parameters.GetMinimumIlluminanceAtAnyPoint() - particles[j].lowestIlluminanceAtAPoint;
			
				//if they are better, replace the previous particle with its newer version
                if (particles[j].spacing > 0 && particles[j].height > 0)
                {
                    if (trialDevFromAverage[j] < this.devFromAverage[j])
                        RegisterBetterParticle(j, particles, trialParticles[j]);
                    else if (trialDevFromAverage[j] == this.devFromAverage[j])
                    {
                        if (trialDevFromMinimum[j] < this.devFromMinimum[j])
                            RegisterBetterParticle(j, particles, trialParticles[j]);
                        else if (trialDevFromMinimum[j] == this.devFromMinimum[j])
                        {
                            if (trialParticles[j].lightingEfficiency > particles[j].lightingEfficiency)
                                RegisterBetterParticle(j, particles, trialParticles[j]);
                            else if (trialParticles[j].lightingEfficiency <= particles[j].lightingEfficiency &&
                                            trialParticles[j].lowestEnergyGeneratedByALuminaire > particles[j].lowestEnergyGeneratedByALuminaire)
                                RegisterBetterParticle(j, particles, trialParticles[j]);
                        }
                    }
                }
            }

			this.currentIteration++;

            SetBest(particles);

			if (this.consecutivelyBestCount > 9) {				
				this.optimizationIsDone = true;
				//if (!this.bestIsBuildable)
				//	InitializeOptimization ();
				break;
			}
			if (this.currentIteration + 1 == this.iterationsToRun) {
				this.optimizationIsDone = true;
				//if (!this.bestIsBuildable)
				//	InitializeOptimization ();
			}
			yield return new WaitForSeconds (1.0f);
        }
		//yield return new WaitForSeconds(1.0f);
		ParticleSimulator.Instance().RepeatSimulatedParticle(this.bestParticle);
	}

	private List<SolutionParticle> AdjustParticles(List<SolutionParticle> particles)
    {
		OptimizationParameterManager parameters = OptimizationParameterManager.Instance();

		for (int j = 0; j < particleCount; j++)
		{
			this.spacingVelocity[j] = CalcVelocity(this.spacingVelocity[j], this.personalBestSpacing[j], particles[j].spacing, this.bestSpacing);
			this.heightVelocity[j] = CalcVelocity(this.heightVelocity[j], this.personalBestHeight[j], particles[j].height, this.bestHeight);

			particles[j].xOffset = 0;

			if (parameters.GetLessThanCountPivot() && this.spacingVelocity[j] + particles[j].spacing < 
								parameters.GetRoadLength() / (parameters.GetLuminaireCountPivot() - 1))
			{
				if (parameters.GetEqualToCountPivot())
					particles[j].spacing = parameters.GetRoadLength() / (parameters.GetLuminaireCountPivot() - 1f);
				else
					particles[j].spacing = parameters.GetRoadLength() / (parameters.GetLuminaireCountPivot() - 1f) + .5f;
			}
			else if (parameters.GetGreaterThanCountPivot() && (this.spacingVelocity[j] + particles[j].spacing > 
						parameters.GetRoadLength() / parameters.GetLuminaireCountPivot() || 
						this.spacingVelocity[j] + particles[j].spacing <= 5))
			{
				if (this.spacingVelocity[j] + particles[j].spacing <= 5)
					particles[j].spacing = 6f;
				else if (parameters.GetEqualToCountPivot())
					particles[j].spacing = parameters.GetRoadLength() / parameters.GetLuminaireCountPivot();
				else
					particles[j].spacing = parameters.GetRoadLength() / parameters.GetLuminaireCountPivot() - .5f;
			}
			else if (this.spacingVelocity[j] + particles[j].spacing > parameters.GetRoadLength())
				particles[j].spacing = parameters.GetRoadLength();
			else if (parameters.GetEqualToCountPivot() && (this.spacingVelocity[j] + particles[j].spacing > 
						parameters.GetRoadLength() / (parameters.GetLuminaireCountPivot() - 1f) || 
						this.spacingVelocity[j] + particles[j].spacing < parameters.GetRoadLength() / parameters.GetLuminaireCountPivot()))
			{
				if (this.spacingVelocity[j] + particles[j].spacing > parameters.GetRoadLength() / (parameters.GetLuminaireCountPivot() - 1f))
					particles[j].spacing = parameters.GetRoadLength() / (parameters.GetLuminaireCountPivot() - 1f);
				else
					particles[j].spacing = parameters.GetRoadLength() / parameters.GetLuminaireCountPivot();
			}
			else
				particles[j].spacing += this.spacingVelocity[j];

			particles[j].luminaireCount = Mathf.FloorToInt(parameters.GetRoadLength() / particles[j].spacing) + 1;
			if (parameters.GetLuminaireArrangement() == OptimizationParameterManager.LuminaireArrangementSettings.Paired)
				particles[j].luminaireCount *= 2;

			if (this.heightVelocity[j] + particles[j].height < 6f)
				particles[j].height = 6f;
			else if (parameters.GetLessThanHeightPivot() &&	
						this.heightVelocity[j] + particles[j].height > parameters.GetLuminaireHeightPivot())
			{
				if (parameters.GetEqualToHeightPivot())
					particles[j].height = parameters.GetLuminaireHeightPivot();
				else
					particles[j].height = parameters.GetLuminaireHeightPivot() - .5f;
			}
			else if (parameters.GetGreaterThanHeightPivot() && 
						this.heightVelocity[j] + particles[j].height < parameters.GetLuminaireHeightPivot())
			{
				if (parameters.GetEqualToHeightPivot())
					particles[j].height = parameters.GetLuminaireHeightPivot();
				else
					particles[j].height = parameters.GetLuminaireHeightPivot() + .5f;
			}
			else if (parameters.GetEqualToHeightPivot())
				particles[j].height = parameters.GetLuminaireHeightPivot();
			else
				particles[j].height += this.heightVelocity[j];
		}

		return ParticleGenerator.Instance().CopyParticles(particles);
	}

	/*private void RunIteration(List<SolutionParticle> particles)
    {
		OptimizationParameterManager parameters = OptimizationParameterManager.Instance();

		for (int j = 0; j < particleCount; j++)
		{
			particles[j].xOffset = 0;

			ParticleSimulator.Instance().SimulateParticle(particles[j]);

			if (particles[j].averageIlluminance >= parameters.GetMinimumAverageIlluminance() &&
						particles[j].averageIlluminance <= parameters.GetMaximumAverageIlluminance())
				trialDevFromAverage[j] = 0;
			else if (particles[j].averageIlluminance < parameters.GetMinimumAverageIlluminance())
				trialDevFromAverage[j] = parameters.GetMinimumAverageIlluminance() - particles[j].averageIlluminance;
			else
				trialDevFromAverage[j] = particles[j].averageIlluminance - parameters.GetMaximumAverageIlluminance();
			if (particles[j].lowestIlluminanceAtAPoint >= parameters.GetMinimumIlluminanceAtAnyPoint())
				trialDevFromMinimum[j] = 0;
			else
				trialDevFromMinimum[j] = parameters.GetMinimumIlluminanceAtAnyPoint() - particles[j].lowestIlluminanceAtAPoint;
		}
	}*/

    private void RegisterBetterParticle(int index, List<SolutionParticle> trueParticleList, SolutionParticle betterParticle)
    {
        this.personalBestSpacing[index] = betterParticle.spacing;
        this.personalBestHeight[index] = betterParticle.height;

		this.devFromAverage[index] = trialDevFromAverage[index];
		this.devFromMinimum[index] = trialDevFromMinimum[index];

		trueParticleList[index] = betterParticle;     
    }

    private void SetBest(List<SolutionParticle> particles)
    {	
        List<float> stage1Candidates = new List<float>();
        List<int> stage1CandidatesIndices = new List<int>();
		//stage 1 is filtering those that do not meet the minimum deviation from average illuminance which should be ideally 0
		for (int i = 0; i < particleCount; i++)
        {
			if (this.devFromAverage[i] == Mathf.Min(this.devFromAverage.ToArray()) && particles[i].isBuildable)
            {
                stage1Candidates.Add(this.devFromMinimum[i]);
                stage1CandidatesIndices.Add(i);
            }
        }

        List<float> stage2Candidates = new List<float>();
		List<int> stage2CandidateIndices = new List<int>();
		//stage 2 is filtering those that do not meet the minimum deviation from lowest illuminance at a point which should ideally be 0 as well
        for (int i = 0; i < stage1Candidates.Count; i++)
        {
            if (stage1Candidates[i] == Mathf.Min(stage1Candidates.ToArray()))
            {
				stage2Candidates.Add(particles[stage1CandidatesIndices[i]].lowestEnergyGeneratedByALuminaire);
                stage2CandidateIndices.Add(stage1CandidatesIndices[i]);
            }
        }

		List<float> finalCandidates = new List<float>();
		List<float> failedStage3Candidates = new List<float>();
		List<int> finalCandidateIndices = new List<int>();
		List<int> failedStage3CandidateIndices = new List<int>();
		//stage 3 filters those that do not meet the energy generation requirement
		for (int i = 0; i < stage2Candidates.Count; i++) {
			if (stage2Candidates[i] >= OptimizationParameterManager.Instance().GetMinimumTargetEnergyGeneration()) {
				finalCandidates.Add(particles[stage2CandidateIndices[i]].lightingEfficiency);
				finalCandidateIndices.Add(stage2CandidateIndices[i]);
			}
			else
            {
				failedStage3Candidates.Add(stage2Candidates[i] - OptimizationParameterManager.Instance().GetMinimumTargetEnergyGeneration());
				failedStage3CandidateIndices.Add(stage2CandidateIndices[i]);
            }
		}

		int index = 0;
		//get the index of the "best" particle
		if (finalCandidates.Count == 0)
		{
			if (stage2Candidates.Count == 0)
			{	//if stage2candidates is empty, then stage1candidates must also be empty
				index = this.devFromAverage.IndexOf(Mathf.Min(this.devFromAverage.ToArray()));
			}
			else
			{
				index = failedStage3CandidateIndices[failedStage3Candidates.IndexOf(Mathf.Max(failedStage3Candidates.ToArray()))];
			}
		}
		else
		{
			index = finalCandidateIndices[finalCandidates.IndexOf(Mathf.Max(finalCandidates.ToArray()))];
		}

        this.bestSpacing = particles[index].spacing;
        this.bestHeight = particles[index].height;
		this.bestIsBuildable = particles[index].isBuildable;
		this.bestParticle = ParticleGenerator.Instance().CopyParticle(particles[index]);

		if (currentIteration > Mathf.FloorToInt (iterationsToRun / 2.5f)) {
			//if the current best particle is within 0.5m of the all time best particle's spacing and height, count it as being consecutively the best
			if (this.allTimeBestSpacing <= this.bestSpacing + .5f && this.allTimeBestSpacing >= this.bestSpacing - .5f && 
					this.allTimeBestHeight <= this.bestHeight + .5f && this.allTimeBestHeight >= this.bestHeight - .5f)
				this.consecutivelyBestCount++;
			else {
				this.consecutivelyBestCount = 0;
				this.allTimeBestSpacing = this.bestSpacing;
				this.allTimeBestHeight = this.bestHeight;
			}
		}
		//set the best particle's velocity to 0 so that it does not change until it is beaten by another
        this.spacingVelocity[index] = 0;
        this.heightVelocity[index] = 0;
    }
    
    private float CalcVelocity(float velocity,float closest, float position, float best){
        return velocity + c1 * r1 * (closest - position) + c2 * r2 * (best - position);
    }

	public void ClearData()
	{
		this.consecutivelyBestCount = 0;
		this.allTimeBestSpacing = 0;
		this.allTimeBestHeight = 0;
		this.currentIteration = 0;
		IlluminationPointsCreator.Instance().ClearIlluminationPoints();
		RoadAndLuminaireCreator.Instance().ClearRoad();
		StructureBuilder.Instance().ClearStructures();

		this.devFromAverage.Clear();
		this.devFromMinimum.Clear();
		this.trialDevFromAverage.Clear();
		this.trialDevFromMinimum.Clear();

		this.spacingVelocity.Clear();
		this.heightVelocity.Clear();

		this.personalBestSpacing.Clear();
		this.personalBestHeight.Clear();
	}

	public SolutionParticle GetBestParticle()
    {
		return this.bestParticle;
    }

	public int GetIterationsDone()
    {
		return this.currentIteration;
    }
}
