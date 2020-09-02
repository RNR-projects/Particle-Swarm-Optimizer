using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour {
    public Texture2D tex;
    private RaycastHit hit;
    [SerializeField] LayerMask hitZone;
    public Text text;
	public Camera cam;

    void Start () {
        text = GetComponent<Text>();
	}

	void Update () {
		if (cam.enabled) {
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			if (Physics.Raycast (ray, out hit, 50f * GlobalScaler.Instance().GetGlobalScale(), hitZone)) {
				LightLevel lightPoint = hit.collider.GetComponent<LightLevel> ();
				AverageScore score = hit.collider.GetComponent<AverageScore> ();
				if (lightPoint != null) {
					text.text = "Illuminance:           " + lightPoint.illuminance + " lux";
				} else if (score != null) {
					text.text = "AnnualEnergyGenerated: " + score.averageSunHours + " kWh";
				}
			}
		}
    }
}
