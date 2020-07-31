using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum ObjectType
{
    Block,
    BackGroundBlock,
    SpecialBlock
}

public class EditorScript : MonoBehaviour {

    [SerializeField] private GameObject standertBlock;
    private GameObject currentGameobject = null;
    private GameObject needToCreate = null;
    private Vector2 mousePosition;
    private Stack<GameObject> stackOfAction = new Stack<GameObject>();
    [SerializeField] private GameObject startBlock = null;
    [SerializeField] private static int Dimension = 512;
    private GameObject[,] arrOfBlocks = new GameObject[Dimension * 2, Dimension * 2];
    private GameObject[,] arrOfBackgrondBlocks = new GameObject[Dimension * 2, Dimension * 2];
    private List<GameObject> listOfSpecoalObjects = new List<GameObject>();
    public Camera cameraComponent;
    [SerializeField] private GameObject box;

    [SerializeField] private GameObject blocksButtons;
    [SerializeField] private GameObject backGroundBlocksButtons;
    [SerializeField] private GameObject specialBlocksButtons;

    [SerializeField] private GameObject boxingObject;
    [SerializeField] private GameObject blocks;
    [SerializeField] private GameObject backGroundBlocks;

    [SerializeField]
    private GameObject startBox;
    private List<GameObject> boxing = new List<GameObject>();

    public void SwitchButtons (int num)
    {
        switch (num)
        {
            case 1:
                blocksButtons.SetActive(true);
                backGroundBlocksButtons.SetActive(false);
                specialBlocksButtons.SetActive(false);
                break;
            case 2:
                blocksButtons.SetActive(false);
                backGroundBlocksButtons.SetActive(true);
                specialBlocksButtons.SetActive(false);
                break;
            case 3:
                blocksButtons.SetActive(false);
                backGroundBlocksButtons.SetActive(false);
                specialBlocksButtons.SetActive(true);
                break;
        }
    }

    public void ChangeGameObject(GameObject needGameObject)
    {
        needToCreate = needGameObject;
    }

	void Start () {
        boxing.Add(startBox);
        needToCreate = standertBlock;
        stackOfAction.Push(startBlock);
        for (var i = 0; i < Dimension * 2; i++)
            for (var j = 0; j < Dimension * 2; j++)
            {
                arrOfBlocks[i, j] = null;
                arrOfBackgrondBlocks[i, j] = null;
            }
        arrOfBlocks[Dimension, Dimension] = startBlock;
    }

    public void MakeBoxing()
    {
        var newBoxing = new List<GameObject>();
        for (var j = 0; j < Dimension * 2; j++)
            for (var i = 0; i < Dimension * 2; i++)
            {
                if (arrOfBlocks[i, j] != null)
                {
                    var xPositions = -1.0f;
                    //var yPositions = -1.0f;
                    while (i < Dimension * 2 && arrOfBlocks[i,j] != null)
                    {
                        xPositions++;
                        i++;
                    }
                    i--;
                    GameObject createdBox = Instantiate(box, new Vector3((i - Dimension) * 2 - xPositions, (j - Dimension) * 2), Quaternion.identity) as GameObject;
                    createdBox.transform.localScale = new Vector3(xPositions + 1 - 0.1f, 0.9f, 1);
                    newBoxing.Add(createdBox);
                    createdBox.transform.parent = boxingObject.transform;
                }
            }
        foreach (var box in boxing)
            Destroy(box);
        boxing = newBoxing;
    }

    void Update () {
        if (Input.GetMouseButton(0) && Input.mousePosition.x > 320 && needToCreate != null)
            ClickOnMap();
        if (Input.GetMouseButton(1) && Input.mousePosition.x > 320)
            ChouseBlock();
        if (stackOfAction.Count != 0 && Input.GetKeyDown(KeyCode.Q))
            CancelAction();
        if (stackOfAction.Count != 0 && Input.GetKeyDown(KeyCode.E) && currentGameobject != null)
            DestroyThisBlock();
        if (Input.GetKeyDown(KeyCode.Alpha1))
            SwitchButtons(1);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            SwitchButtons(2);
        if (stackOfAction.Count != 0 && Input.GetKeyDown(KeyCode.F) && currentGameobject != null)
            FlipSpriteX();
        if (stackOfAction.Count != 0 && Input.GetKeyDown(KeyCode.G) && currentGameobject != null)
            FlipSpriteY();
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
            CameraMoving();
        //if (Input.GetMouseButtonUp(0))
        //    MakeBoxing();
    }

