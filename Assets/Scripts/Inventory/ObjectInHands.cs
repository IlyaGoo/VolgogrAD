using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectInHands : MonoBehaviour {

    public Vector3 sidePosition = new Vector3(-0.5f, 0, 0);
    public Vector3 backPosition = new Vector3(0, 0.6f, 0.6f);
    public Vector3 frontPosition = new Vector3(0, -0.2f, -0.2f);

    [SerializeField] protected Vector3 sideAllposition = new Vector3(0, 0.1f, 0.1f);
    [SerializeField] protected GameObject[] parts;
    [SerializeField] protected Vector3 startScale = new Vector3(1, 1, 1);
    [SerializeField] protected bool freezePositionOnSide = false;
    protected int currentState = -1;
    protected bool currentScale = true;
    public Animator[] anims;
    protected bool inited = false;
    protected bool needInit = false;

    [SerializeField] SpriteRenderer[] renders;
    protected Vector3 backPos1;
    protected Vector3 backPos2;
    protected Vector3 frontPos1;
    protected Vector3 frontPos2;
    protected Vector3 positiveScale;
    protected Vector3 negativeScale;

    public virtual void CustomStart()
    {
        //Invoke("Init", 0.1f);
        sidePosition += sideAllposition;
        SetPositions();
        if (!freezePositionOnSide)
            transform.localScale = startScale;
        else
        {
            transform.localScale = transform.parent.localScale.x > 0 ? positiveScale : negativeScale;
        }
    }

    protected virtual void SetPositions()
    {
        backPos1 = backPosition;
        backPos2 = new Vector3(-backPosition.x, backPosition.y, backPosition.z);
        frontPos1 = frontPosition;
        frontPos2 = new Vector3(-frontPosition.x, frontPosition.y, frontPosition.z);
        positiveScale = startScale;
        negativeScale = new Vector3(-startScale.x, startScale.y, startScale.z);
        SetPosition(currentState, currentScale, true);
    }

    private void Init()
    {
        foreach (var render in renders)
            render.enabled = true;
        inited = true;
    }

    void Update()
    {
        if (!inited)
        {
            if (needInit)
                Init();
            else
                needInit = true;
        }
    }

    public void SetPositionValues(int frontState, bool posScale)
    {
        currentState = frontState;
        currentScale = posScale;
    }

    public virtual void SetPosition(int frontState, bool posScale, bool force = false)
    {
        //if (currentState == frontState && currentScale == posScale && !force) return; //Защита от дублирования итак есть
        currentState = frontState;
        currentScale = posScale;

        if (anims.Length != 0)
            foreach (var part in anims)
            {
                part.SetInteger("State", frontState);
            }

        if (freezePositionOnSide)
        {
            transform.localPosition = new Vector3((posScale ? 1 : -1) * sidePosition.x, sidePosition.y, sidePosition.z);
            transform.localPosition += sideAllposition;
            transform.localScale = transform.localScale = posScale ? positiveScale : negativeScale;
        }
        else
            switch (frontState)
            {
                case 1:
                    //currentRender.transform.localScale = new Vector3(1,1,1);
                    transform.localPosition = posScale ? backPos1 : backPos2;
                    transform.localScale = posScale ? positiveScale : negativeScale;
                    break;
                case 2:
                    //currentRender.transform.localScale = new Vector3(posScale?-1:1, 1, 1);
                    transform.localPosition = sidePosition;
                    transform.localScale = startScale;
                    break;
                default:
                    //currentRender.transform.localScale = new Vector3(-1, 1, 1);
                    transform.localPosition = posScale ? frontPos1 : frontPos2;
                    transform.localScale = posScale ? positiveScale : negativeScale;
                    break;
            }
    }
}
