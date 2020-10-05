using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridInputHandler : MonoBehaviour
{
	private float compassDirection = 0;
	private float roadDirection = 0;
	[SerializeField] private Image compass;
	[SerializeField] private Text compassText;
	[SerializeField] private ProcessTracker tracker;
	[SerializeField] private GameObject builderUI;

    private void Awake()
    {
		BuildingLocationsManager.Instance().SetBuilderUI(this.builderUI);
    }

    private void OnEnable()
    {
		this.roadDirection = 0;
		this.compassDirection = 0;
    }
    // Update is called once per frame
    void Update()
	{
		if (Cursor.lockState == CursorLockMode.Locked)
		{
			this.compassDirection += Mathf.Clamp(Input.GetAxis("Mouse ScrollWheel"), -1, 1) * 10f;
			this.compassDirection += Mathf.Clamp(Input.GetAxis("Compass Stick"), -1, 1);
			this.compassDirection += Mathf.Clamp(Input.GetAxis("Mouse X"), -1, 1) * 400f * Time.deltaTime;
			this.roadDirection += Mathf.Clamp(Input.GetAxis("Compass Stick"), -1, 1);
			this.roadDirection += Mathf.Clamp(Input.GetAxis("Mouse ScrollWheel"), -1, 1) * 10f;
			OptimizationParameterManager.Instance().SetRoadDirection(this.roadDirection);
		}
		this.compass.rectTransform.localRotation = Quaternion.Euler(0, 0, this.compassDirection);
		this.compass.rectTransform.anchoredPosition = new Vector3(406f - 10 * Mathf.Sin(Mathf.Deg2Rad * this.compassDirection),
															150f + 10 * Mathf.Cos(Mathf.Deg2Rad * this.compassDirection), 0);
		if (this.compassDirection >= 360)
			this.compassDirection -= 360;
		if (this.compassDirection < 0)
			this.compassDirection += 360;
		compassText.text = Mathf.RoundToInt(this.compassDirection) + " degrees";

		Camera.main.transform.position = new Vector3(Mathf.Clamp(Camera.main.transform.position.x,
								-5f * GlobalScaler.Instance().GetGlobalScale(),
								(OptimizationParameterManager.Instance().GetRoadLength() + 5f) * GlobalScaler.Instance().GetGlobalScale()),
								Mathf.Clamp(Camera.main.transform.position.y, GlobalScaler.Instance().GetGlobalScale(),
								30f * GlobalScaler.Instance().GetGlobalScale()), Mathf.Clamp(Camera.main.transform.position.z,
								-5f * GlobalScaler.Instance().GetGlobalScale(),
								(OptimizationParameterManager.Instance().GetRoadWidth() + 5f) * GlobalScaler.Instance().GetGlobalScale()));

		if (Input.GetMouseButton(1))//right click
			Cursor.lockState = CursorLockMode.None;

		if (Cursor.lockState == CursorLockMode.None)
		{
			Cursor.visible = true;
			/*if (BuildingLocationsManager.Instance().GetIsCompletingNewSet())
			{
				BuildingLocationsManager.Instance().HighlightHoveredArea();
			}*/
		}
	}
}
