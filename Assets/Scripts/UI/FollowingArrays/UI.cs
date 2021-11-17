using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI : FollowingArrays
{
    public UIFollowingArrayElement[] uiFollowingArray;
    public FollowingArrays[] uiFollowingArrayElementsFollowingArrays;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (this.parentFollowingArrays)
            {
                if (!this.parentFollowingArrays.enabled)
                {
                    this.parentFollowingArrays.enabled = true;
                    this.HideArrays();
                }
            }
            else
            {
                this.HideArrays();
            }
        }
        if (this.GetComponent<Renderer>().enabled && !this.parentFollowingArrays && Input.GetKeyDown(KeyCode.Z))
        {
            this.ShowArrays();
            if (this.parentFollowingArrays) this.parentFollowingArrays.enabled = false;
        }
        else
        {
            int uiFollowingArraySelectedIndex = 100;
            if (Input.GetKeyDown(KeyCode.Z)) uiFollowingArraySelectedIndex = 0;
            if (Input.GetKeyDown(KeyCode.X)) uiFollowingArraySelectedIndex = 1;
            if (Input.GetKeyDown(KeyCode.C)) uiFollowingArraySelectedIndex = 2;
            if (Input.GetKeyDown(KeyCode.V)) uiFollowingArraySelectedIndex = 3;
            if (Input.GetKeyDown(KeyCode.B)) uiFollowingArraySelectedIndex = 4;
            if (this.uiFollowingArrayElementsFollowingArrays.Length >= (uiFollowingArraySelectedIndex + 1)) 
            {
                this.uiFollowingArrayElementsFollowingArrays[uiFollowingArraySelectedIndex].ShowArrays();
                this.enabled = false;
            }
        }
    }
    public override void ShowArrays()
    {
        FollowingArrayInScript.RenderFollowingArrays(followingArrayInScripts, user, uiElements);
        FollowingArrayInArray.RenderFollowingArrays(followingArrayInArrays, user, uiElements);
        UIFollowingArrayElement.RenderUIElements(this.uiFollowingArray, this.user, this.uiElements, ref this.uiFollowingArrayElementsFollowingArrays, this);
        this.AddCloseButton();
        if (this.parentFollowingArrays)
        {
            DestroyUIElements().ExceptGameObject(this.gameObject).Execute();
            this.parentFollowingArrays.uiElements.Clear();
        }
        this.SetWholeGameObjectRenderer(false);
    }
}
