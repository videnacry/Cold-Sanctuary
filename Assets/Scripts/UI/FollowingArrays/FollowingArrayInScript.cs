using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[Serializable]
public struct FollowingArrayInScript
{
    public HashSetHolder script;
    public GameObject arrayItemTemplate;
    public FollowingElementBehavior followingElementBehavior;
    public static void RenderFollowingArrays(FollowingArrayInScript[] followingArrayInScripts, GameObject user, Queue<GameObject> uiElements)
    {
        foreach (FollowingArrayInScript followingArrayInScript in followingArrayInScripts)
        {
            foreach (GameObject element in followingArrayInScript.script.GetHashSetHolded())
            {
                GameObject followingElement = MonoBehaviour.Instantiate(followingArrayInScript.arrayItemTemplate, user.transform);
                FollowingElement followingElementScript = followingElement.AddComponent<FollowingElement>();
                followingElementScript.followingElementBehavior = followingArrayInScript.followingElementBehavior;
                followingElementScript.followingElementBehavior.Init(element);
                uiElements.Enqueue(followingElement);
            }
        }
    }
}