//CollisionBounds.cs
//Created on 01.04.16 by Aaron C Gaudette
//Handles bounds collision, passes it up to ship or breakable

using UnityEngine;

public class CollisionBounds : MonoBehaviour{
	void OnTriggerEnter2D(Collider2D c){transform.parent.SendMessage("ProcessCollision",c);}
	void OnTriggerExit2D(Collider2D c){transform.parent.SendMessage("ProcessExit",c);}
}
