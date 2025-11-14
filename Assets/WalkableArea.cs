using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Diagnostics;


/*

	Here's some important math to remember!
	
	The angle from one hexagon to a non-horizontally adjacent hexagon is:
	arctan(y_diff / x_diff)
	arctan(1 / 0.5)
	arctan(2)
	= 63.434948822922010648427806279547 degrees
	
	The distance between these two hexagons is
	Distance = √((x2 - x1)^2 + (y2 - y1)^2)
	The two points are (0,0) and (0.5,1), so:

	x1 = 0
	y1 = 0
	x2 = 0.5
	y2 = 1
	
	Distance = √((0.5 - 0)^2 + (1 - 0)^2)
	Distance = √((0.5)^2 + 1^2)
	Distance = √(0.25 + 1)
	Distance = √1.25
	Distance = 1.1180339887498948482045868343656
	
	Using the same method, the angle from one hexagon to one two away, offset on the z axis by one is
	arctan(1 / 1.5)
	= 33.69006752597978691352549456166 degrees
	
	and the distance between these is 
	Distance = √3.25
	Distance = 1.8027756377319946465596106337352
	
	Standard door is 2m tall, .9m wide. Standard walls are 2.5m high. Walls are .2m thick.
	
	
	180-33.69006752597978691352549456166-63.434948822922010648427806279547
	=82.874983651098202438046699158793
*/

public class WalkableArea : MonoBehaviour
{
	public static readonly Vector3 invalidVector3 = new Vector3(float.NaN, float.NaN, float.NaN);
    public Vector2 gridSize;
	public Transform movementHex;
	public GameObject hexPrefab;
	public Transform player;
	public Transform playerModel;
	public float playerSpeed;
	public TMP_Text dataText;
	public TMP_Text pathfindingText;
	public HexGrid hexGrid = new HexGrid();
	public Vector3[] wallPairs;
	public Vector3[] doors;
	public GameObject levelEditor;
	private Stopwatch stopwatch;
	public Transform hexParent;
	private UIData uiData;
	
	public class HexGrid
	{
		private Dictionary<Vector3Int, bool> grid = new Dictionary<Vector3Int,bool>();
		public void MarkBlocked(Vector3Int coordinates, bool blocked)
		{
			grid[coordinates] = blocked;  // Mark a cell as blocked
		}

		public bool IsBlocked(Vector3Int coordinates)
		{
			return grid.TryGetValue(coordinates, out bool isBlocked) && isBlocked;
		}
		
		public void ClearGrid()
		{
			grid.Clear();
		}
	}
	
    void Start()
    {
		uiData = GameObject.FindWithTag("UIData").GetComponent<UIData>();
    }
	
	public bool IsInvalidVector3(Vector3 vector)
	{
		return float.IsNaN(vector.x) || float.IsNaN(vector.y) || float.IsNaN(vector.z);
	}
	
	void Update()
    {
		if(Camera.main.depth != -1)
		{
			return;
		}
		Vector3 hexPoint = GetMousePositionVector3();
		if(!IsInvalidVector3(hexPoint))
		{
			Vector3Int hexPointInt = Vector3ToCoordinate(hexPoint);
			dataText.text = hexPoint+"\n"+hexPointInt;
			Vector3 origin = movementHex.localPosition;
			if(origin != hexPoint)
			{
				movementHex.localPosition = hexPoint;
				if(hexGrid.IsBlocked(hexPointInt))
				{
					movementHex.GetChild(0).gameObject.SetActive(true);
				}
				else
				{
					movementHex.GetChild(0).gameObject.SetActive(false);
				}
			}
			if(Input.GetMouseButtonDown(0))
			{
				if(player.gameObject.activeSelf)
				{
					if(uiData.mouseCursor.currentCursor == 0)	// not the best way to handle this
					{
						MoveAlongPath(player,hexPoint,hexPointInt);
					}
				}
			}
		}
		//return; // I wonder how long it will take me to find this
		if(Input.GetKeyDown(KeyCode.Q))
		{
			player.gameObject.SetActive(!player.gameObject.activeSelf);
		}
		if(Input.GetKeyDown(KeyCode.E))
		{
			levelEditor.SetActive(!levelEditor.activeSelf);
		}
		if(Input.GetKeyDown(KeyCode.R))
		{
			/* for(int i = hexParent.childCount - 1; i >=0; i--)
			{
				Destroy(hexParent.GetChild(i).gameObject);
			} */
			for(int i = 0; i < hexParent.childCount; i++)
			{
				Destroy(hexParent.GetChild(i).gameObject);
			}
		}
		
	}
	
