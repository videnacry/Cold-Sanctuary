using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EL ASCENSOR DEL TRONCO (docs/sitio-nuclear.md §23): sin alfombras en el Santuario Aéreo, Kushal sube del andén a los pisos, al techo y a la plataforma en esta cabina estanca.
/// Vive en la escena y nunca sale de ella: no es de la red de alfombras. Los trabajadores no cargan la escena: su viaje se cuenta como tiempo (<see cref="AereoRule.TripSeconds"/>).
/// </summary>
public class AereoLift : MonoBehaviour
{
    [Tooltip("El edificio de este ascensor.")] public string building;
    [Tooltip("Las paradas, de abajo arriba: la primera es el andén.")] public List<Transform> stops = new List<Transform>();
    [Tooltip("El nombre de cada parada, en el mismo orden.")] public List<string> labels = new List<string>();

    public bool Busy { get; private set; }
    /// <summary>La parada donde acaba de dejar a Kushal: no vuelve a preguntar hasta que salga de ella.</summary>
    public Transform ArrivedAt { get; private set; }
    public void ClearArrival(Transform stop) { if (ArrivedAt == stop) ArrivedAt = null; }

    /// <summary>Ofrece las demás paradas a <paramref name="p"/>, que está en <paramref name="from"/>.</summary>
    public void Ask(GameObject p, Transform from)
    {
        if (Busy || p == null) return;
        var options = new List<string>(); var to = new List<Transform>();
        for (int i = 0; i < stops.Count; i++)
            if (stops[i] != null && stops[i] != from) { to.Add(stops[i]); options.Add(i < labels.Count ? labels[i] : stops[i].name); }
        options.Add("Quedarse");
        ChoiceDialog.Show($"Ascensor de {building}", options.ToArray(), i => { if (i < to.Count) StartCoroutine(Ride(p, from, to[i])); }, null, from, 5f);
    }

    IEnumerator Ride(GameObject p, Transform from, Transform to)
    {
        Busy = true;
        PlayerController pc = p.GetComponent<PlayerController>();
        if (pc != null) pc.HoldBody(true);
        yield return AereoLink.Black();
        float seconds = AereoRule.LiftSeconds(Mathf.Abs(to.position.y - from.position.y)), w = 0f;
        while (w < seconds) { w += Time.unscaledDeltaTime; yield return null; }
        ArrivedAt = to;
        if (p != null) AereoLink.Put(p, to.position);
        if (pc != null) pc.HoldBody(false);
        ScreenFader.Get().FadeFromBlack();
        Busy = false;
    }

    /// <summary>(Pruebas) Deja a <paramref name="body"/> en la parada, sin pantalla en negro ni espera.</summary>
    public void PlaceForTest(GameObject body, Transform stop) { ArrivedAt = stop; AereoLink.Put(body, stop.position); }
}
