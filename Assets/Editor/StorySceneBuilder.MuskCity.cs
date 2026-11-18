using UnityEngine;

/// <summary>
/// HAWTHORNE 2008: EL REALISMO DEL ESCENARIO, MÁS ALLÁ DE MUSK (Berón 2026-11-18 21:54: «quienes crean las naves son los trabajadores, los
/// científicos… la producción de alimentos o la seguridad urbana… muchos sistemas, animales, personas, bacterias, plantas»). Lote 2:
/// - LA GENTE QUE HACÍA LOS COHETES con sus puestos de verdad: la SOLDADURA POR FRICCIÓN (FSW) de los tanques de aluminio-litio con un
///   cabezal de pasador retráctil Nova-Tech (The Fabricator, sept. y nov. de 2008); el taller de máquinas y la soldadura de arco; en la misma
///   planta, el Falcon 9 y el Dragon junto al Falcon 1 (Satellite Today, 24-11-2008); el guardia, la limpiadora, el encargado de envíos;
/// - LOS CIENTÍFICOS: el LABORATORIO DE MATERIALES (máquina de tracción, probetas, microscopio, durómetro) y la SALA DE DATOS DE PRUEBAS
///   (los ensayos del Merlin en McGregor, Texas), en una NAVE ANEXA cerrada (supuesto de sitio);
/// - LA COMIDA: la cámara frigorífica y el lavamanos de la cocina, la tarjeta de inspección sanitaria del condado («A»; el sistema de
///   letras del Condado de Los Ángeles existe desde 1998), la ENTREGA del proveedor en el muelle y la LONCHERA (camión de comida que visita
///   los polígonos industriales de Los Ángeles; supuesto de sitio);
/// - LA SEGURIDAD: la GARITA del guardia con su barrera y el lector de tarjetas en la entrada del aparcamiento (supuesto: estándar de una
///   empresa con contratos ITAR), los dispensadores de gel y el hidrante;
/// - LAS BACTERIAS donde importan: la SALA LIMPIA de aviónica con su vestuario (buzos, gorros, calzas, alfombra adhesiva), la higiene de la
///   cocina y el POZO DE REGISTRO del alcantarillado (Hawthorne es miembro de los Sanitation Districts del Condado; su red, el Joint Outfall
///   System, depura en Carson —A. K. Warren Water Resource Facility—, según lacsd.org).
/// Todo lo que no es mueble es decorado con NOMBRE FIJO (para los retos de CodeShip: docs/microcosmos-musk-historia.md §Hawthorne 2008) y
/// fuera del paso de los muebles; la nave anexa, con muros, puerta y vista de corte.
/// </summary>
public static partial class StorySceneBuilder
{
    // Devuelve cuántos insectos-humanos añade.
    static int MuskCity(Transform t, Transform fac)
    {
        Material steel = M("Acero", 0.7f, 0.72f, 0.75f), dark = M("Pantalla", 0.08f, 0.1f, 0.14f), white = M("Fuselaje_blanco", 0.94f, 0.94f, 0.95f);
        Material yellow = M("Amarillo_seguridad", 0.95f, 0.78f, 0.1f), black = M("Negro_mate", 0.07f, 0.07f, 0.08f), red = M("Rojo_bandera", 0.7f, 0.12f, 0.14f);
        Material glass = M("Vidrio_garita", 0.6f, 0.75f, 0.85f), curtain = M("Cortina_de_sala_limpia", 0.85f, 0.9f, 0.95f), blue = M("Azul_seguridad", 0.1f, 0.35f, 0.7f);
        Material green = M("Verde_lechuga", 0.35f, 0.6f, 0.25f), cardboard = M("Carton", 0.68f, 0.52f, 0.34f), alu = M("Aluminio_litio", 0.78f, 0.8f, 0.82f);
        float hz = MuskFactorySize.y * 0.5f;
        Vector3 F(float x, float z) => MuskFactoryAt + new Vector3(x, 0f, z);

        // ── LA SALA LIMPIA DE AVIÓNICA: cortinas de lamas alrededor del banco de aviónica y su VESTUARIO a la entrada ──
        Transform clean = new GameObject("Sala limpia de aviónica").transform; clean.SetParent(fac, false); clean.localPosition = new Vector3(0.6f, 0f, 12f);
        Prop(clean, "Cortina_de_sala_limpia", new Vector3(-1.9f, 1.2f, 0f), new Vector3(0.02f, 2.4f, 4.4f), curtain, false);
        Prop(clean, "Cortina_de_sala_limpia", new Vector3(2.1f, 1.2f, 0f), new Vector3(0.02f, 2.4f, 4.4f), curtain, false);
        Prop(clean, "Cortina_de_sala_limpia", new Vector3(-1.2f, 1.2f, -2.2f), new Vector3(1.4f, 2.4f, 0.02f), curtain, false);
        Prop(clean, "Cortina_de_sala_limpia", new Vector3(1.6f, 1.2f, -2.2f), new Vector3(1.0f, 2.4f, 0.02f), curtain, false);
        Prop(clean, "Marco_de_la_sala_limpia", new Vector3(0.2f, 2.45f, 0f), new Vector3(4.1f, 0.08f, 4.5f), steel, false);
        Sign(fac, "SALA LIMPIA · ISO 8", new Vector3(0.8f, 3.1f, 9.7f), 0f, blue, white);
        Transform gown = new GameObject("Vestuario de la sala limpia").transform; gown.SetParent(fac, false); gown.localPosition = new Vector3(2.9f, 0f, 9.2f);
        Prop(gown, "Banco_de_vestirse", new Vector3(0f, 0.2f, 0f), new Vector3(0.4f, 0.4f, 1.2f), steel, false);
        Prop(gown, "Perchero_de_buzos", new Vector3(0.45f, 0.9f, 0f), new Vector3(0.05f, 1.8f, 1.2f), steel, false);
        for (int k = 0; k < 3; k++) Prop(gown, "Buzo_blanco", new Vector3(0.38f, 1.1f, -0.4f + k * 0.4f), new Vector3(0.06f, 1.0f, 0.3f), white, false);
        Prop(gown, "Dispensador_de_calzas_y_gorros", new Vector3(0.45f, 1.2f, 0.75f), new Vector3(0.12f, 0.3f, 0.25f), blue, false);
        Prop(gown, "Alfombra_adhesiva", new Vector3(-1.5f, 0.09f, 0.6f), new Vector3(1.2f, 0.01f, 0.9f), M("Azul_alfombra", 0.3f, 0.5f, 0.8f), false);
        Sanitizer(gown, new Vector3(-0.3f, 0f, -0.75f), steel, white);

        // ── LA COCINA: cámara frigorífica, lavamanos, gel, la tarjeta de inspección («A») y el reciclaje ──
        Transform cold = new GameObject("Cámara frigorífica de la cocina").transform; cold.SetParent(fac, false); cold.localPosition = new Vector3(-21f, 0f, -13.6f);
        Prop(cold, "Camara", new Vector3(0f, 1.1f, 0f), new Vector3(1.2f, 2.2f, 1.6f), M("Acero_inox", 0.8f, 0.82f, 0.84f), true);
        Prop(cold, "Tirador", new Vector3(0.62f, 1.1f, 0.3f), new Vector3(0.04f, 0.4f, 0.04f), black, false);
        Transform kwash = new GameObject("Lavamanos de la cocina (higiene)").transform; kwash.SetParent(fac, false); kwash.localPosition = new Vector3(-15.2f, 0f, -hz + 0.45f);
        Prop(kwash, "Pila", new Vector3(0f, 0.85f, 0f), new Vector3(0.5f, 0.2f, 0.4f), M("Acero_inox", 0.8f, 0.82f, 0.84f), false);
        Prop(kwash, "Pie", new Vector3(0f, 0.4f, 0.1f), new Vector3(0.1f, 0.8f, 0.1f), steel, false);
        Prop(kwash, "Dispensador_de_jabon", new Vector3(0.35f, 1.25f, 0.18f), new Vector3(0.12f, 0.2f, 0.08f), white, false);
        Prop(kwash, "Toallas_de_papel", new Vector3(-0.4f, 1.3f, 0.18f), new Vector3(0.3f, 0.3f, 0.1f), white, false);
        Transform grade = new GameObject("Tarjeta de inspección sanitaria (A)").transform; grade.SetParent(fac, false); grade.localPosition = new Vector3(-17f, 2.2f, -hz + 0.2f);
        Prop(grade, "Tarjeta", Vector3.zero, new Vector3(0.6f, 0.75f, 0.01f), white, false);
        Text3D(grade, "A", new Vector3(0f, 0f, 0.01f), 0.12f, new Color(0.1f, 0.25f, 0.6f)).localRotation = Quaternion.Euler(0f, 180f, 0f);
        Prop(fac, "Cubo_de_reciclaje", new Vector3(-9.4f, 0.45f, -hz + 0.4f), new Vector3(0.5f, 0.9f, 0.5f), blue, false);
        Prop(fac, "Cubo_de_basura", new Vector3(-8.8f, 0.45f, -hz + 0.4f), new Vector3(0.5f, 0.9f, 0.5f), dark, false);
        Sanitizer(fac, new Vector3(2.8f, 0f, -hz + 0.5f), steel, white);   // a la entrada de la nave
        Sanitizer(fac, new Vector3(-7f, 0f, -hz + 0.5f), steel, white);    // en el comedor

        // ── LA ENTREGA DEL PROVEEDOR en el muelle: la furgoneta y las cajas de verdura, fruta y leche ──
        Transform drop = new GameObject("Entrega de comida en el muelle").transform; drop.SetParent(fac, false); drop.localPosition = new Vector3(23.6f, 1.2f, 4f);
        for (int k = 0; k < 4; k++) Prop(drop, "Caja_de_verdura", new Vector3((k % 2) * 0.6f, 0.15f + (k / 2) * 0.3f, 0f), new Vector3(0.55f, 0.3f, 0.4f), k % 2 == 0 ? green : M("Naranja_fruta", 0.95f, 0.55f, 0.1f), false);
        Prop(drop, "Cajas_de_leche", new Vector3(-0.7f, 0.25f, 0f), new Vector3(0.4f, 0.5f, 0.4f), white, false);
        Transform van = new GameObject("Furgoneta del proveedor de comida (supuesto)").transform; van.SetParent(fac, false); van.localPosition = new Vector3(29f, 0f, 1.5f); van.localRotation = Quaternion.Euler(0f, 90f, 0f);
        Prop(van, "Caja", new Vector3(0f, 1.2f, -0.4f), new Vector3(2f, 2f, 4f), white, true);
        Prop(van, "Cabina", new Vector3(0f, 0.95f, 2.1f), new Vector3(1.9f, 1.5f, 1.3f), white, true);
        Prop(van, "Rotulo_del_proveedor", new Vector3(1.01f, 1.4f, -0.4f), new Vector3(0.01f, 0.6f, 2.6f), green, false);
        foreach (var (x, z) in new[] { (-0.9f, 2.1f), (0.9f, 2.1f), (-0.9f, -1.5f), (0.9f, -1.5f) }) Prop(van, "Rueda", new Vector3(x, 0.35f, z), new Vector3(0.25f, 0.7f, 0.7f), black, false);

        // ── LA NAVE ANEXA (supuesto de sitio, 20 × 12 m, cerrada): FSW de los tanques del Falcon 9, laboratorio de materiales y datos de pruebas ──
        Transform annex = new GameObject("Nave anexa (tanques del Falcon 9, materiales y datos de pruebas)").transform; annex.SetParent(t, false); annex.localPosition = F(38f, -8f);   // (de la historia, no de la nave: su propia vista de corte)
        Prop(annex, "Losa_de_la_nave_anexa", new Vector3(0f, 0.04f, 0f), new Vector3(20f, 0.08f, 12f), M("Hormigon_pulido", 0.74f, 0.74f, 0.72f), false);
        CivicBuilder.Enclose(annex, new Vector2(20f, 12f), 7f, new Color(0.9f, 0.91f, 0.92f), new Color(0.62f, 0.64f, 0.66f), CivicBuilder.Roof.Flat, 3.6f, 4f, "supuesto: puerta de nave de 3,6 m");
        // La FSW: el anillo del tanque (3,66 m, el diámetro del Falcon 9), el útil de sujeción y el pórtico con el cabezal de pasador retráctil.
        Transform fsw = new GameObject("Soldadura por fricción (FSW): tanque del Falcon 9").transform; fsw.SetParent(annex, false); fsw.localPosition = new Vector3(-4.5f, 0f, 0.5f);
        Prop(fsw, "Base_del_util", new Vector3(0f, 0.3f, 0f), new Vector3(4.4f, 0.6f, 4.4f), steel, true);
        Prop(fsw, "Virola_del_tanque (aluminio-litio)", new Vector3(0f, 2.1f, 0f), new Vector3(3.66f, 3f, 3.66f), alu, true);
        for (int s = -1; s <= 1; s += 2) Prop(fsw, "Columna_del_portico", new Vector3(s * 2.6f, 2.5f, 0f), new Vector3(0.35f, 5f, 0.35f), yellow, true);
        Prop(fsw, "Viga_del_portico", new Vector3(0f, 5f, 0f), new Vector3(5.6f, 0.4f, 0.4f), yellow, false);
        Prop(fsw, "Cabezal_FSW (pasador retráctil, Nova-Tech)", new Vector3(1.95f, 3.6f, 0f), new Vector3(0.5f, 0.8f, 0.5f), dark, false);
        Prop(fsw, "Armario_de_control_FSW", new Vector3(-3.2f, 1f, 2.4f), new Vector3(0.9f, 2f, 0.6f), M("Gris_armario", 0.45f, 0.47f, 0.5f), false);
        Prop(fsw, "Cordon_de_soldadura", new Vector3(0f, 3.0f, -1.84f), new Vector3(3.2f, 0.04f, 0.01f), M("Cordon_brillante", 0.92f, 0.92f, 0.95f), false);
        // El laboratorio de materiales: la máquina de tracción, las probetas, el microscopio y el durómetro.
        Transform lab = new GameObject("Laboratorio de materiales").transform; lab.SetParent(annex, false); lab.localPosition = new Vector3(5f, 0f, 2.6f);
        Prop(lab, "Maquina_de_traccion (bastidor)", new Vector3(-1.5f, 1.1f, 0f), new Vector3(1f, 2.2f, 0.6f), M("Azul_maquina", 0.2f, 0.3f, 0.55f), true);
        Prop(lab, "Probeta_en_las_mordazas", new Vector3(-1.5f, 1.1f, -0.31f), new Vector3(0.04f, 0.3f, 0.01f), alu, false);
        Prop(lab, "Mesa_del_laboratorio", new Vector3(1.2f, 0.45f, 0f), new Vector3(2.2f, 0.9f, 0.75f), M("Blanco_lab", 0.9f, 0.9f, 0.88f), true);
        Prop(lab, "Microscopio", new Vector3(0.6f, 1.1f, 0f), new Vector3(0.25f, 0.4f, 0.3f), black, false);
        Prop(lab, "Durometro", new Vector3(1.5f, 1.05f, 0f), new Vector3(0.3f, 0.3f, 0.35f), M("Gris_armario", 0.45f, 0.47f, 0.5f), false);
        for (int k = 0; k < 6; k++) Prop(lab, "Probeta", new Vector3(1.9f + (k % 3) * 0.08f, 0.92f, -0.2f + (k / 3) * 0.1f), new Vector3(0.03f, 0.02f, 0.25f), alu, false);
        // La sala de datos de pruebas: el escritorio con dos pantallas, la impresora y las gráficas de McGregor en el corcho.
        Transform data = new GameObject("Sala de datos de pruebas (propulsión)").transform; data.SetParent(annex, false); data.localPosition = new Vector3(5f, 0f, -3.2f);
        Prop(data, "Escritorio", new Vector3(0f, 0.38f, 0f), new Vector3(2f, 0.76f, 0.8f), M("Gris_mesa", 0.55f, 0.56f, 0.58f), true);
        for (int s = -1; s <= 1; s += 2) Prop(data, "Pantalla_de_datos", new Vector3(s * 0.45f, 1.05f, 0.25f), new Vector3(0.55f, 0.35f, 0.04f), M("Grafica_azul", 0.25f, 0.55f, 0.85f), false);
        Prop(data, "Impresora", new Vector3(1.5f, 0.4f, 0f), new Vector3(0.5f, 0.8f, 0.45f), M("Beige_instrumento", 0.78f, 0.75f, 0.66f), true);
        Prop(data, "Corcho_con_graficas", new Vector3(0f, 1.6f, -2.6f), new Vector3(2.4f, 1.1f, 0.04f), M("Corcho", 0.7f, 0.55f, 0.35f), false);
        for (int k = 0; k < 4; k++) Prop(data, "Grafica_de_McGregor", new Vector3(-0.8f + k * 0.55f, 1.6f, -2.57f), new Vector3(0.42f, 0.55f, 0.01f), white, false);
        Sign(annex, "DATOS DE PRUEBAS · McGREGOR", new Vector3(5f, 3.2f, -1.2f), 0f, blue, white);
        Sign(annex, "SOLDADURA FSW · FALCON 9", new Vector3(-4.5f, 5.8f, -2.6f), 0f, blue, white);

        // ── LA SEGURIDAD: la garita de la entrada del aparcamiento, la barrera, el lector de tarjetas; el hidrante y el pozo de registro ──
        Transform gate = new GameObject("Garita de seguridad (1 Rocket Road)").transform; gate.SetParent(fac, false); gate.localPosition = new Vector3(-22.5f, 0f, -19.5f);
        Prop(gate, "Suelo_de_la_garita", new Vector3(0f, 0.1f, 0f), new Vector3(2.4f, 0.2f, 2f), M("Hormigon_isla", 0.72f, 0.71f, 0.68f), false);
        foreach (var (x, z, w, d) in new[] { (0f, 1f, 2.4f, 0.06f), (0f, -1f, 2.4f, 0.06f), (1.2f, 0f, 0.06f, 2f), (-1.2f, 0f, 0.06f, 2f) }) Prop(gate, "Cristal_de_la_garita", new Vector3(x, 1.4f, z), new Vector3(w, 1.6f, d), glass, false);
        Prop(gate, "Zocalo_de_la_garita", new Vector3(0f, 0.45f, 0f), new Vector3(2.4f, 0.7f, 2f), white, true);
        Prop(gate, "Tejadillo", new Vector3(0f, 2.35f, 0f), new Vector3(2.0f, 0.15f, 1.9f), white, false);
        Prop(gate, "Monitor_del_guardia", new Vector3(0.4f, 1.0f, 0.5f), new Vector3(0.4f, 0.3f, 0.05f), dark, false);
        Prop(gate, "Lector_de_tarjetas", new Vector3(-3.4f, 0.6f, 0.9f), new Vector3(0.15f, 1.2f, 0.15f), dark, true);
        Prop(gate, "Pantalla_del_lector", new Vector3(-3.4f, 1.1f, 0.82f), new Vector3(0.1f, 0.12f, 0.02f), M("Verde_DEA", 0.1f, 0.55f, 0.35f), false);
        Prop(gate, "Poste_de_la_barrera", new Vector3(-4.6f, 0.5f, 1.4f), new Vector3(0.35f, 1f, 0.35f), yellow, true);
        Prop(gate, "Barrera", new Vector3(-8.6f, 0.95f, 1.4f), new Vector3(8f, 0.08f, 0.08f), red, false);
        Label(gate, "Garita de seguridad (1 Rocket Road)", 2.8f);
        Prop(fac, "Hidrante", new Vector3(-20f, 0.45f, -hz - 1.8f), new Vector3(0.3f, 0.9f, 0.3f), yellow, true);
        Transform manhole = new GameObject("Pozo de registro (alcantarillado del condado)").transform; manhole.SetParent(fac, false); manhole.localPosition = new Vector3(-8f, 0.09f, -hz - 3f);
        Prop(manhole, "Tapa_de_fundicion", Vector3.zero, new Vector3(0.8f, 0.02f, 0.8f), M("Fundicion", 0.25f, 0.24f, 0.23f), false);
        Label(manhole, "Pozo de registro: al colector del condado (depura en Carson)", 0.6f);

        // ── LA LONCHERA, aparcada en la calle junto a la acera ──
        Transform truck = new GameObject("Lonchera (camión de comida)").transform; truck.SetParent(t, false); truck.localPosition = MuskShift + new Vector3(-39.5f, 0f, 2f);
        Prop(truck, "Caja_de_la_lonchera", new Vector3(0f, 1.6f, -0.6f), new Vector3(2.3f, 2.6f, 5f), M("Plata_lonchera", 0.82f, 0.82f, 0.84f), true);
        Prop(truck, "Cabina", new Vector3(0f, 1.1f, 2.6f), new Vector3(2.1f, 1.6f, 1.6f), white, true);
        Prop(truck, "Ventanilla", new Vector3(1.16f, 1.8f, -0.6f), new Vector3(0.02f, 0.9f, 2.4f), dark, false);
        Prop(truck, "Toldo", new Vector3(1.6f, 2.5f, -0.6f), new Vector3(0.9f, 0.05f, 2.8f), red, false);
        Prop(truck, "Menu", new Vector3(1.17f, 2.25f, 1.1f), new Vector3(0.02f, 0.5f, 0.8f), white, false);
        foreach (var (x, z) in new[] { (-1f, 2.6f), (1f, 2.6f), (-1f, -2.2f), (1f, -2.2f) }) Prop(truck, "Rueda", new Vector3(x, 0.4f, z), new Vector3(0.3f, 0.8f, 0.8f), black, false);
        Label(truck, "Lonchera (camión de comida, supuesto)", 3.2f);

        // ── LA GENTE QUE HACÍA LOS COHETES (y la que los cuidaba), cada una en su puesto ──
        Transform door = Spot(fac, "Entrada de la nave", new Vector3(0f, 0f, -hz - 1.5f), null);
        GameObject guard = Who(t, "Guardia de seguridad (Eolo)", F(-22.5f, -21.5f), false); CubeInsect.WearShirt(guard, new Color(0.1f, 0.12f, 0.2f)); Routine(guard, gate, door);
        GameObject welder = Who(t, "Soldador de FSW (Prometeo)", F(34f, -4.8f), false); CubeInsect.WearShirt(welder, new Color(0.35f, 0.33f, 0.3f)); Routine(welder, fsw);
        GameObject sci = Who(t, "Científica de materiales (Temis)", F(41f, -6.5f), true); CubeInsect.WearShirt(sci, white.color); Routine(sci, lab, data);
        GameObject testEng = Who(t, "Ingeniero de datos de pruebas (Ceo)", F(43f, -10f), false); CubeInsect.WearShirt(testEng, new Color(0.2f, 0.25f, 0.35f)); Routine(testEng, data, lab);
        GameObject cleaner = Who(t, "Limpiadora de la nave (Hestia)", F(-15f, -13f), true); CubeInsect.WearShirt(cleaner, new Color(0.3f, 0.5f, 0.55f)); Routine(cleaner, kwash, gown, cold);
        GameObject ship = Who(t, "Encargado de envíos (Hermes)", F(26f, 3f), false); CubeInsect.WearShirt(ship, new Color(0.6f, 0.45f, 0.2f)); Routine(ship, drop, van);
        GameObject cook = Who(t, "Cocinera de la lonchera (Deméter)", MuskShift + new Vector3(-37.5f, 0f, 1.5f), true); CubeInsect.WearShirt(cook, new Color(0.85f, 0.85f, 0.8f)); Routine(cook, truck);
        return 7;
    }

    // Un DISPENSADOR DE GEL de pie (alcohol al 60–70 %: lo que mata las bacterias de las manos), con su bandeja de goteo.
    static void Sanitizer(Transform parent, Vector3 at, Material steel, Material white)
    {
        Transform s = new GameObject("Dispensador de gel hidroalcohólico").transform; s.SetParent(parent, false); s.localPosition = at;
        Prop(s, "Pie", new Vector3(0f, 0.6f, 0f), new Vector3(0.06f, 1.2f, 0.06f), steel, false);
        Prop(s, "Dispensador", new Vector3(0f, 1.25f, 0f), new Vector3(0.14f, 0.25f, 0.12f), white, false);
        Prop(s, "Bandeja_de_goteo", new Vector3(0f, 1.05f, 0.04f), new Vector3(0.12f, 0.02f, 0.1f), steel, false);
    }
}
