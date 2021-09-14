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
    public FollowingArrayInScript[] followingArrayInScripts;
    public FollowingArrayInArray[] followingArrayInArrays;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }
    public void Show()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y - 500, transform.position.z);
        FollowingArrayInScript.RenderFollowingArrays(followingArrayInScripts, user, uiElements);
        FollowingArrayInArray.RenderFollowingArrays(followingArrayInArrays, user, uiElements);
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
    private void OnMouseDown()
    {
        Show();
    }
}
