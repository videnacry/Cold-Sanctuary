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
            if (followingArray.array.Length > 0)
            {
                foreach (GameObject element in followingArray.array)
                {
                    GameObject followingElement = MonoBehaviour.Instantiate(followingArray.arrayItemTemplate, user.transform);
                    followingElement.SetActive(true); // Instantiate copia activeSelf del template — las plantillas se guardan inactivas
                    if (followingArray.followingElementBehavior != null)
                    {
                        FollowingElement followingElementScript = followingElement.AddComponent<FollowingElement>();
                        // Misma razón que en FollowingArrayInScript: instancia propia por elemento.
                        FollowingElementBehavior behaviorInstance =
                            (FollowingElementBehavior)followingElement.AddComponent(followingArray.followingElementBehavior.GetType());
                        followingElementScript.followingElementBehavior = behaviorInstance;
                        behaviorInstance.Init(element);
                    }
                    uiElements.Enqueue(followingElement);
                }
            }
            else
            {
                GameObject followingElement = MonoBehaviour.Instantiate(followingArray.arrayItemTemplate, user.transform);
                followingElement.SetActive(true); // Instantiate copia activeSelf del template — las plantillas se guardan inactivas
                uiElements.Enqueue(followingElement);
            }
        }
    }
}