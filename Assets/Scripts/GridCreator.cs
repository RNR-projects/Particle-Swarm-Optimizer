using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridCreator : MonoBehaviour {
	[SerializeField] private GameObject button, reset, scale;
	[SerializeField] private GameObject panel, label;
	private List<GameObject> gridObjects = new List<GameObject>();	
	public void CreateNewGrid()
    {
		this.CreateResetButton();
		this.CreateBlockLifeScaleReference();
		this.CreateBuildingPositionBlocks();
		this.CreateLuminaireAreaMarkers();
    }

	private void CreateResetButton()
    {
		GameObject resetButton = Instantiate(reset, new Vector3(-GlobalScaler.Instance().GetGlobalScale() / 2f, 
							OptimizationParameterManager.Instance().GetRoadWidth() / 2f * GlobalScaler.Instance().GetGlobalScale(), 
							2f * GlobalScaler.Instance().GetGlobalScale()), 
							Quaternion.Euler(new Vector3(0, 0, 90)));
		resetButton.transform.localScale = new Vector3(3f / 25f * GlobalScaler.Instance().GetGlobalScale(), 
											GlobalScaler.Instance().GetGlobalScale() / 25f, GlobalScaler.Instance().GetGlobalScale() / 25f);
		resetButton.transform.SetParent(this.transform, false);
		resetButton.GetComponent<Button>().onClick.AddListener(BuildingLocationsManager.Instance().Reset);
		this.gridObjects.Add(resetButton);
	}

	private void CreateBlockLifeScaleReference()
    {
		GameObject BlockScale = Instantiate(scale, new Vector3(-GlobalScaler.Instance().GetGlobalScale() * 2f, 
							OptimizationParameterManager.Instance().GetRoadWidth() / 2f * GlobalScaler.Instance().GetGlobalScale(), 
							2f * GlobalScaler.Instance().GetGlobalScale()), 
							new Quaternion(0, 0, 0, 0));
		BlockScale.transform.localScale = new Vector3(GlobalScaler.Instance().GetGlobalScale() / 25f, 
											GlobalScaler.Instance().GetGlobalScale() / 25f, 1);
		BlockScale.transform.SetParent(this.transform, false);
		this.gridObjects.Add(BlockScale);
	}

	private void CreateBuildingPositionBlocks()
    {
		BuildingLocationsManager.Instance().InitializeStructurePlaceHolders(
												(int)OptimizationParameterManager.Instance().GetRoadLength() + 10, 10);
		//extra buffer of 5 meters on both ends, cares about buildings only at most 5 meters away from the road's edge
		for (int x = 0; x < OptimizationParameterManager.Instance().GetRoadLength() + 10; x++)
		{
			for (int y = 0; y < 10; y++)//create the buttons for the grid
			{
				GameObject gridBlock;
				if (y < 5)
					gridBlock = Instantiate(button, new Vector3((x - 4.5f) * GlobalScaler.Instance().GetGlobalScale(), 
												-(y + 0.5f) * GlobalScaler.Instance().GetGlobalScale(), 
												2f * GlobalScaler.Instance().GetGlobalScale()), new Quaternion(0, 0, 0, 0));
				else
					gridBlock = Instantiate(button, new Vector3((x - 4.5f) * GlobalScaler.Instance().GetGlobalScale(), 
							(y - 4.5f + OptimizationParameterManager.Instance().GetRoadWidth()) * GlobalScaler.Instance().GetGlobalScale(), 
							2f * GlobalScaler.Instance().GetGlobalScale()), new Quaternion(0, 0, 0, 0));
				gridBlock.transform.localScale = new Vector3(GlobalScaler.Instance().GetGlobalScale() / 25f, 
																GlobalScaler.Instance().GetGlobalScale() / 25f, 1);
				gridBlock.transform.SetParent(this.transform, false);

				GridInteraction structureBlock = gridBlock.GetComponentInChildren<GridInteraction>();
				structureBlock.xIndex = x;
				structureBlock.yIndex = y;

				BuildingLocationsManager.Instance().RegisterStructurePlaceHolderBlock(gridBlock, x, y);
				this.gridObjects.Add(gridBlock);
			}
			if (x % 10 == 5)//distance markers
			{
				GameObject marker = Instantiate(label, new Vector3((x - 5f) * GlobalScaler.Instance().GetGlobalScale(),
									OptimizationParameterManager.Instance().GetRoadWidth() / 2f * GlobalScaler.Instance().GetGlobalScale(), 
									2f * GlobalScaler.Instance().GetGlobalScale()), new Quaternion(0, 0, 0, 0));
				marker.transform.localScale = new Vector3(1, GlobalScaler.Instance().GetGlobalScale() / 5f, 1);
				marker.transform.SetParent(this.transform, false);
				marker.GetComponentInChildren<Text>().text = (x - 5f) + "m";
				this.gridObjects.Add(marker);
			}
		}
	}

	private void CreateLuminaireAreaMarkers()
    {
		GameObject luminaireAreaMarker = Instantiate(panel, new Vector3(OptimizationParameterManager.Instance().GetRoadLength() / 2 *
											GlobalScaler.Instance().GetGlobalScale(), -.6f * GlobalScaler.Instance().GetGlobalScale(), 
											2f * GlobalScaler.Instance().GetGlobalScale()), new Quaternion(0, 0, 0, 0));
		luminaireAreaMarker.transform.SetParent(this.transform, false);
		luminaireAreaMarker.transform.localScale = new Vector3(OptimizationParameterManager.Instance().GetRoadLength() / 125f * 
												GlobalScaler.Instance().GetGlobalScale(), GlobalScaler.Instance().GetGlobalScale() / 12.5f, 
												GlobalScaler.Instance().GetGlobalScale() / 25f);
		this.gridObjects.Add(luminaireAreaMarker);

		if (OptimizationParameterManager.Instance().GetLuminaireArrangement() != 
				OptimizationParameterManager.LuminaireArrangementSettings.OneSided)
		{
			GameObject luminaireAreaMarker2 = Instantiate(panel, new Vector3(OptimizationParameterManager.Instance().GetRoadLength() / 2 * 
								GlobalScaler.Instance().GetGlobalScale(), 
								(0.6f + OptimizationParameterManager.Instance().GetRoadWidth()) * GlobalScaler.Instance().GetGlobalScale(), 
								2f * GlobalScaler.Instance().GetGlobalScale()), new Quaternion(0, 0, 0, 0));
			luminaireAreaMarker2.transform.SetParent(this.transform, false);
			luminaireAreaMarker2.transform.localScale = new Vector3(OptimizationParameterManager.Instance().GetRoadLength() / 125f * 
												GlobalScaler.Instance().GetGlobalScale(), GlobalScaler.Instance().GetGlobalScale() / 12.5f, 
												GlobalScaler.Instance().GetGlobalScale() / 25f);
			this.gridObjects.Add(luminaireAreaMarker2);
		}
	}

	public void ClearGridObjects()
	{
		foreach (GameObject item in this.gridObjects)
		{
			Destroy(item);
		}
		this.gridObjects.Clear();
	}

	public bool AreaLayoutInstantiated()
    {
		return this.gridObjects.Count > 0;
    }
}
