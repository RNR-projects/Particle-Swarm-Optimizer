using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SolutionParticle : MonoBehaviour
{	//instantiated with the particle
	public float xOffset;	

	public float spacing;
	public float height;

	public int luminaireCount;

	public bool isReversed;
	public bool isBuildable;

	//to be assigned during the simulation process
	public float averageIlluminance;
	public float lowestIlluminanceAtAPoint;
	public float lightingEfficiency;
	public float lowestEnergyGeneratedByALuminaire;
}
