using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*

From 0,0 to the back end of the house in the 'path' file, 2,0,-3: 52ms, 1107 nodes. After fixing g value

*/

public class NodeScript : MonoBehaviour
{
    public Vector3 location;
	public int x;
	public int y;
	public int z;
	public float g;
	public float h;
	public float f;
	public WalkableArea.Node parent;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
