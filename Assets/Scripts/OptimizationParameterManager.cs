using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptimizationParameterManager
{
    private static OptimizationParameterManager sharedInstance;

    private float RoadLength;
    private float RoadWidth;

    private float MinimumIlluminanceAtAnyPoint;
    private float MinimumAverageIlluminance;
    private float MaximumAverageIlluminance;
    private float LuminaireHeightPivot;
    private float LuminaireCountPivot;
    private float MinimumTargetEnergyGeneration;

    private LuminaireArrangementSettings LuminaireArrangement;
    private bool LessThanHeightPivot;
    private bool EqualToHeightPivot;
    private bool LessThanCountPivot;
    private bool EqualToCountPivot;

    public enum LuminaireArrangementSettings
    {
        Alternating = 1,
        OneSided = 2,
        Paired = 3
    }
    public static OptimizationParameterManager Instance()
    {
        if (sharedInstance == null)
            sharedInstance = new OptimizationParameterManager();
        return sharedInstance;
    }

    private OptimizationParameterManager()
    {

    }

    public void SetRoadLength(float value)
    {
        this.RoadLength = value;
    }

    public float GetRoadLength()
    {
        return this.RoadLength;
    }

    public void SetRoadWidth(float value)
    {
        this.RoadWidth = value;
    }

    public float GetRoadWidth()
    {
        return this.RoadWidth;
    }

    public void SetMinimumIlluminanceAtAnyPoint(float value)
    {
        this.MinimumIlluminanceAtAnyPoint = value;
    }

    public float GetMinimumIlluminanceAtAnyPoint()
    {
        return this.MinimumIlluminanceAtAnyPoint;
    }

    public void SetMinimumAverageIlluminance(float value)
    {
        this.MinimumAverageIlluminance = value;
    }

    public float GetMinimumAverageIlluminance()
    {
        return this.MinimumAverageIlluminance;
    }

    public void SetMaximumAverageIlluminance(float value)
    {
        this.MaximumAverageIlluminance = value;
    }

    public float GetMaximumAverageIlluminance()
    {
        return this.MaximumAverageIlluminance;
    }

    public void SetLuminaireHeightPivot(float value)
    {
        this.LuminaireHeightPivot = value;
    }

    public float GetLuminaireHeightPivot()
    {
        return this.LuminaireHeightPivot;
    }

    public void SetLuminaireCountPivot(float value)
    {
        this.LuminaireCountPivot = value;
    }

    public float GetLuminaireCountPivot()
    {
        return this.LuminaireCountPivot;
    }

    public void SetMinimumTargetEnergyGeneration(float value)
    {
        this.MinimumTargetEnergyGeneration = value;
    }

    public float GetMinimumTargetEnergyGeneration()
    {
        return this.MinimumTargetEnergyGeneration;
    }

    public void SetLuminaireArrangement(LuminaireArrangementSettings setting)
    {
        this.LuminaireArrangement = setting;
    }

    public LuminaireArrangementSettings GetLuminaireArrangement()
    {
        return this.LuminaireArrangement;
    }

    public void SetLessThanHeightPivot(bool value)
    {
        this.LessThanHeightPivot = value;
    }

    public bool GetLessThanHeightPivot()
    {
        return this.LessThanHeightPivot;
    }

    public void ToggleEqualToHeightPivot()
    {
        this.EqualToHeightPivot = !this.EqualToHeightPivot;
    }

    public bool GetEqualToHeightPivot()
    {
        return this.EqualToHeightPivot;
    }

    public void SetLessThanCountPivot(bool value)
    {
        this.LessThanCountPivot = value;
    }

    public bool GetLessThanCountPivot()
    {
        return this.LessThanCountPivot;
    }

    public void ToggleEqualToCountPivot()
    {
        this.EqualToCountPivot = !this.EqualToCountPivot;
    }

    public bool GetEqualToCountPivot()
    {
        return this.EqualToCountPivot;
    }
}
