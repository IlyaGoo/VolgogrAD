using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarMoving : MonoBehaviourExtension {

    public float speed;                //Floating point variable to store the player's movement speed.
    const float angleBetweenStates = 22.5f;
    const float halfAngleBetweenStates = angleBetweenStates / 2;

    private Rigidbody2D rb2d;        //Store a reference to the Rigidbody2D component required to use 2D Physics.
    [SerializeField] AudioSource motor;

    public string motionName = "BuhankaWheels";
    public Vector2 CurrentDirection = new Vector2(0, 1);
    public Vector3 lastpos;
    public bool canMove = false;
    public int currentState = 1;
    Animator _anim;
    public Animator _animWheels;
    [SerializeField] GameObject triggerObject;
    public static Vector3 passagersOffset = new Vector3(0, 0, 0.1f);

    public float moveHorizontal;
    public float moveVertical;

    public List<GameObject> passagers = new List<GameObject>();
    [SerializeField] GameObject[] Lights;
    [SerializeField] GameObject[] StopLights;
    [SerializeField] GameObject[] Boxes;
    [SerializeField] GameObject[] Perticles;
    GameObject currentLight;
    GameObject currentStopLight;
    GameObject currentBox;
    public GameObject currentParticles;
    [SerializeField] GameObject LightParent;
    public bool LightsState;
    public bool ParticlesState;
    public bool stopLightActive;
    bool inited = false;

    public void ClearMissingsPassagers()
    {
        List<int> needRemove = new List<int>();
        for (var i = 0; i < passagers.Count; i++)
        {
            try
            {
                var a = passagers[i].transform;
            }
            catch
            {
                needRemove.Add(i);
            }
        }
        foreach(var removeIndex in needRemove)
        {
            passagers.RemoveAt(removeIndex);
        }
    }

    public void SetLight(int num)
    {
        if (currentBox == Boxes[num]) return;
        currentLight.SetActive(false);
        currentLight = Lights[num];
        currentLight.SetActive(true);
        currentStopLight.SetActive(false);
        currentStopLight = StopLights[num];
        currentStopLight.SetActive(true);
        currentBox.SetActive(false);
        currentBox = Boxes[num];
        currentBox.SetActive(true);

        currentParticles.SetActive(false);
        currentParticles = Perticles[num];
        if (ParticlesState)
            currentParticles.SetActive(true);
    }

    public void SetLightsState(bool state)
    {
        LightParent.SetActive(state);
        LightsState = state;
    }

    private void Start()
    {
        Init();
    }

    public void Init()
    {
        if (inited) return;
        inited = true;
        currentStopLight = StopLights[currentState-1];
        currentStopLight.SetActive(true);
        currentBox = Boxes[currentState - 1];
        currentBox.SetActive(true);
        currentParticles = Perticles[currentState - 1];

        currentLight = Lights[currentState-1];
        currentLight.SetActive(true);
        _anim = GetComponent<Animator>();
        lastpos = transform.position;
        //Get and store a reference to the Rigidbody2D component so that we can access it.
        rb2d = GetComponent<Rigidbody2D>();
    }

    public void SetStopLights(bool state)
    {
        stopLightActive = state;
        foreach (var ch in currentStopLight.transform.GetAllAllChilds())
        {
            ch.GetComponent<LightScript>().needUpdate = state;
            if (state)
                ch.GetComponent<LightScript>().SetFullLight();
            else
                ch.GetComponent<LightScript>().SetZeroLight();
        }
    }

    //FixedUpdate is called at a fixed interval and is independent of frame rate. Put physics code here.
    void FixedUpdate()
    {
        if (!canMove) return;
        var dif = (transform.position - lastpos).magnitude;
        lastpos = transform.position;

        transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.y / 100);

        //Store the current horizontal input in the float moveHorizontal.
        float moveHorizontal = Input.GetAxis("Horizontal");
        //Store the current vertical input in the float moveVertical.
        float moveVertical = Input.GetAxis("Vertical");

        if (moveVertical < 0) moveVertical /= 2;
        var magn = rb2d.velocity.magnitude;

        //SetStopLights(moveVertical == 0 && magn > 0.1f);
        bool newStopState = moveVertical == 0 && magn > 0.1f;
        if (newStopState != stopLightActive)
            localCommands.CmdSetStopLights(gameObject, newStopState);

        CurrentDirection = Rotate(CurrentDirection, dif * moveHorizontal * 20 * -moveVertical * Mathf.Deg2Rad);

        foreach (var pas in passagers)
        {
            pas.transform.position = transform.position + passagersOffset;
        }

        var angle = Vector3.Angle(Vector2.up, CurrentDirection);

        var previousCarState = currentState;

        if (CurrentDirection.x <= 0)
        {
            currentState = 1 + (int)((angle + halfAngleBetweenStates) / angleBetweenStates);
        }
        else
        {
            currentState = angle < halfAngleBetweenStates ? 1 : 16 - (int)((angle - halfAngleBetweenStates) / angleBetweenStates);
        }

        rb2d.AddForce(CurrentDirection * speed * moveVertical);
        var spSpeed = magn / 4.25f;
        _animWheels.speed = spSpeed;
        motor.pitch = 1 + spSpeed;
        if (previousCarState != currentState)
        {
            SetLight(currentState - 1);
            _anim.SetInteger("State", currentState);
            _animWheels.SetInteger("State", currentState);
            SetTriggerAngle();
            localCommands.CmdSetCarState(gameObject, currentState, _animWheels.speed);
            localCommands.CmdSetCarCurrentDirection(gameObject, CurrentDirection.x, CurrentDirection.y);
        }
    }

    public void SetState(int newState, float wheelSpeed)
    {
        if (canMove) return;
        currentState = newState;
        SetTriggerAngle();
        SetLight(currentState - 1);
        _anim.SetInteger("State", currentState);
        _animWheels.SetInteger("State", currentState);
        _animWheels.speed = wheelSpeed;
        motor.pitch = 1 + wheelSpeed;
    }

    void SetTriggerAngle()
    {
        triggerObject.transform.eulerAngles = new Vector3(0, 0, angleBetweenStates * (currentState - 1));
    }

    public static Vector2 Rotate(Vector2 v, float rad)
    {
        float sin = Mathf.Sin(rad);
        float cos = Mathf.Cos(rad);

        float tx = v.x;
        float ty = v.y;
        v.x = (cos * tx) - (sin * ty);
        v.y = (sin * tx) + (cos * ty);
        return v;
    }
}
