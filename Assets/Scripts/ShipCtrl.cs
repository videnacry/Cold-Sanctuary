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
    public float minCamPitch = -80f, maxCamPitch = 80f;
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
            // Pitch libre del ratón, acotado a [minCamPitch, maxCamPitch] para que la cámara
            // no se dé la vuelta (antes: placeholder if(1==1) con ramas de clamp inalcanzables).
            float pitch = Mathf.DeltaAngle(0f, camAngles.x) + yAngle;
            pitch = Mathf.Clamp(pitch, minCamPitch, maxCamPitch);
            Camera.main.transform.localEulerAngles = new Vector3(pitch, camAngles.y, camAngles.z);
            Camera.main.transform.Rotate(0, xAngle, 0);
        }
    }
}
