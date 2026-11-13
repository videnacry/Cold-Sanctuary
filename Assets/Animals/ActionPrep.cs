using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class ActionPrep
{
    public string aniName;
    public float navSpeed;
    public float aniSpeed;
    public float energyCost;
    public ActionPrep(string pAniName, float pNavSpeed, float pAniSpeed = 1, float pEnergyCost = 0.5f)
    {
        this.aniName = pAniName;
        this.navSpeed = pNavSpeed;
        this.aniSpeed = pAniSpeed;
        this.energyCost = pEnergyCost/TimeController.timeController.TimeSpeedMinuteSecs/4;
    }
    /// <summary>Pausas («idle») de otra conducta que no se aplicaron porque el animal llevaba un viaje de deseo en marcha (diagnóstico).</summary>
    public static int IdleRefused;
    public void Prep(Animal pScript, float pAnimationTime)
    {
        // (2026-11-13, vivo77) EL VIAJE EN MARCHA ES DUEÑO DE LA VELOCIDAD: «idle» pone nav.speed = 0, y lo llamaban conductas que no llevan el
        // cuerpo (PostNatalManager con madres y crías, el Forager que sigue comiendo, un hábito): el animal quedaba con camino completo y
        // «pedida 0,0» (osos, pingüinos, crías de la Cría; los animales no llevan WalkSpell, así que nada se la devolvía). Mientras el viaje
        // vaya, la pausa de otro no se aplica; al acabar el viaje, quien lo lleva ya se para él solo.
        if (pScript != null && pScript.ActsPrep != null && this == pScript.ActsPrep.idle && DesireCatalog.LiveTrip(pScript, out _)) { IdleRefused++; return; }
        // ROBUSTEZ: el sistema reproduce clips POR NOMBRE (ani.Play("Idle"/"Walk"/"Run"+especie)). Si el prefab no trae
        // Animator/Controller o le falta ese estado, NO reventamos (antes NPE-aba al primer movimiento): saltamos la
        // animación y seguimos con nav/exhaustion. Ver docs/unity-editor-manual.md §1.
        if (pScript.ani != null && pScript.ani.runtimeAnimatorController != null && pScript.ani.HasState(0, Animator.StringToHash(this.aniName)))
        {
            pScript.ani.Play(this.aniName);
            pScript.ani.speed = this.aniSpeed;
        }
        if (pScript.nav != null) pScript.nav.speed = this.navSpeed;          // fallback; si hay WalkSpell, FeedWalkSpeed lo sobrescribe
        pScript.Running = (pScript.ActsPrep != null && this == pScript.ActsPrep.run);   // ¿es la acción de CORRER? → channeling
        float exhaustion = this.energyCost * pAnimationTime;
        pScript.exhaustion += exhaustion;
        pScript.hungry += (exhaustion > 0) ? pScript.Body.GetMealWeight(pScript.rig.mass) * exhaustion : 0;
    }
}
