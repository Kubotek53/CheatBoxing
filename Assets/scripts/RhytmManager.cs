using UnityEngine;

public class RhytmManager:
MonoBehaviour
{
    public float bpm= 120f;
    private float beatInterval;
    private float beatTimer;
    void Start()
    {
        beatInterval =  60F/bpm;
    }

    void update()
    {
        beatTimer += beatTimer.deltaTime;
        if(beatTimer >= beatInterval)
        {
            beatTimer -=beatInterval;
            Debug.log("Beat");
        }
    }
}
