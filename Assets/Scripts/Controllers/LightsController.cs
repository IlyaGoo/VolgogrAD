using UnityEngine;


/** Временный костыль-контроллер света, пока не можем решить все проблемы со светом */
public class LightsController : MonoBehaviour
{
    public static LightsController instance;
    
    public UnityEngine.Rendering.Universal.Light2D globalLight;
    public float currentLightLevel = 1;

    private void Awake()
    {
        instance = this;
    }

    public void SetConnectionLight(float intensy, float secondIntensy)
    {
        globalLight.intensity = intensy;
        currentLightLevel = secondIntensy;

        foreach (var l in LightScript.allLights)
            l.SetLight(currentLightLevel);
    }

    public void SetNightObject(bool state)
    {
        foreach (var nightObject in NightObject.allNightObjects)
            nightObject.SetState(state);
    }
    
    public void SetLight(float intensy, float secondIntensy)
    {
        SetConnectionLight(Mathf.Max(0, intensy + 0.2f), Mathf.Max(0, secondIntensy - globalLight.intensity));
    }
}
