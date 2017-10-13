//Terrain.cs
//Created on 03.30.16 by Matt Gitlin
//

using UnityEngine;
[ExecuteInEditMode]
[RequireComponent(typeof(PolygonCollider2D))]

public class WorldTerrain : MonoBehaviour {
	const int pointLayer = 10; //need to standardize this across all code

	[Header("Game Logic")]
	public Color color = Color.white;
	public int team = -1;
	public bool isGoal = false;
	public int health = 0;

	[Header("Shape")]
	public int sides = 4;
	public int radius = 10;
	[Range(0,360)] public int theta = 45;
	public bool generateVertices = true;
	public Vector2[] vertices = new Vector2[]{Vector2.zero};
	
	public LineRenderer prefab;
	
	[Header("Editor")]
	public bool regenerate = false;

	[HideInInspector] public LineRenderer[] _lrCache = new LineRenderer[0];
	[HideInInspector] public LineRenderer[][] _goalShieldCache = new LineRenderer[0][];

	PolygonCollider2D polygonCollider;
	
	Vector2 position{get{return transform.position;}}

	void Build(){
		Clear();
		_lrCache = new LineRenderer[sides];
		if (generateVertices) {
			vertices = new Vector2[sides];
			for (int i = 0; i < sides; i++) {
				vertices [i] = new Vector2(
					radius * Mathf.Cos (2 * Mathf.PI * i / sides + (theta * (Mathf.PI / 180))),
					radius * Mathf.Sin (2 * Mathf.PI * i / sides + (theta * (Mathf.PI / 180)))
				);
			}
		}
		for (int i = 0; i < sides; i++) {
			LineRenderer lr = Instantiate(prefab, Vector2.zero, Quaternion.identity) as LineRenderer;

            lr.startColor = color;
            lr.endColor = color;
            
            lr.SetPosition(0,vertices[i]+position);
			lr.SetPosition(1,vertices[(i + 1) % sides]+position);
			lr.transform.parent = transform;
			_lrCache [i] = lr;
		}
		polygonCollider.SetPath(0,vertices);

		if (isGoal) {
			health = 3;
			_goalShieldCache = new LineRenderer[3][];
			
			for (int i = 0; i < 3; ++i) {
				_goalShieldCache [i] = new LineRenderer[sides];
				for (int j = 0; j < sides; ++j) {
					LineRenderer lr = Instantiate(prefab, Vector2.zero, Quaternion.identity) as LineRenderer;

                    lr.startColor = color;
                    lr.endColor = color;
                   
                    lr.SetPosition(0,(vertices[j] * (i + 1)/4)+position);
					lr.SetPosition(1,(vertices[(j + 1) % sides] * (i+1))/4+position);
					lr.transform.parent = transform;
					_goalShieldCache [i][j] = lr;
				}
			}
		}
	}
	void Clear(){
		foreach(LineRenderer lr in _lrCache)if(lr != null)
			DestroyImmediate(lr.gameObject);
		if (isGoal) {
			foreach (LineRenderer[] l in _goalShieldCache) {
				if (l != null) {
					foreach (LineRenderer lr in l) {
						if (lr != null)
							DestroyImmediate (lr.gameObject);
					}
				}
			}
		}
	}

	void OnTriggerEnter2D(Collider2D c){
        ShipRevamped s = null;
		Rowball[] rs = null;
		float distance = 0;

		//Checks if ship has collided with goal
		if (c.gameObject.layer == pointLayer && c.GetComponent<Point> ().handler != null) {
			s = c.GetComponent<Point> ().handler.GetComponent<ShipRevamped> ();
		}

		//Checks if rowball has collided with goal
		if (c.gameObject.layer == 8 && c.GetComponents<Rowball>().Length > 0) {
			rs = c.GetComponents<Rowball> ();
			if (rs != null) {
				foreach (Rowball r in rs) {
					if (r != null) {
						//Shield check
						if (health > 0) {
							if (r.speed > 3) {
								for (int j = 0; j < sides; ++j) {
									_goalShieldCache [health-1][j].startColor = Color.clear;
                                    _goalShieldCache[health - 1][j].endColor = Color.clear;
                                }
								health--;
							}
							//TODO: Figure out if deterministic physics are good
							r.velocity = -r.velocity;
							r.velocity += new Vector2(Random.Range (-2, 2), Random.Range (-2, 2));
							break;
						} else {
							s = r.shooter.GetComponent<ShipRevamped>();
							distance = Vector3.Distance (r.shotPosition, transform.position);
							break;
						}
					}
				}
			}
		}
			
		if(s!=null){
			if (health > 0) {
				return;
			}
			//PlayerManager.PlayerInfo p = (s.hasRowball || rs != null) ? PlayerManager.GetPlayer(s) : null;
			//if(p!=null && p.player.team==team && GameManager.instance.roundState==GameManager.RoundState.PLAYING && !GameManager.instance.roundOver){
			//	s.scoredGoal = true;
			//	s.shotDistance = distance;
			//	Observer.OnGoal.Invoke (s);
			//}
		}
	}

	void Start(){
		polygonCollider=GetComponent<PolygonCollider2D>();
		Build();
		Update();
	}
	void Update(){

		if (isGoal) {
			polygonCollider.isTrigger = true;
			color=PlayerManager.TeamColor(team);
		}
		foreach(LineRenderer l in _lrCache)
        {
            l.startColor = color;
            l.endColor = color;
        }

		if (isGoal) {
			for (int i = 0; i < health; ++i) {
				foreach (LineRenderer l in _goalShieldCache[i])
                {
                    l.startColor = color;
                    l.endColor = color;
                }
            }
		}

		//Editor
		if(regenerate){
			Build();
			regenerate=false;
		}
	}
}