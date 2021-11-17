using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[Serializable]
public struct UIFollowingArrayElement
{
    public GameObject sheetButton;
    public FollowingArrays followingArrays;
    public static void RenderUIElements(UIFollowingArrayElement[] uiFollowingArrayElements, GameObject user, Queue<GameObject> uiElements, ref FollowingArrays[] elementsFollowingArrays, FollowingArrays parent)
    {
        elementsFollowingArrays = new FollowingArrays[uiFollowingArrayElements.Length];
        int elementsFollowingArraysIndex = -1;
        foreach (UIFollowingArrayElement uiFollowingArrayElement in uiFollowingArrayElements)
        {
            elementsFollowingArraysIndex++;
            GameObject followingElement = MonoBehaviour.Instantiate(uiFollowingArrayElement.sheetButton, user.transform);
            FollowingArrays followingElementScript = followingElement.GetComponent<FollowingArrays>();
            followingElementScript.user = user;
            followingElementScript.parentFollowingArrays = parent;
            uiElements.Enqueue(followingElement);
            elementsFollowingArrays[elementsFollowingArraysIndex] = followingElementScript;
        }
    }
}