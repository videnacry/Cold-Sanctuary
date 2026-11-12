using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// HERRAMIENTAS PARA HACER LAS ACTIVIDADES (docs/actividades-y-uniformes.md; Berón 2026-11-04: «parece que solo van al piso que les
/// toca y no hacen nada, deberían realizar las actividades»). Lo que comparten la comida (<see cref="MealRitual"/>), el que hace una
/// actividad (<see cref="ActivityPerformer"/>) y los uniformes (<see cref="Uniforms"/>), para un cuerpo de cubos humano
/// (<see cref="CreatureRig"/>) y para un insecto-humano (<see cref="CubeInsect"/>):
///  · LA MANO: dónde se agarra algo (la mano del cuerpo de cubos; la punta del brazo delantero del insecto).
///  · LOS UTENSILIOS (<see cref="Prop"/>): de cubos, en la mano, con su nombre «Utensilio_…» (se quitan todos con <see cref="ClearProps"/>).
///  · EL MOVIMIENTO DE LOS BRAZOS (<see cref="Arms"/>): dos fotogramas A/B por hueso, mezclados cada fotograma (se ve el gesto entero,
///    no un salto), con el candado de postura para que ni el andar ni la emoción lo pisen.
///  · LOS EFECTOS (<see cref="Burst"/>): unos cubitos que salen y se van (vapor, chispas, polvo, gotas, hojas, papel…), con tope global.
/// </summary>
public static class ActivityKit
{
    // ── La mano ─────────────────────────────────────────────────────────────────────────────────────────────────

    /// <summary>¿Cuerpo de cubos humano (con rig)?</summary>
    public static bool IsHuman(GameObject g) => g != null && g.GetComponent<CreatureRig>() != null && g.GetComponent<CubeInsect>() == null;
    public static bool IsInsect(GameObject g) => g != null && g.GetComponent<CubeInsect>() != null;

    /// <summary>La mano de <paramref name="g"/> (derecha o izquierda): el hueso de la mano del cuerpo de cubos o la punta del brazo
    /// delantero del insecto-humano. null si no tiene.</summary>
    public static Transform Hand(GameObject g, bool right)
    {
        if (g == null) return null;
        if (IsHuman(g))
        {
            CreatureRig rig = g.GetComponent<CreatureRig>();
            return rig.Get(right ? BodyPart.HandRight : BodyPart.HandLeft);
        }
        if (IsInsect(g))
        {
            Transform root = g.transform.Find("Insecto"); if (root == null) return null;
            string leg = right ? "Pata_0_D" : "Pata_0_I", arm = right ? "Brazo_D" : "Brazo_I";
            Transform best = null;
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            {
                if (t.name != leg && t.name != arm) continue;
                // La punta más honda del brazo (la de la tibia/tarso); si no tiene, la tibia; si no, el brazo.
                Transform tip = null;
                // (Sin contar las «Punta» de los utensilios que ya lleva: si no, la mano pasaba a ser la punta del utensilio, Held no lo
                // veía —ronda 7: «insecto: come con cuchara — None»— y los utensilios se apilaban uno dentro de otro.)
                foreach (Transform c in t.GetComponentsInChildren<Transform>(true)) if (c.name == "Punta" && !InProp(c, t)) tip = c;
                if (tip == null) foreach (Transform c in t.GetComponentsInChildren<Transform>(true)) if (c.name == "Tibia_art") tip = c;
                best = tip ?? t;
                break;
            }
            return best;
        }
        return null;
    }

    // ¿Cuelga <paramref name="c"/> de un utensilio (entre él y el brazo <paramref name="arm"/>)?
    static bool InProp(Transform c, Transform arm)
    {
        for (Transform p = c; p != null && p != arm; p = p.parent) if (p.name.StartsWith(PropPrefix)) return true;
        return false;
    }

    /// <summary>La estatura de <paramref name="g"/> respecto a la del cuerpo de cubos (1,75 m): escala de los utensilios del insecto.</summary>
    public static float Scale(GameObject g) => g == null ? 1f : Mathf.Clamp(PersonLife.HeightOf(g.transform) / 1.75f, 0.05f, 4f);

