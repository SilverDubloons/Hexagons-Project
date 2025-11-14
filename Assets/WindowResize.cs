using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class WindowResize : MonoBehaviour, IBeginDragHandler, IDragHandler, IPointerEnterHandler, IPointerExitHandler, IEndDragHandler
{
	private Vector2 dragStart;
	private Vector2 sizeStart;
	private Vector2 posStart;
	public bool bottom;
	public bool left;
	public bool onlyX;
	public bool onlyY;
	public bool onlyMovement;
	public RectTransform rt;
	public ResizableWindow resizeableWindow;
	public TMP_Text dataText;
	public Vector2 constantSize; // only needed if resizeableWindow is null
	
	public void ChangeCursor()
	{
		int newCursor = 0;
		if((bottom && left) || (!bottom && !left) && !onlyX && !onlyY)
		{
			newCursor = 1;
		}
		if((!bottom && left) || (bottom && !left) && !onlyX && !onlyY)
		{
			newCursor = 2;
		}
		if(onlyX)
		{
			newCursor = 3;
		}
		if(onlyY)
		{
			newCursor = 4;
		}
		if(onlyMovement)
		{
			newCursor = 5;
		}
		if(resizeableWindow != null)
		{
			resizeableWindow.uiData.mouseCursor.ChangeCursor(newCursor);
		}
		else
		{
			GameObject.FindWithTag("UIData").GetComponent<UIData>().mouseCursor.ChangeCursor(newCursor);
		}
	}
	
	public void OnPointerEnter(PointerEventData pointerEventData)
	{
		if(!Input.GetMouseButton(0))
		{
			ChangeCursor();
		}
	}
	public void OnPointerExit(PointerEventData pointerEventData)
	{
		if(!Input.GetMouseButton(0))
		{
			if(resizeableWindow != null)
			{
				resizeableWindow.uiData.mouseCursor.ChangeCursor(0);
			}
			else
			{
				GameObject.FindWithTag("UIData").GetComponent<UIData>().mouseCursor.ChangeCursor(0);
			}
		}
	}
	
	public void OnBeginDrag(PointerEventData eventData)
    {
		//dragStart = new Vector2((Input.mousePosition.x/Screen.width)*resizeableWindow.uiData.canvasRefRes.x,(Input.mousePosition.y/Screen.height)*resizeableWindow.uiData.canvasRefRes.y);
		dragStart = Input.mousePosition;
		sizeStart = rt.sizeDelta;
		posStart = rt.anchoredPosition;
		//print("dragStart = " + dragStart + " sizeStart = " + sizeStart + " posStart = " + posStart);
	}
	public void OnEndDrag(PointerEventData eventData)
	{
		if(resizeableWindow != null)
		{
			resizeableWindow.ResizingFinished();
		}
		else
		{
			GameObject.FindWithTag("UIData").GetComponent<UIData>().mouseCursor.ChangeCursor(0);
		}
	}
	
    public void OnDrag(PointerEventData data)
    {
		ChangeCursor();
		//Vector2 uiMousePos = new Vector2((Input.mousePosition.x/Screen.width)*resizeableWindow.uiData.canvasRefRes.x,(Input.mousePosition.y/Screen.height)*resizeableWindow.uiData.canvasRefRes.y);
		Vector2 uiMousePos = Input.mousePosition;
		Vector2 desiredSize = sizeStart;
		Vector2 desiredPos = posStart;
		float desiredXDifference = uiMousePos.x - dragStart.x;
		float desiredYDifference = uiMousePos.y - dragStart.y;
		desiredSize.x = sizeStart.x + desiredXDifference;
		desiredSize.y = sizeStart.y + desiredYDifference;
		int totalIconSize = 0;
		Vector2 minSize = new Vector2();
		if(resizeableWindow != null)
		{
			totalIconSize = Mathf.RoundToInt(resizeableWindow.uiData.itemSize + resizeableWindow.uiData.itemPadding);
			minSize = resizeableWindow.minimumSize;
		}
		else
		{
			minSize = constantSize;
		}
		bool snapping = false;
		if(!onlyMovement && (Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftShift)))
		{
			snapping = true;
		}
		if(snapping) // base size is 273, 342
		{
			desiredXDifference = RoundToNearestMultiple(desiredXDifference, totalIconSize);
			desiredYDifference = RoundToNearestMultiple(desiredYDifference, totalIconSize);
			desiredSize.x = RoundToNearestMultiple(sizeStart.x, totalIconSize) - (RoundToNearestMultiple(minSize.x, totalIconSize) - minSize.x) + desiredXDifference;
			desiredSize.y = RoundToNearestMultiple(sizeStart.y, totalIconSize) - (RoundToNearestMultiple(minSize.y, totalIconSize) - minSize.y) + desiredYDifference;
		}
		//dataText.text = "uiMousePos = " + uiMousePos + "\ndesiredXDifference = " + desiredXDifference + "\ndesiredYDifference = " + desiredYDifference;
		if(bottom)
		{
			desiredPos.y = posStart.y + desiredYDifference;
			desiredSize.y = sizeStart.y - desiredYDifference;
			if(snapping)
			{
				desiredSize.y = RoundToNearestMultiple(sizeStart.y, totalIconSize) - (RoundToNearestMultiple(minSize.y, totalIconSize) - minSize.y) - desiredYDifference;
				desiredPos.y = (posStart.y + sizeStart.y) - desiredSize.y;
			}
		}
		if(left)
		{
			desiredPos.x = posStart.x + desiredXDifference;
			desiredSize.x = sizeStart.x - desiredXDifference;
			if(snapping)
			{
				desiredSize.x = RoundToNearestMultiple(sizeStart.x, totalIconSize) - (RoundToNearestMultiple(minSize.x, totalIconSize) - minSize.x) - desiredXDifference;
				desiredPos.x = (posStart.x + sizeStart.x) - desiredSize.x;
			}
		}
		if(desiredSize.x < minSize.x)
		{
			desiredSize.x = minSize.x;
			if(left)
			{
				desiredPos.x = posStart.x + sizeStart.x - minSize.x;
			}
			else
			{
				desiredPos.x = posStart.x;
			}
		}
		if(desiredSize.y < minSize.y)
		{
			desiredSize.y = minSize.y;
			if(bottom)
			{
				desiredPos.y = posStart.y + sizeStart.y - minSize.y;
			}
			else
			{
				desiredPos.y = posStart.y;
			}
		}
		if(onlyMovement)
		{
			desiredSize = sizeStart;
			desiredPos.x = posStart.x + desiredXDifference;
			desiredPos.y = posStart.y + desiredYDifference;
		}
		//if(Mathf.Floor(desiredPos.x + desiredSize.x) > resizeableWindow.uiData.canvasRefRes.x)
		if(Mathf.Floor(desiredPos.x + desiredSize.x) > Screen.width)
		{
			if(onlyMovement)
			{
				//desiredPos.x = resizeableWindow.uiData.canvasRefRes.x - sizeStart.x;
				desiredPos.x = Screen.width - sizeStart.x;
			}
			else
			{
				//desiredSize.x = resizeableWindow.uiData.canvasRefRes.x - posStart.x;
				desiredSize.x = Screen.width - posStart.x;
				desiredPos.x = posStart.x;
			}
		}
		//if(Mathf.Floor(desiredPos.y + desiredSize.y) > resizeableWindow.uiData.canvasRefRes.y)
		if(Mathf.Floor(desiredPos.y + desiredSize.y) > Screen.height)
		{
			if(onlyMovement)
			{
				//desiredPos.y = resizeableWindow.uiData.canvasRefRes.y - sizeStart.y;
				desiredPos.y = Screen.height - sizeStart.y;
			}
			else
			{
				//desiredSize.y = resizeableWindow.uiData.canvasRefRes.y - posStart.y;
				desiredSize.y = Screen.height - posStart.y;
				desiredPos.y = posStart.y;
			}
		}
		if(Mathf.Ceil(desiredPos.x) < 0)
		{
			desiredPos.x = 0;
			if(!onlyMovement)
			{
				desiredSize.x = posStart.x + sizeStart.x;
			}
		}
		if(Mathf.Ceil(desiredPos.y) < 0)
		{
			desiredPos.y = 0;
			if(!onlyMovement)
			{
				desiredSize.y = posStart.y + sizeStart.y;
			}
			
		}
		if(onlyX)
		{
			desiredSize.y = sizeStart.y;
			desiredPos.y = posStart.y;
		}
		if(onlyY)
		{
			desiredSize.x = sizeStart.x;
			desiredPos.x = posStart.x;
		}
		rt.sizeDelta = desiredSize;
		rt.anchoredPosition = desiredPos;
		if(resizeableWindow != null)
		{
			resizeableWindow.WindowResized();
		}
	}
	
	public float RoundToNearestMultiple(float number, int multiple)
	{
		float nearestMultiple = Mathf.Round(number / multiple) * multiple;
		return nearestMultiple;
	}
}


