/**
 * GameController.cs
 * by Matthew Gitlin
**/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

    //Singleton
    private static GameController gameController = null;
    public static GameController instance
    {
        get
        {
            if (gameController == null)
            {
                gameController = (GameController)FindObjectOfType(typeof(GameController));
                if (FindObjectsOfType(typeof(GameController)).Length > 1) Debug.Log("More than one GameInitializer instance in the scene!");
            }
            return gameController;
        }
    }

    public Canvas gui;
    public Camera boundsCamera;
    
    public static WorldManager map = null;
    public static GameManager game = null;
    public static Canvas gameUI = null;

    public bool paused = false;

    public IEnumerator InitGame(GameManager mode, WorldManager world)
    {
        TransitionController.instance.Transition();
        AudioController.instance.ChangeMusic((int)(Random.Range(2, AudioController.instance.bgmClips.Length)));

        yield return new WaitForSeconds(1.5f);

        ScreenShake s = GameObject.Find("Camera").GetComponent<ScreenShake>();
        s.ColorFade(s.day, 1);
        if (map) DestroyImmediate(map.gameObject);
        if (game) DestroyImmediate(game.gameObject);
        if (gameUI) DestroyImmediate(gameUI.gameObject);
        map = Instantiate(world, Vector3.zero, Quaternion.identity) as WorldManager;
        game = Instantiate(mode, Vector3.zero, Quaternion.identity) as GameManager;
        gameUI = Instantiate(gui, Vector3.zero, Quaternion.identity) as Canvas;
    }

    void PauseGame()
    {
        paused = true;
        //CanvasController.instance.OpenCanvas(CanvasController.instance.pauseMenu);
    }

    public void ResumeGame()
    {
        paused = false;
       // CanvasController.instance.CloseCanvas(CanvasController.instance.pauseMenu);
    }

    public void ReturnToMain()
    {
        StartCoroutine(ReturnToMainMenu());
    }

    IEnumerator ReturnToMainMenu()
    {
        TransitionController.instance.Transition();
        AudioController.instance.ChangeMusic((int)(Random.Range(0, 2)));
        AudioController.instance.bgm.loop = true;
        yield return new WaitForSeconds(1.0f);
        //.instance.CloseCanvas(CanvasController.instance.pauseMenu);
        ScreenShake s = GameObject.Find("Camera").GetComponent<ScreenShake>();
        s.ColorFade(s.sunset, 1);
        Destroy(map.gameObject);
        Destroy(gameUI.gameObject);
        Destroy(game.gameObject);
        map = null;
        gameUI = null;
        game = null;
        ObjectManager.Flush();
        paused = false;
        //CanvasController.instance.gameUI = null;
        //CanvasController.instance.OpenCanvas(CanvasController.instance.mainMenu);
    }

    private void Update()
    {
        if (Input.GetKeyDown("p"))
            if (!paused)
                PauseGame();
            else
                ResumeGame();
    }
}
