using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayMenu : MonoBehaviour {

    //Singleton
    private static PlayMenu playMenu = null;
    public static PlayMenu instance
    {
        get
        {
            if (playMenu == null)
            {
                playMenu = (PlayMenu)FindObjectOfType(typeof(PlayMenu));
                if (FindObjectsOfType(typeof(PlayMenu)).Length > 1) Debug.Log("More than one PlayMenu instance in the scene!");
            }
            return playMenu;
        }
    }

    [Header("Buttons")]
    public Button playButton;
    public Button backButton;
    [Header("Controllers")]
    public ModeController modeController;
    public MapController mapController;
    [Header("Audio")]
    public AudioSource buttonSelect;
    public AudioSource buttonSubmit;
    [Header("Connected Menus")]
    public MainMenu mainMenu;

    

    public void Open() {
        mainMenu.Close();
        modeController.currentMode = 0;
        modeController.UpdateSelection();
        mapController.currentMap = 0;
        mapController.UpdateSelection();
        GetComponent<Animator>().Play("OpenCanvas");
        modeController.GetComponent<Button>().Select();
    }

    public void Close()
    {
        GetComponent<Animator>().Play("CloseCanvas");
        mainMenu.Open();
        mainMenu.playButton.Select();
    }

    public void PlayButtonSelect()
    {
        buttonSelect.Play();
    }

    public void PlayButtonSubmit()
    {
        buttonSubmit.Play();
    }

    public void StartGame()
    {
        mainMenu.gameObject.SetActive(false);
        Close();
        StartCoroutine(GameController.instance.InitGame(modeController.modes[modeController.currentMode], mapController.maps[mapController.currentMap]));
    }

    

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
