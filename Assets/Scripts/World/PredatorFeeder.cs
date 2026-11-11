using System.Collections.Generic;
using UnityEngine;

// (En su propio archivo: Unity solo serializa en la escena un MonoBehaviour cuyo archivo se llama como la clase; si no, en Play queda «Missing Script».)
/// <summary>
/// EL ALIMENTADOR (misión de la jornada, por rotación: cualquiera; docs/alimentador.md): recoge raciones en el almacén del laboratorio, se reserva un
/// depredador de la lista compartida (<see cref="PredatorRound"/>), lo sigue hasta su distancia, evalúa, lanza o marca y sigue. Cada espera, acotada.
/// </summary>
public class PredatorFeeder : MonoBehaviour
{
    public float speed = 3.2f;
    public const int Carry = 24;
    /// <summary>Si en este tiempo no llega, lo devuelve a la cola y va a por otro: no es una salida, sigue pendiente.</summary>
    public const float RequeueAfter = 120f;

    enum Duty { Load, Seek, Track, Retreat, Done }
    Duty _duty = Duty.Load;
    PredatorRound.Entry _e;
    Vector3 _away;
    float _since;
    float _nextLook;
    bool _empty;
    public int Rations { get; private set; }
    public int fed { get; private set; }
    public int checkedOk { get; private set; }
    public string Doing => _duty == Duty.Track && _e != null && _e.animal != null ? $"sigue a «{_e.animal.name}»" : _duty == Duty.Load ? "recoge raciones" : _duty == Duty.Retreat ? "se retira" : "alimentador";

    void OnEnable() { _duty = Duty.Load; _e = null; Debug.Log($"[Alimentador] «{name}» empieza su turno: {PredatorRound.Left} depredadores por atender, {(PredatorRound.Store != null ? "almacén " + PredatorRound.Store.name : "sin almacén")}."); }   // (una línea por turno, también de los clones)
    void OnDisable()
    {
        if (_e != null && _e.state == PredatorRound.State.Claimed) { _e.state = PredatorRound.State.Pending; _e.by = null; }   // deja la reserva: otro lo atenderá
        _e = null;
    }

    void Update()
    {
        if (LeisureDirector.Drives(gameObject)) return;
        if (TryGetComponent(out ActionQueue q) && q.Own.Count > 0 && (q.Own[0] is ToiletSpell || q.Own[0] is DineSpell || q.Own[0] is SleepSpell || q.Own[0] is EatSpell)) return;
        switch (_duty)
        {
            case Duty.Load:
                Transform store = PredatorRound.Store;
                if (store == null || Rations >= Carry) { _duty = Duty.Seek; return; }
                if (PathMover.Of(this).StepToward(store.position, speed) < 1.8f)
                {
                    Rations += PredatorRound.TakeRations(Carry - Rations);
                    if (Rations == 0 && _empty)   // sin raciones y alguien con hambre: espera al próximo lote; avisa una vez por minuto
                    {
                        if (Time.time >= _nextLook) { _nextLook = Time.time + 60f; Debug.LogWarning($"[Alimentador] «{name}» espera raciones: el almacén está vacío y hay depredadores con hambre."); }
                        return;
                    }
                    _empty = false;
                    Debug.Log($"[Alimentador] «{name}» recoge {Rations} raciones.");
                    _duty = Duty.Seek;
                }
                break;
            case Duty.Seek:
                if (Time.time < _nextLook) return;
                _nextLook = Time.time + 2f;
                _e = PredatorRound.Claim(name, transform.position);
                if (_e == null) { _duty = Duty.Done; return; }
                _since = Time.time; _duty = Duty.Track;
                break;
            case Duty.Track:
                Animal a = _e != null ? _e.animal : null;
                if (a == null || a.death) { _e = null; _duty = Duty.Seek; return; }   // (la lista lo quita sola)
                float d = Flat(a.transform.position - transform.position), safe = PredatorRound.SafeDistance(a.SpeciesName);
                if (d < safe * 0.5f)   // se le ha acercado: no se le da de comer con él encima
                {
                    _away = PredatorRound.StandOff(a, transform.position); _since = Time.time; _duty = Duty.Retreat;
                    PredatorRound.Requeue(_e, "se ha acercado demasiado"); _e = null;
                    return;
                }
                if (d <= safe + 1.5f) { Serve(a); return; }
                PathMover.Of(this).StepToward(PredatorRound.StandOff(a, transform.position), speed);
                if (Time.time - _since > RequeueAfter) { PredatorRound.Requeue(_e, "no se le alcanza"); _e = null; _duty = Duty.Seek; }
                break;
            case Duty.Retreat:
                if (PathMover.Of(this).StepToward(_away, speed * 1.2f) < 1.5f || Time.time - _since > 15f) _duty = Duty.Seek;
                break;
            case Duty.Done:   // lista hecha: mira de vez en cuando si ya hay ronda nueva
                if (Time.time >= _nextLook) { _nextLook = Time.time + 20f; if (PredatorRound.Left > 0) _duty = Rations < 2 ? Duty.Load : Duty.Seek; }
                break;
        }
    }

    void Serve(Animal a)
    {
        if (!PredatorRound.ShouldFeed(a)) { PredatorRound.Report(_e, PredatorRound.State.Checked); checkedOk++; }
        else
        {
            int want = PredatorRound.RationsFor(a.SpeciesName), n = Mathf.Min(want, Rations);
            if (n > 0) { PredatorRound.Throw(a, n, transform.position); Rations -= n; fed++; PredatorRound.Report(_e, PredatorRound.State.Fed); }
            else { PredatorRound.Requeue(_e, "tiene hambre y no quedan raciones"); _empty = true; }   // nunca se da por hecho a uno con hambre
        }
        _e = null;
        _duty = Rations < 2 ? Duty.Load : Duty.Seek;
    }

    static float Flat(Vector3 v) => new Vector2(v.x, v.z).magnitude;
}
