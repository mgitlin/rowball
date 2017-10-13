////Ship.cs
////Created on 18.03.16 by Aaron C Gaudette
////Generates ship and manages input, logic, animations

//using UnityEngine;
//using UnityEngine.Events;
//using System.Collections;
//using GamepadInput;
//[ExecuteInEditMode]
//[RequireComponent(typeof(PolygonCollider2D))]
//[RequireComponent(typeof(PointEffector2D))]

//public class Ship : MonoBehaviour
//{
//    public DynamicModule module;
//    public ShipModule shipModule;

//    //Ship generation data point for editor
//    [System.Serializable]
//    public class Vertex
//    {
//        [HideInInspector]
//        public string name = "";
//        [Range(-1, 1)]
//        public float x = 0, y = 0;
//        public bool leftOar = false, rightOar = false, cannon = false;
//        [HideInInspector]
//        public int pointIndex; //Reference point

//        public Vector2 offset { get { return new Vector2(x, y); } }
//    }
//    public Vector2 vertexOffset(Vertex v) { return v.offset * size; }
//    public Point vertexPoint(Vertex v) { return points[v.pointIndex]; }

//    public Color color = Color.white, altColor = Color.gray;
//    public int size = 12;
//    public bool randomize = false;
//    public Vertex[] edge;

//    public bool centeredCannon = true; //Generate cannon at points[0]
//    public float centerCannonOffset = -1.5f;
//    public bool useDeck = false;

//    //Tells us if player is ready to start
//    public bool playerReady = false;
//    //Signals if player has tried to signal that they're ready
//    public bool startPressed = false;

//    //Distance from which ball was launched to goal
//    [HideInInspector]
//    public float shotDistance = 0;
//    //Time player started holding down the cannon button
//    [HideInInspector]
//    float cannonHeldStartStamp = -1;
//    //Has priority for drag when there is a terrainAffector
//    [HideInInspector]
//    float terrainAffectorDragFactor;
//    //Tells us if being affected by terrainAffector
//    [HideInInspector]
//    bool onTerrainAffector = false;
//    //Determines if rowball heading for player is going to be bounced
//    [HideInInspector]
//    bool volley = false;
//    //TimeStamp of when volley was pressed
//    [HideInInspector]
//    float volleyTimeStamp = -1;
//    [HideInInspector]
//    float freezeShipTimestamp = -1;
//    //Gives us a way to allow for more free movement in mud after a certain timespan
//    [HideInInspector]
//    public float stuckMudTimestamp = -1;
//    //Helps for gradually introducing the brake
//    [HideInInspector]
//    float brakeTimestamp = -1;
//    [HideInInspector]
//    public Vector2 max = Vector2.zero;
//    [HideInInspector]
//    public Vector2 min = Vector2.zero;
//    [HideInInspector]
//    public Vector2 boundsOffset;
//    public float furtherBoundsOffset = 10f;

//    //Input
//    [System.Serializable]
//    public class Control
//    {
//        public string leftOars = "q", cannon = "g", rightOars = "p";
//        public bool invertOars = true;

//        public enum InputType { KEYBOARD, JOYSTICK, PS3, PS4 };
//        public InputType inputType = InputType.KEYBOARD;
//        public int joystick = 1;

//        public string left { get { return invertOars ? rightOars : leftOars; } }
//        public string right { get { return invertOars ? leftOars : rightOars; } }
//        public bool keyboard { get { return inputType == InputType.KEYBOARD; } }

//        string[] keys = {
//            "`","1","2","3","4","5","6","7","8","9","0","-","=",
//            "q","w","e","r","t","y","u","i","o","p","[","]","\\",
//            "a","s","d","f","g","h","j","k","l",";","'",
//            "z","x","c","v","b","n","m",",",".","/"
//        };

//        //Randomizes input keys
//        public void Randomize()
//        {
//            leftOars = RandomKey();
//            cannon = RandomKey(new string[] { left });
//            rightOars = RandomKey(new string[] { left, cannon });
//        }
//        string RandomKey()
//        {
//            return keys[Random.Range(0, keys.Length)];
//        }
//        string RandomKey(string[] avoids)
//        {
//            string key; bool avoid;
//            do
//            {
//                key = keys[Random.Range(0, keys.Length)];
//                avoid = false;
//                foreach (string s in avoids) if (key == s) { avoid = true; break; }
//            }
//            while (avoid);
//            return key;
//        }
//    }
//    public Control control = new Control();

//    public bool useForceCollision = true;

//    //Regenerate ship on button press
//    [Header("Editor")]
//    public bool regenerate = false;

//    [Header("Read-only")]
//    public float mass = 0;
//    public bool loaded = false;
//    public bool totaled = false;
//    public bool scoredGoal = false;

//    [HideInInspector]
//    public Point[] points;
//    float lastFire = 0;

//    //Input
//    public float cannonInput = 0, charge = 0;
//    public int chargeLevel = 0;

//    //
//    [System.Serializable]
//    public class OarInput
//    {
//        public float input = 0, delay = 0, cooldown = 0;
//        float lastDelay = 0;
//        //
//        public void Compute(bool on, float delta, float damping, float cool)
//        {
//            input = on ? Mathf.Min(1, input + delta * Time.deltaTime) : 0;
//            delay = Mathf.Lerp(lastDelay, input, Time.deltaTime * damping);
//            lastDelay = input > lastDelay ? input : delay;

//            //
//            cooldown = Mathf.Max(0, cooldown - Time.deltaTime * cool);
//        }
//        //?
//        public void Cooldown()
//        {
//            cooldown = 1;
//        }
//    }
//    public OarInput leftIn, rightIn;

