using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// HUEVO de verdad (docs/ciclo-de-vida.md; Berón 2026-11-01): los ovíparos del santuario (<see cref="LifeHistory.eggs"/>) ya
/// no «gestan» el huevo dentro de la madre. Tras la cópula y su tiempo de formación (<see cref="Reproduction.Calendar"/>),
/// la hembra PONE: un objeto en el mundo que se incuba y del que nace la cría.
///   · PINGÜINO EMPERADOR: un huevo de ~12 cm sobre las PATAS de quien incuba (no hay nido de piedras). La hembra se lo pasa
///     al MACHO en cuanto él llega a su lado y se va al mar a comer (ha gastado sus reservas en formarlo); él lo incuba
///     2,1 meses AYUNANDO (incubar le gana al hambre: el deseo «incubate» de <see cref="DesireCatalog"/>). Si nadie lo cubre
///     se hiela (<see cref="LifeHistory.eggChillHours"/> horas de juego). Eclosiona donde esté.
///   · ARAÑA DE LAS NIEVES: un SACO de seda en su tela que la madre guarda; se desarrolla aunque ella se aparte (el frío no
///     lo hiela: los sacos de las arañas invernan).
/// La cría nace por <see cref="Reproduction.Hatch"/> (mismo camino que el parto: evento <see cref="Reproduction.Born"/>).
/// </summary>
public class Egg : MonoBehaviour
{
    public static readonly List<Egg> All = new List<Egg>();
    public static event System.Action<Egg> Laid, Transferred, Hatched, Lost;

    public string species;
    public Animal mother, father, incubator;
    public Reproduction mom;
    LifeHistory _life;
    /// <summary>Fracción incubada (0..1).</summary>
    public float progress;
    public float chillSeconds, attendedSeconds;
    public bool transferred;
    public bool Attended { get; private set; }
    public LifeHistory Life => _life;

    void OnEnable() => All.Add(this);
    void OnDisable() => All.Remove(this);

