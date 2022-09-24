using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StandartMinigame : Minigame
{
    [SerializeField] protected float minusEnergyDelay = 0.7f;
    protected float minusEnergyTimer;

    public void Init()
    {
        minusEnergyTimer = minusEnergyDelay;
    }

    protected virtual KeyCode[] GetUsingKeyKodes()
    {
        return null;
    }

    protected void TimerTick()
    {
        minusEnergyTimer -= Time.fixedDeltaTime;
        if (minusEnergyTimer <= 0)
        {
            minusEnergyTimer = minusEnergyDelay;
            localHealthBar.AddEnergy(-1);
            CheckEndEnergy();
        }
    }

    protected void CheckEndEnergy()
    {
        if (localHealthBar.Energy < 1)
        {
            //step.ReinitializeTrigger();
            step.PreEndGame();
        }
    }
    
    /**Дополнительные действия при ивентах нажатия кнопок, например в QEMinigame нажате Q или E**/
    protected virtual void AddOnGUI(){}

    void OnGUI()
    {
        var e = Event.current;
        if (Event.current.isKey && Event.current.type == EventType.KeyDown)
        {
            if (!GetUsingKeyKodes().Contains(e.keyCode))
            {
                //step.ReinitializeTrigger();
                step.PreEndGame();
            }
            else
            {
                AddOnGUI();
            }
        }
        else if (Event.current.isScrollWheel)
        {
            //step.ReinitializeTrigger();
            step.PreEndGame();
        }
    }
}
