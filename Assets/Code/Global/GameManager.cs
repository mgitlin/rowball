/**
 * GameManager.cs
 * by Daniel W. Zhang
 * 
 */

//Merged with Game.cs (deprecated) on 01.04.16 by Aaron C Gaudette, refactored -> 03.04.16

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.Collections;
[ExecuteInEditMode]

public class GameManager : MonoBehaviour{
	//Singleton
	private static GameManager gameManager = null;
	public static GameManager instance{get{
		if(gameManager==null){
			gameManager=(GameManager)FindObjectOfType(typeof(GameManager));
			if(FindObjectsOfType(typeof(GameManager)).Length>1)Debug.Log("More than one GameManager instance in the scene!");
		}
		return gameManager;
	}}

    public string modeName;

	public enum GameMode{CORE,FFA,DUEL,CUSTOM}
	public enum ScoreMode{GOAL,LMS,KILL} //more placeholder than anything
	public enum RoundState{SETUP,RESPAWN,STARTING_PLAY,PLAYING,RESTARTING,PAUSED,END_SCREEN}
	
	//Statics (persistent data)
	static int CURRENT_ROUND = 0;
	static int[] SCORES = null;
	
	public static bool warmupOn = true;
	static bool hadWarmup = false;
	
	//Instance
	public GameMode mode = GameMode.FFA;
	
	public float restartTime = 3;
	public int teams = 0; //Zero specifier = no teams, just individuals
	public int minPlayers = 1, maxPlayers = 4;
	
	public int scoreToWin = 30;
	public bool displayPercentScore = false;
	public ScoreMode scoreMode = ScoreMode.LMS;
	public bool scorePerRound = false; //Does the winner score at round end, or not
	private int baseScorePoints = 1;
	public int penaltyPoints = -1;
	public float penaltyMultiplier = 0.5f;
	public int multiKillThreshhold = 4;
	public int spreeThreshhold = 3;
	
	public bool useRespawns = false;
	public float respawnTime = 4;
	
	[Header("Read-only")]
	public int round = 0;
	public RoundState roundState = RoundState.SETUP; // Used for transitioning when players are ready
	public PlayerManager.PlayerInfo[] winners = null;
	
	[HideInInspector] public float freezeTimer = 0;
	[HideInInspector] public float freezeTime = 1;
	[HideInInspector] public bool roundOver = false, endSequenceInitiated = false;
	[HideInInspector] RoundState previous;
	GameMode lastMode = GameMode.CUSTOM;
	
	void Start(){
		//Load statics
		round=CURRENT_ROUND;
		PlayerManager.LoadScores(SCORES);
		
		if (warmupOn) {
			round = -1;
			roundState = RoundState.SETUP;
			hadWarmup = true;
		}
		
		//Observe
		if(!Application.isEditor || Application.isPlaying){
			Observer.OnKill.AddListener(OnKill);
			//Observer.OnGoal.AddListener(OnGoal);
		}
		
		winners=null;
	}


    void CreateExplosion(Vector3 pos)
    {
        GameObject ps = Instantiate(Resources.Load("Explosion"), pos, Quaternion.identity) as GameObject;
        Destroy(ps, 1);
    }

