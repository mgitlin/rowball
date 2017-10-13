//PlayerManager.cs
//Created on 01.04.16 by Aaron C Gaudette
//Manages players and spawns ships

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
[ExecuteInEditMode]

public class PlayerManager : MonoBehaviour{
	//Singleton (private)
	public static PlayerManager playerManager = null;
	public static PlayerManager instance{get{
		if(playerManager==null){
			playerManager=(PlayerManager)FindObjectOfType(typeof(PlayerManager));
			if(FindObjectsOfType(typeof(PlayerManager)).Length>1)Debug.Log("More than one PlayerManager instance in the scene!");
		}
		return playerManager;
	}}

	[System.Serializable]
	public class Player{
		[HideInInspector] public string name = "";
		[HideInInspector] public int index;
		[HideInInspector] public ShipRevamped ship; //Ship reference

		public string playerName;
		public PlayerColor color;
		public bool inUse = false;
		public ShipRevamped.Control control = new ShipRevamped.Control();
		
		public int team = 0;
		public bool ignoreAutoToggle = false;
	}
	//Information for each player (the outside interface)
	[System.Serializable]
	public class PlayerInfo{
		[HideInInspector] public string name = "";
		[HideInInspector] public int order;
		
		//Player reference
		[HideInInspector] public int index;
		public Player player{get{return Reference(this);}}
		
		public int score = 0;
		public void Score(int amount){
			InterfaceManager.LerpScore(score,score+amount,player.team);
			score+=amount;
		}
		public int teamScore{get{return TeamScore(player.team);}}
		public int spree = 0;
		public int multikill = 0;
		
		public float lastKillTime;
		
		public bool alive{get{return player.ship!=null && !player.ship.totaled;}}
		public bool totaled{get{return player.ship!=null && player.ship.totaled;}}
		//public bool scoredGoal{get{return player.ship!=null && player.ship.scoredGoal;}}
		
		public bool ready{
			get{return (player.ship==null)?false:player.ship.playerReady;}
			set{if(player.ship!=null)player.ship.playerReady=value;}
		}
		
		public Vector2 position{get{return player.ship.position;}}
		
		public int GetMetric(Metric m){
			switch(m){
				case Metric.SCORE: return score;
				case Metric.VOLLEYS: return volleys;
				case Metric.STEALS: return steals;
				case Metric.L_KILLS: return leaderKills;
				case Metric.M_KILLS: return multiKills;
				case Metric.SPREE: return longestSpree;
				case Metric.SUICIDES: return suicides;
				case Metric.LOSER: return score;
				default: return -1;
			}
		}
		public int volleys, steals, leaderKills, multiKills, longestSpree, suicides;
	}
	public static Player Reference(PlayerInfo p){return instance.players[p.index];} //Player reference

	//Static
	public static bool setupComplete{get{return instance._setupComplete;}}
	public static void Refresh(){instance.regenerate=true;}
	public static PlayerInfo[] ActivePlayers(bool checkAlive = false){
		int i = 0;
		PlayerInfo[] info = new PlayerInfo[instance.ActivePlayerCount(checkAlive)];

		for(int p=0;p<instance.players.Length;++p)if(instance.players[p]!=null && instance.players[p].inUse)
			if(!checkAlive || instance.playerInfo[p].alive){
				if(p>=instance.playerInfo.Length)info[i++]=null;
				else info[i++]=instance.playerInfo[p];
			}
		return info;
	}
	public static PlayerInfo[] lastAlive{get{
		PlayerInfo[] alive = new PlayerInfo[instance.ActivePlayerCount(true)];
		int i = 0, team = -1;
		foreach(PlayerInfo p in ActivePlayers(true)){
			if(p.player.team!=team){
				if(team==-1)team=p.player.team;
				else return null;
				alive[i++]=p;
			}
		}
		return alive;
	}}
	
	public enum Metric{
		SCORE, //Who's winning?
		VOLLEYS,STEALS,L_KILLS,M_KILLS,SPREE,SUICIDES,
		LOSER //Lowest score, special case--refactor?
	}
	//Returns top player by metric
	
	//Worst implementation ever
	public static PlayerInfo TopPlayer(Metric m){
		PlayerInfo top = null;
		int high = 0;
		bool match = false;
		
		foreach(PlayerInfo p in ActivePlayers()){
			int pm = m!=Metric.LOSER?p.GetMetric(m):-p.GetMetric(Metric.SCORE);
			
			if(top==null || pm>high){
				top=p;
				high=pm;
				match=false;
			}
			else if(pm==high)match=true;
		}
		return match?null:top;
	}
	public static PlayerInfo leader{get{return TopPlayer(Metric.SCORE);}}
	
