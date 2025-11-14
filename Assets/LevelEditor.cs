using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;
using UnityEngine.EventSystems;

/*

	*** IDEAS ***
	support pillars, usually in observed areas, but easier to destroy (think needing a sledgehammer vs explosives). When destroyed, all tiles above them (use a collider to find them) take damage.

*/

public class LevelEditor : MonoBehaviour
{
	[System.Serializable]
	public class EditorObject
	{
		public GameObject prefab;
		public Sprite icon;
		public string name;
		public EditorObject(GameObject prefab, Sprite icon, string name)
		{
			this.prefab = prefab;
			this.icon = icon;
			this.name = name;
		}
	}
	
	public EditorObject[] editorObjects;
	public GameObject editorButton;
	public Transform buttonsParent;
	public Vector3 firstButtonLocation;
	public WalkableArea walkableArea;
	private int curIndex;
	private GameObject curPrefab;
	public List<EditorData> placedObjectsData;
	public TMP_InputField filenameInput;
	public string currentFileManagerVersion;
	public Texture[] wallTextures;
	public bool objectsTransparent;
	public Color colorBaseTransparent;
	public Color colorBase;
	public Color colorWhiteTransparent;
	public Color selectedColorTransparent;
	public Color selectedColor;
	public TMP_Text mouseDataText;
	public Vector2 canvasReferenceResolution;
	public RectTransform soloSelectionIndicator;
	public int soloSelectionIndex;
	
	void SetupButtons()
	{
		for(int i = 0;i <editorObjects.Length; i++)
		{
			GameObject newButton = Instantiate(editorButton,new Vector3(0,0,0), Quaternion.identity);
			newButton.transform.SetParent(buttonsParent);
			newButton.transform.localPosition = firstButtonLocation + new Vector3(i*90,0,0);
			newButton.GetComponent<EditorButton>().index = i;
			newButton.GetComponent<EditorButton>().levelEditor = this;
			newButton.transform.localScale = new Vector3(1,1,1);
			newButton.transform.GetChild(0).GetComponent<Image>().sprite = editorObjects[i].icon;
		}
	}
	
	public void EditorButtonRightClicked(int index)
	{
		soloSelectionIndex = index;
	}
	
	public void EditorButtonClicked(int index)
	{
		if(curPrefab != null)
		{
			Destroy(curPrefab);
		}
		soloSelectionIndex = -1;
		soloSelectionIndicator.gameObject.SetActive(false);
		if(copyEditorDatas.Count>0)
		{
			ClearSelection();
			for(int i = 0; i <copyEditorDatas.Count; i++)
			{
				Destroy(copyEditorDatas[i].gameObject);
			}
			copyEditorDatas.Clear();
		}
		if(index>=0)
		{
			GameObject newPrefab = Instantiate(editorObjects[index].prefab, new Vector3(0,0,0), editorObjects[index].prefab.transform.rotation);
			curPrefab = newPrefab;
		}
		curIndex = index;
	}
	
    void Start()
    {
		soloSelectionIndex = -1;
		curIndex = -1;
        SetupButtons();
		LoadLevel("kl");
    }
	
	void MoveAndPlacePrefab()
	{
		if(curPrefab != null)
		{
			Vector3 mousePos = walkableArea.GetMousePositionVector3();
			if(!walkableArea.IsInvalidVector3(mousePos))
			{
				curPrefab.transform.position = mousePos;
				curPrefab.transform.position += curPrefab.GetComponent<EditorData>().placementOffset;
				if(Input.GetMouseButtonDown(0))
				{
					EditorData editorData = curPrefab.GetComponent<EditorData>();
					editorData.baseHex = mousePos;
					placedObjectsData.Add(editorData);
					UpdateColor(editorData);
					
					GameObject newPrefab = Instantiate(editorObjects[curIndex].prefab, new Vector3(0,0,0), editorObjects[curIndex].prefab.transform.rotation);
					curPrefab = newPrefab;
					EditorData newEditorData = newPrefab.GetComponent<EditorData>();
					editorData.objectIndex = curIndex;
					UpdateColor(newEditorData);
					RecalculatePathing();
					DeleteContextMenus();
				}
			}
		}
	}
	
	public List<Transform> currentSelectionTransforms;
	
