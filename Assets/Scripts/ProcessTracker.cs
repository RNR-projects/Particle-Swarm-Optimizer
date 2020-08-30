using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProcessTracker : MonoBehaviour {
	public Texture2D tex;
	private SwarmOptimizer swarm;
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
		//swarm = GameObject.Find("Swarm").GetComponent<SwarmOptimizer> ();
		//Camera.main.transform.position = new Vector3 (swarm.roadLength / 2 * GlobalScaler.Instance().GetGlobalScale(),
		//	8 * GlobalScaler.Instance().GetGlobalScale(), -swarm.roadWidth * 2 * GlobalScaler.Instance().GetGlobalScale());
	}

	void Update () {
		if (Input.GetKeyDown (KeyCode.Escape) && !MainMenuObjects.activeSelf) {
			this.ToMainMenu();
		}
	}

	public void ToGridCreator() {
		AreaLayoutObjects.SetActive(true);
		MainMenuObjects.SetActive(false);
		this.currentStep = OptimizationSteps.AreaLayout;

		Camera.main.transform.position = new Vector3 (0, 2 * GlobalScaler.Instance().GetGlobalScale(), 
						OptimizationParameterManager.Instance().GetRoadWidth() / 2 * GlobalScaler.Instance().GetGlobalScale());

		Camera.main.GetComponent<CameraMovement> ().SetVerticalAngle(0);
		Camera.main.GetComponent<CameraMovement> ().SetHorizontalAngle(0);
		Cursor.SetCursor(tex, new Vector2(0, 0), CursorMode.Auto);
		Cursor.lockState = CursorLockMode.Locked;

		if (!AreaLayoutObjects.GetComponentInChildren<GridCreator>().AreaLayoutInstantiated())
		{
			AreaLayoutObjects.GetComponentInChildren<GridCreator>().CreateNewGrid();
		}
	}

	public void ToSwarm() {
		Camera.main.transform.position = new Vector3 (swarm.roadLength / 2 * GlobalScaler.Instance().GetGlobalScale(), 
			8 * GlobalScaler.Instance().GetGlobalScale(), swarm.roadWidth / 2 * GlobalScaler.Instance().GetGlobalScale());
		Camera.main.GetComponent<CameraMovement> ().SetVerticalAngle(-90);
		Camera.main.GetComponent<CameraMovement> ().SetHorizontalAngle(0);
		MainMenuObjects.SetActive(false);
		SwarmOptimizerObjects.SetActive(true);
		this.currentStep = OptimizationSteps.SwarmOptimization;
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.SetCursor (tex, new Vector2 (0, 0), CursorMode.Auto);

		/*if (!buildingsAreInstantiated) {
			grid.buildings = new GameObject[grid.coordSet];
			for (int i = 0; i < grid.coordSet; i++) {
				if (grid.yEdge1 [i] < 5 && grid.yEdge2 [i] < 5) {
					GameObject building = Instantiate (structure, new Vector3 ((grid.xEdge1 [i] - 4.5f + 
						(grid.xEdge2 [i] - grid.xEdge1 [i]) / 2f) * GlobalScaler.Instance().GetGlobalScale(), 
						(grid.structureHeight [i] / 2f - 1) * GlobalScaler.Instance().GetGlobalScale(), 
						-(grid.yEdge1 [i] + .5f + (grid.yEdge2 [i] - grid.yEdge1 [i]) / 2f) * GlobalScaler.Instance().GetGlobalScale()), 
						Quaternion.Euler (0, 0, 0));				
					building.transform.localScale = new Vector3 ((Mathf.Abs (grid.xEdge1 [i] - grid.xEdge2 [i]) + 1) * 
						GlobalScaler.Instance().GetGlobalScale(), 
						grid.structureHeight [i] * GlobalScaler.Instance().GetGlobalScale(), 
						(Mathf.Abs (grid.yEdge1 [i] - grid.yEdge2 [i]) + 1) * GlobalScaler.Instance().GetGlobalScale());
					grid.buildings [i] = building;
				} else if (grid.yEdge1 [i] >= 5 && grid.yEdge2 [i] >= 5) {
					GameObject building = Instantiate (structure, new Vector3 ((grid.xEdge1 [i] - 4.5f + 
						(grid.xEdge2 [i] - grid.xEdge1 [i]) / 2f) * GlobalScaler.Instance().GetGlobalScale(), 
						(grid.structureHeight [i] / 2f - 1) * GlobalScaler.Instance().GetGlobalScale(), 
						(grid.yEdge1 [i] - 4.5f + swarm.roadWidth + (grid.yEdge2 [i] - grid.yEdge1 [i]) / 2f) * 
						GlobalScaler.Instance().GetGlobalScale()), Quaternion.Euler (0, 0, 0));
					building.transform.localScale = new Vector3 ((Mathf.Abs (grid.xEdge1 [i] - grid.xEdge2 [i]) + 1) * 
						GlobalScaler.Instance().GetGlobalScale(), grid.structureHeight [i] * GlobalScaler.Instance().GetGlobalScale(), 
						(Mathf.Abs (grid.yEdge1 [i] - grid.yEdge2 [i]) + 1) * GlobalScaler.Instance().GetGlobalScale());
					grid.buildings [i] = building;
				}
				buildingsAreInstantiated = true;
			}
			GameObject[] gridSpace = GameObject.FindGameObjectsWithTag ("Player");
			for (int i = 0; i < gridSpace.Length; i++)
				Destroy (gridSpace [i]);
			grid.instantiated = false;
		}*/
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

		MainMenuObjects.SetActive(true);
    }

	public OptimizationSteps GetCurrentStep()
    {
		return this.currentStep;
    }
}
