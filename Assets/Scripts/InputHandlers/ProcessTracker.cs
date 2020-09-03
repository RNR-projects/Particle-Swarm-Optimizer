using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProcessTracker : MonoBehaviour {
	public Texture2D tex;
	[SerializeField] private SwarmOptimizer swarm;
	public GameObject structure;
	[SerializeField] GameObject AreaLayoutObjects, MainMenuObjects, SwarmOptimizerObjects;
	private OptimizationSteps currentStep;
	public enum OptimizationSteps
    {
		MainMenu = 1,
		AreaLayout = 2,
		SwarmOptimization = 3
    }

	void Awake () {
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		this.currentStep = OptimizationSteps.MainMenu;
	}

	void Update () {
		if (Input.GetKeyDown (KeyCode.Escape) && !MainMenuObjects.activeSelf) {
			this.ToMainMenu();
		}
	}

	public void ToGridCreator()
	{
		AreaLayoutObjects.SetActive(true);
		MainMenuObjects.SetActive(false);
		this.currentStep = OptimizationSteps.AreaLayout;

		Camera.main.transform.position = new Vector3(0, 2 * GlobalScaler.Instance().GetGlobalScale(), 0);
		Camera.main.GetComponent<CameraMovement>().SetVerticalAngle(0);
		Camera.main.GetComponent<CameraMovement>().SetHorizontalAngle(0);

		Cursor.SetCursor(tex, new Vector2(0, 0), CursorMode.Auto);
		Cursor.lockState = CursorLockMode.Locked;

		AreaLayoutObjects.GetComponentInChildren<GridCreator>().CreateNewGrid();
	}

	public void ToSwarm() {
		Camera.main.transform.position = new Vector3(0, 2 * GlobalScaler.Instance().GetGlobalScale(), 0);
		Camera.main.GetComponent<CameraMovement> ().SetVerticalAngle(0);
		Camera.main.GetComponent<CameraMovement> ().SetHorizontalAngle(0);

		MainMenuObjects.SetActive(false);
		SwarmOptimizerObjects.SetActive(true);

		RoadAndLuminaireCreator.Instance().CreateRoad();
		IlluminationPointsCreator.Instance().InitializeIlluminationPoints();
		StructureBuilder.Instance().CreateStructures();

		swarm.InitializeOptimization();

		this.currentStep = OptimizationSteps.SwarmOptimization;
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.SetCursor (tex, new Vector2 (0, 0), CursorMode.Auto);

	}

	private void ToMainMenu()
    {
		switch(this.currentStep)
        {
			case OptimizationSteps.AreaLayout:
				AreaLayoutObjects.SetActive(false);
				break;
			case OptimizationSteps.SwarmOptimization:
				SwarmOptimizerObjects.SetActive(false);
				break;
        }
		Cursor.lockState = CursorLockMode.None;
		Cursor.SetCursor(null, new Vector2(0, 0), CursorMode.Auto);

		Camera.main.GetComponent<CameraMovement>().SetHorizontalAngle(0);
		Camera.main.GetComponent<CameraMovement>().SetVerticalAngle(0);

		MainMenuObjects.SetActive(true);
    }

	public OptimizationSteps GetCurrentStep()
    {
		return this.currentStep;
    }
}
