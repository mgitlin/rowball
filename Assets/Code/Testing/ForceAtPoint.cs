//ForceAtPoint.cs
//Created on 19.03.16 by Aaron C Gaudette
//Testing script for force on a rigidbody

using UnityEngine;

public class ForceAtPoint : MonoBehaviour{
	public int amount = 512;
	
	Vector2 point;
	bool input = false;
	
	void Update(){
		point+=new Vector2(Input.GetAxis("Horizontal")*24*Time.deltaTime,Input.GetAxis("Vertical")*10*Time.deltaTime);
		Debug.DrawLine(point,point+Vector2.up*8,Color.red);
		input=Input.GetKey(KeyCode.Space);
	}
	void FixedUpdate(){
		if(input)GetComponent<Rigidbody2D>().AddForceAtPosition(Vector2.up*amount,point);
	}
}