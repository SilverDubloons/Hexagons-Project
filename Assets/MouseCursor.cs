using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MouseCursor : MonoBehaviour
{
	public UIData uiData;
	public RectTransform rt;
	public Image imageComponent;
	
	public Sprite[] cursors;
	public Vector2[] cursorOffsets;
	public int currentCursor;
	public float cursorScale;
	
	void Start()
	{
		Cursor.visible = false;
		ChangeCursor(0);
	}
	
    void Update()
    {
        //Vector2 cursorPos = new Vector2((Input.mousePosition.x/Screen.width)*uiData.canvasRefRes.x, (Input.mousePosition.y/Screen.height)*uiData.canvasRefRes.y);
        Vector2 cursorPos = Input.mousePosition;
		rt.anchoredPosition = cursorPos + cursorOffsets[currentCursor] * cursorScale;
    }
	
	public void ChangeCursor(int newCursor)
	{
		Cursor.visible = false;
		currentCursor = newCursor;
		imageComponent.sprite = cursors[newCursor];
		rt.localScale = new Vector3(cursorScale, cursorScale, 1);
	}
}
