using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public abstract class Herbivore : Animal
{
    /// <summary>
    /// Stops walking, go to hunt the near rabbit
    /// </summary>
    /// <returns>yield</returns>
    public override IEnumerator Feed()
    {
        yield return new WaitForSeconds(1);
    }
}