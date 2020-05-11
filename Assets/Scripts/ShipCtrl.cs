using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipCtrl : MonoBehaviour
{
    // Start is called before the first frame update
    Animator ani;
    Rigidbody rig;
    float speed = 0,xAngle,yAngle;
    public float upAngle,rightAngle;
    Vector3 eulerAngles,camAngles;
    void Start()
    {
        ani = GetComponent<Animator>();
        rig = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        //camera, ship  angles, mouse axis

        upAngle = Camera.main.transform.localEulerAngles.y;
        rightAngle = Camera.main.transform.localEulerAngles.x;
        eulerAngles = transform.rotation.eulerAngles;
        xAngle = Input.GetAxis("Mouse X");
        yAngle = Input.GetAxis("Mouse Y");
        camAngles = Camera.main.transform.localEulerAngles;
        ani.Play("Idle");
        ani.speed = speed;

        //Movimiento del helicóptero(arriba,abajo,izquierda y derecha)

        rig.AddForce(transform.TransformDirection(Vector3.up)*speed,ForceMode.Acceleration);
        if (Input.GetKey(KeyCode.W))
        {
            if (speed < 13)
            {
                speed+=1;
            }
        }
        else if (Input.GetKey(KeyCode.S))
        {
            if (speed > 0)
            {
                speed-=1;
            }
        }
        else if (Input.GetKey(KeyCode.A))
        {
            transform.localRotation=Quaternion.Euler(this.eulerAngles.x,this.eulerAngles.y-1,this.eulerAngles.z);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            transform.localRotation = Quaternion.Euler(this.eulerAngles.x, this.eulerAngles.y + 1, this.eulerAngles.z);
        }
        else if (Input.GetKey(KeyCode.F))
        {
            ani.Play("Shoot");
        }

        //Movimiento del helicóptero

        if (Input.GetKey(KeyCode.Mouse0))
        {
            transform.Rotate(-yAngle, 0, xAngle);
        }

        //Movimiento de la cámara

        if (Input.mouseScrollDelta.y > 0 && Camera.main.fieldOfView < 60)
        {
            Camera.main.fieldOfView += 10;
        }
        else if (Input.mouseScrollDelta.y < 0 && Camera.main.fieldOfView > 20)
        {
            Camera.main.fieldOfView -= 10;
        }

        else
        {
            if (1 == 1)
            {
                Camera.main.transform.Rotate(yAngle, 0, 0);
            }
            else if (Camera.main.transform.rotation.eulerAngles.x < 180)
            {
                Camera.main.transform.rotation = Quaternion.Euler(180, camAngles.y, camAngles.z);
            }
            else
            {
                Camera.main.transform.rotation = Quaternion.Euler(250, camAngles.y, camAngles.z);
            }
            if (1 == 1)
            {
                Camera.main.transform.Rotate(0, xAngle, 0);
            }
            else if (Camera.main.transform.rotation.eulerAngles.y < 50)
            {
                Camera.main.transform.Rotate(0, 10, 0);
            }
            else
            {
                Camera.main.transform.Rotate(0, -10, 0);
            }
        }
    }
}
