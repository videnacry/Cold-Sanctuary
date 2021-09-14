using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FollowingArrays : MonoBehaviour
{
    public Queue<GameObject> uiElements = new Queue<GameObject>();
    public GameObject user;
    public GameObject closeButtonTemplate;
    public FollowingArrays parentFollowingArrays;
    [Serializable]
    public struct FollowingArray_Array
    {
        public GameObject[] array;
        public GameObject arrayItemTemplate;
        public FollowingElementBehavior followingElementBehavior;
    }
    [Serializable]
    public struct FollowingArray_Script
    {
        public HashSetHolder script;
        public GameObject arrayItemTemplate;
        public FollowingElementBehavior followingElementBehavior;
    }
    public FollowingArray_Script[] followingArray_Scripts;
    public FollowingArray_Array[] followingArray_Arrays;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    public FollowingArray_Array[] RenderFollowingArray(FollowingArray_Script[] followingArray_Scripts)
    {
        foreach (FollowingArray_Script followingArray in followingArray_Scripts)
        {
            foreach (GameObject element in followingArray.script.GetHashSetHolded())
            {
                GameObject followingElement = Instantiate(followingArray.arrayItemTemplate, user.transform);
                FollowingElement followingElementScript = followingElement.AddComponent<FollowingElement>();
                followingElementScript.followingElementBehavior = followingArray.followingElementBehavior;
                followingElementScript.followingElementBehavior.Init(element);
                uiElements.Enqueue(followingElement);
            }
        }
        return followingArray_Arrays;
    }
    public void RenderFollowingArray(FollowingArray_Array[] followingArray_Arrays)
    {
        foreach (FollowingArray_Array followingArray in followingArray_Arrays)
        {
            foreach (GameObject element in followingArray.array)
            {
                GameObject followingElement = Instantiate(followingArray.arrayItemTemplate, user.transform);
                FollowingElement followingElementScript = followingElement.AddComponent<FollowingElement>();
                followingElementScript.followingElementBehavior = followingArray.followingElementBehavior;
                followingElementScript.followingElementBehavior.Init(element);
                uiElements.Enqueue(followingElement);
            }
        }
    }
    private void OnMouseDown()
    {
        Show();
    }
    public void Show()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y - 500, transform.position.z);
        RenderFollowingArray(followingArray_Arrays);
        RenderFollowingArray(followingArray_Scripts);
        GameObject closeButton = Instantiate(closeButtonTemplate, transform);
        HideFollowingArrays hideFollowingArrays = closeButton.AddComponent<HideFollowingArrays>();
        hideFollowingArrays.followingArrays = this;
        closeButton.transform.localPosition = new Vector3(0, 1000, 0);
        uiElements.Enqueue(closeButton);
    }
    public void Hide()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y + 500, transform.position.z);
        foreach ( GameObject gameObject in uiElements)
        {
            Destroy(gameObject);
        }
        uiElements.Clear();
    }
}