	public void UpdateColor(EditorData editorData)
	{
		for(int j = 0; j < editorData.renderers.Length; j++)
		{
			Material newMaterial = new Material(editorData.renderers[j].material);
			if(objectsTransparent)
			{
				if(editorData.textureIndices[j] == 0)
				{
					newMaterial.color = colorBaseTransparent;
				}
				else
				{
					newMaterial.color = colorWhiteTransparent;
				}
			}
			else
			{
				if(editorData.textureIndices[j] == 0)
				{
					newMaterial.color = colorBase;
				}
				else
				{
					newMaterial.color = Color.white;
				}
				
			}
			if(currentSelectionTransforms.Contains(editorData.transform))
			{
				if(objectsTransparent)
				{
					newMaterial.color =selectedColorTransparent;
				}
				else
				{
					newMaterial.color = selectedColor;
				}
			}
			editorData.renderers[j].material = newMaterial;
		}
	}
	
	void ClearSelection()
	{
		for (int i = currentSelectionTransforms.Count - 1; i >= 0; i--)
		{
			EditorData editorData = currentSelectionTransforms[i].GetComponent<EditorData>();
			currentSelectionTransforms.Remove(currentSelectionTransforms[i]);
			UpdateColor(editorData);
		}
		currentSelectionTransforms.Clear();
		textureSelector.gameObject.SetActive(false);
		textureScaleSelector.gameObject.SetActive(false);
	}
	
	void RecalculatePathing()
	{
		walkableArea.hexGrid.ClearGrid();
		for(int i = 0;i < placedObjectsData.Count; i++)
		{
			for(int j = 0; j < placedObjectsData[i].relativeHexesToBlock.Length; j++)
			{
				walkableArea.hexGrid.MarkBlocked(walkableArea.Vector3ToCoordinate(placedObjectsData[i].baseHex+placedObjectsData[i].relativeHexesToBlock[j]),true);
			}
		}
	}
	
	void SaveLevel(string fileName)
	{
		string path = Application.persistentDataPath+"/"+fileName+".txt";
		if(File.Exists(path))
		{
			File.WriteAllText(path, "");
		}
		StreamWriter writer = new StreamWriter(path, true);
		writer.WriteLine(currentFileManagerVersion);
		for(int i = 0; i < placedObjectsData.Count; i++)
		{
			string obj = "";
			obj += placedObjectsData[i].objectIndex+",";
			obj += placedObjectsData[i].baseHex.x+","+placedObjectsData[i].baseHex.y+","+placedObjectsData[i].baseHex.z+",";
			obj += placedObjectsData[i].renderers.Length+",";
			for(int j = 0; j < placedObjectsData[i].renderers.Length; j++)
			{
				obj += placedObjectsData[i].textureIndices[j]+",";
				obj += placedObjectsData[i].textureScales[j].x+","+placedObjectsData[i].textureScales[j].y+",";
			}
			writer.WriteLine(obj);
		}
		writer.Close();
	}
	
	void DeleteAllplacedObjectsData()
	{
		for(int i = 0; i < placedObjectsData.Count; i++)
		{
			Destroy(placedObjectsData[i].gameObject);
		}
		placedObjectsData.Clear();
	}
	
	void LoadLevel(string fileName)
	{
		string path = Application.persistentDataPath+"/"+fileName+".txt";
		if(!File.Exists(path))
		{
			Debug.Log(path+ " not found");
			return;
		}
		ClearSelection();
		DeleteAllplacedObjectsData();
		StreamReader reader = new StreamReader(path);
		string levelData = reader.ReadToEnd();
		string[] lines = levelData.Split('\n');
		string fileManagerVersion = lines[0].Trim();
		if(fileManagerVersion == currentFileManagerVersion)
		{
			for(int i = 1; i < lines.Length-1; i++)
			{
				string[] values = lines[i].Split(',');
				int index = int.Parse(values[0]);
				Vector3 spawnLocation = new Vector3(float.Parse(values[1]),float.Parse(values[2]),float.Parse(values[3]));
				
				GameObject newPrefab = Instantiate(editorObjects[index].prefab, spawnLocation, editorObjects[index].prefab.transform.rotation);
				EditorData editorData = newPrefab.GetComponent<EditorData>();
				editorData.objectIndex = index;
				newPrefab.transform.position += editorData.placementOffset;
				editorData.baseHex = spawnLocation;
				for(int j = 0; j < int.Parse(values[4]); j++)
				{
					editorData.textureIndices[j] = int.Parse(values[5+3*j]);
					editorData.textureScales[j] = new Vector2(float.Parse(values[5+3*j+1]),float.Parse(values[5+3*j+2]));
					editorData.renderers[j].material.mainTexture = wallTextures[editorData.textureIndices[j]];
					editorData.renderers[j].material.SetTextureScale("_MainTex", editorData.textureScales[j]);
				}
				UpdateColor(editorData);
				placedObjectsData.Add(editorData);
			}
		}
		else
		{
			Debug.Log("Trying to load a version \"" + lines[0] + "\" level. Your version is \"" + currentFileManagerVersion + "\"");
		}
		
		RecalculatePathing();
		reader.Close();
	}
	
