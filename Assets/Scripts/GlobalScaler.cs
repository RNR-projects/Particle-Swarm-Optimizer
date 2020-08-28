using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GlobalScaler {
	private float GlobalScale;

	private static GlobalScaler sharedInstance;

	public static GlobalScaler Instance()
    {
		if (sharedInstance == null)
			sharedInstance = new GlobalScaler();
		return sharedInstance;
    }

	public void SetGlobalScale(float value) {
		this.GlobalScale = value;
	}

	public float GetGlobalScale()
    {
		return this.GlobalScale;
    }
}
