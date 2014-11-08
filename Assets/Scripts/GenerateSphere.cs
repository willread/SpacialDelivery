using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GenerateSphere : MonoBehaviour {
	public GameObject prefab;
	private int count = 128;
	private float size = 15;
	ArrayList dots = new ArrayList();
	private Mesh mesh;
	int recursionLevel = 4;
	float radius = 20f;

	void Start () {
		mesh = CreateIcoSphere ();

		// Place objects on each vertice and rotate to face center

		float perlinX = Random.Range (0f, 9999f);
		float perlinY = Random.Range (0f, 9999f);
		float perlinScaleX = 10f;
		float perlinScaleY = 20f;

		foreach(Vector3 v in mesh.vertices){
			GameObject g = (GameObject)GameObject.Instantiate(prefab, v, Quaternion.identity);
			g.transform.parent = transform;
			g.transform.localRotation = Quaternion.LookRotation (transform.position - g.transform.position);

			// Generate some terrain using perlin noise
			// http://docs.unity3d.com/ScriptReference/Mathf.PerlinNoise.html

			Vector2 polar = CartesianToPolar(g.transform.position);
			float noise = Mathf.PerlinNoise(perlinX + Normalize(polar.x, -90, 90) * perlinScaleX, perlinY + Normalize (polar.y, -180, 180) * perlinScaleY);
			// g.transform.localScale = new Vector3(1f, 1f, noise * 20f);
			// Debug.Log(noise);
			if(noise > 0.5f){
				g.renderer.enabled = false;
			}
		}
	}

	void Update (){
		transform.Rotate (Vector3.up * Time.deltaTime * 5f);
	}

	private float Normalize(float num, float min, float max){
		return (num - min) / (max - min);
	}

	// Below code borrowed and converted to C# from a stack overflow answer:
	// @ http://answers.unity3d.com/questions/189724/polar-spherical-coordinates-to-xyz-and-vice-versa.html

	private Vector2 CartesianToPolar(Vector3 point){
		Vector2 polar = new Vector2();

		polar.y = Mathf.Atan2(point.x, point.y);

		float xzLen = (new Vector2(point.x, point.z)).magnitude;
		polar.x = Mathf.Atan2 (-point.y, xzLen);

		polar *= Mathf.Rad2Deg;

		return polar;
	}


	// Below code lifted from the Unity wiki
	// @ http://wiki.unity3d.com/index.php/ProceduralPrimitives

	private struct TriangleIndices
	{
		public int v1;
		public int v2;
		public int v3;
		
		public TriangleIndices(int v1, int v2, int v3)
		{
			this.v1 = v1;
			this.v2 = v2;
			this.v3 = v3;
		}
	}
	
	// return index of point in the middle of p1 and p2

	private static int getMiddlePoint(int p1, int p2, ref List<Vector3> vertices, ref Dictionary<long, int> cache, float radius)
	{
		// first check if we have it already
		bool firstIsSmaller = p1 < p2;
		long smallerIndex = firstIsSmaller ? p1 : p2;
		long greaterIndex = firstIsSmaller ? p2 : p1;
		long key = (smallerIndex << 32) + greaterIndex;
		
		int ret;
		if (cache.TryGetValue(key, out ret))
		{
			return ret;
		}
		
		// not in cache, calculate it
		Vector3 point1 = vertices[p1];
		Vector3 point2 = vertices[p2];
		Vector3 middle = new Vector3
			(
				(point1.x + point2.x) / 2f, 
				(point1.y + point2.y) / 2f, 
				(point1.z + point2.z) / 2f
				);
		
		// add vertex makes sure point is on unit sphere
		int i = vertices.Count;
		vertices.Add( middle.normalized * radius ); 
		
		// store it, return index
		cache.Add(key, i);
		
		return i;
	}
	
	public Mesh CreateIcoSphere(){
		MeshFilter filter = gameObject.AddComponent< MeshFilter >();
		Mesh mesh = filter.mesh;
		mesh.Clear();
		
		List<Vector3> vertList = new List<Vector3>();
		Dictionary<long, int> middlePointIndexCache = new Dictionary<long, int>();
		int index = 0;
		
		// create 12 vertices of a icosahedron
		float t = (1f + Mathf.Sqrt(5f)) / 2f;
		
		vertList.Add(new Vector3(-1f,  t,  0f).normalized * radius);
		vertList.Add(new Vector3( 1f,  t,  0f).normalized * radius);
		vertList.Add(new Vector3(-1f, -t,  0f).normalized * radius);
		vertList.Add(new Vector3( 1f, -t,  0f).normalized * radius);
		
		vertList.Add(new Vector3( 0f, -1f,  t).normalized * radius);
		vertList.Add(new Vector3( 0f,  1f,  t).normalized * radius);
		vertList.Add(new Vector3( 0f, -1f, -t).normalized * radius);
		vertList.Add(new Vector3( 0f,  1f, -t).normalized * radius);
		
		vertList.Add(new Vector3( t,  0f, -1f).normalized * radius);
		vertList.Add(new Vector3( t,  0f,  1f).normalized * radius);
		vertList.Add(new Vector3(-t,  0f, -1f).normalized * radius);
		vertList.Add(new Vector3(-t,  0f,  1f).normalized * radius);
		
		
		// create 20 triangles of the icosahedron
		List<TriangleIndices> faces = new List<TriangleIndices>();
		
		// 5 faces around point 0
		faces.Add(new TriangleIndices(0, 11, 5));
		faces.Add(new TriangleIndices(0, 5, 1));
		faces.Add(new TriangleIndices(0, 1, 7));
		faces.Add(new TriangleIndices(0, 7, 10));
		faces.Add(new TriangleIndices(0, 10, 11));
		
		// 5 adjacent faces 
		faces.Add(new TriangleIndices(1, 5, 9));
		faces.Add(new TriangleIndices(5, 11, 4));
		faces.Add(new TriangleIndices(11, 10, 2));
		faces.Add(new TriangleIndices(10, 7, 6));
		faces.Add(new TriangleIndices(7, 1, 8));
		
		// 5 faces around point 3
		faces.Add(new TriangleIndices(3, 9, 4));
		faces.Add(new TriangleIndices(3, 4, 2));
		faces.Add(new TriangleIndices(3, 2, 6));
		faces.Add(new TriangleIndices(3, 6, 8));
		faces.Add(new TriangleIndices(3, 8, 9));
		
		// 5 adjacent faces 
		faces.Add(new TriangleIndices(4, 9, 5));
		faces.Add(new TriangleIndices(2, 4, 11));
		faces.Add(new TriangleIndices(6, 2, 10));
		faces.Add(new TriangleIndices(8, 6, 7));
		faces.Add(new TriangleIndices(9, 8, 1));
		
		
		// refine triangles
		for (int i = 0; i < recursionLevel; i++)
		{
			List<TriangleIndices> faces2 = new List<TriangleIndices>();
			foreach (var tri in faces)
			{
				// replace triangle by 4 triangles
				int a = getMiddlePoint(tri.v1, tri.v2, ref vertList, ref middlePointIndexCache, radius);
				int b = getMiddlePoint(tri.v2, tri.v3, ref vertList, ref middlePointIndexCache, radius);
				int c = getMiddlePoint(tri.v3, tri.v1, ref vertList, ref middlePointIndexCache, radius);
				
				faces2.Add(new TriangleIndices(tri.v1, a, c));
				faces2.Add(new TriangleIndices(tri.v2, b, a));
				faces2.Add(new TriangleIndices(tri.v3, c, b));
				faces2.Add(new TriangleIndices(a, b, c));
			}
			faces = faces2;
		}
		
		mesh.vertices = vertList.ToArray();
		
		List< int > triList = new List<int>();
		for( int i = 0; i < faces.Count; i++ )
		{
			triList.Add( faces[i].v1 );
			triList.Add( faces[i].v2 );
			triList.Add( faces[i].v3 );
		}		
		mesh.triangles = triList.ToArray();
		mesh.uv = new Vector2[ mesh.vertices.Length ];
		
		Vector3[] normales = new Vector3[ vertList.Count];
		for( int i = 0; i < normales.Length; i++ )
			normales[i] = vertList[i].normalized;
		
		
		mesh.normals = normales;
		
		mesh.RecalculateBounds();
		mesh.Optimize();
		return mesh;
	}
}