	void CycleMaterials()
	{
		for(int i = 0; i < currentSelectionTransforms.Count; i++)
		{
			EditorData editorData = currentSelectionTransforms[i].GetComponent<EditorData>();
			for(int j = 0; j < editorData.renderers.Length; j++)
			{
				int nextIndex = editorData.textureIndices[j];
				nextIndex++;
				if(nextIndex >= wallTextures.Length)
				{
					nextIndex = 0;
					Color newColor = editorData.renderers[j].material.color;
					//newColor = editorData.originalColors[j];
					if(objectsTransparent)
					{
						editorData.renderers[j].material.color = selectedColorTransparent;
					}
					else
					{
						editorData.renderers[j].material.color = selectedColor;
					}
					
					
				}
				else
				{
					Color newColor = editorData.renderers[j].material.color;
					newColor = Color.white;
					editorData.renderers[j].material.color = newColor;
				}
				//editorData.renderers[j].material.SetTextureScale("_MainTex", new Vector2(5,5));
				editorData.textureIndices[j] = nextIndex;
				editorData.renderers[j].material.mainTexture = wallTextures[nextIndex];
			}
		}
	}
	
	public GameObject contextMenuPrefab;
	public Transform contextMenuParent;
	
	void DeleteContextMenus()
	{
		for(int i = 0; i < contextMenuParent.childCount; i++)
		{
			Destroy(contextMenuParent.GetChild(i).gameObject);
		}
	}
	
	void BringUpContextMenu()
	{
		DeleteContextMenus();
		if (!EventSystem.current.IsPointerOverGameObject())
		{
			if(currentSelectionTransforms.Count>0)
			{
				int minRenderers = 9001;
				for(int i = 0; i < currentSelectionTransforms.Count; i++)
				{
					EditorData editorData = currentSelectionTransforms[i].GetComponent<EditorData>();
					if(editorData.renderers.Length < minRenderers)
					{
						minRenderers = editorData.renderers.Length;
					}
				}
				string[] rendererLabels = new string[]
				{
					"Front",
					"Back",
					"Sides",
					"Top and Bottom"
				};
				string[] menuLabels = new string[]
				{
					"Change Texture",
					"Texture Scale"
				};
				for(int j = 0; j < menuLabels.Length; j++)
				{
					GameObject newMenu = Instantiate(contextMenuPrefab, new Vector3(0,0,0), Quaternion.identity);
					newMenu.transform.SetParent(contextMenuParent);
					RectTransform rt = newMenu.GetComponent<RectTransform>();
					rt.localScale = new Vector3(1,1,1);
					Vector2 normalizedMousePos = new Vector2(Input.mousePosition.x/Screen.width,Input.mousePosition.y/Screen.height);
					//rt.anchoredPosition = new Vector2(normalizedMousePos.x * canvasReferenceResolution.x - canvasReferenceResolution.x/2,normalizedMousePos.y * canvasReferenceResolution.y - canvasReferenceResolution.y/2 - 40*j);
					rt.anchoredPosition = new Vector2(Input.mousePosition.x,Input.mousePosition.y - 40*j);
					ContextMenuObject cmo = newMenu.GetComponent<ContextMenuObject>();
					cmo.SetupContextMenu(menuLabels[j],"",true,false);
					
					GameObject newAll = Instantiate(contextMenuPrefab, new Vector3(0,0,0), Quaternion.identity);
					newAll.transform.SetParent(newMenu.transform);
					RectTransform rta = newAll.GetComponent<RectTransform>();
					rta.localScale = new Vector3(1,1,1);
					rta.anchoredPosition = new Vector2(340,0);
					ContextMenuObject cmoa = newAll.GetComponent<ContextMenuObject>();
					cmoa.SetupContextMenu("All","",false,true);
					cmo.childMenus.Add(cmoa);
					
					for(int k = 0; k < minRenderers; k++)
					{
						GameObject newNumber = Instantiate(contextMenuPrefab, new Vector3(0,0,0), Quaternion.identity);
						newNumber.transform.SetParent(newMenu.transform);
						RectTransform rtn = newNumber.GetComponent<RectTransform>();
						rtn.localScale = new Vector3(1,1,1);
						rtn.anchoredPosition = new Vector2(340,-40-40*k);
						ContextMenuObject cmon = newNumber.GetComponent<ContextMenuObject>();
						cmon.SetupContextMenu("" + k + " " + rendererLabels[k],"",false,true);
						cmo.childMenus.Add(cmon);
					}
					cmo.SetActiveChildren(false);
				}
			}
		}
	}
	
