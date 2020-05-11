using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface DestructibleStats
{
    // Start is called before the first frame update
    bool aware
    {
        get;
        set;
    }
    void Hurt(float damage);
    IEnumerator Escape(bool team, List<GameObject> enemies);
}
