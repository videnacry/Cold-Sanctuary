using System;
using UnityEngine;

/// <summary>
/// Represents a single body part placement that composes an Asana or Exercise.
/// </summary>
[Serializable]
public class BodyPosition
{
    public BodyPart bodyPart;
    public string description;   // e.g. "Pie de atrás girado en 45°"
    public KeyCode hotkey;       // desktop key aligned to typing position

    public BodyPosition(BodyPart pBodyPart, string pDescription, KeyCode pHotkey)
    {
        bodyPart  = pBodyPart;
        description = pDescription;
        hotkey    = pHotkey;
    }
}

public enum BodyPart
{
    Elbows,
    Hands,
    Knees,
    Feet,
    Hips,
    Back,
    Shoulders,
    Head
}
