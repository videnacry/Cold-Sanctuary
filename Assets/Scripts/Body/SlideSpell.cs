using UnityEngine;

/// <summary>
/// BAJAR POR EL TOBOGÁN de la cama-litera (Berón 2026-11-04: «las literas tienen resbaladera»; docs/pijamadas.md). Quien despierta en
/// el piso de ARRIBA (un invitado de la pijamada) no baja trepando: se sienta en lo alto del tobogán y se desliza por él hasta el
/// suelo de la casa (<see cref="Bedroom.slideTop"/> → <see cref="Bedroom.slideBottom"/>), y allí se pone de pie. Para subir se sigue
/// trepando por el poste del pie. Acotado: <see cref="Seconds"/> de bajada, sin esperas.
/// </summary>
public class SlideSpell : QueuedSpell
{
    public Bedroom room;
    /// <summary>Lo que dura la bajada (s): unos 3 m de tobogán a ~2 m/s, frenando al final.</summary>
    public const float Seconds = 1.6f;
    /// <summary>Cuántos han bajado por un tobogán (las pruebas).</summary>
    public static int Slides;

    float _t = -1f;
    bool _ccWas;
    public override string Label => "bajar por el tobogán";

    public override bool Tick(Anima self)
    {
        if (self == null || room == null || room.slideTop == null || room.slideBottom == null) return true;
        CharacterController cc = self.GetComponent<CharacterController>();
        if (_t < 0f)
        {
            _t = 0f;
            _ccWas = cc != null && cc.enabled; if (cc != null) cc.enabled = false;
            Debug.Log($"[Cuerpo] «{self.name}» baja de la litera por el tobogán ({room.name}).");
        }
        _t += Time.deltaTime;
        float u = Mathf.Clamp01(_t / Seconds), s = 1f - (1f - u) * (1f - u);   // arranca rápido y frena al llegar abajo
        Vector3 a = room.slideTop.position, b = room.slideBottom.position;
        self.transform.position = Vector3.Lerp(a, b, s) + Vector3.up * 0.05f;
        Vector3 dir = b - a; dir.y = 0f;
        if (dir.sqrMagnitude > 1e-4f) self.transform.rotation = Quaternion.LookRotation(dir);
        if (u < 1f) { DailyPoses.Sit().ApplyFinal(self); return false; }
        DailyPoses.Stand().ApplyFinal(self);
        if (cc != null) cc.enabled = _ccWas;
        Slides++;
        return true;
    }
}
