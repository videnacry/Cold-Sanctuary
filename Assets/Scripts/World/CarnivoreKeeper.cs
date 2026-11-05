using UnityEngine;

/// <summary>
/// GUARDIA DEL SANTUARIO: patrulla los puestos de vigía. Dar de comer a los depredadores ya no es cosa suya ni tiene sitio fijo (Berón 2026-11-05: «retira el
/// comedero»): es la misión del alimentador, que sigue a cada uno por el mapa (<see cref="PredatorFeeder"/>, docs/alimentador.md).
/// </summary>
public class CarnivoreKeeper : MonoBehaviour
{
    public float speed = 3.2f;
    Transform[] _posts;
    int _post;
    public string Doing => "patrulla";

    void Start()
    {
        var posts = new System.Collections.Generic.List<Transform>();
        foreach (Transform t in FindObjectsByType<Transform>(FindObjectsSortMode.None)) if (t.name.StartsWith("PuestoVigia_")) posts.Add(t);
        posts.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
        _posts = posts.ToArray();
        if (TryGetComponent(out IdleBehavior idle)) idle.enabled = false;   // su trabajo es patrullar, no merodear
    }

    void Update()
    {
        // Sus NECESIDADES van antes (baño, comer, dormir): mientras tanto no patrulla. Antes lo llevaba en alfombra al techo
        // estando en la cabina del aseo y, al salir, el aseo lo devolvía de golpe 60 m (JornadaTest 2026-10-29, Gohageneis).
        // (2026-11-04, ronda 6 del grupo 89: Gohageneis no llegaba al ring) En su TIEMPO LIBRE lo lleva el horario libre: la patrulla espera.
        if (LeisureDirector.Drives(gameObject)) return;
        if (TryGetComponent(out ActionQueue q) && q.Own.Count > 0 && (q.Own[0] is ToiletSpell || q.Own[0] is DineSpell || q.Own[0] is SleepSpell || q.Own[0] is EatSpell)) return;
        if (_posts == null || _posts.Length == 0) return;
        if (PathMover.Of(this).StepToward(_posts[_post].position, speed * 0.6f) < 1.5f) _post = (_post + 1) % _posts.Length;
    }
}
