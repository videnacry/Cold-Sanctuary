using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class Adulthood : LifeStage
{
    /*
    public override IEnumerator Live (GameObject script)
    {
        Animal script = script.GetComponent<Animal>();
        int interval = Random.Range(40, 60);
        int livedDays = 0;
        while ((stageDays - livedDays) > 0)
        {

            if ((livedDays % 365) == 0)
            {
                script.StartCoroutine(script.Reproduction.Live(script));
            }
            livedDays++;
            yield return new WaitForSeconds(interval);
        }
        script.adult = true;
    }
    */
    public override IEnumerator Live (Animal script, TimeController timeController)
    {
        script.transform.localScale = sizePotential;
        while ((stageDays - livedDays) > 0)
        {
            livedDays++;

            yield return new WaitForSeconds(timeController.TimeSpeedMinuteSecs / Random.Range(1.0f, 2.0f));
        }
        script.moving = false;
    }
    
}