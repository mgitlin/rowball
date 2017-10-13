//SpriteAnimator.cs
//Created on 29.04.16 by Aaron C Gaudette
//Dynamically animates a sprite through collections of frames (states)

using UnityEngine;
using System.Collections;

public class SpriteAnimator : MonoBehaviour{
	[System.Serializable]
	public class State{
		public string name = "Default";
		
		public string target = "";
		public bool loop = false;
		public bool play = true;
		
		public float fps = 144;
		public Sprite[] frames;
		
		public bool randomOffset = false;
		
		[HideInInspector] public int frame = 0;
		[HideInInspector] public int offset = 0;
		
		public void Init(){
			if(randomOffset)offset=Random.Range(0,frames.Length);
		}
	}
	public State[] states = new State[1]; //Using this (vs. a dictionary) for transparency (in the inspector)
	public string startingState = "Default";
	
	[Header("Read-only:")]
	public string state;
	public int currentFrame = 0;
	float initTime = 0;
	
	SpriteRenderer r = null;
	void Start(){
		foreach(State s in states)s.Init();
		Play(startingState);
	}
	
	public void Play(string s){
		state=s;
		initTime=Time.time;
	}
	public void Set(string s, float p){
		foreach(State check in states)if(check.name==s){
			check.frame=(int)(p*(check.frames.Length-1));
			return;
		}
		Debug.Log("Animation not found");
	}
	public void Set(string s, int f){
		foreach(State check in states)if(check.name==s){
			check.frame=f;
			return;
		}
		Debug.Log("Animation not found");
	}
	
	void Update(){
		//Get renderer
		if(r==null)r=GetComponent<SpriteRenderer>();
		
		//Animate active state
		foreach(State s in states)if(s.name==state){
			if(s.play){
				//Compute frame by fps and start time
				int frame = 0;
				float f = (Time.time-initTime)*s.fps;
				if(s.loop)frame=(int)(f+s.offset)%s.frames.Length;
				else{
					frame=(int)Mathf.Min(f,s.frames.Length-1);
					//End one-shot and target new state
					if(frame==s.frames.Length-1 && s.target!="")Play(s.target);
				}
				s.frame=frame;
			}
			r.sprite=s.frames[s.frame];
			//Read-only
			currentFrame=s.frame;
			break;
		}
	}
}