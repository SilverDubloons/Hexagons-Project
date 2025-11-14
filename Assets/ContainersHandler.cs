using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainersHandler : MonoBehaviour
{
	public List<ResizableWindow> resizableWindows;
	public UIData uiData;
	public bool currentlyDragging;
	public UIItem singularItemBeingDragged;
	
	public void ClearSelectionInOtherWindows(ResizableWindow originalWindow)
	{
		for(int i = 0; i < resizableWindows.Count; i++)
		{
			if(resizableWindows[i] != originalWindow)
			{
				resizableWindows[i].ClearCurrentSelection();
			}
		}
	}
	
	public void ResetBorderColors()
	{
		for(int i = 0; i < resizableWindows.Count; i++)
		{
			resizableWindows[i].windowBorderRect.GetComponent<UnityEngine.UI.Image>().color = Color.black;
		}
	}
	
	public int WhichIndexIsMouseOver()
	{
		//Vector2 uiMousePos = new Vector2((Input.mousePosition.x/Screen.width)*uiData.canvasRefRes.x, (Input.mousePosition.y/Screen.height)*uiData.canvasRefRes.y);
		Vector2 uiMousePos = Input.mousePosition;
		for(int i = transform.childCount - 1; i >= 0; i--)
		{
			RectTransform childRT = transform.GetChild(i).GetComponent<RectTransform>();
			if(uiMousePos.x >= childRT.anchoredPosition.x && uiMousePos.x <= childRT.anchoredPosition.x + childRT.sizeDelta.x && uiMousePos.y >= childRT.anchoredPosition.y && uiMousePos.y <= childRT.anchoredPosition.y + childRT.sizeDelta.y)
			{
				ResizableWindow windowMouseIsOver = childRT.GetComponent<ResizableWindow>();
				foreach(ResizableWindow curWindow in resizableWindows)
				{
					if(curWindow == windowMouseIsOver)
					{
						childRT.transform.SetSiblingIndex(transform.childCount - 1);
						return resizableWindows.IndexOf(windowMouseIsOver);
					}
				}
				
			}
		}
		return -1;
	}
	
	void Update()
	{
		if(Input.GetMouseButtonDown(0))
		{
			WhichIndexIsMouseOver();
		}
	}
}
