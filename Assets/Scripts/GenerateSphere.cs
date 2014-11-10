using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// TODO: Destroy all lines and materials!!!

public class GenerateSphere : MonoBehaviour {
	public GameObject prefab;
	private int count = 128;
	private float size = 15;
	ArrayList dots = new ArrayList();
	private Mesh mesh;
	int recursionLevel = 3;
	float radius = 20f;
	float seaLevel = -0.25f;
	public Material seaMaterial;
	public Material landMaterial;

	void Start () {
		mesh = CreateIcoSphere ();

		// Generate a list of mesh vertices using simplex noise

		List<int> points = new List<int> ();

		for(int ii = 0; ii < mesh.vertices.Length; ii++){
			Vector3 v = mesh.vertices[ii];
			float noise = (float)SimplexNoise.noise (v.x, v.y, v.z);
			if(noise > seaLevel){
				points.Add (ii);
			}
		}

		for(int ii = 0; ii <= mesh.triangles.Length - 3; ii += 3){

			// Check whether all three points of the triangle are in our list of noise generated points

			Vector3 p1 = mesh.vertices[mesh.triangles[ii]];
			Vector3 p2 = mesh.vertices[mesh.triangles[ii + 1]];
			Vector3 p3 = mesh.vertices[mesh.triangles[ii + 2]];
			
			if(points.IndexOf(mesh.triangles[ii]) > -1 && points.IndexOf (mesh.triangles[ii + 1]) > -1 && points.IndexOf (mesh.triangles[ii + 2]) > -1){
				DrawLine (p1, p2, Color.red);
				DrawLine (p2, p3, Color.red);
				DrawLine (p3, p1, Color.red);
				CreateTriangle (p1, p2, p3, landMaterial);



				// Vector3 middle = GetMiddleOfTriangle(p1, p2, p3);
				// GameObject g = (GameObject)GameObject.Instantiate(prefab, middle, Quaternion.identity);
				// g.transform.parent = transform;
				// g.transform.localRotation = Quaternion.LookRotation (transform.position - g.transform.position);
			}else{
				CreateTriangle (p1, p2, p3, seaMaterial);
			}
		}
		
		// Draw lines between nodes

		float threshold = 12f;

		foreach(Vector3 v in mesh.vertices){
			foreach(Vector3 v2 in mesh.vertices){
				if(v != v2){
					float distance = Vector3.Distance(v, v2);
					if(distance < threshold){
						// DrawLine(v, v2, Color.yellow);
					}
				}
			}
		}
	}

	void DrawLine(Vector3 a, Vector3 b, Color c){
		GameObject g = new GameObject();
		g.transform.parent = transform;
		LineRenderer l = g.AddComponent<LineRenderer>();
		l.useWorldSpace = true;
		l.SetVertexCount(2);
		l.SetPosition(0, a);
		l.SetPosition(1, b);
		l.SetWidth(0.1f, 0.1f);
		l.material = new Material (Shader.Find("Particles/Additive"));
		l.material.color = Color.red;
	}
	
	ArrayList GetTrianglesForVertex(int vIndex){
		ArrayList tris = new ArrayList();
		for(int ii = 0; ii < mesh.triangles.Length - 2; ii += 3){
			if(mesh.triangles[ii] == vIndex || mesh.triangles[ii + 1] == vIndex || mesh.triangles[ii + 2] == vIndex){
				tris.Add (new [] {mesh.triangles[ii], mesh.triangles[ii + 1], mesh.triangles[ii + 2]});
			}
		}

		return tris;
	}

	private void CreateTriangle(Vector3 p1, Vector3 p2, Vector3 p3, Material mat){
		GameObject g = (GameObject)GameObject.Instantiate (prefab, transform.position, Quaternion.identity);
		g.transform.parent = transform;
		g.AddComponent ("MeshFilter");
		g.AddComponent ("MeshRenderer");
		g.AddComponent ("MeshCollider");
		Mesh m = (g.GetComponent<MeshFilter> ()).mesh;

		m.Clear ();
		m.vertices = new Vector3[] {p1, p2, p3};

		// m.uv = new []{new Vector2 (0, 0), new Vector2 (0, 1), new Vector2 (1, 1)};
		m.uv = new Vector2[ m.vertices.Length ];
		m.triangles = new int[]{0, 1, 2};

		
		m.RecalculateBounds();
		m.RecalculateNormals ();
		m.Optimize();

		MeshCollider c = (MeshCollider)g.GetComponent<MeshCollider> ();
		c.sharedMesh = m;

		g.renderer.material = mat;

		// g.transform.parent = transform;
	}

	private Vector3 GetNormalOfTriangle(Vector3 p1, Vector3 p2, Vector3 p3){
		return Vector3.Cross(p2 - p1, p2 - p3).normalized;
	}

	private Vector3 GetMiddleOfTriangle(Vector3 p1, Vector3 p2, Vector3 p3){
		return ((p1 + p2 + p3) / 3);
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
		gameObject.GetComponent<MeshRenderer> ().enabled = false;
		
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
