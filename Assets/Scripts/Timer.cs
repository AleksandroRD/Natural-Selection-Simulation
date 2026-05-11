using System;
using UnityEngine;

public class Timer
{
    float timeFromStart = 0;
    float endTime;
    bool started = false;
    public event Action OnTimerStarted;
    public event Action OnTimerFinished;
    public Timer(float time = 3)
    {
        endTime = time;
    }

    public void Start()
    {
        started = true; 
        OnTimerStarted?.Invoke();
    }

    public void Tick()
    {
        if(!started) { return; }
        timeFromStart += Time.deltaTime;

        if(timeFromStart >= endTime)
        {
            OnTimerFinished.Invoke();

            timeFromStart = 0;

            started = false;
        }
    }
}
