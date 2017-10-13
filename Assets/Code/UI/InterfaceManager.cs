//

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class InterfaceManager : MonoBehaviour{
	//Singleton
	private static InterfaceManager interfaceManager = null;
	public static InterfaceManager instance{get{
		if(interfaceManager==null){
			interfaceManager=(InterfaceManager)FindObjectOfType(typeof(InterfaceManager));
			if(FindObjectsOfType(typeof(InterfaceManager)).Length>1)Debug.Log("More than one InterfaceManager instance in the scene!");
		}
		return interfaceManager;
	}}
	
	//UI Text wrapper for overlay
	[System.Serializable]
	public class TextOverlay{
		public Text gui = null;
		public Vector2 position{set{gui.transform.position=value;}}
		public Image background;
		
		public Color color{
			get{return gui.color;}
			set{gui.color=value;}
		}
		public float alpha{set{color = new Color(color.r,color.g,color.b,value);}}
		public string text{
			get{return gui.text;}
			set{gui.text=value;}
		}
		
		public string deathMessage = "";
		
		public TextOverlay(float visibleTime, float fadeTime){
			visibleTime=this.visibleTime;
			fadeTime=this.fadeTime;
		}
		
		public float blinkStartTime = 0, startTime = 0;
		public float visibleTime, fadeTime;
	}
	
	public Text[] scores;
	public Text gameStatus;
	public Image statusLoader;
    public Image statusBg;
	public Text gameObjective;

    public Image readyImg, goImg;
	
	public Text scoreTextPrefab;
	public Text shipTextPrefab;
	public Text effectTextPrefab;
	
	public Transform shipTextContainer;
	
	public float closeToWinPercent = 0.8f;
	public float xOffset = 0.5f;
	public Vector2 size = new Vector2(0.04f,0.22f);
	public float offset = 4;
	public float goTime = 1.3f;
	public float blinkRate = 0.4f, readyUpRate = 0.7f, blinkTime = 6;
	public string[] deathMessages = new string[]{"K.O."};
	
	bool finishedSetup = false;
	private float startTime;
	private float visibleTime = 3.0f;
	
	[Header("Text Effects:")]
	public int maxRotation = 28;
	public Vector2 scaleRange = new Vector2(20,26);
	public float solidTime = 0, fadeTime = 4f;
	public float plusFadeTime = 2f;
	
	[HideInInspector] public TextOverlay[] texts;
	[HideInInspector] public PlayerManager.Player[] playerCache;
	[HideInInspector] public static float textAlpha;
	[HideInInspector] float lastTime = 0;
	[HideInInspector] bool statusUpdated = false;
	
	Camera viewCamera;
	
	public static void CreateTextEffect(string text, Vector2 position, Color color, float fadeT = -1, int fontSize = -1){
		if(fadeT==-1)fadeT=instance.fadeTime;
		if(fontSize==-1)
			fontSize = (int)Random.Range (instance.scaleRange.x, instance.scaleRange.y);
		instance.StartCoroutine(instance.AddTextEffect(text,position,color,fadeT,fontSize));
	}
	IEnumerator AddTextEffect(string text, Vector2 position, Color color, float fadeT, int fontSize){
		// Create the text effect
		TextOverlay t = new TextOverlay(0,0);
		t.gui = Instantiate(effectTextPrefab,Vector3.zero,Quaternion.identity) as Text;
		t.gui.transform.SetParent(transform,false);
		
		t.text = text;
		t.color = color;
		
		t.gui.transform.eulerAngles = new Vector3(0,0,Random.Range(-maxRotation,maxRotation));
		t.gui.fontSize=fontSize;
		
		//Solid time
		float start = Time.time;
		while(Time.time<start+solidTime){
			t.position = viewCamera.WorldToScreenPoint(position);
			yield return 0;
			//
			if(GameManager.instance.roundState==GameManager.RoundState.END_SCREEN)break;
		}
		//Fade time
		float alpha = 1.0f, lastTime = Time.time;
		while (alpha >= 0) {
			alpha = 1.0f-((Time.time-lastTime)/fadeT);
			t.alpha = alpha;
			t.position = viewCamera.WorldToScreenPoint(position);
			yield return 0;
			//
			if(GameManager.instance.roundState==GameManager.RoundState.END_SCREEN)break;
		}
		Destroy(t.gui.gameObject);
	}
	
	public static void LerpScore(int scoreOld, int scoreNew, int team){
		instance.StartCoroutine(instance.InterpolateScore(scoreOld,scoreNew,team));
	}
	IEnumerator InterpolateScore(int scoreOld, int scoreNew, int team){
		float score = scoreOld;

		int teams = (GameManager.instance.teams==0?PlayerManager.instance.ActivePlayerCount():GameManager.instance.teams);
		float offset = (640) / (teams - 1);

		Text t = Instantiate(scoreTextPrefab,Vector3.zero,Quaternion.identity) as Text;
		t.transform.SetParent (transform, false);
		Vector2 screenPos = new Vector2 ((-320 + offset*(team)), -55f);
		t.rectTransform.anchoredPosition = screenPos;
		t.color = PlayerManager.TeamColor (team);
		t.fontSize = 28;
		
		t.rectTransform.anchoredPosition+=new Vector2(Random.Range(-16,16),Random.Range(-10,10)); //More magic parameters
		
		bool negative = scoreNew-scoreOld<0;
		t.text = (negative?"-":"+")+Mathf.Abs(scoreNew-scoreOld);
		
		float alpha = 1.0f, lastTime = Time.time;
		while(alpha>0){
			alpha = 1.0f-((Time.time-lastTime)/plusFadeTime);
			t.color = new Color (t.color.r, t.color.g, t.color.b, alpha);
			
			t.rectTransform.anchoredPosition += Vector2.up*Time.deltaTime*28; //Magic parameter
			yield return 0;
			//
			if(GameManager.instance.roundState==GameManager.RoundState.END_SCREEN)break;
		}
		
		while (score!=scoreNew && score < GameManager.instance.scoreToWin) {
			score+=negative?-1:1;
			scores [team].text = score + "/" + GameManager.instance.scoreToWin;
			yield return 0;
		}
		
		yield return new WaitForSeconds(1);
		Destroy(t.gameObject);
	}
	
	public static void ShowStatsScreen(){
		instance.StartCoroutine(instance.StatsScreen());
	}
	
	IEnumerator StatsScreen(){
		SetStatus();
		//gameStatus.text = "Press any key";
		//setImageAlpha(statusLoader);
		
		//Get text objects (needs refactor)
		Text volleys	=transform.Find("Stats/Volleys").GetComponent<Text>();
		Text steals		=transform.Find("Stats/Steals").GetComponent<Text>();
		Text lKills		=transform.Find("Stats/LeaderKills").GetComponent<Text>();
		Text mKills		=transform.Find("Stats/MultiKills").GetComponent<Text>();
		Text spree		=transform.Find("Stats/LongestSpree").GetComponent<Text>();
		Text suicides	=transform.Find("Stats/Suicides").GetComponent<Text>();
		Text loser		=transform.Find("Stats/Loser").GetComponent<Text>();
		
		InitStat(volleys,"Rowballer",PlayerManager.Metric.VOLLEYS,"volley");
		InitStat(steals,"Thief",PlayerManager.Metric.STEALS,"steal");
		InitStat(lKills,"Assassin",PlayerManager.Metric.L_KILLS,"regicide");
		InitStat(mKills,"Shredder",PlayerManager.Metric.M_KILLS,"multi-kill");
		InitStat(spree,"Unstoppable",PlayerManager.Metric.SPREE,"spree",false);
		InitStat(suicides,"Clumsy",PlayerManager.Metric.SUICIDES,"suicide");
		InitStat(loser,"Goat",PlayerManager.Metric.LOSER,"point");
		
		//Just used for UI balance -- Can put more in of course, need to build a more dynamic system
		Text winner=transform.Find("Stats/Balance").GetComponent<Text>();
		InitStat(winner,"Boss",PlayerManager.Metric.SCORE,"");
		
		//Fade
		float a = 0, start = Time.time;
		
		while(Time.time<start+5){ //Magic number, need to actually check
			a+=Time.deltaTime;
			
			//Scores
			foreach(Text t in instance.scores)SetAlpha(t,1-a);
			
			//Stats
			SetAlpha(volleys,a-0.5f);
			SetAlpha(steals,a-1);
			SetAlpha(lKills,a-1.5f);
			SetAlpha(mKills,a-2);
			SetAlpha(spree,a-2.5f);
			SetAlpha(suicides,a-3);
			SetAlpha(loser,a-3.5f);
			SetAlpha(winner,a-4);
			
			/*
			if (statusLoader.fillAmount == 0) {
				statusLoader.fillClockwise = true;
				flip = 1;
			} else if (statusLoader.fillAmount == 1) {
				statusLoader.fillClockwise = false;
				flip = -1;
			}
			statusLoader.fillAmount += flip * (Time.deltaTime);
			// End game after input
			if (Input.anyKeyDown && !(Input.GetKeyDown (KeyCode.Mouse0) || Input.GetKeyDown (KeyCode.Mouse1) || Input.GetKeyDown (KeyCode.Mouse2))) {
				done = true;
				gameStatus.text = "Restarting";
				GameManager.instance.roundState = GameManager.RoundState.RESTARTING;
				GameManager.instance.EndGame ();
			}
			*/
			yield return 0;
		}
		GameManager.instance.roundState = GameManager.RoundState.RESTARTING;
		GameManager.instance.EndGame();
	}
	
	void InitStat(Text t, string s, PlayerManager.Metric m, string num, bool plural = true){
		t.text=s+": ";
		PlayerManager.PlayerInfo p = PlayerManager.TopPlayer(m);
		if(p!=null){
			t.text+="Player "+(p.order+1);
			string ps = plural?Mathf.Abs(p.GetMetric(m))!=1?"s":"":"";
			t.text+=(num==""?"":" ("+p.GetMetric(m)+" "+num+ps+")");
			t.color=PlayerManager.TeamColor(p);
		}
		else{
			t.text+="Not today...";
			t.color=Color.white;
		}
		SetAlpha(t,0);
	}
	
	public void SetAlpha(Text t, float a = 1){
		t.color=new Color(t.color.r,t.color.g,t.color.b,Mathf.Clamp01(a));
	}
	public void SetAlpha(Image i, float a = 1){
		i.color=new Color(i.color.r,i.color.g,i.color.b,Mathf.Clamp01(a));
	}
	
	void Start(){
		SetStatus();
		
		viewCamera=GameObject.Find("CameraRig").transform.GetChild(0).GetComponent<Camera>();
		//statusLoader.fillAmount = 0f;
		int teams = (GameManager.instance.teams==0?PlayerManager.instance.ActivePlayerCount():GameManager.instance.teams);
		scores = new Text[teams];
		float offset = (640) / (teams - 1);
		//Score texts
		for (int i = 0; i < teams; i++) {
			Text t = Instantiate(scoreTextPrefab,Vector3.zero,Quaternion.identity) as Text;
			t.transform.SetParent (transform, false);
			Vector2 screenPos = new Vector2 ((-320 + offset*(i)), -25f);
			t.rectTransform.anchoredPosition = screenPos;
			t.color = PlayerManager.TeamColor (i);
			t.text = "0/" + GameManager.instance.scoreToWin;
			scores [i] = t;
		}
	}
	
	public void Update(){
		//Regenerate text objects if cache is stale
		PlayerManager.PlayerInfo[] players = PlayerManager.ActivePlayers();
		if(PlayerManager.HasChanged(ref playerCache)){
			//Clear
			foreach(TextOverlay t in texts)if(t!=null && t.gui!=null){
				if(!Application.isEditor || Application.isPlaying)Destroy(t.gui.gameObject);
				else DestroyImmediate(t.gui.gameObject);
			}
			
			//Populate and parent
			if(players.Length>0){
				texts = new TextOverlay[players.Length];
				for(int i=0;i<texts.Length;++i)if(players[i]!=null){
					texts[i]=new TextOverlay(4.6f,2f);
					//Instantiate for each ship
					texts[i].gui = Instantiate(shipTextPrefab,Vector3.zero,Quaternion.identity) as Text;
					texts[i].gui.gameObject.name="Text Overlay (Player "+(players[i].order+1)+")";
					//Container
					texts[i].gui.transform.SetParent(shipTextContainer, false);
				}
			}
		}
		
		for(int i=0;i<texts.Length;++i)if(texts[i]!=null){
			TextOverlay t = texts[i];
			
			//Update position, scale, color, and text
			Vector2 screenPos = players[i].position-Vector2.up*(players[i].player.ship.size+offset)+Vector2.right*xOffset;
			screenPos.y -= 10.0f;
			screenPos=viewCamera.WorldToScreenPoint(screenPos);
			t.gui.transform.position = screenPos;
			
			//Scale (uses zoom range constants)
			float scale = Mathf.Lerp(size.x,size.y,1-((CameraController.zoom-32)/(256-32)));
			t.gui.GetComponent<RectTransform>().localScale=new Vector3(scale,scale,1);
			
			t.color=PlayerManager.TeamColor(players[i]);
			
			/*
			if(players[i].totaled){ //Totaled
				//Reset
				if(t.deathMessage==""){
					t.deathMessage=deathMessages[Random.Range(0,deathMessages.Length)];
					//Set blink start time so that the blink doesn't cut out early
					if((Time.time+blinkTime)%(blinkRate*2)>blinkRate)t.blinkStartTime=Time.time-blinkRate;
					else t.blinkStartTime=Time.time;
				}
				t.text=t.deathMessage;
				bool blink = Time.time-t.blinkStartTime<blinkTime?Time.time%(blinkRate*2)>blinkRate:false;
				t.alpha=blink?1:0;
			}
			*/
			if(PlayerManager.Winner(players[i])){ //Winner
				t.text=GameManager.instance.scoreMode==GameManager.ScoreMode.GOAL?"SCORE!":"VICTORY";
				bool blink = Time.time%(blinkRate*2)>blinkRate;
				t.alpha=blink?1:0;
			}
			else{ //Normal behavior, displays controls
				t.deathMessage=""; //Reset death message for respawn
				
				if(GameManager.instance.roundState == GameManager.RoundState.SETUP &&
					GameManager.instance.round == -1){
					if(!players[i].ready){
                            t.text =
                            "Player " + players[i].player.control.controller;
							//players[i].player.control.leftOars+" | "+
							//players[i].player.control.cannon+" | "+
							//players[i].player.control.rightOars;//+
							//"\nReady up";
						//t.text+=
							//Time.time%(readyUpRate*4)>readyUpRate*3?"...":
							//Time.time%(readyUpRate*4)>readyUpRate*2?"..":
							//Time.time%(readyUpRate*4)>readyUpRate*1?".":
							//"";
					}
					else t.text="READY";
					finishedSetup = false;
				} else {
					if (!finishedSetup) {
						finishedSetup = true;
						//GameManager.instance.roundState = GameManager.RoundState.PLAYING;
						startTime = Time.time;
					}
					t.text = "";
				}
			}
		}

        if (GameManager.instance.roundState == GameManager.RoundState.SETUP &&
            GameManager.instance.round == -1) {
            //statusLoader.enabled = true;
            gameStatus.text = "Warmup \n" + PlayerManager.playersReady + "/" + PlayerManager.ActivePlayers().Length + " Players Ready";
            //if (statusLoader.fillAmount == 0) {
            //    statusLoader.fillClockwise = true;
            //    flip = 1;
            //} else if (statusLoader.fillAmount == 1) {
            //    statusLoader.fillClockwise = false;
            //    flip = -1;
            //}
            //statusLoader.fillAmount += flip * (Time.deltaTime);
            finishedSetup = false;
        } else {
            if (!finishedSetup) {
                finishedSetup = true;
                statusBg.gameObject.SetActive(false);
                startTime = Time.time;
            }
            if (finishedSetup && GameManager.instance.freezeTimer > -goTime) {
                statusBg.gameObject.SetActive(false);
                // Handle status text
                if (GameManager.instance.roundState == GameManager.RoundState.PLAYING) {
                    if (statusUpdated == false) {
                        lastTime = Time.time;
                        statusLoader.fillAmount = 0;
                        statusLoader.fillClockwise = true;
                        for (int i = 0; i < texts.Length; ++i)
                            if (texts[i] != null)
                                texts[i].text = "";

                        SetStatus(true);
                        statusUpdated = true;
                        gameObjective.gameObject.SetActive(false);
                    }
                }
                if (GameManager.instance.freezeTimer > 0) {
                    gameStatus.text = "Waiting...";
                    readyImg.gameObject.SetActive(true);
                } else {
                    readyImg.gameObject.SetActive(false);
                    gameStatus.text = "GO!";
                    SetAlpha(statusLoader, 0);
                    goImg.gameObject.SetActive(true);
                }

                // Keep the loading circle moving
                statusLoader.fillAmount = (Time.time - lastTime) / GameManager.instance.freezeTime;
            }
            if (Time.time - startTime > visibleTime && (GameManager.instance.roundState == GameManager.RoundState.PLAYING || GameManager.instance.roundState == GameManager.RoundState.STARTING_PLAY)) { 
                SetAlpha(gameStatus, 0);
                goImg.gameObject.SetActive(false);
            }
			if (GameManager.instance.roundState == GameManager.RoundState.RESTARTING) {
				SetAlpha(gameStatus);
				SetAlpha(statusLoader);
			}
		}
		for (int i = 0; i < scores.Length; i++) 
			if(PlayerManager.TeamScore(i) >= GameManager.instance.scoreToWin * closeToWinPercent)
				scores[i].fontSize = scoreTextPrefab.fontSize + (int)(4 * ((1 + Mathf.Sin (Time.time * 15) / 2)));
	}
	void SetStatus(bool center = false){
		if(center){
			gameStatus.alignment = TextAnchor.MiddleCenter;
			gameStatus.rectTransform.position = viewCamera.WorldToScreenPoint(Vector3.zero);
			gameStatus.fontSize = 28;
			
			statusLoader.rectTransform.position = viewCamera.WorldToScreenPoint(Vector3.zero);
			statusLoader.rectTransform.localScale=Vector3.one*8;
		}
		else{
			gameStatus.alignment = TextAnchor.MiddleRight;
			gameStatus.rectTransform.anchoredPosition = new Vector3(-123.2f,32,0);
			gameStatus.fontSize = 14;
			
			statusLoader.rectTransform.anchoredPosition = new Vector3(-28.2f,32,0);
			statusLoader.rectTransform.localScale=Vector3.one*0.8f;
		}
	}
}