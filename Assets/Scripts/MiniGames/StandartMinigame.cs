using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StandartMinigame : Minigame
{
    [SerializeField] protected float minusEnergyDelay = 0.7f;
    protected float minusEnergyTimer;
    protected HealthBar bar;

    public void Init(HealthBar newBar)
    {
        minusEnergyTimer = minusEnergyDelay;
        bar = newBar;
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
            bar.AddEnergy(-1);
            CheckEndEnergy();
        }
    }

    protected void CheckEndEnergy()
    {
        if (bar.Energy < 1)
        {
            _controller.ReinitializeTrigger();
            _controller.PreEndGame();
        }
    }

    void OnGUI()
    {
        var e = Event.current;
        if (Event.current.isKey && Event.current.type == EventType.KeyDown)
        {
            if (!GetUsingKeyKodes().Contains(e.keyCode))
            {
                _controller.ReinitializeTrigger();
                _controller.PreEndGame();
            }
        }
        else if (Event.current.isScrollWheel)
        {
            _controller.ReinitializeTrigger();
            _controller.PreEndGame();
        }
    }
}
