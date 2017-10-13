//CameraBounds.cs
//Created on 18.03.16 by Aaron C Gaudette
//Updates collision bounds of camera

using UnityEngine;
[ExecuteInEditMode]

public class CameraBounds : MonoBehaviour{
	public Camera boundedCam;
	
	public AreaEffector2D effector;
	public int force = 32000, depth = 32;
	public bool trackWinner = false;
	[HideInInspector] public AreaEffector2D[] effectors;

	EdgeCollider2D edgeCollider; //Cache
	[HideInInspector] public Vector2[] corners = new Vector2[5];
	
	void Start(){
		BuildRing();
		Update();
		
		//Capping framerate to mitigate bounds issues at large framerates
		//Need to fix!
		Application.targetFrameRate=60;
	}
	void BuildRing(){
		Clear();
		effectors=new AreaEffector2D[8];
		for(int i=0;i<8;++i){
			effectors[i]=Instantiate(effector,Vector3.zero,Quaternion.identity) as AreaEffector2D;
			effectors[i].gameObject.name="Bounds Effector";
			effectors[i].transform.parent=transform;
		}
	}
	void Clear(){
		foreach(AreaEffector2D e in effectors)if(e!=null)DestroyImmediate(e.gameObject);
	}
	
	void Update(){
		//Temporary, will function in GameManager later
		if(GameController.game != null)
			trackWinner=GameManager.instance.scoreMode==GameManager.ScoreMode.KILL;
		
		//Edge bound
		if(edgeCollider==null)edgeCollider=GetComponent<EdgeCollider2D>();
		
		float xOffset = boundedCam.orthographicSize*boundedCam.aspect; //X offset depends on aspect ratio
		float yOffset = boundedCam.orthographicSize;

		corners[0]=new Vector2(-xOffset,yOffset); corners[1]=new Vector2(xOffset,yOffset);
		corners[2]=new Vector2(xOffset,-yOffset); corners[3]=new Vector2(-xOffset,-yOffset);
		corners[4]=corners[0];
		
		edgeCollider.points=corners;

		if (GameController.game != null) {
			foreach (PlayerManager.PlayerInfo p in PlayerManager.ActivePlayers()) {
				p.player.ship.boundsOffset = new Vector2 (xOffset, yOffset);
			}

			//Effector ring--generated manually for now (prototype)
			if (GameManager.instance.winners == null || GameManager.instance.winners.Length > 1 || !trackWinner) {
				if (effectors.Length == 0 || effectors [0] == null)
					BuildRing ();
				foreach (AreaEffector2D e in effectors)
					e.forceMagnitude = force;
				Vector2[] vertices = new Vector2[4];
			
				//Corners
				vertices [0] = new Vector2 (-xOffset - depth, yOffset + depth);
				vertices [1] = new Vector2 (-xOffset, yOffset + depth);
				vertices [2] = new Vector2 (-xOffset, yOffset);
				vertices [3] = new Vector2 (-xOffset - depth, yOffset);
				effectors [0].GetComponent<PolygonCollider2D> ().SetPath (0, vertices);
				effectors [0].forceAngle = -45;
			
				vertices [0] = new Vector2 (xOffset, yOffset + depth);
				vertices [1] = new Vector2 (xOffset + depth, yOffset + depth);
				vertices [2] = new Vector2 (xOffset + depth, yOffset);
				vertices [3] = new Vector2 (xOffset, yOffset);
				effectors [1].GetComponent<PolygonCollider2D> ().SetPath (0, vertices);
				effectors [1].forceAngle = -135;
			
				vertices [0] = new Vector2 (xOffset, -yOffset);
				vertices [1] = new Vector2 (xOffset + depth, -yOffset);
				vertices [2] = new Vector2 (xOffset + depth, -yOffset - depth);
				vertices [3] = new Vector2 (xOffset, -yOffset - depth);
				effectors [2].GetComponent<PolygonCollider2D> ().SetPath (0, vertices);
				effectors [2].forceAngle = -225;
			
				vertices [0] = new Vector2 (-xOffset - depth, -yOffset);
				vertices [1] = new Vector2 (-xOffset, -yOffset);
				vertices [2] = new Vector2 (-xOffset, -yOffset - depth);
				vertices [3] = new Vector2 (-xOffset - depth, -yOffset - depth);
				effectors [3].GetComponent<PolygonCollider2D> ().SetPath (0, vertices);
				effectors [3].forceAngle = -315;
			
				//Edges
				vertices [0] = new Vector2 (-xOffset, yOffset + depth);
				vertices [1] = new Vector2 (xOffset, yOffset + depth);
				vertices [2] = new Vector2 (xOffset, yOffset);
				vertices [3] = new Vector2 (-xOffset, yOffset);
				effectors [4].GetComponent<PolygonCollider2D> ().SetPath (0, vertices);
				effectors [4].forceAngle = -90;
			
				vertices [0] = new Vector2 (xOffset, yOffset);
				vertices [1] = new Vector2 (xOffset + depth, yOffset);
				vertices [2] = new Vector2 (xOffset + depth, -yOffset);
				vertices [3] = new Vector2 (xOffset, -yOffset);
				effectors [5].GetComponent<PolygonCollider2D> ().SetPath (0, vertices);
				effectors [5].forceAngle = -180;
			
				vertices [0] = new Vector2 (-xOffset, -yOffset);
				vertices [1] = new Vector2 (xOffset, -yOffset);
				vertices [2] = new Vector2 (xOffset, -yOffset - depth);
				vertices [3] = new Vector2 (-xOffset, -yOffset - depth);
				effectors [6].GetComponent<PolygonCollider2D> ().SetPath (0, vertices);
				effectors [6].forceAngle = -270;
			
				vertices [0] = new Vector2 (-xOffset - depth, yOffset);
				vertices [1] = new Vector2 (-xOffset, yOffset);
				vertices [2] = new Vector2 (-xOffset, -yOffset);
				vertices [3] = new Vector2 (-xOffset - depth, -yOffset);
				effectors [7].GetComponent<PolygonCollider2D> ().SetPath (0, vertices);
				effectors [7].forceAngle = 0;
			} else { //Disable all bounds for winner
				if (!Application.isEditor || Application.isPlaying)
					edgeCollider.enabled = false;
				Clear ();
			}
		}
	}
}