    /// <summary>La hembra pone: un huevo (pingüino) o un saco (araña) junto a ella.</summary>
    public static Egg Lay(Reproduction r, Animal mother, Animal father, LifeHistory life)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Object.Destroy(go.GetComponent<Collider>());   // no estorba a la navegación ni a los pies
        bool sac = !life.eggOnFeet;
        go.name = sac ? $"{mother.LifeSpecies}_saco_de_huevos" : $"{mother.LifeSpecies}_huevo";
        // Pingüino emperador: huevo de ~12 × 8 cm (≈450 g) [A]; saco de la araña de 30 cm: ~6 cm de seda blanca.
        go.transform.localScale = sac ? new Vector3(0.06f, 0.05f, 0.06f) : new Vector3(0.085f, 0.12f, 0.085f);
        Renderer rd = go.GetComponent<Renderer>();
        if (rd != null) rd.material.color = sac ? new Color(0.95f, 0.95f, 0.92f) : new Color(0.93f, 0.91f, 0.84f);
        Egg e = go.AddComponent<Egg>();
        e.species = mother.LifeSpecies; e.mother = mother; e.father = father; e.mom = r; e._life = life;
        e.incubator = mother;   // al principio, la madre (el emperador se lo pasa después al macho)
        go.transform.position = sac ? mother.transform.position + Vector3.down * 0.25f : Feet(mother);
        Debug.Log($"[Huevo] «{mother.name}» {(sac ? "teje un saco de huevos en su tela" : "pone un huevo")} (incubación {life.incubationMonths:0.0} meses).");
        Laid?.Invoke(e);
        return e;
    }

    static Vector3 Feet(Animal a) => a.transform.position + a.transform.forward * 0.12f + Vector3.up * 0.06f;

    /// <summary>¿Quiere <paramref name="a"/> incubar (o ir a por) un huevo? (deseo «incubate»).</summary>
    public static Egg ToIncubate(Animal a)
    {
        if (a == null) return null;
        foreach (Egg e in All)
        {
            if (e == null || e._life == null || !e._life.eggOnFeet) continue;   // el saco no se lleva encima: la araña sigue en su tela
            if (e.incubator == a) return e;
            if (e._life.incubator == LifeHistory.Incubator.Male && !e.transferred && e.father == a) return e;   // el macho va a por él
            if (e._life.incubator == LifeHistory.Incubator.Both && (e.father == a || e.mother == a) && (e.incubator == null || e.incubator.death)) return e;
        }
        return null;
    }

    void Update()
    {
        if (_life == null) { Destroy(gameObject); return; }
        TimeController tc = TimeController.timeController;
        float dt = Time.deltaTime;
        // Quien incuba murió: lo toma el otro progenitor (si vive).
        if (incubator == null || incubator.death)
        {
            Animal other = incubator == mother ? father : mother;
            incubator = other != null && !other.death ? other : null;
        }
        // Relevo del emperador: la hembra le pasa el huevo al macho en cuanto lo tiene al lado (y se va a comer al mar).
        if (_life.incubator == LifeHistory.Incubator.Male && !transferred && incubator == mother && father != null && !father.death
            && Flat(father.transform.position - transform.position) <= 1.5f)
        {
            incubator = father; transferred = true;
            if (mother != null) mother.hungry = Mathf.Max(mother.hungry, 0.8f);   // agotada: ha gastado sus reservas en el huevo
            Debug.Log($"[Huevo] «{mother?.name}» pasa el huevo a «{father.name}», que lo incubará sobre sus patas; ella se va al mar.");
            Transferred?.Invoke(this);
        }

        float reach = _life.eggOnFeet ? 1.0f : 1.6f;
        Attended = incubator != null && !incubator.death && Vector3.Distance(incubator.transform.position, transform.position) <= reach;
        if (Attended)
        {
            attendedSeconds += dt;
            if (_life.eggOnFeet) transform.position = Feet(incubator);   // va sobre sus patas, bajo el pliegue del vientre
        }

        float incubationSeconds = tc != null ? tc.SecondsForMonths(_life.incubationMonths) : 120f;
        if (_life.eggChillHours > 0f)
        {
            if (Attended) { progress += dt / Mathf.Max(0.01f, incubationSeconds); chillSeconds = 0f; }
            else
            {
                chillSeconds += dt;
                float limit = _life.eggChillHours * (tc != null ? tc.SecondsPerGameDay / 24f : 150f);
                if (chillSeconds > limit)
                {
                    Debug.Log($"[Huevo] El huevo de «{mother?.name}» se ha helado: nadie lo incubaba.");
                    Lost?.Invoke(this);
                    Destroy(gameObject);
                    return;
                }
            }
        }
        else progress += dt / Mathf.Max(0.01f, incubationSeconds);   // el saco se desarrolla solo; la madre lo guarda
        // El emperador: la hembra solo lo tiene las primeras horas; lo incuba el MACHO (si vive). Sin relevo no avanza más.
        if (_life.incubator == LifeHistory.Incubator.Male && !transferred && father != null && !father.death) progress = Mathf.Min(progress, 0.1f);

        // (si la madre murió, la cría sale igual: la «copia» la hace el padre)
        Reproduction hatcher = mother != null && !mother.death ? mom : father != null && !father.death ? father.GetComponent<Reproduction>() : null;
        if (progress >= 1f && hatcher != null)
        {
            Vector3 at = transform.position; at.y = (incubator != null ? incubator.transform.position.y : at.y);
            Animal baby = hatcher.Hatch(at, hatcher == mom ? father : null);
            Debug.Log($"[Huevo] Eclosiona el {(_life.eggOnFeet ? "huevo" : "saco")} de «{mother?.name}»: nace «{baby?.name}».");
            Hatched?.Invoke(this);
            Destroy(gameObject);
        }
    }

    static float Flat(Vector3 v) => new Vector2(v.x, v.z).magnitude;
}
