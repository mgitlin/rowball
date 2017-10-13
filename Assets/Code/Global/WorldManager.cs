//WorldManager.cs
//Created on 20.03.16 by Aaron C Gaudette as ShipManager.cs
//Edited on 03.27.16 by Matt Gitlin
//Manages and spawns breakable objects and static terrain

using UnityEngine;
using UnityEngine.UI;
[ExecuteInEditMode]

public class WorldManager : MonoBehaviour{
	//Singleton (private)
	static WorldManager worldManager = null;
	public static WorldManager instance{get{
		if(worldManager==null){
			worldManager=(WorldManager)FindObjectOfType(typeof(WorldManager));
			if(FindObjectsOfType(typeof(WorldManager)).Length>1)Debug.Log("More than one WorldManager instance in the scene!");
		}
		return worldManager;
	}}
	
	[HideInInspector] public Transform _boundsContainer;
	public static Transform boundsContainer{get{
		foreach(Transform t in instance.GetComponentsInChildren<Transform>()){
			if(t.gameObject.name=="Bounds"){
				instance._boundsContainer=t;
				return t;
			}
		}
		GameObject child = new GameObject("Bounds");
		child.transform.parent=instance.transform;
		instance._boundsContainer=child.transform;
		return child.transform;
	}}
	[HideInInspector] public Transform _breakableContainer;
	public static Transform breakableContainer{get{
		foreach(Transform t in instance.GetComponentsInChildren<Transform>()){
			if(t.gameObject.name=="Breakables"){
				instance._breakableContainer=t;
				return t;
			}
		}
		GameObject child = new GameObject("Breakables");
		child.transform.parent=instance.transform;
		instance._breakableContainer=child.transform;
		return child.transform;
	}}
	[HideInInspector] public Transform _terrainContainer;
	public static Transform terrainContainer{get{
		foreach(Transform t in instance.GetComponentsInChildren<Transform>()){
			if(t.gameObject.name=="Terrain"){
				instance._terrainContainer=t;
				return t;
			}
		}
		GameObject child = new GameObject("Terrain");
		child.transform.parent=instance.transform;
		instance._terrainContainer=child.transform;
		return child.transform;
	}}

	[HideInInspector] public Transform _goalContainer;
	public static Transform goalContainer{get{
		foreach(Transform t in instance.GetComponentsInChildren<Transform>()){
			if(t.gameObject.name=="Goals"){
				instance._goalContainer=t;
				return t;
			}
		}
		GameObject child = new GameObject("Goals");
		child.transform.parent=instance.transform;
		instance._goalContainer=child.transform;
		return child.transform;
	}}

	[HideInInspector] public Transform _terrainAffectorContainer;
	public static Transform terrainAffectorContainer{get{
		foreach (Transform t in instance.GetComponentsInChildren<Transform>()) {
			if (t.gameObject.name == "TerrainAffector") {
				instance._terrainAffectorContainer = t;
				return t;
			}
		}
		GameObject child = new GameObject("TerrainAffector");
		child.transform.parent=instance.transform;
		instance._terrainAffectorContainer=child.transform;
		return child.transform;
	}}
	[HideInInspector] public Transform _rowballSpawnerContainer;
	public static Transform rowballSpawnerContainer{get{
			foreach (Transform t in instance.GetComponentsInChildren<Transform>()) {
				if (t.gameObject.name == "RowballSpawner") {
					instance._terrainAffectorContainer = t;
					return t;
				}
			}
			GameObject child = new GameObject("RowballSpawner");
			child.transform.parent=instance.transform;
			instance._terrainAffectorContainer=child.transform;
			return child.transform;
	}}
	
	[System.Serializable]
	public class WorldObject{
		[HideInInspector] public string name = "";
		public GameObject worldObject;
		
		public bool ignore = false;
		public bool isStatic = true;
		public bool isTerrainAffector = false;
		public bool isRowballSpawner = false;
		public Vector2 spawnPosition;
	}

	public static int worldObjectsLength{get{return instance.worldObjects.Length;}}
	public static int goalsLength{get{return GameManager.instance.teams;}}
	
	public static GameObject GetGoal(int team){return instance.goals[team];}

	public static Vector2 bounds{get{return instance.worldBounds;}}
	//Get player spawn point
	public static Vector2 SpawnPoint(int i){
		if(PlayerManager.ActivePlayers().Length!=instance.spawnPoints.Length)instance.GenerateSpawnPoints();
		return instance.spawnPoints[i];
	}

    public string mapName;
    public Sprite preview;

