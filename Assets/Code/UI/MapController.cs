using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MapController : MonoBehaviour, ISelectHandler, IDeselectHandler
{

    [Header("Parent Menu")]
    public PlayMenu playMenu;

    [Header("Selection")]
    public Text selectedMapName;
    public Image selectedMapPreview;
    public WorldManager[] maps;

    bool selected = false;
    float delay = 0.25f;
    float lastPress = 0.0f;

    [HideInInspector]
    public int currentMap = 0;

    public void OnSelect(BaseEventData eventData)
    {
        selected = true;
    }

    public void OnDeselect(BaseEventData eventData)
    {
        selected = false;
    }

    public void MapUp()
    {
        if (currentMap == maps.Length - 1)
        {
            currentMap = 0;
        }
        else
        {
            currentMap++;
        }
        UpdateSelection();
    }

    public void MapDown()
    {
        if (currentMap == 0)
        {
            currentMap = maps.Length - 1;
        }
        else
        {
            currentMap--;
        }
        UpdateSelection();
    }

    public void UpdateSelection()
    {
        selectedMapName.text = maps[currentMap].mapName;
        selectedMapPreview.sprite = maps[currentMap].preview;
    }

    // Use this for initialization
    void Start()
    {
        UpdateSelection();
    }

    // Update is called once per frame
    void Update()
    {
        if ((lastPress == 0.0f || Time.time - lastPress > delay) && selected)
        {
            if (Input.GetAxis("Horizontal") == 1)
            {
                MapUp();
                playMenu.PlayButtonSelect();
                lastPress = Time.time;
            }
            else if (Input.GetAxis("Horizontal") == -1)
            {
                MapDown();
                playMenu.PlayButtonSelect();
                lastPress = Time.time;
            }
        }
    }
}
