using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightLevel : MonoBehaviour {
    [HideInInspector] public float illuminance;

	void Start () {
		
	}

	public void Show() {
        Debug.Log(illuminance);
	}
    public void SetIlluminance(float x)
    {
        illuminance = x;
    }
}
