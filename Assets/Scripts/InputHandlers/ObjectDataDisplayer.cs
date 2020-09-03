using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectDataDisplayer : MonoBehaviour
{
	private RaycastHit hit;
	[SerializeField] private Text text;

	void Update()
	{
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		if (Physics.Raycast(ray, out hit, 50f * GlobalScaler.Instance().GetGlobalScale()))
		{
			LightLevel lightPoint = hit.collider.GetComponent<LightLevel>();
			AverageEnergyGeneration score = hit.collider.GetComponent<AverageEnergyGeneration>();
			if (lightPoint != null)
			{
				text.text = "Illuminance:\n" + lightPoint.illuminance + " lux";
			}
			else if (score != null)
			{
				text.text = "AnnualEnergyGenerated:\n" + score.averageSunHours + " kWh";
			}
		}
	}
}
