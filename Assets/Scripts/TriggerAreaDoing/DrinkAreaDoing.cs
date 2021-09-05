using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DrinkAreaDoing : TriggerAreaDoing, IScaleDoing
{
    readonly string[] potionsNames = new string[] { "WaterEmpty", "PyatLEmpty" };

    public override bool Do()
    {
        //Создать у всех видимую шкалу, а у этого только действующую
        localMoving.SetScaleInHands(1, localHealthBar.Energy, true, 1, this);
        //cmd.CmdDestroyScaleInHands(drinkPlayer);
        localCommands.CmdSetScaleInHands(localPlayer, 1, localHealthBar.Energy, 1);//создаем шкалу у все
        return true;
    }

    public void ScaleDo()
    {
        FindObjectOfType<DebafsController>().AddDebaf(1);//кидаем баф
        localHealthBar.AddWater(10);
        CheckFill();
    }

    void CheckFill()
    {
        for (var n = 0; n < localPlayerInventoryController.inventories.Length; n++)
        {
            var data = localPlayerInventoryController.inventories[n];
            for (var y = 0; y < data.ROWS; y++)
                for (var x = 0; x < data.COLS; x++)
                {
                    if (potionsNames.Contains(data.cells[y,x].ItemName))
                    {
                        localCommands.CmdFillAllPotions(n, y, x);
                    }
                }
        }
    }
}
