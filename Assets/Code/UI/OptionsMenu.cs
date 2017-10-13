/**
 * OptionsMenu.cs
 * by Matthew Gitlin
**/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour {

    //Singleton
    private static OptionsMenu optionsMenu = null;
    public static OptionsMenu instance
    {
        get
        {
            if (optionsMenu == null)
            {
                optionsMenu = (OptionsMenu)FindObjectOfType(typeof(OptionsMenu));
                if (FindObjectsOfType(typeof(OptionsMenu)).Length > 1) Debug.Log("More than one OptionsMenu instance in the scene!");
            }
            return optionsMenu;
        }
    }

    [Header("Buttons")]
    public Button audioButton;
    public Button controlButton;
    public Button backButton;
    [Header("Panels")]
    public GameObject audioPanel;
    public GameObject controlPanel;
    [Header("Controllers")]
    public VolumeController musicVolume;
    public VolumeController effectsVolume;
    [Header("Connected Menus")]
    public MainMenu mainMenu;
    public PauseMenu pauseMenu;
    [Header("Audio Sources")]
    public AudioSource buttonSelect;
    public AudioSource buttonSubmit;

    public void Open()
    {
        mainMenu.Close();
        GetComponent<Animator>().Play("OpenCanvas");
        AudioButtonSelect();
    }

    public void Close()
    {
        GetComponent<Animator>().Play("CloseCanvas");
        if (GameManager.instance != null)
        {

        }
        else
        {
            mainMenu.Open();
            mainMenu.optionsButton.Select();
        }
    }

    public void PlayButtonSelect()
    {
        buttonSelect.Play();
    }

    public void PlayButtonSubmit()
    {
        buttonSubmit.Play();
    }

    public void AudioButtonSelect()
    {
        audioButton.Select();
    }

    public void AudioPanelOpen()
    {
        audioPanel.gameObject.SetActive(true);
    }

    public void AudioPanelClose()
    {
        audioPanel.gameObject.SetActive(false);
    }

    public void AudioPanelSelect()
    {
        musicVolume.GetComponent<Button>().Select();
    }

    public void ControlButtonSelect()
    {
        controlButton.Select();
    }

    public void ControlPanelOpen()
    {
        controlPanel.gameObject.SetActive(true);
    }

    public void ControlPanelClose()
    {
        controlPanel.gameObject.SetActive(false);
    }

    public void ControlPanelSelect()
    {

    }
    
}
