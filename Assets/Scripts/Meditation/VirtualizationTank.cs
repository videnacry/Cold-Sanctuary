using System.Collections;
using UnityEngine;

/// <summary>
/// TANQUE DE VIRTUALIZACIÓN (la máquina por dentro): un tanque de agua de cristal con escalerilla y máscara de respiración.
/// Para virtualizar, el personaje se acerca, se pone la MÁSCARA (con su tubo), sube por la escalerilla, se mete en el
/// agua y, ya sumergido, se hace el cambio a la escena del microcosmos. Al volver reaparece fuera del tanque (no en el
/// agua). Todo de cubos (octógono de paneles de cristal, agua, aros de metal, escalerilla, consola).
/// </summary>
public class VirtualizationTank : MonoBehaviour
{
    public float radius = 0.9f, height = 2.2f;
    public Transform front;   // dónde se pone el personaje antes de subir
    static bool _busy;

    public static VirtualizationTank Build(Transform machine)
    {
        Color glass = new Color(0.78f, 0.9f, 0.96f, 1f), water = new Color(0.45f, 0.75f, 0.88f, 1f), metal = KitchenProps.Steel, dark = KitchenProps.DarkSteel;
        Transform t = new GameObject("Tanque").transform;
        t.SetParent(machine, false);
        t.localPosition = new Vector3(0f, -1f, 0f);   // la máquina está a 1 m; el tanque apoya en el suelo
        VirtualizationTank tank = t.gameObject.AddComponent<VirtualizationTank>();
        float r = tank.radius, h = tank.height;
        KitchenProps.C(t, new Vector3(0f, 0.1f, 0f), new Vector3(r * 2.3f, 0.2f, r * 2.3f), dark, 22.5f, "Base");
        KitchenProps.C(t, new Vector3(0f, 0.1f, 0f), new Vector3(r * 2.3f, 0.2f, r * 2.3f), dark, 67.5f, "Base");
        for (int i = 0; i < 8; i++)   // paneles de cristal en octógono
        {
            Quaternion q = Quaternion.Euler(0f, i * 45f, 0f);
            Transform p = KitchenProps.C(t, q * Vector3.forward * r + Vector3.up * (0.2f + h * 0.5f), new Vector3(r * 0.83f, h, 0.04f), glass, i * 45f, "Cristal");
            p.localRotation = q;
        }
        KitchenProps.C(t, Vector3.up * (0.2f + h * 0.42f), new Vector3(r * 1.75f, h * 0.84f, r * 1.75f), water, 22.5f, "Agua");
        for (int k = 0; k < 3; k++)   // aros de metal
            KitchenProps.C(t, Vector3.up * (0.25f + k * h * 0.48f), new Vector3(r * 2.1f, 0.06f, r * 2.1f), metal, 22.5f, "Aro");
        // Escalerilla por delante (+Z) y plataforma para subir.
        Transform ladder = new GameObject("Escalerilla").transform; ladder.SetParent(t, false); ladder.localPosition = new Vector3(0f, 0f, -r - 0.35f);
        for (int s = -1; s <= 1; s += 2) KitchenProps.C(ladder, new Vector3(s * 0.25f, (h + 0.4f) * 0.5f, 0f), new Vector3(0.05f, h + 0.4f, 0.05f), metal, 0f, "Larguero");
        for (float y = 0.35f; y < h + 0.3f; y += 0.35f) KitchenProps.C(ladder, new Vector3(0f, y, 0f), new Vector3(0.5f, 0.04f, 0.05f), metal, 0f, "Peldaño");
        // Máscara de respiración colgada con su tubo, y consola de control.
        KitchenProps.C(t, new Vector3(0.45f, h + 0.45f, -r - 0.3f), new Vector3(0.18f, 0.14f, 0.1f), new Color(0.3f, 0.32f, 0.36f), 0f, "Mascara");
        KitchenProps.C(t, new Vector3(0.45f, h * 0.7f, -r - 0.25f), new Vector3(0.04f, h * 0.55f, 0.04f), new Color(0.55f, 0.7f, 0.8f), 0f, "Tubo");
        KitchenProps.C(t, new Vector3(-r - 0.5f, 0.6f, -0.3f), new Vector3(0.5f, 1.2f, 0.4f), KitchenProps.White, 0f, "Consola");
        KitchenProps.C(t, new Vector3(-r - 0.5f, 1.05f, -0.51f), new Vector3(0.4f, 0.25f, 0.01f), new Color(0.12f, 0.25f, 0.32f), 0f, "Pantalla");
        tank.front = new GameObject("Frente").transform;
        tank.front.SetParent(t, false); tank.front.localPosition = new Vector3(0f, 0f, -r - 1.1f);
        return tank;
    }

    /// <summary>Máscara puesta, sube, entra al agua… y entonces <paramref name="then"/> (cargar el microcosmos).</summary>
    public IEnumerator Enter(Transform who, System.Action then)
    {
        if (_busy || who == null) { then?.Invoke(); yield break; }
        _busy = true;
        CharacterController cc = who.GetComponent<CharacterController>();
        PlayerController pc = who.GetComponent<PlayerController>();
        if (cc != null) cc.enabled = false;
        if (pc != null) pc.enabled = false;

        Vector3 start = front.position + Vector3.up * 1f;
        yield return Move(who, start, 0.6f);
        who.rotation = Quaternion.LookRotation(transform.position - front.position);
        // Se pone la máscara (con su tubo) en la cara.
        Transform mask = PutMask(who);
        Debug.Log($"[Virtualización] «{who.name}» se pone la máscara de respiración y sube al tanque.");
        yield return new WaitForSeconds(0.6f);
        yield return Move(who, transform.position + Vector3.up * (height + 1.3f) - transform.forward * (radius + 0.3f), 1.2f);   // sube por la escalerilla
        yield return Move(who, transform.position + Vector3.up * (height + 1.3f), 0.6f);                                       // sobre la boca del tanque
        yield return Move(who, transform.position + Vector3.up * 1.1f, 1.2f);                                                  // se hunde en el agua
        Debug.Log($"[Virtualización] «{who.name}» está sumergido: empieza la virtualización.");
        yield return new WaitForSeconds(0.5f);

        MobWorldLoader.Instance.SetReturnPoint(front.position + Vector3.up * 1.1f);
        then?.Invoke();
        // Ya cargando: devuelve el control (el cargador teletransporta al jugador al spawn del microcosmos).
        yield return new WaitForSeconds(0.2f);
        if (mask != null) Destroy(mask.gameObject);
        if (cc != null) cc.enabled = true;
        if (pc != null) pc.enabled = true;
        _busy = false;
    }

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
