//Deck.cs
//Created on 11.04.16 by Aaron C Gaudette
//Deck point attachment

using UnityEngine;
[ExecuteInEditMode]
[RequireComponent(typeof(SpriteRenderer))]

public class Deck : MonoBehaviour{
	[HideInInspector]
    public Vector3 heading; //Passed down the chain

	public Vector2 position{get{return transform.position;}}

	void Start(){
		if(GetComponent<TrailRenderer>()!=null)
			GetComponent<TrailRenderer>().sortingLayerName="WorldFX";
	}

	void Update(){
		//Orientation
		if(!Application.isEditor || Application.isPlaying){
			transform.LookAt(transform.position+heading,-Vector3.forward);
			transform.Rotate(90,0,0); //Correct manually
		}
	}
}