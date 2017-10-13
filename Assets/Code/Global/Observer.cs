//Observer.cs
//Created on 02.04.16 by Aaron C Gaudette
//

using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class Observer : MonoBehaviour{
	//Singleton
	static Observer observer = null;
	static Observer instance{get{
		if(observer==null){
			observer=(Observer)FindObjectOfType(typeof(Observer));
			if(FindObjectsOfType(typeof(Observer)).Length>1)Debug.Log("More than one Observer instance in the scene!");
		}
		return observer;
	}}

	public class ShipRowballEvent : UnityEvent<ShipRevamped, Rowball>{}
	public class ShipEvent : UnityEvent<ShipRevamped> {}
	public class ShipVector2Event : UnityEvent<ShipRevamped,Vector2>{}
	public class ShipBoolEvent : UnityEvent<ShipRevamped, bool>{}
    public class CannonEvent : UnityEvent<CannonRevamped> { }
	public class VoidEvent : UnityEvent{}

	//Track when a kill occurs, the killing rowball and the ship target
	ShipRowballEvent onKill = new ShipRowballEvent();
	public static ShipRowballEvent OnKill{get{return instance.onKill;}}

	//Track when a goal is scored, with the scoring ship
	ShipEvent onGoal = new ShipEvent();
	public static ShipEvent OnGoal{get{return instance.onGoal;}}

	//Track when a fire event has started (button is pressed down)
	ShipEvent onFireBegin = new ShipEvent();
	public static ShipEvent OnFireBegin{get{return instance.onFireBegin;}}

    //Track when a cannon fires
    CannonEvent onFire = new CannonEvent();
	public static CannonEvent OnFire {get{return instance.onFire;}}

    //Track when a ship respawns
    VoidEvent onRespawnAfterSuicide = new VoidEvent();
    public static VoidEvent OnRespawnAfterSuicide { get{ return instance.onRespawnAfterSuicide; }}

	//Track when a game ends
	//VoidEvent onWin = new VoidEvent();
	//public static VoidEvent OnWin{get{return instance.onWin;}}
	
	//Track when a ship paddles
	ShipBoolEvent onPaddle = new ShipBoolEvent();
	public static ShipBoolEvent OnPaddle{get{return instance.onPaddle;}}
	
	//Special kills
	VoidEvent onDoubleKill = new VoidEvent();
	public static VoidEvent OnDoubleKill{get{return instance.onDoubleKill;}}
	VoidEvent onTripleKill = new VoidEvent();
	public static VoidEvent OnTripleKill{get{return instance.onTripleKill;}}
	VoidEvent onLeaderKill = new VoidEvent();
	public static VoidEvent OnLeaderKill{get{return instance.onLeaderKill;}}
	VoidEvent onSuicide = new VoidEvent();
	public static VoidEvent OnSuicide{get{return instance.onSuicide;}}
}