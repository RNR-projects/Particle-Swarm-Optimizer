using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSimulator
{
    private static ParticleSimulator sharedInstance;

    public static ParticleSimulator Instance()
    {
        if (sharedInstance == null)
            sharedInstance = new ParticleSimulator();
        return sharedInstance;
    }

    private ParticleSimulator()
    {

    }

    public void SimulateParticle(SolutionParticle particle)
    {
        LuminairePositionsCalculator.Instance().CalculateFinalLuminairePositions(particle);

        RoadAndLuminaireCreator.Instance().CreateLuminaires(particle);

        IlluminanceCalculator.Instance().CalcIlluminance(particle);

        EnergyGenerationCalculator.Instance().CalculateEnergyGeneration(particle);

        RoadAndLuminaireCreator.Instance().ClearLuminaires();
    }

    public void RepeatSimulatedParticle(SolutionParticle particle)
    {
        RoadAndLuminaireCreator.Instance().CreateLuminaires(particle);

        IlluminanceCalculator.Instance().CalcIlluminance(particle);

        EnergyGenerationCalculator.Instance().CalculateEnergyGeneration(particle);
    }
}
