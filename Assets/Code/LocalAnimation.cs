//LocalAnimation.cs
//Created on 22.04.16 by Aaron C Gaudette
//Brute-force offsets animation to local position

using UnityEngine;

public class LocalAnimation : MonoBehaviour{
	Vector3 cache;
	void Start(){cache=transform.position;}
	void LateUpdate(){transform.position+=cache;}
}