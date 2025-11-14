using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TexturePreview : MonoBehaviour, IPointerClickHandler
{
	public int index;
	public TextureSelector textureSelector;
	
    public void OnPointerClick(PointerEventData eventData)
    {
		textureSelector.TextureSelected(index);
	}
    void Start()
    {
        
    }
    void Update()
    {
        
    }
}
