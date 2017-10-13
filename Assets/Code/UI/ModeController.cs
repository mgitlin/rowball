using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ModeController : MonoBehaviour, ISelectHandler, IDeselectHandler
{

    [Header("Parent Menu")]
    public PlayMenu playMenu;

    [Header("Selection")]
    public Text selectedModeName;
    public GameManager[] modes;

    bool selected = false;
    float delay = 0.25f;
    float lastPress = 0.0f;

    [HideInInspector]
    public int currentMode = 0;

    public void OnSelect(BaseEventData eventData)
    {
        selected = true;
    }

    public void OnDeselect(BaseEventData eventData)
    {
        selected = false;
    }

    public void ModeUp()
    {
        if (currentMode == modes.Length - 1)
        {
            currentMode = 0;
        }
        else
        {
            currentMode++;
        }
        UpdateSelection();
    }

    public void ModeDown()
    {
        if(currentMode == 0)
        {
            currentMode = modes.Length - 1;
        }
        else
        {
            currentMode--;
        }
        UpdateSelection();
    }

    public void UpdateSelection()
    {
        selectedModeName.text = modes[currentMode].modeName;
    }

    // Use this for initialization
    void Start () {
        UpdateSelection();
	}
	
	// Update is called once per frame
	void Update () {
        if ((lastPress == 0.0f || Time.time - lastPress > delay) && selected)
        {
            if (Input.GetAxis("Horizontal") == 1)
            {
                ModeUp();
                playMenu.PlayButtonSelect();
                lastPress = Time.time;
            }
            else if (Input.GetAxis("Horizontal") == -1)
            {
                ModeDown();
                playMenu.PlayButtonSelect();
                lastPress = Time.time;
            }
        }
    }
}
