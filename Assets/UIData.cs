using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIData : MonoBehaviour
{
	public Canvas canvas;
	public Vector2 canvasRefRes = new Vector2(2560,1440);
	public MouseCursor mouseCursor;
	
    public GameObject rangeGraphPrefab;
	public GameObject graphLinePrefab;
	public GameObject graphAxisLabelXPrefab;
	public GameObject graphAxisLabelYPrefab;
	public Transform rangeGraphParent;
	
	public Color creamWhite;
	public Color slightlyTransparentWhite;
	public Color darkGray;
	public Color lightGray;
	
	// for container UI
	public GameObject containerWindowPrefab;
	public ContainersHandler containersHandler;
	public GameObject itemUIPrefab;
	public float itemPadding;
	public float itemSize;
	public float itemBorderPadding;
	public float scrollbarWidth;
	public GameObject itemTypeTogglePrefab;
	public Sprite[] itemTypeRepresentativeSprites;
	public float itemTypeToggleSize;
	public float itemTypeTogglePadding;
	public float cornerResizerSize;
	public float menuBarSize;
	public float generalPadding;
	public float edgeSize;
	public float sortByButtonSize;
	public Transform destroyedObjectsTempStorage;
	public ItemSplitInterface itemSplitInterface;
	
	void Start()
	{
		canvasRefRes = new Vector2(Screen.width, Screen.height);
		//canvas.GetComponent<CanvasScaler>().referenceResolution = canvasRefRes;
	}
}
