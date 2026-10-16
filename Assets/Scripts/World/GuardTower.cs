using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TORRE DE GUARDIA (docs/spells-suprahechizo-and-thoughts.md §5b): el lugar del trabajo de guardián. Los vigías
/// SUBEN a la torre por su escalera y en lo alto, en el <see cref="GuardBoard"/>, obtienen sus misiones: patrullar el
/// santuario, alimentar animales, llevar heridos a la enfermería y enseñar yoga al rezagado (lo que la Magnate hace
/// sola). Construida con cubos: cuatro pilares de madera helada, riostras, plataforma con barandilla, tejado
/// escalonado, farol y bandera.
/// </summary>
public static class GuardTower
{
    public const float Height = 8f;

    public static Transform Build(Transform parent, Vector3 groundPos)
    {
        Transform t = new GameObject("TorreDeGuardia").transform;
        t.SetParent(parent, true);
        t.position = groundPos;
        Color wood = new Color(0.62f, 0.52f, 0.42f), woodDark = new Color(0.48f, 0.4f, 0.33f), roof = new Color(0.55f, 0.66f, 0.75f), ice = new Color(0.82f, 0.9f, 0.95f);
        float half = 2.4f;

        for (int sx = -1; sx <= 1; sx += 2)
            for (int sz = -1; sz <= 1; sz += 2)
                Block(t, new Vector3(sx * half, Height * 0.5f, sz * half), new Vector3(0.45f, Height, 0.45f), wood, true, "Pilar");
        for (int lv = 1; lv <= 3; lv++)   // riostras horizontales en cada lado
        {
            float y = Height * lv / 4f;
            for (int s = -1; s <= 1; s += 2)
            {
                Block(t, new Vector3(0f, y, s * half), new Vector3(half * 2f, 0.2f, 0.2f), woodDark, false, "Riostra");
                Block(t, new Vector3(s * half, y, 0f), new Vector3(0.2f, 0.2f, half * 2f), woodDark, false, "Riostra");
            }
        }
        // Plataforma con barandilla (hueco al sur, donde llega la escalera).
        Block(t, new Vector3(0f, Height, 0f), new Vector3(half * 2f + 0.8f, 0.25f, half * 2f + 0.8f), woodDark, true, "Plataforma");
        float rh = Height + 0.6f, edge = half + 0.35f;
        Block(t, new Vector3(0f, rh, edge), new Vector3(edge * 2f, 0.1f, 0.1f), wood, true, "Barandilla");
        Block(t, new Vector3(-edge, rh, 0f), new Vector3(0.1f, 0.1f, edge * 2f), wood, true, "Barandilla");
        Block(t, new Vector3(edge, rh, 0f), new Vector3(0.1f, 0.1f, edge * 2f), wood, true, "Barandilla");
        Block(t, new Vector3(-edge * 0.6f, rh, -edge), new Vector3(edge * 0.8f, 0.1f, 0.1f), wood, true, "Barandilla");
        Block(t, new Vector3(edge * 0.6f, rh, -edge), new Vector3(edge * 0.8f, 0.1f, 0.1f), wood, true, "Barandilla");
        for (int i = 0; i < 8; i++)
        {
            float a = i * 45f * Mathf.Deg2Rad;
            Vector3 p = new Vector3(Mathf.Clamp(Mathf.Cos(a) * edge * 1.42f, -edge, edge), Height + 0.3f, Mathf.Clamp(Mathf.Sin(a) * edge * 1.42f, -edge, edge));
            if (p.z < -edge + 0.01f && Mathf.Abs(p.x) < 0.8f) continue;   // hueco de la escalera
            Block(t, p, new Vector3(0.1f, 0.6f, 0.1f), wood, false, "Balaustre");
        }
        // Tejado: postes y una pirámide escalonada de bloques de hielo.
        for (int sx = -1; sx <= 1; sx += 2)
            for (int sz = -1; sz <= 1; sz += 2)
                Block(t, new Vector3(sx * (half - 0.1f), Height + 1.4f, sz * (half - 0.1f)), new Vector3(0.2f, 2.8f, 0.2f), wood, false, "Poste");
        for (int k = 0; k < 4; k++)
            Block(t, new Vector3(0f, Height + 2.9f + k * 0.35f, 0f), new Vector3(half * 2.4f - k * 1.2f, 0.35f, half * 2.4f - k * 1.2f), k % 2 == 0 ? roof : ice, false, "Tejado");
        Block(t, new Vector3(0f, Height + 5f, 0f), new Vector3(0.06f, 1.6f, 0.06f), woodDark, false, "Mastil");
        Block(t, new Vector3(0.4f, Height + 5.5f, 0f), new Vector3(0.7f, 0.45f, 0.03f), new Color(0.75f, 0.65f, 0.88f), false, "Bandera");
        Block(t, new Vector3(half - 0.3f, Height + 2.2f, half - 0.3f), new Vector3(0.25f, 0.35f, 0.25f), new Color(1f, 0.85f, 0.45f), false, "Farol");

        // Escalera recta por el sur: peldaños de 0,25 m de alto (se suben andando).
        int steps = Mathf.CeilToInt(Height / 0.25f);
        for (int i = 0; i < steps; i++)
        {
            float y = (i + 1) * 0.25f;
            float z = -half - 0.6f - (steps - 1 - i) * 0.32f;
            Block(t, new Vector3(0f, y - 0.125f, z), new Vector3(1.4f, 0.25f, 0.34f), i % 2 == 0 ? wood : woodDark, true, "Peldano");
            if (i % 4 == 0)
                for (int s = -1; s <= 1; s += 2) Block(t, new Vector3(s * 0.75f, y * 0.5f, z), new Vector3(0.12f, y, 0.12f), woodDark, false, "Zanca");
        }

        // Tablero de misiones de guardia en lo alto.
        Transform board = Block(t, new Vector3(0f, Height + 0.95f, half - 0.3f), new Vector3(1.4f, 1f, 0.12f), new Color(0.86f, 0.78f, 0.62f), true, "Tablero de guardia");
        board.gameObject.AddComponent<GuardBoard>();
        WorldLabel wl = board.gameObject.AddComponent<WorldLabel>(); wl.text = "Tablero de guardia"; wl.height = 0.9f;
        WorldLabel tl = t.gameObject.AddComponent<WorldLabel>(); tl.text = "Torre de guardia"; tl.height = Height + 6.5f;
        return t;
    }