    private void CancelAction()
    {
        var deleted = false;
        while (!deleted && stackOfAction.Count > 0)
            try
            {
                var nedToDelete = stackOfAction.Pop();
                if (nedToDelete.tag == "BackGroundBlock")
                    arrOfBackgrondBlocks[(int)nedToDelete.transform.position.x + Dimension, (int)nedToDelete.transform.position.y + Dimension] = null;
                else
                    arrOfBlocks[(int)nedToDelete.transform.position.x + Dimension, (int)nedToDelete.transform.position.y + Dimension] = null;
                Destroy(nedToDelete);
                deleted = true;
            }
            catch
            {
            }
        //MakeBoxing();
    }

    private void FlipSpriteX()
    {
        var spriteRender = currentGameobject.GetComponent<SpriteRenderer>();
        if (spriteRender.flipX)
            spriteRender.flipX = false;
        else
            spriteRender.flipX = true;
    }

    private void FlipSpriteY()
    {
        var spriteRender = currentGameobject.GetComponent<SpriteRenderer>();
        if (spriteRender.flipY)
            spriteRender.flipY = false;
        else
            spriteRender.flipY = true;
    }

    private void CameraMoving()
    {
        if (Input.mousePosition.x > 320)
            cameraComponent.orthographicSize -= Input.GetAxis("Mouse ScrollWheel") * 5;
        else
        {
            var position = blocksButtons.transform.position;
            blocksButtons.transform.position = position - new Vector3(0, Input.GetAxis("Mouse ScrollWheel") * 50, 0);
            position = backGroundBlocksButtons.transform.position;
            backGroundBlocksButtons.transform.position = position - new Vector3(0, Input.GetAxis("Mouse ScrollWheel") * 50, 0);
        }
    }

    private void DestroyThisBlock()
    {
        if (currentGameobject.tag == "BackGroundBlock")
            arrOfBackgrondBlocks[(int)currentGameobject.transform.position.x + Dimension, (int)currentGameobject.transform.position.y + Dimension] = null;
        else
            arrOfBlocks[(int)currentGameobject.transform.position.x + Dimension, (int)currentGameobject.transform.position.y + Dimension] = null;
        Destroy(currentGameobject);
        currentGameobject = null;
        //MakeBoxing();
    }

    private void ChouseBlock()
    {
        var pos = Input.mousePosition;
        var ray = Camera.main.ScreenToWorldPoint(new Vector3(pos.x, pos.y, 5));
        ray.x = (float)(Math.Round(ray.x / 2) * 2);
        ray.y = (float)(Math.Round(ray.y / 2) * 2);

        var thisBlockInList = arrOfBlocks[(int)ray.x / 2 + Dimension, (int)ray.y / 2 + Dimension];
        if (thisBlockInList == null)
            thisBlockInList = arrOfBackgrondBlocks[(int)ray.x / 2 + Dimension, (int)ray.y / 2 + Dimension];

        currentGameobject = thisBlockInList;
    }

    private void ClickOnMap()
    {
        var pos = Input.mousePosition;
        var ray = Camera.main.ScreenToWorldPoint(new Vector3(pos.x, pos.y, 5));
        ray.x = (float)(Math.Round(ray.x));
        ray.y = (float)(Math.Round(ray.y));

        var thisBlockInList = arrOfBlocks[(int)ray.x + Dimension, (int)ray.y + Dimension];
        var thisBackGronfBlockInList = arrOfBackgrondBlocks[(int)ray.x + Dimension, (int)ray.y + Dimension];

        if (needToCreate.tag == "StandartBlock" && thisBlockInList == null || needToCreate.tag == "BackGroundBlock" && thisBackGronfBlockInList == null)
        {
            GameObject currentObject = Instantiate(needToCreate, ray, Quaternion.identity) as GameObject;
            stackOfAction.Push(currentObject);
            if (needToCreate.tag == "BackGroundBlock")
            {
                arrOfBackgrondBlocks[(int)(ray.x) + Dimension, (int)(ray.y) + Dimension] = currentObject;
                currentObject.transform.parent = backGroundBlocks.transform;
            }
            else
            {
                arrOfBlocks[(int)(ray.x) + Dimension, (int)(ray.y) + Dimension] = currentObject;
                currentObject.transform.parent = blocks.transform;
            }
        }
        else
        {
            currentGameobject = thisBlockInList;
            if (currentGameobject == null)
                currentGameobject = thisBackGronfBlockInList;
        }
    }
}
