/**
 * MainMenu.cs
 * by Matthew Gitlin
**/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MainMenu : MonoBehaviour {

    //Singleton
    private static MainMenu mainMenu = null;
    public static MainMenu instance
    {
        get
        {
            if (mainMenu == null)
            {
                mainMenu = (MainMenu)FindObjectOfType(typeof(MainMenu));
                if (FindObjectsOfType(typeof(MainMenu)).Length > 1) Debug.Log("More than one MainMenu instance in the scene!");
            }
            return mainMenu;
        }
    }

    [Header("Cursor")]
    public Cursor cursor;
    [Header("Buttons")]
    public Button playButton;
    public Button optionsButton;
    [Header("Connected Menus")]
    public PlayMenu playMenu;
    public OptionsMenu optionsMenu;
    [Header("Audio Sources")]
    public AudioSource buttonSelect;
    public AudioSource buttonSubmit;

    public void Open()
    {
        GetComponent<Animator>().Play("OpenCanvas");
    }

    public void Close()
    {
        GetComponent<Animator>().Play("CloseCanvas");
    }

    public void UpdateCursorLocation(Transform t)
    {
        Vector3 newPos = t.position;
        newPos.x = t.position.x - 115.0f;
        cursor.transform.position = newPos;
    }

    public void PlayButtonSelect()
    {
        buttonSelect.Play();
    }

    public void PlayButtonSubmit()
    {
        cursor.ActionSubmitted();
        buttonSubmit.Play();
    }

    public void PlayMenu()
    {
        playMenu.gameObject.SetActive(true);
        playMenu.Open();
    }

    public void OptionsMenu ()
    {
        optionsMenu.gameObject.SetActive(true);
        optionsMenu.Open();
    }
    
}
