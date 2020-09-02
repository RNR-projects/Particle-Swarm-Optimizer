using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingLocationsManager
{
	private static BuildingLocationsManager sharedInstance;

	private List<int> buildingXEdge1;
	private List<int> buildingXEdge2;
	private List<int> buildingYEdge1;
	private List<int> buildingYEdge2;
	private GameObject[,] structuresToCreate;
	private bool isCompletingNewSet;
	private List<float> buildingHeights;
	//private int[] temporarySecondCorner;
	private GameObject builderUI;

	public static BuildingLocationsManager Instance()
    {
		if (sharedInstance == null)
			sharedInstance = new BuildingLocationsManager();
		return sharedInstance;
    }

	private BuildingLocationsManager()
	{
		this.buildingXEdge1 = new List<int>();
		this.buildingXEdge2 = new List<int>();
		this.buildingYEdge1 = new List<int>();
		this.buildingYEdge2 = new List<int>();
		this.isCompletingNewSet = false;
		this.buildingHeights = new List<float>();
		//this.temporarySecondCorner = new int[2];
	}

	/*public void HighlightHoveredArea(LayerMask mask) does not work right now
	{
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		Debug.DrawRay(ray.origin, ray.direction * 1000f, Color.green, 1000f);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, mask))
        {
            GridInteraction structureBlock = hit.collider.GetComponent<GridInteraction>();
			Debug.Log("ok");
            if (structureBlock != null)
            {
				Debug.Log("hello");
                this.temporarySecondCorner = structureBlock.GetTemporaryEdge();
                for (int i = 0; i < OptimizationParameterManager.Instance().GetRoadLength() + 10; i++)
                {
                    for (int j = 0; j < 10; j++)
                    {//highlight all blocks within the set corner and the currently hovered over corner
                        if (((i <= this.temporarySecondCorner[0] && i >= this.buildingXEdge1[this.buildingXEdge1.Count - 1]) ||
                                (i <= this.buildingXEdge1[this.buildingXEdge1.Count - 1] && i >= this.temporarySecondCorner[0])) &&
                                    ((j <= this.temporarySecondCorner[1] && j >= this.buildingYEdge1[this.buildingYEdge1.Count - 1]) ||
                                    (j <= this.buildingYEdge1[this.buildingYEdge1.Count - 1] && j >= this.temporarySecondCorner[1])))
                        {
							Debug.Log("hi");
                            this.structuresToCreate[i, j].GetComponent<GridInteraction>().HighlightBlock();
                        }
                    }
                }
            }
        }
    }*/

	public void Reset()
	{
		this.buildingXEdge1.Clear();
		this.buildingXEdge2.Clear();
		this.buildingYEdge1.Clear();
		this.buildingYEdge2.Clear();
		this.isCompletingNewSet = false;
		foreach(GameObject block in this.structuresToCreate)
			block.GetComponent<GridInteraction>().SelectBlock(false);

		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = true;
	}

	public void InitializeStructurePlaceHolders(int numRows, int numCols)
    {
		this.structuresToCreate = new GameObject[numRows, numCols];
    }

	public void RegisterStructurePlaceHolderBlock(GameObject item, int xIndex, int yIndex)
    {
		this.structuresToCreate[xIndex, yIndex] = item;
    }

	public void RegisterEdge(int xIndex, int yIndex)
    {
		if (!this.isCompletingNewSet)//false meaning there is currently no new set to make
		{
			this.buildingXEdge1.Add(xIndex);
			this.buildingYEdge1.Add(yIndex);
		}
		else
        {
			this.buildingXEdge2.Add(xIndex);
			this.buildingYEdge2.Add(yIndex);

			for (int i = 0; i < OptimizationParameterManager.Instance().GetRoadLength() + 10; i++)
			{
				for (int j = 0; j < 10; j++)
				{//highlight all blocks within the 2 corners
					if (((i <= this.buildingXEdge2[this.buildingXEdge2.Count - 1] && i >= 
							this.buildingXEdge1[this.buildingXEdge1.Count - 1]) ||
						(i <= this.buildingXEdge1[this.buildingXEdge1.Count - 1] && i >= 
							this.buildingXEdge2[this.buildingXEdge2.Count - 1])) &&
						((j <= this.buildingYEdge2[this.buildingYEdge2.Count - 1] && j >= 
							this.buildingYEdge1[this.buildingYEdge1.Count - 1]) ||
						(j <= this.buildingYEdge1[this.buildingYEdge1.Count - 1] && j >= 
							this.buildingYEdge2[this.buildingYEdge2.Count - 1])))
					{
						this.structuresToCreate[i, j].GetComponent<GridInteraction>().SelectBlock(true);
					}
				}
			}

			this.SetBuildingHeight();
        }
		this.isCompletingNewSet = !this.isCompletingNewSet;
    }

	public bool GetIsCompletingNewSet()
    {
		return this.isCompletingNewSet;
    } 

	public void SetBuilderUI(GameObject gameObject)
    {
		this.builderUI = gameObject;
    }

	private void SetBuildingHeight()
    {
		this.builderUI.SetActive(true);
    }

	public void RegisterNewBuildingHeight(float value)
    {
		this.buildingHeights.Add(value);
    } 

	public List<Vector3> GetBuildingLocations()
    {
		List<Vector3> buildingLocations = new List<Vector3>();

		for (int i = 0; i < buildingHeights.Count; i++)
        {
			if (this.buildingYEdge1[i] < 5 && this.buildingYEdge2[i] < 5)
			{
				buildingLocations.Add(new Vector3((this.buildingXEdge1[i] - 4.5f + (this.buildingXEdge2[i] - this.buildingXEdge1[i])
					/ 2f) * GlobalScaler.Instance().GetGlobalScale(),
					(this.buildingHeights[i] / 2f - 1) * GlobalScaler.Instance().GetGlobalScale(),
					-(this.buildingYEdge1[i] + .5f + (this.buildingYEdge2[i] - this.buildingYEdge1[i]) / 2f)
					* GlobalScaler.Instance().GetGlobalScale()));
			}
			else if (this.buildingYEdge1[i] >= 5 && this.buildingYEdge2[i] >= 5)
			{
				buildingLocations.Add(new Vector3((this.buildingXEdge1[i] - 4.5f + (this.buildingXEdge2[i] - this.buildingXEdge1[i]) 
					/ 2f) * GlobalScaler.Instance().GetGlobalScale(),
					(this.buildingHeights[i] / 2f - 1) * GlobalScaler.Instance().GetGlobalScale(),
					(this.buildingYEdge1[i] - 4.5f + OptimizationParameterManager.Instance().GetRoadWidth() + 
					(this.buildingYEdge2[i] - this.buildingYEdge1[i]) / 2f) * GlobalScaler.Instance().GetGlobalScale()));
			}
		}

		return buildingLocations;
	}

	public List<Vector3> GetBuildingSizes()
    {
		List<Vector3> buildingSizes = new List<Vector3>();

		for (int i = 0; i < buildingHeights.Count; i++)
		{
			buildingSizes.Add(new Vector3((Mathf.Abs(this.buildingXEdge1[i] - this.buildingXEdge2[i]) + 1) *
				GlobalScaler.Instance().GetGlobalScale(), this.buildingHeights[i] * GlobalScaler.Instance().GetGlobalScale(),
				(Mathf.Abs(this.buildingYEdge1[i] - this.buildingYEdge2[i]) + 1) * GlobalScaler.Instance().GetGlobalScale()));
		}

		return buildingSizes;
    }
}
