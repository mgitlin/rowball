/**
 * VolumeController.cs
 * by Matthew Gitlin
**/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class VolumeController : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    public OptionsMenu optionsMenu;
    public Sprite defaultTickSprite;
    public Sprite highlightedTickSprite;
    public enum VolumeMode { MUSIC, EFFECTS }
    public VolumeMode mode;
    public Image[] ticks;

    bool selected = false;
    float delay = 0.25f;
    float lastPress = 0.0f;

    [Header("Read-only")]
    public float volume = 1.0f;

    public void OnSelect(BaseEventData eventData)
    {
        foreach(Image i in ticks)
        {
            i.sprite = highlightedTickSprite;
        }
        selected = true;
    }

    public void OnDeselect(BaseEventData eventData)
    {
        foreach (Image i in ticks)
        {
            i.sprite = defaultTickSprite;
        }
        selected = false;
    }

    public void VolUp()
    {
        if (volume < 1.0f) { volume += 0.1f; }
        for (float i = 0.0f; i < (volume * 10.0f); i++) { ticks[(int)i].enabled = true; }
        if (mode == VolumeMode.MUSIC) { PlayerPrefs.SetFloat("musicVolume", volume); AudioController.instance.UpdateBGMVolume(); }
        else if (mode == VolumeMode.EFFECTS) { PlayerPrefs.SetFloat("effectsVolume", volume); AudioController.instance.UpdateEffectsVolume(); }
        PlayerPrefs.Save();
    }

    public void VolDown()
    {
        if (volume > 0.0f) { volume -= 0.1f; }
        for (float i = 9.0f; i > (volume * 10.0f); i--) { ticks[(int)i].enabled = false; }
        if (mode == VolumeMode.MUSIC) { PlayerPrefs.SetFloat("musicVolume", volume); AudioController.instance.UpdateBGMVolume(); }
        else if (mode == VolumeMode.EFFECTS) { PlayerPrefs.SetFloat("effectsVolume", volume); AudioController.instance.UpdateEffectsVolume(); }
        PlayerPrefs.Save();
    }

    // Use this for initialization
    void Start () {
		if (mode == VolumeMode.MUSIC) { volume = PlayerPrefs.GetFloat("musicVolume", 1.0f); AudioController.instance.UpdateBGMVolume(); }
        else if (mode == VolumeMode.EFFECTS) { volume = PlayerPrefs.GetFloat("effectsVolume", 1.0f); AudioController.instance.UpdateEffectsVolume(); }

        for (float i = 9.0f; i > (volume * 10.0f); i--) { ticks[(int)i].enabled = false; }
    }

    private void Update()
    {
        if((lastPress == 0.0f || Time.time - lastPress > delay) && selected)
        {
            if (Input.GetAxis("Horizontal") == 1)
            {
                VolUp();
                optionsMenu.PlayButtonSelect();
                lastPress = Time.time;
            }
            else if (Input.GetAxis("Horizontal") == -1)
            {
                VolDown();
                optionsMenu.PlayButtonSelect();
                lastPress = Time.time;
            }
        }
    }
}
