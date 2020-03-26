﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class VRUIVibration : MonoBehaviour
{
    [SerializeField]
    private float vibrationFrequency;
    [SerializeField]
    private float vibrationAmplitude;
    [SerializeField]
    private float vibrationDuration;

    public abstract void Vibrate();

    public float VibrationFrequency
    {
        get { return vibrationFrequency; }
        set
        {
            if (value <= 0)
                vibrationFrequency = 0;
            else if (value >= 1)
                vibrationFrequency = 1;
            else
                vibrationFrequency = value;
        }
    }

    public float VibrationAmplitude
    {
        get { return vibrationAmplitude; }
        set
        {
            if (value <= 0)
                vibrationAmplitude = 0;
            else if (value >= 1)
                vibrationAmplitude = 1;
            else
                vibrationAmplitude = value;
        }
    }

    public float VibrationDuration
    {
        get { return vibrationDuration; }
        set { vibrationDuration = value; }
    }
}
