﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WholeScript : MonoBehaviour, ScaleDoing
{

    private GameObject block;
    List<DetectingItem>[] items = new List<DetectingItem>[3] {new List<DetectingItem>(), new List<DetectingItem>(), new List<DetectingItem>() };
    int currentLevel;
    [SerializeField] Sprite[] whole1Sprites;
    [SerializeField] Sprite[] whole2Sprites;
    [SerializeField] Sprite[] whole3Sprites;
    [SerializeField] BoxCollider2D carCollider;

    List<WholeScript> nearWholes = new List<WholeScript>();

    [SerializeField] GameObject miniDirt;
    GameObject[] parts = new GameObject[4];

    bool[,] squares = new bool[3,3];

    public GameObject Block
    {
        get
        {
            return block;
        }

        set
        {
            block = value;
        }
    }

    void Start()
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position + new Vector3(0.1f, 0.1f, 0), Vector2.zero);

        foreach (var hit in hits)
        {
            if (hit.collider != null && hit.collider.gameObject.tag == "StandartBlock")
            {
                transform.parent = hit.transform;
                hit.transform.GetComponent<BlockController>().CurrentWhole = this;
                return;
            }
        }
    }

    public void SetData(string data)
    {
        var d = data.Split('|');
        SpawnObjects();
    }

    void SpawnParts()
    {
        parts[0] = Instantiate(miniDirt, gameObject.transform.position + new Vector3(0,0.5f,0) + new Vector3(0.25f, 0.25f,0), Quaternion.identity);
        parts[1] = Instantiate(miniDirt, gameObject.transform.position + new Vector3(0.5f, 0.5f, 0) + new Vector3(0.25f, 0.25f, 0), Quaternion.identity);
        parts[3] = Instantiate(miniDirt, gameObject.transform.position + new Vector3(0.25f, 0.25f, 0), Quaternion.identity);
        parts[2] = Instantiate(miniDirt, gameObject.transform.position + new Vector3(0.5f, 0, 0) + new Vector3(0.25f, 0.25f, 0), Quaternion.identity);
        foreach(var p in parts)
            p.transform.parent = gameObject.transform;
    }

    void SetParts()
    {
        Block.GetComponent<SpriteRenderer>().enabled = false;
        SetSquares();
        SetPart(parts[0], squares[1,2], squares[0, 2], squares[0, 1], 0);
        SetPart(parts[1], squares[2, 1], squares[2, 2], squares[1, 2], 1);
        SetPart(parts[2], squares[1, 0], squares[2, 0], squares[2, 1], 2);
        SetPart(parts[3], squares[0, 1], squares[0, 0], squares[1, 0], 3);
    }

    void SetPart(GameObject part, bool p1, bool p2, bool p3, int turn)
    {
        int spriteNum;
        if (p1)
        {
            if (p2)
            {
                if (p3)
                {
                    spriteNum = 0;
                }
                else
                {
                    spriteNum = 2;
                }
            }
            else if (p3)
            {
                spriteNum = 1;
            }
            else
            {
                spriteNum = 2;
            }
        }
        else if (p2)
        {
            if (p3)
            {
                spriteNum = 2;//перевернуть
                turn++;
            }
            else
            {
                spriteNum = 3;
            }
        }
        else if (p3)
        {
            spriteNum = 2;//перевернуть
            turn++;
        }
        else
        {
            spriteNum = 3;
        }
        //print(spriteNum);
        part.GetComponent<SpriteRenderer>().sprite = new Sprite[][]{ whole1Sprites, whole2Sprites, whole3Sprites}[currentLevel - 1][spriteNum];
        part.transform.rotation = new Quaternion(0,0,0,1);
        part.transform.Rotate(0, 0, -90 * turn);
    }

    void SetSquares()
    {
        for (var x = 0; x < 3; x++)
        {
            for (var y = 0; y < 3; y++)
            {
                if (y == x && x == 1) continue;
                var w = IsWhole(transform.position + new Vector3(0.2f, 0.2f, 0) + new Vector3(x - 1, y - 1, 0));
                squares[x, y] = w != null && w.currentLevel != 0;
                //print(squares[x, y]);
                if (squares[x, y])
                    nearWholes.Add(w);
            }
        }
    }

    WholeScript IsWhole(Vector3 pos)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(pos, Vector2.zero);

        foreach (var hit in hits)
        {
            if (hit.collider != null && hit.collider.gameObject.tag == "StandartBlock")
            {
                var sc = hit.transform.GetComponent<BlockController>();
                if (sc == null) return null;
                return sc.CurrentWhole;
            }
        }

        return null;
    }

    public void GiveItems()
    {
        if (IsEnd()) return;
        var pl = GameObject.Find("LocalPlayer");
        var inv = pl.GetComponent<PlayerInventoryController>();
        foreach (var item in items[currentLevel])
        {
            if (inv.CanPutItem(item.itemData.CopyItem())) 
            {
                pl.GetComponent<Commands>().CmdTakeDetectingItem(item.gameObject);
            }
            else
                pl.GetComponent<Commands>().CmdThroveDetectingItem(item.gameObject, pl.transform.position, Quaternion.identity);
        }
    }

    public bool IsEnd() { return currentLevel + 1 > items.Length; }

    public void Dig()
    {
        if (IsEnd()) return;

        for (var i = currentLevel + 1; i < items.Length; i++)
        {
            foreach (var item in items[i])
            {
                item.GetComponent<DetectingItem>().currentDepth -= 1; 
            }
        }

        items[currentLevel].Clear();
        currentLevel++;
        carCollider.enabled = true;

        if (parts[0] == null)
            SpawnParts();
        SetParts();

        foreach (var wh in nearWholes)
        {
            wh.SetParts();
        }
        nearWholes.Clear();
    }

    void SpawnObjects()
    {
        int s = Random.Range(1, 121);
        if (s <= 60) return;
        else if (s <= 90)
        {
            SpawnNumObjectS(1);
        }
        else if (s <= 105)
        {
            SpawnNumObjectS(2);
        }
        else if (s <= 120)
        {
            SpawnNumObjectS(3);
        }
    }

    void SpawnNumObjectS(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var depth = Random.Range(1,4);
            var ob = GameObject.FindGameObjectWithTag("TaskManager").GetComponent<ObjectsScript>().GetRandomObject();
            Vector3 pos = gameObject.transform.position + new Vector3(0.15f, 0.15f, 0) + new Vector3(Random.Range(0, 0.7f), Random.Range(0, 0.7f), 0.6f);
            GameObject.Find("LocalPlayer").GetComponent<Commands>().CmdSpawnFindObject(ob, pos, new Quaternion(0,0,Random.Range(-1f,1f), 1), gameObject, depth);
        }
    }

    public void AddItem(DetectingItem item)
    {
        items[item.depth - 1].Add(item);
    }

    public void ScaleDo()
    {
        var player = GameObject.Find("LocalPlayer");
        GiveItems();
        player.GetComponent<PlayerNet>().CmdDig(gameObject);
        player.GetComponent<HealthBar>().AddEnergy(-5);
    }

    enum Side
    {
        Soviet,
        German
    }

    enum Quality
    {
        Standart,
        Rich
    }
}

public interface ScaleDoing
{
    void ScaleDo();
}