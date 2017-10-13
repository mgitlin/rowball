//ObjectManager.cs
//Created on 18.03.16 by Aaron C Gaudette
//Instantiates and manages world objects

using UnityEngine;
[ExecuteInEditMode]

public class ObjectManager : MonoBehaviour{
	//Singleton (private)
	static ObjectManager objectManager = null;
	static ObjectManager instance{get{
		if(objectManager==null){
			objectManager=(ObjectManager)FindObjectOfType(typeof(ObjectManager));
			if(FindObjectsOfType(typeof(ObjectManager)).Length>1)Debug.Log("More than one ObjectManager instance in the scene!");
		}
		return objectManager;
	}}
	
	//Containers
	Transform _pointContainer = null, _rowballContainer = null;
	public static Transform pointContainer{get{return GetContainer("Points",instance._pointContainer);}}
	public static Transform rowballContainer{get{return GetContainer("Rowballs",instance._rowballContainer);}}
	public static Rowball[] rowballs{get{return rowballContainer.GetComponentsInChildren<Rowball>();}}
	
	static Transform GetContainer(string name, Transform container){
		if(container==null){
			foreach(Transform t in instance.GetComponentsInChildren<Transform>()){
				if(t.gameObject.name==name){
					container=t;
					return t;
				}
			}
			GameObject child = new GameObject(name);
			child.transform.parent=instance.transform;
			container=child.transform;
			return child.transform;
		}
		return container;
	}
	
	//Object references
	public Point point;
	public Rowball rowball;
	public LinkRenderer linkRenderer;
	public Oar oar;
	public CannonRevamped cannon;
	public Deck deck;
	
	//Clear objects on button press
	[Header("Editor")]
	public bool flush = false;
	public bool ignoreRowballs = true;
	void Update(){
		if(flush){
			DestroyImmediate(pointContainer.gameObject);
			if(!ignoreRowballs)DestroyImmediate(rowballContainer.gameObject);
			flush=false;
			if(GameController.map != null)
				DestroyImmediate (WorldManager.rowballSpawnerContainer.gameObject);
		}
		if (GameController.game != null && GameManager.instance.roundState == GameManager.RoundState.RESPAWN) {
			DestroyImmediate(rowballContainer.gameObject);
			DestroyImmediate (WorldManager.rowballSpawnerContainer.gameObject);
		}
	}
	public static void Flush(){instance.flush=true;}
	
	//Physical objects
	public static Point AddPoint(Vector2 position, int mass, Color color){return AddPoint(position,mass,color,null);}
	public static Point AddPoint(Vector2 position, int mass, Color color, GameObject handler){
		Point p = Instantiate(instance.point,position,Quaternion.identity) as Point;
		p.mass=mass; p.color=color;
		p.handler=handler;
		
		p.gameObject.name="Point"+(handler!=null?(" ("+handler.name+")"):"");
		return p;
	}
	public static Rowball AddRowball(Vector2 position, Color color){
        Debug.Log("Rowball Spawned");
		Rowball r = Instantiate(instance.rowball,position,Quaternion.identity) as Rowball;
		r.color=color;
		return r;
	}
	
	//Point children (non-physical)
	public static LinkRenderer AddLink(Point start, Point end, SpringJoint2D springReference, Color color, bool render = true){
		LinkRenderer l = Instantiate(instance.linkRenderer,Vector3.zero,Quaternion.identity) as LinkRenderer;
		l.render=render;
		l.start=start; l.end=end;
		l.springReference=springReference;
		l.color=color;
		
		l.transform.parent=start.transform;
		l.gameObject.name="Link"+(end.invisible?" (Dummy)":"");
		l.initialized=true;
		return l;
	}
	public static Oar AddOar(Point parent, bool left){
		Oar o = Instantiate(instance.oar,parent.position,Quaternion.identity) as Oar;
		o.transform.parent=parent.transform;
		o.left=left;
		o.gameObject.name="Oar ("+(left?"Left)":"Right)");
		return o;
	}
	public static CannonRevamped AddCannon(Point parent, Color color){
		Vector3 cannonPosition = new Vector3(parent.position.x,parent.position.y,instance.cannon.transform.position.z);
		CannonRevamped c = Instantiate(instance.cannon,cannonPosition,Quaternion.identity) as CannonRevamped;
        c.transform.parent = parent.transform;
        c.color=color;
		c.gameObject.name="Cannon";
		return c;
	}
	public static Deck AddDeck(Point parent){
		Vector3 position = new Vector3(parent.position.x,parent.position.y,0);
		Deck d = Instantiate(instance.deck,position,Quaternion.identity) as Deck;
		d.transform.parent=parent.transform;
		d.gameObject.name="Deck";
		return d;
	}
}