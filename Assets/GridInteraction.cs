using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridInteraction : MonoBehaviour {
	public Button button;
	public int xIndex, yIndex, setNo;
	public bool selected = false;
	private Grid grid;

	void Start () {
		grid = this.transform.GetComponentInParent<Grid> ();
	}

	void Update () {
		if (selected)
			button.image.color = Color.green;
		else
			button.image.color = Color.white;
	}

	public void setEdge() {
		if (!grid.highlighting) {
			grid.createNewSet ();
			grid.highlighting = true;
			grid.xEdge1[grid.coordSet - 1] = xIndex;
			grid.yEdge1[grid.coordSet - 1] = yIndex;
			grid.xEdge2[grid.coordSet - 1] = xIndex;
			grid.yEdge2[grid.coordSet - 1] = yIndex;
		} else {
			grid.highlighting = false;
			grid.xEdge2[grid.coordSet - 1] = xIndex;
			grid.yEdge2[grid.coordSet - 1] = yIndex;
			grid.SetHeight ();
		}
	}

	public void setOtherEdge() {
		grid.xEdge2[grid.coordSet - 1] = xIndex;
		grid.yEdge2[grid.coordSet - 1] = yIndex;
	}
}