//    public bool hasRowball
//    {
//        get { return loaded; }
//        set
//        {
//            if (value)
//            {
//                loaded = true;
//                lerpingSprite = true;
//            }
//            else {
//                loaded = false;
//                lerpingSprite = false;
//            }
//        }
//    }

//    public Point center { get { return points[0]; } }
//    public Vector2 position { get { return center != null ? center.position : spawnPosition; } } //Average?
//    public Vector2 velocity { get { return center.velocity; } } //Average?
//    public float speed { get { return velocity.magnitude; } }

//    public Vector2 spawnPosition { get { return transform.position; } }
//    public int cannonDirection { get { return 1; } } //{get{return flipCannon?-1:1;}} //public bool flipCannon = true;

//    public Vector2 heading { get { return (vertexPoint(edge[0]).position - position).normalized; } }
//    public Vector2 starboard { get { return new Vector2(heading.y, -heading.x).normalized; } }
//    public Vector2 aiming { get { return starboard * cannonDirection; } }
//    public Vector2 CannonTip(Cannon cannon) { return cannon.position + aiming * shipModule.rowballOffset; }

//    public void InitSpritePosition(Vector2 position) { lastSpritePosition = position - center.cannon.position; }
//    Vector2 lastSpritePosition;
//    //Either collecting rowball (lerp) or have collected it (lock)
//    bool lerpingSprite = false;

//    //References
//    GameObject rowballSprite;
//    PolygonCollider2D bounds, collisionBounds;
//    PointEffector2D effector;
//    void Start()
//    {
//        //for(int i=0;i<Input.GetJoystickNames().Length;++i){
//        //	print(i+" "+Input.GetJoystickNames()[i]);
//        //}

//        rowballSprite = transform.GetChild(0).gameObject;
//        bounds = GetComponent<PolygonCollider2D>();
//        effector = GetComponent<PointEffector2D>();
//        collisionBounds = transform.GetChild(1).GetComponent<PolygonCollider2D>(); //Somewhat hardcoded

//        //Ensure freeze does not occur at game start
//        freezeShipTimestamp = -shipModule.freezeShipDuration * 2;

//        Build();
//        Update();
//    }
//    void OnDisable() { Erase(); }

//    public void setTerrainAffectorOn(float dragFactor)
//    {
//        onTerrainAffector = true;
//        terrainAffectorDragFactor = dragFactor;
//        if (stuckMudTimestamp == -1)
//            stuckMudTimestamp = Time.time;
//    }
//    public void setTerrainAffectorOff()
//    {
//        onTerrainAffector = false;
//        terrainAffectorDragFactor = 1;
//        stuckMudTimestamp = -1;
//    }

//    //Build ship
//    void Build()
//    {
//        //TODO: help why please god
//        setTerrainAffectorOff();

//        //ObjectManager.Flush();
//        Erase();

//        //Randomize
//        if (randomize)
//        {
//            //Random size and vertex count
//            size = (int)Random.Range(shipModule.randomSize.x, shipModule.randomSize.y);
//            edge = new Vertex[Random.Range(4, shipModule.maxVertices + 1)];
//            centeredCannon = true; //Force centered cannon

//            //Check for at least one oar for each side
//            bool leftOar = false, rightOar = false;
//            //Generate vertex positions and spawn oars
//            for (int i = 0; i < edge.Length; ++i)
//            {
//                edge[i] = new Vertex();

//                //Algorithm is not optimal but works relatively quickly in most cases
//                if (shipModule.simplePolygon)
//                {
//                    bool disqualify; int drop = 0;
//                    do
//                    {
//                        disqualify = false;
//                        //If a new position cannot be found that does not intersect, retry
//                        drop++;
//                        if (drop > shipModule.dropIterations)
//                        {
//                            //Debug.Log("Procedural ship design dropped, retrying...");
//                            Build();
//                            return;
//                        }
//                        //Get new vertex position
//                        edge[i].x = Random.Range(-1f, 1f);
//                        edge[i].y = Random.Range(-1f, 1f);

//                        //Simplified distance to center
//                        if (Mathf.Sqrt(edge[i].x * edge[i].x + edge[i].y * edge[i].y) < shipModule.vertexCenterOffset)
//                        {
//                            disqualify = true;
//                            continue;
//                        }

//                        if (!disqualify && i > 1)
//                        {
//                            //Check against previous segments
//                            for (int x = 1; x < i; ++x)
//                            {
//                                Vector2 a = new Vector2(edge[x - 1].x, edge[x - 1].y);
//                                Vector2 b = new Vector2(edge[x].x, edge[x].y);

//                                Vector2 c = new Vector2(edge[i - 1].x, edge[i - 1].y);
//                                Vector2 d = new Vector2(edge[i].x, edge[i].y);

//                                //Check distance
//                                if (Vector2.Distance(d, a) < shipModule.minVertexDistance || Vector2.Distance(d, b) < shipModule.minVertexDistance)
//                                {
//                                    disqualify = true;
//                                    break;
//                                }
//                                //Check intersection (skip adjacent segment)
//                                disqualify = (x == i - 1) ? false : Intersect(a, b, c, d);
//                                //Intersect(a,b,c,d,true);
//                                if (disqualify) break;

