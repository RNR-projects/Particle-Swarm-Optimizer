using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridInteraction : MonoBehaviour {
	public Button button;
	public int xIndex, yIndex;
	private bool selected = false;
	private bool shouldHighlight = false;

    private void Update()
    {
		if (!this.selected)
		{
			if (this.shouldHighlight)
			{
				this.button.image.color = Color.green;
			}
			else
				this.button.image.color = Color.white;
			this.shouldHighlight = false;
		}
		
    }

    public void SetEdge()
	{
		BuildingLocationsManager.Instance().RegisterEdge(this.xIndex, this.yIndex);
	}

	public int[] GetTemporaryEdge() {
		int[] coordinates = new int[2];
		coordinates[0] = this.xIndex;
		coordinates[1] = this.yIndex;
		return coordinates;
	}

	public void SelectBlock(bool value)
    {
		this.selected = value;
		if (selected)
			button.image.color = Color.green;
		else
			button.image.color = Color.white;
	}

	public void HighlightBlock()
    {
		if (!this.selected)
			this.shouldHighlight = true;
    }
}
