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
    private int LuminaireCountPivot;
    private float MinimumTargetEnergyGeneration;

    private LuminaireArrangementSettings LuminaireArrangement;
    private bool LessThanHeightPivot;
    private bool GreaterThanHeightPivot;
    private bool EqualToHeightPivot;
    private bool LessThanCountPivot;
    private bool GreaterThanCountPivot;
    private bool EqualToCountPivot;

    private float roadDirection;

    public const int LONGCALPOINTS = 15;//shorthand for longitudinal calculation points
    public const int ITERATIONLIMIT = 100;
    public const int PARTICLECOUNT = 30;

    public const int LAMPLUMEN = 9299;
    public const int LUMINAIREPOWER = 96;
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
        this.MinimumAverageIlluminance = 7.5f;
        this.MaximumAverageIlluminance = 11.5f;
        this.MinimumIlluminanceAtAnyPoint = 2;
        this.RoadLength = 50;
        this.RoadWidth = 6;
        this.roadDirection = 0;
        this.LuminaireCountPivot = 1;
        this.LuminaireHeightPivot = 6;
        this.LuminaireArrangement = LuminaireArrangementSettings.Alternating;
        this.GreaterThanCountPivot = true;
        this.GreaterThanHeightPivot = true;
        this.EqualToHeightPivot = false;
        this.EqualToCountPivot = false;
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
        this.LuminaireHeightPivot = Mathf.Max(value, 6);
    }

    public float GetLuminaireHeightPivot()
    {
        return this.LuminaireHeightPivot;
    }

    public void SetLuminaireCountPivot(int value)
    {
        this.LuminaireCountPivot = Mathf.Max(value, 1);
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

    public void SetGreaterThanHeightPivot(bool value)
    {
        this.GreaterThanHeightPivot = value;
    }

    public bool GetGreaterThanHeightPivot()
    {
        return this.GreaterThanHeightPivot;
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

    public void SetGreaterThanCountPivot(bool value)
    {
        this.GreaterThanCountPivot = value;
    }

    public bool GetGreaterThanCountPivot()
    {
        return this.GreaterThanCountPivot;
    }

    public void ToggleEqualToCountPivot()
    {
        this.EqualToCountPivot = !this.EqualToCountPivot;
    }

    public bool GetEqualToCountPivot()
    {
        return this.EqualToCountPivot;
    }

    public float GetRoadDirection()
    {
        return this.roadDirection;
    }

    public void SetRoadDirection(float value)
    {
        this.roadDirection = value;
    }
}
