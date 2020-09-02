using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSimulator
{
    private static ParticleSimulator sharedInstance;

    [SerializeField] private LuminairePositionsCalculator positionsCalc;
    [SerializeField] private LightCalculator illuminanceCalc;
    [SerializeField] private EnergyGenerationCalculator energyCalc;

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
        positionsCalc.CalculateFinalLuminairePositions(particle);

        RoadAndLuminaireCreator.Instance().CreateLuminaires(particle);

        illuminanceCalc.CalcIlluminance(particle);

        energyCalc.CalculateEnergyGeneration(particle);
    }

    public void RepeatSimulatedParticle(SolutionParticle particle)
    {
        RoadAndLuminaireCreator.Instance().CreateLuminaires(particle);

        illuminanceCalc.CalcIlluminance(particle);

        energyCalc.CalculateEnergyGeneration(particle);
    }
}
