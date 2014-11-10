using UnityEngine;
using System.Collections;

public class Triangle : MonoBehaviour {

	public Material highlightMaterial;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetMouseButtonDown(0)){
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if(Physics.Raycast(ray, out hit)){
				// Debug.Log ("HIT" + hit.collider);
				// hit.collider.renderer.enabled = false;
				if(hit.collider.gameObject == gameObject){
					renderer.material = highlightMaterial;
				}
			}
		}
	}
}
