using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DrivePreparation : MonoBehaviour
{
    // Start is called before the first frame update
    MonoBehaviour playerScript, shipScript;
    PullDoor doorScript,door1Script ;
    NavMeshAgent nav;
    public bool showSitted, moving = false, into;
    public GameObject player, ship,door,door1;
    public Vector3 chairPlace,exitPlace;
    void Start()
    {
        this.playerScript=player.GetComponent<MonoBehaviour>();
        this.shipScript = ship.GetComponent<MonoBehaviour>();
        this.nav = player.GetComponent<NavMeshAgent>();
        this.doorScript = this.door.GetComponent<PullDoor>();
        this.door1Script = this.door1.GetComponent<PullDoor>();
    }

    // Update is called once per frame
    void Update()
    {
        if (showSitted)
        {
            player.transform.position = transform.position+transform.TransformDirection(chairPlace);
            player.transform.LookAt(transform.position,transform.TransformDirection(Vector3.forward));
            if (into)
            {
                player.transform.position = transform.position + transform.TransformDirection(exitPlace);
            }
        }
        if (into)
        {
            player.transform.position = (transform.position + transform.TransformDirection(chairPlace));
            player.transform.LookAt(transform.position);
        }
    }
    private void OnMouseDown()
    {
        if (!moving)
        {
            if (into)
            {
                moving = true;
                shipScript.enabled = false;
                doorScript.enabled = true;
                door1Script.enabled = true;
                StartCoroutine("Exit");
            }
            else if (doorScript.open)
            {
                moving = true;
                playerScript.enabled = false;
                nav.enabled = false;
                StartCoroutine("SitDown");
            }
        }        
    }
    IEnumerator SitDown()
    {
        while (player.transform.position != transform.position+transform.TransformDirection(chairPlace))
        {
            player.transform.position = Vector3.MoveTowards(player.transform.position, transform.position+transform.TransformDirection(chairPlace),.2f);
            player.transform.LookAt(Vector3.MoveTowards(player.transform.position,transform.position,1));
            yield return new WaitForSeconds(0.01f);
        }
        doorScript.StartCoroutine("Close");
        if (door1Script.open)
        {
            door1Script.StartCoroutine("Close");
        }
        while (doorScript.open||door1Script.open)
        {
            yield return new WaitForSeconds(0.01f);
        }
        doorScript.enabled = false;
        door1Script.enabled = false;
        shipScript.enabled = true;
        into = true;
        moving = false;
    }

    IEnumerator Exit()
    {
        doorScript.StartCoroutine("Open");
        while (!doorScript.open)
        {
            yield return new WaitForSeconds(.01f);
        }
        into=false;
        while (player.transform.position != transform.TransformDirection(exitPlace) + transform.position)
        {
            player.transform.position = Vector3.MoveTowards(player.transform.position, transform.position + transform.TransformDirection(exitPlace), .2f);
            yield return null;
        }
        nav.enabled = true;
        playerScript.enabled = true;
        moving = false;
    }
}
