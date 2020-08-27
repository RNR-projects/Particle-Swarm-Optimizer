using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AverageScore : MonoBehaviour {
	[HideInInspector] public float averageSunHours;

	void Start () {
		
	}

	void Update () {
		
	}

	public void setScore(float score) {
		averageSunHours = score;
	}
}