	//Observe
	void OnKill(ShipRevamped targetShip, Rowball rowball){
		int targetScore = PlayerManager.GetPlayer (targetShip).score;
		PlayerManager.PlayerInfo shooter = PlayerManager.GetPlayer(rowball.shooter.GetComponent<ShipRevamped>());
		PlayerManager.Player target = PlayerManager.GetPlayer(targetShip).player;

        CreateExplosion(rowball.position);

		string killContext = "";
		PlayerManager.GetPlayer(targetShip).spree=0;
		
		if(scoreMode == ScoreMode.KILL){
			if(rowball.shooter != null){
				float score = baseScorePoints;
				if (shooter.player.team == target.team) {
					score=penaltyPoints;
					killContext += "SUICIDE";//+" (-1)\n";
					killContext+="\n";
					
					Observer.OnSuicide.Invoke();
					
					shooter.Score((int)score);
					shooter.suicides++;
				} else {
					if(Time.time<=shooter.lastKillTime+multiKillThreshhold){
						shooter.multikill++;
					}
					else{
						shooter.multikill = 0;
					}
					
					shooter.lastKillTime = Time.time;
					shooter.spree++;
					if (shooter.spree > shooter.longestSpree)
						shooter.longestSpree = shooter.spree;

					if (PlayerManager.GetPlayer (targetShip) == PlayerManager.leader && PlayerManager.leader.score != 0) {
						//score *= leaderKillBonus;
						score++;
						killContext += "LEADER KILL";//+" (+1) \n";
						killContext+="\n";
						shooter.leaderKills++;
						//
						Observer.OnLeaderKill.Invoke();
						//Debug.Log ("Leader has been killed! x" + leaderKillBonus + "\n");
					}
					
					//float relativeSpeed = rowball.RelativeVelocity (targetShip.velocity).magnitude;
//					if (relativeSpeed > brutalityThreshold) {
//						score += (baseScorePoints * (relativeSpeed / 1000));
//						if(displayPercentScore)
//							shooter.player.textRef.text += "Brutality (+" + (baseScorePoints * relativeSpeed/1000)/scoreToWin * 100 + "%)" + "\n";
//						else
//							shooter.player.textRef.text += "Brutality (+" + (int)(baseScorePoints * relativeSpeed/1000) + ")" + "\n";
//						Debug.Log ("Brutality: +" + (baseScorePoints * relativeSpeed / 1000));
//					}
					
					if (shooter.multikill > 0) {
						score += (shooter.multikill); // Multikill bonus
						killContext += shooter.multikill==1?"DOUBLE KILL":
							shooter.multikill==2?"TRIPLE KILL":
							"MULTI: " + (shooter.multikill+1);
						killContext+="\n";
						
						//
						if(shooter.multikill==1)Observer.OnDoubleKill.Invoke();
						else if(shooter.multikill==2)Observer.OnTripleKill.Invoke();
						
						//Stats
						if(shooter.multikill>0)shooter.multiKills++;
						
						//killContext+=" (+" + shooter.player.multikill + ")" + "\n";
						//Debug.Log ("Multi ("+ shooter.player.multikill +"): x" + (shooter.player.multikill * (1.0f + multiKillMultiplier)));
					}
					
					if (shooter.spree >= spreeThreshhold) {
						score++;
						killContext += "SPREE: " + shooter.spree;// + " (+1)\n";
						killContext+="\n";
						//Debug.Log ("Spree: " + shooter.player.spree);
					}
					
					//Debug.Log ("Player has earned " + score + " points for a kill.");
					shooter.Score ((int)score);
				}
				if(score!=1)
					InterfaceManager.CreateTextEffect(
						killContext,
						target.ship.position,
						PlayerManager.TeamColor(shooter.player.team)
					);
				//else
					/*
					InterfaceManager.CreateTextEffect(
						InterfaceManager.instance.deathMessages[(int)Random.Range (0, InterfaceManager.instance.deathMessages.Length)],
						target.ship.position,
						PlayerManager.TeamColor(shooter.player.team),
						2,13
					);
					*/
				}
				
			} else if (scoreMode == ScoreMode.LMS) {
				if (rowball.shooter != null) {
					if (rowball.shooter == targetShip || shooter.player.team == target.team) {
						shooter.Score (penaltyPoints + targetScore / 10);
					} else {
						shooter.Score (baseScorePoints);
					}
				}
			}
			float respawnTimePenalty = ( targetScore > scoreToWin / 2)? targetScore/100 * penaltyMultiplier:0.0f;
			if(useRespawns && winners==null)targetShip.Respawn(respawnTime + respawnTimePenalty);
		}
	//void OnGoal(Ship ship){
	//	if(scoreMode==ScoreMode.GOAL && !roundOver){
	//		PlayerManager.PlayerInfo scorer = PlayerManager.GetPlayer(ship);
	//		if (ship.shotDistance > 325) {
	//			scorer.Score (3);
	//		} else if (ship.shotDistance > 175) {
	//			scorer.Score (2);
	//		} else {
	//			scorer.Score (baseScorePoints);
	//		}
	//	}
	//}
	
	//hacked in for build, needs to be fixed
	//round state will never = paused to allow for RT menu access
	[HideInInspector] public static bool isPaused = false;
	
	public void Pause(){
        isPaused = true;
        //MenuManager.OpenPauseMenu ();
        Time.timeScale = 0;
	}
	
	public void UnPause(){
        Time.timeScale = 1;
        //MenuManager.ClosePauseMenu ();
		isPaused=false;
	}
	
