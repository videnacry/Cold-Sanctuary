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
        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (uiElements.Count > 0) this.Hide();
            else this.Show();
        }
    }
    public void SetWholeGameObjectRenderer(bool val)
    {
        Renderer renderer = this.GetComponent<Renderer>();
        renderer.enabled = val;
        Canvas canvas = this.GetComponentInChildren<Canvas>();
        canvas.enabled = val;
    }
    public void Show()
    {
        GameObject closeButton = Instantiate(closeButtonTemplate, transform.parent.transform);
        FollowingArrayInScript.RenderFollowingArrays(followingArrayInScripts, user, uiElements);
        FollowingArrayInArray.RenderFollowingArrays(followingArrayInArrays, user, uiElements);
        HideFollowingArrays hideFollowingArrays = closeButton.AddComponent<HideFollowingArrays>();
        hideFollowingArrays.followingArrays = this;
        this.uiElements.Enqueue(closeButton);
        this.SetWholeGameObjectRenderer(false);
    }
    public void Hide()
    {
        foreach ( GameObject gameObject in uiElements)
        {
            Destroy(gameObject);
        }
        this.uiElements.Clear();
        this.SetWholeGameObjectRenderer(true);
    }
    private void OnMouseDown()
    {
        this.Show();
    }
}
