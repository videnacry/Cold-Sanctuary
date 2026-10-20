using System.Collections;
using UnityEngine;

/// <summary>
/// TANQUE DE VIRTUALIZACIÓN = una PECERA cuadrada y transparente, llena de agua, apoyada en el suelo (sobre un zócalo bajo).
/// Se entra DE FRENTE: la pared delantera es una PUERTA de cristal de dos hojas que se deslizan a los lados. Al abrirla,
/// un CAMPO MAGNÉTICO en el marco de la puerta sostiene el agua como una pared vertical (el agua es DIAMAGNÉTICA: los
/// campos muy intensos la repelen, el "efecto Moisés"), así que no se sale aunque la puerta esté abierta; el personaje
/// la atraviesa y queda sumergido dentro.
/// Física real, para que se sepa qué parte es magia: sostener una pared de agua de ~2,4 m necesita aguantar ~24 kPa en la
/// base; con la susceptibilidad del agua (χ ≈ −9·10⁻⁶) eso pide un campo de unos 75–80 T, casi el doble del imán continuo
/// más fuerte que existe (~45 T). En el santuario lo hacen las bobinas del marco (tecnología mágica de la Magnate).
/// </summary>
public class VirtualizationTank : MonoBehaviour
{
    public float radius = 0.9f;       // medio lado de la pecera
    public float height = 2.4f;       // alto del agua
    public const float Base = 0.25f;  // zócalo: el suelo de la pecera queda a esta altura
    public Transform front;           // dónde se pone el personaje antes de entrar (delante de la puerta)
    [SerializeField] Transform _doorA, _doorB, _field;
    static bool _busy;

    public static VirtualizationTank Build(Transform machine)
    {
        Color glass = new Color(0.82f, 0.94f, 1f, 0.18f), water = new Color(0.25f, 0.62f, 0.88f, 0.55f),
              metal = KitchenProps.Steel, dark = KitchenProps.DarkSteel;
        Color coil = new Color(0.72f, 0.45f, 0.22f), glow = new Color(0.55f, 0.85f, 1f, 0.35f);
        Transform t = new GameObject("Tanque").transform;
        t.SetParent(machine, false);
        t.localPosition = new Vector3(0f, -1f, 0f);   // la máquina está a 1 m; el tanque apoya en el suelo
        VirtualizationTank tank = t.gameObject.AddComponent<VirtualizationTank>();
        float r = tank.radius, h = tank.height, y0 = Base;

        KitchenProps.C(t, new Vector3(0f, y0 * 0.5f, 0f), new Vector3(r * 2.2f, y0, r * 2.2f), dark, 0f, "Zocalo");
        // Paredes de cristal: atrás y laterales; delante, el MARCO de la puerta.
        for (int i = 1; i < 4; i++)   // i = 0 sería la de delante (−Z): ahí va la puerta
        {
            Quaternion q = Quaternion.Euler(0f, 180f + i * 90f, 0f);
            Transform p = KitchenProps.C(t, q * new Vector3(0f, y0 + h * 0.5f, r), new Vector3(r * 2f + 0.04f, h, 0.03f), glass, 0f, "Cristal");
            p.localRotation = q;
        }
        for (int sx = -1; sx <= 1; sx += 2)
            for (int sz = -1; sz <= 1; sz += 2)
                KitchenProps.C(t, new Vector3(sx * r, y0 + h * 0.5f, sz * r), new Vector3(0.06f, h, 0.06f), metal, 0f, "Arista");
        KitchenProps.C(t, new Vector3(0f, y0 + h + 0.04f, 0f), new Vector3(r * 2.1f, 0.08f, r * 2.1f), dark, 0f, "Tapa");
        KitchenProps.C(t, new Vector3(0f, y0 + h * 0.5f, 0f), new Vector3(r * 1.96f, h - 0.02f, r * 1.96f), water, 0f, "Agua");

        // MARCO DE LA PUERTA con las bobinas (cobre) que crean el campo, y el campo mismo: una lámina de luz que se enciende
        // cuando la puerta está abierta (es lo que ve el personaje al cruzar: el agua quieta como una pared).
        float door = 0.55f;   // media anchura del hueco
        for (int s = -1; s <= 1; s += 2)
            KitchenProps.C(t, new Vector3(s * (door + 0.06f), y0 + h * 0.5f, -r - 0.02f), new Vector3(0.1f, h, 0.1f), coil, 0f, "Bobina");
        KitchenProps.C(t, new Vector3(0f, y0 + h - 0.05f, -r - 0.02f), new Vector3(door * 2f + 0.2f, 0.1f, 0.1f), coil, 0f, "Bobina");
        for (int s = -1; s <= 1; s += 2)   // cristal fijo a los lados del hueco
            KitchenProps.C(t, new Vector3(s * (door + 0.11f + (r - door - 0.11f) * 0.5f), y0 + h * 0.5f, -r), new Vector3(r - door - 0.11f, h, 0.03f), glass, 0f, "Cristal");
        tank._field = KitchenProps.C(t, new Vector3(0f, y0 + (h - 0.1f) * 0.5f, -r + 0.01f), new Vector3(door * 2f, h - 0.1f, 0.01f), glow, 0f, "Campo_magnetico");
        tank._field.gameObject.SetActive(false);
        // PUERTA: dos hojas de cristal que se deslizan hacia los lados (por fuera, delante del marco).
        tank._doorA = KitchenProps.C(t, new Vector3(-door * 0.5f, y0 + (h - 0.1f) * 0.5f, -r - 0.07f), new Vector3(door, h - 0.1f, 0.03f), glass, 0f, "Puerta");
        tank._doorB = KitchenProps.C(t, new Vector3(door * 0.5f, y0 + (h - 0.1f) * 0.5f, -r - 0.07f), new Vector3(door, h - 0.1f, 0.03f), glass, 0f, "Puerta");
        for (int s = -1; s <= 1; s += 2)   // tiradores
            KitchenProps.C(s < 0 ? tank._doorA : tank._doorB, new Vector3(-s * 0.4f, 0f, -0.8f), new Vector3(0.06f, 0.18f, 0.8f), metal, 0f, "Tirador");

        // Máscara colgada con su tubo, y consola.
        KitchenProps.C(t, new Vector3(r + 0.25f, 1.5f, -r), new Vector3(0.18f, 0.14f, 0.1f), new Color(0.3f, 0.32f, 0.36f), 0f, "Mascara");
        KitchenProps.C(t, new Vector3(r + 0.25f, 0.95f, -r), new Vector3(0.04f, 0.9f, 0.04f), new Color(0.55f, 0.7f, 0.8f), 0f, "Tubo");
        KitchenProps.C(t, new Vector3(-r - 0.6f, 0.6f, -0.3f), new Vector3(0.5f, 1.2f, 0.4f), KitchenProps.White, 0f, "Consola");
        KitchenProps.C(t, new Vector3(-r - 0.6f, 1.05f, -0.51f), new Vector3(0.4f, 0.25f, 0.01f), new Color(0.12f, 0.25f, 0.32f), 0f, "Pantalla");
        tank.front = new GameObject("Frente").transform;
        tank.front.SetParent(t, false); tank.front.localPosition = new Vector3(0f, 0f, -r - 1.1f);
        return tank;
    }