    // ── Los utensilios ──────────────────────────────────────────────────────────────────────────────────────────

    public enum Prop
    {
        None, Spoon, Fork, Knife, Cup, Plate, Tray, Ladle, Whisk, Pan, Broom, Mop, Cloth, Spray, Hoe, WateringCan, Basket,
        Machete, Hammer, Trowel, Bucket, Wrench, Screwdriver, Clipboard, Pen, Stethoscope, Syringe, Thermometer, Book, Chalk,
        Pipette, Flask, Tablet, Needle, Yarn, Flashlight, Bottle, Brush, Box, Rope, Shears, Bandage, Scalpel, Sponge, Pitcher,
    }

    const string PropPrefix = "Utensilio_";
    static readonly Color Steel = new Color(0.73f, 0.75f, 0.78f), Dark = new Color(0.17f, 0.17f, 0.18f), Wood = new Color(0.71f, 0.56f, 0.42f),
        White = new Color(0.95f, 0.95f, 0.93f), Yellow = new Color(0.95f, 0.8f, 0.2f), Red = new Color(0.8f, 0.2f, 0.18f), Green = new Color(0.3f, 0.6f, 0.3f),
        Blue = new Color(0.25f, 0.45f, 0.8f), Glass = new Color(0.8f, 0.9f, 0.93f, 0.6f), Paper = new Color(0.97f, 0.96f, 0.9f);

    /// <summary>Pone <paramref name="p"/> en la mano de <paramref name="g"/> (quita lo que tuviera en ESA mano). Devuelve el utensilio.</summary>
    public static Transform Hold(GameObject g, bool right, Prop p)
    {
        Transform hand = Hand(g, right);
        if (hand == null) return null;
        string side = right ? "D" : "I";
        foreach (Transform c in hand) if (c.name.StartsWith(PropPrefix)) { c.name = "Retirado"; c.gameObject.SetActive(false); Object.Destroy(c.gameObject); }
        if (p == Prop.None) return null;
        Transform root = new GameObject($"{PropPrefix}{p}_{side}").transform;
        root.SetParent(hand, false);
        root.localPosition = IsHuman(g) ? new Vector3(0f, -0.1f, 0.02f) : Vector3.zero;
        root.localRotation = Quaternion.identity;
        if (!IsHuman(g))   // el insecto: a su escala (las piezas están hechas para la mano del cuerpo de cubos de 1,9 m)
        {
            Vector3 ls = hand.lossyScale;
            float k = Scale(g) / Mathf.Max(1e-4f, (Mathf.Abs(ls.x) + Mathf.Abs(ls.y) + Mathf.Abs(ls.z)) / 3f);
            root.localScale = Vector3.one * k;
        }
        Build(root, p);
        return root;
    }

    /// <summary>¿Qué tiene en la mano? (None si nada)</summary>
    public static Prop Held(GameObject g, bool right)
    {
        Transform hand = Hand(g, right); if (hand == null) return Prop.None;
        foreach (Transform c in hand)
            if (c.name.StartsWith(PropPrefix))
            {
                string n = c.name.Substring(PropPrefix.Length); int u = n.LastIndexOf('_'); if (u > 0) n = n.Substring(0, u);
                if (System.Enum.TryParse(n, out Prop p)) return p;
            }
        return Prop.None;
    }

    /// <summary>Quita los utensilios de las dos manos.</summary>
    public static void ClearProps(GameObject g) { Hold(g, true, Prop.None); Hold(g, false, Prop.None); }

    /// <summary>La punta de lo que tiene en la mano (donde sale el efecto): la mano si no tiene nada.</summary>
    public static Vector3 Tip(GameObject g, bool right)
    {
        Transform hand = Hand(g, right);
        if (hand == null) return g != null ? g.transform.position + g.transform.forward * 0.5f : Vector3.zero;
        foreach (Transform c in hand)
            if (c.name.StartsWith(PropPrefix))
            {
                Transform tip = c.Find("Punta");
                return tip != null ? tip.position : c.position;
            }
        return hand.position;
    }

