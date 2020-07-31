using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour {

    private Text EnergyBar;
    private Text WaterBar;
    public int Energy = 100;
    public int multiplayer = 1;

    public int Water = 100;

    private float WaterTimer;
    private float EnergyTimer;

    private void Start() {
        EnergyBar = GameObject.FindGameObjectWithTag("HPScale").GetComponent<Text>();
        WaterBar = GameObject.FindGameObjectWithTag("WaterBar").GetComponent<Text>();
    }

    void Update()
    {
        WaterTimer += Time.deltaTime;
        EnergyTimer += Time.deltaTime;
        if (WaterTimer > 30)
        {
            WaterTimer = 0;
            AddWater(-1);
        }
        if (EnergyTimer > 5f / multiplayer && Water > 0)
        {
            EnergyTimer = 0;
            AddEnergy(1);
        }
    }

    public void AddEnergy(int plusEnergy) {
        if (Energy == 100)
            EnergyTimer = 0;
        Energy += plusEnergy;
        Energy = Mathf.Min(Energy, 100);
        Energy = Mathf.Max(Energy, 0);
        EnergyBar.text = "" + Energy;
    }

    public void AddWater(int water)
    {
        if (Water == 100)
            WaterTimer = 0;
        Water += water;
        Water = Mathf.Min(Water, 100);
        Water = Mathf.Max(Water, 0);
        WaterBar.text = "" + Water;
    }

    private void check_death()
    {
        if (Energy < 1 || Water < 1)
        {
            //Написать смерть
        }
    }
}