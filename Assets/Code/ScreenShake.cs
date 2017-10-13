//ScreenShake.cs
//Created on 7.04.16 by Aaron C Gaudette
//

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScreenShake : MonoBehaviour{
	//Editor classes
	[System.Serializable]
	public class ShakeData{
		public float amount = 16, speed = 0.01f, damping = 24;
	}
	public ShakeData cannonShake;
	[System.Serializable]
	public class RumbleData{
		public float amount = 16, speed = 0.01f, damping = 24, time = 1;
	}
	public RumbleData killShake;
	
	[System.Serializable]
	public class BumpData{
		public float delay = 0.05f, punchDamping = 32, releaseDamping = 2;
	}
	public BumpData shotBump;
	[System.Serializable]
	public class SlomoData{
		public float delay = 0, factor = 0.5f, time = 1;
	}
	public SlomoData killSlomo;
	
	public Color day, night, sunset;
	public float colorDamping = 2;
	
	//Layer camera shake effects
	class ShakeLayer{
		public float amount, speed, damping;
		public float lastTime;
		public Vector2 lastPosition, target;
		
		public ShakeLayer(float amount, float speed, float damping){
			this.amount=amount;
			this.speed=speed;
			this.damping=damping;
			lastTime=0;
			lastPosition=target=Vector2.zero;
		}
		
		public Vector3 Compute(float time){
			if(time>lastTime+speed){
				target=new Vector2(Random.Range(-amount,amount),Random.Range(-amount,amount));
				lastTime=time;
			}
			Vector2 lerp = Vector2.Lerp(lastPosition,target,Time.deltaTime*damping);
			lastPosition=lerp;
			return new Vector3(lerp.x,lerp.y,0);
		}
	}
	List<ShakeLayer> shakeLayers;
	
	class BumpLayer{
		public float delay, punchDamping, releaseDamping;
		public Vector2 lastPosition, target;
		public float startTime;
		
		public BumpLayer(Vector2 force, float delay, float punchDamping, float releaseDamping, float startTime){
			this.delay=delay;
			this.punchDamping=punchDamping;
			this.releaseDamping=releaseDamping;
			lastPosition=Vector2.zero;
			target=force;
			this.startTime=startTime;
		}
		public Vector3 Compute(float time){
			float damping=punchDamping;
			if(time>startTime+delay){
				target=Vector2.zero;
				damping=releaseDamping;
			}
			Vector2 lerp = Vector2.Lerp(lastPosition,target,Time.deltaTime*damping);
			lastPosition=lerp;
			return new Vector3(lerp.x,lerp.y,0);
		}
	}
	List<BumpLayer> bumpLayers;
	
	//Observe
	void OnFireBegin(ShipRevamped s){
		StartCoroutine(CannonShake(s));
	}
	IEnumerator CannonShake(ShipRevamped s){
		while(s.cannon.charge==0)yield return 0; //Wait for input
		
		ShakeLayer l = new ShakeLayer(cannonShake.amount,cannonShake.speed,cannonShake.damping);
		shakeLayers.Add(l);
		while(s.cannon.charge>0 && s!=null)yield return 0;
		
		//Wait to clear
		l.amount=0;
		while(l.lastPosition!=Vector2.zero)yield return 0;
		shakeLayers.Remove(l);
	}
	
	void OnKill(ShipRevamped targetShip, Rowball rowball){
		Rumble(killShake.amount,killShake.speed,killShake.damping,killShake.time);
		Slomo(killSlomo.factor,killSlomo.time,killSlomo.delay);
		ColorShift(night,day,killSlomo.time,killSlomo.delay);
	}
	
	void Rumble(float amount, float speed, float damping, float time){
		StartCoroutine(OneShotShake(amount,speed,damping,time));
	}
	IEnumerator OneShotShake(float amount, float speed, float damping, float time){
		float startingTime = time, startingAmount=amount;
		ShakeLayer l = new ShakeLayer(amount,speed,damping);
		shakeLayers.Add(l);
		while(l.amount>0){
			yield return 0;
			time-=Time.deltaTime;
			l.amount=startingAmount*(time/startingTime);
		}
		//Wait to clear
		l.amount=0;
		while(l.lastPosition!=Vector2.zero)yield return 0;
		shakeLayers.Remove(l);
	}
	void Slomo(float factor, float time, float delay = 0){
		StartCoroutine(StartSlomo(factor,time,delay));
	}
	IEnumerator StartSlomo(float factor, float time, float delay){
		yield return new WaitForSeconds(delay); //Delay at constant time
		
		//Have as many layers of slomo as you desire
		Time.timeScale*=factor;
		Time.fixedDeltaTime*=factor;
		yield return new WaitForSeconds(time*Time.timeScale); //Keep time running smoothly
		Time.fixedDeltaTime=cachedTimestep;
		Time.timeScale=1;
	}
	//hacked public!
	public void ColorFade(Color to, float time){StartCoroutine(Fade(to,time));}
	IEnumerator Fade(Color to, float time){
		Color cachedColor = targetColor;
		float start = Time.time;
		while(Time.time<start+time){
			targetColor=Color.Lerp(cachedColor,to,(Time.time-start)/time);
			yield return 0;
		}
	}
	
	void ColorShift(Color to, Color cycle, float time = 0, float delay = 0){
		StartCoroutine(Shift(to,cycle,time,delay));
	}
	IEnumerator Shift(Color to, Color cycle, float time, float delay){
		yield return new WaitForSeconds(delay); //Delay at constant time
		
		targetColor=to;
		yield return new WaitForSeconds(time*Time.timeScale); //Keep time running smoothly
		targetColor=cycle;
	}
	
	void OnFire(CannonRevamped c/*, Vector2 recoil*/){
		//Bump(recoil*2,shotBump.delay,shotBump.punchDamping,shotBump.releaseDamping,Time.time);
	}
	void Bump(Vector2 force, float delay, float punchDamping, float releaseDamping, float startTime){
		StartCoroutine(Punch(force,delay,punchDamping,releaseDamping,startTime));
	}
	IEnumerator Punch(Vector2 force, float delay, float punchDamping, float releaseDamping, float startTime){
		BumpLayer b = new BumpLayer(force,delay,punchDamping,releaseDamping,startTime);
		bumpLayers.Add(b);
		while(b.lastPosition==Vector2.zero)yield return 0;
		while(b.lastPosition!=Vector2.zero)yield return 0;
		bumpLayers.Remove(b);
	}
	
	Camera thisCamera;
	float cachedTimestep;
	Color cachedColor, targetColor;
	
	void Start(){
		cachedTimestep = Time.fixedDeltaTime;
		thisCamera=GetComponent<Camera>();
		cachedColor=thisCamera.backgroundColor;
		targetColor=cachedColor;
		
		shakeLayers=new List<ShakeLayer>();
		bumpLayers=new List<BumpLayer>();
		
		//Observe
		if(!Application.isEditor || Application.isPlaying){
			Observer.OnFireBegin.AddListener(OnFireBegin);
			Observer.OnKill.AddListener(OnKill);
			Observer.OnFire.AddListener(OnFire);
		}
	}
	
	void Update(){
		//if(Input.GetKeyDown(KeyCode.LeftShift))Bump(Vector2.left*1024,shotBump.delay,shotBump.punchDamping,shotBump.releaseDamping,Time.time);
		//if(Input.GetKeyDown(KeyCode.RightShift))OnKill(null,null);
		
		//Apply layers
		transform.localPosition=new Vector3(0,0,transform.position.z);
		foreach(ShakeLayer s in shakeLayers)
			transform.localPosition+=s.Compute(Time.time);
		foreach(BumpLayer b in bumpLayers)
			transform.localPosition+=b.Compute(Time.time);
		
		//Color animation
		thisCamera.backgroundColor=Color.Lerp(thisCamera.backgroundColor,targetColor,Time.deltaTime*colorDamping);
	}
}