	public TextureSelector textureSelector;
	public TextureScaleSelector textureScaleSelector;
	
	public void ContextMenuClicked(string inputA, string inputB)
	{
		//print("context menu was clicked. inputA= "+inputA+" inputB= "+inputB);
		if(inputA == "Change Texture")
		{
			if(inputB == "All")
			{
				textureSelector.SetupTextureSelector(true,-1);
			}
			else
			{
				textureSelector.SetupTextureSelector(false,int.Parse(inputB[0].ToString()));
			}
		}
		if(inputA == "Texture Scale")
		{
			if(inputB == "All")
			{
				textureScaleSelector.SetupTextureScaleSelector(true,-1,currentSelectionTransforms[0].GetComponent<EditorData>().renderers[0].material.mainTextureScale);
			}
			else
			{
				textureScaleSelector.SetupTextureScaleSelector(false,int.Parse(inputB[0].ToString()),currentSelectionTransforms[0].GetComponent<EditorData>().renderers[int.Parse(inputB[0].ToString())].material.mainTextureScale);
			}
		}
		DeleteContextMenus();
	}
	
	void ToggleTransparency()
	{
		for(int i = 0; i < placedObjectsData.Count; i++)
		{
			EditorData editorData = placedObjectsData[i].GetComponent<EditorData>();
			for(int j = 0; j < editorData.renderers.Length; j++)
			{
				Material newMaterial = new Material(editorData.renderers[j].material);
				Color newColor = editorData.renderers[j].material.color;
				if(objectsTransparent)
				{
					newColor.a = 1f;
				}
				else
				{
					newColor.a = 0.7f;
				}
				newMaterial.color = newColor;
				editorData.renderers[j].material = newMaterial;
			}
		}
		objectsTransparent=!objectsTransparent;
		if(curIndex >= 0)
		{
			EditorData editorData = curPrefab.GetComponent<EditorData>();
			UpdateColor(editorData);
		}
		
	}
	
