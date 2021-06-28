using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class TimeController
{
    public static TimeController timeController = new TimeController();

    public byte TimeSpeed { get; private set; } = 1;
    public float TimeSpeedMinuteSecs { get; private set; } = 60 / 1;
    public void SetTimeSpeed (byte pTimeSpeed)
    {
        TimeSpeed = pTimeSpeed;
        TimeSpeedMinuteSecs = 60 / pTimeSpeed;
    }
}