	//Get array of players that last scored a goal (same team)
	//public static PlayerInfo[] lastScoredGoal{get{
	//	int n=0;
	//	//foreach(PlayerInfo p in ActivePlayers())if(p.scoredGoal)n++;
	//	PlayerInfo[] scored = new PlayerInfo[n];
		
	//	int i = 0;
	//	//foreach(PlayerInfo p in ActivePlayers())
	//		//if(p.scoredGoal)scored[i++]=p;
			
	//	if(scored.Length==0)scored=null;
	//	return scored;
	//}}
	public static PlayerInfo[] TeamPlayers(int team){
		int n=0;
		foreach(PlayerInfo p in ActivePlayers())if(p.player.team==team)n++;
		PlayerInfo[] teamed = new PlayerInfo[n];
		
		int i = 0;
		foreach(PlayerInfo p in ActivePlayers())
			if(p.player.team==team)teamed[i++]=p;
			
		if(teamed.Length==0)teamed=null;
		return teamed;
	}
	//coupling? (tmp)
	public static bool Winner(PlayerInfo info){
		if(GameManager.instance.winners!=null)
			foreach(PlayerInfo p in GameManager.instance.winners)if(info.player.team==p.player.team)return true;
		return false;
	}
	public static PlayerInfo lastTotaled{get{return instance.totaledOrder.Count>0?instance.totaledOrder.Peek():null;}}
	//public static PlayerInfo GetPlayer(Ship s){
	//	foreach(PlayerInfo p in ActivePlayers())if(p.player.ship!=null && p.player.ship==s)return p;
	//	return null;
	//}
    public static PlayerInfo GetPlayer(ShipRevamped s)
    {
        foreach (PlayerInfo p in ActivePlayers()) if (p.player.ship != null && p.player.ship == s) return p;
        return null;
    }
    //Get number of ready players
    public static int playersReady{get{
		int n = 0;
		foreach (PlayerInfo p in ActivePlayers())
			if(p.ready)n++;
		return n;
	}}
	//Checks if all players are ready to play
	public static bool gameReady{get{
		foreach (PlayerInfo p in ActivePlayers())
			if(!p.ready)return false;
		return true;
	}}
	
	//Check cache with actual and update
	public static bool HasChanged(ref Player[] cache){
		bool c = instance.players!=cache;
		cache=instance.players;
		return c;
	}
	
	public static int TeamScore(int team){
		int n = 0;
		foreach(PlayerInfo p in ActivePlayers())if(p!=null && p.player.team==team)n+=p.score;
		return n;
	}
	public static Color TeamColor(PlayerInfo p){return TeamColor(p.player.team);}
	public static Color TeamColor(int team){
		if(instance.teamColors.Length>instance.colorBank.Length)return Color.white;
		if(team>=instance.colorBank.Length || instance.teamColors[team]==PlayerColor.AUTO)return Color.white;
		return instance.colorBank[(int)instance.teamColors[team]-1];
	}
	//Static persistence
	public static void LoadScores(int[] scores){
		if(scores==null)return;
		instance.StartCoroutine(instance.LoadScoresWhenReady(scores));
	}
	IEnumerator LoadScoresWhenReady(int[] scores){
		while(!readyToLoad)yield return 0;
		int i = 0;
		for(int p=0;p<players.Length;++p)if(players[p]!=null && players[p].inUse)
			playerInfo[p].score=scores[i++];
	}
	public static int[] GetScores(){
		PlayerInfo[] info = ActivePlayers();
		int[] scores = new int[info.Length];
		for(int i=0;i<info.Length;++i)scores[i]=info[i].score;
		return scores;
	}
	public static void ClearScores(){
		foreach(PlayerInfo p in ActivePlayers())if(p!=null)p.score=0;
	}

	//Instance
	public ShipRevamped shipType; //Ship type for all players
	public bool autoAssignTeams = true;
	public bool autoTogglePlayers = true;
	public Player[] players = new Player[4];
	public PlayerInfo[] playerInfo = new PlayerInfo[4];
	public static bool everyoneSpring = false;

	[HideInInspector] public Transform[] teamContainers = new Transform[0]; //Child sorting
	Transform TeamContainer(int team){
		if(teamContainers.Length<team+1)teamContainers=new Transform[team+1];
		if(teamContainers[team]==null){
			foreach(Transform t in GetComponentsInChildren<Transform>()){
				if(t.gameObject.name=="Team "+(team+1)){
					teamContainers[team]=t;
					return t;
				}
			}
			GameObject child = new GameObject("Team "+(team+1));
			child.transform.parent=transform;
			teamContainers[team]=child.transform;
			return child.transform;
		}
		return teamContainers[team];
	}