	void CopySelection()
	{
		copyEditorDatas.Clear();
		if(currentSelectionTransforms.Count == 1)
		{
			EditorButtonClicked(currentSelectionTransforms[0].GetComponent<EditorData>().objectIndex);
		}
		if(currentSelectionTransforms.Count > 1)
		{
			for(int i = 0; i < currentSelectionTransforms.Count; i++)
			{
				EditorData editorData = currentSelectionTransforms[i].GetComponent<EditorData>();
				GameObject copyObject = Instantiate(editorObjects[editorData.objectIndex].prefab,currentSelectionTransforms[i].localPosition,editorObjects[editorData.objectIndex].prefab.transform.rotation);
				EditorData copyData = copyObject.GetComponent<EditorData>();
				copyData.textureScales = editorData.textureScales;
				copyData.textureIndices = editorData.textureIndices;
				copyData.objectIndex = editorData.objectIndex;
				copyData.baseHex = editorData.baseHex;
				for(int j = 0; j < editorData.renderers.Length; j++)
				{
					copyData.renderers[j].material.mainTexture = wallTextures[copyData.textureIndices[j]];
					copyData.renderers[j].material.SetTextureScale("_MainTex", copyData.textureScales[j]);
				}
				copyEditorDatas.Add(copyObject.GetComponent<EditorData>());
			}
			ClearSelection();
			// Find the most central transform to be the relative position others are judged by.
			Vector3 averagePosition = Vector3.zero;
			for(int k = 0; k < copyEditorDatas.Count; k++)
			{
				averagePosition += copyEditorDatas[k].transform.localPosition;
			}
			averagePosition /= copyEditorDatas.Count;
			Transform mostCentralTransform = null;
			float minDistance = float.MaxValue;
			for(int l = 0 ; l <copyEditorDatas.Count; l++)
			{
				float distance = Vector3.Distance(copyEditorDatas[l].transform.localPosition, averagePosition);
				if(distance < minDistance)
				{
					minDistance = distance;
					mostCentralTransform = copyEditorDatas[l].transform;
				}
			}
			if(mostCentralTransform != null && mostCentralTransform != copyEditorDatas[0].transform)
			{
				EditorData mostCentralEditorData = mostCentralTransform.GetComponent<EditorData>();
				int indexOfMostCentral = copyEditorDatas.IndexOf(mostCentralEditorData);
				EditorData tempData = copyEditorDatas[0];
				copyEditorDatas[0] = mostCentralEditorData;
				copyEditorDatas[indexOfMostCentral] = tempData;
			}
			for(int m = 1; m < copyEditorDatas.Count; m++)
			{
				copyEditorDatas[m].copyRelativeVector = copyEditorDatas[m].transform.localPosition - copyEditorDatas[0].transform.localPosition;
				copyEditorDatas[m].copyRelativeBaseHex = copyEditorDatas[m].baseHex - copyEditorDatas[0].baseHex;
				currentSelectionTransforms.Add(copyEditorDatas[m].transform);
				UpdateColor(copyEditorDatas[m]);
			}
			currentSelectionTransforms.Add(copyEditorDatas[0].transform);
			UpdateColor(copyEditorDatas[0]);
			curIndex = -2;
		}
	}
	
	private float mouseHoldTime;
	private bool boxSelecting;
	public RectTransform boxSelector;
	private Vector3 boxSelectionAnchor;
	public List<EditorData> copyEditorDatas;
	
	void CheckInput()
	{
		if(Input.GetKeyDown(KeyCode.Space))
		{
			EditorButtonClicked(-1);
		}
		if(Input.GetKeyDown(KeyCode.Alpha1))
		{
			EditorButtonClicked(0);
		}
		if(Input.GetKeyDown(KeyCode.Alpha2))
		{
			EditorButtonClicked(1);
		}
		if(Input.GetKeyDown(KeyCode.Alpha3))
		{
			EditorButtonClicked(2);
		}
		if(Input.GetKeyDown(KeyCode.Alpha4))
		{
			EditorButtonClicked(3);
		}
		if(Input.GetKeyDown(KeyCode.Alpha5))
		{
			EditorButtonClicked(4);
		}
		if(Input.GetKeyDown(KeyCode.C) && Input.GetKey(KeyCode.LeftControl))
		{
			CopySelection();
		}
		if(Input.GetKeyDown(KeyCode.F1))
		{
			SaveLevel(filenameInput.text);
		}
		if(Input.GetKeyDown(KeyCode.F2))
		{
			LoadLevel(filenameInput.text);
		}
		if(Input.GetKeyDown(KeyCode.Z))
		{
			CycleMaterials();
		}
		if(Input.GetMouseButtonDown(1))
		{
			BringUpContextMenu();
		}
		if(Input.GetKeyDown(KeyCode.T))
		{
			ToggleTransparency();
		}
	}
	