//                                //Check intersection with last segment (to first vertex)
//                                if (i == edge.Length - 1)
//                                {
//                                    Vector2 e = new Vector2(edge[0].x, edge[0].y);
//                                    disqualify = (x == 1) ? false : Intersect(a, b, d, e);
//                                }
//                                if (disqualify) break;
//                            }
//                        }
//                    }
//                    while (disqualify);
//                }
//                //Purely random
//                else {
//                    edge[i].x = Random.Range(-1f, 1f);
//                    edge[i].y = Random.Range(-1f, 1f);
//                }
//                if (Random.Range(0f, 1f) > 0.5f)
//                {
//                    if (Random.Range(0f, 1f) > 0.5f) edge[i].leftOar = leftOar = true;
//                    else edge[i].rightOar = rightOar = true;
//                }
//            }
//            //Add at least one of each oar
//            if (!leftOar || !rightOar)
//            {
//                int randomIndex = Random.Range(1, edge.Length);
//                edge[randomIndex].leftOar = true;
//                edge[randomIndex].rightOar = false;
//                edge[0].rightOar = true;
//                edge[0].leftOar = false;
//            }
//        }
//        if (randomize && control.keyboard) control.Randomize();

//        //Points
//        points = new Point[edge.Length + 1];
//        //Center
//        points[0] = ObjectManager.AddPoint(spawnPosition, module.pointMass, altColor, gameObject);

//        for (int i = 1; i < edge.Length + 2; ++i)
//        {
//            //Edge
//            if (i < edge.Length + 1)
//            {
//                points[i] = ObjectManager.AddPoint(spawnPosition + vertexOffset(edge[i - 1]), module.pointMass, color, gameObject);
//                edge[i - 1].pointIndex = i;
//                //Oars
//                if (edge[i - 1].leftOar || edge[i - 1].rightOar) points[i].AddOar(edge[i - 1].leftOar);
//                //Cannons
//                if (edge[i - 1].cannon) points[i].AddCannon(color);
//            }
//            //
//            if (useDeck) center.invisible = true;
//            //Links
//            if (i > 1)
//            {
//                Point start = points[i - 1], end = points[((i - 1) % edge.Length) + 1];
//                start.AddLink(end, Vector2.Distance(start.position, end.position), module.stiffness, module.damping, color);
//                //Framework
//                end = center;
//                end.AddLink(start, Vector2.Distance(start.position, end.position), module.stiffness, module.damping, altColor, !useDeck); //
//            }
//        }
//        //Cannon
//        if (centeredCannon)
//        {
//            center.AddCannon(color);
//            Vector2 cannonOffset = position + heading * centerCannonOffset;
//            center.cannon.position3 = new Vector3(cannonOffset.x, cannonOffset.y, center.cannon.position3.z);
//        }
//        //Deck
//        if (useDeck) center.AddDeck();
//    }
//    void Erase()
//    {
//        foreach (Point p in points) if (p != null)
//            {
//                if (Application.isEditor) DestroyImmediate(p.gameObject);
//                else Destroy(p.gameObject);
//            }
//    }
//    //Segment intersection code, used for procedural generation
//    bool Intersect(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
//    {
//        Orientation abc = GetOrientation(a, b, c);
//        Orientation abd = GetOrientation(a, b, d);

//        Orientation cda = GetOrientation(c, d, a);
//        Orientation cdb = GetOrientation(c, d, b);

//        //Intersect if the orientations between the two segments are all different
//        if (abc != abd && cda != cdb) return true;

//        //Otherwise, check if collinear
//        if (abc == Orientation.Collinear && abd == Orientation.Collinear)
//        {
//            //If so, check if any of the four points are on the two segments
//            if (OnSegment(a, b, c)) return true;
//            if (OnSegment(a, b, d)) return true;
//            if (OnSegment(c, d, a)) return true;
//            if (OnSegment(c, d, b)) return true;
//        }

//        //No intersection
//        return false;
//    }
//    enum Orientation { Clockwise, CounterClockwise, Collinear };
//    Orientation GetOrientation(Vector2 a, Vector2 b, Vector2 c)
//    {
//        //Get difference in slopes between two segments with shared point b
//        float difference = (b.y - a.y) * (c.x - b.x) - (c.y - b.y) * (b.x - a.x);
//        //Fudge provides some leeway in checking for collinearity
//        if (Mathf.Abs(difference) <= shipModule.collinearFudge) return Orientation.Collinear;
//        else return difference > 0 ? Orientation.Clockwise : Orientation.CounterClockwise;
//    }
//    bool OnSegment(Vector2 a, Vector2 b, Vector2 c)
//    {
//        //Fudge provides leeway for segments in cardinal directions
//        return c.x <= Mathf.Max(a.x, b.x) + shipModule.segmentFudge && c.x >= Mathf.Min(a.x, b.x) - shipModule.segmentFudge &&
//            c.y <= Mathf.Max(a.y, b.y) + shipModule.segmentFudge && c.y >= Mathf.Min(a.y, b.y) - shipModule.segmentFudge;
//    }

//    delegate bool GetKeyboardInput(string input);
//    //delegate bool GetControllerInput(GamePad.Button button, GamePad.Index controlIndex);
//    void Update()
//    {
//        //Editor code
//        if (regenerate)
//        {
//            Build();
//            regenerate = false;
//        }

//        //Update components in real time
//        for (int i = 0; i < points.Length; ++i) if (points[i] != null)
//            {
//                //this is the ideal place to update drag, need to refactor
//                points[i].UpdateData(module.pointMass, module.drag, i > 0 ? color : altColor, freezeShipTimestamp);
//                points[i].UpdateLinks(module.stiffness, module.damping, i > 0 ? color : altColor);

//                Vector3 heading3 = new Vector3(heading.x, heading.y, 0);

//                if (points[i].hasOar) points[i].UpdateOar(edge[i - 1].leftOar, heading3);
//                if (points[i].hasCannon) points[i].UpdateCannon(color, heading3);
//                if (points[i].hasDeck) points[i].UpdateDeck(heading3);
//            }
//        if (useForceCollision)
//        {
//            effector.useColliderMask = false;
//            effector.forceMagnitude = module.boundsForce;
//            effector.distanceScale = 1;
//            effector.forceVariation = effector.drag = module.boundsDrag;
//            effector.angularDrag = 0;
//            effector.forceSource = EffectorSelection2D.Collider;
//            effector.forceTarget = EffectorSelection2D.Rigidbody;
//            effector.forceMode = EffectorForceMode2D.Constant;
//        }
//        else effector.enabled = false;

