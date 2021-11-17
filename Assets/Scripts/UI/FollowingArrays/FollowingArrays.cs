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
        if (this.parentFollowingArrays)
            if (!this.parentFollowingArrays.enabled) 
                if (Input.GetKeyDown(KeyCode.Escape)) this.HideArrays();
        if (!this.parentFollowingArrays)
            if (Input.GetKeyDown(KeyCode.Z)) this.ShowArrays();
    }
    public void SetWholeGameObjectRenderer(bool val)
    {
        Renderer renderer = this.GetComponent<Renderer>();
        renderer.enabled = val;
        Canvas canva = this.GetComponentInChildren<Canvas>();
        canva.enabled = val;
        Collider collider = this.GetComponent<Collider>();
        collider.enabled = val;
    }
    public void AddCloseButton()
    {
        GameObject closeButton = Instantiate(closeButtonTemplate, transform.parent.transform);
        HideFollowingArrays hideFollowingArrays = closeButton.AddComponent<HideFollowingArrays>();
        hideFollowingArrays.followingArrays = this;
        this.uiElements.Enqueue(closeButton);
    }
    public virtual void ShowArrays()
    {
        FollowingArrayInScript.RenderFollowingArrays(followingArrayInScripts, user, uiElements);
        FollowingArrayInArray.RenderFollowingArrays(followingArrayInArrays, user, uiElements);
        this.AddCloseButton();
        if (this.parentFollowingArrays)
        {
            this.parentFollowingArrays.DestroyUIElements().ExceptGameObject(this.gameObject).Execute();
            this.parentFollowingArrays.uiElements.Clear();
        }
        this.SetWholeGameObjectRenderer(false);
    }
    public struct Destroyer 
    {
        public Queue<GameObject> uiElements;
        GameObject gameObject;
        public Destroyer ExceptGameObject(GameObject pGameObject)
        {
            this.gameObject = pGameObject;
            return this;
        }
        public void Execute()
        {
            foreach ( GameObject gameObject in uiElements)
            {
                if (gameObject == this.gameObject) continue;
                Destroy(gameObject);
            }
        }
    }
    public Destroyer DestroyUIElements()
    {
        Destroyer destroyer = new Destroyer();
        destroyer.uiElements = this.uiElements;
        return destroyer;
    }
    public void HideArrays()
    {
        foreach ( GameObject gameObject in uiElements)
        {
            Destroy(gameObject);
        }
        this.uiElements.Clear();
        if (this.parentFollowingArrays) 
        {
            this.parentFollowingArrays.enabled = true;
            this.parentFollowingArrays.ShowArrays();
            Destroy(this.gameObject);
        }
        else this.SetWholeGameObjectRenderer(true);
    }
    private void OnMouseDown()
    {
        this.ShowArrays();
    }
}
