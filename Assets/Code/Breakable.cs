//Breakable.cs
//Created on 20.03.16 by Aaron C Gaudette as Ship.cs
//Edited on 03.27.16 by Matt Gitlin
//Generates breakables and manages logic, animations

using UnityEngine;
[ExecuteInEditMode]
[RequireComponent(typeof(PolygonCollider2D))]
[RequireComponent(typeof(PointEffector2D))]

public class Breakable : MonoBehaviour{
	public DynamicModule module;
	
	//Generation data point for editor
	[System.Serializable]
	public class Vertex{
		[HideInInspector] public string name = "";
		[Range(-1,1)] public float x = 0, y = 0;
		[HideInInspector] public int pointIndex; //Reference point
		
		public Vector2 offset{get{return new Vector2(x,y);}}
	}
	public Vector2 vertexOffset(Vertex v){return v.offset*size;}
	public Point vertexPoint(Vertex v){return points[v.pointIndex];}
	
	public Color color = Color.white, altColor = Color.gray;
	public int size = 12;
	public Vertex[] edge;
	
	[Header("Shape")]
	public bool generateVertices = true;
	public int sides = 4;
	public int radius = 1;
	[Range(0,360)] public int theta = 45;
	
	//Regenerate ship on button press
	[Header("Editor")]
	public bool regenerate = false;
	
	[Header("Read-only")]
	public float mass = 0;
	public bool hasRowball = true;
	public bool totaled = false;
	
	[HideInInspector] public Point[] points;
	
	public Point center{get{return points[0];}}
	public Vector2 position{get{return center.position;}}
	public Vector2 velocity{get{return center.velocity;}}
	public float speed{get{return velocity.magnitude;}}
	public Vector2 spawnPosition{get{return transform.position;}}
	
	//References
	GameObject rowballSprite;
	PolygonCollider2D bounds, collisionBounds;
	PointEffector2D effector;
	void Start(){
		rowballSprite=transform.GetChild(0).gameObject;
		bounds=GetComponent<PolygonCollider2D>();
		effector=GetComponent<PointEffector2D>();
		collisionBounds=transform.GetChild(1).GetComponent<PolygonCollider2D>(); //Somewhat hardcoded
		
		Build();
		Update();
	}
	void OnDisable(){Clear();}
	
	//Build breakable
	void Build(){
		Clear();
		
		//Points
		points = new Point[edge.Length+1];
		//Center
		points[0]=ObjectManager.AddPoint(spawnPosition, module.pointMass, altColor, gameObject);
		
		if (generateVertices) {
			for (int i = 0; i < sides; i++) {
					Vertex v = new Vertex();
					v.x = (radius * Mathf.Cos (2 * Mathf.PI * i / sides + (theta * (Mathf.PI / 180))));
					v.y = (radius * Mathf.Sin (2 * Mathf.PI * i / sides + (theta * (Mathf.PI / 180))));
					v.pointIndex = i + 1;
					edge [i] = v;
				}
		}
		
		for(int i=1;i<edge.Length+2;++i){
			//Vertices
			if(i<edge.Length+1){
				points[i]=ObjectManager.AddPoint(spawnPosition+vertexOffset(edge[i-1]), module.pointMass,color,gameObject);
				edge[i-1].pointIndex=i;
			}
			//Links
			if(i>1){
				Point start = points[i-1], end = points[((i-1)%edge.Length)+1];
				start.AddLink(end,Vector2.Distance(start.position,end.position), module.stiffness, module.damping,color);
				//Framework
				end=center;
				end.AddLink(start,Vector2.Distance(start.position,end.position), module.stiffness, module.damping,altColor);
			}
		}
	}
	void Clear(){
		edge = new Vertex[sides];
		foreach(Point p in points)if(p!=null){
			if(Application.isEditor)DestroyImmediate(p.gameObject);
			else Destroy(p.gameObject);
		}
	}
	
