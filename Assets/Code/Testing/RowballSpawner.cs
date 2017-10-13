//RowballSpawner.cs
//Created on 23.03.16 by Aaron C Gaudette
//Test script, spawns rowball at specified velocity

using UnityEngine;
using System.Collections;

public class RowballSpawner : MonoBehaviour{
	public bool spawn = false; //Editor toggle
	public float spawnRate = 0; //Auto-spawning
	
	public Vector2 direction = -Vector2.up;
	public float speed = 128;
	public Color color = Color.red;
	
	bool spawning = false;
	
	void Update(){
		if(spawn){
			Spawn();
			spawn=false;
		}
		if(!spawning && spawnRate>0){
			spawning=true;
			StartCoroutine(Spawn(spawnRate));
		}
	}
	void Spawn(){
		Rowball r = ObjectManager.AddRowball(transform.position,color);
		r.velocity=direction*speed;
		//r.level=2;
		spawn=false;
	}
	IEnumerator Spawn(float rate){
		Spawn();
		yield return new WaitForSeconds(rate);
		if(spawnRate>0)StartCoroutine(Spawn(spawnRate));
		else spawning=false;
	}
}