	[HideInInspector] public Stack<PlayerInfo> totaledOrder = new Stack<PlayerInfo>();

	public int ActivePlayerCount(bool checkAlive = false){
		int n = 0;
		for(int i=0;i<players.Length;++i)if(players[i]!=null && players[i].inUse)
			if(!checkAlive || (i<playerInfo.Length && playerInfo[i].alive))n++;
		return n;
	}

	//Regenerate players on button press
	[Header("Editor")]
	public bool regenerate = false;

	public enum PlayerColor{
		AUTO,
		CINNABAR,MAHOGANY,CIANWOOD,AZALEA,	//Reds
		GOLDENROD,SAFFRON,TERRACOTTA,		//Oranges
		CERULEAN,AZURE,						//Blues
		VIRIDIAN,CELADON,OLIVINE,GREEN,		//Greens
		INDIGO,PURPLE,VIOLET,FUCHSIA,		//Purples
		PEWTER								//Monochrome
	}
	[HideInInspector] public PlayerColor[] teamColors;
	Color[] colorBank = new Color[]{
		new Color(227.0f/255f,66f/255f,52f/255f),	//Cinnabar
		new Color(192f/255f,64f/255f,0f/255f),		//Mahogany
		new Color(192f/255f,32f/255f,24f/255f),		//Cianwood
		new Color(241f/255f,178f/255f,225f/255f),	//Azalea
		
		new Color(218f/255f,165f/255f,32f/255f),	//Goldenrod
		new Color(255f/255f,153f/255f,51f/255f),	//Saffron
		new Color(226f/255f,114f/255f,91f/255f),	//Terracotta
		
		new Color(0f/255f,123f/255f,167f/255f),		//Cerulean
		new Color(0f,127f/255f,255f/255f),			//Azure
		
		new Color(64f/255f,130f/255f,109f/255f),	//Viridian
		new Color(172f/255f,255f/255f,175f/255f),	//Celadon
		new Color(154f/255f,185f/255f,115f/255f),	//Olivine
		new Color(0f/255f,220f/255f,60f/255f),		//Green
		
		new Color(75f/255f,0f/255f,130f/255f),		//Indigo
		new Color(170/255f,56/255f,250/255f),		//Purple
		new Color(127f/255f,0f/255f,255f/255f),		//Violet
		new Color(255f/255f,0f/255f,255f/255f),		//Fuchsia
		
		new Color(142f/255f,146f/255f,148f/255f)	//Pewter
	};
	bool readyToLoad=false;
	[HideInInspector] public bool[] usageCache;
	[HideInInspector] public PlayerColor[] colorCache;
	[HideInInspector] public bool _setupComplete = false; //Signals that all players are ready and have been respawned

	void Start(){
		Update(); //Preliminary update
		Spawn();
	}
	void Spawn(){
		readyToLoad=false;

		Clear();

		if (GameManager.instance.roundState == GameManager.RoundState.SETUP) playerInfo = new PlayerInfo[players.Length]; //Checks if game has been initialized before

		usageCache = new bool[players.Length];
		colorCache = new PlayerColor[players.Length];
		teamColors = new PlayerColor[players.Length];

		int spawnPoint = 0;
		for(int i=0;i<players.Length;++i){
			if (GameManager.instance.roundState == GameManager.RoundState.SETUP) {
				playerInfo[i]=new PlayerInfo(); //Initialize info
				playerInfo[i].ready=false; //Initialize PlayerReady boolean in ship if game has not been initialized before
			}

			if(players[i].inUse){
				//Spawn player and set data
				ShipRevamped s = Instantiate(shipType,WorldManager.SpawnPoint(spawnPoint),Quaternion.identity) as ShipRevamped;
				players[i].ship = s;
                s.control = players[i].control;
				spawnPoint++;
                //Debug.Log("Spawned player " + i + " with control " + players[i].control.ToString());
				usageCache[i]=true;
			}
			UpdatePlayer(players[i],i);
		}

		//Update team colors
		UpdateTeamColors();
		foreach(PlayerInfo p in playerInfo)UpdatePlayerShip(p);

		readyToLoad=true;
	}
	void Clear(){foreach(Transform t in teamContainers)if(t!=null)DestroyImmediate(t.gameObject);}
	void UpdatePlayer(Player p, int index){
		p.index=index;
		playerInfo[index].index=index;
		playerInfo[index].order=-1;
		
		if(p.inUse){
			for(int i=0;i<index+1;++i)if(players[i].inUse)playerInfo[index].order++;
			
			string playerName = p.playerName==""?("Player "+(playerInfo[index].order+1)):p.playerName;
			p.name=playerName+" (Team "+(p.team+1)+")";
			playerInfo[index].name=playerName+" ("+playerInfo[index].teamScore+", "+teamColors[p.team]+")";
			
			//Team distribution
			if(autoAssignTeams)
				p.team=playerInfo[index].order%(GameManager.instance.teams==0?ActivePlayerCount():GameManager.instance.teams);
		}
		else p.name=playerInfo[index].name="[Disabled]";
		
		UpdatePlayerShip(playerInfo[index]);
	}
	void UpdatePlayerShip(PlayerInfo p){
		if(p.player.ship!=null){
			p.player.ship.gameObject.name="Player "+(p.order+1);
			p.player.ship.color=TeamColor(p.player.team);
			p.player.ship.control=p.player.control;
			p.player.ship.transform.parent=TeamContainer(p.player.team);
			if (everyoneSpring) {
				p.ready=p.player.ship.playerReady;
			} else {
				if (p.player.ship.playerReady) {
					p.ready=true;
				}
			}
		}
	}
	
