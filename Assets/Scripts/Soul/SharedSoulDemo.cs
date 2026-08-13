using UnityEngine;

/// <summary>Demo (OnGUI) del ALMA COMPARTIDA (docs/soul-relations-reincarnation §4) para el sandbox
/// `AlmaCompartida_AUTO`: dos cuerpos (melaza/hormiga) comparten un alma; entrenar/lesionar el poder o añadir un
/// bond se **propaga a ambos**. Solo para el sandbox.</summary>
public class SharedSoulDemo : MonoBehaviour
{
    public SharedSoul soul;
    public SoulComposition bodyA;
    public SoulComposition bodyB;

    void OnGUI()
    {
        if (soul == null) return;
        int x = 392, y = 308;
        GUI.Box(new Rect(x, y, 342, 168), "AlmaCompartida (reencarnaciones) — docs soul-relations §4");
        y += 24;
        GUI.Label(new Rect(x + 8, y, 326, 20), $"«{soul.soulName}» poder {soul.power:0.00} · bonds {soul.sharedBonds.Count} · cuerpos {soul.BodyCount}");
        y += 22;
        if (bodyA != null && bodyA.anima != null)
        { GUI.Label(new Rect(x + 8, y, 326, 20), $"A ({bodyA.name}): str {bodyA.anima.strength:0.00} masa {bodyA.anima.bodyMass:0.00} agi {bodyA.anima.agility:0.00}"); y += 20; }
        if (bodyB != null && bodyB.anima != null)
        { GUI.Label(new Rect(x + 8, y, 326, 20), $"B ({bodyB.name}): str {bodyB.anima.strength:0.00} masa {bodyB.anima.bodyMass:0.00} agi {bodyB.anima.agility:0.00}"); y += 24; }

        if (GUI.Button(new Rect(x + 8, y, 158, 26), "Entrena poder (+0.5)")) soul.GainPower(0.5f);
        if (GUI.Button(new Rect(x + 176, y, 158, 26), "Se lesiona (−0.3)")) soul.GainPower(-0.3f);
        y += 30;
        if (GUI.Button(new Rect(x + 8, y, 326, 26), "Medea vincula con Ruth (+bond a TODAS las reencarnaciones)")) soul.AddBond("Ruth");
    }
}
