using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EditorButton : MonoBehaviour, IPointerClickHandler
{
    public int index;
	public LevelEditor levelEditor;
	
	public void OnClick()
	{
		levelEditor.EditorButtonClicked(index);
	}
	
	public void OnPointerClick(PointerEventData eventData)
    {
		if (eventData.button == PointerEventData.InputButton.Right)
		{
            levelEditor.EditorButtonRightClicked(index);
			levelEditor.soloSelectionIndicator.gameObject.SetActive(true);
			levelEditor.soloSelectionIndicator.anchoredPosition = this.GetComponent<RectTransform>().anchoredPosition;
		}
	}
    
    void Update()
    {
        
    }
}
