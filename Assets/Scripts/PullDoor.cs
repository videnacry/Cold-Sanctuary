using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PullDoor : MonoBehaviour
{
    // Start is called before the first frame update
    public float speed,lp;
    public bool showPath = false,showFall=false;
    public bool open = false,moving=false;
    public Vector3 origin, pull, slide,rotation,place;
    void Start()
    {
        this.origin= transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if (showPath)
        {
            transform.localPosition = origin + slide;
            if (showFall)
            {
                transform.localPosition = origin + place;
                transform.localRotation = Quaternion.Euler(rotation);
            }
        }
    }

    private void OnMouseDown()
    {
        if (moving == false)
        {
            if (open)
            {
                StartCoroutine("Close");
            }
            else
            {
                StartCoroutine("Open");
            }
        }
    }
    IEnumerator Open()
    {
        moving = true;
        while(transform.localPosition!=origin+this.pull)
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, origin+this.pull, Time.deltaTime);
            yield return new WaitForSeconds(speed);
        }
        while(transform.localPosition!=origin+this.slide)
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, origin+this.slide, Time.deltaTime);
            yield return new WaitForSeconds(speed);
        }
        this.open = true;
        moving = false;
    }

    IEnumerator Close()
    {
        moving = true;
        while(transform.localPosition!=origin+this.pull)
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, origin+this.pull, Time.deltaTime);
            yield return new WaitForSeconds(speed);
        }
        while(transform.localPosition!=this.origin)
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, origin, Time.deltaTime);
            yield return new WaitForSeconds(speed);
        }
        this.open = false;
        moving = false;
    }

    IEnumerator Fall()
    {
        this.moving = true;
        while (transform.localRotation != Quaternion.Euler(this.rotation))
        {
            transform.localRotation=Quaternion.RotateTowards(transform.localRotation, Quaternion.Euler(rotation), 1);
            yield return new WaitForSeconds(0.001f);
        }
        this.moving = false;
    }

    void OnCollissionEnter()
    {
        if (lp>0)
        {
            lp--;
        }
        else
        {
            StartCoroutine("Fall");
        }
    }
}
