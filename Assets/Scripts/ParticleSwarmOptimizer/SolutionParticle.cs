using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SolutionParticle : MonoBehaviour
{
	public float xOffset;
	public float lowestEnergyGeneratedByALuminaire;

	public float spacing;
	public float height;
	public int luminaireCount;
	public bool isReversed;

	public float spacingVelocity;
	public float heightVelocity;

	public float closestSpacing;
	public float closestHeight;

	public bool isBuildable;

	public float averageIlluminance;
	public float lowestIlluminanceAtAPoint;
	public float lightingEfficiency;

	public float deviationFromAverageIlluminanceLimit;
	public float deviationFromMinimumIlluminanceAtAPoint;
}
