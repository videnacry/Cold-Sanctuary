
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdBehavior : MonoBehaviour
{

    public int min, max;
    public float speed, tripTime;
    float velocity=0, lapse,altura;
    int giro=0;
    bool moving = false;
    Animator ani;

    // Start is called before the first frame update
    void Start()
    {
        ani = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!moving)
        {
            this.lapse = Random.Range(30, this.tripTime);
            this.velocity = Random.Range(speed, speed / 3)/60;
            this.giro = Random.Range(0, 4);
            altura = Altura(this.giro);
            Fly(velocity);
        }
    }

    void Fly(float velocity)
    {
        StartCoroutine("Move");
        ani.Play("Fly");
        ani.speed = velocity;
    }

    int Altura(int rotation)
    {
        switch (rotation)
        {
            case 1:
                return Random.Range(this.min, this.min + (this.max * 1));
            case 2:
                return Random.Range(this.min, this.min + (this.max * 2));
            case 3:
                return Random.Range(this.min, this.min + (this.max * 3));
            default:
                return Random.Range(this.min, this.min + (this.max * 4));
        }
    }

    //Coroutines

    IEnumerator Move()
    {
        moving = true;
        while (this.lapse>0)
        {
            transform.Rotate(Vector3.up * this.giro);
            transform.Translate(Vector3.forward * this.velocity);
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(transform.position.x, this.altura, transform.position.z), velocity / 2);
            lapse--;
            yield return new WaitForSeconds(.1f);
        }
        moving = false;
    }

}
