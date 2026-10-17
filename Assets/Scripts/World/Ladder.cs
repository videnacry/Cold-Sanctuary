using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ESCALERA VERTICAL: F al pie = SUBIR, F arriba = BAJAR. Mientras sube, el cuerpo queda pegado a la escalera (sin
/// gravedad ni control) y brazos y piernas alternan peldaño a peldaño. Al llegar, deja al jugador en la plataforma (o en
/// el suelo). `ClimbAnima` también la usa la IA/los tests.
/// </summary>
public class Ladder : MonoBehaviour, IInteractable
{
    public bool top;
    public Transform ladder;
    public float deckY = 11f;
    public float speed = 1.6f;   // m/s
    static bool _busy;

    public string InteractLabel => top ? "Bajar por la escalera" : "Subir a la casa del árbol";
    public bool CanInteract => !_busy;

    public void Interact()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null && !_busy) StartCoroutine(Climb(p.transform, !top));
    }

    /// <summary>Sube (up) o baja al <paramref name="who"/> por la escalera.</summary>
    public IEnumerator Climb(Transform who, bool up)
    {
        _busy = true;
        CharacterController cc = who.GetComponent<CharacterController>();
        var controllers = new List<Behaviour>();
        foreach (MonoBehaviour mb in who.GetComponents<MonoBehaviour>())
            if (mb != null && mb.enabled && (mb is PlayerController)) { controllers.Add(mb); mb.enabled = false; }
        if (cc != null) cc.enabled = false;
        CreatureRig rig = who.GetComponent<CreatureRig>();

        Vector3 baseP = ladder.position - ladder.forward * 0.45f;            // pegado a la escalera, por fuera
        float y0 = up ? 0.05f : deckY + 0.1f, y1 = up ? deckY + 0.1f : 0.05f;
        who.rotation = Quaternion.LookRotation(ladder.forward);
        float yy = y0, phase = 0f;
        Debug.Log($"[Guardia] «{who.name}» {(up ? "sube" : "baja")} por la escalera de la casa del árbol.");
        while (up ? yy < y1 : yy > y1)
        {
            yy += (up ? 1f : -1f) * speed * Time.deltaTime;
            phase += speed * Time.deltaTime / 0.35f * Mathf.PI;
            who.position = new Vector3(baseP.x, ladder.position.y + yy + 1f, baseP.z);   // +1: el pivote del jugador va en su centro
            if (rig != null) Limbs(rig, phase);
            yield return null;
        }
        // Al llegar arriba, un paso hacia la plataforma; abajo, un paso atrás.
        who.position = up ? ladder.position + ladder.forward * 1.4f + Vector3.up * (deckY + 1.2f) : baseP - ladder.forward * 0.8f + Vector3.up * 1.1f;
        if (cc != null) cc.enabled = true;
        foreach (Behaviour b in controllers) b.enabled = true;
        Debug.Log($"[Guardia] «{who.name}» llega {(up ? "a la casa del árbol" : "al suelo")}.");
        _busy = false;
    }

    static void Limbs(CreatureRig rig, float phase)
    {
        float s = Mathf.Sin(phase);
        Set(rig, BodyPart.ShoulderLeft, -150f + 25f * s); Set(rig, BodyPart.ShoulderRight, -150f - 25f * s);
        Set(rig, BodyPart.ThighLeft, -40f - 30f * s);     Set(rig, BodyPart.ThighRight, -40f + 30f * s);
        Set(rig, BodyPart.ShinLeft, 60f + 20f * s);       Set(rig, BodyPart.ShinRight, 60f - 20f * s);
    }

    static void Set(CreatureRig rig, BodyPart p, float x)
    {
        Transform t = rig.Get(p);
        if (t == null) return;
        rig.LockPose(p, 0.2f);
        t.localRotation = Quaternion.Euler(x, 0f, 0f);
    }
}
