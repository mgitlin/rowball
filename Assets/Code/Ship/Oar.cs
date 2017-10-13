//Oar.cs
//Created on 19.03.16 by Aaron C Gaudette
//Oar point attachment

using UnityEngine;
using System.Collections;
[ExecuteInEditMode]

public class Oar : MonoBehaviour{
	public enum RenderMode{LINE,SPRITE,ANIM};
	public RenderMode renderMode = RenderMode.LINE;
	public bool legacy{get{return renderMode==RenderMode.LINE;}}
	new public bool animation{get{return renderMode==RenderMode.ANIM;}}
	public bool tapAnimation = true;
	public float animationFudge = 0.0001f;
	
	[Range(0,1)] public float load = 0; //Animation x
	public Vector2 range = new Vector2(-0.6f,0.4f); //Animation range (oar offset)
	public float length = 8, offset = 2; //Oar length and offset from edge
	public bool left = true; //Direction
	
	public float damping = 10;
	
	[HideInInspector] public Vector3 heading; //Passed down the chain
	float lastLoad = 0;
	
	//Reference
	LineRenderer lineRenderer;
	SpriteAnimator animator;
	void Start(){
		if(legacy){
			lineRenderer=GetComponent<LineRenderer>();
			Update();
		}
		if(animation)animator=GetComponent<SpriteAnimator>();
		//Observe
		if(!Application.isEditor || Application.isPlaying){
			Observer.OnPaddle.AddListener(OnPaddle);
		}
	}
	
	//need to rework for variable paddling
	//get load var and play at that
	
	void OnPaddle(ShipRevamped s, bool l){
		bool ship = s.gameObject==transform.parent.GetComponent<Point>().handler; //hack, fix!
		if(ship && ((l && left) || (!l && !left))){ //simplify boolean
			animator.Play("Paddle");
		}
		if(tapAnimation)StartCoroutine(Brake(s,l)); //multithreading?
	}
	IEnumerator Brake(ShipRevamped s, bool l){
		while((l?s.leftIn.input:s.rightIn.input)==0){
			//hm...
			yield return 0;
		}
		while((l?s.leftIn.input:s.rightIn.input)>0){
			if(animator.state!="Paddle")animator.Play("Brake");
			yield return 0;
		}
		if(animator.state=="Brake")animator.Play("Default");
	}
	void Update(){
		Vector3 target;
		//Lerp load value
		float lerpLoad = Mathf.Lerp(lastLoad,load,Time.deltaTime*damping);
		lastLoad=lerpLoad;
		
		//Manual animation
		if(!animation){
			//Update static line
			if(legacy){
				lineRenderer.SetPosition(0,new Vector3(-length/2+(left?-offset:offset),0,0));
				lineRenderer.SetPosition(1,new Vector3(length/2+(left?-offset:offset),0,0));
			}
			
			//Animate look at (rotation) between two points, flip for direction
			target=transform.position+heading+
				new Vector3(heading.y,-heading.x)*Mathf.Lerp(left?range.x:-range.x,left?range.y:-range.y,lerpLoad);
		}
		else target=transform.position+heading;
		
		//Orient
		if(legacy)transform.LookAt(target,-Vector3.forward);
		else{
			transform.LookAt(target,-Vector3.forward);
			transform.Rotate(90,0,0); //Correct manually
			GetComponent<SpriteRenderer>().flipX=!left;
		}
		
		//Load animation vs. state animation
		if(!tapAnimation)animator.Set("Default",lerpLoad+animationFudge);
	} 
}