//        //Centered cannon and sprite
//        rowballSprite.SetActive(hasRowball);
//        if (centeredCannon && center != null && center.cannon != null)
//        {
//            if (lerpingSprite)
//            {
//                //Perform in "local" space
//                Vector2 spritePosition = Vector2.Lerp(lastSpritePosition, Vector2.zero, Time.deltaTime * shipModule.rowballSpriteDamping);
//                rowballSprite.transform.position = spritePosition + center.cannon.position;
//                InitSpritePosition(rowballSprite.transform.position);
//            }
//            //Cannon
//            Vector2 cannonOffset = position + heading * centerCannonOffset;
//            center.cannon.position3 = new Vector3(cannonOffset.x, cannonOffset.y, center.cannon.position3.z);
//        }

//        //Update vertex objects in editor
//        for (int i = 0; i < edge.Length; ++i)
//        {
//            Vertex v = edge[i];
//            v.name = "Vertex " + i + " [" + v.x + "," + v.y + "]";
//            v.name += v.leftOar ? " (Left)" : v.rightOar ? " (Right)" : v.cannon ? " (Cannon)" : "";
//        }
//        edge[0].name += " (Front)";

//        Vector2[] polygon = new Vector2[edge.Length];
//        //Update effector polygon
//        if (useForceCollision)
//        {
//            for (int i = 0; i < polygon.Length; ++i)
//                if (vertexPoint(edge[i]) != null) polygon[i] = vertexPoint(edge[i]).position - spawnPosition;
//            bounds.SetPath(0, polygon);
//            //Ignore effectors on own ship
//            foreach (Point p in points) if (p != null) Physics2D.IgnoreCollision(p.GetComponent<Collider2D>(), bounds, true);
//        }
//        else bounds.enabled = false;

//        //Update collision bounds
//        for (int i = 0; i < polygon.Length; ++i) if (vertexPoint(edge[i]) != null)
//            {
//                Vector2 pointPosition = vertexPoint(edge[i]).position;
//                polygon[i] = pointPosition + (pointPosition - position).normalized * (1 + module.collisionBuffer) - spawnPosition;
//            }
//        collisionBounds.SetPath(0, polygon);

//        //Calculate mass
//        mass = module.pointMass * (edge.Length + 1); //this is going to need to change later

//        //Non-editor code
//        if (!Application.isEditor || Application.isPlaying)
//        {
//            if (!totaled)
//            {

//                //Reset variables
//                bool left = false, right = false, cannon = false;

//                //If ship should be frozen, return
//                if (Time.time - freezeShipTimestamp < shipModule.freezeShipDuration) return;
//                else freezeShipTimestamp = -shipModule.freezeShipDuration * 2;

//                //Input (trigger)
//                AssignInput(InputStage.DOWN, ref left, ref right, ref cannon);
//                if (cannon) Observer.OnFireBegin.Invoke(this);

//                //New control system
//                if (shipModule.experimental) PropelDirect(left, right);

//                //Input (release)
//                AssignInput(InputStage.UP, ref left, ref right, ref cannon);
//                if (shipModule.legacy || shipModule.redone) Propel(left, right);
//                if (cannon) Fire();

//                //Input (held)
//                AssignInput(InputStage.HELD, ref left, ref right, ref cannon);
//                startPressed = startPressed || (left && right && cannon);

//                //Curves not applied here in order to get linear input
//                cannonInput = cannon ? Mathf.Min(1, cannonInput + shipModule.cannonSpeed * Time.deltaTime) : 0;
//                leftIn.Compute(left, shipModule.oarSpeed, shipModule.inputDecay, shipModule.inputCooldown);
//                rightIn.Compute(right, shipModule.oarSpeed, shipModule.inputDecay, shipModule.inputCooldown);

//                //Logic for restricting cannon charge
//                if (cannon)
//                {
//                    if (cannonHeldStartStamp == -1)
//                    {
//                        cannonHeldStartStamp = Time.time;
//                    }
//                    if (Time.time - cannonHeldStartStamp > shipModule.cannonMaxChargeDuration)
//                    {
//                        cannonInput = charge = chargeLevel = 0;
//                        cannonHeldStartStamp = -1;
//                        Fire();
//                        freezeShipTimestamp = Time.time;
//                    }
//                }
//                else cannonHeldStartStamp = -1;

//                charge = shipModule.cannonFalloff.Evaluate(cannonInput);
//                chargeLevel =
//                    (charge > shipModule.chargeLevels.x && charge <= shipModule.chargeLevels.y) ?
//                    1 : charge > shipModule.chargeLevels.y ? 2 : 0;

//                //Old braking system
//                if (shipModule.legacy)
//                {
//                    if (left && right)
//                    {
//                        float drag = 0;

//                        if (brakeTimestamp == -1) brakeTimestamp = Time.time;
//                        drag = shipModule.brakeFactor * module.drag;

//                        //Code duplication, not good
//                        for (int i = 0; i < points.Length; ++i) if (points[i] != null)
//                            {
//                                points[i].UpdateData(module.pointMass, drag, i > 0 ? color : altColor, freezeShipTimestamp);
//                                points[i].UpdateLinks(module.stiffness, module.damping, i > 0 ? color : altColor);

//                                Vector3 heading3 = new Vector3(heading.x, heading.y, 0);

