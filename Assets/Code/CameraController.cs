//CameraController.cs
//Created on 18.03.16 by Aaron C Gaudette
//Controls game camera

using UnityEngine;
[ExecuteInEditMode]

public class CameraController : MonoBehaviour{
	//Singleton
	static CameraController cameraController = null;
	static CameraController instance{get{
		if(cameraController==null){
			cameraController=(CameraController)FindObjectOfType(typeof(CameraController));
			if(FindObjectsOfType(typeof(CameraController)).Length>1)Debug.Log("More than one CameraController instance in the scene!");
		}
		return cameraController;
	}}

	public static float zoom{get{return instance._zoom;}}
	public static void Reset(){instance.ResetPosition();}

	//Instance
	[Range(32,256)] public float _zoom = 96, winnerZoom = 32;
	public float damping = 1.8f, winnerDamping = 2.4f, zoomDamping = 1;
	[Range(0,1)] public float totaledFactor = 0.5f;
	public float targetSwapTime = 1;

	//Private
	bool trackMultiple;
	float endTime = 0;
//	int readyUpIndex = 0;

	Camera attachedCamera;
	void Start(){attachedCamera=transform.GetChild(0).GetComponent<Camera>();}

	void Update(){
		//Track multiple winners for anything but duels
		if(GameController.game != null)
			trackMultiple=GameManager.instance.scoreMode!=GameManager.ScoreMode.LMS;
		if(!Application.isEditor || Application.isPlaying)if(!trackMultiple)totaledFactor=1;

		//Zoom
		if(attachedCamera==null)attachedCamera=GetComponent<Camera>();
		attachedCamera.orthographicSize=_zoom;

		if((!Application.isEditor || Application.isPlaying) && GameController.game != null ){
			Vector2 averagePosition = Vector2.zero;
			PlayerManager.PlayerInfo[] winners = GameController.game.winners;
			PlayerManager.PlayerInfo[] players = PlayerManager.ActivePlayers();

			//Normal gameplay (position calculation)
			if(winners==null || !trackMultiple){
				//Get weight
				float totalWeight = 0;
				foreach(PlayerManager.PlayerInfo p in players)totalWeight+=(p.totaled?totaledFactor:1);
				//Get average position
				foreach(PlayerManager.PlayerInfo p in players){
					float totaledWeight = (p.totaled?totaledFactor:1)/totalWeight;
					averagePosition+=p.position*totaledWeight;
				}
			}
			//Zoom on winners
			else{
				Vector2 goalPos = Vector2.zero;
				//if(GameManager.instance.scoreMode==GameManager.ScoreMode.GOAL){
				//	Vector3 pos = WorldManager.GetGoal(PlayerManager.lastScoredGoal[0].player.team).transform.position;
				//	goalPos=new Vector2(pos.x,pos.y);
				//}

				Vector2 firstTarget = new Vector2(transform.position.x,transform.position.y);
				firstTarget=(GameManager.instance.scoreMode==GameManager.ScoreMode.GOAL)?
					goalPos:
					PlayerManager.lastTotaled!=null?PlayerManager.lastTotaled.position:firstTarget;

				Vector2 winnersPosition = Vector2.zero;
				foreach(PlayerManager.PlayerInfo p in winners)if(p!=null)winnersPosition+=p.position;
				winnersPosition/=winners.Length;

				if(endTime==0)endTime=Time.time;
				averagePosition=Time.time-endTime<targetSwapTime?firstTarget:winnersPosition;

				if(winners.Length<2)_zoom = Mathf.Lerp(_zoom,winnerZoom,Time.deltaTime*zoomDamping);
				damping=winnerDamping;
			}
			//Constrain
			if(winners==null || !trackMultiple)
				averagePosition=new Vector2(
					Mathf.Clamp(averagePosition.x,-WorldManager.bounds.x*0.5f,WorldManager.bounds.x*0.5f),
					Mathf.Clamp(averagePosition.y,-WorldManager.bounds.y*0.5f,WorldManager.bounds.y*0.5f)
				);

			//Lerp
			Vector2 lerpedPosition = Vector2.Lerp(transform.position,averagePosition,Time.deltaTime*damping);
			transform.position = new Vector3(lerpedPosition.x,lerpedPosition.y,transform.position.z);
		}
	}
	public void ResetPosition(){transform.position = Vector3.zero;}
}