    /// <summary>Máscara, ante la puerta, se abre (el campo sujeta el agua), entra y queda sumergido, se cierra… y
    /// entonces <paramref name="then"/> (cargar el microcosmos).</summary>
    public IEnumerator Enter(Transform who, System.Action then)
    {
        if (_busy || who == null) { then?.Invoke(); yield break; }
        _busy = true;
        CharacterController cc = who.GetComponent<CharacterController>();
        PlayerController pc = who.GetComponent<PlayerController>();
        if (cc != null) cc.enabled = false;
        if (pc != null) pc.enabled = false;

        // CÁMARA PANORÁMICA: mientras entra, se ve desde fuera cómo se pone la máscara, cruza la pared de agua, la máquina
        // se enciende (burbujas) y cierra los ojos; luego, el cambio de escena.
        if (pc != null && CameraManager.Instance != null)
            CameraManager.Instance.RequestRobbery(new CameraRobbery { type = RobberyType.OrbitTarget, orbitTarget = transform, holdDuration = 6.5f, returnOnFinish = true });
        yield return Move(who, front.position + Vector3.up * 1f, 0.6f);
        who.rotation = Quaternion.LookRotation(transform.position - front.position);
        Transform mask = PutMask(who);
        Debug.Log($"[Virtualización] «{who.name}» se pone la máscara de respiración ante la puerta del tanque.");
        yield return new WaitForSeconds(0.5f);
        yield return Hatch(true);
        Debug.Log("[Virtualización] Puerta abierta: el campo magnético del marco sostiene el agua como una pared (no sale).");
        yield return Move(who, transform.position + Vector3.up * (Base + 1.0f), 1.4f);   // cruza la pared de agua y queda dentro
        yield return Hatch(false);
        Debug.Log($"[Virtualización] «{who.name}» está sumergido y la puerta, cerrada: empieza la virtualización.");
        // LA MÁQUINA SE ENCIENDE: lo lleva al CENTRO del tanque, lo rodean burbujas y cierra los ojos.
        yield return Move(who, transform.position + Vector3.up * (Base + 1.2f), 0.6f);
        StartCoroutine(Bubbles(who.position, 2.5f));
        who.GetComponentInChildren<CubeFace>()?.Show(CubeFace.Expression.Asleep, 3f);
        Debug.Log($"[Virtualización] La máquina se enciende: «{who.name}», en el centro, rodeado de burbujas, cierra los ojos.");
        yield return new WaitForSeconds(1.8f);
        yield return new WaitForSeconds(0.5f);

        MobWorldLoader.Instance.SetReturnPoint(front.position + Vector3.up * 1.1f);
        then?.Invoke();
        yield return new WaitForSeconds(0.2f);
        if (mask != null) Destroy(mask.gameObject);
        if (cc != null) cc.enabled = true;
        if (pc != null) pc.enabled = true;
        _busy = false;
    }

