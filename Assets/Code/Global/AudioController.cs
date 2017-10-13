//AudioController.cs
//Created by Aaron C Gaudette on 08.04.16
//Plays audio based on game events

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioController : MonoBehaviour{

    //Singleton (private)
    public static AudioController audioController = null;
    public static AudioController instance
    {
        get
        {
            if (audioController == null)
            {
                audioController = (AudioController)FindObjectOfType(typeof(AudioController));
                if (FindObjectsOfType(typeof(AudioController)).Length > 1) Debug.Log("More than one AudioController instance in the scene!");
            }
            return audioController;
        }
    }

    public AudioSource bgm;
    public AudioClip[] bgmClips;
    public AudioSource buttonSelect;
    public AudioSource buttonSubmit;
    int lastClip = -1;
    bool fadingMusic = false;

    public float slomoScale = 0.5f;
	
	public float cannonThreshold = 0.35f;
	public Vector2 cannonWeakPitchRange = new Vector2(0.9f,1.1f);
	public Vector2 cannonPitchRange = new Vector2(0.8f,1.2f);
	
	public Vector2 smashPitchRange = new Vector2(0.9f,1.1f);
	public AudioSource smash;
	
	public Vector2 chargeRange = new Vector2(0,1);
	public AudioSource charge;
	
	public AudioSource voice;
    public AudioClip[] killClips;
    public AudioClip afterSuicide;
	public AudioClip doubleKillClip, tripleKillClip, leaderKillClip, suicideClip;
	
	List<AudioSource> sources = null;
	List<float> pitches = null;
//	float lastChargeTime = 0;
	
	void Start(){
		sources = new List<AudioSource>();
		pitches = new List<float>();

        UpdateBGMVolume();

        AddMusic((int)(Random.Range(0, 2)));
        bgm.loop = true;

        //Observe
        if (!Application.isEditor || Application.isPlaying){
			Observer.OnKill.AddListener(OnKill);
			Observer.OnFire.AddListener(OnFire);
			Observer.OnFireBegin.AddListener(OnFireBegin);
			//Observer.OnGoal.AddListener(OnGoal);
			
			Observer.OnDoubleKill.AddListener(OnDoubleKill);
			Observer.OnTripleKill.AddListener(OnTripleKill);
			Observer.OnLeaderKill.AddListener(OnLeaderKill);
			Observer.OnSuicide.AddListener(OnSuicide);
            Observer.OnRespawnAfterSuicide.AddListener(OnRespawnAfterSuicide);
		}
	}
	//Slomo
	void Update(){
		for(int i=0;i<sources.Count;++i){
			if(Time.timeScale==1){
				if(pitches[i]==0)pitches[i]=sources[i].pitch;
				if(pitches[i]!=-1)sources[i].pitch=pitches[i];
			}
			else if(pitches[i]!=-1)sources[i].pitch=pitches[i]*slomoScale;
		}

        if (!bgm.isPlaying && !fadingMusic)
        {
            AddMusic((int)(Random.Range(2, bgmClips.Length)));
        }
	}
	//Sound management
	AudioSource AddSound(AudioSource a, float specificVolume, float pitch = -1, bool loop = false, bool ignoreSlomo = false){
		if(pitch==-1)pitch=a.pitch;
		GameObject o = Instantiate(a.gameObject,Vector2.zero,Quaternion.identity) as GameObject;
		o.name=a.gameObject.name+" (Sound)";
		o.transform.parent=soundContainer;
		
		AudioSource oa = o.GetComponent<AudioSource>();
		oa.pitch=pitch;
        oa.volume = oa.volume * specificVolume * PlayerPrefs.GetFloat("masterVolume", 1.0f);
		pitches.Add(ignoreSlomo?-1:0);
		sources.Add(oa);
		
		if(!loop){
			DestroySound(oa,a.clip.length*2);
			return null;
		}
		else return oa;
	}
	void DestroySound(AudioSource a, float delay = 0){StartCoroutine(DeleteSound(a,delay));}
	IEnumerator DeleteSound(AudioSource a, float delay){
		yield return new WaitForSeconds(delay);
		Destroy(a.gameObject);
		pitches.RemoveAt(sources.IndexOf(a));
		sources.Remove(a);
	}
	
	Transform container = null;
	Transform soundContainer{get{
		if(container==null){
			foreach(Transform t in GetComponentsInChildren<Transform>()){
				if(t.gameObject.name=="Sounds"){
					container=t;
					return t;
				}
			}
			GameObject child = new GameObject("Sounds");
			child.transform.parent=transform;
			container=child.transform;
			return child.transform;
		}
		return container;
	}}
	
	//Events
	void OnKill(ShipRevamped target, Rowball r){
		AddSound(target.destroyExplosion, PlayerPrefs.GetFloat("effectsVolume", 1.0f), Random.Range(smashPitchRange.x,smashPitchRange.y));
        AddEffect(killClips[(int)(Random.Range(0, killClips.Length))]);
	}
	void OnFire(CannonRevamped c/*, Vector2 recoil*/){
        //if(s.cannon.charge>cannonThreshold){
        //	pitch=Mathf.Lerp(cannonPitchRange.x,cannonPitchRange.y,(s.cannon.charge-cannonThreshold)/(1-cannonThreshold));
        //	cannon.clip=cannonClips[Random.Range(0,cannonClips.Length)];
        //	cannonDry.clip=cannonDryClip;
        //	AddSound(s.hasRowball?cannon:cannonDry,pitch);
        //}
        //else{
        //	pitch=Mathf.Lerp(cannonWeakPitchRange.x,cannonWeakPitchRange.y,s.cannon.charge/cannonThreshold);
        //	if(s.hasRowball){
        //		cannon.clip=cannonWeakClip;
        //		AddSound(cannon,pitch);
        //	}
        //	else{
        //		cannonDry.clip=cannonWeakDryClip;
        //		AddSound(cannonDry,pitch);
        //	}
        //}
        if (c.loaded)
        {
            AddSound(c.fireSoundLoaded, PlayerPrefs.GetFloat("effectsVolume", 1.0f));
        }
        else
        {
            AddSound(c.fireSoundDry, PlayerPrefs.GetFloat("effectsVolume", 1.0f));
        }


	}
	void OnFireBegin(ShipRevamped s){StartCoroutine(ChargeCannon(s));}
	IEnumerator ChargeCannon(ShipRevamped s){
		while(s.cannon.charge==0)yield return 0; //Wait for input
		
//		float pitch = chargeRange.x;
//		lastChargeTime=Time.time;
		AudioSource a = AddSound(charge, PlayerPrefs.GetFloat("effectsVolume", 1.0f), -1,true);
		while(s.cannon.charge>0 && s!=null){
			pitches[sources.IndexOf(a)]=Mathf.Lerp(chargeRange.x,chargeRange.y,s.cannon.charge);
			yield return 0;
		}
		DestroySound(a);
	}
	//void OnGoal(Ship s){AddSound(goal, PlayerPrefs.GetFloat("effectsVolume", 1.0f));}

    void OnRespawnAfterSuicide() { AddEffect(afterSuicide);}


    void OnDoubleKill(){ AddEffect(doubleKillClip);}
	void OnTripleKill(){ AddEffect(tripleKillClip);}
	void OnLeaderKill(){ AddEffect(leaderKillClip);}
	void OnSuicide(){ AddEffect(suicideClip);}

    public void UpdateBGMVolume()
    {
        bgm.volume = PlayerPrefs.GetFloat("musicVolume", 1.0f);
    }

    public void UpdateEffectsVolume()
    {
        buttonSelect.volume = PlayerPrefs.GetFloat("effectsVolume", 1.0f);
        buttonSubmit.volume = PlayerPrefs.GetFloat("effectsVolume", 1.0f);
    }

    void AddEffect(AudioClip clip){
        if (clip != null)
        {
            voice.clip = clip;
            AddSound(voice, PlayerPrefs.GetFloat("effectsVolume", 1.0f), -1, false, true);
        }
	}
    public void AddMusic(int clipIdx, float delay = 0)
    {
        while (clipIdx == lastClip)
        {
            clipIdx = Random.Range(2, bgmClips.Length);
        }
        lastClip = clipIdx;
        bgm.clip = bgmClips[clipIdx];
        bgm.loop = false;
        bgm.Play();
    }

    public void ChangeMusic(int clipIdx, float delay = 1.0f)
    {
        while (clipIdx == lastClip)
        {
            clipIdx = Random.Range(2, bgmClips.Length);
        }
        lastClip = clipIdx;
        StartCoroutine(ChangeMusicFade(bgmClips[clipIdx], delay));
    }

    public IEnumerator ChangeMusicFade(AudioClip clip, float delay)
    {
        fadingMusic = true;
        //Fade time
        float volume = bgm.volume, lastTime = Time.time;
        while (bgm.volume > 0)
        {
            bgm.volume = volume - ((Time.time - lastTime) / 5.0f);
            yield return 0;
        }
        bgm.Stop();
        bgm.clip = clip;
        bgm.loop = false;
        yield return new WaitForSeconds(delay);
        bgm.Play();
        lastTime = Time.time;
        while (bgm.volume < volume)
        {
            bgm.volume = 0 + ((Time.time - lastTime) / 5.0f);
            yield return 0;
            if (bgm.volume == volume)
                break;
        }
        fadingMusic = false;
    }
}