using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProcessTracker : MonoBehaviour {
	public Button toSwarm;
	public Texture2D tex;
	private SwarmOptimizer swarm;
	public Grid grid;
	public GameObject structure;
	private bool buildingsAreInstantiated = false;
	[SerializeField] GameObject AreaLayoutUI, MainMenuUI, SwarmOptimizerUI;

	void Start () {
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		swarm = GameObject.Find("Swarm").GetComponent<SwarmOptimizer> ();
		Camera.main.transform.position = new Vector3 (swarm.roadLength / 2 * GlobalScaler.Instance().GetGlobalScale(),
			8 * GlobalScaler.Instance().GetGlobalScale(), -swarm.roadWidth * 2 * GlobalScaler.Instance().GetGlobalScale());
	}

	void Update () {
		Cursor.visible = true;
		if (Input.GetKeyDown (KeyCode.Escape) && MainMenuUI.activeSelf) {
			Cursor.lockState = CursorLockMode.None;
			Cursor.SetCursor (null, new Vector2 (0, 0), CursorMode.Auto);
		}
		if (AreaLayoutUI.activeSelf) {
			if (Input.GetMouseButton (1))//right click
				Cursor.lockState = CursorLockMode.None;
			
		}
	}

	public void ToGrid() {
		Cursor.lockState = CursorLockMode.Locked;
		swarm.ClearData ();
		GameObject[] road = GameObject.FindGameObjectsWithTag ("Finish");
		for (int i = 0; i < road.Length; i++)
			Destroy (road[i]);
		AreaLayoutUI.SetActive(true);
		MainMenuUI.SetActive(false);

		Camera.main.transform.position = new Vector3 (swarm.roadLength / 2 * GlobalScaler.Instance().GetGlobalScale(), 
			8 * GlobalScaler.Instance().GetGlobalScale(), swarm.roadWidth / 2 * GlobalScaler.Instance().GetGlobalScale());
		Camera.main.GetComponent<CameraMovement> ().Vangle = -90;
		Camera.main.GetComponent<CameraMovement> ().Hangle = 0;
		Cursor.SetCursor(tex, new Vector2(0, 0), CursorMode.Auto);
		Cursor.visible = true;

		buildingsAreInstantiated = false;
		for (int i = 0; i < grid.coordSet; i++)
			Destroy (grid.buildings [i]);
	}

	public void ToSwarm() {
		Camera.main.transform.position = new Vector3 (swarm.roadLength / 2 * GlobalScaler.Instance().GetGlobalScale(), 
			8 * GlobalScaler.Instance().GetGlobalScale(), swarm.roadWidth / 2 * GlobalScaler.Instance().GetGlobalScale());
		Camera.main.GetComponent<CameraMovement> ().Vangle = -90;
		Camera.main.GetComponent<CameraMovement> ().Hangle = 0;
		MainMenuUI.SetActive(false);
		SwarmOptimizerUI.SetActive(true);
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.SetCursor (tex, new Vector2 (0, 0), CursorMode.Auto);

		if (!buildingsAreInstantiated) {
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
		}
	}
}
