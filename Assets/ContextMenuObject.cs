using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class ContextMenuObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
	private bool isMouseOver = false;
	private float mouseOverTime = 0f;
	public TMP_Text label;
	public TMP_Text hotkeyLabel;
	public UnityEngine.UI.Image mouseOverBackdrop;
	public UnityEngine.UI.Image expandArrow;
	public List<ContextMenuObject> childMenus;
	public bool canBeClicked = false;
	
    public void OnPointerEnter(PointerEventData eventData)
    {
		mouseOverBackdrop.gameObject.SetActive(true);
        isMouseOver = true;
		mouseOverTime = 0f;
    }
	public void OnPointerExit(PointerEventData eventData)
    {
		mouseOverBackdrop.gameObject.SetActive(false);
        isMouseOver = false;
		mouseOverTime = 0f;
		SetActiveChildren(false);
    }
	public void OnPointerClick(PointerEventData eventData)
    {
		if(canBeClicked)
		{
			ContextMenuObject cmoParent = transform.parent.GetComponent<ContextMenuObject>();
			LevelEditor levelEditor = GameObject.FindWithTag("LevelEditor").GetComponent<LevelEditor>();
			if(levelEditor != null)
			{
				if(cmoParent != null)
				{
					levelEditor.ContextMenuClicked(cmoParent.label.text,label.text);
				}
				else
				{
					levelEditor.ContextMenuClicked(label.text, "null");
				}
			}
			else
			{
				Debug.Log("Couldn't find Level Editor");
			}
		}
    }
	
	public void SetupContextMenu(string labelText, string hotkeyLabelText, bool hasChildren, bool clickable)
	{
		label.text = labelText;
		hotkeyLabel.text = hotkeyLabelText;
		if(!hasChildren)
		{
			expandArrow.gameObject.SetActive(false);
		}
		canBeClicked = clickable;
	}
	
	public void SetActiveChildren(bool active)
	{
		for(int i = 0; i < childMenus.Count; i++)
		{
			childMenus[i].mouseOverBackdrop.gameObject.SetActive(false);
			childMenus[i].gameObject.SetActive(active);
		}
	}
	
	
    void Start()
    {
        
    }
    void Update()
    {
        if(isMouseOver)
		{
			mouseOverTime += Time.deltaTime;
			if(mouseOverTime >= 0.1f)
			{
				SetActiveChildren(true);
				mouseOverTime = -9001f;
			}
		}
    }
}