    // Burbujas que suben alrededor de quien está dentro (cubitos translúcidos que se desvanecen).
    IEnumerator Bubbles(Vector3 at, float seconds)
    {
        var list = new System.Collections.Generic.List<(Transform t, float speed)>();
        for (float t = 0f; t < seconds; t += Time.deltaTime)
        {
            if (list.Count < 60 && Random.value < 0.6f)
            {
                GameObject b = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Destroy(b.GetComponent<Collider>());
                b.name = "Burbuja"; b.transform.SetParent(transform, true);
                b.transform.position = at + new Vector3(Random.Range(-0.6f, 0.6f), Random.Range(-1f, 0f), Random.Range(-0.6f, 0.6f));
                b.transform.localScale = Vector3.one * Random.Range(0.03f, 0.08f);
                b.GetComponent<Renderer>().sharedMaterial = KitchenProps.Mat(new Color(0.85f, 0.95f, 1f, 0.55f));
                list.Add((b.transform, Random.Range(0.6f, 1.4f)));
            }
            foreach (var p in list) if (p.t != null) p.t.position += Vector3.up * p.speed * Time.deltaTime;
            yield return null;
        }
        foreach (var p in list) if (p.t != null) Destroy(p.t.gameObject);
    }

    /// <summary>Abre o cierra la puerta delantera (las hojas se deslizan a los lados); el campo magnético se enciende al
    /// abrir y se apaga al cerrar.</summary>
    public IEnumerator Hatch(bool open)
    {
        if (_doorA == null || _doorB == null) yield break;
        if (open && _field != null) _field.gameObject.SetActive(true);
        float w = _doorA.localScale.x;
        Vector3 aFrom = _doorA.localPosition, bFrom = _doorB.localPosition;
        Vector3 aTo = new Vector3(open ? -w * 1.5f : -w * 0.5f, aFrom.y, aFrom.z), bTo = new Vector3(open ? w * 1.5f : w * 0.5f, bFrom.y, bFrom.z);
        for (float k = 0f; k < 0.6f; k += Time.deltaTime)
        {
            float s = Mathf.SmoothStep(0f, 1f, k / 0.6f);
            _doorA.localPosition = Vector3.Lerp(aFrom, aTo, s); _doorB.localPosition = Vector3.Lerp(bFrom, bTo, s);
            yield return null;
        }
        _doorA.localPosition = aTo; _doorB.localPosition = bTo;
        if (!open && _field != null) _field.gameObject.SetActive(false);
    }

    /// <summary>¿Está abierta la puerta? (y por tanto el campo encendido).</summary>
    public bool HatchOpen => _doorA != null && _doorA.localPosition.x < -_doorA.localScale.x;
    public bool FieldOn => _field != null && _field.gameObject.activeSelf;

    static IEnumerator Move(Transform who, Vector3 to, float seconds)
    {
        Vector3 from = who.position;
        for (float t = 0f; t < seconds; t += Time.deltaTime)
        {
            who.position = Vector3.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t / seconds));
            yield return null;
        }
        who.position = to;
    }

    static Transform PutMask(Transform who)
    {
        CreatureRig rig = who.GetComponent<CreatureRig>();
        Transform head = rig != null ? rig.Get(BodyPart.Head) : null;
        Transform parent = head != null ? head : who;
        Transform m = new GameObject("MascaraPuesta").transform;
        m.SetParent(parent, false);
        m.localPosition = head != null ? new Vector3(0f, 0.08f, 0.14f) : new Vector3(0f, 1.6f, 0.2f);
        KitchenProps.C(m, Vector3.zero, new Vector3(0.2f, 0.14f, 0.08f), new Color(0.3f, 0.32f, 0.36f), 0f, "Mascara");
        KitchenProps.C(m, new Vector3(0f, -0.05f, 0.06f), new Vector3(0.06f, 0.06f, 0.1f), new Color(0.55f, 0.7f, 0.8f), 0f, "Boquilla");
        return m;
    }
}
