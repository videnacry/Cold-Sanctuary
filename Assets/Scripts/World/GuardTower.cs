using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CASA DEL ÁRBOL DE GUARDIA (docs/spells-suprahechizo-and-thoughts.md §5b): el puesto de los vigías es una cabaña en lo
/// alto de uno de los árboles de hielo del santuario, pero ×2,5 de grande (40 m) y que NO caza. Se sube por una ESCALERA
/// VERTICAL de peldaños (F al pie: subir; F arriba: bajar). Arriba, una plataforma con barandilla rodea el tronco y la
/// cabaña guarda el <see cref="GuardBoard"/> con las misiones de vigía. Todo de cubos.
/// </summary>
public static class GuardTower
{
    public const float TreeHeight = 40f;
    public const float DeckHeight = 11f;   // bajo el primer piso de ramas (≈15 m): la cabaña no queda atravesada

    public static Transform Build(Transform parent, Vector3 groundPos)
    {
        GameObject treeGO = CubeTree.Build(parent, "CasaArbolGuardia", groundPos, 4242, TreeHeight, carnivorous: false);
        Transform t = treeGO.transform;
        Color wood = new Color(0.62f, 0.52f, 0.42f), woodDark = new Color(0.48f, 0.4f, 0.33f), roof = new Color(0.55f, 0.66f, 0.75f),
              glass = new Color(0.75f, 0.88f, 0.95f), lamp = new Color(1f, 0.85f, 0.45f);
        float y = DeckHeight, half = 4.6f;

        // Plataforma alrededor del tronco, con vigas de apoyo hacia el tronco.
        Block(t, new Vector3(0f, y, 0f), new Vector3(half * 2f, 0.3f, half * 2f), woodDark, true, "Plataforma");
        for (int i = 0; i < 4; i++)
        {
            Quaternion q = Quaternion.Euler(0f, i * 90f + 45f, 0f);
            Transform beam = Block(t, new Vector3(0f, y - 1.6f, 0f) + q * Vector3.forward * half * 0.55f, new Vector3(0.35f, 0.35f, half * 1.3f), wood, false, "Viga");
            beam.localRotation = q * Quaternion.Euler(-35f, 0f, 0f);
        }
        // Barandilla con hueco donde llega la escalera (lado sur, -Z).
        float rh = y + 0.75f;
        Block(t, new Vector3(0f, rh, half), new Vector3(half * 2f, 0.1f, 0.12f), wood, true, "Barandilla");
        Block(t, new Vector3(-half, rh, 0f), new Vector3(0.12f, 0.1f, half * 2f), wood, true, "Barandilla");
        Block(t, new Vector3(half, rh, 0f), new Vector3(0.12f, 0.1f, half * 2f), wood, true, "Barandilla");
        Block(t, new Vector3(-half * 0.6f, rh, -half), new Vector3(half * 0.8f, 0.1f, 0.12f), wood, true, "Barandilla");
        Block(t, new Vector3(half * 0.6f, rh, -half), new Vector3(half * 0.8f, 0.1f, 0.12f), wood, true, "Barandilla");
        for (float p = -half; p <= half + 0.01f; p += 1.15f)
        {
            Block(t, new Vector3(p, y + 0.45f, half), new Vector3(0.09f, 0.6f, 0.09f), wood, false, "Balaustre");
            Block(t, new Vector3(-half, y + 0.45f, p), new Vector3(0.09f, 0.6f, 0.09f), wood, false, "Balaustre");
            Block(t, new Vector3(half, y + 0.45f, p), new Vector3(0.09f, 0.6f, 0.09f), wood, false, "Balaustre");
            if (Mathf.Abs(p) > 0.9f) Block(t, new Vector3(p, y + 0.45f, -half), new Vector3(0.09f, 0.6f, 0.09f), wood, false, "Balaustre");
        }

        // Cabaña al norte del tronco: paredes de tablones, puerta, ventanas y tejado escalonado.
        Vector3 c = new Vector3(0f, y, 2.6f);
        float cw = 6f, cd = 3.2f, ch = 2.6f;
        Block(t, c + new Vector3(0f, ch * 0.5f, cd * 0.5f), new Vector3(cw, ch, 0.15f), wood, true, "Pared_Norte");
        Block(t, c + new Vector3(-cw * 0.5f, ch * 0.5f, 0f), new Vector3(0.15f, ch, cd), wood, true, "Pared_Oeste");
        Block(t, c + new Vector3(cw * 0.5f, ch * 0.5f, 0f), new Vector3(0.15f, ch, cd), wood, true, "Pared_Este");
        Block(t, c + new Vector3(-2.1f, ch * 0.5f, -cd * 0.5f), new Vector3(1.8f, ch, 0.15f), wood, true, "Pared_Sur");
        Block(t, c + new Vector3(2.1f, ch * 0.5f, -cd * 0.5f), new Vector3(1.8f, ch, 0.15f), wood, true, "Pared_Sur");
        Block(t, c + new Vector3(0f, ch - 0.3f, -cd * 0.5f), new Vector3(2.4f, 0.6f, 0.15f), wood, true, "Dintel");
        for (int s = -1; s <= 1; s += 2)
            Block(t, c + new Vector3(s * cw * 0.5f, ch * 0.55f, 0f), new Vector3(0.17f, 0.7f, 1f), glass, false, "Ventana");
        for (int k = 0; k < 4; k++)
            Block(t, c + new Vector3(0f, ch + 0.15f + k * 0.3f, 0f), new Vector3(cw + 0.8f - k * 1.5f, 0.3f, cd + 0.8f - k * 0.7f), k % 2 == 0 ? roof : woodDark, false, "Tejado");
        Block(t, c + new Vector3(cw * 0.5f - 0.4f, ch - 0.4f, -cd * 0.5f - 0.2f), new Vector3(0.25f, 0.35f, 0.25f), lamp, false, "Farol");
        Block(t, c + new Vector3(0f, ch + 2.2f, 0f), new Vector3(0.06f, 1.6f, 0.06f), woodDark, false, "Mastil");
        Block(t, c + new Vector3(0.4f, ch + 2.7f, 0f), new Vector3(0.7f, 0.45f, 0.03f), new Color(0.75f, 0.65f, 0.88f), false, "Bandera");

        // Tablero de misiones dentro de la cabaña, contra la pared norte.
        Transform board = Block(t, c + new Vector3(0f, 1.3f, cd * 0.5f - 0.15f), new Vector3(1.6f, 1.1f, 0.08f), new Color(0.86f, 0.78f, 0.62f), true, "Tablero de guardia");
        board.gameObject.AddComponent<GuardBoard>();
        WorldLabel wl = board.gameObject.AddComponent<WorldLabel>(); wl.text = "Tablero de guardia"; wl.height = 0.9f;

        // ESCALERA VERTICAL del suelo a la plataforma (lado sur): dos largueros y peldaños cada 35 cm.
        float lz = -half - 0.35f;
        Transform ladder = new GameObject("Escalera").transform; ladder.SetParent(t, false); ladder.localPosition = new Vector3(0f, 0f, lz);
        for (int s = -1; s <= 1; s += 2) Block(ladder, new Vector3(s * 0.32f, (y + 1f) * 0.5f, 0f), new Vector3(0.09f, y + 1f, 0.09f), woodDark, false, "Larguero");
        for (float h = 0.35f; h < y + 0.9f; h += 0.35f) Block(ladder, new Vector3(0f, h, 0f), new Vector3(0.64f, 0.05f, 0.06f), wood, false, "Peldano");
        Ladder bottom = Block(ladder, new Vector3(0f, 1f, -0.6f), new Vector3(1.4f, 2f, 1.2f), Color.white, true, "Pie de la escalera").gameObject.AddComponent<Ladder>();
        bottom.GetComponent<Collider>().isTrigger = true; bottom.GetComponent<Renderer>().enabled = false;
        bottom.top = false; bottom.ladder = ladder; bottom.deckY = y;
        Ladder top = Block(ladder, new Vector3(0f, y + 1f, 0.9f), new Vector3(1.4f, 2f, 1.2f), Color.white, true, "Lo alto de la escalera").gameObject.AddComponent<Ladder>();
        top.GetComponent<Collider>().isTrigger = true; top.GetComponent<Renderer>().enabled = false;
        top.top = true; top.ladder = ladder; top.deckY = y;

        WorldLabel tl = t.gameObject.AddComponent<WorldLabel>(); tl.text = "Casa del árbol de la guardia"; tl.height = y + 5f;
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
/// ESCALERA VERTICAL: F al pie = SUBIR, F arriba = BAJAR. Mientras sube, el cuerpo queda pegado a la escalera (sin
/// gravedad ni control) y brazos y piernas alternan peldaño a peldaño. Al llegar, deja al jugador en la plataforma (o en
/// el suelo). `ClimbAnima` también la usa la IA/los tests.
/// </summary>
public class Ladder : MonoBehaviour, IInteractable
{
    public bool top;
    public Transform ladder;
    public float deckY = 11f;
    public float speed = 1.6f;   // m/s
    static bool _busy;

    public string InteractLabel => top ? "Bajar por la escalera" : "Subir a la casa del árbol";
    public bool CanInteract => !_busy;

    public void Interact()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null && !_busy) StartCoroutine(Climb(p.transform, !top));
    }

