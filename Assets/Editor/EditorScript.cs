using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TMPro;

[CustomEditor(typeof(WalkableArea))]
public class EditorScript : Editor
{
	//public TMP_Text dataText;
	//seems to work only after the collider is selected.
	
	//what about making some kind of visual editor that can make levels? would make a ton of this nonsense
	//a lot easier.
	
	//Story idea: The Last Coomer. After the appocolypse came, nothing much was left. But Germinus Beepnor
	//didn't mind. He didn't even know anything happened. With his automatic cheeto delivery system and
	//cocksucking machine, he never had to leave the comfort of his goon cave, for 40 years.
	//But today, the thrust regulator on the cocksucking machine broke. Now Germinus must face the
	//evils of the wastes in hopes of finding someone capable of repairing... his cocksucking machine.
	
    private void OnSceneGUI()
    {
        WalkableArea walkableArea = (WalkableArea)target; // Cast the target to your specific script type

        Event currentEvent = Event.current;
        EventType eventType = currentEvent.type;

        if (eventType == EventType.MouseDown)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                // Check if the hit object is the plane
                if (hit.collider.gameObject == walkableArea.gameObject)
                {
                    Vector3 origin = walkableArea.movementHex.localPosition;
                    Vector3 clickPoint = hit.point;
					
					float closestZ = Mathf.Round(clickPoint.z);
					float closestX = 0;
					if(closestZ % 2 == 0)
					{
						closestX = Mathf.Round(clickPoint.x);
					}
					else
					{
						closestX = Mathf.Floor(clickPoint.x) + 0.5f;
					}
					
					Vector3 hexPoint = new Vector3(closestX, clickPoint.y, closestZ);
					Vector3Int hexPointInt = walkableArea.Vector3ToCoordinate(hexPoint);
					
					Debug.Log(""+hexPoint+ "  " +hexPointInt);
					walkableArea.dataText.text = "" + hexPoint + "\n" + hexPointInt;
					if(origin != hexPoint)
					{
						walkableArea.movementHex.localPosition = hexPoint;
					}
                }
            }
        }
    }
}




