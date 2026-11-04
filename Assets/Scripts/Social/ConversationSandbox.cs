using UnityEngine;

/// <summary>
/// LA SALA DE CHARLAS de la escena de prueba <c>Prueba_Conversaciones</c> (docs/conversaciones.md §Escena de prueba): la mesa con
/// sus bancos y las seis personas que charlan (dos humanos, dos insectos-humanos de especies distintas y una pareja mixta). Aquí no
/// hay casa, restaurante ni jornada: para que las charlas se vean sin que el cuerpo se los lleve, sus necesidades se mantienen bajas
/// (<see cref="sustainNeeds"/>) salvo cuando la prueba quiere una urgente; los humanos se quedan sentados en su banco.
/// </summary>
public class ConversationSandbox : MonoBehaviour
{
    [Tooltip("Las personas de la sala (las pone el constructor).")]
    public Anima[] cast = new Anima[0];
    [Tooltip("Mantener bajas el hambre, el baño, la suciedad y el sueño (la prueba lo apaga para la necesidad urgente).")]
    public bool sustainNeeds = true;
    [Tooltip("Los humanos, sentados en el banco (pose de sentado).")]
    public bool seated = true;

    float _next;

    void Update()
    {
        if (Time.time < _next) return;
        _next = Time.time + 1f;
        foreach (Anima a in cast)
        {
            if (a == null || a.death) continue;
            if (sustainNeeds)
            {
                BodyMetabolism m = a.GetComponent<BodyMetabolism>();
                if (m != null) { m.hunger = Mathf.Min(m.hunger, 0.3f); m.toilet = Mathf.Min(m.toilet, 0.3f); m.dirt = Mathf.Min(m.dirt, 0.3f); }
                a.sleepiness = Mathf.Min(a.sleepiness, 0.3f);
            }
            if (seated && !a.asleep) HomeMove.KeepSitting(a);
        }
    }
}
