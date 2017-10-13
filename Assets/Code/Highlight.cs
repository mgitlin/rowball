//Highlight.cs
//Created on 22.04.16 by Aaron C Gaudette
//Animates/toggles highlight for leading ship

//Should probably be in Point/Rowball

using UnityEngine;

public class Highlight : MonoBehaviour{
	public bool isShip = true;
	public float speedThreshold = 64;
	public bool shake = true;
	public float shakeAmount = 0.8f;
	
	public Vector2 range = new Vector2(0.9f,1.1f);
	public float speed = 1;
	
	ShipRevamped handler = null;
    new SpriteRenderer renderer;
	float cachedScale;
	void Start(){
		if(isShip){
			Point p = transform.parent.parent.GetComponent<Point>();
			if(p.handler!=null)handler=p.handler.GetComponent<ShipRevamped>();
		}
		cachedScale=transform.localScale.x;
	}
	void Update(){
		if(renderer==null)renderer=GetComponent<SpriteRenderer>();
		if(isShip)renderer.enabled=PlayerManager.leader==PlayerManager.GetPlayer(handler);
		else renderer.enabled=transform.parent.GetComponent<Rowball>().speed<speedThreshold;
		
		//Animate
		float alpha=Mathf.Lerp(range.x,range.y,0.5f+0.5f*Mathf.Sin(Time.time*speed));
		
		if(!isShip)alpha*=1-(transform.parent.GetComponent<Rowball>().speed/speedThreshold);
		
		//
		float scale = Random.Range(cachedScale*shakeAmount,cachedScale);
		if(shake)transform.localScale=new Vector3(scale,scale,scale);
		
		//transform.localScale=new Vector3(scale,scale,scale);
		renderer.color=new Color(renderer.color.r,renderer.color.g,renderer.color.b,alpha);
		
		//Color
		if(isShip)renderer.material.color=PlayerManager.TeamColor(PlayerManager.GetPlayer(handler));
	}
}