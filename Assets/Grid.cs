using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Grid : MonoBehaviour {
	public GameObject button, reset, scale;
	public GameObject panel, label;
	private SwarmOptimizer swarm;
	public Camera cam, cam2, cam3;
	[HideInInspector] public bool instantiated = false, highlighting = false;
	public int coordSet = 0;
	public int[] xEdge1, xEdge2, yEdge1, yEdge2, tempX1, tempX2, tempY1, tempY2;
	public GameObject[,] gridButtons;
	private RaycastHit hit;
	public LayerMask hitZone;
	public GlobalScaler scaler;
	public GameObject heightSetter, canvas;
	public float[] structureHeight = new float[0];
	public float prevAngle, dir;
	public GameObject[] buildings;
	public Image compass;
	public float z = 0;
	public Text text;

	void Start () {
		swarm = GameObject.Find ("Swarm").GetComponent<SwarmOptimizer> ();
		scaler = GameObject.Find ("GlobalScaler").GetComponent<GlobalScaler> ();
		cam2.enabled = false;
	}

	void FixedUpdate () {
		if (cam.enabled && Cursor.lockState == CursorLockMode.Locked) {
			z += Mathf.Clamp (Input.GetAxis ("Mouse ScrollWheel"), -1, 1) * 10f;
			z += Mathf.Clamp (Input.GetAxis ("Compass Stick"), -1, 1);
			z += Mathf.Clamp (Input.GetAxis ("Mouse X"), -1, 1) * 400f * Time.deltaTime;
			dir += Mathf.Clamp (Input.GetAxis ("Compass Stick"), -1, 1);
			dir += Mathf.Clamp (Input.GetAxis ("Mouse ScrollWheel"), -1, 1) * 10f;
		}
		if (cam3.enabled)
			z += Mathf.Clamp (Input.GetAxis ("Mouse X"), -1, 1) * 400f * Time.deltaTime;
		compass.rectTransform.localRotation = Quaternion.Euler (0, 0, z);
		compass.rectTransform.anchoredPosition = new Vector3 (406f - 10 * Mathf.Sin (Mathf.Deg2Rad * z), 150f + 10 * Mathf.Cos (Mathf.Deg2Rad * z), 0);
		if (z >= 360)
			z -= 360;
		if (z < 0)
			z += 360;
		text.text = Mathf.RoundToInt(z) + " degrees";
		if (highlighting) {
			highlight ();
		}
		cam.transform.position = new Vector3(Mathf.Clamp (cam.transform.position.x, -5f * scaler.GlobalScale, (swarm.roadLength + 5f) * scaler.GlobalScale), 
			Mathf.Clamp (cam.transform.position.y, scaler.GlobalScale, 30f * scaler.GlobalScale), Mathf.Clamp (cam.transform.position.z, -5f * scaler.GlobalScale, (swarm.roadWidth + 5f) * scaler.GlobalScale));
		if (cam.enabled && !instantiated) {
			GameObject resetButton = Instantiate (reset, new Vector3 (-scaler.GlobalScale / 2f, swarm.roadWidth / 2f * scaler.GlobalScale, 2f * scaler.GlobalScale), Quaternion.Euler(new Vector3(0, 0, 90)));
			resetButton.transform.localScale = new Vector3 (3f / 25f * scaler.GlobalScale, scaler.GlobalScale / 25f, scaler.GlobalScale / 25f);
			resetButton.transform.SetParent (this.transform, false);
			resetButton.GetComponent<Button> ().onClick.AddListener (Reset);
			GameObject reference = Instantiate (scale, new Vector3 (-scaler.GlobalScale * 2f, swarm.roadWidth / 2f * scaler.GlobalScale, 2f * scaler.GlobalScale), new Quaternion (0, 0, 0, 0));
			reference.transform.localScale = new Vector3 (scaler.GlobalScale / 25f, scaler.GlobalScale / 25f, 1);
			reference.transform.SetParent (this.transform, false);
			gridButtons = new GameObject[(int)swarm.roadLength + 10, 10];
			for (int x = 0; x < swarm.roadLength + 10; x++) {
				for (int y = 0; y < 10; y++) {
					GameObject gridSpace;
					if (y < 5)
						gridSpace = Instantiate (button, new Vector3 ((x - 4.5f) * scaler.GlobalScale, -(y + 0.5f) * scaler.GlobalScale, 2f * scaler.GlobalScale), new Quaternion (0, 0, 0, 0));
					else
						gridSpace = Instantiate (button, new Vector3 ((x - 4.5f) * scaler.GlobalScale, (y - 4.5f + swarm.roadWidth) * scaler.GlobalScale, 2f * scaler.GlobalScale), new Quaternion (0, 0, 0, 0));
					gridSpace.transform.localScale = new Vector3 (scaler.GlobalScale / 25f, scaler.GlobalScale / 25f, 1);
					gridSpace.transform.SetParent (this.transform, false);
					GridInteraction gridAct = gridSpace.GetComponentInChildren<GridInteraction> ();
					gridAct.xIndex = x;
					gridAct.yIndex = y;
					gridButtons [x, y] = gridSpace;
				}
				if (x % 10 == 5) {
					GameObject marker = Instantiate (label, new Vector3 ((x - 5f) * scaler.GlobalScale, swarm.roadWidth / 2f * scaler.GlobalScale, 2f * scaler.GlobalScale), new Quaternion (0, 0, 0, 0));
					marker.transform.localScale = new Vector3 (1, scaler.GlobalScale / 5f, 1);
					marker.transform.SetParent (this.transform, false);
					marker.GetComponentInChildren<Text>().text = (x - 5f) + "m";
				}
			}
			GameObject luminaireArea = Instantiate (panel, new Vector3 (swarm.roadLength / 2 * scaler.GlobalScale, -.6f * scaler.GlobalScale , 2f * scaler.GlobalScale), new Quaternion (0, 0, 0, 0));
			luminaireArea.transform.SetParent (this.transform, false);
			luminaireArea.transform.localScale = new Vector3 (swarm.roadLength / 125f * scaler.GlobalScale, scaler.GlobalScale / 12.5f, scaler.GlobalScale / 25f);
			if (swarm.arrangement1 || swarm.arrangement3) {
				GameObject luminaireArea2 = Instantiate (panel, new Vector3 (swarm.roadLength / 2 * scaler.GlobalScale, (0.6f + swarm.roadWidth) * scaler.GlobalScale, 2f * scaler.GlobalScale), new Quaternion (0, 0, 0, 0));
				luminaireArea2.transform.SetParent (this.transform, false);
				luminaireArea2.transform.localScale = new Vector3 (swarm.roadLength / 125f * scaler.GlobalScale, scaler.GlobalScale / 12.5f, scaler.GlobalScale / 25f);
			}
			instantiated = true;
		}
		if (cam.enabled) {
			for (int k = 0; k < coordSet; k++) {
				for (int i = 0; i < swarm.roadLength + 10; i++) {
					for (int j = 0; j < 10; j++) {
						if (((i <= xEdge2 [k] && i >= xEdge1 [k]) || (i <= xEdge1 [k] && i >= xEdge2 [k])) &&
						    ((j <= yEdge2 [k] && j >= yEdge1 [k]) || (j <= yEdge1 [k] && j >= yEdge2 [k]))) {
							gridButtons [i, j].GetComponent<GridInteraction> ().selected = true;
							gridButtons [i, j].GetComponent<GridInteraction> ().setNo = k;
						}
					}
				}
			}
		}
	}

	public void highlight() {
		Ray ray = cam.ScreenPointToRay (Input.mousePosition);
		if (Physics.Raycast (ray, out hit, 50f * scaler.GlobalScale, hitZone)) {
			GridInteraction gridAct = hit.collider.GetComponent<GridInteraction> ();
			if (gridAct != null) {
				Debug.Log ("1");
				gridAct.setOtherEdge ();
			}
		}
	}

	public void createNewSet() {
		if (coordSet > 0) {
			tempX1 = new int[coordSet];
			tempX2 = new int[coordSet];
			tempY1 = new int[coordSet];
			tempY2 = new int[coordSet];
			for (int i = 0; i < coordSet; i++) {
				tempX1 [i] = xEdge1 [i];
				tempX2 [i] = xEdge2 [i];
				tempY1 [i] = yEdge1 [i];
				tempY2 [i] = yEdge2 [i];
			}
		}
		coordSet++;
		xEdge1 = new int[coordSet];
		xEdge2 = new int[coordSet];
		yEdge1 = new int[coordSet];
		yEdge2 = new int[coordSet];
		if (coordSet > 1) {
			for (int i = 0; i < coordSet - 1; i++) {
				xEdge1 [i] = tempX1 [i];
				xEdge2 [i] = tempX2 [i];
				yEdge1 [i] = tempY1 [i];
				yEdge2 [i] = tempY2 [i];
			}
		}
	}

	public void Reset() {
		coordSet = 0;
		xEdge1 = new int[coordSet];
		xEdge2 = new int[coordSet];
		yEdge1 = new int[coordSet];
		yEdge2 = new int[coordSet];
		for (int i = 0; i < swarm.roadLength + 10; i++) {
			for (int j = 0; j < 10; j++) {
				gridButtons [i, j].GetComponent<GridInteraction> ().selected = false;
			}
		}
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = true;
	}

	public void SetHeight() {
		prevAngle = cam.GetComponent<Cam> ().Vangle;
		cam.GetComponent<Cam> ().Vangle = 45f;
		Cursor.lockState = CursorLockMode.Locked;
		cam.enabled = false;
		cam2.enabled = true;
	}
}