    static Transform C(Transform t, Vector3 pos, Vector3 size, Color c, string name = "pieza", float xRot = 0f) => KitchenProps.C(t, pos, size, c, new Vector3(xRot, 0f, 0f), name);
    static void Tip(Transform t, Vector3 at) { Transform p = new GameObject("Punta").transform; p.SetParent(t, false); p.localPosition = at; }

    // Las piezas, en el marco de la mano: −Y sigue al brazo (la punta de los dedos), +Z es la palma hacia delante.
    static void Build(Transform t, Prop p)
    {
        switch (p)
        {
            case Prop.Spoon: C(t, new Vector3(0f, -0.06f, 0f), new Vector3(0.012f, 0.13f, 0.006f), Steel, "Mango"); C(t, new Vector3(0f, -0.14f, 0f), new Vector3(0.035f, 0.045f, 0.012f), Steel, "Cazoleta"); Tip(t, new Vector3(0f, -0.15f, 0f)); break;
            case Prop.Fork: C(t, new Vector3(0f, -0.06f, 0f), new Vector3(0.012f, 0.13f, 0.006f), Steel, "Mango"); for (int i = 0; i < 3; i++) C(t, new Vector3(-0.01f + i * 0.01f, -0.15f, 0f), new Vector3(0.004f, 0.05f, 0.004f), Steel, "Diente"); Tip(t, new Vector3(0f, -0.17f, 0f)); break;
            case Prop.Knife: C(t, new Vector3(0f, -0.03f, 0f), new Vector3(0.016f, 0.08f, 0.012f), Dark, "Cacha"); C(t, new Vector3(0f, -0.12f, 0f), new Vector3(0.025f, 0.11f, 0.004f), Steel, "Hoja"); Tip(t, new Vector3(0f, -0.17f, 0f)); break;
            case Prop.Scalpel: C(t, new Vector3(0f, -0.04f, 0f), new Vector3(0.01f, 0.1f, 0.01f), Steel, "Mango"); C(t, new Vector3(0f, -0.1f, 0f), new Vector3(0.012f, 0.025f, 0.003f), White, "Hoja"); Tip(t, new Vector3(0f, -0.11f, 0f)); break;
            case Prop.Cup: C(t, new Vector3(0f, -0.05f, 0.05f), new Vector3(0.07f, 0.09f, 0.07f), White, "Taza"); C(t, new Vector3(0f, -0.01f, 0.05f), new Vector3(0.055f, 0.005f, 0.055f), new Color(0.45f, 0.28f, 0.16f), "Bebida"); Tip(t, new Vector3(0f, 0f, 0.05f)); break;
            case Prop.Plate: { Transform d = KitchenProps.Plate(t, "Plato", new Vector3(0f, -0.04f, 0.1f)); d.localRotation = Quaternion.Euler(-90f, 0f, 0f); Tip(t, new Vector3(0f, -0.04f, 0.12f)); break; }
            case Prop.Tray: C(t, new Vector3(0.18f, -0.04f, 0.14f), new Vector3(0.42f, 0.015f, 0.3f), new Color(0.55f, 0.36f, 0.22f), "Bandeja", -90f); Tip(t, new Vector3(0.18f, -0.04f, 0.14f)); break;
            case Prop.Ladle: C(t, new Vector3(0f, -0.1f, 0f), new Vector3(0.012f, 0.22f, 0.012f), Steel, "Mango"); C(t, new Vector3(0f, -0.23f, 0.025f), new Vector3(0.07f, 0.04f, 0.07f), Steel, "Cazo"); Tip(t, new Vector3(0f, -0.24f, 0.025f)); break;
            case Prop.Whisk: C(t, new Vector3(0f, -0.05f, 0f), new Vector3(0.016f, 0.1f, 0.016f), Steel, "Mango"); for (int i = 0; i < 3; i++) KitchenProps.C(t, new Vector3(0f, -0.15f, 0f), new Vector3(0.05f, 0.1f, 0.004f), Steel, i * 60f, "Varillas"); Tip(t, new Vector3(0f, -0.2f, 0f)); break;
            case Prop.Pan: C(t, new Vector3(0f, -0.06f, 0f), new Vector3(0.02f, 0.14f, 0.02f), Dark, "Mango"); C(t, new Vector3(0f, -0.2f, 0.06f), new Vector3(0.22f, 0.02f, 0.22f), Dark, "Sarten", -90f); Tip(t, new Vector3(0f, -0.2f, 0.08f)); break;
            case Prop.Broom: C(t, new Vector3(0f, -0.25f, 0f), new Vector3(0.025f, 1.0f, 0.025f), Wood, "Palo"); C(t, new Vector3(0f, -0.8f, 0f), new Vector3(0.28f, 0.12f, 0.06f), Yellow, "Cepillo"); Tip(t, new Vector3(0f, -0.86f, 0f)); break;
            case Prop.Mop: C(t, new Vector3(0f, -0.25f, 0f), new Vector3(0.025f, 1.0f, 0.025f), Blue, "Palo"); C(t, new Vector3(0f, -0.8f, 0f), new Vector3(0.18f, 0.12f, 0.18f), White, "Fregona"); Tip(t, new Vector3(0f, -0.86f, 0f)); break;
            case Prop.Cloth: C(t, new Vector3(0f, -0.04f, 0.02f), new Vector3(0.1f, 0.03f, 0.08f), new Color(0.35f, 0.6f, 0.85f), "Bayeta"); Tip(t, new Vector3(0f, -0.05f, 0.02f)); break;
            case Prop.Sponge: C(t, new Vector3(0f, -0.04f, 0.02f), new Vector3(0.09f, 0.04f, 0.06f), Yellow, "Esponja"); C(t, new Vector3(0f, -0.065f, 0.02f), new Vector3(0.09f, 0.01f, 0.06f), Green, "Estropajo"); Tip(t, new Vector3(0f, -0.07f, 0.02f)); break;
            case Prop.Spray: C(t, new Vector3(0f, -0.05f, 0.02f), new Vector3(0.06f, 0.12f, 0.05f), new Color(0.3f, 0.75f, 0.85f), "Bote"); C(t, new Vector3(0f, 0.03f, 0.04f), new Vector3(0.03f, 0.04f, 0.05f), White, "Gatillo"); Tip(t, new Vector3(0f, 0.04f, 0.07f)); break;
            case Prop.Hoe: C(t, new Vector3(0f, -0.3f, 0f), new Vector3(0.025f, 1.0f, 0.025f), Wood, "Palo"); C(t, new Vector3(0f, -0.8f, 0.07f), new Vector3(0.16f, 0.025f, 0.12f), Steel, "Azada"); Tip(t, new Vector3(0f, -0.82f, 0.1f)); break;
            case Prop.WateringCan: C(t, new Vector3(0f, -0.12f, 0.06f), new Vector3(0.14f, 0.16f, 0.18f), Green, "Regadera"); C(t, new Vector3(0f, -0.08f, 0.2f), new Vector3(0.025f, 0.025f, 0.16f), Green, "Pitorro", 30f); Tip(t, new Vector3(0f, -0.04f, 0.28f)); break;
            case Prop.Basket: C(t, new Vector3(0f, -0.14f, 0.06f), new Vector3(0.24f, 0.14f, 0.18f), Wood, "Cesta"); C(t, new Vector3(0f, -0.06f, 0.06f), new Vector3(0.2f, 0.02f, 0.14f), new Color(0.85f, 0.35f, 0.25f), "Cosecha"); Tip(t, new Vector3(0f, -0.06f, 0.06f)); break;
            case Prop.Machete: C(t, new Vector3(0f, -0.04f, 0f), new Vector3(0.025f, 0.1f, 0.02f), Dark, "Cacha"); C(t, new Vector3(0f, -0.22f, 0f), new Vector3(0.05f, 0.3f, 0.006f), Steel, "Hoja"); Tip(t, new Vector3(0f, -0.37f, 0f)); break;
            case Prop.Shears: for (int s = -1; s <= 1; s += 2) C(t, new Vector3(s * 0.012f, -0.08f, 0f), new Vector3(0.01f, 0.16f, 0.008f), Steel, "Hoja"); C(t, new Vector3(0f, -0.01f, 0f), new Vector3(0.04f, 0.04f, 0.015f), Red, "Mango"); Tip(t, new Vector3(0f, -0.16f, 0f)); break;
            case Prop.Hammer: C(t, new Vector3(0f, -0.1f, 0f), new Vector3(0.025f, 0.26f, 0.025f), Wood, "Mango"); C(t, new Vector3(0f, -0.24f, 0.02f), new Vector3(0.05f, 0.05f, 0.12f), Dark, "Cabeza"); Tip(t, new Vector3(0f, -0.24f, 0.08f)); break;
            case Prop.Trowel: C(t, new Vector3(0f, -0.04f, 0f), new Vector3(0.025f, 0.08f, 0.025f), Wood, "Mango"); C(t, new Vector3(0f, -0.13f, 0.02f), new Vector3(0.08f, 0.11f, 0.006f), Steel, "Llana"); Tip(t, new Vector3(0f, -0.18f, 0.02f)); break;
            case Prop.Bucket: C(t, new Vector3(0f, -0.2f, 0f), new Vector3(0.2f, 0.22f, 0.2f), new Color(0.6f, 0.62f, 0.66f), "Cubo"); C(t, new Vector3(0f, -0.09f, 0f), new Vector3(0.17f, 0.01f, 0.17f), new Color(0.55f, 0.45f, 0.35f), "Barro"); Tip(t, new Vector3(0f, -0.09f, 0f)); break;
            case Prop.Wrench: C(t, new Vector3(0f, -0.08f, 0f), new Vector3(0.022f, 0.18f, 0.01f), Steel, "Mango"); C(t, new Vector3(0f, -0.18f, 0f), new Vector3(0.06f, 0.035f, 0.012f), Steel, "Boca"); Tip(t, new Vector3(0f, -0.2f, 0f)); break;
            case Prop.Screwdriver: C(t, new Vector3(0f, -0.03f, 0f), new Vector3(0.03f, 0.08f, 0.03f), Red, "Mango"); C(t, new Vector3(0f, -0.12f, 0f), new Vector3(0.008f, 0.12f, 0.008f), Steel, "Vastago"); Tip(t, new Vector3(0f, -0.18f, 0f)); break;
            case Prop.Clipboard: C(t, new Vector3(0f, -0.06f, 0.06f), new Vector3(0.18f, 0.24f, 0.012f), Wood, "Tabla", -60f); C(t, new Vector3(0f, -0.055f, 0.07f), new Vector3(0.16f, 0.2f, 0.004f), Paper, "Hoja", -60f); Tip(t, new Vector3(0f, -0.05f, 0.08f)); break;
            case Prop.Pen: C(t, new Vector3(0f, -0.05f, 0f), new Vector3(0.01f, 0.12f, 0.01f), Blue, "Boli"); Tip(t, new Vector3(0f, -0.11f, 0f)); break;
            case Prop.Chalk: C(t, new Vector3(0f, -0.04f, 0f), new Vector3(0.014f, 0.06f, 0.014f), White, "Tiza"); Tip(t, new Vector3(0f, -0.07f, 0f)); break;
            case Prop.Stethoscope: C(t, new Vector3(0f, -0.04f, 0.02f), new Vector3(0.05f, 0.012f, 0.05f), Steel, "Campana"); C(t, new Vector3(0f, 0.08f, 0f), new Vector3(0.01f, 0.22f, 0.01f), Dark, "Tubo"); Tip(t, new Vector3(0f, -0.05f, 0.03f)); break;
            case Prop.Syringe: C(t, new Vector3(0f, -0.06f, 0f), new Vector3(0.02f, 0.1f, 0.02f), Glass, "Cuerpo"); C(t, new Vector3(0f, -0.13f, 0f), new Vector3(0.003f, 0.04f, 0.003f), Steel, "Aguja"); Tip(t, new Vector3(0f, -0.15f, 0f)); break;
            case Prop.Thermometer: C(t, new Vector3(0f, -0.05f, 0f), new Vector3(0.02f, 0.1f, 0.012f), White, "Termometro"); C(t, new Vector3(0f, -0.02f, 0.007f), new Vector3(0.014f, 0.02f, 0.002f), new Color(0.3f, 0.9f, 0.5f), "Pantalla"); Tip(t, new Vector3(0f, -0.1f, 0f)); break;
            case Prop.Bandage: C(t, new Vector3(0f, -0.05f, 0.02f), new Vector3(0.06f, 0.06f, 0.06f), White, "Venda"); Tip(t, new Vector3(0f, -0.08f, 0.02f)); break;
            case Prop.Book: C(t, new Vector3(0f, -0.06f, 0.08f), new Vector3(0.2f, 0.26f, 0.03f), new Color(0.55f, 0.18f, 0.2f), "Tapas", -70f); C(t, new Vector3(0f, -0.055f, 0.085f), new Vector3(0.19f, 0.25f, 0.034f), Paper, "Paginas", -70f); Tip(t, new Vector3(0f, -0.04f, 0.1f)); break;
            case Prop.Pipette: C(t, new Vector3(0f, -0.07f, 0f), new Vector3(0.02f, 0.14f, 0.02f), White, "Pipeta"); C(t, new Vector3(0f, -0.17f, 0f), new Vector3(0.008f, 0.07f, 0.008f), Glass, "Punta"); Tip(t, new Vector3(0f, -0.21f, 0f)); break;
            case Prop.Flask: C(t, new Vector3(0f, -0.1f, 0.03f), new Vector3(0.08f, 0.08f, 0.08f), Glass, "Matraz"); C(t, new Vector3(0f, -0.04f, 0.03f), new Vector3(0.025f, 0.06f, 0.025f), Glass, "Cuello"); C(t, new Vector3(0f, -0.12f, 0.03f), new Vector3(0.07f, 0.04f, 0.07f), new Color(0.4f, 0.8f, 0.5f, 0.8f), "Liquido"); Tip(t, new Vector3(0f, -0.01f, 0.03f)); break;
            case Prop.Tablet: C(t, new Vector3(0f, -0.06f, 0.07f), new Vector3(0.2f, 0.27f, 0.012f), Dark, "Tableta", -60f); C(t, new Vector3(0f, -0.055f, 0.078f), new Vector3(0.18f, 0.24f, 0.003f), new Color(0.35f, 0.65f, 0.95f), "Pantalla", -60f); Tip(t, new Vector3(0f, -0.05f, 0.09f)); break;
            case Prop.Needle: C(t, new Vector3(0f, -0.05f, 0f), new Vector3(0.004f, 0.08f, 0.004f), Steel, "Aguja"); C(t, new Vector3(0f, -0.02f, 0.03f), new Vector3(0.003f, 0.003f, 0.06f), Red, "Hilo"); Tip(t, new Vector3(0f, -0.09f, 0f)); break;
            case Prop.Yarn: C(t, new Vector3(0f, -0.06f, 0.03f), new Vector3(0.08f, 0.08f, 0.08f), new Color(0.75f, 0.3f, 0.55f), "Ovillo"); Tip(t, new Vector3(0f, -0.06f, 0.03f)); break;
            case Prop.Flashlight: C(t, new Vector3(0f, -0.06f, 0f), new Vector3(0.03f, 0.13f, 0.03f), Dark, "Linterna"); C(t, new Vector3(0f, -0.13f, 0f), new Vector3(0.04f, 0.02f, 0.04f), Yellow, "Luz"); Tip(t, new Vector3(0f, -0.15f, 0f)); break;
            case Prop.Bottle: C(t, new Vector3(0f, -0.07f, 0f), new Vector3(0.05f, 0.12f, 0.05f), White, "Biberon"); C(t, new Vector3(0f, -0.15f, 0f), new Vector3(0.02f, 0.04f, 0.02f), new Color(0.85f, 0.6f, 0.3f), "Tetina"); Tip(t, new Vector3(0f, -0.17f, 0f)); break;
            case Prop.Pitcher: C(t, new Vector3(0f, -0.1f, 0.05f), new Vector3(0.1f, 0.16f, 0.1f), new Color(0.85f, 0.55f, 0.35f), "Jarra"); Tip(t, new Vector3(0f, -0.02f, 0.1f)); break;
            case Prop.Brush: C(t, new Vector3(0f, -0.05f, 0f), new Vector3(0.012f, 0.12f, 0.012f), Wood, "Mango"); C(t, new Vector3(0f, -0.12f, 0f), new Vector3(0.03f, 0.03f, 0.015f), Dark, "Pelo"); Tip(t, new Vector3(0f, -0.14f, 0f)); break;
            case Prop.Box: C(t, new Vector3(0.16f, -0.08f, 0.12f), new Vector3(0.34f, 0.24f, 0.26f), new Color(0.72f, 0.58f, 0.38f), "Caja"); Tip(t, new Vector3(0.16f, 0.05f, 0.12f)); break;
            case Prop.Rope: C(t, new Vector3(0f, -0.06f, 0.03f), new Vector3(0.1f, 0.1f, 0.04f), new Color(0.75f, 0.65f, 0.45f), "Rollo"); C(t, new Vector3(0f, -0.2f, 0.03f), new Vector3(0.012f, 0.2f, 0.012f), new Color(0.75f, 0.65f, 0.45f), "Cabo"); Tip(t, new Vector3(0f, -0.3f, 0.03f)); break;
        }
    }

