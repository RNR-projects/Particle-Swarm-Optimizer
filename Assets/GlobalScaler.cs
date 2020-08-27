using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GlobalScaler : MonoBehaviour {
	public float GlobalScale;
	public Slider scaler;

	void Start () {
		scaler.onValueChanged.AddListener (delegate {changeScale ();});
	}

	void Update () {
		
	}

	public void changeScale() {
		GlobalScale = scaler.value * 10f;
	}
}
