using UnityEngine;

/// <summary>
/// DETALLE 3 DEL PILOTO (ronda piloto 3 del 2026-11-18: «desde la puerta, a la altura de los ojos —1,6 m de una persona y 0,9 m de un
/// insecto-humano— se ve poco»). Lo que se ve al entrar por la puerta de la fachada (sur, x = 0) mirando al norte:
/// - LUZ: campanas industriales colgadas cada 6 m en filas (con su cable) y las BANDEJAS DE CABLES a 6 m, con las bajadas a los bancos;
/// - RÓTULOS colgados por zonas (ENSAMBLAJE, MOTORES, AVIÓNICA, CONTROL DE MISIÓN, INGENIERÍA, COMEDOR) y avisos de seguridad en los
///   muros (protección ocular, carretillas);
/// - PIEZAS DEL FALCON 1 en el suelo, donde no cortan el paso: cajas de transporte, una tobera Merlin de repuesto en su soporte, un domo
///   de tanque y botellas de helio (COPV) en su jaula;
/// - una fila más de BANCOS con herramientas: el banco de mecanizado (válvulas e inyectores) con su panel de herramientas, al este;
/// - un tercer CATRE del turno de noche y el FRUTAL de la nave, fuera junto a la fachada (regla 11: la nave tiene su hogar del turno de noche);
/// - en el comedor, la mesa alta de seis apoyos con los cojines en naranja y su rótulo (en MuskDetail).
/// Los muebles nuevos (banco, catre, frutal) guardan 2,4 m; lo demás es decorado alto o pegado a los muros. Las piezas colgadas van en
/// tramos de menos de 4 m² (MicroBuildRules.IsRoof): no se desvanecen con la cubierta. SUPUESTOS de juego, salvo las piezas del Falcon 1
/// (Merlin 1C, domo, COPV de helio: Koenigsmann et al. 2009).
/// </summary>
public static partial class StorySceneBuilder
{
    static void MuskDetail3(Transform fac)
    {
        Material steel = M("Acero", 0.7f, 0.72f, 0.75f), dark = M("Pantalla", 0.08f, 0.1f, 0.14f), white = M("Fuselaje_blanco", 0.94f, 0.94f, 0.95f);
        Material yellow = M("Amarillo_seguridad", 0.95f, 0.78f, 0.1f), black = M("Negro_mate", 0.07f, 0.07f, 0.08f), copper = M("Cobre_tobera", 0.72f, 0.42f, 0.25f);
        Material lamp = M("Luz_de_campana", 1f, 0.97f, 0.85f), tray = M("Bandeja_galvanizada", 0.6f, 0.62f, 0.64f), wood = M("Madera_caja", 0.62f, 0.48f, 0.3f);
        Material blue = M("Azul_seguridad", 0.1f, 0.35f, 0.7f), cable = M("Cable_negro", 0.05f, 0.05f, 0.05f), board = M("Panel_perforado", 0.85f, 0.82f, 0.74f);
        Material red = M("Rojo_bandera", 0.7f, 0.12f, 0.14f), green = M("Verde_extintor", 0.15f, 0.5f, 0.2f);
        float hx = MuskFactorySize.x * 0.5f, hz = MuskFactorySize.y * 0.5f;

        // ── LUZ: campanas industriales cada 6 m (4 filas × 7) ──
        for (int i = 0; i < 7; i++)
            for (int j = 0; j < 4; j++)
            {
                float x = -18f + i * 6f, z = -11f + j * 7.3f;
                Prop(fac, "Cable_de_la_campana", new Vector3(x, 8.05f, z), new Vector3(0.02f, 1.7f, 0.02f), cable, false);
                Prop(fac, "Campana_industrial", new Vector3(x, 7.1f, z), new Vector3(0.6f, 0.3f, 0.6f), steel, false);
                Prop(fac, "Luz", new Vector3(x, 6.93f, z), new Vector3(0.45f, 0.04f, 0.45f), lamp, false);
            }
        // ── BANDEJAS DE CABLES a 6 m, de este a oeste por el centro, con las bajadas a los bancos ──
        for (int k = 0; k < 7; k++) Prop(fac, "Bandeja_de_cables", new Vector3(-18f + k * 6f, 6f, 1.5f), new Vector3(5.9f, 0.1f, 0.4f), tray, false);
        foreach (var (x, z) in new[] { (-16f, -1f), (-9f, -1f), (0.5f, 12f), (-3.5f, -4f), (6f, -2.8f), (10.5f, -4.8f), (14.5f, -1f) })
        {
            Prop(fac, "Bajada_de_cables", new Vector3(x, 3.5f, z), new Vector3(0.05f, 5f, 0.05f), cable, false);
            Prop(fac, "Bajada_de_cables", new Vector3(x + 0.08f, 3.5f, z), new Vector3(0.03f, 5f, 0.03f), M("Cable_amarillo", 0.9f, 0.75f, 0.1f), false);
        }

        // ── RÓTULOS colgados por zonas y avisos en los muros ──
        Sign(fac, "ENSAMBLAJE · FALCON 1", new Vector3(-11f, 5.2f, 4.6f), 0f, blue, white);
        Sign(fac, "MOTORES · MERLIN 1C", new Vector3(-12.5f, 4.2f, -2.4f), 0f, blue, white);
        Sign(fac, "AVIÓNICA", new Vector3(0.5f, 4.2f, 10.2f), 0f, blue, white);
        Sign(fac, "CONTROL DE MISIÓN", new Vector3(12f, 3.6f, 1.7f), 0f, black, white);
        Sign(fac, "INGENIERÍA", new Vector3(8f, 4.2f, -6f), 0f, blue, white);
        Sign(fac, "MECANIZADO", new Vector3(14.5f, 4.2f, -2.2f), 0f, blue, white);
        foreach (var (txt, x) in new[] { ("PROTECCIÓN OCULAR OBLIGATORIA", -10f), ("ATENCIÓN: CARRETILLAS", 10f) })
        {
            Prop(fac, "Aviso_de_seguridad", new Vector3(x, 2.6f, -hz + 0.2f), new Vector3(3.2f, 0.7f, 0.04f), yellow, false);
            Text3D(fac, txt, new Vector3(x, 2.6f, -hz + 0.17f), 0.035f, Color.black).localRotation = Quaternion.Euler(0f, 180f, 0f);
        }
        for (int k = 0; k < 3; k++)   // extintores rojos en los pilares de los muros (cada 12 m)
            Prop(fac, "Extintor", new Vector3(-hx + 0.3f, 0.45f, -10f + k * 10f), new Vector3(0.18f, 0.6f, 0.18f), red, false);
        Prop(fac, "Salida_de_emergencia", new Vector3(0f, 2.75f, -hz + 0.2f), new Vector3(1.2f, 0.3f, 0.04f), green, false);

        // ── PIEZAS DEL FALCON 1 en el suelo (donde no cortan el paso) ──
        foreach (var (x, z, w, d) in new[] { (-20.6f, -13.6f, 1.2f, 2.4f), (-5f, 12.6f, 1.2f, 1.0f), (21f, 14f, 1.0f, 1.4f) })
        {
            Prop(fac, "Caja_de_transporte", new Vector3(x, 0.55f, z), new Vector3(w, 1.1f, d), wood, true);
            Prop(fac, "Rotulo_de_la_caja", new Vector3(x, 0.8f, z - d * 0.5f - 0.01f), new Vector3(w * 0.6f, 0.25f, 0.01f), black, false);
        }
        // La tobera Merlin de repuesto, en su soporte, junto a la grúa (fuera del recuadro y del paso).
        Transform noz = new GameObject("Tobera Merlin 1C de repuesto").transform; noz.SetParent(fac, false); noz.localPosition = new Vector3(-20.4f, 0f, 6.3f);
        Prop(noz, "Soporte", new Vector3(0f, 0.3f, 0f), new Vector3(1.2f, 0.6f, 1.2f), steel, true);
        Prop(noz, "Campana", new Vector3(0f, 1.2f, 0f), new Vector3(0.9f, 1.2f, 0.9f), copper, false);
        Prop(noz, "Garganta", new Vector3(0f, 1.95f, 0f), new Vector3(0.4f, 0.3f, 0.4f), copper, false);
        // El domo de un tanque y la jaula de las botellas de helio (COPV), en el muro norte entre estanterías.
        Prop(fac, "Domo_de_tanque", new Vector3(-3.2f, 0.45f, 14.1f), new Vector3(1.6f, 0.9f, 1.4f), white, false);
        Transform cage = new GameObject("Jaula de botellas de helio (COPV)").transform; cage.SetParent(fac, false); cage.localPosition = new Vector3(21.2f, 0f, -2.2f);
        for (int k = 0; k < 4; k++) Prop(cage, "Botella_COPV", new Vector3(0f, 0.6f, -0.9f + k * 0.6f), new Vector3(0.4f, 1.2f, 0.4f), M("Composite_negro", 0.12f, 0.12f, 0.13f), false);
        for (int s = -1; s <= 1; s += 2) Prop(cage, "Barra_de_la_jaula", new Vector3(s * 0.3f, 0.7f, 0f), new Vector3(0.04f, 1.4f, 2.4f), yellow, false);

        // ── EL BANCO DE MECANIZADO (válvulas e inyectores), con su panel de herramientas ──
        Transform mech = Piece(fac, "banco_taller", new Vector3(14.5f, 0f, -1f), 0f, "Banco de mecanizado: válvulas e inyectores del Merlin", new Vector2(2f, 0.75f));
        Prop(mech, "Panel_de_herramientas", new Vector3(0f, 1.6f, 0.45f), new Vector3(1.9f, 1.0f, 0.04f), board, false);
        for (int k = 0; k < 8; k++) Prop(mech, "Herramienta", new Vector3(-0.8f + k * 0.23f, 1.5f + (k % 3) * 0.18f, 0.42f), new Vector3(0.05f, 0.25f, 0.02f), k % 2 == 0 ? steel : M("Mango_rojo", 0.75f, 0.15f, 0.12f), false);
        Prop(mech, "Inyector_del_Merlin", new Vector3(0.4f, 0.98f, 0f), new Vector3(0.3f, 0.12f, 0.3f), steel, false);
        Prop(mech, "Valvula", new Vector3(-0.3f, 0.99f, 0.05f), new Vector3(0.14f, 0.14f, 0.2f), M("Laton", 0.75f, 0.6f, 0.25f), false);
        WorkPlatform(fac, mech, 0.9f, 2f, steel);
        Spot(fac, "Banco de mecanizado", new Vector3(14.5f, 0f, -2.4f), null);

        // ── EL TERCER CATRE del turno de noche y el FRUTAL de la nave (regla 11: tiene hogar, el del turno de noche) ──
        Piece(fac, "cama", new Vector3(-10f, 0f, -5.5f), 0f, "Catre de los turnos de noche (supuesto)", null, station: false);
        Transform fruit = CivicBuilder.FruitTree(fac, fac.name.GetHashCode());
        fruit.localPosition = new Vector3(-17f, 0f, -hz - 6.5f);   // fuera, junto a la fachada (dentro su copa no guardaba los 2,4 m)
    }

    /// <summary>Un RÓTULO colgado (panel, letras y dos cables al techo), legible desde la puerta (mira al sur).</summary>
    static void Sign(Transform parent, string text, Vector3 at, float yaw, Material panel, Material letters)
    {
        Transform s = new GameObject("Rotulo " + text).transform; s.SetParent(parent, false); s.localPosition = at; s.localRotation = Quaternion.Euler(0f, yaw, 0f);
        float w = Mathf.Max(2f, text.Length * 0.22f);
        Prop(s, "Panel_del_rotulo", Vector3.zero, new Vector3(w, 0.55f, 0.05f), panel, false);
        for (int k = -1; k <= 1; k += 2) Prop(s, "Cable_del_rotulo", new Vector3(k * w * 0.4f, (8.8f - at.y) * 0.5f + 0.28f, 0f), new Vector3(0.015f, 8.8f - at.y - 0.28f, 0.015f), M("Cable_negro", 0.05f, 0.05f, 0.05f), false);
        Text3D(s, text, new Vector3(0f, 0f, -0.03f), 0.06f, letters.color);
    }
}
