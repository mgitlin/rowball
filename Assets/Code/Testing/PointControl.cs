//PointControl.cs
//Created by Aaron C Gaudette on 18.03.16
//Testing script for WASD point control using forces

using UnityEngine;

public class PointControl : MonoBehaviour{
	public float strength = 8.0f;
	bool up, down, left, right;
	
	void Update(){
		up=(Input.GetKey(KeyCode.W));
		down=(Input.GetKey(KeyCode.S));
		left=(Input.GetKey(KeyCode.A));
		right=(Input.GetKey(KeyCode.D));
	}
	void FixedUpdate(){
		if(up)GetComponent<Point>().AddForce(Vector2.up*strength);
		if(down)GetComponent<Point>().AddForce(Vector2.down*strength);
		if(left)GetComponent<Point>().AddForce(Vector2.left*strength);
		if(right)GetComponent<Point>().AddForce(Vector2.right*strength);
	}
}