using UnityEngine;

/// <summary>
/// El PASEO guiado de onboarding (docs/kitchen-simulation.md §1). Un anfitrión recorre las
/// <see cref="TourStation"/> en orden, **señalando cada área y lo que se puede hacer** (el pensamiento de
/// "enseñar a los nuevos"), mientras el **novato lo acompaña** vía petición → alma compartida
/// (<see cref="HelpRequest"/>) o, si no hay, siguiéndolo directamente. Al terminar, deja lista la primera
/// tarea (limpieza). El anfitrión se mueve reutilizando un <see cref="FollowBrain"/> de alta relevancia
/// que se re-apunta a cada estación (el control lo elige por relevancia — docs anima §11.5).
/// </summary>
[RequireComponent(typeof(AnimaController))]
public class GuidedTour : MonoBehaviour
{
    [Tooltip("El novato al que se le hace el paseo (opcional).")]
    public AnimaController guest;
    public TourStation[] stations;
    public float arriveDistance = 1.5f;
    [Tooltip("Segundos parado 'explicando' cada estación.")]
    public float dwell = 2f;
    public bool startOnPlay = true;

    AnimaController _host;
    FollowBrain _walk;
    int _i = -1;
    float _dwellUntil;
    bool _announced, _done;

    void Start()
    {
        _host = GetComponent<AnimaController>();
        if (startOnPlay) StartTour();
    }

    public void StartTour()
    {
        if (stations == null || stations.Length == 0) { Debug.Log($"[Paseo] «{name}» sin estaciones."); return; }

        // El novato acompaña: petición → alma compartida (sigue al anfitrión). Si no hay HelpRequest, sigue directo.
        if (guest != null)
        {
            HelpRequest req = GetComponent<HelpRequest>();
            if (req != null) req.AskGoTogether(guest, transform, 999f);
            else
            {
                FollowBrain gf = guest.gameObject.AddComponent<FollowBrain>();
                gf.target = transform; gf.relevance = 5f; guest.RefreshBrains();
            }
        }

        // El anfitrión camina con un FollowBrain de alta relevancia re-apuntado por estación.
        _walk = gameObject.AddComponent<FollowBrain>();
        _walk.relevance = 5f;
        if (_host != null) _host.RefreshBrains();

        Debug.Log($"[Paseo] «{name}» empieza el paseo{(guest != null ? $" con «{guest.name}»" : "")} ({stations.Length} estaciones).");
        Advance();
    }

    void Advance()
    {
        _i++;
        if (_i >= stations.Length) { EndTour(); return; }
        _announced = false;
        if (_walk != null) _walk.target = stations[_i] != null ? stations[_i].transform : null;
    }

    void Update()
    {
        if (_done || _i < 0 || _i >= stations.Length) return;
        TourStation st = stations[_i];
        if (st == null) { Advance(); return; }

        if (!_announced)
        {
            if (Vector3.Distance(transform.position, st.transform.position) <= arriveDistance)
            {
                _announced = true;
                _dwellUntil = Time.time + dwell;
                Debug.Log($"[Paseo] «{name}» enseña «{st.stationName}»: {st.canDoHere}");
            }
        }
        else if (Time.time >= _dwellUntil)
        {
            Advance();
        }
    }

    void EndTour()
    {
        _done = true;
        if (_walk != null) Destroy(_walk);
        if (_host != null) _host.RefreshBrains();
        Debug.Log($"[Paseo] Fin del paseo. «{(guest != null ? guest.name : "el novato")}» ya conoce el área → primera tarea: limpiar.");
    }
}
