using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Timer : NetworkBehaviourExtension
{
    public static Timer instance;

    private float _normalTimeMode = 8;
    private float _speedTimeMode = 0.1f;
    
    [SerializeField] private Text timerText;
    
    private int _timeMultiplier = 1;
    
    private readonly Dictionary<string, float> _playersSkipDict = new Dictionary<string, float>();
    private int _endSkipTime = 0;
    
    public SleepAreaDoing nowSleepObject;//todo выглядит как костыль
    
    public float currentTime = 740;//Реальное время
    private float currentTimeMode = 8;
    public int gameTimer;//Игровое время
    
    private void Awake()
    {
        instance = this;
    }
    
    void FixedUpdate ()
    {
        if (!isServer) return;
        currentTime += Time.fixedDeltaTime * _timeMultiplier;
        var newTime = (int)(currentTime/ currentTimeMode);
        if (newTime != gameTimer)
            localCommands.RpcSendTime(newTime);
    }
    
    public void SetGameTimer(int newTime)
    {
        gameTimer = newTime;
        if (!GameSystem.instance.allInited) return;//Не должны ничего делать, пока не инициализируем все полностью
        if (gameTimer > 1439)
        {
            if (_endSkipTime != 0)
            {
                _endSkipTime -= 1439;
                if (_endSkipTime == 0)
                    CleanSkip();
            }
            gameTimer = 0;
            currentTime = 0;
            SetNewDay();
            if(isServer) taskManager.SetDayTasksTemplate();//Заполняем таски на день
            taskManager.SetDayDoingsTemplate();//Заполняем действия типа затемнения
        }
        var val1 = gameTimer / 60;
        var val2 = gameTimer % 60;
        var hourText = val1 < 10 ? "0" + val1 : val1.ToString();
        var minutesText = val2 < 10 ? "0" + val2 : val2.ToString();
        UpdateTimer(hourText + ":" + minutesText);
        taskManager.CheckEndStartTimeDoings(); 
        if(isServer) taskManager.CheckEndStartTasks();

        if (_endSkipTime != 0 && gameTimer > _endSkipTime) CleanSkip();
    }
    
    /**
     * Выполняется только на сервере
     */
    public void ReadyToSkip(string id, int newTime)
    {
        if (_playersSkipDict.ContainsKey(id))
            _playersSkipDict[id] = newTime;
        else
            _playersSkipDict.Add(id, newTime);
        Sleep(false);
    }

    /**
     * Выполняется только на сервере
     */
    public void UnReadyToSkip(string id, bool removing)
    {
        if (_playersSkipDict.ContainsKey(id))
            _playersSkipDict.Remove(id);
        Sleep(removing);
    }

    /**
     * Выполняется только на сервере
     */
    private void Sleep(bool removing)
    {
        if (_playersSkipDict.Count != EntitysController.instance.playersData.Count - (removing ? 1 : 0)) 
        {
            StopSkip();
            return;
        }
        SkipTo((int)_playersSkipDict.Values.Min());
        if (!isServer) return;
        foreach (var mini in GameObject.FindGameObjectsWithTag("SleepMiniGame"))//todo Find
        {
            mini.GetComponent<SleepMiniGame>().AllSleep();
        }
    }

    /**
     * Выполняется только на сервере
     */
    public void SkipTo(int endTime, int mult = 320)
    {
        localCommands.RpcSetEnergyMultiplayer(100);
        _endSkipTime = endTime;
        _timeMultiplier = mult;
        ObjectsScript.instance.botSpeedMultiplayer = 4;
    }

    private void StopSkip()
    {
        _timeMultiplier = 1;
        localCommands.RpcSetEnergyMultiplayer(1);
        _endSkipTime = 0;
        ObjectsScript.instance.botSpeedMultiplayer = 1;
    }
    
    private void CleanSkip()
    {
        _playersSkipDict.Clear();
        StopSkip();
        localCommands.RpcVakeUp();
    }

    private void UpdateTimer(string newText)
    {
        timerText.text = newText;
    }

    private void SetNewDay()
    {
        if (nowSleepObject != null)
        {
            nowSleepObject.SendReadyToSkip(localPlayerId);
        }
    }
}
