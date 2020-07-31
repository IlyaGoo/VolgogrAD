using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChatHelper : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IListener, INumListener, IScrollListener
{

    public TMP_InputField InputF;
    public TextMeshProUGUI TextBlock;
    [SerializeField] GameObject ChatObject;
    [SerializeField] GameObject SendButton;
    [SerializeField] float delay = 5f;
    float currentTimer = 0;
    bool mouseOn;
    bool inputFIsSelected = false;

    bool needSlashInNextFrame = false;
    bool needSlashInNextFrame2 = false;


    const int massageMemory = 10;
    List<string> previousMassages = new List<string>(massageMemory);
    string currentMassage = "";
    int currentMassagesPosition = -1;

    public ChatPlayerHelper _currentplayer;
    public ListenersManager manager;
    bool isActive;

    public string Nickname = "";

    public void StuckPlayer()
    {
        inputFIsSelected = true;
        _currentplayer.GetComponent<Moving>().stacked = true;
        if (!manager.SpaceListeners.Contains(this))
        {
            manager.SpaceListeners.Add(this);
            manager.NumbersListeners.Add(this);
            manager.FListeners.Add(this);
            manager.ScrollListeners.Add(this);
            manager.AlphaListeners.Add(this);
        }
    }

    public void UnStuckPlayer()
    {
        inputFIsSelected = false;
        _currentplayer.GetComponent<Moving>().stacked = false;
        if (manager.SpaceListeners.Contains(this))
        {
            manager.SpaceListeners.Remove(this);
            manager.NumbersListeners.Remove(this);
            manager.FListeners.Remove(this);
            manager.ScrollListeners.Remove(this);
            manager.AlphaListeners.Remove(this);
        }
        if (!mouseOn)
        {
            SetVisualState(false);
            currentTimer = delay;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetVisualState(true);
        mouseOn = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        mouseOn = false;
        if (InputF.isFocused) return;
        SetVisualState(false);
        currentTimer = delay;

    }

    void SetVisualState(bool state)
    {
        isActive = state;
        if (!state) InputF.DeactivateInputField();
        InputF.gameObject.SetActive(state);
        ChatObject.SetActive(state);
        TextBlock.gameObject.SetActive(true);
        SendButton.SetActive(state);
    }

    public void Send()
    {
        UnStuckPlayer();
        if (!mouseOn)
        {
            SetVisualState(false);
            currentTimer = delay;
        }
        if (InputF.text == "") return;
        AddMassage(InputF.text);
        currentMassagesPosition = -1;
        _currentplayer.Send(Nickname, InputF.text);
        InputF.text = "";
    }

    void AddMassage(string massage)
    {
        if (previousMassages.Count == massageMemory) previousMassages.RemoveAt(0);
        previousMassages.Add(string.Copy(massage));
    }

    public void AddMessage(string id, string nickname, string message)
    {
        AddLine(nickname + " : " + message);
    }

    public void AddLine(string message)
    {
        currentTimer = delay;
        TextBlock.gameObject.SetActive(true);
        TextBlock.GetComponent<TextMeshProUGUI>().SetText(message + System.Environment.NewLine + TextBlock.GetComponent<TextMeshProUGUI>().GetParsedText());
    }

    void Update()
    {
        if (currentTimer > 0)
        {
            currentTimer -= Time.deltaTime;
            if (currentTimer <= 0 && !isActive)
                TextBlock.gameObject.SetActive(false);
        }
        if(needSlashInNextFrame)
        {
            if (needSlashInNextFrame2)
            {
                if (!inputFIsSelected)
                {
                    SetVisualState(true);
                    SelectInputF();
                    SetSlash();
                }
                needSlashInNextFrame2 = false;
                needSlashInNextFrame = false;
            }
            else
                needSlashInNextFrame2 = true;
        }
    }

    void OnGUI()
    {
        var e = Event.current;
        if (e.isKey && e.type == EventType.KeyDown)
        {
            switch (e.keyCode)
            {
                case KeyCode.Return:
                    if (isActive)
                    {
                        if (inputFIsSelected)
                            Send();
                        else
                        {
                            SelectInputF();
                        }
                    }
                    else
                    {
                        SetVisualState(true);
                        SelectInputF();
                    }
                    break;
                case KeyCode.Slash:
                    if (!inputFIsSelected)
                    {
                        needSlashInNextFrame = true;
                    }
                    break;
                case KeyCode.UpArrow:
                    if (!inputFIsSelected)
                        break;
                    if (currentMassagesPosition == -1 && previousMassages.Count > 0)
                    {
                        currentMassage = string.Copy(InputF.text);
                        currentMassagesPosition += 1;
                        InputF.text = previousMassages[previousMassages.Count - 1];
                        InputF.caretPosition = InputF.text.Length;
                    }
                    else if (currentMassagesPosition != -1 && previousMassages.Count > currentMassagesPosition + 1)
                    {
                        currentMassagesPosition += 1;
                        InputF.text = previousMassages[previousMassages.Count - 1 - currentMassagesPosition];
                        InputF.caretPosition = InputF.text.Length;
                    }
                    break;
                case KeyCode.DownArrow:
                    if (!inputFIsSelected)
                        break;
                    if (currentMassagesPosition == 0)
                    {
                        currentMassagesPosition = -1;
                        InputF.text = currentMassage;
                        InputF.caretPosition = InputF.text.Length;
                    }
                    else if (currentMassagesPosition > 0)
                    {
                        currentMassagesPosition -= 1;
                        InputF.text = previousMassages[previousMassages.Count - 1 - currentMassagesPosition];
                        InputF.caretPosition = InputF.text.Length;
                    }
                    break;
                case KeyCode.Backspace:
                    if (!Input.GetKey(KeyCode.LeftControl) || !inputFIsSelected || InputF.caretPosition == 0)
                        break;
                    var currentWords = InputF.text.Substring(0, InputF.caretPosition).Split(' ');
                    if (currentWords.Length > 0)
                    {
                        var wordLength = currentWords[currentWords.Length - 1].Length;
                        var deleteWorldLength = wordLength + (InputF.caretPosition == wordLength ? 0 : 1);
                        var startPos = InputF.caretPosition - deleteWorldLength;
                        InputF.text = InputF.text.Remove(startPos + 1, deleteWorldLength-1);
                        InputF.caretPosition = startPos + 1;
                    }
                    break;
                case KeyCode.Delete:
                    if (!Input.GetKey(KeyCode.LeftControl) || !inputFIsSelected || InputF.caretPosition == InputF.text.Length)
                        break;
                    var currentWords2 = InputF.text.Substring(InputF.caretPosition).Split(' ');
                    if (currentWords2.Length > 0)
                    {
                        var wordLength = currentWords2[0].Length;
                        var deleteWorldLength = wordLength + (InputF.text.Length - InputF.text[InputF.caretPosition] == ' ' ? 1 : 0);
                        InputF.text = InputF.text.Remove(InputF.caretPosition, deleteWorldLength);
                    }
                    break;
            }
        }
    }

    void SetSlash()
    {
        InputF.text = "/";
        InputF.caretPosition = InputF.text.Length;
    }

    void SelectInputF()
    {
        StuckPlayer();
        InputF.ActivateInputField();
        //OffSelection();
    }

    void OffSelection()
    {
        StartCoroutine(MoveTextEnd_NextFrame());
    }

    IEnumerator MoveTextEnd_NextFrame()
    {
        yield return 0; // Skip the first frame in which this is called.
        InputF.MoveTextEnd(false); // Do this during the next frame.
    }

    public void EventDid()
    {
    }

    public void EventNumDid(int code)
    {
    }

    public void EventScrollDid(float code)
    {
    }
}
