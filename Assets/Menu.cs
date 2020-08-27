using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour {
	public InputField roadLength, roadWidth, minimumIlluminance, maximumAverageIlluminance, minimumAverageIlluminance, heightLimit, luminaireLimit, minSunlight;
	private SwarmOptimizer swarm;
	public Button button1, button2, button3, button4, button5, button6, button7, button8, button9, exit;
	private Toggle Button1, Button2, Button3, Button4, Button5, Button6, Button7, Button8, Button9;
	private bool bool1 = false, bool2 = false, bool3 = false, bool4 = false, bool5 = false, bool6 = false, bool7 = false;

	void Start () {
		swarm = GameObject.Find("Swarm").GetComponent<SwarmOptimizer>();
		Button1 = button1.GetComponent<Toggle>();
		Button2 = button2.GetComponent<Toggle>();
		Button3 = button3.GetComponent<Toggle>();
		Button4 = button4.GetComponent<Toggle>();
		Button5 = button5.GetComponent<Toggle>();
		Button6 = button6.GetComponent<Toggle>();
		Button7 = button7.GetComponent<Toggle>();
		Button8 = button8.GetComponent<Toggle>();
		Button9 = button9.GetComponent<Toggle>();
		Button7.click ();
	}

	void Update () {
		if (Button1.pressed && !Button3.pressed)
			bool1 = true;
		else if (!Button1.pressed)
			bool1 = false;
		if (!Button1.pressed && Button3.pressed)
			bool2 = true;
		else if (!Button3.pressed)
			bool2 = false;
		if (Button4.pressed && !Button6.pressed)
			bool3 = true;
		else if (!Button4.pressed)
			bool3 = false;
		if (!Button4.pressed && Button6.pressed)
			bool4 = true;
		else if (!Button6.pressed)
			bool4 = false;
		if (Button7.pressed && !Button8.pressed && !Button9.pressed)
			bool5 = true;
		else if (!Button7.pressed)
			bool5 = false;
		if (!Button7.pressed && Button8.pressed && !Button9.pressed)
			bool6 = true;
		else if (!Button8.pressed)
			bool6 = false;
		if (!Button7.pressed && !Button8.pressed && Button9.pressed)
			bool7 = true;
		else if (!Button9.pressed)
			bool7 = false;
		if (Button3.pressed && bool1) {
			Button1.click ();
			bool1 = false;
		}
		if (Button1.pressed && bool2) {
			Button3.click ();
			bool2 = false;
		}
		if (Button6.pressed && bool3) {
			Button4.click ();
			bool3 = false;
		}
		if (Button4.pressed && bool4) {
			Button6.click ();
			bool4 = false;
		}
		if ((Button8.pressed || Button9.pressed) && bool5) {
			Button7.click ();
			bool5 = false;
		}
		if ((Button7.pressed || Button9.pressed) && bool6) {
			Button8.click ();
			bool6 = false;
		}
		if ((Button8.pressed || Button7.pressed) && bool7) {
			Button9.click ();
			bool7 = false;
		}
		swarm.hBool1 = Button1.pressed;
		swarm.hBool2 = Button2.pressed;
		swarm.hBool3 = Button3.pressed;
		swarm.sBool1 = Button4.pressed;
		swarm.sBool2 = Button5.pressed;
		swarm.sBool3 = Button6.pressed;
		swarm.arrangement1 = Button7.pressed;
		swarm.arrangement2 = Button8.pressed;
		swarm.arrangement3 = Button9.pressed;
		roadLength.onEndEdit.AddListener(enter);
		roadWidth.onEndEdit.AddListener(enter2);
		minimumIlluminance.onEndEdit.AddListener(enter3);
		maximumAverageIlluminance.onEndEdit.AddListener(enter4);
		minimumAverageIlluminance.onEndEdit.AddListener(enter5);
		heightLimit.onEndEdit.AddListener(enter6);
		luminaireLimit.onEndEdit.AddListener(enter7);
		minSunlight.onEndEdit.AddListener (enter8);
		exit.onClick.AddListener (exitGame);
	}

	private void enter(string s) {
		float x;
		if (float.TryParse(s, out x))
			swarm.roadLength = x;
	}

	private void enter2(string s) {
		float x;
		if (float.TryParse(s, out x))
			swarm.roadWidth = x;
	}

	private void enter3(string s) {
		float x;
		if (float.TryParse(s, out x))
			swarm.minimumIlluminance = x;
	}

	private void enter4(string s) {
		float x;
		if (float.TryParse(s, out x))
			swarm.highestAverageilluminance = x;
	}

	private void enter5(string s) {
		float x;
		if (float.TryParse(s, out x))
			swarm.lowestAverageIlluminance = x;
	}

	private void enter6(string s) {
		float x;
		if (float.TryParse(s, out x))
			swarm.heightLimit = x;
	}

	private void enter7(string s) {
		float x;
		if (float.TryParse(s, out x))
			swarm.luminaireLimit = x;
	}

	private void enter8(string s) {
		float x;
		if (float.TryParse(s, out x))
			swarm.minScore = x;
	}

	private void exitGame() {
		Application.Quit ();
	}
}
