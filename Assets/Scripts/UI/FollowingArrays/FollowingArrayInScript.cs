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
                followingElement.SetActive(true); // Instantiate copia activeSelf del template — las plantillas se guardan inactivas
                FollowingElement followingElementScript = followingElement.AddComponent<FollowingElement>();
                // Cada elemento necesita su propia instancia del behavior — compartir una sola
                // (como antes) hace que Init() de cada iteración pise el estado de la anterior.
                FollowingElementBehavior behaviorInstance =
                    (FollowingElementBehavior)followingElement.AddComponent(followingArrayInScript.followingElementBehavior.GetType());
                followingElementScript.followingElementBehavior = behaviorInstance;
                behaviorInstance.Init(element);
                uiElements.Enqueue(followingElement);
            }
        }
    }
}