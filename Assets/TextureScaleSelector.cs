using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class TextureScaleSelector : MonoBehaviour
{
    public UnityEngine.UI.Slider xSlider;
	public TMP_InputField xInputField;
	public UnityEngine.UI.Slider ySlider;
	public TMP_InputField yInputField;
	public Vector2 inputScale;
	public bool allTextures;
	public int rendererIndex;
	public LevelEditor levelEditor;
	public UnityEngine.UI.Toggle linkToggle;
	
	public void xSliderUpdated()
	{
		inputScale.x = (float)Mathf.Round(xSlider.value*10f)/10f;
		xInputField.text = "" + inputScale.x;
		if(linkToggle.isOn)
		{
			inputScale.y = (float)Mathf.Round(xSlider.value*10f)/10f;
			yInputField.text = "" + inputScale.x;
			ySlider.value = inputScale.y;
		}
	}
	
	public void ySliderUpdated()
	{
		inputScale.y = (float)Mathf.Round(ySlider.value*10f)/10f;
		yInputField.text = "" + inputScale.y;
		if(linkToggle.isOn)
		{
			inputScale.x = (float)Mathf.Round(ySlider.value*10f)/10f;
			xInputField.text = "" + inputScale.y;
			xSlider.value = inputScale.x;
		}
	}
	
	public void SetupTextureScaleSelector(bool allTex, int renIndex, Vector2 baseScale)
	{
		allTextures = allTex;
		rendererIndex = renIndex;
		this.gameObject.SetActive(true);
		inputScale = baseScale;
		xInputField.text = "" + inputScale.x;
		yInputField.text = "" + inputScale.y;
		xSlider.value = inputScale.x;
		ySlider.value = inputScale.y;
	}
	
	public void ApplyScaleChange()
	{
		try
		{
 			inputScale.x = float.Parse(xInputField.text);
			inputScale.y = float.Parse(yInputField.text);
		}
		catch
		{
			inputScale.x = (float)Mathf.Round(xSlider.value*10f)/10f;
			inputScale.y = (float)Mathf.Round(ySlider.value*10f)/10f;
		}
		for(int i = 0; i < levelEditor.currentSelectionTransforms.Count; i++)
		{
			EditorData editorData = levelEditor.currentSelectionTransforms[i].GetComponent<EditorData>();
			if(allTextures)
			{
				for(int j = 0; j < editorData.renderers.Length; j++)
				{
					editorData.renderers[j].material.SetTextureScale("_MainTex", inputScale);
					editorData.textureScales[j] = inputScale;
				}
			}
			else
			{
				editorData.renderers[rendererIndex].material.SetTextureScale("_MainTex", inputScale);
				editorData.textureScales[rendererIndex] = inputScale;
			}
		}
	}
	
	public void ApplyButtonClicked()
	{
		ApplyScaleChange();
		this.gameObject.SetActive(false);
	}
	
	public void InputFieldUpdated()
	{
		/* try
		{
 			xVal = float.Parse(xInputField.text);
			xSlider.value = xVal;
			yVal = float.Parse(yInputField.text);
			ySlider.value = yVal; 
		}
		catch (Exception e)
		{
			//Debug.LogError("An exception occurred: " + e.Message);
			if(xInputField.text == "")
			{
				xVal = 0.1f;
				xSlider.value = xVal;
			}
			if(yInputField.text == "")
			{
				yVal = 0.1f;
				ySlider.value = yVal;
			}
		} */
	}
	
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        ApplyScaleChange();
    }
}
