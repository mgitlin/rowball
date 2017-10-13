//ShipModule.cs
//Created on 20.03.16 by Aaron C Gaudette
//Container class for common (non-instance) ship data

using UnityEngine;

[System.Serializable]
public class ShipModule : MonoBehaviour{
	public enum ControlType{LEGACY,REDONE,EXP};
	[Header("Input")]
	public ControlType controlType = ControlType.LEGACY;
	
	//Time to fully charge
	public float oarSpeed = 2.5f;
	public float cannonSpeed = 1.4f;
	public AnimationCurve oarFalloff = AnimationCurve.Linear(0,0,1,1); //Curve for actual forces
	public AnimationCurve oarVisualFalloff = AnimationCurve.Linear(0,0,1,1); //Curve for animation
	public AnimationCurve cannonFalloff = AnimationCurve.Linear(0,0,1,1);
	
	[Space(8)]
	public float oarStrength = 600;
	[Range(0,1)] public float turnBoost = 0;
	[Range(0,1)] public float turnBoostLimit = 0.1f;
	public float pivotBoost = 0;
	[Range(0,1)] public float dashBoost = 0;
	public float inputDecay = 6;
	public float inputCooldown = 1;
	
	public float firepower = 2500;
	public float recoil = 1500;
	[Range(0,1)] public float minCharge = 0.1f;
	
	public Vector2 angularDrag = new Vector2(0,16);
	public AnimationCurve dragFalloff = AnimationCurve.Linear(0,0,1,1);
	public Vector2 speedFactorRange = new Vector2(0,300);
	public AnimationCurve speedFactor = AnimationCurve.Linear(0,1,1,1);
	public float brakeFactor = 5f;				// Factor for changing up how much the ship will brake
	
	[Header("Cannon")]
	[Range(0,1)] public float dryFire = 0.26f; //Multiplier for firing without a rowball
	public int forcePushRadius = 48;
	public int pushForce = 1800;
	public AnimationCurve forcePushCurve = AnimationCurve.Linear(0,0,1,1);
	public float rowballOffset = 14; //Cannon length
	public Vector2 chargeLevels = new Vector2(0.5f,0.75f);
	public float spinBound = 256;
	[Range(0,1)] public float volleyWidth = 0.6f; //Width to capture a volley
	public double volleyLength = 0.6; 			// Duration of volley
	public float volleySpeed = 2.0f; 			// How fast a volley will be
	public float stuckMudDuration = 3; 			// How long ship is stuck in mud before it can move more freely
	public float freezeShipDuration = 3;        // How long to freeze ship when overcharged
	public float cannonMaxChargeDuration = 3;   // How long a ship can charge for before being frozen
	
	[Header("Rowball")]
	public float pickupReload = 0.3f; //Time between pickups
	public float rowballSpriteDamping = 30; //Collection speed
	
	[Header("Random")]
	public int maxVertices = 9;
	public Vector2 randomSize = new Vector2(11,32);
	public bool simplePolygon = true;
	public float vertexCenterOffset = 0.65f;
	public float minVertexDistance = 0.65f;
	public float collinearFudge = 0.4f, segmentFudge = 0.3f;
	public int dropIterations = 16384;
	
	public bool legacy{get{return controlType==ControlType.LEGACY;}}
	public bool redone{get{return controlType==ControlType.REDONE;}}
	public bool experimental{get{return controlType==ControlType.EXP;}}
}