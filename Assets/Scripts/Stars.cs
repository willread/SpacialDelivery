using UnityEngine;
using System.Collections;

public class Stars : MonoBehaviour {

	public GameObject prefab;
	private float minRadius = 1000f;
	private float maxRadius = 1500f;
	private float num = 500;

	// Use this for initialization
	void Start () {
		for(int ii = 0; ii < num; ii++){
			GameObject g = (GameObject)GameObject.Instantiate (prefab);
			g.transform.position = Random.insideUnitSphere * Random.Range(minRadius, maxRadius);
			g.transform.parent = transform;
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
