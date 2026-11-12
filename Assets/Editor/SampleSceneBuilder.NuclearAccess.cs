using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LO DE LA LÍNEA QUE QUEDA EN EL SANTUARIO 1 (docs/sitio-nuclear.md §20): la estación en el casquete y los primeros metros del tubo y del pozo de escape, hasta el fondo somero.
/// Lo de abajo lo hace <see cref="BuildMarineScene"/>, en su propia escena.
/// </summary>
public static partial class SampleSceneBuilder
{
    const float LineBore = 5.2f;   // cabe el módulo (3 × 3,2 m de sección, 4,4 de diagonal): lo que se sacaría por aquí
    static SiteLine _nuclearLine;

    /// <summary>La estación, el tramo de tubo que se ve y la salida de escape.</summary>
    static SiteLine BuildCavernLine(Transform root, Vector3 site, string building)
    {

        Color conc = new Color(0.6f, 0.6f, 0.58f), steel = TreeSteel, dark = new Color(0.25f, 0.27f, 0.3f), yel = new Color(0.95f, 0.8f, 0.2f), green = new Color(0.25f, 0.7f, 0.4f);
        // (2026-11-08) En la estación: la Línea al oeste, su pista al norte, el pozo de escape al sur (a 20 m del tubo), la chimenea al sureste y el radar al este.
        float edge = 8f;
        Vector3 top = site + new Vector3(-edge, 0f, 2f), inward = Vector3.right, side = Vector3.forward;

        Transform st = new GameObject("Linea_cavernas").transform; st.SetParent(root, false); st.SetPositionAndRotation(top, Quaternion.LookRotation(-inward));
        SiteLine line = st.gameObject.AddComponent<SiteLine>();
        BoxCollider trig = st.gameObject.AddComponent<BoxCollider>(); trig.isTrigger = true; trig.center = new Vector3(0f, 1f, 0f); trig.size = new Vector3(3f, 2f, 3f);
        // La caseta y su esclusa (las estaciones cerradas evitan lo que empeoró Kaprun: una puerta abierta tiró del fuego), y el motor, FUERA del tubo.
        KitchenProps.C(st, new Vector3(0f, 0.05f, 1.2f), new Vector3(4.4f, 0.1f, 4f), dark, 0f, "Anden_linea");
        for (int s = -1; s <= 1; s += 2)
            for (int k = 0; k < 2; k++) KitchenProps.C(st, new Vector3(s * 2.2f, 1.6f, -1f + k * 4f), new Vector3(0.2f, 3.2f, 0.2f), steel, 0f, "Poste_caseta");
        KitchenProps.C(st, new Vector3(0f, 3.3f, 1f), new Vector3(5f, 0.2f, 5.4f), conc, 0f, "Techo_caseta");
        KitchenProps.C(st, new Vector3(0f, 1.3f, 3f), new Vector3(3f, 2.6f, 0.2f), yel, 0f, "Esclusa_estacion");
        KitchenProps.C(st, new Vector3(3.8f, 1f, 3.6f), new Vector3(2.2f, 2f, 2.2f), new Color(0.35f, 0.45f, 0.55f), 0f, "Cabrestante_electrico");
        Label(st, $"Línea de las cavernas: {SiteLineRule.OneWay(SiteLineRule.Drop):0} s hasta el Santuario Marino, esclusas incluidas", 4f);
        line.gameObject.AddComponent<HunterCell>();   // el árbol cazador se lleva desde arriba, como la roca (docs/contencion-arbol-cazador.md)
        // La alfombra llega aquí: una pista de la RED DEL SANTUARIO, como el muelle del Aéreo (2026-11-08: ya no hay árbol del sitio), en su propio soporte.
        Transform padHolder = new GameObject("Linea_alfombras").transform; padHolder.SetParent(root, false);
        Vector3 padAt = top + side * 7.5f + Vector3.up * 0.01f;
        CarpetPad.BuildZone(padHolder, padAt, Quaternion.LookRotation(inward), CarpetPad.Kind.Santuario, building, 0, $"{building}#linea", SiteLine.AreaName, 3);

        // ── EL TUBO QUE SE VE: de cristal, del casquete al fondo somero, con su escudo por fuera. Es el tramo que se suelta ──
        float reach = SiteLineRule.VisibleDrop + 2f, h = LineBore * 0.5f;
        Transform tube = new GameObject("Tubo_linea").transform; tube.SetParent(root, false); tube.SetPositionAndRotation(top, Quaternion.LookRotation(Vector3.down, inward));
        tube.position -= inward * (h - 1.4f);
        Transform upper = new GameObject("Tubo_superior").transform; upper.SetParent(tube, false);
        line.upperTube = upper;
        for (int s = -1; s <= 1; s += 2)
        {
            KitchenProps.C(upper, new Vector3(s * h, 0f, reach * 0.5f), new Vector3(0.1f, LineBore, reach), VirtualizationTank.GlassColor, 0f, "Tubo_cristal");
            KitchenProps.C(upper, new Vector3(0f, s * h, reach * 0.5f), new Vector3(LineBore, 0.1f, reach), VirtualizationTank.GlassColor, 0f, "Tubo_cristal");
            KitchenProps.C(upper, new Vector3(s * 0.8f, -h + 0.15f, reach * 0.5f), new Vector3(0.12f, 0.2f, reach), dark, 0f, "Rail_linea");
        }
        for (float z = 2f; z < reach; z += 4f)
            for (int s = -1; s <= 1; s += 2)
            {
                KitchenProps.C(upper, new Vector3(s * (h + 0.1f), 0f, z), new Vector3(0.2f, LineBore + 0.4f, 0.3f), steel, 0f, "Marco_tubo");
                KitchenProps.C(upper, new Vector3(0f, s * (h + 0.1f), z), new Vector3(LineBore + 0.4f, 0.2f, 0.3f), steel, 0f, "Marco_tubo");
            }
        // EL ESCUDO: dientes de hormigón a su alrededor. Para trozos, no para un iceberg entero (§14). Y la boca en el fondo somero: de ahí sigue el viaje.
        for (int k = 0; k < 4; k++)
        {
            Vector3 d = Quaternion.Euler(0f, k * 90f, 0f) * Vector3.forward;
            KitchenProps.C(root, root.InverseTransformPoint(tube.position + Vector3.down * 9f + d * (h + 2.5f)), new Vector3(2f, 12f, 2f), conc, k * 90f, "Escudo_tubo");
        }
        KitchenProps.C(root, root.InverseTransformPoint(new Vector3(tube.position.x, NuclearSiteRule.SeabedY + 0.5f, tube.position.z)), new Vector3(LineBore + 4f, 1f, LineBore + 4f), conc, 0f, "Boca_linea");

        // ── EL CARRO: cabina nivelada; abierta hasta el pasamanos, para ver a quien va ──
        Transform car = new GameObject("Carro_linea").transform; car.SetParent(st, true); car.SetPositionAndRotation(top, Quaternion.LookRotation(inward));
        KitchenProps.C(car, new Vector3(0f, 0.04f, 0f), new Vector3(2.6f, 0.08f, 3f), dark, 0f, "Piso_carro");
        for (int s = -1; s <= 1; s += 2)
        {
            KitchenProps.C(car, new Vector3(s * 1.3f, 0.55f, 0f), new Vector3(0.08f, 1.1f, 3f), yel, 0f, "Costado_carro");
            KitchenProps.C(car, new Vector3(0f, 0.55f, s * 1.5f), new Vector3(2.6f, 1.1f, 0.08f), yel, 0f, "Frontal_carro");
        }
        line.car = car;

        // ── LA SEGUNDA SALIDA: otra tapa y otro pozo, lejos del tubo ──
        Vector3 eHead = top - side * 20f + Vector3.up * 0.05f;
        Transform eT = new GameObject("Salida_escape").transform; eT.SetParent(root, false); eT.position = eHead;
        KitchenProps.C(eT, new Vector3(0f, 0.4f, 0f), new Vector3(3.2f, 0.8f, 3.2f), green, 0f, "Tapa_escape");
        KitchenProps.C(root, root.InverseTransformPoint(new Vector3(eHead.x, NuclearSiteRule.SeabedY * 0.5f, eHead.z)), new Vector3(2.8f, -NuclearSiteRule.SeabedY, 2.8f), conc, 0f, "Pozo_escape");
        KitchenProps.C(root, root.InverseTransformPoint(new Vector3(eHead.x, -8f, eHead.z)), new Vector3(7f, 12f, 7f), conc, 0f, "Escudo_escape");
        Label(eT, "Salida de escape: pozo propio con jaula a baterías (ni por el tubo ni por el pozo del reactor)", 2.5f);
        line.escapeTop = eT;
        return line;
    }

    /// <summary>La regla de la línea: menos de un minuto, esclusas incluidas, y segunda salida.</summary>
    static void ValidateCavernLine(SiteLine l)
    {
        if (l == null) return;
        var bad = new List<string>();
        float one = SiteLineRule.OneWay(SiteLineRule.Drop);
        if (one >= SiteLineRule.MaxOneWay) bad.Add($"{one:0.0} s por viaje (pide < {SiteLineRule.MaxOneWay:0} s)");
        if (l.escapeTop == null) bad.Add("sin salida de escape");
        if (bad.Count > 0) Debug.LogError($"[Sitio nuclear] Línea: {string.Join("; ", bad)}.");
        else Debug.Log($"[Sitio nuclear] Línea: {SiteLineRule.Drop:0} m, {one:0.0} s por viaje; pozo de escape en {SiteLineRule.EscapeSeconds(l.EscapeDrop, l.EscapeWalk):0} s.");
    }
}
