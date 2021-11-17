using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HideFollowingArrays : MonoBehaviour
{
    public FollowingArrays followingArrays;
    private void OnMouseDown()
    {
        followingArrays.HideArrays();
    }
    private void Update()
    {
    }
}
