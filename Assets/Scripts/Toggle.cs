using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Toggle : MonoBehaviour {
	public bool pressed = false;
	public Button button;

	void Start () {
		
	}

	void Update () {
		
	}

	public void click() {
		pressed = !pressed;
		if (pressed)
			button.image.color = Color.gray;
		else
			button.image.color = Color.white;
	}
}
