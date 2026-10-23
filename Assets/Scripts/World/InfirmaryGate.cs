using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LA INICIACIÓN ES BLOQUEANTE (docs/jornada-y-rotacion.md). Quien acaba de llegar (Kushal, un turista) no está registrado
/// en el santuario: no puede quedarse fuera de la ENFERMERÍA hasta hacerse los análisis, vacunarse y completar la
/// iniciación con quien tenga hoy la misión de iniciación (NewcomerInitiator). Dentro se mueve libre.
/// Nadie bloquea la salida: si sale, LA MAGNATE se teletransporta delante de él, le explica que aún no puede irse y lo
/// devuelve dentro (<see cref="MagnateWarden"/>; antes lo perseguía un guardia y lo arrastraba, con movimientos raros).
/// </summary>
public class InfirmaryGate : MonoBehaviour
{
    [Tooltip("Punto de vuelta dentro de la enfermería, en coordenadas locales del área (junto a la entrada).")]
    public Vector3 backLocal = new Vector3(0f, 1.3f, 5.4f);
    float _next;

    /// <summary>¿Puede salir? Cuando ya está analizado, vacunado y certificado (o no es un recién llegado).</summary>
    public static bool Cleared(GameObject who)
    {
        if (who == null) return true;
        HealthRecord hr = who.GetComponent<HealthRecord>();
        SanctuaryOnboarding ob = who.GetComponent<SanctuaryOnboarding>();
        bool health = hr == null || !hr.NeedsCare;
        bool initiated = ob == null || ob.apt;
        return health && initiated;
    }

    SanctuaryArea _area;

    // Dentro = en cualquier parte del área, también en la entreplanta de las camas y en la planta de arriba.
    bool Inside(Vector3 p)
    {
        if (_area == null) _area = GetComponent<SanctuaryArea>();
        return _area == null || _area.Contains(p);
    }

    void Update()
    {
        if (ColdSanctuary.TestRunner.Running || Time.time < _next) return;
        _next = Time.time + 0.25f;
        foreach (SanctuaryOnboarding o in SceneCache.All<SanctuaryOnboarding>(0.5f))
        {
            if (o == null || Cleared(o.gameObject) || Inside(o.transform.position)) continue;
            MagnateWarden.Bring(o.gameObject, transform.TransformPoint(backLocal),
                "Aún no eres del santuario. Termina tu iniciación en la enfermería: análisis, vacuna y tu primera virtualización.");
        }
    }
}