//                                if (points[i].hasOar) points[i].UpdateOar(edge[i - 1].leftOar, heading3);
//                                if (points[i].hasCannon) points[i].UpdateCannon(color, heading3);
//                                if (points[i].hasDeck) points[i].UpdateDeck(heading3);
//                            }
//                    }
//                    else brakeTimestamp = -1;
//                }

//                //Input (Pressed)

//                //Volley check
//                if (volley)
//                {
//                    if (Time.time - volleyTimeStamp > shipModule.volleyLength)
//                    {
//                        volley = false;
//                        volleyTimeStamp = -1;
//                    }
//                }
//                else {
//                    AssignInput(0, ref left, ref right, ref cannon);
//                    volley = cannon;
//                    volleyTimeStamp = Time.time;
//                }
//            }
//            //Flush input if totaled
//            else leftIn.input = rightIn.input = cannonInput = charge = 0;

//            //Braking
//            if (shipModule.redone || shipModule.experimental) ApplyBrake();
//            //Angular drag
//            ApplyAngularDrag();
//            //Environmental drag
//            ApplyMud();

//            //Animate oars and cannons (repeats vertex loop above)
//            foreach (Vertex v in edge) if (vertexPoint(v) != null)
//                {
//                    if (v.leftOar) vertexPoint(v).oar.load = shipModule.oarVisualFalloff.Evaluate(leftIn.input);
//                    else if (v.rightOar) vertexPoint(v).oar.load = shipModule.oarVisualFalloff.Evaluate(rightIn.input);
//                    if (v.cannon) vertexPoint(v).cannon.load = charge;
//                }
//            if (centeredCannon && center != null) center.cannon.load = charge;
//        }
//        Debug.DrawRay(CannonTip(center.cannon), aiming * 512, Color.red);
//    }
//    //Should probably move this to control class (?)
//    enum InputStage { DOWN, HELD, UP };
//    void AssignInput(InputStage stage, ref bool left, ref bool right, ref bool cannon)
//    {
//        //Reset input if within freeze time
//        if (MenuManager.game != null &&
//            (GameManager.instance.freezeTimer > 0) || (GameManager.instance.roundState == GameManager.RoundState.PAUSED)
//        )
//        {
//            left = right = cannon = false;
//            return;
//        }

//        if (control.inputType == Control.InputType.JOYSTICK)
//        {
//            GamePad.Index idx = 0;
//            if (control.joystick == 1) { idx = GamePad.Index.One; }
//            else if (control.joystick == 2) { idx = GamePad.Index.Two; }
//            else if (control.joystick == 3) { idx = GamePad.Index.Three; }
//            else if (control.joystick == 4) { idx = GamePad.Index.Four; }
//            switch (stage)
//            {
//                case InputStage.DOWN:
//                    left = GamePad.GetButtonDown(GamePad.Button.LeftShoulder, idx);
//                    right = GamePad.GetButtonDown(GamePad.Button.RightShoulder, idx);
//                    cannon = GamePad.GetButtonDown(GamePad.Button.A, idx);
//                    break;
//                case InputStage.HELD:
//                    left = GamePad.GetButton(GamePad.Button.LeftShoulder, idx);
//                    right = GamePad.GetButton(GamePad.Button.RightShoulder, idx);
//                    cannon = GamePad.GetButton(GamePad.Button.A, idx);
//                    break;
//                case InputStage.UP:
//                    left = GamePad.GetButtonUp(GamePad.Button.LeftShoulder, idx);
//                    right = GamePad.GetButtonUp(GamePad.Button.RightShoulder, idx);
//                    cannon = GamePad.GetButtonUp(GamePad.Button.A, idx);
//                    break;
//                default: return;
//            }


//        }
//        else {
//            GetKeyboardInput getInput;
//            switch (stage)
//            {
//                case InputStage.DOWN:
//                    getInput = Input.GetKeyDown;
//                    break;
//                case InputStage.HELD:
//                    getInput = Input.GetKey;
//                    break;
//                case InputStage.UP:
//                    getInput = Input.GetKeyUp;
//                    break;
//                default: return;
//            }

//            string leftButton = control.left, rightButton = control.right, cannonButton = control.cannon;
//            // not using keyboard input anymore
//            left = getInput(leftButton);
//            right = getInput(rightButton);
//            cannon = getInput(cannonButton);
//        }
//    }
//        //
//        int TranslateJoystick(string b){
//            if (control.inputType == Control.InputType.PS3)
//            {
//                if (b == "x") return 14;
//                else if (b == "l1") return 10;
//                else if (b == "r1") return 11;
//            }
//            else if (control.inputType == Control.InputType.PS4)
//            {
//                if (b == "x") return 1;
//                else if (b == "l1") return 4;
//                else if (b == "r1") return 5;
//            }
//            return -1;
//        }
//        //Movement
//        void Propel(bool left, bool right){
//            Vector2 f = heading * shipModule.oarStrength;
//            //Speed factor
//            float s = (speed - shipModule.speedFactorRange.x) / (shipModule.speedFactorRange.y - shipModule.speedFactorRange.x);
//            float speedFactor = shipModule.speedFactor.Evaluate(s);
//            f *= speedFactor;

//            float leftMult = shipModule.oarFalloff.Evaluate(leftIn.input);
//            float rightMult = shipModule.oarFalloff.Evaluate(rightIn.input);

