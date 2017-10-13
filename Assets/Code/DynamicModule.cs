//DynamicModule.cs
//Created on 01.04.16 by Aaron C Gaudette
//Container class for common (non-instance) dynamic data

using UnityEngine;

[System.Serializable]
public class DynamicModule : MonoBehaviour{
	[Header("Components")]
	public int pointMass = 4;
	public float drag = 0.5f;
	[Range(0,1)] public float stiffness = 0.55f, damping = 0.6f;
	public int boundsForce = 32000;
	public int boundsDrag = 256;
	public int pointLayer = 10;
	
	[Header("Collision")]
	public float collisionBuffer = 4;
	public float damageSpeed = 330;
	public float stealSpeed = 80;
	public float rowballDropFactor = 0.6f;
	[Range(0,1)] public float edgeDestroyChance = 0.75f;
	[Range(0,1)] public float frameworkDestroyChance = 0.75f;
	public float minDestroyForce = 0.5f, maxDestroyForce = 1.35f; //Min force multiplier to apply on hit
	public int rowballLayer = 8;
	public float ramMultiplier = 10;
	public float ramScale = 8;
}
