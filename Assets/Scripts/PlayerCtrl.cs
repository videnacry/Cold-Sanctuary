using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerCtrl : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject arm, forearm, finger1, finger2, finger3, finger4,rifle,torso,leftArm,leftForearm;
    public Vector3 torsoRotation, leftArmRotation, leftForearmRotation;
    public Vector3 armRotation,armDestiny,forearmRotation;
    public Vector3 finger1Rotation, finger2Rotation, finger3Rotation, finger4Rotation;
    public Vector3 rifleRotation,rifleDestiny,angle,shootPoint;
    public GameObject bullet;
    public int shootSpeed;
    public bool showRotation;
    public NavMeshAgent nav;
    Animator ani;
    Vector3 torsoOrigin, leftForearmOrigin, leftArmOrigin, rifleOrigin, armOrigin, forearmOrigin, finger1Origin, finger2Origin, finger3Origin, finger4Origin;
    Vector3 armRoot, rifleRoot;
    bool pointing=false,moving=false;
    float speed = 0.2f, potential = 0;
    int act = 0,bullets = 1000;
    string objective = "bunny";
    Vector3 angles;
    void Start()
    {
        ani=GetComponent<Animator>();
        armOrigin = arm.transform.localRotation.eulerAngles;
        forearmOrigin = forearm.transform.localRotation.eulerAngles;
        finger1Origin = finger1.transform.localRotation.eulerAngles;
        finger2Origin = finger2.transform.localRotation.eulerAngles;
        finger3Origin = finger3.transform.localRotation.eulerAngles;
        finger4Origin = finger4.transform.localRotation.eulerAngles;
        rifleOrigin = rifle.transform.localRotation.eulerAngles;
        rifleRoot = rifle.transform.localPosition;
        torsoOrigin = torso.transform.localRotation.eulerAngles;
        leftArmOrigin = leftArm.transform.localRotation.eulerAngles;
        leftForearmOrigin = leftForearm.transform.localRotation.eulerAngles;
        armRoot = arm.transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        //Get main camera angles

        angles = Camera.main.transform.eulerAngles;

        //Show the final position of the rifle

        if (showRotation)
        {
            arm.transform.localRotation = Quaternion.Euler(armRotation+armOrigin);
            forearm.transform.localRotation = Quaternion.Euler(forearmRotation+forearmOrigin);
            finger1.transform.localRotation = Quaternion.Euler(finger1Rotation+finger1Origin);
            finger2.transform.localRotation = Quaternion.Euler(finger2Rotation+finger2Origin);
            finger3.transform.localRotation = Quaternion.Euler(finger3Rotation+finger3Origin);
            finger4.transform.localRotation = Quaternion.Euler(finger4Rotation+finger4Origin);
            rifle.transform.localRotation = Quaternion.Euler(rifleRotation+rifleOrigin);
            rifle.transform.localPosition = rifleDestiny+rifleRoot;
            torso.transform.localRotation = Quaternion.Euler(torsoRotation+torsoOrigin);
            leftArm.transform.localRotation = Quaternion.Euler(leftArmRotation+leftArmOrigin);
            leftForearm.transform.localRotation = Quaternion.Euler(leftForearmRotation+leftForearmOrigin);
            arm.transform.localPosition = armDestiny+armRoot;
        }

        //Animación

        switch (act)
        {
            case 0:
                ani.speed = 1;
                ani.Play("Idle");
                break;
            case 1:
                ani.speed = 1;
                ani.Play("Walk");
                break;
            case 2:
                ani.speed = speed * 4;
                ani.Play("Run");
                break;
        }

        //Movimiento del personaje(adelante,atrás,izquierda y derecha)

        if (Input.GetKey(KeyCode.W))
        {
            nav.transform.Translate(transform.forward * speed);
            potential = 0.4f;
            act = 1;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            nav.transform.Translate(transform.forward * -speed);
            potential = 0.3f;
            act = 1;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            nav.transform.Translate(transform.right * -speed);
            potential = 0.3f;
            act = 1;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            nav.transform.Translate(transform.right * speed);
            potential = 0.3f;
            act = 1;
        }
        else
        {
            act = 0;
            speed = 0.1f;
        }

        //Incrementar velocidad de desplazamiento o la disminuye

        if (Input.GetKey(KeyCode.LeftShift) && speed > 0)
        {
            act = 2;
            if (speed < (potential-0.05f))
            {
                speed += Time.deltaTime/2;
            }
            else if(speed>potential)
            {
                speed = 0.1f;
            }
        }
        else
        {
            if (speed > 0.2f)
            {
                act = 2;
                speed -= Time.deltaTime;
            }
        }

        //Movimiento de la cámara apuntando con el rifle

        if (Input.GetKey(KeyCode.Mouse1)&&!moving)
        {
            if (!pointing)
            {
                moving = true;
                StartCoroutine("Point");
            }
            else if(!moving)
            {
                float rotY = Input.GetAxis("Mouse X");
                float rotX = Input.GetAxis("Mouse Y");
                transform.Rotate(0, rotY, 0);
                if (((angles.x < 120 || angles.x > 300) && rotX > 0) || ((angles.x < 70 || angles.x > 250) && rotX < 0))
                {
                    Camera.main.transform.Rotate(-rotX, 0, 0);
                    arm.transform.Rotate(rotX, 0, 0);
                    leftArm.transform.Rotate(-rotX, 0, 0);
                    rifle.transform.Rotate(0, rotX, 0);
                }
                if (Input.GetKey(KeyCode.Mouse2))
                {
                    if (bullets > 0)
                    {
                        bullets--;
                        StartCoroutine("Shoot");
                        Debug.Log(bullets);
                        GameObject shoot = Instantiate(bullet, rifle.transform.position + rifle.transform.TransformDirection(shootPoint), rifle.transform.rotation);
                        if(this.objective == "bunny")
                        {
                            foreach (GameObject bunny in Respawn.rabbits)
                            {
                                StartCoroutine(bunny.GetComponent<BunnyBehavior>().Shooted(shoot));
                            }
                        }
                        else if(this.objective == "bear")
                        {
                            foreach (GameObject bear in Respawn.bears)
                            {
                                StartCoroutine(bear.GetComponent<BearBehaviour>().Shooted(shoot));
                            }
                        }
                        shoot.GetComponent<Rigidbody>().AddForce(rifle.transform.TransformDirection(shootPoint * shootSpeed), ForceMode.Acceleration);
                    }
                }
                if (Input.GetKey(KeyCode.G))
                {
                    //Change objective, so change drug to make the target sleep
                    switch (this.objective)
                    {
                        case "bunny":objective = "bear"; Debug.Log("bear") ; break;
                        case "bear":objective = "bunny"; Debug.Log("bunny"); break;
                    }
                }
            }
        }
        else
        {
            if (pointing&&!moving)
            {
                moving = true;
                StartCoroutine("Refocus");
            }
        }

        //Movimiento de la cámara

        if (Camera.main.fieldOfView<60 && Input.mouseScrollDelta.y>0)
        {
            Camera.main.fieldOfView += 10;
        }
        if (Input.mouseScrollDelta.y<0 && Camera.main.fieldOfView > 20)
        {
            Camera.main.fieldOfView -= 10;
        }

        if (!pointing&&!moving)
        {
            rifleRoot = rifle.transform.localPosition;
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                transform.Rotate(Vector3.down);
            }
            else if (Input.GetKey(KeyCode.RightArrow))
            {
                transform.Rotate(Vector3.up);
            }
            if (Input.GetKey(KeyCode.UpArrow))
            {
                if (angles.x < 120 || angles.x > 300)
                {
                    Camera.main.transform.Rotate(Vector3.left);
                    this.angle = Camera.main.transform.eulerAngles;
                }
            }
            else if (Input.GetKey(KeyCode.DownArrow))
            {
                if (angles.x < 70 || angles.x >250)
                {
                    Camera.main.transform.Rotate(Vector3.right);
                }
            }
        }
    }
    
    //Coroutines

    IEnumerator Point()
    {
        while (rifle.transform.localRotation.eulerAngles != rifleOrigin+rifleRotation)
        {
            rifle.transform.localRotation = Quaternion.RotateTowards(rifle.transform.localRotation, Quaternion.Euler(rifleRotation+rifleOrigin), 2);
            rifle.transform.localPosition = Vector3.MoveTowards(rifle.transform.localPosition, rifleDestiny+rifleRoot, Time.deltaTime*2);
            arm.transform.localPosition = Vector3.MoveTowards(arm.transform.localPosition, armDestiny+armRoot, Time.deltaTime*2);
            arm.transform.localRotation = Quaternion.RotateTowards(arm.transform.localRotation, Quaternion.Euler(armRotation+armOrigin), 2);
            forearm.transform.localRotation = Quaternion.RotateTowards(forearm.transform.localRotation, Quaternion.Euler(forearmRotation+forearmOrigin), 2);
            finger1.transform.localRotation = Quaternion.RotateTowards(finger1.transform.localRotation, Quaternion.Euler(finger1Rotation+finger1Origin), 2);
            finger2.transform.localRotation = Quaternion.RotateTowards(finger2.transform.localRotation, Quaternion.Euler(finger2Rotation+finger2Origin), 2);
            finger3.transform.localRotation = Quaternion.RotateTowards(finger3.transform.localRotation, Quaternion.Euler(finger3Rotation+finger3Origin), 2);
            finger4.transform.localRotation = Quaternion.RotateTowards(finger4.transform.localRotation, Quaternion.Euler(finger4Rotation+finger4Origin), 2);
            torso.transform.localRotation = Quaternion.RotateTowards(torso.transform.localRotation, Quaternion.Euler(torsoRotation+torsoOrigin), 2);
            leftArm.transform.localRotation = Quaternion.RotateTowards(leftArm.transform.localRotation, Quaternion.Euler(leftArmRotation+leftArmOrigin), 2);
            leftForearm.transform.localRotation = Quaternion.RotateTowards(leftForearm.transform.localRotation, Quaternion.Euler(leftForearmRotation+leftForearmOrigin), 2);
            yield return new WaitForSeconds(0.01f);
        }
        moving = false;
        pointing = true;
    }

    IEnumerator Refocus()
    {
        while (rifle.transform.localEulerAngles!=rifleOrigin)
        {
            rifle.transform.localRotation = Quaternion.RotateTowards(rifle.transform.localRotation, Quaternion.Euler(rifleOrigin), 2);
            rifle.transform.localPosition = Vector3.MoveTowards(rifle.transform.localPosition,rifleRoot, Time.deltaTime*2);
            arm.transform.localRotation = Quaternion.RotateTowards(arm.transform.localRotation, Quaternion.Euler(armOrigin),2);
            arm.transform.localPosition = Vector3.MoveTowards(arm.transform.localPosition, armRoot, Time.deltaTime*2);
            forearm.transform.localRotation = Quaternion.RotateTowards(forearm.transform.localRotation, Quaternion.Euler(forearmOrigin), 2);
            finger1.transform.localRotation = Quaternion.RotateTowards(finger1.transform.localRotation, Quaternion.Euler(finger1Origin), 2);
            finger2.transform.localRotation = Quaternion.RotateTowards(finger2.transform.localRotation, Quaternion.Euler(finger2Origin), 2);
            finger3.transform.localRotation = Quaternion.RotateTowards(finger3.transform.localRotation, Quaternion.Euler(finger3Origin), 2);
            finger4.transform.localRotation = Quaternion.RotateTowards(finger4.transform.localRotation, Quaternion.Euler(finger4Origin), 2);
            torso.transform.localRotation = Quaternion.RotateTowards(torso.transform.localRotation, Quaternion.Euler(torsoOrigin), 2);
            leftArm.transform.localRotation = Quaternion.RotateTowards(leftArm.transform.localRotation, Quaternion.Euler(leftArmOrigin), 2);
            leftForearm.transform.localRotation = Quaternion.RotateTowards(leftForearm.transform.localRotation, Quaternion.Euler(leftForearmOrigin), 2);
            yield return new WaitForSeconds(0.01f);
        }
        moving = false;
        pointing = false;
    }

    IEnumerator Shoot()
    {
        moving = true;
        yield return new WaitForSeconds(1);
        moving = false;
    }
}