    static Transform Block(Transform parent, Vector3 local, Vector3 size, Color c, bool solid, string name)
    {
        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = name;
        if (!solid) { Collider col = g.GetComponent<Collider>(); if (Application.isPlaying) Object.Destroy(col); else Object.DestroyImmediate(col); }
        g.transform.SetParent(parent, false);
        g.transform.localPosition = local;
        g.transform.localScale = size;
        g.GetComponent<Renderer>().sharedMaterial = KitchenProps.Mat(c);
        return g.transform;
    }
}

/// <summary>
/// TABLERO DE GUARDIA (F en lo alto de la torre): lista las misiones de vigía. «Patrullar» ya se juega: recorrer los
/// puestos de vigía del santuario (marcados con un poste y farol) con un indicador de dirección. Las demás se listan con
/// lo que piden; la Magnate ya hace de oficio «enseñar al rezagado».
/// </summary>
public class GuardBoard : MonoBehaviour, IInteractable
{
    public string InteractLabel => "Ver misiones de guardia";
    public bool CanInteract => true;

    bool _open;
    PatrolMission _patrol;

    static readonly string[] Missions =
    {
        "1  Patrullar: recorre los puestos de vigía del santuario.",
        "2  Alimentar animales: lleva comida de la barra de servicio a los animales del bosque (próximamente).",
        "3  Llevar heridos a la enfermería: acompaña al que esté enfermo hasta Linduarda (próximamente).",
        "4  Enseñar al rezagado: la Magnate lo hace sola; acompáñala cuando enseñe yoga.",
    };

