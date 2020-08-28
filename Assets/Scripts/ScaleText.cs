using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScaleText : MonoBehaviour {
	public Slider scaler;
	public Text text;

	void Start () {
		scaler.onValueChanged.AddListener (delegate {changeScale ();});
	}

	void Update () {
		
	}

	public void changeScale() {
		text.text = "Scale: " + scaler.value;
	}
}