	void Update(){
        if (Input.GetKeyDown (KeyCode.Escape) || GamepadInput.GamePad.GetButtonDown(GamepadInput.GamePad.Button.Start, GamepadInput.GamePad.Index.Any)) {
			if(!isPaused){//if (roundState != RoundState.PAUSED) {
				Pause ();
			} else {
				UnPause ();
			}
		}
		if (roundState != RoundState.PAUSED) {
			if (roundState != RoundState.PLAYING && (!warmupOn || round != -1))
				roundState = RoundState.PLAYING;
		
			freezeTimer -= Time.deltaTime; //Goes negative for fading
		
			//Set mode-specific variables
			switch (mode) {
			case(GameMode.CORE):
				restartTime = 2.5f;
				teams = 2;
				minPlayers = 2;
				maxPlayers = 4;
				//scoreToWin = 60;
				scoreMode = ScoreMode.GOAL;
				scorePerRound = false; //Technically true, but the hook is for the goal shot
				useRespawns = true;
				respawnTime = 4;
				break;
			case(GameMode.FFA):
				restartTime=10;
				teams=0;
				minPlayers=2;
				maxPlayers=4;
				//scoreToWin=20;
				scoreMode=ScoreMode.KILL;
				scorePerRound=false;
				useRespawns=true;
				respawnTime=3;
				break;
			case(GameMode.DUEL):
				restartTime = 1.5f;
				teams = 0;
				minPlayers = 2;
				maxPlayers = 2;
				//scoreToWin = 10;
				scoreMode = ScoreMode.LMS;
				scorePerRound = true;
				useRespawns = false;
				respawnTime = 0;
				break;
			}
			//Winners
			switch (scoreMode) {
			//case(ScoreMode.GOAL):
			//	winners = PlayerManager.lastScoredGoal;
			//	break;
				
			case(ScoreMode.LMS):
				winners = PlayerManager.lastAlive;
				break;
				
			case(ScoreMode.KILL):
				//Round never ends
				break;
			}
			//End round
			if (scoreMode == ScoreMode.KILL) {
				for (int i = 0; i < (teams == 0 ? PlayerManager.ActivePlayers ().Length : teams); ++i) {
					if (PlayerManager.TeamScore (i) >= scoreToWin) {
						winners = PlayerManager.TeamPlayers (i);
						//EndGame();
						if (!endSequenceInitiated)
							InterfaceManager.ShowStatsScreen ();
						endSequenceInitiated = true;
						break;
					}
				}
			} else if (winners != null && !roundOver)
				EndRound (winners);
			
			//Hard reset (debug)
			if(Input.GetKeyDown(KeyCode.Space)){
				//Restart ();
			}
			
			if (PlayerManager.gameReady && roundState == RoundState.SETUP) {
				roundState = RoundState.RESPAWN;
			}
			
			//Editor
			if (mode != lastMode) {
				PlayerManager.Refresh ();
				lastMode = mode;
			}
			maxPlayers = Mathf.Max (minPlayers, maxPlayers);
			scoreToWin = Mathf.Max (1, scoreToWin);
		}
	}
	
	void EndRound(PlayerManager.PlayerInfo[] winners){
		roundOver=true;
		if(scorePerRound)winners[0].Score(baseScorePoints); //Score target is undefined, entire team scores
		round++;
		warmupOn = false;
		
		//Check score to end game
		if(scoreMode!=ScoreMode.KILL){
			for(int i=0;i<(teams==0?PlayerManager.ActivePlayers().Length:teams);++i){
				if(PlayerManager.TeamScore(i)>=scoreToWin){
					//EndGame();
					if(!endSequenceInitiated)
						InterfaceManager.ShowStatsScreen();
					endSequenceInitiated = true;
					break;
				}
			}
		}
		
		Restart(restartTime);
	}
	public void EndGame(){
//		MenuManager.instance.title.color = new Color (1, 1, 1, 1);
		round=0;
		PlayerManager.ClearScores();
		if (hadWarmup) warmupOn = true;
		
		if(scoreMode==ScoreMode.KILL){ //Single-round logic
			roundOver=true;
			Restart(restartTime);
		}
	}
	
	//Reload level (clears anything but statics)
	void Restart(){Restart(0);}
	void Restart(float time){StartCoroutine(RestartCoroutine(time));}
	IEnumerator RestartCoroutine(float time){
		float lastTime = Time.time;
		
		InterfaceManager.instance.SetAlpha (InterfaceManager.instance.gameStatus, 1);
		InterfaceManager.instance.SetAlpha (InterfaceManager.instance.statusLoader, 1);
		InterfaceManager.instance.statusLoader.fillClockwise = true;
		InterfaceManager.instance.statusLoader.fillAmount = 0;
		InterfaceManager.instance.statusLoader.fillAmount = (Time.time-lastTime)/time;
		
		yield return new WaitForSeconds(time);
		
		//Save statics
		CURRENT_ROUND=round;
		SCORES=PlayerManager.GetScores();
		
		//MenuManager.instance.ReturnToMain();
	}
	
	public void ResetFreezeTime(){freezeTimer=freezeTime;}

    public void Awake() {
        isPaused = false;
    }
}