	//Instance
	[Header("World Bounds")]
	public Color boundsColor = Color.blue;
	public Camera boundsCamera;
	public float startingZoom = 128; //tmp var for bounds scaling
	public LineRenderer lineRenderer;
	public Vector2 worldBounds = new Vector2(2048,1024);
	public int boundsOffset = 128;
	[HideInInspector] public Vector2[] boundVertices = new Vector2[]{Vector2.zero};
	[HideInInspector] public LineRenderer[] _boundsLRCache = new LineRenderer[4];
	public static bool setupComplete{get{return instance._setupComplete;}}
	[HideInInspector] public bool _setupComplete = false;

	[Header("Objects")]
	public GameObject defaultBreakable;
	public GameObject defaultTerrain;
	public GameObject defaultRowballSpawner;
	public GameObject defaultGoal;
	public GameObject defaultTerrainAffector;
	public Color breakableColor = Color.white, terrainColor = Color.gray;
	public bool generatePositions = false;
	public int generateOffset = 32;
	
	public WorldObject[] worldObjects = new WorldObject[]{new WorldObject()};
	public GameObject[] goals = new GameObject[0];
	
	public Vector2[] goalSpawnPositions;

	//Spawn points for players
	public bool generateSpawnPoints = false;
	public int spawnPointOffset = 32;
	public Vector2[] spawnPoints = new Vector2[]{Vector2.zero};
	
	[HideInInspector] public GameObject[] objectCache = new GameObject[0];

	//Regenerate objects on button press
	[Header("Editor")]
	public bool regenerate = false;

	void Start(){
        if(GameController.instance != null)
		    boundsCamera = GameController.instance.boundsCamera;
		GenerateSpawnPoints();
		GenerateBounds();
		//SpawnGoals();
		SpawnObjects();
		Update();
	}

	void GenerateBounds(){
		_boundsLRCache = new LineRenderer[4];
		boundVertices = new Vector2[4];
		if(_boundsContainer!=null)DestroyImmediate(_boundsContainer.gameObject);
		
		for (int i = 0; i < 4; i++) {
			LineRenderer lr = Instantiate(lineRenderer, Vector2.zero, Quaternion.identity) as LineRenderer;
			lr.transform.parent = boundsContainer;
			_boundsLRCache [i] = lr;
		}
	}

	//void SpawnGoals(){
	//	if (_goalContainer != null)
	//		DestroyImmediate (_goalContainer.gameObject);
	//	if (GameManager.instance.mode == GameManager.GameMode.CORE) {
	//		goals = new GameObject[goalsLength];
	//		for (int i = 0; i < goalsLength; ++i) {
	//			goals [i] = SpawnGoal (goalSpawnPositions [i], i);
	//		}
	//	}
	//}

	void SpawnObjects(){
		if(_breakableContainer!=null)DestroyImmediate(_breakableContainer.gameObject);
		if(_terrainContainer!=null)DestroyImmediate(_terrainContainer.gameObject);
		if (_terrainAffectorContainer != null)DestroyImmediate (_terrainAffectorContainer.gameObject);
		if (_rowballSpawnerContainer != null) DestroyImmediate (_rowballSpawnerContainer.gameObject);
		if (objectCache != null) {
			foreach (Object o in objectCache) {
				DestroyImmediate (o);
			}
		}
		objectCache = new GameObject[worldObjectsLength];
		if (generatePositions) {
			//Get optimum side length
			int yLength = (int)Mathf.Round (Mathf.Sqrt (worldObjectsLength));
			int xLength = (int)Mathf.Ceil (worldObjectsLength / (float)yLength);

			for (int i = 0; i < worldObjectsLength; ++i) {
				worldObjects[i].spawnPosition = new Vector2 ((i % xLength) * generateOffset, (i / xLength) * generateOffset); //Grid
				worldObjects[i].spawnPosition -= new Vector2 ((xLength - 1) * generateOffset * 0.5f, (yLength - 1) * generateOffset * 0.5f); //Center
				//Last-line offset
				if ((i / xLength) % 2 == yLength - 1 && worldObjectsLength % xLength != 0)
					worldObjects[i].spawnPosition += new Vector2 (generateOffset * 0.5f, 0);
			}
		}
		for(int i=0;i<worldObjectsLength;++i){
			objectCache[i]=Spawn(worldObjects[i]);
		}
	}

