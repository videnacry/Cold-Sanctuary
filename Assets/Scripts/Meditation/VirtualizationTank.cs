using System.Collections;
using UnityEngine;

/// <summary>
/// TANQUE DE VIRTUALIZACIÓN = una PECERA cuadrada y transparente, llena de agua hasta arriba y con la tapa SELLADA,
/// subida sobre un bastidor. Se entra por DEBAJO, por una ESCOTILLA HORIZONTAL en el suelo del tanque: al abrirla el
/// agua no cae, por dos razones que se suman.
///  · PRESIÓN ATMOSFÉRICA (física real): con la tapa sellada no puede entrar aire por arriba, así que el aire de abajo
///    empuja el agua hacia dentro con ~10 t/m² (sostiene hasta 10 m de columna). Es el principio de los acuarios
///    invertidos de los estanques y de la campana de buceo.
///  · ANILLO MAGNÉTICO (lo que añade el santuario): una superficie de agua boca abajo de casi un metro es inestable (se
///    formarían gotas y burbujas que intercambian agua por aire: inestabilidad de Rayleigh–Taylor). El agua es
///    DIAMAGNÉTICA (la repelen los imanes fuertes: "efecto Moisés"), y unas bobinas alrededor de la escotilla mantienen
///    la superficie plana. No tienen que sostener el peso del agua, solo estabilizarla, así que bastan campos mucho
///    menores que los de la levitación diamagnética (≈16 T).
/// Para virtualizar: el personaje se pone bajo el tanque con la MÁSCARA de respiración, se abre la escotilla, sube por
/// la escalerilla al agua, la escotilla se cierra y, ya sumergido, se carga el microcosmos.
/// </summary>
public class VirtualizationTank : MonoBehaviour
{
    public float radius = 0.9f;       // medio lado de la pecera
    public float height = 2.2f;       // alto del agua
    public const float LegHeight = 1.4f;   // el suelo del tanque queda a esta altura (se entra por debajo)
    public Transform front;           // dónde se pone el personaje antes de entrar
    [SerializeField] Transform _hatchA, _hatchB, _ringGlow;
    static bool _busy;

