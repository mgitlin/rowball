//LinkRenderer.cs
//Created on 19.03.16 by Aaron C Gaudette
//Manages visual for spring joints

using UnityEngine;
[ExecuteInEditMode]
[RequireComponent(typeof(LineRenderer))]

public class LinkRenderer : MonoBehaviour{
	public Color color = Color.white;
	//Read-only
	public bool render;
	public Point start, end;
	[HideInInspector] public SpringJoint2D springReference;
	[HideInInspector] public bool initialized = false;
	
	//Cache components
	LineRenderer lineRenderer;
	void Start(){
		lineRenderer=GetComponent<LineRenderer>();
		Update();
	}
	
	void Update(){
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
		//Render
		lineRenderer.enabled=render && start && end;
		if(start && end){
			lineRenderer.SetPosition(0,start.position);
			lineRenderer.SetPosition(1,end.position);
		}
		//Destroy if spring is destroyed
		if(springReference==null && initialized){
			if(Application.isEditor)DestroyImmediate(gameObject);
			else Destroy(gameObject);
		}
	}
}