//            foreach (Vertex v in edge)
//            {
//                if (v.leftOar)
//                {
//                    if (left) vertexPoint(v).AddImpulse(f * leftMult);
//                    //Turn boost
//                    if (right && leftIn.delay < shipModule.turnBoostLimit)
//                        vertexPoint(v).AddImpulse(-f * rightMult * (1 - leftIn.delay) * shipModule.turnBoost);
//                }
//                else if (v.rightOar)
//                {
//                    if (right) vertexPoint(v).AddImpulse(f * rightMult);
//                    //Turn boost
//                    if (left && rightIn.delay < shipModule.turnBoostLimit)
//                        vertexPoint(v).AddImpulse(-f * leftMult * (1 - rightIn.delay) * shipModule.turnBoost);
//                }
//            }
//            //Event (double)
//            if (left) Observer.OnPaddle.Invoke(this, true);
//            if (right) Observer.OnPaddle.Invoke(this, false);
//        }
//    //Refactor upwards
//        void PropelDirect(bool left, bool right){
//        Vector2 f = heading * shipModule.oarStrength;
//        //Speed factor
//        float s = (speed - shipModule.speedFactorRange.x) / (shipModule.speedFactorRange.y - shipModule.speedFactorRange.x);
//        float speedFactor = shipModule.speedFactor.Evaluate(s);
//        f *= speedFactor;

//        foreach (Vertex v in edge)
//        {
//            if (v.leftOar)
//            {
//                if (left)
//                {
//                    float b0 = 0, b1 = 0;
//                    //Pivot boost
//                    b0 = rightIn.input * shipModule.pivotBoost;
//                    //Dash boost
//                    b1 = rightIn.cooldown * shipModule.dashBoost;
//                    vertexPoint(v).AddImpulse(f + f * b0 + f * b1);
//                }
//                //Turn boost
//                if (right && leftIn.delay < shipModule.turnBoostLimit)
//                    vertexPoint(v).AddImpulse(-f * (1 - leftIn.delay) * shipModule.turnBoost);
//            }
//            else if (v.rightOar)
//            {
//                if (right)
//                {
//                    float b0 = 0, b1 = 0;
//                    //Pivot boost
//                    b0 = leftIn.input * shipModule.pivotBoost;
//                    //Dash boost
//                    b1 = leftIn.cooldown * shipModule.dashBoost;
//                    vertexPoint(v).AddImpulse(f + f * b0 + f * b1);
//                }
//                //Turn boost
//                if (left && rightIn.delay < shipModule.turnBoostLimit)
//                    vertexPoint(v).AddImpulse(-f * (1 - rightIn.delay) * shipModule.turnBoost);
//            }
//        }
//        //Event (double)
//        if (left) Observer.OnPaddle.Invoke(this, true);
//        if (right) Observer.OnPaddle.Invoke(this, false);
//    }
//    //Control system really needs a refactor
//    //
//        void ApplyBrake(){
//            float leftMult = shipModule.oarFalloff.Evaluate(leftIn.input);
//            float rightMult = shipModule.oarFalloff.Evaluate(rightIn.input);

//            ApplyDrag(leftMult * module.drag * shipModule.brakeFactor, true, false);
//            ApplyDrag(rightMult * module.drag * shipModule.brakeFactor, false, true);
//        }
//        void ApplyAngularDrag(){
//            //Calculate based on angle between forward vector
//            float difference = Vector2.Dot(center.velocity.normalized, aiming);
//            difference = shipModule.dragFalloff.Evaluate(Mathf.Abs(difference));

//            float d = Mathf.Lerp(shipModule.angularDrag.x, shipModule.angularDrag.y, difference);
//            //Ridiculous legacy bug that somehow made it in
//            if (shipModule.legacy)
//                ApplyDrag(d, true, false);
//            //Intended behavior
//            else ApplyDrag(d);
//        }
//        void ApplyMud(){
//            if (onTerrainAffector)
//                ApplyDrag(terrainAffectorDragFactor * module.drag);
//            else ResetDrag();
//        }
//        //need to refactor
//        void ResetDrag(){ ApplyDrag(module.drag); }
//        void ApplyDrag(float d, bool left = true, bool right = true){
//            foreach (Vertex v in edge)
//                if ((left && v.leftOar) || (right && v.rightOar) || (!v.leftOar && !v.rightOar))
//                    //Take max, so you don't overwrite previous drag levels
//                    vertexPoint(v).drag = Mathf.Max(d, vertexPoint(v).drag);
//        }
//        //Cannon shot
//        void Fire(){
//            if (MenuManager.game != null && GameManager.instance.roundState != GameManager.RoundState.PAUSED)
//            {
//                //Recoil
//                Vector2 recoil = charge * -aiming * shipModule.recoil;
//                if (!hasRowball) recoil *= shipModule.dryFire; //Lower recoil if no rowball is in the chamber
//                AddImpulse(recoil);

//                //Event
//                Observer.OnFire.Invoke(this, recoil);

//                if (hasRowball)
//                {
//                    //Pass ball to appropriate ship that has signalled for passing
//                    //TODO: Move this logic to appropriate place
//                    /*
//                    foreach (PlayerManager.PlayerInfo p in PlayerManager.ActivePlayers()) {
//                        Ship s = p.player.ship;
//                        if (!s.Equals (this) && s.color == this.color && s.readyForPass) {
//                            Pass(s);
//                            return;
//                        }
//                    }
//                    */

//                    //Instantiate rowball
//                    Rowball r = SpawnRowball();
//                    r.AddImpulse((charge == 0 ? shipModule.minCharge : charge) * aiming * shipModule.firepower);
//                    r.shooter = this;

//                    //Bounds checking (tmp)
//                    float currX = r.transform.position.x;
//                    float currY = r.transform.position.y;

//                    max = boundsOffset;
//                    min = new Vector2(-boundsOffset.x, -boundsOffset.y);

//                    //Clamps x values
//                    if (currX > max.x - furtherBoundsOffset)
//                        currX = max.x;//- furtherBoundsOffset;
//                    if (currX < min.x + furtherBoundsOffset)
//                        currX = min.x;//+ furtherBoundsOffset;
//                                      //Clamps y values
//                    if (currY > max.y - furtherBoundsOffset)
//                        currY = max.y;//- furtherBoundsOffset;
//                    if (currY < min.y + furtherBoundsOffset)
//                        currY = min.y;//+ furtherBoundsOffset;
//                    r.transform.position = new Vector2(currX, currY);