    // ── Los brazos (y el tronco) ────────────────────────────────────────────────────────────────────────────────

    /// <summary>Un gesto: dos fotogramas (A ↔ B) por hueso y su periodo. Los huesos que no nombra quedan como los dejó la postura base.</summary>
    public sealed class Arms
    {
        public (BodyPart part, Vector3 a, Vector3 b)[] bones;
        public float period = 1.6f;
        public Arms(float period, params (BodyPart, Vector3, Vector3)[] bones) { this.period = period; this.bones = bones; }
    }

    /// <summary>Aplica el gesto <paramref name="arms"/> en el instante <paramref name="t"/> (0 → A, ½ periodo → B, y vuelta).</summary>
    public static void Apply(GameObject g, Arms arms, float t)
    {
        if (arms == null || g == null) return;
        CreatureRig rig = g.GetComponent<CreatureRig>(); if (rig == null) return;
        float k = 0.5f - 0.5f * Mathf.Cos(t / Mathf.Max(0.1f, arms.period) * Mathf.PI * 2f);
        foreach (var b in arms.bones)
        {
            Transform bone = rig.Get(b.part); if (bone == null) continue;
            rig.LockPose(b.part, 0.3f);
            bone.localRotation = Quaternion.Slerp(Quaternion.Euler(b.a), Quaternion.Euler(b.b), k);
        }
    }

