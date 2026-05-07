using UnityEngine;

public class RhytmManager:
MonoBehaviour
{
    public float bpm= 120f;
    public int i = 0;
    private float beatInterval;
    private float beatTimer;
    void Start()
    {
        beatInterval =  60F/bpm;
    }

    void Update()
    {
        beatTimer += Time.deltaTime;
        if(beatTimer >= beatInterval)
        {
            beatTimer -=beatInterval;
            i += 1;
            Debug.Log("Beat "+ i);
        }  
          }
}
