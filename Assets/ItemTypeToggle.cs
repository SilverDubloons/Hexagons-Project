using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemTypeToggle : MonoBehaviour
{
	public int itemType;
	public ResizableWindow resizableWindow;
	public RectTransform rt;
	public UnityEngine.UI.Image background;
    public UnityEngine.UI.Image representativeImage;
	
	public void ResetToggleColors()
	{
		representativeImage.color = Color.white;
		background.color = resizableWindow.uiData.creamWhite;
	}
	
	public void ChangeColorsToDisabled()
	{
		representativeImage.color = resizableWindow.uiData.slightlyTransparentWhite;
		background.color = Color.red;
	}
	
	public void ItemTypeToggleClicked()
	{
		if(Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftShift))
		{
			if(resizableWindow.itemTypesExcluded.Contains(itemType))
			{
				resizableWindow.itemTypesExcluded.Remove(itemType);
				ResetToggleColors();
			}
			else
			{
				resizableWindow.itemTypesExcluded.Add(itemType);
				ChangeColorsToDisabled();
			}
		}
		else
		{
			if(resizableWindow.itemTypesExcluded.Count > 0 && !resizableWindow.itemTypesExcluded.Contains(itemType))
			{
				resizableWindow.itemTypesExcluded.Clear();
				for(int i = 0; i < resizableWindow.itemTypeToggleRects.Count; i++)
				{
					ItemTypeToggle curToggle = resizableWindow.itemTypeToggleRects[i].GetComponent<ItemTypeToggle>();
					curToggle.ResetToggleColors();
				}
			}
			else
			{
				resizableWindow.itemTypesExcluded.Clear();
				for(int i = 0; i < resizableWindow.itemTypeToggleRects.Count; i++)
				{
					ItemTypeToggle curToggle = resizableWindow.itemTypeToggleRects[i].GetComponent<ItemTypeToggle>();
					if(curToggle.itemType != itemType)
					{
						curToggle.ChangeColorsToDisabled();
						resizableWindow.itemTypesExcluded.Add(curToggle.itemType);
					}
					else
					{
						curToggle.ResetToggleColors();
					}
				}
			}
		}
		resizableWindow.ClearCurrentSelection();
		resizableWindow.RearangeContainer();
	}
}