//                    r.shotPosition = r.transform.position;
//                    r.level = chargeLevel;
//                    //Spin
//                    Vector2 spinVector = vertexPoint(edge[0]).velocity - center.velocity;
//                    //Direction from dot product
//                    float spin = (spinVector.magnitude / shipModule.spinBound) * Mathf.Round(Vector2.Dot(spinVector.normalized, aiming));
//                    r.spin = Mathf.Clamp(spin, -1, 1);

//                    hasRowball = false;
//                    lastFire = Time.time;
//                }
//                //Force push
//                //No multi-cannon support (yet)
//                else if (centeredCannon)
//                {
//                    //Get a list of colliders in the force push circle
//                    Vector2 tip = CannonTip(center.cannon);
//                    //Try Physics2D.OverlapCircleNonAlloc ?
//                    Collider2D[] colliders = Physics2D.OverlapCircleAll(tip, shipModule.forcePushRadius);

//                    //Make sure a collider is not part of the ship
//                    foreach (Collider2D c in colliders) if (c != bounds)
//                        {
//                            bool colliderIsSelf = false;
//                            foreach (Point p in points) if (c == p.GetComponent<Collider2D>())
//                                {
//                                    colliderIsSelf = true;
//                                    break;
//                                }
//                            if (colliderIsSelf) continue;
//                            //Apply force
//                            if (c.GetComponent<Rigidbody2D>() != null)
//                            {
//                                Vector2 cPosition = new Vector2(c.transform.position.x, c.transform.position.y);
//                                float distanceMultiplier =
//                                    shipModule.forcePushCurve.Evaluate(1 - Vector2.Distance(cPosition, tip) / shipModule.forcePushRadius);

//                                c.GetComponent<Rigidbody2D>().AddForce(
//                                    (cPosition - tip).normalized * shipModule.pushForce * distanceMultiplier * charge,
//                                    ForceMode2D.Impulse
//                                );
//                                //Debug.DrawLine(tip,cPosition,Color.Lerp(Color.black,Color.white,distanceMultiplier),2);
//                            }
//                        }
//                }
//            }
//        }
//        void AddImpulse(Vector2 f){
//            foreach (Point p in points) if (p != null) p.AddImpulse(f);
//        }
//        void AddImpulseAtPoint(Vector2 f, Vector2 pos, float scale){
//            foreach (Point p in points) if (p != null)
//                {
//                    float factor = Mathf.Max(0, (scale - Vector2.Distance(pos, p.position)) / scale);
//                    p.AddImpulse(f * factor);
//                    //Debug.DrawLine(pos,p.position,Color.Lerp(Color.black,Color.green,factor),2);
//                }
//        }
//        //Spawn rowball at cannon
//        Rowball SpawnRowball(){ return ObjectManager.AddRowball(CannonTip(center.cannon), Color.white); }
//        //Teleport rowball to other ship
//        void Pass(Ship s){
//            if (hasRowball)
//            {
//                hasRowball = false;
//                lastFire = Time.time;
//                //Process pickup
//                s.InitSpritePosition(rowballSprite.transform.position);
//                s.hasRowball = true;
//            }
//        }

//    public void ProcessExit(Collider2D other)
//    {
//        //Clear rowball shot
//        if (other.gameObject.layer == module.rowballLayer)
//        {
//            Rowball rowball = other.GetComponent<Rowball>();
//            if (rowball.shooter != null && !rowball.clearedShot)
//                rowball.clearedShot = true;
//        }
//    }
//    public void ProcessCollision(Collider2D other)
//    {
//        if (!totaled)
//        {
//            //Process rowball
//            if (other.gameObject.layer == module.rowballLayer)
//            {
//                Rowball rowball = other.GetComponent<Rowball>();
//                RowballState state = GetRowballState(rowball);

//                //
//                float comparator = 2 * shipModule.volleyWidth - 1;

//                for (float x = -1; x <= 1; x += 0.3f) for (float y = -1; y <= 1; y += 0.3f)
//                    {
//                        Vector2 v = new Vector2(x, y).normalized;
//                        float d0 = Vector2.Dot(aiming.normalized, v);
//                        float d1 = Vector2.Dot(rowball.velocity.normalized, v);
//                        Debug.DrawRay(position, -v * 32, (d0 < comparator) ? Color.green : Color.red, 4);
//                        if (d1 < comparator) Debug.DrawRay(position, v * 16, Color.blue, 4);
//                    }

//                if (volley && rowball.shooter != null && rowball.level > 1)
//                {
//                    float dotProduct = Vector2.Dot(rowball.velocity.normalized, aiming.normalized);
//                    if (dotProduct < comparator)
//                    {
//                        rowball.velocity = (new Vector2(-1 * shipModule.volleySpeed * rowball.velocity.x, -1 * shipModule.volleySpeed * rowball.velocity.y));
//                        rowball.shooter = this; //Update shooter on volley

//                        InterfaceManager.CreateTextEffect(
//                            "VOLLEY",
//                            position + new Vector2(Random.Range(size, size * 2), Random.Range(size, size * 2)),
//                            color, 0.3f //Magic number
//                        );
//                        PlayerManager.GetPlayer(this).volleys++;

//                        //Hardcoded for now
//                        return;
//                    }
//                }

//                if (state == RowballState.Kill)
//                {
//                    //
//                    if (!rowball.clearedShot && rowball.shooter == this)
//                    {
//                        //print("Rowball hasn't cleared shot!");
//                        return;
//                    }

