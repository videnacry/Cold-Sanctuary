using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class Sex
{
    public const char male = 'm';
    public const char female = 'f';
    public static char SwitchSex (char sex)
    {
        char actualSex = sex == female ? male : female;
        return actualSex;
    }
}