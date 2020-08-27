using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Builder : MonoBehaviour {
	private Grid grid;
	public Text text;
	public Camera cam1, cam2;
	public InputField field;

	void Start () {
		grid = GameObject.Find ("Grid").GetComponent<Grid> ();
	}

	void Update () {
		if (cam1.enabled)
			Cursor.lockState = CursorLockMode.None;
	}

	public void EnterHeight() {
		float x;
		if (float.TryParse (text.text, out x)) {
			float[] tempValues = new float[0];
			if (grid.structureHeight.Length > 0) {
				tempValues = new float[grid.structureHeight.Length];
				for (int i = 0; i < grid.structureHeight.Length; i++) {
					tempValues [i] = grid.structureHeight [i];
				}
			}
			grid.structureHeight = new float[grid.structureHeight.Length + 1];
			if (grid.structureHeight.Length > 1) {
				for (int i = 0; i < grid.structureHeight.Length - 1; i++) {
					grid.structureHeight [i] = tempValues [i];
				}
			}
			grid.structureHeight [grid.structureHeight.Length - 1] = x;
			Cursor.lockState = CursorLockMode.Locked;
			cam2.GetComponent<Cam> ().Vangle = grid.prevAngle;
			cam1.enabled = false;
			cam2.enabled = true;
			field.text = " ";
		}
		else
			text.text = "Invalid";
	}
}
