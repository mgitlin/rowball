//Rowball.cs
//Created on 19.03.16 by Aaron C Gaudette
//Game ball

using UnityEngine;
[ExecuteInEditMode]
[RequireComponent(typeof(Rigidbody2D))]

public class Rowball : MonoBehaviour{
	public int mass = 2;
	
	public Vector3 dragLevels = new Vector3(0.22f,0.15f,0.08f);
	public Vector3 bounceLevels = new Vector3(0.7f, 0.55f, 0.1f);
	public Color mediumColor = Color.yellow, highColor = Color.red;
	
	public float decayTime = 2;
	public float spinAmount = 1024;
	public float fullSpinSpeed = 1000;
	public AnimationCurve spinCurve = AnimationCurve.Linear(0,0,1,1);
	
	//Frames
	public Sprite[] frames;
    int currentFrame;

	[Header("Read-only")]
	public float level = 0;
	public float spin = 0;
	public Color color = Color.white;
	public ShipRevamped shooter = null; //Track who fired the rowball last
	public bool clearedShot = false; //Has the rowball moved clear of the shooter? (Fixes false suicides)
	
	//Keeping track of shot position for scoring purposes
	[HideInInspector]public Vector3 shotPosition;
	[HideInInspector]public float shotTimer = 180;
	
	PhysicsMaterial2D mat = null;
	
	float lastDecay = 0;
	
	//Cache components
	SpriteRenderer spriteRenderer;
	void Start(){
		spriteRenderer=GetComponent<SpriteRenderer>();
		transform.parent=ObjectManager.rowballContainer; //Move to container
		lastDecay=Time.time;
		mat=new PhysicsMaterial2D(GetComponent<Collider2D>().sharedMaterial.name + " (Clone)");
		Update();
	}
	Rigidbody2D _rbody;
	Rigidbody2D rbody{get{
		if(_rbody==null)_rbody=GetComponent<Rigidbody2D>();
		return _rbody;
	}}
	void Update(){
		rbody.mass=mass;
		if(level>0 && Time.time>lastDecay+decayTime){
			level--;
			lastDecay=Time.time;
		}else{
			//Reset shooter of rowball
			//if(level<=0)shooter=null;
		}
		
		//Rotation
		float angle = Mathf.Atan2(velocity.normalized.y,velocity.normalized.x)*Mathf.Rad2Deg;
		transform.rotation=Quaternion.AngleAxis(angle,Vector3.forward);
		
		//Charge levels
		rbody.drag=level==0?dragLevels.x:level==1?dragLevels.y:dragLevels.z; //Drag
		
		Color chargeColor = level==0?Color.white:level==1?mediumColor:highColor;
		//spriteRenderer.color=chargeColor;
		if(!Application.isEditor || Application.isPlaying){
			GetComponent<TrailRenderer>().material.color=chargeColor; //Trail color
			//GetComponentInChildren<ParticleSystem>().startColor=chargeColor;
		}
		mat.bounciness=level==0?bounceLevels.x:level==1?bounceLevels.y:bounceLevels.z; //Bounce
		GetComponent<Collider2D>().sharedMaterial=mat;
        //Sprites
        //int f = ((velocity.magnitude/fullSpinSpeed));
        currentFrame = (int)level;
		spriteRenderer.sprite=frames[currentFrame];
		
		//
		if (!shotPosition.Equals(Vector3.zero)) {
			if (shotTimer <= 0) {
				shotPosition = Vector3.zero;
				shotTimer = 180;
			} else {
				shotTimer--;
			}
		}
	}
	void FixedUpdate(){
		//Spin
		Vector2 spinForce = new Vector2(velocity.normalized.y,-velocity.normalized.x).normalized; //Apply perpendicular
		float spinSpeed = spinCurve.Evaluate(velocity.magnitude/fullSpinSpeed);
		AddForce(spinForce*spin*spinAmount*spinSpeed);
	}

	public Vector2 position{get{return transform.position;}}
	public Vector2 velocity{
		get{return rbody.velocity;}
		set{rbody.velocity=value;}
	}
	public Vector2 RelativeVelocity(Vector2 other){return velocity-other;}
	public float speed{get{return velocity.magnitude;}}

	public void AddForce(Vector2 f){rbody.AddForce(f,ForceMode2D.Force);}
	public void AddImpulse(Vector2 f){rbody.AddForce(f,ForceMode2D.Impulse);}
}