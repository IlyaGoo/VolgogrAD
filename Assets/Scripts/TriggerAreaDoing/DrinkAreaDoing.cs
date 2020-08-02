using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DrinkAreaDoing : TriggerAreaDoing, IScaleDoing
{
    readonly string[] potionsNames = new string[] { "WaterEmpty", "PyatLEmpty" };
    GameObject pl;
    Moving mov;
    HealthBar bar;
    Commands cmd;
    InventoryController controller;

    public override bool Do(GameObject drinkPlayer)
    {
        if (pl == null)
        {
            pl = drinkPlayer;
            mov = drinkPlayer.GetComponent<Moving>();
            bar = drinkPlayer.GetComponent<HealthBar>();
            cmd = drinkPlayer.GetComponent<Commands>();
            controller = drinkPlayer.GetComponent<InventoryController>();
        }
        //Создать у всех видимую шкалу, а у этого только действующую
        mov.SetScaleInHands(1, bar.Energy, true, 1, this);
        //cmd.CmdDestroyScaleInHands(drinkPlayer);
        cmd.CmdSetScaleInHands(pl, 1, bar.Energy, 1);//создаем шкалу у все
        return true;
    }

    public void ScaleDo()
    {
        FindObjectOfType<DebafsController>().AddDebaf(1);//кидаем баф
        bar.AddWater(10);
        CheckFill();
    }

    void CheckFill()
    {
        for (var n = 0; n < controller.inventories.Length; n++)
        {
            var data = controller.inventories[n];
            for (var y = 0; y < data.ROWS; y++)
                for (var x = 0; x < data.COLS; x++)
                {
                    if (potionsNames.Contains(data.cells[y,x].ItemName))
                    {
                        cmd.CmdFillAllPotions(n, y, x);
                    }
                }
        }
    }
}