    public static VirtualizationTank Build(Transform machine)
    {
        Color glass = new Color(0.82f, 0.94f, 1f, 0.18f), water = new Color(0.25f, 0.62f, 0.88f, 0.55f),   // transparentes: se ve quién está dentro
              metal = KitchenProps.Steel, dark = KitchenProps.DarkSteel;
        Color coil = new Color(0.72f, 0.45f, 0.22f), glow = new Color(0.55f, 0.85f, 1f);
        Transform t = new GameObject("Tanque").transform;
        t.SetParent(machine, false);
        t.localPosition = new Vector3(0f, -1f, 0f);   // la máquina está a 1 m; el tanque apoya en el suelo
        VirtualizationTank tank = t.gameObject.AddComponent<VirtualizationTank>();
        float r = tank.radius, h = tank.height, y0 = LegHeight;

        // Bastidor: cuatro patas de acero.
        for (int sx = -1; sx <= 1; sx += 2)
            for (int sz = -1; sz <= 1; sz += 2)
                KitchenProps.C(t, new Vector3(sx * (r + 0.05f), y0 * 0.5f, sz * (r + 0.05f)), new Vector3(0.1f, y0, 0.1f), dark, 0f, "Pata");
        // Suelo del tanque: un marco con el hueco de la escotilla en el centro.
        float hole = 0.45f;   // medio lado de la escotilla
        KitchenProps.C(t, new Vector3(0f, y0 - 0.05f, (r + hole) * 0.5f), new Vector3(r * 2.1f, 0.1f, r - hole), dark, 0f, "Suelo");
        KitchenProps.C(t, new Vector3(0f, y0 - 0.05f, -(r + hole) * 0.5f), new Vector3(r * 2.1f, 0.1f, r - hole), dark, 0f, "Suelo");
        KitchenProps.C(t, new Vector3((r + hole) * 0.5f, y0 - 0.05f, 0f), new Vector3(r - hole, 0.1f, hole * 2f), dark, 0f, "Suelo");
        KitchenProps.C(t, new Vector3(-(r + hole) * 0.5f, y0 - 0.05f, 0f), new Vector3(r - hole, 0.1f, hole * 2f), dark, 0f, "Suelo");

        // Pecera: cuatro paredes de cristal, tapa sellada (con su junta) y el agua llenándola hasta arriba.
        for (int i = 0; i < 4; i++)
        {
            Quaternion q = Quaternion.Euler(0f, i * 90f, 0f);
            Transform p = KitchenProps.C(t, q * new Vector3(0f, y0 + h * 0.5f, r), new Vector3(r * 2f + 0.04f, h, 0.03f), glass, 0f, "Cristal");
            p.localRotation = q;
            Transform edge = KitchenProps.C(t, q * new Vector3(r, y0 + h * 0.5f, r), new Vector3(0.05f, h, 0.05f), metal, 0f, "Arista");
            edge.localRotation = q;
        }
        KitchenProps.C(t, new Vector3(0f, y0 + h + 0.03f, 0f), new Vector3(r * 2.1f, 0.06f, r * 2.1f), glass, 0f, "Tapa_sellada");
        KitchenProps.C(t, new Vector3(0f, y0 + h + 0.07f, 0f), new Vector3(r * 2.12f, 0.03f, r * 2.12f), dark, 0f, "Junta");
        KitchenProps.C(t, new Vector3(0f, y0 + h * 0.5f, 0f), new Vector3(r * 1.96f, h - 0.02f, r * 1.96f), water, 0f, "Agua");

        // Anillo magnético alrededor de la escotilla: bobinas de cobre y un aro de luz que se enciende al abrir.
        for (int i = 0; i < 4; i++)
        {
            Quaternion q = Quaternion.Euler(0f, i * 90f, 0f);
            Transform c = KitchenProps.C(t, q * new Vector3(0f, y0 - 0.16f, hole + 0.07f), new Vector3(hole * 2.3f, 0.12f, 0.1f), coil, 0f, "Bobina");
            c.localRotation = q;
        }
        tank._ringGlow = KitchenProps.C(t, new Vector3(0f, y0 - 0.02f, 0f), new Vector3(hole * 2f, 0.02f, hole * 2f), glow, 0f, "Campo_magnetico");
        tank._ringGlow.gameObject.SetActive(false);

        // ESCOTILLA HORIZONTAL: dos hojas que se deslizan hacia los lados.
        tank._hatchA = KitchenProps.C(t, new Vector3(-hole * 0.5f, y0 - 0.05f, 0f), new Vector3(hole, 0.06f, hole * 2f), metal, 0f, "Escotilla");
        tank._hatchB = KitchenProps.C(t, new Vector3(hole * 0.5f, y0 - 0.05f, 0f), new Vector3(hole, 0.06f, hole * 2f), metal, 0f, "Escotilla");

        // Escalerilla corta bajo la escotilla, máscara colgada con su tubo y consola.
        Transform ladder = new GameObject("Escalerilla").transform; ladder.SetParent(t, false); ladder.localPosition = new Vector3(0f, 0f, -0.3f);
        for (int s = -1; s <= 1; s += 2) KitchenProps.C(ladder, new Vector3(s * 0.22f, y0 * 0.5f, 0f), new Vector3(0.04f, y0, 0.04f), metal, 0f, "Larguero");
        for (float y = 0.3f; y < y0; y += 0.3f) KitchenProps.C(ladder, new Vector3(0f, y, 0f), new Vector3(0.44f, 0.03f, 0.05f), metal, 0f, "Peldaño");
        KitchenProps.C(t, new Vector3(r + 0.25f, 1.5f, -r), new Vector3(0.18f, 0.14f, 0.1f), new Color(0.3f, 0.32f, 0.36f), 0f, "Mascara");
        KitchenProps.C(t, new Vector3(r + 0.25f, 0.95f, -r), new Vector3(0.04f, 0.9f, 0.04f), new Color(0.55f, 0.7f, 0.8f), 0f, "Tubo");
        KitchenProps.C(t, new Vector3(-r - 0.6f, 0.6f, -0.3f), new Vector3(0.5f, 1.2f, 0.4f), KitchenProps.White, 0f, "Consola");
        KitchenProps.C(t, new Vector3(-r - 0.6f, 1.05f, -0.51f), new Vector3(0.4f, 0.25f, 0.01f), new Color(0.12f, 0.25f, 0.32f), 0f, "Pantalla");
        tank.front = new GameObject("Frente").transform;
        tank.front.SetParent(t, false); tank.front.localPosition = new Vector3(0f, 0f, -r - 1.1f);
        return tank;
    }

