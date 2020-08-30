using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LightCalculator : MonoBehaviour
{
    public Text text;
    private int longCalPoints, lumIndex, lumCount;
	[HideInInspector] public float pointDistanceX, pointDistanceY, pointEdgeX, pointEdgeY, illuminance, totalIlluminance, minIlluminance, averageIlluminance, illuminancePoint, score, C, Y;
    public float lampLumen, height, spacing, luminairePower, lightingEfficiency, roadWidth;
	private float innerdSpaceLim, xOffset = 0, initialOffset;
    private float[] lumPointsX, lumPointsY, lumX, lumY, Poffsets, Noffsets;
	private int[,] CList;
    public GameObject luminaire;
	public Camera mainCam;
	public SwarmOptimizer swarm;
	public GridCreator grid;
	public TextAsset textFile, solarGenerationFile, azimuthFile, horizonAngleFile;
	public LayerMask hitbox, structure;
	private RaycastHit hit;
	private RaycastHit[] lamps;
	public float[] scores;
	public bool buildable;
	private float [,] azimuth, horizonAngle, solarGeneration;
	private int[] monthlyHours = { 11, 11, 12, 13, 13, 13, 13, 13, 12, 12, 12, 11 };

    void Start()
    {
		grid = GameObject.Find ("GridCreator").GetComponent<GridCreator> ();
		swarm = this.transform.GetComponentInChildren<SwarmOptimizer>();
        text = GetComponent<Text>();
		string textContent = textFile.text;
		string[] tempList = textContent.Split (new[] {"\r\n", "\r", "\n"}, System.StringSplitOptions.RemoveEmptyEntries);
		string[] tempList3 = tempList [0].Split (new[] { ",", ";", " " }, System.StringSplitOptions.RemoveEmptyEntries);
		CList = new int[tempList.Length + 1, tempList3.Length + 1];
		for (int i = 0; i < tempList.Length + 1; i++) {
			if (i < tempList.Length)
				tempList3 = tempList [i].Split (new[] { ",", ";", " " }, System.StringSplitOptions.RemoveEmptyEntries);
			for (int j = 0; j < tempList3.Length + 1; j++) {
				int x;
				if (i == tempList.Length || j == tempList3.Length)
					CList [i, j] = 0;
				else if (int.TryParse (tempList3 [j], out x)) {
					CList [i, j] = x;
					}
				}
			}
		string textContent2 = azimuthFile.text;
		string[] tempList21 = textContent2.Split (new[] {"\r\n", "\r", "\n"}, System.StringSplitOptions.RemoveEmptyEntries);
		string[] tempList23 = tempList21 [0].Split (new[] { ",", ";", " " }, System.StringSplitOptions.RemoveEmptyEntries);
		azimuth = new float[12, 13];
		for (int i = 0; i < 12; i++) {
			for (int j = 0; j < 13; j++) {
				tempList23 = tempList21 [i].Split (new[] { ",", ";", " " }, System.StringSplitOptions.RemoveEmptyEntries);
				float x;
				if (float.TryParse (tempList23 [j], out x)) {
					azimuth [i, j] = x;
				}
			}
		}
		string textContent3 = horizonAngleFile.text;
		string[] tempList31 = textContent3.Split (new[] {"\r\n", "\r", "\n"}, System.StringSplitOptions.RemoveEmptyEntries);
		string[] tempList33 = tempList31 [0].Split (new[] { ",", ";", " " }, System.StringSplitOptions.RemoveEmptyEntries);
		horizonAngle = new float[12, 13];
		for (int i = 0; i < 12; i++) {
			for (int j = 0; j < 13; j++) {
				tempList33 = tempList31 [i].Split (new[] { ",", ";", " " }, System.StringSplitOptions.RemoveEmptyEntries);
				float x;
				if (float.TryParse (tempList33 [j], out x)) {
					horizonAngle [i, j] = x;
				}
			}
		}
		string textContent4 = solarGenerationFile.text;
		string[] tempList41 = textContent4.Split (new[] {"\r\n", "\r", "\n"}, System.StringSplitOptions.RemoveEmptyEntries);
		string[] tempList43 = tempList41 [0].Split (new[] { ",", ";", " " }, System.StringSplitOptions.RemoveEmptyEntries);
		solarGeneration = new float[12, 13];
		for (int i = 0; i < 12; i++) {
			for (int j = 0; j < 13; j++) {
				tempList43 = tempList41 [i].Split (new[] { ",", ";", " " }, System.StringSplitOptions.RemoveEmptyEntries);
				float x;
				if (float.TryParse (tempList43 [j], out x)) {
					solarGeneration [i, j] = x;
				}
			}
		}
		}

    void FixedUpdate()
    {
		text.text = "Average: " + averageIlluminance + "\nMin: " + minIlluminance + "\nEfficiency:" + lightingEfficiency + "\nOffset:" + (xOffset + swarm.roadLength % spacing);
		if (mainCam.enabled && !swarm.enabled) {
			swarm.enabled = true;
		} 
		else if (!mainCam.enabled && swarm.enabled) {
			swarm.enabled = false;
		}			
    }

    public void CalcIlluminance()
	{
		initialOffset = xOffset;
		buildable = true;
		illuminancePoint = 0;
		score = 0;
		longCalPoints = swarm.longCalPoints;
		pointDistanceX = spacing / longCalPoints;
		pointDistanceY = roadWidth / 3;
		pointEdgeX = pointDistanceX / 2;
		pointEdgeY = pointDistanceY / 2;
		lumPointsX = new float[longCalPoints * 3];
		lumPointsY = new float[longCalPoints * 3];
		if (spacing < 6f)
			spacing = 6f;
		lumCount = Mathf.FloorToInt(swarm.roadLength / spacing + 1);
		if (swarm.arrangement3) {
			Poffsets = new float[lumCount * 2];
			Noffsets = new float[lumCount * 2];
			innerdSpaceLim = spacing - swarm.roadLength / ((float)lumCount) + 0.5f;
			lumCount *= 2;
		}
		GameObject[] lampList = new GameObject[lumCount];
		bool reversed = false;
		scores = new float[lumCount];
		lumX = new float[lumCount];
		lumY = new float[lumCount];
		minIlluminance = 0;
		for (int i = 0; i < 3; i++) {
			for (int j = 0; j < longCalPoints; j++) {
				lumIndex = i * longCalPoints + j;
				lumPointsX [lumIndex] = pointEdgeX + j * pointDistanceX;
				lumPointsY [lumIndex] = pointEdgeY + i * pointDistanceY;
			}
		}
		float[] OPoffsets = new float[lumCount];
		float[] RPoffsets = new float[lumCount];
		float[] ONoffsets = new float[lumCount];
		float[] RNoffsets = new float[lumCount];
		for (int z = 0; z < 12; z++) {
			for (int sun = 0; sun < monthlyHours [z]; sun++) {
				float generation = solarGeneration [z, sun];
				if (z < 7) {
					if (z % 2 == 0)
						generation *= 31f;
					else if (z == 1)
						generation *= 28f;
					else
						generation *= 30f;
				} else {
					if (z % 2 == 1)
						generation *= 31f;
					else
						generation *= 30f;
				}
				for (int i = 0; i < lumCount; i++)
					scores [i] += generation;
			}
		}
		for (int x = 0; x < lumCount; x++) {
			if (swarm.arrangement1) {
				if (Physics.CheckBox (new Vector3 ((spacing * x + (swarm.roadLength % spacing) / 2f) * GlobalScaler.Instance().GetGlobalScale(), 0, roadWidth * Mathf.Abs (Mathf.Sin (Mathf.Deg2Rad * 90 * x)) * GlobalScaler.Instance().GetGlobalScale()), new Vector3 (0.5f * GlobalScaler.Instance().GetGlobalScale(), 2f * GlobalScaler.Instance().GetGlobalScale(), 0.5f * GlobalScaler.Instance().GetGlobalScale()), Quaternion.identity, structure)) {
					for (int j = 0; j < lumCount; j++) {
						if (Physics.CheckBox (new Vector3 ((spacing * j + (swarm.roadLength % spacing) / 2f) * GlobalScaler.Instance().GetGlobalScale(), 0, roadWidth * Mathf.Abs (Mathf.Cos (Mathf.Deg2Rad * 90 * j)) * GlobalScaler.Instance().GetGlobalScale()), new Vector3 (0.5f * GlobalScaler.Instance().GetGlobalScale(), 2f * GlobalScaler.Instance().GetGlobalScale(), 0.5f * GlobalScaler.Instance().GetGlobalScale()), Quaternion.identity, structure)) {
							buildable = false;
							Collider[] str = Physics.OverlapBox (new Vector3 ((spacing * j + (swarm.roadLength % spacing) / 2f) * GlobalScaler.Instance().GetGlobalScale(), 0, roadWidth * Mathf.Abs (Mathf.Cos (Mathf.Deg2Rad * 90 * j)) * GlobalScaler.Instance().GetGlobalScale()), new Vector3 (0.5f * GlobalScaler.Instance().GetGlobalScale(), 2f * GlobalScaler.Instance().GetGlobalScale(), 0.5f * GlobalScaler.Instance().GetGlobalScale()), Quaternion.identity, structure);
							RPoffsets [j] = (str [0].transform.position.x + str [0].transform.lossyScale.x / 2f) / GlobalScaler.Instance().GetGlobalScale() - (spacing * j + (swarm.roadLength % spacing) / 2f) + 0.5f;
							RNoffsets [j] = ((str [0].transform.position.x - str [0].transform.lossyScale.x / 2f) / GlobalScaler.Instance().GetGlobalScale() - (spacing * j + (swarm.roadLength % spacing) / 2f)) - 0.5f;
						} else {
							RPoffsets [j] = 0;
							RNoffsets [j] = 0;
						}
					}
					if (buildable) {
						reversed = true;
						break;
					}
					if (!buildable) {
						for (int y = 0; y < lumCount; y++) {
							if (Physics.CheckBox (new Vector3 ((spacing * y + (swarm.roadLength % spacing) / 2f) * GlobalScaler.Instance().GetGlobalScale(), 0, roadWidth * Mathf.Abs (Mathf.Sin (Mathf.Deg2Rad * 90 * y)) * GlobalScaler.Instance().GetGlobalScale()), new Vector3 (0.5f * GlobalScaler.Instance().GetGlobalScale(), 2f * GlobalScaler.Instance().GetGlobalScale(), 0.5f * GlobalScaler.Instance().GetGlobalScale()), Quaternion.identity, structure)) {
								Collider[] str = Physics.OverlapBox (new Vector3 ((spacing * y + (swarm.roadLength % spacing) / 2f) * GlobalScaler.Instance().GetGlobalScale(), 0, roadWidth * Mathf.Abs (Mathf.Sin (Mathf.Deg2Rad * 90 * y)) * GlobalScaler.Instance().GetGlobalScale()), new Vector3 (0.5f * GlobalScaler.Instance().GetGlobalScale(), 2f * GlobalScaler.Instance().GetGlobalScale(), 0.5f * GlobalScaler.Instance().GetGlobalScale()), Quaternion.identity, structure);
								OPoffsets [y] = (str [0].transform.position.x + str [0].transform.lossyScale.x / 2f) / GlobalScaler.Instance().GetGlobalScale() - (spacing * y + (swarm.roadLength % spacing) / 2f) + 0.5f;
								ONoffsets [y] = ((str [0].transform.position.x - str [0].transform.lossyScale.x / 2f) / GlobalScaler.Instance().GetGlobalScale() - (spacing * y + (swarm.roadLength % spacing) / 2f)) - 0.5f;
							} else {
								OPoffsets [y] = 0;
								ONoffsets [y] = 0;
							}
						}
						break;
					}
				}
			} else if (swarm.arrangement2) {
				if (Physics.CheckBox (new Vector3 ((spacing * x + (swarm.roadLength % spacing) / 2f) * GlobalScaler.Instance().GetGlobalScale(), 0, 0), new Vector3 (0.5f * GlobalScaler.Instance().GetGlobalScale(), 2f * GlobalScaler.Instance().GetGlobalScale(), 0.5f * GlobalScaler.Instance().GetGlobalScale()), Quaternion.identity, structure)) {
					for (int j = 0; j < lumCount; j++) {
						if (Physics.CheckBox (new Vector3 ((spacing * j + (swarm.roadLength % spacing) / 2f) * GlobalScaler.Instance().GetGlobalScale(), 0, roadWidth * GlobalScaler.Instance().GetGlobalScale()), new Vector3 (0.5f * GlobalScaler.Instance().GetGlobalScale(), 2f * GlobalScaler.Instance().GetGlobalScale(), 0.5f * GlobalScaler.Instance().GetGlobalScale()), Quaternion.identity, structure)) {
							buildable = false;
							Collider[] str = Physics.OverlapBox (new Vector3 ((spacing * j + (swarm.roadLength % spacing) / 2f) * GlobalScaler.Instance().GetGlobalScale(), 0, roadWidth * GlobalScaler.Instance().GetGlobalScale()), new Vector3 (0.5f * GlobalScaler.Instance().GetGlobalScale(), 2f * GlobalScaler.Instance().GetGlobalScale(), 0.5f * GlobalScaler.Instance().GetGlobalScale()), Quaternion.identity, structure);
							RPoffsets [j] = (str [0].transform.position.x + str [0].transform.lossyScale.x / 2f) / GlobalScaler.Instance().GetGlobalScale() - (spacing * j + (swarm.roadLength % spacing) / 2f) + 0.5f;
							RNoffsets [j] = ((str [0].transform.position.x - str [0].transform.lossyScale.x / 2f) / GlobalScaler.Instance().GetGlobalScale() - (spacing * j + (swarm.roadLength % spacing) / 2f)) - 0.5f;
						} else {
							RPoffsets [j] = 0;
							RNoffsets [j] = 0;
						}
					}
					if (buildable) {
						reversed = true;
						break;
					}
					if (!buildable) {
						for (int y = 0; y < lumCount; y++) {
							if (Physics.CheckBox (new Vector3 ((spacing * y + (swarm.roadLength % spacing) / 2f) * GlobalScaler.Instance().GetGlobalScale(), 0, 0), new Vector3 (0.5f * GlobalScaler.Instance().GetGlobalScale(), 2f * GlobalScaler.Instance().GetGlobalScale(), 0.5f * GlobalScaler.Instance().GetGlobalScale()), Quaternion.identity, structure)) {
								Collider[] str = Physics.OverlapBox (new Vector3 ((spacing * y + (swarm.roadLength % spacing) / 2f) * GlobalScaler.Instance().GetGlobalScale(), 0, 0), new Vector3 (0.5f * GlobalScaler.Instance().GetGlobalScale(), 2f * GlobalScaler.Instance().GetGlobalScale(), 0.5f * GlobalScaler.Instance().GetGlobalScale()), Quaternion.identity, structure);
								OPoffsets [y] = (str [0].transform.position.x + str [0].transform.lossyScale.x / 2f) / GlobalScaler.Instance().GetGlobalScale() - (spacing * y + (swarm.roadLength % spacing) / 2f) + 0.5f;
								ONoffsets [y] = ((str [0].transform.position.x - str [0].transform.lossyScale.x / 2f) / GlobalScaler.Instance().GetGlobalScale() - (spacing * y + (swarm.roadLength % spacing) / 2f)) - 0.5f;
							} else {
								OPoffsets [y] = 0;
								ONoffsets [y] = 0;
							}
						}
						break;
					}
				}
			} else if (swarm.arrangement3) {
				if (Physics.CheckBox (new Vector3 ((spacing * Mathf.Floor (x / 2f) + (swarm.roadLength % spacing) / 2f) * GlobalScaler.Instance().GetGlobalScale(), 0, roadWidth * Mathf.Abs (Mathf.Sin (Mathf.Deg2Rad * 90 * x)) * GlobalScaler.Instance().GetGlobalScale()), new Vector3 (0.25f * GlobalScaler.Instance().GetGlobalScale(), 2f * GlobalScaler.Instance().GetGlobalScale(), 0.25f * GlobalScaler.Instance().GetGlobalScale()), Quaternion.identity, structure)) {
					for (int j = 0; j < lumCount; j++) {
						if (Physics.CheckBox (new Vector3 ((spacing * Mathf.Floor (j / 2f) + (swarm.roadLength % spacing) / 2f) * GlobalScaler.Instance().GetGlobalScale(), 0, roadWidth * Mathf.Abs (Mathf.Sin (Mathf.Deg2Rad * 90 * j)) * GlobalScaler.Instance().GetGlobalScale()), new Vector3 (0.25f * GlobalScaler.Instance().GetGlobalScale(), 2f * GlobalScaler.Instance().GetGlobalScale(), 0.25f * GlobalScaler.Instance().GetGlobalScale()), Quaternion.identity, structure)) {
							Collider[] str = Physics.OverlapBox (new Vector3 ((spacing * Mathf.Floor (j / 2f) + (swarm.roadLength % spacing) / 2f) * GlobalScaler.Instance().GetGlobalScale(), 0, roadWidth * Mathf.Abs (Mathf.Sin (Mathf.Deg2Rad * 90 * j)) * GlobalScaler.Instance().GetGlobalScale()), new Vector3 (0.25f * GlobalScaler.Instance().GetGlobalScale(), 2f * GlobalScaler.Instance().GetGlobalScale(), 0.25f * GlobalScaler.Instance().GetGlobalScale()), Quaternion.identity, structure);
							Poffsets [j] = (str [0].transform.position.x + str [0].transform.lossyScale.x / 2f) / GlobalScaler.Instance().GetGlobalScale() - (spacing * Mathf.Floor (j / 2f) + (swarm.roadLength % spacing) / 2f) + 0.5f;
							Noffsets [j] = ((str [0].transform.position.x - str [0].transform.lossyScale.x / 2f) / GlobalScaler.Instance().GetGlobalScale() - (spacing * Mathf.Floor (j / 2f) + (swarm.roadLength % spacing) / 2f)) - 0.5f;
							buildable = false;
						} else {
							Poffsets [j] = 0;
							Noffsets [j] = 0;
						}
					}
					break;
				}
			}
		}
		if (swarm.arrangement1 && !buildable) {
			buildable = true;
			if ((spacing * (lumCount - 1f) + (swarm.roadLength % spacing) / 2f + Mathf.Max (OPoffsets)) > swarm.roadLength) {
				float tempSpacing = (spacing * (lumCount - 1f) + (swarm.roadLength % spacing) / 2f + Mathf.Max (OPoffsets) - swarm.roadLength - 0.5f) / (lumCount - 1f);
				if (tempSpacing <= innerdSpaceLim)
				for (int x = 0; x < lumCount; x++) {
					if (Physics.CheckBox (new Vector3 ((spacing + tempSpacing) * x + (swarm.roadLength % (spacing + tempSpacing)) / 2f + Mathf.Max (OPoffsets) - tempSpacing * System.Array.IndexOf (OPoffsets, Mathf.Max (OPoffsets)) + tempSpacing / 2f * (lumCount - 1f), 0, roadWidth * Mathf.Abs (Mathf.Sin (Mathf.Deg2Rad * 90 * x)) * GlobalScaler.Instance().GetGlobalScale()), new Vector3 (0.25f * GlobalScaler.Instance().GetGlobalScale(), 2f * GlobalScaler.Instance().GetGlobalScale(), 0.25f * GlobalScaler.Instance().GetGlobalScale()), Quaternion.identity, structure)) {
						buildable = false;
						break;
					}
						}
				if (buildable) {
					spacing -= tempSpacing;
					xOffset += Mathf.Max (OPoffsets) - tempSpacing * System.Array.IndexOf (OPoffsets, Mathf.Max (OPoffsets)) + tempSpacing / 2f * (lumCount - 1f);
				} else
					buildable = true;
					}
			else {
				for (int x = 0; x < lumCount; x++) {
					if (Physics.CheckBox (new Vector3 ((spacing * x + (swarm.roadLength % spacing) / 2f + Mathf.Max (OPoffsets)) * GlobalScaler.Instance().GetGlobalScale(), 0, roadWidth * Mathf.Abs (Mathf.Sin (Mathf.Deg2Rad * 90 * x)) * GlobalScaler.Instance().GetGlobalScale()), new Vector3 (0.25f * GlobalScaler.Instance().GetGlobalScale(), 2f * GlobalScaler.Instance().GetGlobalScale(), 0.25f * GlobalScaler.Instance().GetGlobalScale()), Quaternion.identity, structure))
						buildable = false;
				}			
				if (buildable) {
					xOffset += Mathf.Max (OPoffsets);
				} else
					buildable = true;
			}
			if (xOffset == initialOffset) {
				if ((swarm.roadLength % spacing) / 2f + Mathf.Min (ONoffsets) < 0) {
					float tempSpacing = ((swarm.roadLength % spacing) / 2f + Mathf.Min (ONoffsets) + 0.5f) / (lumCount - 1f);
					if (tempSpacing <= innerdSpaceLim)
					for (int x = 0; x < lumCount; x++) {
						if (Physics.CheckBox (new Vector3 ((spacing + tempSpacing) * x + (swarm.roadLength % (spacing + tempSpacing)) / 2f + Mathf.Min (ONoffsets) - tempSpacing * System.Array.IndexOf (ONoffsets, Mathf.Min (ONoffsets)) + tempSpacing / 2f * (lumCount - 1f), 0, roadWidth * Mathf.Abs (Mathf.Sin (Mathf.Deg2Rad * 90 * x)) * GlobalScaler.Instance().GetGlobalScale()), new Vector3 (0.25f * GlobalScaler.Instance().GetGlobalScale(), 2f * GlobalScaler.Instance().GetGlobalScale(), 0.25f * GlobalScaler.Instance().GetGlobalScale()), Quaternion.identity, structure)) {
							buildable = false;
							break;
						}
					}
					if (buildable) {
						spacing -= tempSpacing;
						xOffset += Mathf.Min (ONoffsets) - tempSpacing * System.Array.IndexOf (ONoffsets, Mathf.Min (ONoffsets)) + tempSpacing / 2f * (lumCount - 1f);
					} else
						buildable = true;
				}
				else {
					for (int x = 0; x < lumCount; x++) {
						if (Physics.CheckBox (new Vector3 ((spacing * x + (swarm.roadLength % spacing) / 2f + Mathf.Min (ONoffsets)) * GlobalScaler.Instance().GetGlobalScale(), 0, roadWidth * Mathf.Abs (Mathf.Sin (Mathf.Deg2Rad * 90 * x)) * GlobalScaler.Instance().GetGlobalScale()), new Vector3 (0.25f * GlobalScaler.Instance().GetGlobalScale(), 2f * GlobalScaler.Instance().GetGlobalScale(), 0.25f * GlobalScaler.Instance().GetGlobalScale()), Quaternion.identity, structure))
							buildable = false;
					}			
					if (buildable) {
						xOffset += Mathf.Min (ONoffsets);
					} else
						buildable = true;
				}
				if (xOffset == initialOffset) {
					if ((spacing * (lumCount - 1f) + (swarm.roadLength % spacing) / 2f + Mathf.Max (RPoffsets)) > swarm.roadLength) {
						float tempSpacing = (spacing * (lumCount - 1f) + (swarm.roadLength % spacing) / 2f + Mathf.Max (RPoffsets) - swarm.roadLength - 0.5f) / (lumCount - 1f);
						if (tempSpacing <= innerdSpaceLim)
							for (int x = 0; x < lumCount; x++) {
								if (Physics.CheckBox (new Vector3 ((spacing + tempSpacing) * x + (swarm.roadLength % (spacing + tempSpacing)) / 2f + Mathf.Max (RPoffsets) - tempSpacing * System.Array.IndexOf (RPoffsets, Mathf.Max (RPoffsets)) + tempSpacing / 2f * (lumCount - 1f), 0, roadWidth * Mathf.Abs (Mathf.Cos (Mathf.Deg2Rad * 90 * x)) * GlobalScaler.Instance().GetGlobalScale()), new Vector3 (0.25f * GlobalScaler.Instance().GetGlobalScale(), 2f * GlobalScaler.Instance().GetGlobalScale(), 0.25f * GlobalScaler.Instance().GetGlobalScale()), Quaternion.identity, structure)) {
									buildable = false;
									break;
								}
							}
						if (buildable) {
							spacing -= tempSpacing;
							xOffset += Mathf.Max (RPoffsets) - tempSpacing * System.Array.IndexOf (RPoffsets, Mathf.Max (RPoffsets)) + tempSpacing / 2f * (lumCount - 1f);
						} else
							buildable = true;
					}
					else {
						for (int x = 0; x < lumCount; x++) {
							if (Physics.CheckBox (new Vector3 ((spacing * x + (swarm.roadLength % spacing) / 2f + Mathf.Max (RPoffsets)) * GlobalScaler.Instance().GetGlobalScale(), 0, roadWidth * Mathf.Abs (Mathf.Cos (Mathf.Deg2Rad * 90 * x)) * GlobalScaler.Instance().GetGlobalScale()), new Vector3 (0.25f * GlobalScaler.Instance().GetGlobalScale(), 2f * GlobalScaler.Instance().GetGlobalScale(), 0.25f * GlobalScaler.Instance().GetGlobalScale()), Quaternion.identity, structure))
								buildable = false;
						}			
						if (buildable) {
							xOffset += Mathf.Max (RPoffsets);
							reversed = true;
						} else
							buildable = true;
					}
					if (xOffset == initialOffset) {
						if ((swarm.roadLength % spacing) / 2f + Mathf.Min (RNoffsets) < 0) {
							float tempSpacing = ((swarm.roadLength % spacing) / 2f + Mathf.Min (RNoffsets) + 0.5f) / (lumCount - 1f);
							if (tempSpacing <= innerdSpaceLim)
								for (int x = 0; x < lumCount; x++) {
									if (Physics.CheckBox (new Vector3 ((spacing + tempSpacing) * x + (swarm.roadLength % (spacing + tempSpacing)) / 2f + Mathf.Min (RNoffsets) - tempSpacing * System.Array.IndexOf (RNoffsets, Mathf.Min (RNoffsets)) + tempSpacing / 2f * (lumCount - 1f), 0, roadWidth * Mathf.Abs (Mathf.Cos (Mathf.Deg2Rad * 90 * x)) * GlobalScaler.Instance().GetGlobalScale()), new Vector3 (0.25f * GlobalScaler.Instance().GetGlobalScale(), 2f * GlobalScaler.Instance().GetGlobalScale(), 0.25f * GlobalScaler.Instance().GetGlobalScale()), Quaternion.identity, structure)) {
										buildable = false;
										break;
									}
								}
							if (buildable) {
								spacing -= tempSpacing;
								xOffset += Mathf.Min (RNoffsets) - tempSpacing * System.Array.IndexOf (RNoffsets, Mathf.Min (RNoffsets)) + tempSpacing / 2f * (lumCount - 1f);
							} else
								buildable = true;
						}
						else {
							for (int x = 0; x < lumCount; x++) {
								if (Physics.CheckBox (new Vector3 ((spacing * x + (swarm.roadLength % spacing) / 2f + Mathf.Min (RNoffsets)) * GlobalScaler.Instance().GetGlobalScale(), 0, roadWidth * Mathf.Abs (Mathf.Cos (Mathf.Deg2Rad * 90 * x)) * GlobalScaler.Instance().GetGlobalScale()), new Vector3 (0.25f * GlobalScaler.Instance().GetGlobalScale(), 2f * GlobalScaler.Instance().GetGlobalScale(), 0.25f * GlobalScaler.Instance().GetGlobalScale()), Quaternion.identity, structure))
									buildable = false;
							}			
							if (buildable) {
								xOffset += Mathf.Min (RNoffsets);
								reversed = true;
							} else
								buildable = true;
						}
						if (xOffset == initialOffset)
							buildable = false;
					}
				}
			}				
		}
		if (swarm.arrangement2 && !buildable) {
			buildable = true;
			if ((spacing * (lumCount - 1f) + (swarm.roadLength % spacing) / 2f + Mathf.Max (OPoffsets)) > swarm.roadLength) {
				float tempSpacing = (spacing * (lumCount - 1f) + (swarm.roadLength % spacing) / 2f + Mathf.Max (OPoffsets) - swarm.roadLength - 0.5f) / (lumCount - 1f);
				if (tempSpacing <= innerdSpaceLim)
					for (int x = 0; x < lumCount; x++) {
						if (Physics.CheckBox (new Vector3 ((spacing + tempSpacing) * x + (swarm.roadLength % (spacing + tempSpacing)) / 2f + Mathf.Max (OPoffsets) - tempSpacing * System.Array.IndexOf (OPoffsets, Mathf.Max (OPoffsets)) + tempSpacing / 2f * (lumCount - 1f), 0, roadWidth * Mathf.Abs (Mathf.Sin (Mathf.Deg2Rad * 90 * x)) * GlobalScaler.Instance().GetGlobalScale()), new Vector3 (0.25f * GlobalScaler.Instance().GetGlobalScale(), 2f * GlobalScaler.Instance().GetGlobalScale(), 0.25f * GlobalScaler.Instance().GetGlobalScale()), Quaternion.identity, structure)) {
							buildable = false;
							break;
						}
					}
				if (buildable) {
					spacing -= tempSpacing;
					xOffset += Mathf.Max (OPoffsets) - tempSpacing * System.Array.IndexOf (OPoffsets, Mathf.Max (OPoffsets)) + tempSpacing / 2f * (lumCount - 1f);
				} else
					buildable = true;
			}
			else {
				for (int x = 0; x < lumCount; x++) {
					if (Physics.CheckBox (new Vector3 ((spacing * x + (swarm.roadLength % spacing) / 2f + Mathf.Max (OPoffsets)) * GlobalScaler.Instance().GetGlobalScale(), 0, 0), new Vector3 (0.25f * GlobalScaler.Instance().GetGlobalScale(), 2f * GlobalScaler.Instance().GetGlobalScale(), 0.25f * GlobalScaler.Instance().GetGlobalScale()), Quaternion.identity, structure))
						buildable = false;
				}			
				if (buildable) {
					xOffset += Mathf.Max (OPoffsets);
				} else
					buildable = true;
			}
			if (xOffset == initialOffset) {
				if ((swarm.roadLength % spacing) / 2f + Mathf.Min (ONoffsets) < 0) {
					float tempSpacing = ((swarm.roadLength % spacing) / 2f + Mathf.Min (ONoffsets) + 0.5f) / (lumCount - 1f);
					if (tempSpacing <= innerdSpaceLim)
						for (int x = 0; x < lumCount; x++) {
							if (Physics.CheckBox (new Vector3 ((spacing + tempSpacing) * x + (swarm.roadLength % (spacing + tempSpacing)) / 2f + Mathf.Min (ONoffsets) - tempSpacing * System.Array.IndexOf (ONoffsets, Mathf.Min (ONoffsets)) + tempSpacing / 2f * (lumCount - 1f), 0, roadWidth * Mathf.Abs (Mathf.Sin (Mathf.Deg2Rad * 90 * x)) * GlobalScaler.Instance().GetGlobalScale()), new Vector3 (0.25f * GlobalScaler.Instance().GetGlobalScale(), 2f * GlobalScaler.Instance().GetGlobalScale(), 0.25f * GlobalScaler.Instance().GetGlobalScale()), Quaternion.identity, structure)) {
								buildable = false;
								break;
							}
						}
					if (buildable) {
						spacing -= tempSpacing;
						xOffset += Mathf.Min (ONoffsets) - tempSpacing * System.Array.IndexOf (ONoffsets, Mathf.Min (ONoffsets)) + tempSpacing / 2f * (lumCount - 1f);
					} else
						buildable = true;
				}
				else {
					for (int x = 0; x < lumCount; x++) {
						if (Physics.CheckBox (new Vector3 ((spacing * x + (swarm.roadLength % spacing) / 2f + Mathf.Min (ONoffsets)) * GlobalScaler.Instance().GetGlobalScale(), 0, 0), new Vector3 (0.25f * GlobalScaler.Instance().GetGlobalScale(), 2f * GlobalScaler.Instance().GetGlobalScale(), 0.25f * GlobalScaler.Instance().GetGlobalScale()), Quaternion.identity, structure))
							buildable = false;
					}			
					if (buildable) {
						xOffset += Mathf.Min (ONoffsets);
					} else
						buildable = true;
				}
				if (xOffset == initialOffset) {
					if ((spacing * (lumCount - 1f) + (swarm.roadLength % spacing) / 2f + Mathf.Max (RPoffsets)) > swarm.roadLength) {
						float tempSpacing = (spacing * (lumCount - 1f) + (swarm.roadLength % spacing) / 2f + Mathf.Max (RPoffsets) - swarm.roadLength - 0.5f) / (lumCount - 1f);
						if (tempSpacing <= innerdSpaceLim)
							for (int x = 0; x < lumCount; x++) {
								if (Physics.CheckBox (new Vector3 ((spacing + tempSpacing) * x + (swarm.roadLength % (spacing + tempSpacing)) / 2f + Mathf.Max (RPoffsets) - tempSpacing * System.Array.IndexOf (RPoffsets, Mathf.Max (RPoffsets)) + tempSpacing / 2f * (lumCount - 1f), 0, roadWidth * Mathf.Abs (Mathf.Cos (Mathf.Deg2Rad * 90 * x)) * GlobalScaler.Instance().GetGlobalScale()), new Vector3 (0.25f * GlobalScaler.Instance().GetGlobalScale(), 2f * GlobalScaler.Instance().GetGlobalScale(), 0.25f * GlobalScaler.Instance().GetGlobalScale()), Quaternion.identity, structure)) {
									buildable = false;
									break;
								}
							}
						if (buildable) {
							spacing -= tempSpacing;
							xOffset += Mathf.Max (RPoffsets) - tempSpacing * System.Array.IndexOf (RPoffsets, Mathf.Max (RPoffsets)) + tempSpacing / 2f * (lumCount - 1f);
						} else
							buildable = true;
					}
					else {
						for (int x = 0; x < lumCount; x++) {
							if (Physics.CheckBox (new Vector3 ((spacing * x + (swarm.roadLength % spacing) / 2f + Mathf.Max (RPoffsets)) * GlobalScaler.Instance().GetGlobalScale(), 0, roadWidth * GlobalScaler.Instance().GetGlobalScale()), new Vector3 (0.25f * GlobalScaler.Instance().GetGlobalScale(), 2f * GlobalScaler.Instance().GetGlobalScale(), 0.25f * GlobalScaler.Instance().GetGlobalScale()), Quaternion.identity, structure))
								buildable = false;
						}			
						if (buildable) {
							xOffset += Mathf.Max (RPoffsets);
							reversed = true;
						} else
							buildable = true;
					}
					if (xOffset == initialOffset) {
						if ((swarm.roadLength % spacing) / 2f + Mathf.Min (RNoffsets) < 0) {
							float tempSpacing = ((swarm.roadLength % spacing) / 2f + Mathf.Min (RNoffsets) + 0.5f) / (lumCount - 1f);
							if (tempSpacing <= innerdSpaceLim)
								for (int x = 0; x < lumCount; x++) {
									if (Physics.CheckBox (new Vector3 ((spacing + tempSpacing) * x + (swarm.roadLength % (spacing + tempSpacing)) / 2f + Mathf.Min (RNoffsets) - tempSpacing * System.Array.IndexOf (RNoffsets, Mathf.Min (RNoffsets)) + tempSpacing / 2f * (lumCount - 1f), 0, roadWidth * Mathf.Abs (Mathf.Cos (Mathf.Deg2Rad * 90 * x)) * GlobalScaler.Instance().GetGlobalScale()), new Vector3 (0.25f * GlobalScaler.Instance().GetGlobalScale(), 2f * GlobalScaler.Instance().GetGlobalScale(), 0.25f * GlobalScaler.Instance().GetGlobalScale()), Quaternion.identity, structure)) {
										buildable = false;
										break;
									}
								}
							if (buildable) {
								spacing -= tempSpacing;
								xOffset += Mathf.Min (RNoffsets) - tempSpacing * System.Array.IndexOf (RNoffsets, Mathf.Min (RNoffsets)) + tempSpacing / 2f * (lumCount - 1f);
							} else
								buildable = true;
						}
						else {
							for (int x = 0; x < lumCount; x++) {
								if (Physics.CheckBox (new Vector3 ((spacing * x + (swarm.roadLength % spacing) / 2f + Mathf.Min (RNoffsets)) * GlobalScaler.Instance().GetGlobalScale(), 0, roadWidth * GlobalScaler.Instance().GetGlobalScale()), new Vector3 (0.25f * GlobalScaler.Instance().GetGlobalScale(), 2f * GlobalScaler.Instance().GetGlobalScale(), 0.25f * GlobalScaler.Instance().GetGlobalScale()), Quaternion.identity, structure))
									buildable = false;
							}			
							if (buildable) {
								xOffset += Mathf.Min (RNoffsets);
								reversed = true;
							} else
								buildable = true;
						}
						if (xOffset == initialOffset)
							buildable = false;
					}
				}
			}				
		}
		if (swarm.arrangement3 && !buildable) {
			buildable = true;
			if (Mathf.Max (Poffsets) >= -Mathf.Min (Noffsets)) {
				if ((spacing * Mathf.Floor ((lumCount - 1f) / 2f) + (swarm.roadLength % spacing) / 2f + Mathf.Max (Poffsets)) > swarm.roadLength) {
					float tempSpacing = (spacing * (Mathf.Floor(lumCount / 2f) - 1f) + (swarm.roadLength % spacing) / 2f + Mathf.Max (Poffsets) - swarm.roadLength - 0.5f) / (Mathf.Floor(lumCount / 2f) - 1f);
					if (tempSpacing <= innerdSpaceLim)
						for (int x = 0; x < lumCount; x++) {
							if (Physics.CheckBox (new Vector3 ((spacing + tempSpacing) * Mathf.Floor (x / 2f) + (swarm.roadLength % (spacing + tempSpacing)) / 2f + Mathf.Max (Poffsets) - tempSpacing * Mathf.Floor(System.Array.IndexOf (Poffsets, Mathf.Max (Poffsets)) / 2f) + tempSpacing / 2f * ((float)Mathf.Floor(lumCount / 2f) - 1f), 0, roadWidth * Mathf.Abs (Mathf.Sin (Mathf.Deg2Rad * 90 * x)) * GlobalScaler.Instance().GetGlobalScale()), new Vector3 (0.25f * GlobalScaler.Instance().GetGlobalScale(), 2f * GlobalScaler.Instance().GetGlobalScale(), 0.25f * GlobalScaler.Instance().GetGlobalScale()), Quaternion.identity, structure)) {
								buildable = false;
								break;
							}
						}
					if (buildable) {
						spacing -= tempSpacing;
						xOffset += Mathf.Max (Poffsets) - tempSpacing * Mathf.Floor((System.Array.IndexOf (Poffsets, Mathf.Max (Poffsets)) / 2f)) + tempSpacing / 2f * (lumCount / 2 - 1f);
					} else
						buildable = true;
				}
				else {
						for (int x = 0; x < lumCount; x++) {
							if (Physics.CheckBox (new Vector3 ((spacing * Mathf.Floor (x / 2f) + (swarm.roadLength % spacing) / 2f + Mathf.Max (Poffsets)) * GlobalScaler.Instance().GetGlobalScale(), 0, roadWidth * Mathf.Abs (Mathf.Sin (Mathf.Deg2Rad * 90 * x)) * GlobalScaler.Instance().GetGlobalScale()), new Vector3 (0.25f * GlobalScaler.Instance().GetGlobalScale(), 2f * GlobalScaler.Instance().GetGlobalScale(), 0.25f * GlobalScaler.Instance().GetGlobalScale()), Quaternion.identity, structure))
								buildable = false;
						}			
						if (buildable) {
							xOffset += Mathf.Max (Poffsets);
						} else
							buildable = true;
					}
				if (xOffset == initialOffset) {
					if (((swarm.roadLength % spacing) / 2f + Mathf.Min (Noffsets)) < 0) {
						float tempSpacing = ((swarm.roadLength % spacing) / 2f + Mathf.Min (Noffsets) + 0.5f) / (Mathf.Floor(lumCount / 2f) - 1f);
						if (tempSpacing <= innerdSpaceLim)
							for (int x = 0; x < lumCount; x++) {
								if (Physics.CheckBox (new Vector3 ((spacing + tempSpacing) * Mathf.Floor (x / 2f) + (swarm.roadLength % (spacing + tempSpacing)) / 2f + Mathf.Min (Noffsets) - tempSpacing * Mathf.Floor(System.Array.IndexOf (Noffsets, Mathf.Min (Noffsets)) / 2f) + tempSpacing / 2f * ((float)Mathf.Floor(lumCount / 2f) - 1f), 0, roadWidth * Mathf.Abs (Mathf.Sin (Mathf.Deg2Rad * 90 * x)) * GlobalScaler.Instance().GetGlobalScale()), new Vector3 (0.25f * GlobalScaler.Instance().GetGlobalScale(), 2f * GlobalScaler.Instance().GetGlobalScale(), 0.25f * GlobalScaler.Instance().GetGlobalScale()), Quaternion.identity, structure)) {
									buildable = false;
									break;
								}
							}
						if (buildable) {
							spacing -= tempSpacing;
							xOffset += Mathf.Min (Noffsets) - tempSpacing * Mathf.Floor((System.Array.IndexOf (Noffsets, Mathf.Min (Noffsets)) / 2f)) + tempSpacing / 2f * (lumCount / 2 - 1f);
						} else
							buildable = true;
					}
					else {
							for (int x = 0; x < lumCount; x++) {
								if (Physics.CheckBox (new Vector3 ((spacing * Mathf.Floor (x / 2f) + (swarm.roadLength % spacing) / 2f + Mathf.Min (Noffsets)) * GlobalScaler.Instance().GetGlobalScale(), 0, roadWidth * Mathf.Abs (Mathf.Sin (Mathf.Deg2Rad * 90 * x)) * GlobalScaler.Instance().GetGlobalScale()), new Vector3 (0.25f * GlobalScaler.Instance().GetGlobalScale(), 2f * GlobalScaler.Instance().GetGlobalScale(), 0.25f * GlobalScaler.Instance().GetGlobalScale()), Quaternion.identity, structure))
									buildable = false;
							}
							if (buildable) {
								xOffset += Mathf.Min (Noffsets);
							}
						}
					if (xOffset == initialOffset)
						buildable = false;
				}
			} else {
				if (((swarm.roadLength % spacing) / 2f + Mathf.Min (Noffsets)) < 0) {
					float tempSpacing = ((swarm.roadLength % spacing) / 2f + Mathf.Min (Noffsets) + 0.5f) / (Mathf.Floor(lumCount / 2f) - 1f);
					if (tempSpacing <= innerdSpaceLim)
						for (int x = 0; x < lumCount; x++) {
							if (Physics.CheckBox (new Vector3 ((spacing + tempSpacing) * Mathf.Floor (x / 2f) + (swarm.roadLength % (spacing + tempSpacing)) / 2f + Mathf.Min (Noffsets) - tempSpacing * Mathf.Floor(System.Array.IndexOf (Noffsets, Mathf.Min (Noffsets)) / 2f) + tempSpacing / 2f * ((float)Mathf.Floor(lumCount / 2f) - 1f), 0, roadWidth * Mathf.Abs (Mathf.Sin (Mathf.Deg2Rad * 90 * x)) * GlobalScaler.Instance().GetGlobalScale()), new Vector3 (0.25f * GlobalScaler.Instance().GetGlobalScale(), 2f * GlobalScaler.Instance().GetGlobalScale(), 0.25f * GlobalScaler.Instance().GetGlobalScale()), Quaternion.identity, structure)) {
								buildable = false;
								break;
							}
						}
					if (buildable) {
						spacing -= tempSpacing;
						xOffset += Mathf.Min (Noffsets) - tempSpacing * Mathf.Floor((System.Array.IndexOf (Noffsets, Mathf.Min (Noffsets)) / 2f)) + tempSpacing / 2f * (lumCount / 2 - 1f);
					} else
						buildable = true;
				}
				else {
						for (int x = 0; x < lumCount; x++) {
							if (Physics.CheckBox (new Vector3 ((spacing * Mathf.Floor (x / 2f) + (swarm.roadLength % spacing) / 2f + Mathf.Min (Noffsets)) * GlobalScaler.Instance().GetGlobalScale(), 0, roadWidth * Mathf.Abs (Mathf.Sin (Mathf.Deg2Rad * 90 * x)) * GlobalScaler.Instance().GetGlobalScale()), new Vector3 (0.25f * GlobalScaler.Instance().GetGlobalScale(), 2f * GlobalScaler.Instance().GetGlobalScale(), 0.25f * GlobalScaler.Instance().GetGlobalScale()), Quaternion.identity, structure))
								buildable = false;
						}
						if (buildable) {
							xOffset += Mathf.Min (Noffsets);
						} else
							buildable = true;
					}
				if (xOffset == initialOffset) {
					if ((spacing * Mathf.Floor ((lumCount - 1f) / 2f) + (swarm.roadLength % spacing) / 2f + Mathf.Max (Poffsets)) > swarm.roadLength) {
						float tempSpacing = (spacing * (Mathf.Floor(lumCount / 2f) - 1f) + (swarm.roadLength % spacing) / 2f + Mathf.Max (Poffsets) - swarm.roadLength - 0.5f) / (Mathf.Floor(lumCount / 2f) - 1f);
						if (tempSpacing <= innerdSpaceLim)
							for (int x = 0; x < lumCount; x++) {
								if (Physics.CheckBox (new Vector3 ((spacing + tempSpacing) * Mathf.Floor (x / 2f) + (swarm.roadLength % (spacing + tempSpacing)) / 2f + Mathf.Max (Poffsets) - tempSpacing * Mathf.Floor(System.Array.IndexOf (Poffsets, Mathf.Max (Poffsets)) / 2f) + tempSpacing / 2f * ((float)Mathf.Floor(lumCount / 2f) - 1f), 0, roadWidth * Mathf.Abs (Mathf.Sin (Mathf.Deg2Rad * 90 * x)) * GlobalScaler.Instance().GetGlobalScale()), new Vector3 (0.25f * GlobalScaler.Instance().GetGlobalScale(), 2f * GlobalScaler.Instance().GetGlobalScale(), 0.25f * GlobalScaler.Instance().GetGlobalScale()), Quaternion.identity, structure)) {
									buildable = false;
									break;
								}
							}
						if (buildable) {
							spacing -= tempSpacing;
							xOffset += Mathf.Max (Poffsets) - tempSpacing * Mathf.Floor((System.Array.IndexOf (Poffsets, Mathf.Max (Poffsets)) / 2f)) + tempSpacing / 2f * (lumCount / 2 - 1f);
						} else
							buildable = true;
					}
					else {
							for (int x = 0; x < lumCount; x++) {
								if (Physics.CheckBox (new Vector3 ((spacing * Mathf.Floor (x / 2f) + (swarm.roadLength % spacing) / 2f + Mathf.Max (Poffsets)) * GlobalScaler.Instance().GetGlobalScale(), 0, roadWidth * Mathf.Abs (Mathf.Sin (Mathf.Deg2Rad * 90 * x)) * GlobalScaler.Instance().GetGlobalScale()), new Vector3 (0.25f * GlobalScaler.Instance().GetGlobalScale(), 2f * GlobalScaler.Instance().GetGlobalScale(), 0.25f * GlobalScaler.Instance().GetGlobalScale()), Quaternion.identity, structure))
									buildable = false;
							}			
							if (buildable) {
								xOffset += Mathf.Max (Poffsets);
							}
						}
					if (xOffset == initialOffset)
						buildable = false;
				}
			}
		}
		int margin = Mathf.FloorToInt ((swarm.roadLength % spacing) / 2f);
		for (int b = 0; b < lumCount; b++) {
			if (swarm.arrangement3)
				lumX [b] = spacing * Mathf.Floor(b / 2)  + (swarm.roadLength % spacing) / 2f + xOffset;
			else 
				lumX [b] = spacing * b  + (swarm.roadLength % spacing) / 2f + xOffset;
			if (swarm.arrangement1 || swarm.arrangement3) {
				if (!reversed) {
					if (b % 2 == 0)
						lumY [b] = 0;
					else
						lumY [b] = roadWidth;
				} else {
					if (b % 2 == 0)
						lumY [b] = roadWidth;
					else
						lumY [b] = 0;
				}
			} else if (swarm.arrangement2) {
				if (reversed)
					lumY [b] = roadWidth;
				else
					lumY [b] = 0;
			}
			GameObject lampPost = Instantiate (luminaire, new Vector3 (lumX[b] * GlobalScaler.Instance().GetGlobalScale(), (height / 2f - 1f) * GlobalScaler.Instance().GetGlobalScale(), lumY[b] * GlobalScaler.Instance().GetGlobalScale()), new Quaternion (0, 0, 0, 0));
			lampPost.transform.localScale = new Vector3 (.25f * GlobalScaler.Instance().GetGlobalScale(), height / 2f * GlobalScaler.Instance().GetGlobalScale(), .25f * GlobalScaler.Instance().GetGlobalScale());
			lampList [b] = lampPost;
			for (int a = 0; a < longCalPoints * 3; a++) {
				C = Mathf.Rad2Deg * Mathf.Atan (Mathf.Abs (swarm.lumPointsY [a] - lumY [b]) / Mathf.Abs (swarm.lumPointsX [a] - lumX[b]));
				Y = Mathf.Rad2Deg * Mathf.Atan (Mathf.Sqrt (Mathf.Pow (swarm.lumPointsX [a] - lumX[b], 2) + Mathf.Pow (swarm.lumPointsY [a] - lumY [b], 2)) / height);
				illuminancePoint = Mathf.Lerp(Mathf.Lerp(CList [Mathf.FloorToInt (C / 2.5f), Mathf.FloorToInt (Y / 2.5f)],
					CList [Mathf.FloorToInt (C / 2.5f) + 1, Mathf.FloorToInt (Y / 2.5f)], (C % 2.5f) / 2.5f),
					Mathf.Lerp(CList [Mathf.FloorToInt (C / 2.5f), Mathf.FloorToInt (Y / 2.5f) + 1],
						CList [Mathf.FloorToInt (C / 2.5f) + 1, Mathf.FloorToInt (Y / 2.5f) + 1],
						(C % 2.5f) / 2.5f), (Y % 2.5f) / 2.5f);
				swarm.pointIlluminance [a] += (lampLumen * .85f / Mathf.Pow (height, 2)) 
					* (illuminancePoint / lampLumen * Mathf.Pow (Mathf.Cos (Mathf.Deg2Rad * Y), 3));
			}
		}
		totalIlluminance = 0f;
		for (int z = 0; z < longCalPoints * 3; z++) {
			illuminance = swarm.pointIlluminance [z];
			totalIlluminance += illuminance;
			if (minIlluminance == 0 || minIlluminance > illuminance) {
				minIlluminance = illuminance;
			}
		}
		averageIlluminance = totalIlluminance / (longCalPoints * 3);
		for (int i = 0; i < longCalPoints * 3; i++) {
			GameObject point = swarm.calPoints [i];
			point.GetComponent<LightLevel> ().SetIlluminance (swarm.pointIlluminance [i]);
			if (swarm.pointIlluminance [i] < minIlluminance + (averageIlluminance - minIlluminance) / 2)
				point.GetComponent<Renderer> ().material.color = Color.red;
			else if (swarm.pointIlluminance [i] >= minIlluminance + (averageIlluminance - minIlluminance) / 2 && swarm.pointIlluminance [i] < averageIlluminance)
				point.GetComponent<Renderer> ().material.color = Color.yellow;
			else if (swarm.pointIlluminance [i] >= averageIlluminance && swarm.pointIlluminance [i] < averageIlluminance * 2)
				point.GetComponent<Renderer> ().material.color = Color.green;
			else if (swarm.pointIlluminance [i] >= averageIlluminance * 2)
				point.GetComponent<Renderer> ().material.color = Color.blue;
		}
		for (int x = 0; x < lumCount; x++)
			for (int z = 0; z < 12; z++)
				for (int sun = 0; sun < monthlyHours [z]; sun++) {
					int percBlocked = 0;
					//Vector3 sunPos = new Vector3 (Mathf.Sin (Mathf.Deg2Rad * (azimuth [z, sun] - grid.dir)) * Mathf.Cos (Mathf.Deg2Rad * horizonAngle [z, sun]), Mathf.Sin (Mathf.Deg2Rad * horizonAngle [z, sun]), Mathf.Cos (Mathf.Deg2Rad * (azimuth [z, sun] - grid.dir)) * Mathf.Cos (Mathf.Deg2Rad * horizonAngle [z, sun]));
					Vector3 sunPos = Vector3.zero;
					if (Physics.BoxCast (lampList [x].transform.GetChild (0).transform.position, new Vector3 (lampList [x].transform.GetChild (0).transform.lossyScale.x / 2f, lampList [x].transform.GetChild (0).transform.lossyScale.y / 2f, lampList [x].transform.GetChild (0).transform.lossyScale.z / 2f), sunPos, Quaternion.identity, structure)) {
						for (int t = 0; t < 2; t++) {
							for (int u = 0; u < 2; u++) {
								if (Physics.BoxCast (new Vector3 (lampList [x].transform.GetChild (0).transform.position.x + Mathf.Pow (-1f, t) * lampList [x].transform.GetChild (0).transform.lossyScale.x / 4f, lampList [x].transform.GetChild (0).transform.position.y + lampList [x].transform.GetChild (0).transform.lossyScale.y / 2f, lampList [x].transform.GetChild (0).transform.position.z + Mathf.Pow (-1f, u) * lampList [x].transform.GetChild (0).transform.lossyScale.z / 4f), new Vector3 (lampList [x].transform.GetChild (0).transform.lossyScale.x / 4f, 0, lampList [x].transform.GetChild (0).transform.lossyScale.z / 4f), sunPos, Quaternion.identity, structure)) {
									for (int m = 0; m < 5; m++)
										for (int n = 0; n < 5; n++) {
											if (Physics.Raycast (new Vector3 (lampList [x].transform.GetChild (0).transform.position.x + Mathf.Pow (-1f, t) * lampList [x].transform.GetChild (0).transform.lossyScale.x / 4f - lampList [x].transform.GetChild (0).transform.lossyScale.x / 4f + m * lampList [x].transform.GetChild (0).transform.lossyScale.x / 10f, lampList [x].transform.GetChild (0).transform.position.y + lampList [x].transform.GetChild (0).transform.lossyScale.y / 2f, lampList [x].transform.GetChild (0).transform.position.z + Mathf.Pow (-1f, u) * lampList [x].transform.GetChild (0).transform.lossyScale.z / 4f - lampList [x].transform.GetChild (0).transform.lossyScale.z / 4f + n * lampList [x].transform.GetChild (0).transform.lossyScale.z / 10f), sunPos, Mathf.Infinity, structure)) {
												percBlocked++;
											}
										}
								}
							}
						}
					}
					float generation = solarGeneration [z, sun] * (0.5f * percBlocked / 100f);
					if (z < 7) {
						if (z % 2 == 0)
							generation *= 31f;
						else if (z == 1)
							generation *= 28f;
						else
							generation *= 30f;
					} else {
						if (z % 2 == 1)
							generation *= 31f;
						else
							generation *= 30f;
					}
					scores [x] -= generation;
				}
		for (int i = 0; i < lumCount; i++) {
			lampList [i].GetComponentInChildren<AverageScore> ().setScore (scores [i] * 0.607f);
			scores [i] = 0;
		}
		swarm.isBest = false;
		if (Mathf.Min (scores) < swarm.minScore) {
			if (xOffset == 0)
				xOffset = -(float)margin;
			else if (xOffset + 1f <= (float)margin)
				xOffset++;
			else
				score = Mathf.Min (scores);
            Debug.Log("Hi");
			CalcIlluminance ();
		}
		else
			score = Mathf.Min (scores);
		lightingEfficiency = averageIlluminance * spacing * roadWidth / luminairePower;
		for (int i = 0; i < longCalPoints * 3; i++) {
			swarm.pointIlluminance [i] = 0;
		}
	}
}
