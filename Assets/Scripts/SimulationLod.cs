using UnityEngine;

/// <summary>
/// NIVEL DE DETALLE DE LA SIMULACIÓN (Berón 2026-11-09, rendimiento: el perfil de Buda a ×6 era lógica de scripts, 395 ms por fotograma, no
/// polígonos). Un sitio para decidir qué va «lejos»: a más de <see cref="FarDistance"/> m del jugador Y fuera de la vista de la cámara. Lo
/// lejano corre cada <see cref="FarInterval"/> s de juego (con algo de dispersión, para que no caigan todos en el mismo fotograma); los
/// relojes de cada conducta (Time.time) ponen al día lo que pasó entre medias, así que necesidades, horarios y llegadas siguen bien, solo
/// más bastos. Lo usan AnimaController (cerebros), VisualAttention (que lejos no mira) y TreeDoorExit.
/// </summary>
public static class SimulationLod
{
    public static bool Enabled = true;
    public const float FarDistance = 80f, FarInterval = 0.75f;

    static readonly Plane[] _planes = new Plane[6];
    static int _planesFrame = -1; static bool _havePlanes;

    /// <summary>¿Está lejos y fuera de la vista?</summary>
    public static bool IsFar(Vector3 p)
    {
        if (!Enabled) return false;
        Transform f = Lod.FocusNow;
        if (f == null || (p - f.position).sqrMagnitude <= FarDistance * FarDistance) return false;
        if (_planesFrame != Time.frameCount)
        {
            _planesFrame = Time.frameCount;
            Camera c = Camera.main;
            _havePlanes = c != null;
            if (_havePlanes) GeometryUtility.CalculateFrustumPlanes(c, _planes);
        }
        return !(_havePlanes && GeometryUtility.TestPlanesAABB(_planes, new Bounds(p, Vector3.one * 2f)));
    }

    /// <summary>La regla, sin escena: lo cercano corre siempre; lo lejano, una vez cada intervalo. true = este fotograma NO corre.</summary>
    public static bool SkipNow(bool far, float now, ref float next, float interval)
    {
        if (!far) { next = 0f; return false; }
        if (now < next) return true;
        next = now + interval;
        return false;
    }

    /// <summary>¿Se salta este fotograma quien está en <paramref name="p"/>? (<paramref name="next"/>: su próximo turno, lo guarda él)</summary>
    public static bool Skip(Vector3 p, ref float next) => SkipNow(IsFar(p), Time.time, ref next, FarInterval * Random.Range(0.8f, 1.2f));
}