    /// <summary>La fase del gesto (0…1) en el instante <paramref name="t"/>, para el brazo del insecto y para saber cuándo «golpea».</summary>
    public static float Phase(float period, float t) => 0.5f - 0.5f * Mathf.Cos(t / Mathf.Max(0.1f, period) * Mathf.PI * 2f);

    // ── Los efectos ─────────────────────────────────────────────────────────────────────────────────────────────

    public enum Fx { None, Steam, Sparks, Dust, Water, Leaves, Soil, Paper, Bubbles, Thread, Heart, Glow, Chalk, Crumbs, Sweat }

    /// <summary>Cuántos efectos se han lanzado (pruebas) y cuántos hay vivos (tope <see cref="MaxAlive"/>).</summary>
    public static int Bursts { get; private set; }
    public const int MaxAlive = 48;
    internal static int Alive;

    /// <summary>Unos cubitos que salen de <paramref name="at"/> y se van en ~1 s (vapor que sube, chispas que caen, gotas…).</summary>
    public static void Burst(Vector3 at, Fx fx, float scale = 1f)
    {
        if (fx == Fx.None || !Application.isPlaying) return;
        Bursts++;
        if (Alive >= MaxAlive) return;   // (cuenta igual: se lanzó; solo no se dibuja si ya hay muchos)
        Color c; Vector3 v; float g, size, life; int n;
        switch (fx)
        {
            case Fx.Steam:   c = new Color(0.95f, 0.95f, 0.97f, 0.7f); v = new Vector3(0f, 0.6f, 0f); g = -0.2f; size = 0.05f; life = 1.4f; n = 5; break;
            case Fx.Sparks:  c = new Color(1f, 0.8f, 0.25f); v = new Vector3(0f, 1.2f, 0f); g = 4f; size = 0.018f; life = 0.6f; n = 7; break;
            case Fx.Dust:    c = new Color(0.78f, 0.7f, 0.58f, 0.8f); v = new Vector3(0f, 0.35f, 0f); g = 0.1f; size = 0.04f; life = 1.1f; n = 6; break;
            case Fx.Water:   c = new Color(0.45f, 0.7f, 0.95f, 0.85f); v = new Vector3(0f, -0.2f, 0.2f); g = 5f; size = 0.02f; life = 0.7f; n = 8; break;
            case Fx.Leaves:  c = new Color(0.4f, 0.7f, 0.3f); v = new Vector3(0f, 0.5f, 0f); g = 1.2f; size = 0.035f; life = 1.2f; n = 5; break;
            case Fx.Soil:    c = new Color(0.42f, 0.3f, 0.2f); v = new Vector3(0f, 0.8f, 0f); g = 4f; size = 0.03f; life = 0.7f; n = 6; break;
            case Fx.Paper:   c = new Color(0.97f, 0.96f, 0.9f); v = new Vector3(0f, 0.25f, 0f); g = 0.4f; size = 0.04f; life = 1.0f; n = 2; break;
            case Fx.Bubbles: c = new Color(0.85f, 0.95f, 1f, 0.6f); v = new Vector3(0f, 0.5f, 0f); g = -0.1f; size = 0.03f; life = 1.3f; n = 6; break;
            case Fx.Thread:  c = new Color(0.8f, 0.3f, 0.55f); v = new Vector3(0f, 0.2f, 0f); g = 0.5f; size = 0.012f; life = 0.8f; n = 4; break;
            case Fx.Heart:   c = new Color(0.95f, 0.45f, 0.6f); v = new Vector3(0f, 0.5f, 0f); g = -0.1f; size = 0.045f; life = 1.4f; n = 2; break;
            case Fx.Glow:    c = new Color(0.4f, 0.8f, 1f); v = new Vector3(0f, 0.3f, 0f); g = -0.1f; size = 0.025f; life = 0.9f; n = 4; break;
            case Fx.Chalk:   c = new Color(0.97f, 0.97f, 0.97f); v = new Vector3(0f, -0.1f, 0f); g = 1.5f; size = 0.015f; life = 0.8f; n = 5; break;
            case Fx.Crumbs:  c = new Color(0.85f, 0.65f, 0.4f); v = new Vector3(0f, 0.1f, 0f); g = 4f; size = 0.012f; life = 0.5f; n = 4; break;
            default:         c = new Color(0.7f, 0.85f, 1f, 0.7f); v = new Vector3(0f, -0.1f, 0f); g = 3f; size = 0.015f; life = 0.6f; n = 3; break;   // Sweat
        }
        GameObject root = new GameObject("Efecto_" + fx);
        root.transform.position = at;
        FxBurst b = root.AddComponent<FxBurst>();
        b.Init(c, v * scale, g * scale, size * scale, life, n, scale);
    }
}

