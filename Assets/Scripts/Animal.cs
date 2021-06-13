using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public abstract class Animal : MonoBehaviour, IAnimal, IFactory
{
    public static HashSet<GameObject> wholePopulation = new HashSet<GameObject>();
    public abstract HashSet<GameObject> Population { get ; set ; }
    public bool gender = false, moving = false, adult, alone, asleep = false, death = false;
    public float hungry, exhaustion, lp, sensibility;
    public Vector3 size, sizePotential;
    public NavMeshAgent nav;
    public Rigidbody rig;
    public Animator ani;

    public abstract AnimationsName animationsName { get; }

    public bool aware { get; set; } = false;

    public static GameObject[] StaticGenerateSquareRange(GameObject animal, int quantity, float range, float respawnHeight)
    {
        GameObject[] creatures = new GameObject[quantity];
        for (int idx = 0; quantity > idx; idx++)
        {
            GameObject creature = Instantiate(animal, new Vector3(UnityEngine.Random.Range(0, range), respawnHeight, UnityEngine.Random.Range(0, range)), animal.transform.rotation);
            Vector3 scale = creature.transform.localScale;
            creature.transform.localScale = new Vector3(scale.x - UnityEngine.Random.Range(0.1f, 0.4f), scale.y - UnityEngine.Random.Range(0.1f, 0.4f), scale.z - UnityEngine.Random.Range(0.1f, 0.4f));
            creatures[idx] = creature;
            Animal creatureScript = creature.GetComponent<Animal>();
            creatureScript.Population.Add(creature);
            wholePopulation.Add(creature);
        }
        return creatures;
    }
    public virtual GameObject[] GenerateSquareRange(GameObject animal, int quantity, float range, float respawnHeight)
    {
        return Animal.StaticGenerateSquareRange(animal, quantity, range, respawnHeight);
    }

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
            Population.Remove(this.gameObject);
            wholePopulation.Remove(this.gameObject);
            Destroy(this);
        }
    }
}