	public void MoveAlongPath(Transform character, Vector3 hexPoint, Vector3Int hexPointInt)
	{
		if(!hexGrid.IsBlocked(hexPointInt))
		{
			List<Vector3> path = FindPath(character.localPosition,hexPoint);
			/* Debug.Log("Path found from " + character.localPosition + " to " + hexPoint);
			for(int i = 0; i < path.Count; i++)
			{
				Debug.Log("i= "+i+" path= "+ path[i]);
			} */
			StopAllCoroutines();
			StartCoroutine(MovePlayer(path));
		}
	}
	
	public Vector3 GetMousePositionVector3()
	{
		if (!EventSystem.current.IsPointerOverGameObject())
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			//RaycastHit hit;
			RaycastHit[] hits = Physics.RaycastAll(ray);
			foreach (RaycastHit hit in hits)
			{
				if (hit.collider.gameObject == this.gameObject)
				{
					Vector3 mousePoint = hit.point;

					float closestZ = Mathf.Round(mousePoint.z);
					float closestX = 0;
					if(closestZ % 2 == 0)
					{
						closestX = Mathf.Round(mousePoint.x);
					}
					else
					{
						closestX = Mathf.Floor(mousePoint.x) + 0.5f;
					}
					
					Vector3 hexPoint = new Vector3(closestX, mousePoint.y, closestZ);
					return hexPoint;
				}
			}
		}
		return invalidVector3;
	}
	
	public IEnumerator MovePlayer(List <Vector3> path)
	{
		for(int i = 0; i < path.Count-1; i++)
		{
			float t = 0;
			while(t < 1)
			{
				t+=Time.deltaTime*playerSpeed;
				//playerModel.localRotation.y
				player.localPosition = Vector3.Lerp(path[i],path[i+1],t);
				yield return null;
			}
		}
	}
	
	public class Node
	{
		public Vector3 location;
		public int x;
		public int y;
		public int z;
		public float g;
		public float h;
		public float f;
		public Node parent;
		public Node(Vector3 location, int x, int y, int z, float g, float h, float f, Node parent)
		{
			this.location = location;
			this.x = x;
			this.y = y;
			this.z = z;
			this.g = g;
			this.h = h;
			this.f = f;
			this.parent = parent;
		}
		public string GetNodeString()
		{
			if(parent!=null)
			{
				return "loc= "+location+" x= "+x+" y= "+y+" z= "+z+" g= "+g+" h= "+h+" f= " +f+" parentLoc= "+ parent.location;
			}
			else
			{
				return "loc= "+location+" x= "+x+" y= "+y+" z= "+z+" g= "+g+" h= "+h+" f= " +f+" no parent";
			}
		}
	}
	
	public class NodeComparer : IComparer<Node>
	{
		public int Compare(Node x, Node y)
		{
			return x.f.CompareTo(y.f);
		}
	}
	
	public Vector3Int Vector3ToCoordinate(Vector3 vec)
	{
		int y = Mathf.RoundToInt(vec.y);
		int z = Mathf.RoundToInt(vec.z);
		int x = 0;
		if(z % 2 == 0)
		{
			 x = Mathf.RoundToInt(vec.x);
		}
		else
		{
			x = Mathf.RoundToInt(vec.x + 0.5f);
		}
		return new Vector3Int(x,y,z);
	}
	
	public Vector3 CoordinateToVector3(int x, int y, int z)
	{
		float xPos = 0;
		if(z % 2 == 0)
		{
			xPos = x;
		}
		else
		{
			xPos = Mathf.FloorToInt(x) - 0.5f;
		}
		Vector3 pos = new Vector3(xPos,y,z);
		//Debug.Log("Coords ("+x+","+y+","+z+") = "+pos);
		return pos;
	}
	
	public Vector3Int GetAdjacentHex(Vector3Int baseHex, int hexCase) 
	{ // 0 = NW, 1 = NE, 2 = W, 3 = E, 4 = SW, 5 = SE, 6 = same as baseHex		
		Vector3Int returnHex = new Vector3Int(0,0,0);
		returnHex.y = baseHex.y;
		bool evenZ = false;
		if(baseHex.z % 2 == 0)
		{
			evenZ = true;
		}
		switch(hexCase)
		{
			case 0:
			if(evenZ)
			{
				returnHex.x = baseHex.x;
			}
			else
			{
				returnHex.x = baseHex.x - 1;
			}
			returnHex.z = baseHex.z + 1;
			break;
			case 1:
			if(evenZ)
			{
				returnHex.x = baseHex.x + 1;
			}
			else
			{
				returnHex.x = baseHex.x;
			}
			returnHex.z = baseHex.z + 1;
			break;
			case 2:
			returnHex.x = baseHex.x - 1;
			returnHex.z = baseHex.z;
			break;
			case 3:
			returnHex.x = baseHex.x + 1;
			returnHex.z = baseHex.z;
			break;
			case 4:
			if(evenZ)
			{
				returnHex.x = baseHex.x;
			}
			else
			{
				returnHex.x = baseHex.x - 1;
			}
			returnHex.z = baseHex.z - 1;
			break;
			case 5:
			if(evenZ)
			{
				returnHex.x = baseHex.x + 1;
			}
			else
			{
				returnHex.x = baseHex.x;
			}
			returnHex.z = baseHex.z - 1;
			break;
			case 6:
			returnHex.x = baseHex.x;
			returnHex.z = baseHex.z;
			break;
			default:
			UnityEngine.Debug.Log("GetAdjacentHex was called with a non 0-6 value");
			break;
		}
		return returnHex;
	}
	
	public List<Vector3> FindPath(Vector3 start, Vector3 end)
	{
		stopwatch = new Stopwatch();
		stopwatch.Start();
		int nodesAddedToOpenList = 0;
		int startCoordinateY = Mathf.RoundToInt(start.y);
		int startCoordinateZ = Mathf.RoundToInt(start.z);
		int startCoordinateX = 0;
		if(startCoordinateZ % 2 == 0)
		{
			startCoordinateX = Mathf.RoundToInt(start.x);
		}
		else
		{
			startCoordinateX = Mathf.RoundToInt(start.x + 0.5f);
		}
		start = CoordinateToVector3(startCoordinateX,startCoordinateY,startCoordinateZ);
		int endCoordinateZ = Mathf.RoundToInt(end.z);
		int endCoordinateX = 0;
		if(endCoordinateZ % 2 ==0)
		{
			endCoordinateX = Mathf.RoundToInt(end.x);
		}
		else
		{
			endCoordinateX = Mathf.RoundToInt(end.x + 0.5f);
		}
		
		List<Node> openList = new List<Node>();
		List<Node> closedList = new List<Node>();
		openList.Add(new Node(start,startCoordinateX,startCoordinateY,startCoordinateZ,0,0,0,null));
		
		while(openList.Count > 0)
		{
			NodeComparer comparer = new NodeComparer();
			openList.Sort(comparer);
			Node curNode = openList[0];
			//curNode.PrintNode();
			bool evenZ = false;
			if(curNode.z % 2 == 0)
			{
				evenZ = true;
			}
			openList.RemoveAt(0);
			for(int i = 0; i <6; i++)
			{
				int nextXi = 0;
				int nextZi = 0;
				float nextXf = 0;
				float nextZf = 0;
				float newG = 1; //Distance between them, always 1 unless things change.
				switch(i)
				{
					case 0:
					if(evenZ)
					{
						nextXi = curNode.x;
					}
					else
					{
						nextXi = curNode.x - 1;
					}
					nextZi = curNode.z + 1;
					nextXf = curNode.location.x - 0.5f;
					nextZf = curNode.location.z + 1f;
					newG+=.1180339887498948482045868343656f;
					break;
					case 1:
					if(evenZ)
					{
						nextXi = curNode.x + 1;
					}
					else
					{
						nextXi = curNode.x;
					}
					nextZi = curNode.z + 1;
					nextXf = curNode.location.x + 0.5f;
					nextZf = curNode.location.z + 1f;
					newG+=.1180339887498948482045868343656f;
					break;
					case 2:
					nextXi = curNode.x - 1;
					nextZi = curNode.z;
					nextXf = curNode.location.x - 1f;
					nextZf = curNode.location.z;
					break;
					case 3:
					nextXi = curNode.x + 1;
					nextZi = curNode.z;
					nextXf = curNode.location.x + 1f;
					nextZf = curNode.location.z;
					break;
					case 4:
					if(evenZ)
					{
						nextXi = curNode.x;
					}
					else
					{
						nextXi = curNode.x - 1;
					}
					nextZi = curNode.z - 1;
					nextXf = curNode.location.x - 0.5f;
					nextZf = curNode.location.z - 1f;
					newG+=.1180339887498948482045868343656f;
					break;
					case 5:
					if(evenZ)
					{
						nextXi = curNode.x + 1;
					}
					else
					{
						nextXi = curNode.x;
					}
					nextZi = curNode.z - 1;
					nextXf = curNode.location.x + 0.5f;
					nextZf = curNode.location.z - 1f;
					newG+=.1180339887498948482045868343656f;
					break;
				}
				if(!hexGrid.IsBlocked(new Vector3Int(nextXi,startCoordinateY,nextZi)))
				{
					Vector3 newLocation = new Vector3(nextXf,startCoordinateY,nextZf);
					if(nextXi == endCoordinateX && nextZi == endCoordinateZ)
					{
						List<Vector3> returnList = new List<Vector3>();
						returnList.Add(newLocation);
						returnList.Add(curNode.location);
						while(curNode.parent != null)
						{
							curNode = curNode.parent;
							returnList.Add(curNode.location);
						}
						returnList.Reverse();
						stopwatch.Stop();
						long elapsedTimeInMilliseconds = stopwatch.ElapsedMilliseconds;
						pathfindingText.text = "" + elapsedTimeInMilliseconds + " ms\n" + nodesAddedToOpenList + " nodes";
						return returnList;
					}
					
					float newH = Vector3.Distance(newLocation,end);
					float newF = newG + newH;
					bool dontAddToOpenList = false;
					for(int j = 0; j < openList.Count; j++)
					{
						if(openList[j].x == nextXi && openList[j].y == startCoordinateY && openList[j].z == nextZi)
						{
							//print("" + new Vector3(nextXf,startCoordinateY,nextZf) + " already on open list");
							dontAddToOpenList = true;
							j = openList.Count;
							/* if(openList[j].f < newF)
							{
								
							} */
						}
					}
					for(int k = 0; k < closedList.Count; k++)
					{
						if(closedList[k].x == nextXi && closedList[k].y == startCoordinateY && closedList[k].z == nextZi)
						{
							dontAddToOpenList = true;
							k = closedList.Count;
							/* if(closedList[k].f < newF)
							{
								
							} */
						}
					}
					if(!dontAddToOpenList)
					{
						nodesAddedToOpenList++;
						GameObject newHex = Instantiate(hexPrefab, new Vector3(nextXf,startCoordinateY,nextZf),hexPrefab.transform.rotation);
						NodeScript nodeScript = newHex.GetComponent<NodeScript>();
						nodeScript.location = newLocation;
						nodeScript.x = nextXi;
						nodeScript.y = startCoordinateY;
						nodeScript.z = nextZi;
						nodeScript.g = newG;
						nodeScript.h = newH;
						nodeScript.f = newF;
						nodeScript.parent = curNode;
						newHex.transform.SetParent(hexParent);
						newHex.name = "" + nodesAddedToOpenList;
						openList.Add(new Node(new Vector3(nextXf,startCoordinateY,nextZf),nextXi,startCoordinateY,nextZi,newG,newH,newF,curNode));
					}
				}
			}
			closedList.Add(curNode);
		}
		return null;
	}
}
