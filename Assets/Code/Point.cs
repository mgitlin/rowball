//Point.cs
//Created on 18.03.16 by Aaron C Gaudette
//Physical point in space, with links (to other points) and other attachments

using UnityEngine;
using System.Collections.Generic;
[ExecuteInEditMode]
[RequireComponent(typeof(Rigidbody2D))]

//add new link with editor option

public class Point : MonoBehaviour{
	public bool useSprite = false;
	public bool overrideColor = false;

	public int mass = 2; //Could represent with size if necessary
	public float drag = 0.5f;
	public Color color = Color.white;
	public bool invisible = false; //No render
	
	public Color flashColor = Color.red; //Should move to module (?)

	[Header("Read-only")]
	//Attachments
	[HideInInspector] public Oar oar;
	public bool hasOar{get{return oar!=null;}}
	[HideInInspector] public CannonRevamped cannon;
	public bool hasCannon{get{return cannon!=null;}}
	[HideInInspector] public Deck deck;
	public bool hasDeck{get{return deck!=null;}}
	[HideInInspector] float frozenTimeStamp = -1;

	public GameObject handler; //Is this point managed by a dynamic?

	List<LinkRenderer> links = new List<LinkRenderer>();
	public int linkCount{get{return links.Count;}}

	int stiffnessConstant = 24; //Stiffness input multiplier

	const int ignoreCollideLayer = 31;

//	[HideInInspector] Color origMaterialColor = Color.white;

	public float flashFactor = 100f / 7.5f;

	//Cache components
	SpriteRenderer spriteRenderer;
	void Start(){
		spriteRenderer=GetComponent<SpriteRenderer>();
		transform.parent=ObjectManager.pointContainer; //Move to container
		Update();
	}
	Rigidbody2D _rbody;
	Rigidbody2D rbody{get{
			if(_rbody==null)_rbody=GetComponent<Rigidbody2D>();
			return _rbody;
		}}
	void Update(){
		rbody.mass=mass;
		rbody.drag=drag;

		if(!overrideColor)spriteRenderer.color=color;
		spriteRenderer.enabled=!invisible;
	}

	public Vector2 position{get{return transform.position;}}
	public Vector2 velocity{
		get{return rbody.velocity;}
		set{rbody.velocity=value;}
	}

	//Forces
	public void AddForce(Vector2 f){rbody.AddForce(f,ForceMode2D.Force);}
	public void AddImpulse(Vector2 f){rbody.AddForce(f,ForceMode2D.Impulse);}
	public void AddImpulse(Vector2 f, Vector2 position){rbody.AddForceAtPosition(f,position,ForceMode2D.Impulse);}

	public void UpdateData(int mass, float drag, Color color, float frozenTimestamp){
		this.mass=mass;
		this.drag=drag;
		this.color=color;
		this.frozenTimeStamp = frozenTimestamp;
	}

	//Links
	public LinkRenderer AddLink(Point other, float distance, float stiffness, float damping, Color color, bool render = true){
		//Instantiate spring joint
		SpringJoint2D spring = gameObject.AddComponent<SpringJoint2D>() as SpringJoint2D;
		spring.autoConfigureDistance=false;
		spring.enableCollision=false;
		spring.connectedBody=other.GetComponent<Rigidbody2D>();
		spring.distance=distance;
		spring.frequency=stiffness*stiffnessConstant;
		spring.dampingRatio=damping;
		//Link renderer
		LinkRenderer l = ObjectManager.AddLink(this,other,spring,color,render);
		links.Add(l);
		return l;
	}
	public void UpdateLinks(float stiffness, float damping, Color color){
		foreach(LinkRenderer l in links){
			l.springReference.frequency=stiffness*stiffnessConstant;
			l.springReference.dampingRatio=damping;
			l.color=color;
		}
	}
	//Remove first link
	public bool RemoveLink(){return RemoveLink(true);}
	public bool RemoveLink(bool useDummy){return RemoveLink(0,useDummy);}
	//Remove all links
	public void RemoveLinks(){RemoveLinks(true);}
	public void RemoveLinks(bool dummy){for(int i=0;i<links.Count;++i)RemoveLink(i,dummy);}
	//Remove specific link
	public bool RemoveLink(Point other){return RemoveLink(other,true);}
	public bool RemoveLink(Point other, bool useDummy){
		int linkIndex = -1;
		for(int i=0;i<links.Count;++i)if(links[i].end==other){
				linkIndex=i;
				break;
			}
		if(linkIndex==-1)return false;
		return RemoveLink(linkIndex,useDummy);
	}
	public bool RemoveLink(int index){return RemoveLink(index,true);}
	public bool RemoveLink(int index, bool useDummy){
		if(links.Count<index)return false;

		//Dummy
		if(useDummy){
			SpringJoint2D spring = links[index].springReference;
			Point p = ObjectManager.AddPoint(links[index].end.position,links[index].end.mass,links[index].end.color);
			p.invisible=true;
			p.gameObject.layer=ignoreCollideLayer; //Set collision layer
			LinkRenderer l = AddLink(p,spring.distance,spring.frequency/stiffnessConstant,spring.dampingRatio,links[index].color);
			l.render=links[index].render;
		}

		Destroy(links[index].springReference);
		links.RemoveAt(index);
		return true;
	}

	//Oar, cannon
	public void AddOar(bool left){
		if(oar==null && cannon==null)oar=ObjectManager.AddOar(this,left);
	}
	public void UpdateOar(bool left, Vector3 heading){
		if(oar!=null){
			oar.left=left;
			oar.heading=heading;
			if(frozenTimeStamp>0){
				if (((int)(Time.time * flashFactor)) % 2 == 1) oar.GetComponent<SpriteRenderer> ().color = flashColor;
				else oar.GetComponent<SpriteRenderer> ().color = Color.white;
			} else {
				oar.GetComponent<SpriteRenderer> ().color = Color.white;
			}
		}
	}
	public void AddCannon(Color color){
		cannon=ObjectManager.AddCannon(this,color);
	}
	public void UpdateCannon(Color color, Vector3 heading){
		if(cannon!=null){
			cannon.color=color;
			cannon.heading = heading;
			if(frozenTimeStamp>0){
				if (((int)(Time.time * flashFactor)) % 2 == 1) cannon.GetComponent<SpriteRenderer> ().color = flashColor;
				else cannon.GetComponent<SpriteRenderer> ().color = Color.white;
			} else {
				cannon.GetComponent<SpriteRenderer> ().color = Color.white;
			}
		}
	}
	public void AddDeck(){
		deck=ObjectManager.AddDeck(this);
	}
	public void UpdateDeck(Vector3 heading){
		if(deck!=null)deck.heading=heading;
		if(frozenTimeStamp>0){
			if (((int)(Time.time * flashFactor)) % 2 == 1)
                deck.GetComponent<SpriteRenderer> ().color = flashColor;
            else
				deck.GetComponent<SpriteRenderer> ().color = Color.white;
		} else {
			deck.GetComponent<SpriteRenderer> ().color = Color.white;
		}
	}
}