	void Update(){
		if(players.Length!=playerInfo.Length)Spawn(); //Update if a discontinuity is discovered
		
		if (GameManager.instance.roundState == GameManager.RoundState.RESPAWN) {
			Spawn ();
			_setupComplete = true;
			if (setupComplete)
			if (WorldManager.setupComplete)
			if (setupComplete && WorldManager.setupComplete) {
				GameManager.instance.roundState = GameManager.RoundState.PLAYING;
				GameManager.instance.ResetFreezeTime();
				CameraController.Reset();
			}
		}
		
		//Update player cap
		int count = ActivePlayerCount();
		
		if(count<GameManager.instance.minPlayers){
			if(autoTogglePlayers){
				for(int i=players.Length-1;ActivePlayerCount()<GameManager.instance.minPlayers;--i){
					if(i<=0){
						Debug.Log("Not enough player slots for "+GameManager.instance.mode+
							" (add "+(GameManager.instance.minPlayers-players.Length)+" more)");
						break;
					}
					if(!players[i].ignoreAutoToggle)players[i].inUse=true;
				}
			}
			else Debug.Log("Warning: The number of active players is less than the range specified in "+GameManager.instance.mode);
		}
		else if(count>GameManager.instance.maxPlayers){
			if(autoTogglePlayers){
				for(int i=players.Length-1;ActivePlayerCount()>GameManager.instance.maxPlayers;--i)
					if(!players[i].ignoreAutoToggle)players[i].inUse=false;
			}
			else Debug.Log("Warning: The number of active players is greater than the range specified in "+GameManager.instance.mode);
		}
		//Regenerate if any players were toggled
		if(usageCache.Length==players.Length){
			for(int i=0;i<players.Length;++i){
				if(players[i]!=null && players[i].inUse!=usageCache[i])regenerate=true;
				usageCache[i]=players[i].inUse;
			}
		}
		
		//Force unique player colors
		if(colorCache.Length==players.Length){
			for(int i=0;i<players.Length;++i){
				if(players[i].color==PlayerColor.AUTO)players[i].color=UniqueColor(colorCache[i]);
				if(players[i]!=null && players[i].color!=colorCache[i]){
					foreach(Player p in players)if(p!=null && p!=players[i] && p.color==players[i].color){
						Debug.Log(players[i].color+" already in use by "+p.name);
						players[i].color=colorCache[i];
						break;
					}
				}
				colorCache[i]=players[i].color;
			}
		}

		//Update players
		UpdateTeamColors();
		for(int i=0;i<players.Length;++i){
			UpdatePlayer(players[i],i);
			//Update totaled stack (duplicates are OK because of respawns)
			if(playerInfo[i].totaled)totaledOrder.Push(playerInfo[i]);
		}
		
		//Editor
		if(regenerate){
			Spawn();
			regenerate=false;
		}
	}
	//Overwrite team colors (with priority to lower indices)
	void UpdateTeamColors(){
		if(teamColors.Length==players.Length){
			for(int i=players.Length-1;i>=0;--i)if(players[i]!=null && players[i].inUse){
				teamColors[players[i].team]=players[i].color;
			}
		}
	}
	//Get first unique color out of set
	PlayerColor UniqueColor(PlayerColor avoid){
		foreach(PlayerColor color in System.Enum.GetValues(typeof(PlayerColor))){
			if(color==avoid || color==PlayerColor.AUTO)continue;
			bool found = false;
			foreach(Player p in players)if(p!=null && p.color==color){
				found=true;
				break;
			}
			if(found)continue;
			return color;
		}
		return PlayerColor.AUTO;
	}
}