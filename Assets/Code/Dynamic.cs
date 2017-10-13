//Dynamic.cs
//Created on 01.04.16 by Aaron C Gaudette
//Parent class for "collection of point" classes

using UnityEngine;

public class Dynamic : MonoBehaviour{
	public DynamicModule module;
	
	[Header("Read-only")]
	public float mass = 0;
	public bool totaled = false;
	
	[HideInInspector] public Point[] points;
}