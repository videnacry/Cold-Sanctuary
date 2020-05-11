using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SlideDoor : MonoBehaviour
{
    public float speed,lp;
    public bool showPath=false,showFall=false;
    private bool open = false,moving=false;
    public Vector3 destiny,rotation,place;
    Vector3 origin,root;
    NavMeshObstacle nav;
    MonoBehaviour script;
    void Start()
    {
        this.origin = transform.localPosition;
        this.root = transform.localEulerAngles;
        nav = GetComponent<NavMeshObstacle>();
        script = GetComponent<SlideDoor>();
    }

    // Update is called once per frame
    void Update()
    {
        if (showPath)
        {
            transform.localPosition = destiny+origin;
            if (showFall)
            {
                transform.localRotation=Quaternion.Euler(rotation+root);
                transform.localPosition = place+origin;
            }
        }
    }

    private void OnMouseDown()
    {
        if (moving==false)
        {
            if (open == false)
            {
                StartCoroutine("Open");
            }
            else
            {
                StartCoroutine("Close");
            }
        }
    }

    IEnumerator Open()
    {
        this.moving = true;
            while(transform.localPosition!=this.destiny+this.origin)
            {
                transform.localPosition=Vector3.MoveTowards(transform.localPosition, this.destiny+this.origin, Time.deltaTime);
                yield return new WaitForSeconds(this.speed);
            }
        this.open = true;
        this.moving = false;
    }

    IEnumerator Close()
    {
        this.moving = true;
        while(transform.localPosition!=this.origin)
        {
            transform.localPosition=Vector3.MoveTowards(transform.localPosition, this.origin, Time.deltaTime);
            yield return new WaitForSeconds(this.speed);
        }
        this.moving = false;
        this.open = false;
    }

    IEnumerator Fall()
    {
        this.moving = true;
        while (transform.localPosition != this.place+this.root)
        {
            transform.localRotation=Quaternion.RotateTowards(transform.localRotation, Quaternion.Euler(rotation+this.root), 1);
            yield return new WaitForSeconds(0.01f);
        }
        nav.enabled = false;
        this.script.enabled = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (lp > 0)
        {
            lp--;
        }
        else
        {
            StartCoroutine("Fall");
        }
    }
}
