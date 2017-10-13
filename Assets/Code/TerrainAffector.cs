using UnityEngine;
using System.Collections;
using System.Collections.Generic;
[ExecuteInEditMode]
[RequireComponent(typeof(PolygonCollider2D))]

public class TerrainAffector : MonoBehaviour {
	
	//Standardized layer for the TerrainAffector
	const int TerrainAffectorLayer = 12;
	
	const int pointLayer = 10; //need to standardize this across all code
	
	public List<Vector2> vertices = new List<Vector2>();
	
	public int sides = 4;
	public int radius = 4;
	public bool isSquare = true;
	
	public int radiusX = 4;
	public int radiusY = 4;
	
	[Range(0,360)] public int theta = 45;
	
	public Color color = Color.white;
	public LineRenderer prefab;
	
	[HideInInspector] Dictionary<ShipRevamped, List<Point>> pointsCache = new Dictionary<ShipRevamped, List<Point>>();
	[HideInInspector] List<LineRenderer> _lrCache = new List<LineRenderer>();
	
	PolygonCollider2D polygonCollider;
	
	public bool rebuild = false;
	
	public float terrainAffectorDragFactor = 0.05f;
	
	Vector2 position{get{return transform.position;}}
	
	void Build () {
		Clear ();
		vertices = new List<Vector2> ();
		_lrCache = new List<LineRenderer> ();
		pointsCache = new Dictionary<ShipRevamped, List<Point>> ();
		
		if (isSquare)
			radiusX = radiusY = radius;
		
		vertices.Add(new Vector2(-radiusX, radiusY));
		vertices.Add(new Vector2 (radiusX, radiusY));
		vertices.Add(new Vector2 (radiusX, -radiusY));
		vertices.Add(new Vector2 (-radiusX, -radiusY));
		for (int i = 0; i < sides; i++) {
			LineRenderer lr = Instantiate(prefab, Vector2.zero, Quaternion.identity) as LineRenderer;
            
            lr.startColor = color;
            lr.endColor = color;
  
            lr.SetPosition(0,vertices[i]+position);
			lr.SetPosition(1,vertices[(i + 1) % sides]+position);
			lr.transform.parent = transform;
			_lrCache.Add(lr);
		}
		
		polygonCollider.SetPath(0,vertices.ToArray());
	}
	
	void Clear() {
		foreach (LineRenderer l in _lrCache) {
			if (l != null) {
				DestroyImmediate (l.gameObject);
			}
		}
	}
	
	void OnTriggerEnter2D(Collider2D c){
		//print(c.gameObject.name+" entered");
		//Debug.Log ("On Trigger Enter");
		//Debug.Log (c.GetComponent<Point> ().position);
		ShipRevamped s = null;
		
		//Checks if ship has collided with goal
		if (c.gameObject.layer == pointLayer && c.GetComponent<Point> ().handler != null) {
			s = c.GetComponent<Point> ().handler.GetComponent<ShipRevamped> ();
			if (s != null && !s.totaled) {
				if (!pointsCache.ContainsKey (s)) {
					pointsCache.Add (s, new List<Point> ());
				}
				pointsCache[s].Add (c.GetComponent<Point> ());
			}
		}
		
		if (s != null && !s.totaled && pointsCache[s].Count > 0) {
			//Debug.Log ("Ship is not null");
			PlayerManager.PlayerInfo p = PlayerManager.GetPlayer (s);
			if (p != null) {
				//Debug.Log ("Setting terrainAffector on");
				s.SetTerrainAffectorOn (terrainAffectorDragFactor);
			} else {
				//Debug.Log ("Player is null");
			}
		}
	}
	
	void OnTriggerExit2D(Collider2D c){
		//Debug.Log ("On Trigger Exit");
		//Debug.Log (c.GetComponent<Point> ().position);
		ShipRevamped s = null;
		
		//Checks if ship has collided with goal
		if (c.gameObject.layer == pointLayer && c.GetComponent<Point> ().handler != null) {
			s = c.GetComponent<Point> ().handler.GetComponent<ShipRevamped> ();
			if (s != null && !s.totaled) {
				pointsCache[s].Remove(c.GetComponent<Point> ());
			}
		}
		DetectsTerrainAffectorOff(s);
	}
	
	// Use this for initialization
	void Start () {
		polygonCollider = GetComponent<PolygonCollider2D> ();
		polygonCollider.isTrigger = true;
		Build ();
		foreach(LineRenderer l in _lrCache)
        {
            l.startColor = color;
            l.endColor = color;
        }

        //Observe
        if (!Application.isEditor || Application.isPlaying){
			Observer.OnKill.AddListener(OnKill);
		}
	}
	
	//Method to encapsulate whether we should turn off the terrain affector
	void DetectsTerrainAffectorOff(ShipRevamped s) {
		if (s != null) {
			PlayerManager.PlayerInfo p = PlayerManager.GetPlayer (s);
			//Debug.Log ("Points cache size is " + pointsCache.Count);
			if(!pointsCache.ContainsKey (s)) return;
			if (p != null && pointsCache[s].Count == 0) {
				//Debug.Log ("Setting terrainAffector off");
				s.SetTerrainAffectorOff ();
			}
		}
	}
	
	//Removes all points of a ship if dead
	void RemoveAllPoints(ShipRevamped s) {
		if (s != null) {
			PlayerManager.PlayerInfo p = PlayerManager.GetPlayer (s);
			//Debug.Log ("Points cache size is " + pointsCache.Count);
			if(!pointsCache.ContainsKey (s)) return;
			pointsCache[s]=new List<Point>();
			if (p != null && pointsCache[s].Count == 0) {
				//Debug.Log ("Setting terrainAffector off");
				s.SetTerrainAffectorOff ();
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (rebuild) {
			Build ();
			rebuild = false;
		}
		/*
		if(GameManager.instance.winners!=null){
			transform.localScale+=2*Vector3.one*Time.deltaTime;
			GetComponent<SpriteRenderer>().sharedMaterial.color=
				Color.Lerp(new Color(1,0,0.01f),new Color(1,0.01f,0),(Time.time%0.15f)/0.15f);
		}
		else GetComponent<SpriteRenderer>().sharedMaterial.color=new Color(0.9f,0.9f,0.9f,1);
		*/
	}
	
	//
	void OnKill(ShipRevamped targetShip, Rowball rowball){
		//StartCoroutine(Shake(0.5f));
		RemoveAllPoints(targetShip);
	}
	IEnumerator Shake(float t){
		float start = Time.time;
		while(Time.time<start+t){
			transform.Rotate(0,0,Random.Range(0,360));
			yield return 0;
		}
		transform.rotation=Quaternion.identity;
	}
}