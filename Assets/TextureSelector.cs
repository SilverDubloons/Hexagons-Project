using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextureSelector : MonoBehaviour
{
    public LevelEditor levelEditor;
	public float texturePreviewSize;	// size of each individial texture, both x and y
	public float borderSize;			// pixels between textures and border
	public int numberOfPreviewsWide;	// number of textures wide
	public float previewHeight;			// height of the window	
	public RectTransform windowRect;
	public GameObject texturePreviewPrefab;
	public Transform contentParent;
	public bool allTextures;
	public int rendererIndex;
	
	public void SetupTextureSelector(bool allTex, int renIndex)
	{
		allTextures = allTex;
		rendererIndex = renIndex;
		this.gameObject.SetActive(true);
	}
	
	public void TextureSelected(int index)
	{
		for(int i = 0; i < levelEditor.currentSelectionTransforms.Count; i++)
		{
			EditorData editorData = levelEditor.currentSelectionTransforms[i].GetComponent<EditorData>();
			if(allTextures)
			{
				for(int j = 0; j < editorData.renderers.Length; j++)
				{
					//editorData.renderers[j].material.SetTextureScale("_MainTex", new Vector2(3,3));
					editorData.textureIndices[j] = index;
					editorData.renderers[j].material.mainTexture = levelEditor.wallTextures[index];
					levelEditor.UpdateColor(editorData);
				}
			}
			else
			{
				//editorData.renderers[rendererIndex].material.SetTextureScale("_MainTex", new Vector2(3,3));
				editorData.textureIndices[rendererIndex] = index;
				editorData.renderers[rendererIndex].material.mainTexture = levelEditor.wallTextures[index];
				levelEditor.UpdateColor(editorData);
			}
		}
		this.gameObject.SetActive(false);
	}
	
    void Start()
    {
		RebuildWindow();
    }
	
	void RebuildWindow()
	{
		Vector2 windowSize = new Vector2 ((numberOfPreviewsWide*texturePreviewSize) + ((numberOfPreviewsWide+1)*borderSize) + 20, previewHeight); // 20 for the scroll bar
		windowRect.sizeDelta = windowSize;
		
		float contentHeight = (levelEditor.wallTextures.Length / numberOfPreviewsWide + 1) * texturePreviewSize + (levelEditor.wallTextures.Length / numberOfPreviewsWide + 1) * borderSize + borderSize;
		contentParent.GetComponent<RectTransform>().sizeDelta = new Vector2 (0, contentHeight);
        for(int i = 0; i < levelEditor.wallTextures.Length; i++)
		{
			GameObject newTex = Instantiate(texturePreviewPrefab, new Vector3(0,0,0), Quaternion.identity);
			newTex.transform.SetParent(contentParent);
			TexturePreview texturePreview = newTex.GetComponent<TexturePreview>();
			texturePreview.index = i;
			texturePreview.textureSelector = this;
			RectTransform rt = newTex.GetComponent<RectTransform>();
			rt.localScale = new Vector3(1,1,1);
			float x = texturePreviewSize * (i % numberOfPreviewsWide) + borderSize*(i % numberOfPreviewsWide) - windowSize.x/2 + texturePreviewSize/2 + borderSize*2;
			float y = -texturePreviewSize * (i / numberOfPreviewsWide) - borderSize*(i/numberOfPreviewsWide) + contentHeight/2 - texturePreviewSize/2 - borderSize;
			rt.anchoredPosition = new Vector2(x,y);
			rt.sizeDelta = new Vector2(texturePreviewSize,texturePreviewSize);
			newTex.GetComponent<UnityEngine.UI.RawImage>().texture = levelEditor.wallTextures[i];
		}
	}

    void Update()
    {
        
    }
}