    public void Interact() { _open = !_open; }

    void Update()
    {
        if (!_open) return;
        if (Input.GetKeyDown(KeyCode.Escape)) _open = false;
        if (Input.GetKeyDown(KeyCode.Alpha1)) { StartPatrol(); _open = false; }
    }

    /// <summary>Arranca la patrulla (también desde la consola de hechizos o los tests).</summary>
    public PatrolMission StartPatrol()
    {
        if (_patrol == null) { if (!TryGetComponent(out _patrol)) _patrol = gameObject.AddComponent<PatrolMission>(); }
        _patrol.Begin();
        return _patrol;
    }

    void OnGUI()
    {
        if (!_open) return;
        GUI.Box(new Rect(Screen.width * 0.5f - 300f, 80f, 600f, 170f), "<b>Misiones de guardia</b>  (número para elegir · Esc cierra)");
        var st = new GUIStyle(GUI.skin.label) { richText = true, wordWrap = true };
        for (int i = 0; i < Missions.Length; i++) GUI.Label(new Rect(Screen.width * 0.5f - 285f, 110f + i * 32f, 570f, 30f), Missions[i], st);
    }
}

/// <summary>PATRULLA: visitar en orden los puestos de vigía (<c>PuestoVigia_*</c>); al llegar a cada uno, el siguiente.
/// Recompensa: XP de Stats y un poco de observación. HUD con la flecha al siguiente puesto.</summary>
public class PatrolMission : MonoBehaviour
{
    public float reachRadius = 4f;
    readonly List<Transform> _posts = new List<Transform>();
    int _next = -1;
    Transform _player;

    public bool Running => _next >= 0;
    public int Visited => Mathf.Max(0, _next);
    public int Total => _posts.Count;

    public void Begin()
    {
        _posts.Clear();
        foreach (Transform t in FindObjectsByType<Transform>(FindObjectsSortMode.None))
            if (t.name.StartsWith("PuestoVigia_")) _posts.Add(t);
        _posts.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        _player = p != null ? p.transform : null;
        _next = _posts.Count > 0 ? 0 : -1;
        Debug.Log(_posts.Count > 0 ? $"[Guardia] Patrulla: recorre {_posts.Count} puestos de vigía." : "[Guardia] No hay puestos de vigía en esta escena.");
    }

    void Update()
    {
        if (_next < 0 || _player == null || _next >= _posts.Count) return;
        Vector3 d = _posts[_next].position - _player.position; d.y = 0f;
        if (d.magnitude <= reachRadius) Reach();
    }

    /// <summary>Marca el puesto actual como visitado (lo usa Update al llegar, y los tests).</summary>
    public void Reach()
    {
        if (_next < 0 || _next >= _posts.Count) return;
        Debug.Log($"[Guardia] Puesto {_next + 1}/{_posts.Count} vigilado ({_posts[_next].name.Replace("PuestoVigia_", "")}).");
        _next++;
        if (_next >= _posts.Count)
        {
            Debug.Log("[Guardia] ✅ Patrulla completa: el santuario está en calma.");
            if (_player != null) { CharacterLevel cl = _player.GetComponent<CharacterLevel>(); if (cl != null) cl.GainStatsXp(15f); }
            _next = -1;
        }
    }

    void OnGUI()
    {
        if (_next < 0 || _player == null || _next >= _posts.Count) return;
        string arrow = HudGate.Arrow(_player.position, _posts[_next].position);
        float dist = Vector3.Distance(_player.position, _posts[_next].position);
        GUI.Box(new Rect(Screen.width * 0.5f - 230f, 40f, 460f, 28f), $"Patrulla {_next}/{_posts.Count} · siguiente: {_posts[_next].name.Replace("PuestoVigia_", "")} {arrow} {dist:0} m");
    }
}
