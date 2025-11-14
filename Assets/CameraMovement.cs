using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CameraMovement : MonoBehaviour
{
    public float wasdSpeed;
	public float scrollSpeed;
	public float panSpeed;
	public Camera overheadCam;
	public Camera firstPersonCam;
	private bool dragging;
	private Vector2 dragAnchor;
	private int curCam;
	public float fpSensitivity;
	public float fpMoveSpeed;
	private Vector2 fpRotation = Vector2.zero;
	public Transform playerTransform;
	public WalkableArea walkableArea;
	public TMP_Text positionText;
	
    void Start()
    {
        
    }
	
	void HandleOverheadCamera()
	{
		Vector3 pos = transform.localPosition;
        if(Input.GetKey(KeyCode.W))
		{
			pos.z += Time.deltaTime*wasdSpeed;
		}
		if(Input.GetKey(KeyCode.S))
		{
			pos.z -= Time.deltaTime*wasdSpeed;
		}
		if(Input.GetKey(KeyCode.D))
		{
			pos.x += Time.deltaTime*wasdSpeed;
		}
		if(Input.GetKey(KeyCode.A))
		{
			pos.x -= Time.deltaTime*wasdSpeed;
		}
		float scrollWheel = -Input.GetAxisRaw("Mouse ScrollWheel");
		if(scrollWheel != 0)
		{
			if((scrollWheel > 0 && overheadCam.orthographicSize <= 12)||(scrollWheel < 0 && overheadCam.orthographicSize >=3))
			{
				overheadCam.orthographicSize += scrollWheel*scrollSpeed;
			}
		}
		if(Input.GetMouseButtonDown(2))
		{
			dragAnchor = Input.mousePosition;
			dragging = true;
		}
		if(Input.GetMouseButtonUp(2))
		{
			dragging = false;
		}
		if(dragging)
		{
			Vector2 offset = new Vector2(Input.mousePosition.x - dragAnchor.x,Input.mousePosition.y - dragAnchor.y);
			pos.x -= offset.x*panSpeed/Screen.width;
			pos.z -= offset.y*panSpeed/Screen.height;
			dragAnchor = Input.mousePosition;
		}
		transform.localPosition = pos;
	}
	
	void HandleFirstPersonCamera()
	{
		fpRotation.y += Input.GetAxis("Mouse X") * fpSensitivity;
		fpRotation.x += Input.GetAxis("Mouse Y") * fpSensitivity;
		fpRotation.x = Mathf.Clamp(fpRotation.x, -90f,90f);
		
		firstPersonCam.transform.localRotation = Quaternion.Euler(fpRotation);
		
		float moveX = Input.GetAxis("Horizontal") * fpMoveSpeed * Time.deltaTime;
		float moveZ = Input.GetAxis("Vertical") * fpMoveSpeed * Time.deltaTime;
		
		Vector3 moveDirection = new Vector3(moveX,0,moveZ);
		//moveDirection = playerTransform.TransformDirection(moveDirection);
		moveDirection = firstPersonCam.transform.TransformDirection(moveDirection);
		moveDirection.y = 0;
		
		Vector3Int currentHexInt = walkableArea.Vector3ToCoordinate(playerTransform.position);
		positionText.text = "" + playerTransform.position + "\n" + currentHexInt;
		
		if(walkableArea.hexGrid.IsBlocked(walkableArea.Vector3ToCoordinate(playerTransform.position + moveDirection*5)))
		{
			moveDirection = Vector3.zero;
		}
		/* if(walkableArea.hexGrid.IsBlocked(walkableArea.Vector3ToCoordinate(playerTransform.position + new Vector3(moveDirection.x+0.2f,0,0))))
		{
			moveDirection.x = 0;
		}
		if(walkableArea.hexGrid.IsBlocked(walkableArea.Vector3ToCoordinate(playerTransform.position + new Vector3(0,0,moveDirection.z+0.2f))))
		{
			moveDirection.z = 0;
		} */
		
		playerTransform.position += moveDirection;
		//firstPersonCam.transform.position += moveDirection;
	}

    void Update()
    {
		switch(curCam)
		{
			case 0:
			HandleOverheadCamera();
			break;
			case 1:
			HandleFirstPersonCamera();
			break;
		}
		if(Input.GetKeyDown(KeyCode.F))
		{
			if(curCam == 0)
			{
				Cursor.lockState = CursorLockMode.Locked;
				overheadCam.depth = -2;
				firstPersonCam.depth = -1;
				curCam = 1;
			}
			else
			{
				playerTransform.localRotation = Quaternion.identity;
				Cursor.lockState = CursorLockMode.None;
				overheadCam.depth = -1;
				firstPersonCam.depth = -2;
				curCam = 0;
			}
		}
    }
}