    /// <summary>Sube (up) o baja al <paramref name="who"/> por la escalera.</summary>
    public IEnumerator Climb(Transform who, bool up)
    {
        _busy = true;
        CharacterController cc = who.GetComponent<CharacterController>();
        var controllers = new List<Behaviour>();
        foreach (MonoBehaviour mb in who.GetComponents<MonoBehaviour>())
            if (mb != null && mb.enabled && (mb is PlayerController)) { controllers.Add(mb); mb.enabled = false; }
        if (cc != null) cc.enabled = false;
        CreatureRig rig = who.GetComponent<CreatureRig>();

        Vector3 baseP = ladder.position - ladder.forward * 0.45f;            // pegado a la escalera, por fuera
        float y0 = up ? 0.05f : deckY + 0.1f, y1 = up ? deckY + 0.1f : 0.05f;
        who.rotation = Quaternion.LookRotation(ladder.forward);
        float yy = y0, phase = 0f;
        Debug.Log($"[Guardia] «{who.name}» {(up ? "sube" : "baja")} por la escalera de la casa del árbol.");
        while (up ? yy < y1 : yy > y1)
        {
            yy += (up ? 1f : -1f) * speed * Time.deltaTime;
            phase += speed * Time.deltaTime / 0.35f * Mathf.PI;
            who.position = new Vector3(baseP.x, ladder.position.y + yy + 1f, baseP.z);   // +1: el pivote del jugador va en su centro
            if (rig != null) Limbs(rig, phase);
            yield return null;
        }
        // Al llegar arriba, un paso hacia la plataforma; abajo, un paso atrás.
        who.position = up ? ladder.position + ladder.forward * 1.4f + Vector3.up * (deckY + 1.2f) : baseP - ladder.forward * 0.8f + Vector3.up * 1.1f;
        if (cc != null) cc.enabled = true;
        foreach (Behaviour b in controllers) b.enabled = true;
        Debug.Log($"[Guardia] «{who.name}» llega {(up ? "a la casa del árbol" : "al suelo")}.");
        _busy = false;
    }

    static void Limbs(CreatureRig rig, float phase)
    {
        float s = Mathf.Sin(phase);
        Set(rig, BodyPart.ShoulderLeft, -150f + 25f * s); Set(rig, BodyPart.ShoulderRight, -150f - 25f * s);
        Set(rig, BodyPart.ThighLeft, -40f - 30f * s);     Set(rig, BodyPart.ThighRight, -40f + 30f * s);
        Set(rig, BodyPart.ShinLeft, 60f + 20f * s);       Set(rig, BodyPart.ShinRight, 60f - 20f * s);
    }

    static void Set(CreatureRig rig, BodyPart p, float x)
    {
        Transform t = rig.Get(p);
        if (t == null) return;
        rig.LockPose(p, 0.2f);
        t.localRotation = Quaternion.Euler(x, 0f, 0f);
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
