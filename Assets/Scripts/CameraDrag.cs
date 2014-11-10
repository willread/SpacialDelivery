using UnityEngine;
using System.Collections;

public class CameraDrag : MonoBehaviour {

	private float dragSpeed = 0.25f;
	private Vector3 dragOrigin;

	void Start () {
	
	}

	void Update () {
		if(Input.GetMouseButtonDown (0)){
			dragOrigin = Input.mousePosition;
		}else{
			if(Input.GetMouseButton(0)){
				if(Input.mousePosition.x != dragOrigin.x){
					Camera.main.transform.RotateAround(Vector3.zero, Camera.main.transform.up, dragSpeed * (Input.mousePosition.x - dragOrigin.x));
				}

				if(Input.mousePosition.y != dragOrigin.y){
					Camera.main.transform.RotateAround(Vector3.zero, -Camera.main.transform.right, dragSpeed * (Input.mousePosition.y - dragOrigin.y));
				}
			}
			dragOrigin = Input.mousePosition;
		}

		Vector2 diff = dragOrigin - Input.mousePosition;
	}
}