    /// <summary>Máscara, bajo el tanque, escotilla abierta (el agua no cae), sube al agua, escotilla cerrada… y
    /// entonces <paramref name="then"/> (cargar el microcosmos).</summary>
    public IEnumerator Enter(Transform who, System.Action then)
    {
        if (_busy || who == null) { then?.Invoke(); yield break; }
        _busy = true;
        CharacterController cc = who.GetComponent<CharacterController>();
        PlayerController pc = who.GetComponent<PlayerController>();
        if (cc != null) cc.enabled = false;
        if (pc != null) pc.enabled = false;

        yield return Move(who, front.position + Vector3.up * 1f, 0.6f);
        who.rotation = Quaternion.LookRotation(transform.position - front.position);
        Transform mask = PutMask(who);
        Debug.Log($"[Virtualización] «{who.name}» se pone la máscara de respiración y se coloca bajo la escotilla.");
        yield return new WaitForSeconds(0.5f);
        yield return Move(who, transform.position + Vector3.up * 1f, 0.8f);   // debajo de la pecera

        yield return Hatch(true);
        Debug.Log("[Virtualización] Escotilla abierta: la presión del aire y el anillo magnético sujetan el agua (no cae).");
        yield return Move(who, transform.position + Vector3.up * (LegHeight + 1.05f), 1.4f);   // sube por la escalerilla al agua
        yield return Hatch(false);
        Debug.Log($"[Virtualización] «{who.name}» está sumergido y la escotilla, cerrada: empieza la virtualización.");
        yield return new WaitForSeconds(0.5f);

        MobWorldLoader.Instance.SetReturnPoint(front.position + Vector3.up * 1.1f);
        then?.Invoke();
        yield return new WaitForSeconds(0.2f);
        if (mask != null) Destroy(mask.gameObject);
        if (cc != null) cc.enabled = true;
        if (pc != null) pc.enabled = true;
        _busy = false;
    }

    /// <summary>Abre o cierra la escotilla (las hojas se deslizan hacia los lados); el campo magnético se enciende mientras
    /// está abierta.</summary>
    public IEnumerator Hatch(bool open)
    {
        if (_hatchA == null || _hatchB == null) yield break;
        if (open && _ringGlow != null) _ringGlow.gameObject.SetActive(true);
        float hole = _hatchA.localScale.x;
        Vector3 aFrom = _hatchA.localPosition, bFrom = _hatchB.localPosition;
        Vector3 aTo = new Vector3(open ? -hole * 1.5f : -hole * 0.5f, aFrom.y, aFrom.z), bTo = new Vector3(open ? hole * 1.5f : hole * 0.5f, bFrom.y, bFrom.z);
        for (float k = 0f; k < 0.6f; k += Time.deltaTime)
        {
            float s = Mathf.SmoothStep(0f, 1f, k / 0.6f);
            _hatchA.localPosition = Vector3.Lerp(aFrom, aTo, s); _hatchB.localPosition = Vector3.Lerp(bFrom, bTo, s);
            yield return null;
        }
        _hatchA.localPosition = aTo; _hatchB.localPosition = bTo;
        if (!open && _ringGlow != null) _ringGlow.gameObject.SetActive(false);
    }

    /// <summary>¿Está abierta la escotilla? (y por tanto el campo magnético encendido).</summary>
    public bool HatchOpen => _hatchA != null && _hatchA.localPosition.x < -_hatchA.localScale.x;
    public bool FieldOn => _ringGlow != null && _ringGlow.gameObject.activeSelf;

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
