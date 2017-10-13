/*
 * ShipRevamped.cs 
 * Written by Matthew Gitlin
 * Based on Ship.cs by Aaron C Gaudette
 * Generates ship and manages input, logic, animations
 * 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GamepadInput;
[ExecuteInEditMode]
[RequireComponent(typeof(PolygonCollider2D))]
[RequireComponent(typeof(PointEffector2D))]

public class ShipRevamped : MonoBehaviour
{

    public DynamicModule module;
    public ShipModule shipModule;
    public CannonRevamped cannon;


    //Ship generation data point for editor
    [System.Serializable]
    public class Vertex
    {
        [HideInInspector]
        public string name = "";
        [Range(-1, 1)]
        public float x = 0, y = 0;
        public bool leftOar = false, rightOar = false, cannon = false;
        [HideInInspector]
        public int pointIndex; //Reference point

        public Vector2 offset { get { return new Vector2(x, y); } }
    }
    public Vector2 vertexOffset(Vertex v) { return v.offset * size; }
    public Point vertexPoint(Vertex v) { return points[v.pointIndex]; }

    public Color color = Color.white, altColor = Color.gray;
    public AudioSource destroyExplosion;
    public int size = 12;
    public Vertex[] edge;
    public float centerCannonOffset = -1.5f;
    //Tells us if player is ready to start
    public bool playerReady = false;

    //Has priority for drag when there is a terrainAffector
    [HideInInspector]
    float terrainAffectorDragFactor;
    //Tells us if being affected by terrainAffector
    [HideInInspector]
    bool onTerrainAffector = false;
    //Determines if rowball heading for player is going to be bounced
    [HideInInspector]
    //bool volley = false;
    //TimeStamp of when volley was pressed
    //[HideInInspector]
    //float volleyTimeStamp = -1;
    //[HideInInspector]
    float freezeShipTimestamp = -1;
    public float freezeShipDuration = 3; // How long to freeze ship when overcharged
    //Gives us a way to allow for more free movement in mud after a certain timespan
    [HideInInspector]
    public float stuckMudTimestamp = -1;
    //Helps for gradually introducing the brake
    //[HideInInspector]
    //float brakeTimestamp = -1;
    [HideInInspector]
    public Vector2 max = Vector2.zero;
    [HideInInspector]
    public Vector2 min = Vector2.zero;
    [HideInInspector]
    public Vector2 boundsOffset;
    public float furtherBoundsOffset = 10f;

    //Input
    [System.Serializable]
    public class Control
    {
        public string leftOarsKey = "q", cannonKey = "w", rightOarsKey = "e";
        public bool invertOars = true;

        public enum InputType { KEYBOARD, CONTROLLER };
        public InputType inputType = InputType.KEYBOARD;
        public int controller = 1;

        public string leftKey { get { return invertOars ? rightOarsKey : leftOarsKey; } }
        public string rightKey { get { return invertOars ? leftOarsKey : rightOarsKey; } }
        public bool keyboard { get { return inputType == InputType.KEYBOARD; } }

        public override string ToString()
        {
            return (leftOarsKey + " | " + cannonKey + " | " + rightOarsKey);
        }

        public enum InputStage { DOWN, HELD, UP };
        public void AssignInput(InputStage stage, ref bool left, ref bool right, ref bool cannon)
        {
            //Reset input if within freeze time
            if (GameController.game != null &&
                (GameManager.instance.freezeTimer > 0) || (GameManager.instance.roundState == GameManager.RoundState.PAUSED)
            )
            {
                left = right = cannon = false;
                return;
            }

            if (inputType == Control.InputType.CONTROLLER)
            {
                GamePad.Index idx = 0;
                if (controller == 1) { idx = GamePad.Index.One; }
                else if (controller == 2) { idx = GamePad.Index.Two; }
                else if (controller == 3) { idx = GamePad.Index.Three; }
                else if (controller == 4) { idx = GamePad.Index.Four; }
                switch (stage)
                {
                    case InputStage.DOWN:
                        left = GamePad.GetButtonDown(GamePad.Button.LeftShoulder, idx);
                        right = GamePad.GetButtonDown(GamePad.Button.RightShoulder, idx);
                        cannon = GamePad.GetButtonDown(GamePad.Button.X, idx);
                        break;
                    case InputStage.HELD:
                        left = GamePad.GetButton(GamePad.Button.LeftShoulder, idx);
                        right = GamePad.GetButton(GamePad.Button.RightShoulder, idx);
                        cannon = GamePad.GetButton(GamePad.Button.X, idx);
                        break;
                    case InputStage.UP:
                        left = GamePad.GetButtonUp(GamePad.Button.LeftShoulder, idx);
                        right = GamePad.GetButtonUp(GamePad.Button.RightShoulder, idx);
                        cannon = GamePad.GetButtonUp(GamePad.Button.X, idx);
                        break;
                    default: return;
                }


            }
            else
            {
                GetKeyboardInput getInput;
                switch (stage)
                {
                    case InputStage.DOWN:
                        getInput = Input.GetKeyDown;
                        break;
                    case InputStage.HELD:
                        getInput = Input.GetKey;
                        break;
                    case InputStage.UP:
                        getInput = Input.GetKeyUp;
                        break;
                    default: return;
                }


                left = getInput(leftKey);
                right = getInput(rightKey);
                cannon = getInput(cannonKey);
            }

        }
    }
    public Control control = new Control();

    bool useForceCollision = true;

    //Regenerate ship on button press
    [Header("Editor")]
    public bool regenerate = false;

    [Header("Read-only")]
    public float mass = 0;
    public bool loaded = false;
    public bool totaled = false;

    [HideInInspector]
    public Point[] points;
    //float lastFire = 0;

    //Input
    //public int chargeLevel = 0;

    //
    [System.Serializable]
    public class OarInput
    {
        public float input = 0, delay = 0, cooldown = 0;
        float lastDelay = 0;
        //
        public void Compute(bool on, float delta, float damping, float cool)
        {
            input = on ? Mathf.Min(1, input + delta * Time.deltaTime) : 0;
            delay = Mathf.Lerp(lastDelay, input, Time.deltaTime * damping);
            lastDelay = input > lastDelay ? input : delay;

            //
            cooldown = Mathf.Max(0, cooldown - Time.deltaTime * cool);
        }
        //?
        public void Cooldown()
        {
            cooldown = 1;
        }
    }
    public OarInput leftIn, rightIn;

    public bool hasRowball
    {
        get { return loaded; }
        set
        {
            if (value)
            {
                loaded = true;
                this.cannon.loaded = true;
                lerpingSprite = true;
            }
            else
            {
                loaded = false;
                this.cannon.loaded = false;
                lerpingSprite = false;
            }
        }
    }

    public Point center { get { return points[0]; } }
    public Vector2 position { get { return center != null ? center.position : spawnPosition; } } //Average?
    public Vector2 velocity { get { return center.velocity; } } //Average?
    public float speed { get { return velocity.magnitude; } }

    public Vector2 spawnPosition { get { return transform.position; } }

    public Vector2 heading { get { return (vertexPoint(edge[0]).position - position).normalized; } }
    public Vector2 starboard { get { return new Vector2(heading.y, -heading.x).normalized; } }
    public Vector2 aiming { get { return starboard; } }
    //public Vector2 CannonTip(CannonRevamped cannon) { return cannon.position + aiming; }

    public void InitSpritePosition(Vector2 position) { lastSpritePosition = position/* - center.cannon.position*/; }
    Vector2 lastSpritePosition;
    //Either collecting rowball (lerp) or have collected it (lock)
    bool lerpingSprite = false;

    //References
    PolygonCollider2D bounds, collisionBounds;
    PointEffector2D effector;

    void Start()
    {
        //for(int i=0;i<Input.GetJoystickNames().Length;++i){
        //	print(i+" "+Input.GetJoystickNames()[i]);
        //}
        
        bounds = GetComponent<PolygonCollider2D>();
        effector = GetComponent<PointEffector2D>();
        collisionBounds = transform.GetChild(1).GetComponent<PolygonCollider2D>(); //Somewhat hardcoded

        //Ensure freeze does not occur at game start
        freezeShipTimestamp = -freezeShipDuration * 2;

        Build();
        Update();
    }
    void OnDisable() { Erase(); }

    public void SetTerrainAffectorOn(float dragFactor)
    {
        onTerrainAffector = true;
        terrainAffectorDragFactor = dragFactor;
        if (stuckMudTimestamp == -1)
            stuckMudTimestamp = Time.time;
    }
    public void SetTerrainAffectorOff()
    {
        onTerrainAffector = false;
        terrainAffectorDragFactor = 1;
        stuckMudTimestamp = -1;
    }

    //Build ship
    void Build()
    {
        regenerate = false;
        //TODO: help why please god
        SetTerrainAffectorOff();

        Erase();

        //Points
        points = new Point[edge.Length + 1];
        //Center
        points[0] = ObjectManager.AddPoint(spawnPosition, module.pointMass, altColor, gameObject);

        for (int i = 1; i < edge.Length + 2; ++i)
        {
            //Edge
            if (i < edge.Length + 1)
            {
                points[i] = ObjectManager.AddPoint(spawnPosition + vertexOffset(edge[i - 1]), module.pointMass, color, gameObject);
                edge[i - 1].pointIndex = i;
                //Oars
                if (edge[i - 1].leftOar || edge[i - 1].rightOar) points[i].AddOar(edge[i - 1].leftOar);
            }
            //
            center.invisible = true;
            
            //Links
            if (i > 1)
            {
                Point start = points[i - 1], end = points[((i - 1) % edge.Length) + 1];
                start.AddLink(end, Vector2.Distance(start.position, end.position), module.stiffness, module.damping, color);
                //Framework
                end = center;
                end.AddLink(start, Vector2.Distance(start.position, end.position), module.stiffness, module.damping, altColor, false); //
            }
        }
        //Cannon
        center.AddCannon(color);
        center.cannon.ship = this.GetComponent<ShipRevamped>();
        this.cannon = center.cannon;
        //Deck
        center.AddDeck();
    }

    void Erase()
    {
        foreach (Point p in points) if (p != null)
            {
                if (Application.isEditor) DestroyImmediate(p.gameObject);
                else Destroy(p.gameObject);
            }
    }

    public void Overload()
    {
        //Debug.Log("Cannon over charged.");
        cannon.Fire(0.1f);
    }

    //Segment intersection code, used for procedural generation
    bool Intersect(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        Orientation abc = GetOrientation(a, b, c);
        Orientation abd = GetOrientation(a, b, d);

        Orientation cda = GetOrientation(c, d, a);
        Orientation cdb = GetOrientation(c, d, b);

        //Intersect if the orientations between the two segments are all different
        if (abc != abd && cda != cdb) return true;

        //Otherwise, check if collinear
        if (abc == Orientation.Collinear && abd == Orientation.Collinear)
        {
            //If so, check if any of the four points are on the two segments
            if (OnSegment(a, b, c)) return true;
            if (OnSegment(a, b, d)) return true;
            if (OnSegment(c, d, a)) return true;
            if (OnSegment(c, d, b)) return true;
        }

        //No intersection
        return false;
    }
    enum Orientation { Clockwise, CounterClockwise, Collinear };
    Orientation GetOrientation(Vector2 a, Vector2 b, Vector2 c)
    {
        //Get difference in slopes between two segments with shared point b
        float difference = (b.y - a.y) * (c.x - b.x) - (c.y - b.y) * (b.x - a.x);
        //Fudge provides some leeway in checking for collinearity
        if (Mathf.Abs(difference) <= shipModule.collinearFudge) return Orientation.Collinear;
        else return difference > 0 ? Orientation.Clockwise : Orientation.CounterClockwise;
    }
    bool OnSegment(Vector2 a, Vector2 b, Vector2 c)
    {
        //Fudge provides leeway for segments in cardinal directions
        return c.x <= Mathf.Max(a.x, b.x) + shipModule.segmentFudge && c.x >= Mathf.Min(a.x, b.x) - shipModule.segmentFudge &&
            c.y <= Mathf.Max(a.y, b.y) + shipModule.segmentFudge && c.y >= Mathf.Min(a.y, b.y) - shipModule.segmentFudge;
    }

    delegate bool GetKeyboardInput(string input);
    //delegate bool GetControllerInput(GamePad.Button button, GamePad.Index controlIndex);

    void Update()
    {
        //Editor code
        if (regenerate)
        {
            Build();
        }

        //Update components in real time
        for (int i = 0; i < points.Length; ++i) if (points[i] != null)
            {
                //this is the ideal place to update drag, need to refactor
                points[i].UpdateData(module.pointMass, module.drag, i > 0 ? color : altColor, freezeShipTimestamp);
                points[i].UpdateLinks(module.stiffness, module.damping, i > 0 ? color : altColor);

                Vector3 heading3 = new Vector3(heading.x, heading.y, 0);

                if (points[i].hasOar) points[i].UpdateOar(edge[i - 1].leftOar, heading3);
                if (points[i].hasCannon) points[i].UpdateCannon(color, heading3);
                if (points[i].hasDeck) points[i].UpdateDeck(heading3);
            }
        if (useForceCollision)
        {
            effector.useColliderMask = false;
            effector.forceMagnitude = module.boundsForce;
            effector.distanceScale = 1;
            effector.forceVariation = effector.drag = module.boundsDrag;
            effector.angularDrag = 0;
            effector.forceSource = EffectorSelection2D.Collider;
            effector.forceTarget = EffectorSelection2D.Rigidbody;
            effector.forceMode = EffectorForceMode2D.Constant;
        }
        else effector.enabled = false;

        //Update vertex objects in editor
        for (int i = 0; i < edge.Length; ++i)
        {
            Vertex v = edge[i];
            v.name = "Vertex " + i + " [" + v.x + "," + v.y + "]";
            v.name += v.leftOar ? " (Left)" : v.rightOar ? " (Right)" : v.cannon ? " (Cannon)" : "";
        }
        edge[0].name += " (Front)";

        Vector2[] polygon = new Vector2[edge.Length];
        //Update effector polygon
        if (useForceCollision)
        {
            for (int i = 0; i < polygon.Length; ++i)
                if (vertexPoint(edge[i]) != null) polygon[i] = vertexPoint(edge[i]).position - spawnPosition;
            bounds.SetPath(0, polygon);
            //Ignore effectors on own ship
            foreach (Point p in points) if (p != null) Physics2D.IgnoreCollision(p.GetComponent<Collider2D>(), bounds, true);
        }
        else bounds.enabled = false;

        //Update collision bounds
        for (int i = 0; i < polygon.Length; ++i) if (vertexPoint(edge[i]) != null)
            {
                Vector2 pointPosition = vertexPoint(edge[i]).position;
                polygon[i] = pointPosition + (pointPosition - position).normalized * (1 + module.collisionBuffer) - spawnPosition;
            }
        collisionBounds.SetPath(0, polygon);

        //Calculate mass
        mass = module.pointMass * (edge.Length + 1); //this is going to need to change later

        //Non-editor code
        if (!Application.isEditor || Application.isPlaying)
        {
            if (!totaled)
            {

                //Reset variables
                bool left = false, right = false, cannon = false;

                //If ship should be frozen, return
                if (Time.time - freezeShipTimestamp < freezeShipDuration) return;
                else freezeShipTimestamp = -freezeShipDuration * 2;

                //Input (trigger)
                control.AssignInput(Control.InputStage.DOWN, ref left, ref right, ref cannon);
                //if (cannon) Observer.OnFireBegin.Invoke(this);

                //Input (release)
                control.AssignInput(Control.InputStage.UP, ref left, ref right, ref cannon);
                Propel(left, right);
                if (cannon) this.cannon.Fire(Mathf.Min(this.cannon.charge, 1.0f));

                //Input (held)
                control.AssignInput(Control.InputStage.HELD, ref left, ref right, ref cannon);
                if (left && right && cannon && !playerReady)
                {
                    Debug.Log("Ship ready");
                    playerReady = true;
                }
                if (cannon) this.cannon.Charge();

                //Curves not applied here in order to get linear input
                leftIn.Compute(left, shipModule.oarSpeed, shipModule.inputDecay, shipModule.inputCooldown);
                rightIn.Compute(right, shipModule.oarSpeed, shipModule.inputDecay, shipModule.inputCooldown);

                //brakeTimestamp = -1;
            }

            //Input (Pressed)

            //Volley check
            //if (volley)
            //{
            //    if (Time.time - volleyTimeStamp > shipModule.volleyLength)
            //    {
            //        volley = false;
            //        volleyTimeStamp = -1;
            //    }
            //}
            //else
            //{
            //    //this.control.AssignInput(0, ref this.control.leftKey, ref  this.control.rightKey, ref this.control.cannonKey);
            //    volley = cannon;
            //    volleyTimeStamp = Time.time;
            //}
        }
        //Flush input if totaled
        else leftIn.input = rightIn.input = 0;

        //Braking
        if (shipModule.redone || shipModule.experimental) ApplyBrake();
        //Angular drag
        ApplyAngularDrag();
        //Environmental drag
        ApplyMud();

        //Animate oars (repeats vertex loop above)
        foreach (Vertex v in edge) if (vertexPoint(v) != null)
            {
                if (v.leftOar) vertexPoint(v).oar.load = shipModule.oarVisualFalloff.Evaluate(leftIn.input);
                else if (v.rightOar) vertexPoint(v).oar.load = shipModule.oarVisualFalloff.Evaluate(rightIn.input);
            }

        Debug.DrawRay(center.position, aiming * 512, Color.red);
    }

    //Movement
    void Propel(bool left, bool right)
    {
        Vector2 f = heading * shipModule.oarStrength;
        //Speed factor
        float s = (speed - shipModule.speedFactorRange.x) / (shipModule.speedFactorRange.y - shipModule.speedFactorRange.x);
        float speedFactor = shipModule.speedFactor.Evaluate(s);
        f *= speedFactor;

        float leftMult = shipModule.oarFalloff.Evaluate(leftIn.input);
        float rightMult = shipModule.oarFalloff.Evaluate(rightIn.input);

        foreach (Vertex v in edge)
        {
            if (v.leftOar)
            {
                if (left) vertexPoint(v).AddImpulse(f * leftMult);
                //Turn boost
                if (right && leftIn.delay < shipModule.turnBoostLimit)
                    vertexPoint(v).AddImpulse(-f * rightMult * (1 - leftIn.delay) * shipModule.turnBoost);
            }
            else if (v.rightOar)
            {
                if (right) vertexPoint(v).AddImpulse(f * rightMult);
                //Turn boost
                if (left && rightIn.delay < shipModule.turnBoostLimit)
                    vertexPoint(v).AddImpulse(-f * leftMult * (1 - rightIn.delay) * shipModule.turnBoost);
            }
        }
        //Event (double)
        //if (left) Observer.OnPaddle.Invoke(this, true);
        //if (right) Observer.OnPaddle.Invoke(this, false);
    }
    //Refactor upwards
    void PropelDirect(bool left, bool right)
    {
        Vector2 f = heading * shipModule.oarStrength;
        //Speed factor
        float s = (speed - shipModule.speedFactorRange.x) / (shipModule.speedFactorRange.y - shipModule.speedFactorRange.x);
        float speedFactor = shipModule.speedFactor.Evaluate(s);
        f *= speedFactor;

        foreach (Vertex v in edge)
        {
            if (v.leftOar)
            {
                if (left)
                {
                    float b0 = 0, b1 = 0;
                    //Pivot boost
                    b0 = rightIn.input * shipModule.pivotBoost;
                    //Dash boost
                    b1 = rightIn.cooldown * shipModule.dashBoost;
                    vertexPoint(v).AddImpulse(f + f * b0 + f * b1);
                }
                //Turn boost
                if (right && leftIn.delay < shipModule.turnBoostLimit)
                    vertexPoint(v).AddImpulse(-f * (1 - leftIn.delay) * shipModule.turnBoost);
            }
            else if (v.rightOar)
            {
                if (right)
                {
                    float b0 = 0, b1 = 0;
                    //Pivot boost
                    b0 = leftIn.input * shipModule.pivotBoost;
                    //Dash boost
                    b1 = leftIn.cooldown * shipModule.dashBoost;
                    vertexPoint(v).AddImpulse(f + f * b0 + f * b1);
                }
                //Turn boost
                if (left && rightIn.delay < shipModule.turnBoostLimit)
                    vertexPoint(v).AddImpulse(-f * (1 - rightIn.delay) * shipModule.turnBoost);
            }
        }
        //Event (double)
        //if (left) Observer.OnPaddle.Invoke(this, true);
        //if (right) Observer.OnPaddle.Invoke(this, false);
    }
    //Control system really needs a refactor
    //
    void ApplyBrake()
    {
        float leftMult = shipModule.oarFalloff.Evaluate(leftIn.input);
        float rightMult = shipModule.oarFalloff.Evaluate(rightIn.input);

        ApplyDrag(leftMult * module.drag * shipModule.brakeFactor, true, false);
        ApplyDrag(rightMult * module.drag * shipModule.brakeFactor, false, true);
    }
    void ApplyAngularDrag()
    {
        //Calculate based on angle between forward vector
        float difference = Vector2.Dot(center.velocity.normalized, aiming);
        difference = shipModule.dragFalloff.Evaluate(Mathf.Abs(difference));

        float d = Mathf.Lerp(shipModule.angularDrag.x, shipModule.angularDrag.y, difference);
        //Ridiculous legacy bug that somehow made it in
        if (shipModule.legacy)
            ApplyDrag(d, true, false);
        //Intended behavior
        else ApplyDrag(d);
    }
    void ApplyMud()
    {
        if (onTerrainAffector)
            ApplyDrag(terrainAffectorDragFactor * module.drag);
        else ResetDrag();
    }
    //need to refactor
    void ResetDrag() { ApplyDrag(module.drag); }
    void ApplyDrag(float d, bool left = true, bool right = true)
    {
        foreach (Vertex v in edge)
            if ((left && v.leftOar) || (right && v.rightOar) || (!v.leftOar && !v.rightOar))
                //Take max, so you don't overwrite previous drag levels
                vertexPoint(v).drag = Mathf.Max(d, vertexPoint(v).drag);
    }

    public void AddImpulse(Vector2 f)
    {
        foreach (Point p in points) if (p != null) p.AddImpulse(f);
    }
    void AddImpulseAtPoint(Vector2 f, Vector2 pos, float scale)
    {
        foreach (Point p in points) if (p != null)
            {
                float factor = Mathf.Max(0, (scale - Vector2.Distance(pos, p.position)) / scale);
                p.AddImpulse(f * factor);
                //Debug.DrawLine(pos,p.position,Color.Lerp(Color.black,Color.green,factor),2);
            }
    }

    //Teleport rowball to other ship
    void Pass(ShipRevamped s)
    {
        if (hasRowball)
        {
            hasRowball = false;
            cannon.lastFireTime = Time.time;
            //Process pickup
            s.hasRowball = true;
        }
    }

    public void ProcessExit(Collider2D other)
    {
        //Clear rowball shot
        if (other.gameObject.layer == module.rowballLayer)
        {
            Rowball rowball = other.GetComponent<Rowball>();
            if (rowball.shooter != null && !rowball.clearedShot)
                rowball.clearedShot = true;
        }
    }

    public void ProcessCollision(Collider2D other)
    {
        if (!totaled)
        {
            //Process rowball
            if (other.gameObject.layer == module.rowballLayer)
            {
                Rowball rowball = other.GetComponent<Rowball>();
                RowballState state = GetRowballState(rowball);

                //
                float comparator = 2 * shipModule.volleyWidth - 1;

                for (float x = -1; x <= 1; x += 0.3f) for (float y = -1; y <= 1; y += 0.3f)
                    {
                        Vector2 v = new Vector2(x, y).normalized;
                        float d0 = Vector2.Dot(aiming.normalized, v);
                        float d1 = Vector2.Dot(rowball.velocity.normalized, v);
                        Debug.DrawRay(position, -v * 32, (d0 < comparator) ? Color.green : Color.red, 4);
                        if (d1 < comparator) Debug.DrawRay(position, v * 16, Color.blue, 4);
                    }

                //if (volley && rowball.shooter != null && rowball.level > 1)
                //{
                //    float dotProduct = Vector2.Dot(rowball.velocity.normalized, aiming.normalized);
                //    if (dotProduct < comparator)
                //    {
                //        rowball.velocity = (new Vector2(-1 * shipModule.volleySpeed * rowball.velocity.x, -1 * shipModule.volleySpeed * rowball.velocity.y));
                //        rowball.shooter = this.gameObject; //Update shooter on volley

                //        InterfaceManager.CreateTextEffect(
                //            "VOLLEY",
                //            position + new Vector2(Random.Range(size, size * 2), Random.Range(size, size * 2)),
                //            color, 0.3f //Magic number
                //        );
                //        PlayerManager.GetPlayer(this).volleys++;

                //        //Hardcoded for now
                //        return;
                //    }
                //}

                if (state == RowballState.Kill)
                {
                    if (!rowball.clearedShot && rowball.shooter == this)
                    {
                        //Debug.Log("Rowball hasn't cleared shot!");
                        return;
                    }
                    // Only allow kills when game is in active state (not in ready/end state)
                    if (GameManager.instance.roundState != GameManager.RoundState.PLAYING) return;
                    //Process
                    Debug.Log("Player totaled");
                    this.totaled = true;
                    effector.enabled = false;
                    bounds.enabled = false;
                    collisionBounds.gameObject.SetActive(false);

                    //Drop rowball if carrying one
                    Vector2 relativeVelocity = rowball.RelativeVelocity(velocity);
                    if (hasRowball)
                    {
                        Rowball r = ObjectManager.AddRowball(this.position, Color.white); ;
                        r.AddImpulse(relativeVelocity * module.rowballDropFactor);
                        hasRowball = false;
                    }

                    //Break closest point
                    float minDistance = size * 2; //Arbitrary number, should always be larger
                    Point targetPoint = null;
                    foreach (Vertex v in edge)
                    {
                        Point p = vertexPoint(v); float distance;
                        if (p != null)
                        {
                            distance = Vector2.Distance(p.position, rowball.position);
                            if (distance < minDistance)
                            {
                                minDistance = distance;
                                targetPoint = p;
                            }
                        }
                    }
                    if (targetPoint != null)
                    {
                        center.RemoveLink(targetPoint);
                        targetPoint.RemoveLinks();
                    }
                    //Random destruction and force
                    for (int i = 0; i < center.linkCount; ++i)
                        if (Random.Range(0f, 1f) > (1 - module.frameworkDestroyChance)) center.RemoveLink(i); //Framework
                    foreach (Point p in points) if (p != null && p != center)
                        {
                            if (Random.Range(0f, 1f) > (1 - module.edgeDestroyChance)) p.RemoveLinks(); //Edge
                                                                                                        //Force (no else, in order to process dummy links)
                            Vector2 explosionDirection = (relativeVelocity.normalized + (p.position - position).normalized) * 0.5f;
                            p.AddImpulse(explosionDirection * Random.Range(module.minDestroyForce, module.maxDestroyForce));
                        }

                    //Event
                    Debug.Log("Player killed");
                    Observer.OnKill.Invoke(this, rowball);
                }
                else
                {//if(state==RowballState.Pickup){
                    if (!hasRowball && Time.time - cannon.lastFireTime >  cannon.cooldownTime) { 
                      //Process pickup
                        //InitSpritePosition(rowball.position);
                        hasRowball = true;
                        Destroy(rowball.gameObject);
                    }
                    else if (rowball.shooter == null || rowball.clearedShot)
                    {
                        rowball.velocity = new Vector2(this.velocity.x * 2, this.velocity.y * 2); //Magic numbers
                    }
                }
            }

            //Ramming (force-fire and stealing)
            if (other.gameObject.layer == module.pointLayer)
            {
                Point p = other.GetComponent<Point>();
                if (p.handler != null && p.handler.GetComponent<ShipRevamped>() != null)
                {
                    ShipRevamped s = p.handler.GetComponent<ShipRevamped>();
                    Vector2 relativeVelocity = center.velocity - s.center.velocity;
                    //Check if relative velocity is above threshold and stealer is moving faster
                    if (hasRowball && relativeVelocity.magnitude > module.stealSpeed && s.center.velocity.magnitude > center.velocity.magnitude)
                    {
                        string stealString = "BUMP";
                        if (!s.hasRowball)
                        {
                            Pass(s);
                            stealString = "STEAL";
                            PlayerManager.GetPlayer(this).steals++;
                        }
                        //else this.cannon.Fire(0.1f);
                        InterfaceManager.CreateTextEffect(
                            stealString,
                            position + new Vector2(Random.Range(size, size * 2), Random.Range(size, size * 2)),
                            s.color, 0.3f //Magic number
                        );
                    }
                    //Ramming
                    if (s.center.velocity.magnitude > center.velocity.magnitude &&
                        Vector2.Dot(relativeVelocity.normalized, (s.center.position - center.position).normalized) > 0
                    )
                    {
                        AddImpulseAtPoint(-relativeVelocity * module.ramMultiplier, p.position, module.ramScale);
                    }
                }
            }
        }
    }
    enum RowballState { Pickup, Bounce, Kill };
    RowballState GetRowballState(Rowball rowball)
    {
        //float relativeSpeed = rowball.RelativeVelocity(velocity).magnitude;
        //Pickup if rowball is slower than you or the relative speed is low enough
        //if(relativeSpeed<module.damageSpeed || rowball.speed<=speed){
        if (rowball.level < 2)
        {
            //Debug.Log("Rowball collision validated pass at "+relativeSpeed+" (relative speed)");
            return RowballState.Pickup;
        }
        //Kill if the rowball is faster than you and the relative speed is high enough
        else
        {//
         //if(relativeSpeed>=module.damageSpeed && rowball.speed>speed){
         //Debug.Log("Rowball collision killed player at "+relativeSpeed+" (relative speed)");
            return RowballState.Kill;
        }
        //If the rowball is faster than you but the relative speed is too low to damage, bounce
        //else return RowballState.Bounce;
    }

    public void Respawn(float s) { StartCoroutine(RespawnAfterSeconds(s)); }
    IEnumerator RespawnAfterSeconds(float s)
    {
        yield return new WaitForSeconds(s);
        ResetData();
        regenerate = true;
        if (s > GameManager.instance.respawnTime)
            Observer.OnRespawnAfterSuicide.Invoke();
    }

    // Reset certain flags (most likely on death)
    void ResetData()
    {
        totaled = false;
        bounds.enabled = true;
        effector.enabled = true;
        collisionBounds.gameObject.SetActive(true);
    }
}