//                    if (GameManager.instance.roundState != GameManager.RoundState.PLAYING) return;
//                    //Process
//                    totaled = true;
//                    effector.enabled = false;
//                    bounds.enabled = false;
//                    collisionBounds.gameObject.SetActive(false);

//                    //Drop rowball if carrying one
//                    Vector2 relativeVelocity = rowball.RelativeVelocity(velocity);
//                    if (hasRowball)
//                    {
//                        Rowball r = SpawnRowball();
//                        r.AddImpulse(relativeVelocity * module.rowballDropFactor);
//                        hasRowball = false;
//                    }

//                    //Break closest point
//                    float minDistance = size * 2; //Arbitrary number, should always be larger
//                    Point targetPoint = null;
//                    foreach (Vertex v in edge)
//                    {
//                        Point p = vertexPoint(v); float distance;
//                        if (p != null)
//                        {
//                            distance = Vector2.Distance(p.position, rowball.position);
//                            if (distance < minDistance)
//                            {
//                                minDistance = distance;
//                                targetPoint = p;
//                            }
//                        }
//                    }
//                    if (targetPoint != null)
//                    {
//                        center.RemoveLink(targetPoint);
//                        targetPoint.RemoveLinks();
//                    }
//                    //Random destruction and force
//                    for (int i = 0; i < center.linkCount; ++i)
//                        if (Random.Range(0f, 1f) > (1 - module.frameworkDestroyChance)) center.RemoveLink(i); //Framework
//                    foreach (Point p in points) if (p != null && p != center)
//                        {
//                            if (Random.Range(0f, 1f) > (1 - module.edgeDestroyChance)) p.RemoveLinks(); //Edge
//                                                                                                        //Force (no else, in order to process dummy links)
//                            Vector2 explosionDirection = (relativeVelocity.normalized + (p.position - position).normalized) * 0.5f;
//                            p.AddImpulse(explosionDirection * Random.Range(module.minDestroyForce, module.maxDestroyForce));
//                        }

//                    //Event
//                    Observer.OnKill.Invoke(this, rowball);
//                }
//                else {//if(state==RowballState.Pickup){
//                    if (!hasRowball && Time.time > lastFire + shipModule.pickupReload)
//                    { //Only one, with rate of pickup
//                      //Process pickup
//                        InitSpritePosition(rowball.position);
//                        hasRowball = true;
//                        Destroy(rowball.gameObject);
//                    }
//                    else if (rowball.shooter == null || rowball.clearedShot)
//                    {
//                        rowball.velocity = new Vector2(this.velocity.x * 2, this.velocity.y * 2); //Magic numbers
//                    }
//                }
//            }

//            //Ramming (force-fire and stealing)
//            if (other.gameObject.layer == module.pointLayer)
//            {
//                Point p = other.GetComponent<Point>();
//                if (p.handler != null && p.handler.GetComponent<Ship>() != null)
//                {
//                    Ship s = p.handler.GetComponent<Ship>();
//                    Vector2 relativeVelocity = center.velocity - s.center.velocity;
//                    //Check if relative velocity is above threshold and stealer is moving faster
//                    if (hasRowball && relativeVelocity.magnitude > module.stealSpeed && s.center.velocity.magnitude > center.velocity.magnitude)
//                    {
//                        string stealString = "BUMP";
//                        if (!s.hasRowball)
//                        {
//                            Pass(s);
//                            stealString = "STEAL";
//                            PlayerManager.GetPlayer(this).steals++;
//                        }
//                        else Fire();
//                        InterfaceManager.CreateTextEffect(
//                            stealString,
//                            position + new Vector2(Random.Range(size, size * 2), Random.Range(size, size * 2)),
//                            s.color, 0.3f //Magic number
//                        );
//                    }
//                    //Ramming
//                    if (s.center.velocity.magnitude > center.velocity.magnitude &&
//                        Vector2.Dot(relativeVelocity.normalized, (s.center.position - center.position).normalized) > 0
//                    )
//                    {
//                        AddImpulseAtPoint(-relativeVelocity * module.ramMultiplier, p.position, module.ramScale);
//                    }
//                }
//            }
//        }
//    }
//    enum RowballState { Pickup, Bounce, Kill };
//    RowballState GetRowballState(Rowball rowball)
//    {
//        //float relativeSpeed = rowball.RelativeVelocity(velocity).magnitude;
//        //Pickup if rowball is slower than you or the relative speed is low enough
//        //if(relativeSpeed<module.damageSpeed || rowball.speed<=speed){
//        if (rowball.level < 2)
//        {
//            //Debug.Log("Rowball collision validated pass at "+relativeSpeed+" (relative speed)");
//            return RowballState.Pickup;
//        }
//        //Kill if the rowball is faster than you and the relative speed is high enough
//        else {//
//              //if(relativeSpeed>=module.damageSpeed && rowball.speed>speed){
//              //Debug.Log("Rowball collision killed player at "+relativeSpeed+" (relative speed)");
//            return RowballState.Kill;
//        }
//        //If the rowball is faster than you but the relative speed is too low to damage, bounce
//        //else return RowballState.Bounce;
//    }

//    public void Respawn(float s) { StartCoroutine(RespawnAfterSeconds(s)); }
//    IEnumerator RespawnAfterSeconds(float s)
//    {
//        yield return new WaitForSeconds(s);
//        ResetData();
//        regenerate = true;
//        if (s > GameManager.instance.respawnTime)
//            Observer.OnRespawnAfterSuicide.Invoke();
//    }
//    //
//    void ResetData()
//    {
//        totaled = false;
//        bounds.enabled = true;
//        effector.enabled = true;
//        collisionBounds.gameObject.SetActive(true);
//    }
//}