	void SelectObjects()
	{
		if(Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
		{
			mouseHoldTime+=Time.deltaTime;
			if(mouseHoldTime >= 0.0f && !boxSelecting)
			{
				boxSelecting = true;
				boxSelector.gameObject.SetActive(true);
				boxSelectionAnchor = new Vector2(Input.mousePosition.x/Screen.width,Input.mousePosition.y/Screen.height);
			}
		}
		
		if(boxSelecting)
		{
			Vector2 normalizedMousePos = new Vector2(Input.mousePosition.x/Screen.width,Input.mousePosition.y/Screen.height);
			//Vector2 boxPos = new Vector2(boxSelectionAnchor.x * canvasReferenceResolution.x, boxSelectionAnchor.y * canvasReferenceResolution.y);
			Vector2 boxPos = new Vector2(boxSelectionAnchor.x * Screen.width, boxSelectionAnchor.y * Screen.height);
			//Vector2 boxSize = new Vector2((normalizedMousePos.x - boxSelectionAnchor.x)*canvasReferenceResolution.x,(normalizedMousePos.y - boxSelectionAnchor.y)*canvasReferenceResolution.y);
			Vector2 boxSize = new Vector2((normalizedMousePos.x - boxSelectionAnchor.x)*Screen.width,(normalizedMousePos.y - boxSelectionAnchor.y)*Screen.height);
			if(boxSize.x < 0)
			{
				boxPos.x += boxSize.x;
				boxSize.x = Mathf.Abs(boxSize.x);
			}
			if(boxSize.y < 0)
			{
				boxPos.y += boxSize.y;
				boxSize.y = Mathf.Abs(boxSize.y);
			}
			boxSelector.sizeDelta = boxSize;
			boxSelector.anchoredPosition = boxPos;
			if(Input.GetMouseButtonUp(0))
			{
				mouseHoldTime = 0;
				if(boxSelecting)
				{
					Vector2 bottomLeft = boxPos;
					Vector2 topRight = boxPos + boxSize;
					if(Input.GetKey(KeyCode.LeftShift)||Input.GetKey(KeyCode.RightShift))
					{
						
					}
					else
					{
						ClearSelection();
					}
					for(int i = 0; i < placedObjectsData.Count; i++)
					{
						if(soloSelectionIndex == -1 || soloSelectionIndex == placedObjectsData[i].objectIndex)
						{
							Vector3 objectWorldPos = placedObjectsData[i].transform.position;
							Vector3 objectScreenPos = Camera.main.WorldToScreenPoint(objectWorldPos);
							//Vector3 objectScreenPosNormalized = new Vector3(objectScreenPos.x/Screen.width*canvasReferenceResolution.x,objectScreenPos.y/Screen.height*canvasReferenceResolution.y,objectScreenPos.z);
							Vector3 objectScreenPosNormalized = new Vector3(objectScreenPos.x,objectScreenPos.y,objectScreenPos.z);
							if(objectScreenPosNormalized.x >= bottomLeft.x && objectScreenPosNormalized.x <= topRight.x && objectScreenPosNormalized.y >= bottomLeft.y && objectScreenPosNormalized.y <= topRight.y)
							{
								currentSelectionTransforms.Add(placedObjectsData[i].transform);
								EditorData editorData = placedObjectsData[i].GetComponent<EditorData>();
								UpdateColor(editorData);
							}
							//mouseDataText.text = "" + bottomLeft + "\n" + topRight + "\n" + objectScreenPosNormalized;
						}
					}
					boxSelecting = false;
					boxSelector.gameObject.SetActive(false);
				}
			}
		}
		if(Input.GetMouseButtonUp(0))	//this causes problems
		{
			/* if (!EventSystem.current.IsPointerOverGameObject()) // doesn't ignore things tagged with "ignore raycast"
			{ */													// like my cursor
				DeleteContextMenus();
			
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;
				if (Physics.Raycast(ray, out hit))
				{
					/* if (hit.collider.gameObject.layer == LayerMask.NameToLayer("UI"))
					{
						print("hit UI");	// this never seems to fire, but also the UI is blocking the raycast?
						return;				// whatever, it works.
					} */
					if(hit.collider.gameObject==walkableArea.gameObject)
					{
						//ClearSelection();
					}
					else
					{
						bool deselecting = false;
						if(Input.GetKey(KeyCode.LeftShift)||Input.GetKey(KeyCode.RightShift))
						{
							if(currentSelectionTransforms.Contains(hit.collider.transform))
							{
								deselecting=true;
								currentSelectionTransforms.Remove(hit.collider.transform);
							}
							else
							{
								currentSelectionTransforms.Add(hit.collider.transform);
							}
						}
						else
						{
							ClearSelection();
							currentSelectionTransforms.Add(hit.collider.transform);
						}
						EditorData editorData = hit.collider.GetComponent<EditorData>();
						for(int i = 0; i < editorData.renderers.Length; i++)
						{
							if(deselecting)
							{
								UpdateColor(editorData);
							}
							else
							{
								Material newMaterial = new Material(editorData.renderers[i].material);
								if(objectsTransparent)
								{
									newMaterial.color =selectedColorTransparent;
								}
								else
								{
									newMaterial.color = selectedColor;
								}
								editorData.renderers[i].material = newMaterial;
							}
						}
					}
				}
			/* }
			else
			{
				print("mouse cursor over ui");
			} */
		}
		if(Input.GetKeyDown(KeyCode.Delete))
		{
			for (int i = 0; i < currentSelectionTransforms.Count; i++)
			{
				EditorData editorData = currentSelectionTransforms[i].GetComponent<EditorData>();
				if(editorData != null)
				{
					placedObjectsData.Remove(editorData);
				}
				Destroy(currentSelectionTransforms[i].gameObject);
			}
			ClearSelection();
			RecalculatePathing();
			DeleteContextMenus();
		}
	}
	
	void MoveAndPlaceCopies()
	{
		Vector3 mousePos = walkableArea.GetMousePositionVector3();
		if(!walkableArea.IsInvalidVector3(mousePos))
		{
			for(int k = 0; k < copyEditorDatas.Count; k++)
			{
				copyEditorDatas[k].transform.position = mousePos;
				copyEditorDatas[k].transform.position += copyEditorDatas[0].placementOffset;
				copyEditorDatas[k].transform.position += copyEditorDatas[k].copyRelativeVector;
			}
			if(Input.GetMouseButtonDown(0))
			{
				ClearSelection();
				List<EditorData> newCopies = new List<EditorData>();
				for(int i = 0; i < copyEditorDatas.Count; i++)
				{
					//copyEditorDatas[i].baseHex = mousePos + copyEditorDatas[i].copyRelativeVector - copyEditorDatas[i].placementOffset;
					copyEditorDatas[i].baseHex = mousePos + copyEditorDatas[i].copyRelativeBaseHex;
					placedObjectsData.Add(copyEditorDatas[i]);
					UpdateColor(copyEditorDatas[i]);
					
					GameObject newPrefab = Instantiate(editorObjects[copyEditorDatas[i].objectIndex].prefab, copyEditorDatas[i].transform.localPosition, copyEditorDatas[i].transform.localRotation);
					
					EditorData newEditorData = newPrefab.GetComponent<EditorData>();
					newEditorData.objectIndex = copyEditorDatas[i].objectIndex;
					newEditorData.textureIndices = copyEditorDatas[i].textureIndices;
					newEditorData.textureScales = copyEditorDatas[i].textureScales;
					newEditorData.copyRelativeVector = copyEditorDatas[i].copyRelativeVector;
					newEditorData.baseHex = copyEditorDatas[i].baseHex;
					newEditorData.copyRelativeBaseHex = copyEditorDatas[i].copyRelativeBaseHex;
					currentSelectionTransforms.Add(newPrefab.transform);
					for(int j = 0; j < newEditorData.renderers.Length; j++)
					{
						newEditorData.renderers[j].material.mainTexture = wallTextures[copyEditorDatas[i].textureIndices[j]];
						newEditorData.renderers[j].material.SetTextureScale("_MainTex", copyEditorDatas[i].textureScales[j]);
					}
					UpdateColor(newEditorData);
					newCopies.Add(newEditorData);
				}
				copyEditorDatas.Clear();
				copyEditorDatas = newCopies;
				RecalculatePathing();
				DeleteContextMenus();
			}
		}
	}
	
    void Update()
    {
		CheckInput();
		
		if(curIndex == -2)
		{
			MoveAndPlaceCopies();
		}
		if(curIndex == -1)
		{
			SelectObjects();
		}
		else
		{
			MoveAndPlacePrefab();
		}
    }
}