using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Switch : MonoBehaviour {
	public Button switchCam;
	public Camera cam;

	void Start () {
		
	}

	void Update () {
		switchCam.onClick.AddListener(swap);
	}

	private void swap() {
		cam.enabled = true;
	}
}
