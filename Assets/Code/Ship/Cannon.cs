//Cannon.cs
//Created on 19.03.16 by Aaron C Gaudette
//Cannon point attachment

using UnityEngine;
using System.Collections;
[ExecuteInEditMode]

public class Cannon : MonoBehaviour{
	public bool useSprite = false;
	
	public Color color = Color.white;
	public int load = 0; //Animation x
	//public bool left = false;
	
	public Vector2 range = new Vector2(5,20); //Animation scale
	public float randomOffset = 3; //Random animation scale
	
	public Transform fx; //Animation object

    public Sprite[] frames;

	[HideInInspector] public Vector3 heading; //Passed down the chain
    SpriteRenderer spriteRenderer;

	public Vector2 position{get{return transform.position;}}
	//Vector3 to include Z depth
	public Vector3 position3{
		get{return transform.position;}
		set{transform.position=value;}
	}
	
	//Reference
	LineRenderer[] lines;
	void Start(){
        if (!useSprite)
        {
            lines = GetComponentsInChildren<LineRenderer>();
            Update();
        }
        else
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }
	
	void Update(){
		//Orientation
		if(!Application.isEditor || Application.isPlaying){
			transform.LookAt(transform.position+heading,-Vector3.forward);
			transform.Rotate(90,0,0); //Correct manually
		}
		
		if(!useSprite){
            //Render
            foreach (LineRenderer l in lines)
            {
                l.startColor = color;
                l.endColor = color;
            }
		}
		
		//Animation
		if(load>0){
			float size = Mathf.Lerp(range.x,range.y,load);
			size+=Random.Range(-randomOffset,randomOffset);
			fx.localScale=new Vector3(size,size,0);

            int f = (int)(((((float)load) / (10.0f)) * (float)(frames.Length - 1)));
            Debug.Log(f);
            spriteRenderer.sprite = frames[f];
        }
		else fx.localScale=Vector3.zero;
    }
}