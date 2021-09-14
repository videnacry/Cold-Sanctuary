using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[Serializable]
public struct FollowingArrayInArray
{
    public GameObject[] array;
    public GameObject arrayItemTemplate;
    public FollowingElementBehavior followingElementBehavior;
    public static void RenderFollowingArrays(FollowingArrayInArray[] followingArray_Arrays, GameObject user, Queue<GameObject> uiElements)
    {
        foreach (FollowingArrayInArray followingArray in followingArray_Arrays)
        {
            foreach (GameObject element in followingArray.array)
            {
                GameObject followingElement = MonoBehaviour.Instantiate(followingArray.arrayItemTemplate, user.transform);
                FollowingElement followingElementScript = followingElement.AddComponent<FollowingElement>();
                followingElementScript.followingElementBehavior = followingArray.followingElementBehavior;
                followingElementScript.followingElementBehavior.Init(element);
                uiElements.Enqueue(followingElement);
            }
        }
    }
}