using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

/// <summary>
/// MUEBLES QUE EL NAVMESH NO VE: el NavMesh de la escena se horneó antes de colocar parte del mobiliario (las máquinas del
/// gimnasio, los bloques de la central nuclear, la grúa y la miniexcavadora de la obra…), así que el camino pasaba POR
/// DENTRO de ellos y la gente los atravesaba (JornadaTest 2026-10-29: Linduarda y Goluis por el «Bloque_Intercambiador»,
/// Panterilia por la prensa de piernas). Al empezar, a cada caja sólida con NavMesh dentro de su planta se le pone un
/// <see cref="NavMeshObstacle"/> que TALLA el NavMesh (solo una vez: están quietas). Son pocas (≈ 70 de 3500 cajas).
/// No toca los corrales y cuadras de los animales (sus agentes están dentro) ni los trineos. Las RAÍCES de los árboles sí
/// tallan (JornadaTest 2026-10-29: un oso y un pingüino se metían en la «Raiz» camino de su comida).
/// </summary>
public class StaleNavCarver : MonoBehaviour
{
    public static int Carved { get; private set; }

    /// <summary>(2026-11-14) Decorados que no se pisan y se tallan aunque el NavMesh ya los rodee (los montículos de tierra).</summary>
    static bool AlwaysCarve(string name) => name.StartsWith("Monticulo");
    static readonly string[] Skip = { "Cria_", "Cuadra", "Trineo", "Barrera_agua", "Inodoro", "Cabina", "Puerta", "Cama", "Silla", "Asiento" };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        if (FindAnyObjectByType<StaleNavCarver>() != null) return;
        new GameObject("StaleNavCarver").AddComponent<StaleNavCarver>();
    }

    IEnumerator Start()
    {
        yield return new WaitForSeconds(1f);   // (la escena termina de montarse: obras, gimnasio)
        int n = 0;
        foreach (BoxCollider c in FindObjectsByType<BoxCollider>(FindObjectsSortMode.None))
        {
            if (c == null || c.isTrigger || !c.enabled || !c.gameObject.activeInHierarchy) continue;
            if (c.GetComponentInParent<Anima>() != null || c.GetComponent<NavMeshObstacle>() != null) continue;
            if (c.attachedRigidbody != null && !c.attachedRigidbody.isKinematic) continue;
            Bounds b = c.bounds;
            // Suelos, losas, muros largos y el hielo (su cara de arriba es el suelo: max.y ≤ 0,35). Antes se descartaba todo lo
            // que bajara de −0,5 m, y con eso las RAÍCES grandes —inclinadas, se hunden en el hielo— no tallaban nunca: el
            // pingüino de (−134, 0, 18) seguía metiéndose en la «Raiz».
            if (b.size.y < 0.4f || b.size.x > 10f || b.size.z > 10f || b.max.y < 0.35f) continue;
            // (la PUERTA de un recinto de la Cría sí talla: está cerrada; sin hueco en el NavMesh, el recinto quedaba unido al
            // pasillo y a los demás recintos: las crías «alcanzaban» a las del recinto de al lado —huían, peleaban contra el
            // muro— y sus paseos apuntaban fuera)
            bool skip = false; foreach (string s in Skip) if (c.name.StartsWith(s) && !c.name.StartsWith("Puerta_recinto")) { skip = true; break; }
            if (skip) continue;
            // Lo que tiene su PROPIO NavMesh (el Árbol de los Campos, que gira con él) no talla el general.
            NavMeshSurface own = c.GetComponentInParent<NavMeshSurface>();
            if (own != null && own.collectObjects == CollectObjects.Children) continue;
            // (2026-11-14, ronda 110) LOS MONTÍCULOS SE TALLAN SIEMPRE, también por ENCIMA: el de la madriguera del zorro ártico ((−118,−118), dos
            // cajas de 1 y 1,45 m) tenía NavMesh en su cara de arriba, una isla a la que se llegaba por un Warp o un rescate (el punto del NavMesh
            // más cercano) y de la que no se salía; el zorro #68 «dentro de Monticulo» con los pies a 1,3 m, y el atasco de los zorros, renos y el
            // oso de (−118,−123), junto a la boca sur de la madriguera (rondas 102–103, 110). Su caja talla 0,6 m más alta para llevarse esa isla.
            bool always = AlwaysCarve(c.name);
            if (!always && !NavInside(b) && !NavThrough(c, b)) continue;
            NavMeshObstacle o = c.gameObject.AddComponent<NavMeshObstacle>();
            o.shape = NavMeshObstacleShape.Box;
            o.center = c.center; o.size = c.size;
            if (always) { float extra = 0.6f / Mathf.Max(0.01f, c.transform.lossyScale.y); o.size += new Vector3(0f, extra, 0f); o.center += new Vector3(0f, extra * 0.5f, 0f); }
            o.carving = true; o.carveOnlyStationary = true;
            n++;
        }
        // (2026-11-11) LA PARED DE LA RAÍZ de cada torre-árbol (cristal fino, MeshCollider): el NavMesh del hielo de alrededor no la veía y los
        // animales se metían en ella (rondas 26, 58 y 61: reno y zorro ártico «dentro de Pared_raiz» en (-59,0,-92), (-61,0,…)). Su caja talla el
        // NavMesh general: dentro está el agua del tronco, que no es para andar (los marinos entran por abajo, sin NavMesh). No en el árbol que gira.
        int walls = 0;
        foreach (MeshCollider mc in FindObjectsByType<MeshCollider>(FindObjectsSortMode.None))
        {
            if (mc == null || mc.name != "Pared_raiz" || mc.sharedMesh == null || mc.GetComponent<NavMeshObstacle>() != null) continue;
            // (2026-11-11, ronda 73) EL ÁRBOL QUE GIRA, CON SU CAJA, que gira con él y vuelve a tallar al moverse (carveOnlyStationary = false). La
            // cápsula quieta de eddf04d (radio 12,8 m) dejaba fuera las ESQUINAS de la pared cuadrada (a 15,6 m del eje): los 8 «dentro de Pared_raiz»
            // de vivo73 entraban a 12–15,5 m del centro. Y como se mueve, ya no hace falta apagarla y encenderla tras cada paso (65 «Failed to create agent»).
            bool spinning = mc.GetComponentInParent<TreeRotator>() != null;
            NavMeshObstacle o = mc.gameObject.AddComponent<NavMeshObstacle>();
            o.shape = NavMeshObstacleShape.Box;
            Bounds mb = mc.sharedMesh.bounds;
            // (ronda 72) A la altura del SUELO de verdad junto a la pared (no la base de la malla, que puede quedar bajo el hielo), de 1 m por debajo a 3 m
            // por encima: más alto tallaría el techo de alfombras del árbol, que está encima de la raíz.
            float gy = GroundBeside(mc.transform.TransformPoint(mb.center), mb.extents.x * Mathf.Abs(mc.transform.lossyScale.x), mc.bounds.min.y);
            float ly = mc.transform.InverseTransformPoint(new Vector3(0f, gy, 0f)).y;
            o.center = new Vector3(mb.center.x, ly + 1f, mb.center.z); o.size = new Vector3(mb.size.x, 4f, mb.size.z);
            o.carving = true; o.carveOnlyStationary = !spinning;
            if (spinning) Debug.Log($"[NavMesh] Obstáculo de la raíz que gira: caja de {o.size.x:0.0} × {o.size.z:0.0} m que gira con ella (talla al moverse).");
            walls++;
        }
        n += walls;
        Carved = n;
        Debug.Log($"[NavMesh] {n} muebles sin hueco en el NavMesh: ahora lo tallan (no se atraviesan).");
    }

    // ── (2026-11-11) DENTRO DE LA RAÍZ: cada 2 s, el animal de tierra que está dentro de la planta de la raíz de una torre-árbol (en su pared o
    // dentro) lo dice UNA vez con cuándo se le vio por primera vez y sus 3 últimas posiciones, y se le saca al punto del NavMesh más cercano
    // fuera de la pared (ronda 69: un zorro ártico «dentro de Pared_raiz» en (-45,0,-96), quieto, con el obstáculo de 754a752 puesto).
    float _nextRootCheck;
    readonly Dictionary<Animal, (float seen, List<Vector3> last, bool told)> _rootSeen = new Dictionary<Animal, (float, List<Vector3>, bool)>();
    public static int PulledOutOfRoots { get; private set; }

    void Update()
    {
        if (Time.time < _nextRootCheck) return;
        _nextRootCheck = Time.time + 2f;
        foreach (Animal a in FindObjectsByType<Animal>(FindObjectsSortMode.None))
        {
            if (a == null || a.death || a.nav == null || !a.nav.enabled || a.GetComponent<MarineLife>() != null) continue;
            if (!_rootSeen.TryGetValue(a, out var h)) h = (Time.time, new List<Vector3>(), false);
            h.last.Add(a.transform.position); if (h.last.Count > 3) h.last.RemoveAt(0);
            ArbolCampos t = RootAround(a.transform.position, out float half);
            if (t != null)
            {
                Vector3 l = t.transform.InverseTransformPoint(a.transform.position); Vector3 dir = new Vector3(l.x, 0f, l.z);
                if (dir.sqrMagnitude < 0.01f) dir = Vector3.forward;
                Vector3 outside = t.transform.TransformPoint(dir.normalized * (half * 1.45f + 2f)); outside.y = a.transform.position.y;
                if (!h.told) { h.told = true; Debug.Log($"[Fauna] dentro de Pared_raiz: «{a.name}» ({a.SpeciesName}) en la raíz de {t.building}, en {a.transform.position}; visto por primera vez a los {h.seen:0} s; últimas posiciones: {string.Join(" → ", h.last)}."); }
                if (NavMesh.SamplePosition(outside, out NavMeshHit nh, 6f, a.nav.areaMask) && RootAround(nh.position, out _) == null)
                {
                    a.nav.Warp(nh.position); PulledOutOfRoots++;
                    Debug.Log($"[Fauna] «{a.name}» sale de la raíz de {t.building}: a {nh.position} (fuera de la pared).");
                }
            }
            _rootSeen[a] = h;
        }
    }

    // La torre-árbol (que no gira; la que gira lleva su obstáculo redondo) en cuya planta de raíz cae p (a la altura del suelo).
    static ArbolCampos RootAround(Vector3 p, out float half)
    {
        half = 0f;
        foreach (ArbolCampos t in ArbolCampos.All)
        {
            if (t == null || t.wall == null || !t.wall.TryGetComponent(out MeshCollider mc) || mc.sharedMesh == null) continue;
            Vector3 l = t.wall.InverseTransformPoint(p); Bounds mb = mc.sharedMesh.bounds;
            if (l.y < mb.min.y - 1f || l.y > mb.min.y + 4f) continue;
            if (Mathf.Abs(l.x - mb.center.x) <= mb.extents.x + 0.3f && Mathf.Abs(l.z - mb.center.z) <= mb.extents.z + 0.3f) { half = mb.extents.x; return t; }
        }
        return null;
    }

    // La altura del NavMesh junto a la pared de una raíz (a 3 m de su cara, en las cuatro direcciones; el primero que se encuentre).
    static float GroundBeside(Vector3 center, float half, float fallback)
    {
        foreach (Vector3 d in new[] { Vector3.right, Vector3.left, Vector3.forward, Vector3.back })
        {
            Vector3 p = center + d * (half + 3f); p.y = fallback;
            for (float up = 0f; up <= 60f; up += 10f)
                if (NavMesh.SamplePosition(p + Vector3.up * up, out NavMeshHit h, 6f, NavMesh.AllAreas)) return h.position.y;
        }
        return fallback;
    }

    // ¿Hay NavMesh dentro de su planta, a la altura de su base? (en ≥ 2 de 9 puntos)
    static bool NavInside(Bounds b)
    {
        int inside = 0;
        for (float x = -0.3f; x <= 0.31f; x += 0.3f)
            for (float z = -0.3f; z <= 0.31f; z += 0.3f)
            {
                Vector3 p = new Vector3(b.center.x + x * b.size.x, b.min.y + 0.1f, b.center.z + z * b.size.z);
                if (NavMesh.SamplePosition(p, out NavMeshHit h, 0.2f, NavMesh.AllAreas) && h.position.y < b.max.y && h.position.y > b.min.y - 0.3f && ++inside >= 2) return true;
            }
        return false;
    }

    // ¿Pasa el NavMesh POR DENTRO del sólido a cualquier altura de su caja? (lo que se hunde en el suelo: la base queda bajo el
    // NavMesh y la prueba de arriba, a la altura de la base, no lo encontraba). Por cada punto de su planta busca el NavMesh en
    // su columna y mira si ese punto, a la altura de un pie, queda DENTRO del collider (no encima: su tapa transitable no cuenta).
    static bool NavThrough(BoxCollider c, Bounds b)
    {
        for (float x = -0.3f; x <= 0.31f; x += 0.3f)
            for (float z = -0.3f; z <= 0.31f; z += 0.3f)
            {
                Vector3 p = new Vector3(b.center.x + x * b.size.x, b.center.y, b.center.z + z * b.size.z);
                if (!NavMesh.SamplePosition(p, out NavMeshHit h, b.extents.y + 0.3f, NavMesh.AllAreas)) continue;
                if (Mathf.Abs(h.position.x - p.x) > 0.3f || Mathf.Abs(h.position.z - p.z) > 0.3f) continue;
                if (h.position.y > b.max.y - 0.25f) continue;
                Vector3 foot = h.position + Vector3.up * 0.1f;
                if ((c.ClosestPoint(foot) - foot).sqrMagnitude < 1e-6f) return true;   // (ClosestPoint de un punto interior = él mismo: el punto es preciso, basta uno)
            }
        return false;
    }
}
