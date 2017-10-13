//AnimationPlayer.cs
//Created on 22.04.16 by Aaron C Gaudette

using UnityEngine;

public class AnimationPlayer : MonoBehaviour{
	public string clip;
	public float speed = 0.5f;
	void Start(){
		Animator a = GetComponent<Animator>();
		a.speed=speed;
		a.Play(clip);
	}
}