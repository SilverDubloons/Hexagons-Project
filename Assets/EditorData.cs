using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorData : MonoBehaviour
{
	public Vector3 baseHex;
	public Vector3[] relativeHexesToBlock; //These are the hexes that will be blocked relative to the baseHex. Put them in as non-int Vector3's
	public Vector3 placementOffset;
	public int objectIndex; // for editorObjects in LevelEditor
	public int[] textureIndices;
	public Vector2[] textureScales;
	public Renderer[] renderers;
	public Vector3 copyRelativeVector;
	public Vector3 copyRelativeBaseHex;
	
    void Awake()
    {
        textureIndices = new int[renderers.Length];
		textureScales = new Vector2[renderers.Length];
		for(int i = 0; i < textureScales.Length; i++)
		{
			textureScales[i] = new Vector2(1,1);
		}
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
