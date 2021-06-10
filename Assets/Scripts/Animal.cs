using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Animal : MonoBehaviour, IAnimal
{
    public HashSet<GameObject> population;
    public bool gender = false, moving = false, adult, alone, asleep = false, death = false;
    public float hungry, exhaustion, lp, sensibility;
    public Vector3 size, sizePotential;
    public NavMeshAgent nav;
    public Rigidbody rig;
    public Animator ani;

    public virtual AnimationsName animationsName { get; } = new AnimationsName("");

    public virtual bool aware { get; set; }

    public virtual IEnumerator Escape(bool team, List<GameObject> enemies)
    {
        throw new System.NotImplementedException();
    }

    public virtual void Hurt(float damage)
    {
        lp -= damage;
        exhaustion += damage;
        if (lp < (rig.mass * 0.7))
        {
            StopAllCoroutines();
            ani.speed = 0;
            this.rig.isKinematic = true;
            this.nav.enabled = false;
            if (!death) transform.Rotate(Vector3.forward, 90);
            death = true;
        }
        if (lp < 0)
        {
            population.Remove(this.gameObject);
            Destroy(this);
        }
    }
}