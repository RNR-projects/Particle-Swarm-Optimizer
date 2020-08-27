using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour {
	public Vector3 offset;
	public Text text;

	void Start () {

	}

	void Update () {
		
	}

	public void setLocation() {
		this.transform.position = Input.mousePosition + offset;
	}

	public void GenerateLines() {
		text.text = text.text.Replace ("\\n", "\n");
	}
}
