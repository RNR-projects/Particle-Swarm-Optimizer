﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour {
	[SerializeField] private InputField roadLength, roadWidth, minimumIlluminance, maximumAverageIlluminance, minimumAverageIlluminance, 
		heightLimit, luminaireLimit, minSunlight;
	[SerializeField] private Toggle lessThanHeight, equalToHeight, greaterThanHeight, lessThanCount, equalToCount, greaterThanCount,
		alternatingArrangement, oneSidedArrangement, pairedArrangement;
	private ProgressLevel currentProgress = ProgressLevel.RoadParameters;
	[SerializeField] private GameObject roadParameterUI, lightingParameterUI;
	[SerializeField] private Slider globalSizeAdjuster;
	private enum ProgressLevel
    {
		RoadParameters = 1,
		OptimizerParameters = 2
    }

	void Awake () {
		PressAlternatingArrangement();//default setting
		roadLength.onEndEdit.AddListener(AdjustRoadLength);
		roadWidth.onEndEdit.AddListener(AdjustRoadWidth);
		minimumIlluminance.onEndEdit.AddListener(AdjustMinimumIlluminance);
		maximumAverageIlluminance.onEndEdit.AddListener(AdjustHighestAverageIlluminance);
		minimumAverageIlluminance.onEndEdit.AddListener(AdjustLowestAverageIlluminance);
		heightLimit.onEndEdit.AddListener(AdjustHeightPivot);
		luminaireLimit.onEndEdit.AddListener(AdjustLuminaireCountPivot);
		minSunlight.onEndEdit.AddListener(AdjustMinimumEnergyToGenerate);
	}

	private void AdjustRoadLength(string s) {
		float x;
		if (float.TryParse(s, out x))
			OptimizationParameterManager.Instance().SetRoadLength(x);
	}

	private void AdjustRoadWidth(string s) {
		float x;
		if (float.TryParse(s, out x))
			OptimizationParameterManager.Instance().SetRoadWidth(x);
	}

	private void AdjustMinimumIlluminance(string s) {
		float x;
		if (float.TryParse(s, out x))
			OptimizationParameterManager.Instance().SetMinimumIlluminanceAtAnyPoint(x);
	}

	private void AdjustHighestAverageIlluminance(string s) {
		float x;
		if (float.TryParse(s, out x))
			OptimizationParameterManager.Instance().SetMaximumAverageIlluminance(x);
	}

	private void AdjustLowestAverageIlluminance(string s) {
		float x;
		if (float.TryParse(s, out x))
			OptimizationParameterManager.Instance().SetMinimumAverageIlluminance(x);
	}

	private void AdjustHeightPivot(string s) {
		float x;
		if (float.TryParse(s, out x))
			OptimizationParameterManager.Instance().SetLuminaireHeightPivot(x);
	}

	private void AdjustLuminaireCountPivot(string s) {
		float x;
		if (float.TryParse(s, out x))
			OptimizationParameterManager.Instance().SetLuminaireCountPivot(x);
	}

	private void AdjustMinimumEnergyToGenerate(string s) {
		float x;
		if (float.TryParse(s, out x))
			OptimizationParameterManager.Instance().SetMinimumTargetEnergyGeneration(x);
	}

	public void ExitGame() {
		Application.Quit ();
	}

	public void PressLessThanHeight()
    {
		if (greaterThanHeight.pressed)
			greaterThanHeight.click();
		else if (lessThanHeight.pressed)
		{
			if (equalToHeight.pressed)
				lessThanHeight.click();
		}
		else
			lessThanHeight.click();
		OptimizationParameterManager.Instance().SetLessThanHeightPivot(lessThanHeight.pressed);
    }

	public void PressEqualToHeight()
    {
		OptimizationParameterManager.Instance().ToggleEqualToHeightPivot();
    }

	public void PressGreaterThanHeight()
    {
		if (lessThanHeight.pressed)
			lessThanHeight.click();
		else if (greaterThanHeight.pressed)
		{
			if (equalToHeight.pressed)
				greaterThanHeight.click();
		}
		else
			greaterThanHeight.click();
		OptimizationParameterManager.Instance().SetLessThanHeightPivot(lessThanHeight.pressed);
	}

	public void PressLessThanCount()
    {
		if (greaterThanCount.pressed)
			greaterThanCount.click();
		else if (lessThanCount.pressed)
		{
			if (equalToCount.pressed)
				lessThanCount.click();
		}
		else
			lessThanCount.click();
		OptimizationParameterManager.Instance().SetLessThanCountPivot(lessThanCount.pressed);
    }

	public void PressEqualToCount()
    {
		OptimizationParameterManager.Instance().ToggleEqualToCountPivot();
    }

	public void PressGreaterThanCount()
    {
		if (lessThanCount.pressed)
			lessThanCount.click();
		else if (greaterThanCount.pressed)
		{
			if (equalToCount.pressed)
				greaterThanCount.click();
		}
		else
			greaterThanCount.click();
		OptimizationParameterManager.Instance().SetLessThanCountPivot(lessThanCount.pressed);
	}

	public void PressAlternatingArrangement()
    {
		if (oneSidedArrangement.pressed || pairedArrangement.pressed)
		{
			if (oneSidedArrangement.pressed)
				oneSidedArrangement.click();
			else if (pairedArrangement.pressed)
				pairedArrangement.click();
			alternatingArrangement.click();
		}
		OptimizationParameterManager.Instance().SetLuminaireArrangement(OptimizationParameterManager.LuminaireArrangementSettings.Alternating);
    }

	public void PressOneSidedArrangement()
    {
		if (alternatingArrangement.pressed || pairedArrangement.pressed)
		{
			if (alternatingArrangement.pressed)
				alternatingArrangement.click();
			else if (pairedArrangement.pressed)
				pairedArrangement.click();
			oneSidedArrangement.click();
		}
		OptimizationParameterManager.Instance().SetLuminaireArrangement(OptimizationParameterManager.LuminaireArrangementSettings.OneSided);
	}

	public void PressPairedArrangement()
    {
		if (alternatingArrangement.pressed || oneSidedArrangement.pressed)
		{
			if (alternatingArrangement.pressed)
				alternatingArrangement.click();
			else if (oneSidedArrangement.pressed)
				oneSidedArrangement.click();
			pairedArrangement.click();
		}
		OptimizationParameterManager.Instance().SetLuminaireArrangement(OptimizationParameterManager.LuminaireArrangementSettings.Paired);
	}

	public void ProceedToGrid()
    {
		this.currentProgress = ProgressLevel.OptimizerParameters;
		this.roadParameterUI.SetActive(false);
		this.lightingParameterUI.SetActive(true);
    }

	public void ReturnToRoadParameters()
    {
		this.currentProgress = ProgressLevel.RoadParameters;
		this.lightingParameterUI.SetActive(false);
		this.roadParameterUI.SetActive(true);
    }

	public void ChangeGlobalScale ()
    {
		GlobalScaler.Instance().SetGlobalScale(this.globalSizeAdjuster.value * 10f);
    }
}
