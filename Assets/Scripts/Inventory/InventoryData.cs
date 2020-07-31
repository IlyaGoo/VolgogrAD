using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class InventoryData : MonoBehaviour
{

    public int ROWS = 3;
    public int COLS = 8;
    public (string, int)[,] data;
    public int number;

    void Start()
    {
        if (data == null)
            Init();
    }

    public void Init()
    {
        data = new (string, int)[ROWS, COLS];
        for (var y = 0; y < ROWS; y++)
        {
            for (var x = 0; x < COLS; x++)
            {
                data[y, x] = ("", 0);
            }
        }
    }

    public bool RemoveItem(int y, int x, int count)
    {
        data[y, x] = (data[y, x].Item1, data[y, x].Item2-count);
        if (data[y, x].Item2 < 1)
        {
            data[y, x] = ("", 0);
        }
        return true;
    }

    public (string, int) AddItemWithPos(string name, int count, int y, int x)
    {
        ItemData newItem = ((GameObject)Resources.Load(name)).GetComponent<Item>().CopyItem();
        if (data[y, x].Item1 == "")
        {
            data[y, x] = (name, count);
            return ("", 0);
        }
        else if (data[y,x].Item1 == name)
        {
            var dif = data[y, x].Item2 + count - newItem.MaxAmount;
            if (dif < 0)
            {
                data[y, x] = (name, data[y, x].Item2 + count);
                return ("", 0);
            }
            else
            {
                data[y, x] = (name, newItem.MaxAmount);
                return (name, dif);
            }
        }
        else
        {
            var res = (data[y, x].Item1, data[y, x].Item2);
            data[y, x] = (name, count);
            return res;
        }
    }

    public (int, int, int, int) TryAddItemToStack(int MaxAmount, string name, int count)
    {
        
        for (var y = 0; y < ROWS; y++)//пытаемся всунуть в неполный стак
        {
            for (var x = 0; x < COLS; x++)
            {
                if (data[y, x].Item1 == name && data[y, x].Item2 + count <= MaxAmount)
                {
                    data[y, x] = (name, data[y, x].Item2 + count);
                    return (y, x, number, count);
                }
            }
        }
        return (-1, -1, -1, -1);
    }

    public (int, int, int, int) TryAddItemInEmpty(string name, int count)
    {
        for (var y = 0; y < ROWS; y++)//пытаемся всунуть в пустой стак
        {
            for (var x = 0; x < COLS; x++)
            {

                if (data[y, x].Item1 == "")
                {
                    data[y, x] = (name, count);
                    return (y, x, number, count);
                }
            }
        }
        return (-1, -1, -1, -1);
    }

    public (int, int, bool, int) AddItem(string name, int count)
    {
/*        for (var x = 0; x < COLS; x++)//пытаемся всунуть в нижнюю панель в пустой стак
        {
            if (data[3, x].Item1 == "")
            {
                print(true);
            }
            else
                print(false);
        }*/
        ItemData newItem = ((GameObject)Resources.Load(name)).GetComponent<Item>().CopyItem();
        for (var x = 0; x < COLS; x++)//пытаемся всунуть в нижнюю панель в неполный стак
        {
            if(data[3,x].Item1==name && data[3, x].Item2 + count <= newItem.MaxAmount)
            {
                data[3, x] = (name, data[3, x].Item2 + count);
                return (0,x, true, 0);
            }
        }
        for (var y = 0; y < ROWS-1; y++)//пытаемся всунуть в верхнюю панель в неполный стак
        {
            for (var x = 0; x < COLS; x++)
            {
                if (data[y, x].Item1 == name && data[y, x].Item2 + count <= newItem.MaxAmount)
                {
                    data[y, x] = (name, data[y, x].Item2 + count);
                    return (y,x,true,1);
                }
            }
        }




        for (var x = 0; x < COLS; x++)//пытаемся всунуть в нижнюю панель в пустой стак
        {
            if (data[3, x].Item1 == "")
            {
                data[3, x] = (name,  count);
                return (0,x,false,0);
            }
        }
        for (var y = 0; y < ROWS-1; y++)//пытаемся всунуть в верхнюю панель в пустой стак
        {
            for (var x = 0; x < COLS; x++)
            {
                if (data[y, x].Item1 == "")
                {
                    data[y, x] = (name, count);
                    return (y,x,false,1);
                }
            }
        }


        return (-1,-1, false, -1);
    }
}
