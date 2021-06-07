using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAnimal
{
    // Start is called before the first frame update
    public AnimationsName animationsName { get; }
    bool aware { get; set; }
    void Hurt(float damage);
    IEnumerator Escape(bool team, List<GameObject> enemies);
}
