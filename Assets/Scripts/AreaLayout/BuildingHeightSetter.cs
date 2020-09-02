using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingHeightSetter : MonoBehaviour {
	public Text text;
	public InputField field;

	public void EnterHeight() {
        if (float.TryParse(text.text, out float x))
        {
            BuildingLocationsManager.Instance().RegisterNewBuildingHeight(x);
            Cursor.lockState = CursorLockMode.Locked;
            field.text = "\0";

            this.gameObject.SetActive(false);
        }
        else
            text.text = "Invalid";
    }
}
