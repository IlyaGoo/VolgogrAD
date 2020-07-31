using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ListenersManager : MonoBehaviour
{
    public PlayerMetaData Meta;

    private readonly KeyCode[] alphaKeyCodes = {//без девятки, потому что инвентарь на 8 слотов
         KeyCode.Alpha1,
         KeyCode.Alpha2,
         KeyCode.Alpha3,
         KeyCode.Alpha4,
         KeyCode.Alpha5,
         KeyCode.Alpha6,
         KeyCode.Alpha7,
         KeyCode.Alpha8
     };

    public List<IListener> SpaceListeners = new List<IListener>();
    public List<IListener> FListeners = new List<IListener>();
    public List<IListener> QListeners = new List<IListener>();
    public List<IUpListener> QUpListeners = new List<IUpListener>();
    public List<IListener> AltListeners = new List<IListener>();
    public List<IUpListener> AltUpListeners = new List<IUpListener>();

    public List<IListener> TabListeners = new List<IListener>();
    public List<IListener> EscListeners = new List<IListener>();
    public List<IListener> NumbersListeners = new List<IListener>();

    public List<INumListener> AlphaListeners = new List<INumListener>();
    public List<IScrollListener> ScrollListeners = new List<IScrollListener>();

    void OnGUI()
    {
        var e = Event.current;
        if (e.isKey)
        {
            if (e.type == EventType.KeyDown)
            {
                switch (e.keyCode)
                {
                    case KeyCode.Space:
                        if (SpaceListeners.Count == 0) return;
                        SpaceListeners[SpaceListeners.Count - 1].EventDid();
                        break;
                    case KeyCode.F:
                        if (FListeners.Count == 0) return;
                        FListeners[FListeners.Count - 1].EventDid();
                        break;
                    case KeyCode.LeftAlt:
                        if (AltListeners.Count == 0) return;
                        AltListeners[AltListeners.Count - 1].EventDid();
                        break;
                    case KeyCode.Tab:
                        if (TabListeners.Count == 0) return;
                        TabListeners[TabListeners.Count - 1].EventDid();
                        break;
                    case KeyCode.Escape:
                        if (EscListeners.Count == 0) return;
                        EscListeners[EscListeners.Count - 1].EventDid();
                        break;
                    case KeyCode.Q:
                        if (QListeners.Count == 0) return;
                        QListeners[QListeners.Count - 1].EventDid();
                        break;
                    default:
                        var i = Array.IndexOf(alphaKeyCodes, e.keyCode);
                        if (i == -1 || AlphaListeners.Count == 0)
                            break;
                        AlphaListeners[AlphaListeners.Count - 1].EventNumDid(i);
                        break;
                }
            }
            else if (e.type == EventType.KeyUp)
            {
                switch (e.keyCode)
                {
                    case KeyCode.LeftAlt:
                        if (AltUpListeners.Count == 0) return;
                        AltUpListeners[AltUpListeners.Count - 1].EventUpDid();
                        break;
                    case KeyCode.Q:
                        if (QUpListeners.Count == 0) return;
                        QUpListeners[QUpListeners.Count - 1].EventUpDid();
                        break;
                }
            }
        }
        else if (e.isScrollWheel)
        {
            if (ScrollListeners.Count == 0) return;
            ScrollListeners[ScrollListeners.Count - 1].EventScrollDid(e.delta.y);
        }
    }
}

public interface IListener//Класс че-то делает при нажатии клавиши
{
    void EventDid();
}

public interface IUpListener//Класс че-то делает при отпускании клавиши
{
    void EventUpDid();
}

public interface IMultyListener//Класс че-то делает при нажатии любой клавиши из набора
{
    void EventMultyDid(KeyCode code);
}

public interface INumListener//Класс че-то делает при нажатии цифры
{
    void EventNumDid(int code);
}

public interface IScrollListener//Класс че-то делает при кручении колеса
{
    void EventScrollDid(float code);
}