	void Update(){
		//Editor code
		
		if(regenerate){
			Build();
			regenerate=false;
		}
		
		//Update components in real time
		for(int i=0;i<points.Length;++i)if(points[i]!=null){
			points[i].UpdateData(module.pointMass, module.drag,i>0?color:altColor, -1);
			points[i].UpdateLinks(module.stiffness, module.damping,i>0?color:altColor);
		}
		effector.useColliderMask=false;
		effector.forceMagnitude=module.boundsForce;
		effector.distanceScale=1;
		effector.forceVariation=effector.drag=module.boundsDrag;
		effector.angularDrag=0;
		effector.forceSource=EffectorSelection2D.Collider;
		effector.forceTarget=EffectorSelection2D.Rigidbody;
		effector.forceMode=EffectorForceMode2D.Constant;
		
		//Sprite
		rowballSprite.SetActive(hasRowball);
		if(center!=null && hasRowball)rowballSprite.transform.position=center.position;
		
		//Update vertex ojects in editor
		for(int i=0;i<edge.Length;++i){
			Vertex v = edge[i];
			v.name="Vertex "+i+" ["+v.x+","+v.y+"]";
		}
		
		//Update effector polygon
		Vector2[] polygon = new Vector2[edge.Length];
		for(int i=0;i<polygon.Length;++i)
			if(vertexPoint(edge[i])!=null)polygon[i]=vertexPoint(edge[i]).position-spawnPosition;
		bounds.SetPath(0,polygon);
		//Ignore effector on own ship
		foreach(Point p in points)if(p!=null)Physics2D.IgnoreCollision(p.GetComponent<Collider2D>(),bounds,true);
		
		//Update collision bounds
		for(int i=0;i<polygon.Length;++i)if(vertexPoint(edge[i])!=null){
			Vector2 pointPosition = vertexPoint(edge[i]).position;
			polygon[i]=pointPosition+(pointPosition-center.position).normalized*(1+module.collisionBuffer)-spawnPosition;
		}
		collisionBounds.SetPath(0,polygon);
		
		//Calculate mass
		mass = module.pointMass*(edge.Length+1);
		
	}
	
	Rowball SpawnRowball(){
		if(hasRowball){
			hasRowball=false;
			return ObjectManager.AddRowball(position,Color.white);
		}
		return null;
	}
	//Teleport rowball to other ship
	void Pass(ShipRevamped s){
		if(hasRowball && !s.totaled && !s.hasRowball){
			hasRowball=false;
			//Process pickup
			s.InitSpritePosition(rowballSprite.transform.position);
			s.hasRowball=true;
		}
	}
	
	//Collision
	public void ProcessCollision(Collider2D other){if(!totaled){
		//Process rowball
		if(other.gameObject.layer==module.rowballLayer){
			Rowball rowball = other.GetComponent<Rowball>();
			RowballState state = GetRowballState(rowball);
			
			if(state==RowballState.Kill){
				//Process
				totaled=true;
				effector.enabled=false;
				bounds.enabled=false;
				collisionBounds.gameObject.SetActive(false);
				//Drop rowball if carrying one
				Vector2 relativeVelocity = rowball.RelativeVelocity(velocity);
				if(hasRowball){
					Rowball r = SpawnRowball();
					r.AddImpulse(relativeVelocity*module.rowballDropFactor);
				}
				
				//Break closest point
				float minDistance = size*2; //Arbitrary number, should always be larger
				Point targetPoint = null;
				foreach(Vertex v in edge){
					Point p = vertexPoint(v); float distance;
					if(p!=null){
						distance=Vector2.Distance(p.position,rowball.position);
						if(distance<minDistance){
							minDistance=distance;
							targetPoint=p;
						}
					}
				}
				center.RemoveLink(targetPoint);
				targetPoint.RemoveLinks();
				//Random destruction and force
				for(int i=0;i<center.linkCount;++i)
					if(Random.Range(0f,1f)>(1-module.frameworkDestroyChance))center.RemoveLink(i); //Framework
				foreach(Point p in points)if(p!=null){
					if(p!=center && Random.Range(0f,1f)>(1-module.edgeDestroyChance))p.RemoveLinks(); //Edge
					//Force (no else, in order to process dummy links)
					p.AddImpulse(relativeVelocity*Random.Range(module.minDestroyForce,module.maxDestroyForce));
				}
			}
		}
		
		//Ramming (stealing)
		if(other.gameObject.layer==module.pointLayer){
			Point p = other.GetComponent<Point>();
			if(p.handler!=null && p.handler.GetComponent<ShipRevamped>()!=null){
                    ShipRevamped s = p.handler.GetComponent<ShipRevamped>();
				Vector2 relativeVelocity = center.velocity-s.center.velocity;
				//Check if relative velocity is above threshold and stealer is moving faster
				if(relativeVelocity.magnitude>module.stealSpeed && s.center.velocity.magnitude>center.velocity.magnitude)
					Pass(s);
			}
		}
	}}
	enum RowballState{Pickup,Bounce,Kill};
	RowballState GetRowballState(Rowball rowball){
		float relativeSpeed = rowball.RelativeVelocity(velocity).magnitude;
		//Pickup if rowball is slower than you or the relative speed is low enough
		if(relativeSpeed<module.damageSpeed || rowball.speed<=speed){return RowballState.Pickup;}
		//Kill if the rowball is faster than you and the relative speed is high enough
		else if(relativeSpeed>=module.damageSpeed && rowball.speed>speed){
			//Debug.Log("Rowball collision totaled breakable at "+relativeSpeed+" (relative speed)");
			return RowballState.Kill;
		}
		//If the rowball is faster than you but the relative speed is too low to damage, bounce
		else return RowballState.Bounce;
	}
}