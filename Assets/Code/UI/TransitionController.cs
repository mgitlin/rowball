/**
 * TransitionController.cs
 * by Matthew Gitlin
**/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionController : MonoBehaviour {

    //Singleton
    private static TransitionController transitionController = null;
    public static TransitionController instance
    {
        get
        {
            if (transitionController == null)
            {
                transitionController = (TransitionController)FindObjectOfType(typeof(TransitionController));
                if (FindObjectsOfType(typeof(TransitionController)).Length > 1) Debug.Log("More than one TransitionController instance in the scene!");
            }
            return transitionController;
        }
    }

    [HideInInspector]
    public Animator animator;

    public void Transition ()
    {
        animator.Play("ScreenTransitionOpen", 0);
    }

    void Start()
    {
        animator = GetComponent<Animator>();
    }
}
