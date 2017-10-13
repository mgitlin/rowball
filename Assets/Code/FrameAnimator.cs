//FrameAnimator.cs
//Created on 22.04.16 by Aaron C Gaudette
//Loops a sprite animation

//Legacy!

using UnityEngine;

public class FrameAnimator : MonoBehaviour{
	public float fps = 144;
	public Sprite[] frames;
	public bool randomOffset = true;
	
	SpriteRenderer r = null;
	int frame = 0;
	int offset = 0;
	
	void Start(){if(randomOffset)offset=Random.Range(0,frames.Length);}
	void Update(){
		if(r==null)r=GetComponent<SpriteRenderer>();
		frame=(int)((Time.time*fps)+offset)%frames.Length;
		r.sprite=frames[frame];
	}
}