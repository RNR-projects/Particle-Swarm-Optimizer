using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraManager : MonoBehaviour {
	public Camera cam1, cam2, cam3;
	public Button toGrid, toSwarm;
	public Texture2D tex;
	private SwarmOptimizer swarm;
	public GlobalScaler scaler;
	public Grid grid;
	public GameObject structure;
	private bool done = false;
	private float tempH1 = 0;

	void Start () {
		scaler = GameObject.Find ("GlobalScaler").GetComponent<GlobalScaler> ();
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		cam1.enabled = true;
		cam2.enabled = false;
		cam3.enabled = false;
		swarm = GameObject.Find("Swarm").GetComponent<SwarmOptimizer> ();
		cam1.transform.position = new Vector3 (swarm.roadLength / 2 * scaler.GlobalScale, 8 * scaler.GlobalScale, -swarm.roadWidth * 2 * scaler.GlobalScale);
	}

	void Update () {
		Cursor.visible = true;
		toGrid.onClick.AddListener (ToGrid);
		toSwarm.onClick.AddListener (ToSwarm);
		if (Input.GetKeyDown (KeyCode.Escape) && (!cam1.enabled || cam2.enabled || cam3.enabled)) {
			Cursor.lockState = CursorLockMode.None;
			if (cam2.enabled)
				tempH1 = cam2.GetComponent<Cam> ().Hangle;
			else if (cam3.enabled)
				tempH1 = cam3.GetComponent<Cam> ().Hangle;
			cam2.enabled = false;
			cam3.enabled = false;
			cam1.enabled = true;
			cam2.GetComponent<Cam> ().Vangle = 90;
			cam2.GetComponent<Cam> ().Hangle = 0;
			cam3.GetComponent<Cam> ().Vangle = 90;
			cam3.GetComponent<Cam> ().Hangle = 0;
			Cursor.SetCursor (null, new Vector2 (0, 0), CursorMode.Auto);
		}
		if (cam3.enabled) {
			if (Input.GetMouseButton (1))
				Cursor.lockState = CursorLockMode.None;
			Cursor.SetCursor (tex, new Vector2 (0, 0), CursorMode.Auto);
			Cursor.visible = true;
		}
	}

	private void ToGrid() {
		Cursor.lockState = CursorLockMode.Locked;
		swarm.ClearData ();
		GameObject[] road = GameObject.FindGameObjectsWithTag ("Finish");
		for (int i = 0; i < road.Length; i++)
			Destroy (road[i]);
		cam1.enabled = false;
		cam2.enabled = false;
		cam3.enabled = true;
		cam3.transform.position = new Vector3 (swarm.roadLength / 2 * scaler.GlobalScale, 8 * scaler.GlobalScale, swarm.roadWidth / 2 * scaler.GlobalScale);
		cam3.GetComponent<Cam> ().Vangle = -90;
		cam3.GetComponent<Cam> ().Hangle = tempH1;
		done = false;
		for (int i = 0; i < grid.coordSet; i++)
			Destroy (grid.buildings [i]);
	}

	private void ToSwarm() {
		cam2.transform.position = new Vector3 (swarm.roadLength / 2 * scaler.GlobalScale, 8 * scaler.GlobalScale, swarm.roadWidth / 2 * scaler.GlobalScale);
		cam2.GetComponent<Cam> ().Vangle = -90;
		cam2.GetComponent<Cam> ().Hangle = tempH1;
		cam1.enabled = false;
		cam3.enabled = false;
		cam2.enabled = true;
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.SetCursor (tex, new Vector2 (0, 0), CursorMode.Auto);
		if (!done) {
			grid.buildings = new GameObject[grid.coordSet];
			for (int i = 0; i < grid.coordSet; i++) {
				if (grid.yEdge1 [i] < 5 && grid.yEdge2 [i] < 5) {
					GameObject building = Instantiate (structure, new Vector3 ((grid.xEdge1 [i] - 4.5f + (grid.xEdge2 [i] - grid.xEdge1 [i]) / 2f) * scaler.GlobalScale, (grid.structureHeight [i] / 2f - 1) * scaler.GlobalScale, -(grid.yEdge1 [i] + .5f + (grid.yEdge2 [i] - grid.yEdge1 [i]) / 2f) * scaler.GlobalScale), Quaternion.Euler (0, 0, 0));				
					building.transform.localScale = new Vector3 ((Mathf.Abs (grid.xEdge1 [i] - grid.xEdge2 [i]) + 1) * scaler.GlobalScale, grid.structureHeight [i] * scaler.GlobalScale, (Mathf.Abs (grid.yEdge1 [i] - grid.yEdge2 [i]) + 1) * scaler.GlobalScale);
					grid.buildings [i] = building;
				} else if (grid.yEdge1 [i] >= 5 && grid.yEdge2 [i] >= 5) {
					GameObject building = Instantiate (structure, new Vector3 ((grid.xEdge1 [i] - 4.5f + (grid.xEdge2 [i] - grid.xEdge1 [i]) / 2f) * scaler.GlobalScale, (grid.structureHeight [i] / 2f - 1) * scaler.GlobalScale, (grid.yEdge1 [i] - 4.5f + swarm.roadWidth + (grid.yEdge2 [i] - grid.yEdge1 [i]) / 2f) * scaler.GlobalScale), Quaternion.Euler (0, 0, 0));
					building.transform.localScale = new Vector3 ((Mathf.Abs (grid.xEdge1 [i] - grid.xEdge2 [i]) + 1) * scaler.GlobalScale, grid.structureHeight [i] * scaler.GlobalScale, (Mathf.Abs (grid.yEdge1 [i] - grid.yEdge2 [i]) + 1) * scaler.GlobalScale);
					grid.buildings [i] = building;
				}
				done = true;
			}
			GameObject[] gridSpace = GameObject.FindGameObjectsWithTag ("Player");
			for (int i = 0; i < gridSpace.Length; i++)
				Destroy (gridSpace [i]);
			grid.instantiated = false;
		}
	}
}
