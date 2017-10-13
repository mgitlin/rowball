using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour {

    //Singleton
    private static PauseMenu pauseMenu = null;
    public static PauseMenu instance
    {
        get
        {
            if (pauseMenu == null)
            {
                pauseMenu = (PauseMenu)FindObjectOfType(typeof(PauseMenu));
                if (FindObjectsOfType(typeof(PauseMenu)).Length > 1) Debug.Log("More than one PauseMenu instance in the scene!");
            }
            return pauseMenu;
        }
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