	GameObject Spawn(WorldObject o){
		if(o.isStatic && !o.ignore){
			if (o.isRowballSpawner) {
				RowballSpawner r = (Instantiate (o.worldObject, o.spawnPosition, Quaternion.identity) as GameObject).GetComponent<RowballSpawner> ();
				r.gameObject.name = "RowballSpawner (" + o.worldObject.gameObject.name + ")";
				r.transform.parent = rowballSpawnerContainer;
				return r.gameObject;
			} else {
				WorldTerrain s = (Instantiate (o.worldObject, o.spawnPosition, Quaternion.identity) as GameObject).GetComponent<WorldTerrain> ();
				s.color = terrainColor;
				s.gameObject.name = "Terrain (" + o.worldObject.gameObject.name + ")";
				s.transform.parent = terrainContainer;
				return s.gameObject;
			}
		}
		else if(!o.ignore){
			if (o.isTerrainAffector) {
				TerrainAffector ta = (Instantiate (o.worldObject, o.spawnPosition, Quaternion.identity) as GameObject).GetComponent<TerrainAffector> ();
				ta.color = terrainColor;
				ta.gameObject.name = "TerrainAffector (" + o.worldObject.gameObject.name + ")";
				ta.transform.parent = terrainAffectorContainer;
				return ta.gameObject;
			} else {
				//Else
				Breakable b = (Instantiate (o.worldObject, o.spawnPosition, Quaternion.identity) as GameObject).GetComponent<Breakable> ();
				b.color = breakableColor;
				b.gameObject.name = "Breakable (" + o.worldObject.gameObject.name + ")";
				b.transform.parent = breakableContainer;
				return b.gameObject;
			}
		}
		return null;
	}

	GameObject SpawnGoal(Vector2 pos, int i){
		WorldTerrain w = (Instantiate(defaultGoal,pos,Quaternion.identity) as GameObject).GetComponent<WorldTerrain>();
		w.team = i;
		w.gameObject.name = "Goal (Team "+i+")";
		w.transform.parent = goalContainer;
		return w.gameObject;
	}
	
	//Generate spawn points for players
	void GenerateSpawnPoints(){
		if(generateSpawnPoints){
			int count = PlayerManager.ActivePlayers().Length;
			spawnPoints=new Vector2[count];
			//Get optimum side length
			int yLength = (int)Mathf.Round(Mathf.Sqrt(count));
			int xLength = (int)Mathf.Ceil(count/(float)yLength);
			
			for(int i=0;i<count;++i){
				spawnPoints[i]=new Vector2((i%xLength)*spawnPointOffset,(i/xLength)*spawnPointOffset); //Grid
				spawnPoints[i]-=new Vector2((xLength-1)*spawnPointOffset*0.5f,(yLength-1)*spawnPointOffset*0.5f); //Center
				//Last-line offset
				if((i/xLength)%2==yLength-1 && count%xLength!=0)spawnPoints[i]+=new Vector2(spawnPointOffset*0.5f,0);
			}
		}
	}

	void Update(){

		//Set defaults
		foreach(WorldObject o in worldObjects)
			if(o.worldObject==null || (!o.isStatic && o.worldObject==defaultTerrain) || (o.isStatic && o.worldObject==defaultBreakable))
				o.worldObject=o.isStatic?(o.isRowballSpawner?defaultRowballSpawner:defaultTerrain):(o.isTerrainAffector)?defaultTerrainAffector:defaultBreakable;
		
		UpdateBounds();

		if (GameManager.instance.roundState == GameManager.RoundState.RESPAWN) {
			SpawnObjects();
			_setupComplete = true;
			if (setupComplete)
			if (PlayerManager.setupComplete)
			if (setupComplete && PlayerManager.setupComplete) {
				GameManager.instance.roundState = GameManager.RoundState.PLAYING;
				GameManager.instance.ResetFreezeTime();
				CameraController.Reset();
			}
		}

		//Editor
		if(regenerate){
			regenerate=false;
			Start();
		}
	}
	void UpdateBounds(){
		float xOffset = startingZoom*boundsCamera.aspect+worldBounds.x*0.5f-boundsOffset; //X offset depends on aspect ratio
		float yOffset = startingZoom+worldBounds.y*0.5f-boundsOffset;
		
		boundVertices[0]=new Vector2(-xOffset,yOffset);
		boundVertices[1]=new Vector2(xOffset,yOffset);
		boundVertices[2]=new Vector2(xOffset,-yOffset);
		boundVertices[3]=new Vector2(-xOffset,-yOffset);
		
		for (int i = 0; i < 4; i++) {
			_boundsLRCache[i].SetPosition(0,new Vector3(boundVertices[i].x,boundVertices[i].y,2));
			_boundsLRCache[i].SetPosition(1,new Vector3(boundVertices[(i+1)%4].x,boundVertices[(i+1)%4].y,2));
            _boundsLRCache[i].startColor = boundsColor;
            _boundsLRCache[i].endColor = boundsColor;

        }
	}
}