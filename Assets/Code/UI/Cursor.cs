using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Cursor : MonoBehaviour {

    [Header("Animation")]
    public Sprite[] animationFrames;
    int currentFrame; // Current animation frame being displayed

    Image image;

    public void ActionSubmitted ()
    {
        StartCoroutine(ActionSubmittedAnimation());
    }

    IEnumerator ActionSubmittedAnimation()
    {
        for (int i = 0; i < animationFrames.Length; i++)
        {
            currentFrame = i;
            yield return new WaitForSeconds(0.1f);
        }
        currentFrame = 0;
    }

    // Use this for initialization
    void Start () {
        image = GetComponent<Image>();
	}
	
	// Update is called once per frame
	void Update () {
        image.sprite = animationFrames[currentFrame];
    }
}
