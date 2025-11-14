using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
	private UIData uiData;
	public int cursorOnMouseOver;
	
	void Start()
	{
		uiData = GameObject.FindWithTag("UIData").GetComponent<UIData>();
	}
	
    void OnMouseEnter()
	{
		uiData.mouseCursor.ChangeCursor(cursorOnMouseOver);
	}
	
	void OnMouseExit()
	{
		uiData.mouseCursor.ChangeCursor(0);
	}
	
	void OnMouseDown()
	{
		
	}
}
