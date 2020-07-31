using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BodyTest : MonoBehaviour {

    /*
     * Этот скрипт - воплощение ненависти к Unity
     * Я очень долго пытался сделать контроллер, который управлет сразу несколькими частями тела
     * Долго пытался написать контроллер, который будет просто накидывать заранее созданные
     * анимации на части тела
     * Но вышло абсолютно нихуя
     * В итоге это огромный костыль, который позволяет нормально масштабировать и оптимизировать анимации 
     * персонажа, да, здесь много халтуры, но это именно то, что нужно было мне на момент написания кода
     * Господь, выеби юнити
     * Аминь
     */
    
    

    public int LagsNum = 1;
    public int BodyNum = 1;

    [SerializeField] SpriteRenderer body;
    [SerializeField] SpriteRenderer lags;
    [SerializeField] SpriteRenderer head;

    int[] curPor;
    readonly int[] por = new int[] {1,3,2,1,0};
    readonly int[] por2 = new int[] { 1, 2, 1, 0 };
    readonly int[] por3 = new int[] { 1 };

    readonly float delay = 0.08f;
    readonly float HeadDelay = 0.2f;

    public int state = 0;
    public int mainState;
    public bool initializated = false;
    // 0 - движение в сторону
    // 1 - движение вверх
    // 2 - движение вниз
    // 3 - стояние в сторону
    // 4 - стояние вверх
    // 5 - стояние вниз


    float currentTime = 0;
    float currentHeadTime = 0;

    Sprite[] currentBodySprites;
    Sprite[] currentLagsSprites;


    Sprite[] spritesLagsGoSide;
    Sprite[] spritesBodyGoSide;

    Sprite[] spritesLagsGoBack;
    Sprite[] spritesBodyGoBack;

    Sprite[] spritesLagsGoFront;
    Sprite[] spritesBodyGoFront;

    Sprite[] spritesHead;

    Vector3 headStart;

    void Start()
    {
        headStart = head.transform.position;
    }

    // Use this for initialization
    public void ChangeSprites (int i, int y, int z) {
        initializated = true;
        mainState = -1;
        spritesLagsGoSide = Resources.LoadAll<Sprite>("SpritesForBody/Lags" + i + "Side");
        spritesBodyGoSide = Resources.LoadAll<Sprite>("SpritesForBody/Body" + y + "Side");

        spritesLagsGoBack = Resources.LoadAll<Sprite>("SpritesForBody/Lags" + i + "Back");
        spritesBodyGoBack = Resources.LoadAll<Sprite>("SpritesForBody/Body" + y + "Back");

        spritesLagsGoFront = Resources.LoadAll<Sprite>("SpritesForBody/Lags" + i + "Front");
        spritesBodyGoFront = Resources.LoadAll<Sprite>("SpritesForBody/Body" + y + "Front");

        spritesHead = Resources.LoadAll<Sprite>("SpritesForBody/Head" + z);
        ChangeState(0);
    }

    public void SetXY(int x, int y)
    {
        if (y > 0)
        {
            ChangeState(1);
        }
        else if (y < 0)
        {
            ChangeState(2);
        }
        else if (x != 0)
        {
            ChangeState(0);
        }
        else if (mainState < 3)
        {
            switch (mainState)
            {
                case 0:
                    ChangeState(3);
                    break;
                case 1:
                    ChangeState(4);
                    break;
                default:
                    ChangeState(5);
                    break;
            }
        }
    }

    void ChangeState(int stateNum)
    {
        if (stateNum == mainState) return;
        state = 0;
        mainState = stateNum;
        currentTime = 0;
        currentHeadTime = 0;
        switch (stateNum)
        {
            case 0:
                currentBodySprites = spritesBodyGoSide;
                currentLagsSprites = spritesLagsGoSide;
                curPor = por;
                head.sprite = spritesHead[2];
                break;
            case 1:
                currentBodySprites = spritesBodyGoFront;
                currentLagsSprites = spritesLagsGoFront;
                curPor = por2;
                head.sprite = spritesHead[1];
                break;
            case 2:
                currentBodySprites = spritesBodyGoBack;
                currentLagsSprites = spritesLagsGoBack;
                curPor = por2;
                head.sprite = spritesHead[0];
                break;
            case 3:
                currentBodySprites = spritesBodyGoSide;
                currentLagsSprites = spritesLagsGoSide;
                curPor = por3;
                head.sprite = spritesHead[2];
                break;
            case 4:
                currentBodySprites = spritesBodyGoFront;
                currentLagsSprites = spritesLagsGoFront;
                curPor = por3;
                head.sprite = spritesHead[1];
                break;
            default:
                currentBodySprites = spritesBodyGoBack;
                currentLagsSprites = spritesLagsGoBack;
                curPor = por3;
                head.sprite = spritesHead[0];
                break;
        }
        head.gameObject.transform.position = headStart + gameObject.transform.position;
    }
	
	// Update is called once per frame
	void FixedUpdate() {
        if (spritesLagsGoSide == null) return;
        currentTime += Time.deltaTime;
        currentHeadTime += Time.deltaTime;
        if (currentTime >= delay)
        {
            currentTime = 0;
            state = state == curPor.Length - 1 ? 0 : state + 1;
            lags.sprite = currentLagsSprites[curPor[state]];
            body.sprite = currentBodySprites[curPor[state]];
        }

        if (mainState < 3)
            if (currentHeadTime < HeadDelay / 2)
            {
                head.gameObject.transform.position = new Vector3(head.gameObject.transform.position.x, head.gameObject.transform.position.y-Time.deltaTime/2, head.gameObject.transform.position.z);
            }
            else
            {
                head.gameObject.transform.position = new Vector3(head.gameObject.transform.position.x, head.gameObject.transform.position.y + Time.deltaTime/2, head.gameObject.transform.position.z);
                if(currentHeadTime >= HeadDelay)
                {
                    head.gameObject.transform.position = headStart + gameObject.transform.position;
                    currentHeadTime = 0;
                }
            }
    }
}
