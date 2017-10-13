/*
 * CannonRevamped.cs 
 * Written by Matthew Gitlin
 * Based on Cannon.cs by Aaron C Gaudette
 * Cannon point attachment for ships
 * 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonRevamped : MonoBehaviour {

    [Header("Appearance")]
    public bool useSprite = true;
    public Color color = Color.white;

    [Header("Statistic Values")]
    [Range(0, 1)] public float charge = 0; // Current amount of charge
    //public float chargeTime = 3.0f; // Amount of time (seconds) to reach full charge
    public float chargeSpeed = 0.4f; // Speed multiplier for charge increase
    public float maxChargeTime = 1.0f; // Amount of time (seconds) allowed to stay at max charge
    public float cooldownTime = 2.0f; // Time before rowball can be reloaded/fired
    public bool loaded = false;
    public int firepower = 9000;
    public int recoil = 1500;


    [Header("Animation")]
    public Sprite[] animationFrames;
    public int finalChargeFrame; // Cut off between charging and firing frames
    int currentFrame; // Current animation frame being displayed

    [HideInInspector] public Vector3 heading;
    public Vector3 position
    {
        get { return transform.position; }
        set { transform.position = value; }
    }


    [Header("References")]
    public ShipRevamped ship;
    public AudioSource fireSoundLoaded;
    public AudioSource fireSoundDry;
    SpriteRenderer spriteRenderer;
    LineRenderer[] lines;
    bool firing = false;
    float chargeStartTime = -1.0f;
    [HideInInspector]
    public float lastFireTime;
    bool maxCharge = false;
    float maxChargeStartTime = -1.0f;

    public void Charge()
    {
        if (Time.time - lastFireTime > cooldownTime) { // Disallow charging if cannon is on cooldown
            if (chargeStartTime == -1.0f)
            {
                chargeStartTime = Time.time;
            }
            else
            {
                charge = Mathf.Min(Mathf.Max((Time.time - chargeStartTime) * chargeSpeed, 0.1f), 1.0f);

                // Take timestamp of when cannon reaches max charge 
                // (used to prevent ship holding max charge indefinitely)
                if (charge >= 1.0f && !maxCharge)
                {
                    maxCharge = true;
                    maxChargeStartTime = Time.time;
                }

                if (maxChargeStartTime != -1.0f)
                {
                    if (Time.time - maxChargeStartTime > maxChargeTime)
                    {
                        this.ship.Overload(); // Freeze ship for overcharging
                    }
                }
            }
        }
    }

    public void Fire(float charge)
    {
        //Debug.Break();
        if (Time.time - lastFireTime > cooldownTime)
        {
            if (!firing) // Prevent calling this in multiple consecutive frames
            {
                firing = true;
                //Debug.Log("Fire!");

                // Reset variables
                chargeStartTime = -1.0f;
                maxCharge = false;
                maxChargeStartTime = -1.0f;

                // Fire a rowball if loaded
                if (loaded)
                {
                    Vector3 aiming3 = new Vector3(ship.aiming.x, ship.aiming.y);
                    Vector3 spawnPosition = position + aiming3;
                    Rowball r = ObjectManager.AddRowball(spawnPosition, Color.white);
                    r.level = (int)(charge*(float)(r.frames.Length - 1));
                    r.AddImpulse(charge * ship.aiming * firepower);
                    r.shooter = ship;
                }

                // Apply recoil to the ship
                Vector2 recoilVector = charge * recoil * -ship.aiming;
                ship.AddImpulse(recoilVector);

                // Start firing animation
                StartCoroutine(FireRowballAnimation());
            }
        }
    }

    // Frame animation for cannon fire
    IEnumerator FireRowballAnimation()
    {
        Observer.OnFire.Invoke(this);
        ship.hasRowball = false;
        for (int i = finalChargeFrame + 1; i < animationFrames.Length - 1; i++)
        {
            currentFrame = i;
            yield return new WaitForSeconds(0.1f);
        }
        currentFrame = 0;
        firing = false;
        lastFireTime = Time.time;
        charge = 0;
    }

    void Start()
    {
        if (!useSprite)
        {
            lines = GetComponentsInChildren<LineRenderer>();
            foreach (LineRenderer l in lines)
            {
                l.startColor = color;
                l.endColor = color;
            }
        }
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        //Orientation
        if (!Application.isEditor || Application.isPlaying)
        {
            //this.transform.rotation.z = heading.z;
            transform.LookAt(transform.position + heading, -Vector3.forward);
            transform.Rotate(90, 0, 0); //Correct manually
        }

        //Animation frame selection based on cannon charge
        if (charge > 0 && loaded)
        {
            currentFrame = (int)((float)finalChargeFrame * Mathf.Max(0.1f, charge));            
        }
        if(charge == 0)
        {
            if (!loaded)
            {
                currentFrame = 0;
            }
            else
            {
                currentFrame = 1;
            }
        }
        spriteRenderer.sprite = animationFrames[currentFrame];
    }
}
