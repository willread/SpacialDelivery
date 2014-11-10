using UnityEngine;
using System.Collections;

public class CameraDrag : MonoBehaviour {

	private float dragSpeed = 0.25f;
	private Vector3 dragOrigin;
	private float spinSpeed = 100f;
	private Vector3 spinTarget = Vector3.zero;
	private float spinThreshold = 1.5f;

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

		if(spinTarget != Vector3.zero){
			if(Vector3.Distance(Camera.main.transform.position, spinTarget) > spinThreshold){
				Camera.main.transform.RotateAround (Vector3.zero, -Vector3.Cross (spinTarget, Camera.main.transform.position), spinSpeed * Time.deltaTime);
			}
		}
	}

	public void RotateToLookAt (Vector3 pos) {
		spinTarget = pos.normalized * Camera.main.transform.position.magnitude;
	}
}