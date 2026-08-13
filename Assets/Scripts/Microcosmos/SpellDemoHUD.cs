using UnityEngine;

/// <summary>HUD de PRUEBA (OnGUI) del sandbox `SpellDemo_AUTO`: muestra los bonos de forcejeo/channeling del
/// fuego y del andar + la energía (ATP). Solo para el sandbox (docs/testing-checklist §19g).</summary>
public class SpellDemoHUD : MonoBehaviour
{
    public FireSpell fire;
    public WalkSpell walk;
    public Anima anima;

    void OnGUI()
    {
        int x = 392, y = 10;
        GUI.Box(new Rect(x, y, 328, 132), "SpellDemo_AUTO (docs testing §19g)");
        y += 24;
        GUI.Label(new Rect(x + 8, y, 312, 20), "G = fuego múltiple · G+Shift = cargar · WASD = andar · Shift = esprintar");
        y += 22;
        if (fire != null) { GUI.Label(new Rect(x + 8, y, 312, 20), $"Fuego — forcejeo {fire.RawForcejeo:0.00}   channel {fire.RawChannel:0.00}"); y += 20; }
        if (walk != null) { GUI.Label(new Rect(x + 8, y, 312, 20), $"Andar — forcejeo {walk.RawForcejeo:0.00}   channel {walk.RawChannel:0.00}"); y += 20; }
        if (anima != null)
        {
            CharacterLevel cl = anima.GetComponent<CharacterLevel>();
            if (cl != null) GUI.Label(new Rect(x + 8, y, 312, 20), $"Energía (ATP): {cl.currentEnergy:0} / {cl.MaxEnergy:0}");
        }
    }
}
