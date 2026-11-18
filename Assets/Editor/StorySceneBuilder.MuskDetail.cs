using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EL DETALLE DEL PILOTO (Microcosmos_ElonMusk, ronda piloto 2 del 2026-11-18: «se lee vacío y de bloques»). Con las reglas de detalle de
/// CodeShip (reglas.md §1 y §4.5: cada equipo con su forma real y su función, nada de cubos lisos, la escena llena como el lugar real):
/// - DENTRO: suelo de hormigón pulido con sus juntas y las MARCAS DE SEGURIDAD amarillas (pasillos y el área de la grúa); la GRÚA PUENTE
///   de la nave de montaje; ESTANTERÍAS de utillaje y de piezas en los muros; palés con secciones de tanque; el BANCO DE AVIÓNICA (cajas de
///   vuelo y osciloscopio) y el BANCO DE COMPOSITE (las dos mitades de la cofia); más PUESTOS en la planta de cubículos con sus mamparas; la
///   PIZARRA BLANCA con la secuencia del vuelo 4; la BANDERA de EE. UU. colgada del muro; la MESA ALTA del comedor con apoyos de abdomen,
///   la máquina de bebidas y la fuente de agua; y gente trabajando en cada banco.
/// - FUERA: la chapa ondulada (nervios verticales), las ventanas de las oficinas, los TRAGALUCES de la cubierta, el MUELLE DE CARGA con sus
///   dos puertas enrollables y un camión de reparto, el aparcamiento LLENO de coches de 2008, la calle con su acera y las PALMERAS de abanico.
/// Todo lo que no es mueble (MicroPiece) es decorado sin colisión o pegado a los muros, para no cerrar el paso (regla 1); los muebles
/// nuevos (bancos, puestos, pizarra, mesa alta) guardan los 2,4 m. Las piezas altas de la grúa van en tramos de menos de 4 m² para que la
/// vista de corte no las tome por techo (MicroBuildRules.IsRoof); los tragaluces SÍ son techo y se desvanecen con la cubierta.
/// FUENTES (además de las de StorySceneBuilder.Musk): Wired, «The Rocket Factory» (la nave blanca, los bancos de aviónica y composite a la
/// vista); Satellite Today, 24-11-2008 (la planta abierta y los turnos de 24 h). SUPUESTOS: la grúa, las estanterías, las marcas, el
/// camión, los coches (tipos comunes en Los Ángeles en 2008: sedán, todoterreno, pickup), la altura de las palmeras (Washingtonia robusta,
/// 12 m) y la bandera.
/// </summary>
public static partial class StorySceneBuilder
{
    // Devuelve cuántos insectos-humanos añade (la gente que trabaja en los bancos nuevos).
    static int MuskDetail(Transform t, Transform fac)
    {
        Material steel = M("Acero", 0.7f, 0.72f, 0.75f), dark = M("Pantalla", 0.08f, 0.1f, 0.14f), white = M("Fuselaje_blanco", 0.94f, 0.94f, 0.95f);
        Material yellow = M("Amarillo_seguridad", 0.95f, 0.78f, 0.1f), black = M("Negro_mate", 0.07f, 0.07f, 0.08f), joint = M("Junta_de_hormigon", 0.5f, 0.5f, 0.49f);
        Material blueRack = M("Azul_estanteria", 0.15f, 0.3f, 0.6f), orange = M("Naranja_estanteria", 0.9f, 0.45f, 0.1f), cardboard = M("Carton", 0.68f, 0.52f, 0.34f);
        Material wood = M("Palé", 0.7f, 0.56f, 0.38f), carbon = M("Fibra_de_carbono", 0.15f, 0.15f, 0.16f), green = M("Placa_verde", 0.2f, 0.45f, 0.25f);
        Material glassDark = M("Vidrio_oscuro", 0.18f, 0.24f, 0.3f), sky = M("Tragaluz", 0.75f, 0.85f, 0.9f), rib = M("Chapa_ondulada", 0.82f, 0.83f, 0.85f);
        Material concrete = M("Hormigon_muelle", 0.6f, 0.6f, 0.58f), rubber = M("Caucho", 0.1f, 0.1f, 0.1f), red = M("Rojo_bandera", 0.7f, 0.12f, 0.14f);
        Vector2 size = MuskFactorySize; float hx = size.x * 0.5f, hz = size.y * 0.5f, wallH = 9f;

        // ── EL SUELO: hormigón pulido (más claro), sus juntas cada 6 m y las marcas amarillas ──
        Transform floor = fac.Find("Suelo_de_epoxi");
        if (floor != null) { Renderer fr = floor.GetComponent<Renderer>(); if (fr != null) fr.sharedMaterial = M("Hormigon_pulido", 0.74f, 0.74f, 0.72f); }
        for (float x = -hx + 6f; x < hx; x += 6f) Prop(fac, "Junta", new Vector3(x, 0.082f, 0f), new Vector3(0.03f, 0.004f, size.y - 0.6f), joint, false);
        for (float z = -hz + 6f; z < hz; z += 6f) Prop(fac, "Junta", new Vector3(0f, 0.082f, z), new Vector3(size.x - 0.6f, 0.004f, 0.03f), joint, false);
        foreach (float x in new[] { -2.2f, 2.2f }) Prop(fac, "Marca_de_pasillo", new Vector3(x, 0.085f, -7f), new Vector3(0.1f, 0.006f, 16f), yellow, false);   // el pasillo de la puerta
        Prop(fac, "Marca_de_pasillo", new Vector3(-12f, 0.085f, 1.5f), new Vector3(19.5f, 0.006f, 0.1f), yellow, false);
        // El área de la grúa alrededor del Falcon 1: un recuadro amarillo con franjas negras en las esquinas.
        foreach (float z in new[] { 6.6f, 13.1f }) Prop(fac, "Marca_area_de_grua", new Vector3(-11f, 0.085f, z), new Vector3(17.4f, 0.006f, 0.12f), yellow, false);
        foreach (float x in new[] { -19.7f, -2.3f }) Prop(fac, "Marca_area_de_grua", new Vector3(x, 0.085f, 9.85f), new Vector3(0.12f, 0.006f, 6.5f), yellow, false);
        for (int k = 0; k < 6; k++) Prop(fac, "Franja_negra", new Vector3(-19.2f + k * 0.35f, 0.087f, 6.6f), new Vector3(0.15f, 0.006f, 0.12f), black, false);

        // ── LA GRÚA PUENTE de la nave de montaje (supuesto): dos vigas carril en tramos, el puente amarillo y el gancho ──
        foreach (float z in new[] { 5.6f, 13.6f })
        {
            foreach (float x in new[] { -19.6f, -1.6f }) Prop(fac, "Pilar_de_la_grua", new Vector3(x, 3.6f, z), new Vector3(0.35f, 7.2f, 0.35f), yellow, true);
            for (int k = 0; k < 3; k++) Prop(fac, "Viga_carril", new Vector3(-16.6f + k * 6f, 7.3f, z), new Vector3(5.95f, 0.45f, 0.3f), yellow, false);
        }
        for (int k = 0; k < 2; k++) Prop(fac, "Puente_de_la_grua", new Vector3(-11f, 7.75f, 7.6f + k * 4f), new Vector3(0.45f, 0.5f, 3.95f), yellow, false);
        Prop(fac, "Carro_del_polipasto", new Vector3(-11f, 7.4f, 9.6f), new Vector3(0.8f, 0.5f, 0.8f), dark, false);
        Prop(fac, "Cable_del_polipasto", new Vector3(-11f, 5.4f, 9.6f), new Vector3(0.04f, 3.6f, 0.04f), steel, false);
        Prop(fac, "Gancho", new Vector3(-11f, 3.45f, 9.6f), new Vector3(0.3f, 0.35f, 0.15f), yellow, false);
        Prop(fac, "Eslinga_de_la_etapa", new Vector3(-11f, 2.6f, 9f), new Vector3(0.05f, 1.6f, 0.05f), orange, false);

        // ── ESTANTERÍAS (supuesto): de utillaje en el muro oeste y de piezas en el muro norte ──
        foreach (float z in new[] { -2.5f, 1f }) Rack(fac, new Vector3(-hx + 0.65f, 0f, z), 90f, 3f, 2.6f, 0.7f, blueRack, orange, cardboard);
        for (int k = 0; k < 4; k++) Rack(fac, new Vector3(-19f + k * 4f, 0f, hz - 0.65f), 0f, 3.4f, 3.2f, 0.7f, blueRack, orange, cardboard);
        // Palés con secciones de tanque (aluminio-litio, blancas) y cajas.
        Pallet(fac, new Vector3(-7f, 0f, -6.5f), wood, white, true);
        Pallet(fac, new Vector3(-20.8f, 0f, -8.5f), wood, cardboard, false);

        // ── EL BANCO DE AVIÓNICA (cajas de vuelo, osciloscopio, cableado) y el de COMPOSITE (las dos mitades de la cofia) ──
        Transform avSpot = Spot(fac, "Banco de aviónica", new Vector3(0.5f, 0f, 10.6f), "Aviónica: las cajas de vuelo del Falcon 1", 2f);
        Transform av = Piece(fac, "banco_taller", new Vector3(0.5f, 0f, 12f), 0f, "Banco de aviónica", new Vector2(2f, 0.8f));
        Prop(av, "Caja_de_avionica", new Vector3(-0.5f, 1.02f, 0f), new Vector3(0.45f, 0.2f, 0.35f), dark, false);
        Prop(av, "Caja_de_avionica", new Vector3(0.05f, 1.02f, 0.05f), new Vector3(0.35f, 0.18f, 0.3f), M("Aluminio_anodizado", 0.55f, 0.58f, 0.62f), false);
        Prop(av, "Osciloscopio", new Vector3(0.6f, 1.08f, 0.15f), new Vector3(0.35f, 0.3f, 0.3f), M("Beige_instrumento", 0.78f, 0.75f, 0.66f), false);
        Prop(av, "Pantalla_del_osciloscopio", new Vector3(0.6f, 1.1f, -0.01f), new Vector3(0.2f, 0.15f, 0.01f), green, false);
        Prop(av, "Mazo_de_cables", new Vector3(-0.1f, 0.93f, -0.2f), new Vector3(0.9f, 0.04f, 0.06f), orange, false);
        WorkPlatform(fac, av, 0.9f, 2f, steel);
        Transform coSpot = Spot(fac, "Banco de composite", new Vector3(-3.5f, 0f, -5.4f), "Composite: la cofia de fibra de carbono", 2f);
        Transform co = Piece(fac, "banco_taller", new Vector3(-3.5f, 0f, -4f), 0f, "Banco de composite (la cofia)", new Vector2(2.4f, 1f));
        for (int s = -1; s <= 1; s += 2) Prop(co, "Mitad_de_la_cofia", new Vector3(s * 0.55f, 1.15f, 0f), new Vector3(0.5f, 0.45f, 1.0f), carbon, false);
        Prop(co, "Rollo_de_tejido_de_carbono", new Vector3(0f, 0.95f, 0.3f), new Vector3(0.9f, 0.12f, 0.12f), carbon, false);
        WorkPlatform(fac, co, 0.9f, 2.4f, steel);

        // ── MÁS PUESTOS EN LA PLANTA DE CUBÍCULOS, con sus mamparas, y la PIZARRA BLANCA ──
        foreach (var (x, z, label) in new[] { (6f, -2.8f, "Puesto de ingeniería (software de vuelo)"), (10.5f, -4.8f, "Puesto de ingeniería (propulsión)") })
        {
            Transform d = Piece(fac, "consola", new Vector3(x, 0f, z), 0f, label, new Vector2(1.6f, 0.7f));
            WorkPlatform(fac, d, 0.76f, 1.6f, steel);
            Prop(fac, "Mampara_del_cubiculo", new Vector3(x, 0.65f, z + 0.65f), new Vector3(2.2f, 1.3f, 0.06f), M("Mampara_gris", 0.55f, 0.57f, 0.6f), false);
        }
        Prop(fac, "Mampara_del_cubiculo", new Vector3(6f, 0.65f, -7.35f), new Vector3(2.2f, 1.3f, 0.06f), M("Mampara_gris", 0.55f, 0.57f, 0.6f), false);
        Transform wb = Piece(fac, "pizarra", new Vector3(9.5f, 0f, -12.5f), 0f, "Pizarra: la secuencia del vuelo 4", new Vector2(2f, 0.4f));
        Prop(wb, "Pizarra_blanca", new Vector3(0f, 1.0f, -0.03f), new Vector3(1.38f, 0.98f, 0.01f), M("Blanco_pizarra", 0.97f, 0.97f, 0.97f), false);
        for (int k = 0; k < 5; k++) Prop(wb, "Rotulador", new Vector3(-0.45f + (k % 2) * 0.2f, 1.3f - k * 0.13f, -0.04f), new Vector3(0.7f - k * 0.08f, 0.02f, 0.005f), k == 2 ? red : blueRack, false);

        // ── LA BANDERA de EE. UU. colgada del muro norte, sobre el control de misión (supuesto) ──
        Transform flag = new GameObject("Bandera de EE. UU. (muro norte)").transform; flag.SetParent(fac, false); flag.localPosition = new Vector3(12f, 6.2f, hz - 0.25f);
        for (int k = 0; k < 7; k++) Prop(flag, "Franja", new Vector3(0f, 1.1f - k * 0.3f, 0f), new Vector3(4.2f, 0.16f, 0.02f), red, false);
        for (int k = 0; k < 6; k++) Prop(flag, "Franja", new Vector3(0f, 0.95f - k * 0.3f, 0f), new Vector3(4.2f, 0.14f, 0.02f), white, false);
        Prop(flag, "Canton", new Vector3(-1.2f, 0.65f, -0.01f), new Vector3(1.7f, 1.0f, 0.02f), M("Bandera_azul", 0.15f, 0.2f, 0.45f), false);

        // ── EL COMEDOR: una MESA ALTA de insectos-humanos con sus apoyos de abdomen (C1), la máquina de bebidas y la fuente de agua ──
        MicroBody.Fit ant = MicroBody.OfSpecies("Ant");
        Piece(fac, "mesa", new Vector3(-4f, 0f, -11f), 0f, "Comedor: mesa alta con apoyos de abdomen (C1)", null, null, new List<MicroBody.Fit> { ant, ant, ant, ant });
        Prop(fac, "Maquina_de_bebidas", new Vector3(-13f, 0.9f, -hz + 0.6f), new Vector3(1f, 1.8f, 0.8f), red, false);
        Prop(fac, "Frontal_de_la_maquina", new Vector3(-13f, 1.1f, -hz + 1.01f), new Vector3(0.6f, 1.0f, 0.01f), glassDark, false);
        Prop(fac, "Fuente_de_agua", new Vector3(-11.2f, 0.55f, -hz + 0.45f), new Vector3(0.35f, 1.1f, 0.35f), white, false);
        Prop(fac, "Garrafa", new Vector3(-11.2f, 1.25f, -hz + 0.45f), new Vector3(0.28f, 0.35f, 0.28f), M("Agua_azul", 0.55f, 0.75f, 0.9f), false);

        // ── FUERA: la chapa ondulada, las ventanas, los tragaluces y el muelle de carga ──
        for (float x = -hx + 0.75f; x < hx; x += 1.5f)
        {
            if (Mathf.Abs(x) > 1.2f) Prop(fac, "Nervio_de_chapa", new Vector3(x, wallH * 0.5f, -hz - 0.05f), new Vector3(0.08f, wallH, 0.1f), rib, false);
            Prop(fac, "Nervio_de_chapa", new Vector3(x, wallH * 0.5f, hz + 0.05f), new Vector3(0.08f, wallH, 0.1f), rib, false);
        }
        for (float z = -hz + 0.75f; z < hz; z += 1.5f)
        {
            Prop(fac, "Nervio_de_chapa", new Vector3(-hx - 0.05f, wallH * 0.5f, z), new Vector3(0.1f, wallH, 0.08f), rib, false);
            if (z < 3.2f || z > 10.8f) Prop(fac, "Nervio_de_chapa", new Vector3(hx + 0.05f, wallH * 0.5f, z), new Vector3(0.1f, wallH, 0.08f), rib, false);
        }
        for (float x = -20f; x <= 20f; x += 3.4f)
        {
            if (Mathf.Abs(x) < 2.5f) continue;
            Prop(fac, "Ventana_de_oficina", new Vector3(x, 3.3f, -hz - 0.12f), new Vector3(2.4f, 1.5f, 0.04f), glassDark, false);
        }
        for (float z = -12f; z <= 12f; z += 4f) Prop(fac, "Ventana_de_oficina", new Vector3(-hx - 0.12f, 3.3f, z), new Vector3(0.04f, 1.5f, 2.4f), glassDark, false);
        for (int i = 0; i < 4; i++) for (int j = 0; j < 3; j++)
            Prop(fac, "Tragaluz", new Vector3(-15f + i * 10f, wallH + 0.33f, -8f + j * 8f), new Vector3(3f, 0.08f, 2f), sky, false);
        // El muelle de carga (este): el andén de hormigón, las dos puertas enrollables, los topes de caucho y un camión de reparto (supuesto).
        Prop(fac, "Anden_del_muelle", new Vector3(hx + 1.6f, 0.6f, 7f), new Vector3(3f, 1.2f, 8f), concrete, true);
        foreach (float z in new[] { 5f, 9f })
        {
            Prop(fac, "Puerta_enrollable", new Vector3(hx + 0.07f, 3.2f, z), new Vector3(0.06f, 4f, 3.2f), M("Chapa_puerta", 0.7f, 0.72f, 0.74f), false);
            for (int k = 0; k < 8; k++) Prop(fac, "Lama", new Vector3(hx + 0.11f, 1.45f + k * 0.5f, z), new Vector3(0.02f, 0.04f, 3.2f), steel, false);
            Prop(fac, "Tope_de_caucho", new Vector3(hx + 3.15f, 0.8f, z - 1f), new Vector3(0.2f, 0.4f, 0.3f), rubber, false);
            Prop(fac, "Tope_de_caucho", new Vector3(hx + 3.15f, 0.8f, z + 1f), new Vector3(0.2f, 0.4f, 0.3f), rubber, false);
        }
        Transform truck = new GameObject("Camión de reparto (2008, supuesto)").transform; truck.SetParent(fac, false); truck.localPosition = new Vector3(hx + 7.4f, 0f, 9f); truck.localRotation = Quaternion.Euler(0f, 90f, 0f);
        Prop(truck, "Caja", new Vector3(0f, 1.9f, -0.9f), new Vector3(2.3f, 2.3f, 5f), white, true);
        Prop(truck, "Cabina", new Vector3(0f, 1.3f, 2.5f), new Vector3(2.0f, 1.9f, 1.7f), M("Cabina_blanca", 0.88f, 0.88f, 0.9f), true);
        Prop(truck, "Parabrisas", new Vector3(0f, 1.75f, 3.36f), new Vector3(1.7f, 0.7f, 0.02f), glassDark, false);
        foreach (var (x, z) in new[] { (-1f, 2.5f), (1f, 2.5f), (-1f, -2f), (1f, -2f) }) Prop(truck, "Rueda", new Vector3(x, 0.4f, z), new Vector3(0.3f, 0.8f, 0.8f), rubber, false);

        // ── LAS PALMERAS de la fachada (lejos del portal, |x| ≥ 12) y la CALLE con su acera y su fila de palmeras, al oeste ──
        foreach (float x in new[] { -20f, -13f, 13f, 20f }) Palm(fac, new Vector3(x, 0f, -hz - 3.5f));
        Transform street = new GameObject("Calle junto a la nave (acera y palmeras)").transform; street.SetParent(t, false); street.localPosition = new Vector3(-42f, 0f, 8f);
        Prop(street, "Asfalto_de_la_calle", new Vector3(0f, 0.02f, 0f), new Vector3(9f, 0.04f, 44f), M("Asfalto", 0.2f, 0.2f, 0.21f), false);
        Prop(street, "Linea_central", new Vector3(0f, 0.045f, 0f), new Vector3(0.12f, 0.01f, 44f), yellow, false);
        Prop(street, "Acera", new Vector3(-6f, 0.08f, 0f), new Vector3(3f, 0.16f, 44f), M("Acera", 0.72f, 0.71f, 0.68f), false);
        for (int k = 0; k < 6; k++) Palm(street, new Vector3(-6.5f, 0.16f, -19f + k * 7.6f));

        // ── LA GENTE TRABAJANDO en los bancos nuevos ──
        Vector3 F(float x, float z) => MuskFactoryAt + new Vector3(x, 0f, z);
        Transform dataSpot = fac.Find("Consola de datos del vuelo 3"), floorSpot = fac.Find("Nave de montaje");
        GameObject av1 = Who(t, "Técnico de aviónica (Hiperión)", F(0.5f, 10.4f), false); CubeInsect.WearShirt(av1, new Color(0.25f, 0.3f, 0.4f)); Routine(av1, avSpot, dataSpot != null ? dataSpot : avSpot);
        GameObject co1 = Who(t, "Laminadora de composite (Calisto)", F(-3.5f, -5.4f), true); CubeInsect.WearShirt(co1, new Color(0.5f, 0.5f, 0.52f)); Routine(co1, coSpot);
        GameObject crane = Who(t, "Operario de la grúa (Deucalión)", F(-8f, 5f), false); CubeInsect.WearShirt(crane, new Color(0.85f, 0.6f, 0.15f)); Routine(crane, floorSpot != null ? floorSpot : avSpot, coSpot);
        return 3;
    }

    // Una ESTANTERÍA de palés (bastidores, largueros naranjas, baldas y cajas), de ancho w, alto h y fondo d, de espaldas al muro.
    static void Rack(Transform parent, Vector3 at, float yaw, float w, float h, float d, Material frame, Material beam, Material box)
    {
        Transform r = new GameObject("Estanteria").transform; r.SetParent(parent, false); r.localPosition = at; r.localRotation = Quaternion.Euler(0f, yaw, 0f);
        for (int s = -1; s <= 1; s += 2) Prop(r, "Bastidor", new Vector3(s * w * 0.5f, h * 0.5f, 0f), new Vector3(0.08f, h, d), frame, true);
        for (int k = 0; k < 3; k++)
        {
            float y = 0.15f + k * (h - 0.3f) / 2.5f;
            Prop(r, "Larguero", new Vector3(0f, y, 0f), new Vector3(w, 0.1f, d), beam, false);
            for (int b = 0; b < 3; b++) if ((k + b) % 3 != 2) Prop(r, "Caja", new Vector3(-w * 0.32f + b * w * 0.32f, y + 0.25f, 0f), new Vector3(w * 0.26f, 0.4f, d * 0.8f), box, false);
        }
    }

    // Un PALÉ con una sección de tanque (cilindro blanco) o con cajas.
    static void Pallet(Transform parent, Vector3 at, Material wood, Material load, bool tank)
    {
        Transform p = new GameObject(tank ? "Palé con una sección de tanque" : "Palé con cajas").transform; p.SetParent(parent, false); p.localPosition = at;
        Prop(p, "Palé", new Vector3(0f, 0.07f, 0f), new Vector3(1.2f, 0.14f, 1.0f), wood, true);
        if (tank) Prop(p, "Seccion_de_tanque", new Vector3(0f, 0.95f, 0f), new Vector3(1.6f, 1.6f, 1.6f), load, true);
        else for (int k = 0; k < 4; k++) Prop(p, "Caja", new Vector3((k % 2 - 0.5f) * 0.55f, 0.36f + (k / 2) * 0.42f, 0f), new Vector3(0.5f, 0.4f, 0.8f), load, false);
    }

    // Una PALMERA DE ABANICO (Washingtonia robusta, la de las avenidas de Los Ángeles; 12 m: supuesto): tronco, falda de hojas secas y copa.
    static void Palm(Transform parent, Vector3 at)
    {
        Transform p = new GameObject("Palmera de abanico").transform; p.SetParent(parent, false); p.localPosition = at;
        Prop(p, "Tronco", new Vector3(0f, 6f, 0f), new Vector3(0.45f, 12f, 0.45f), M("Tronco_palmera", 0.5f, 0.4f, 0.3f), true);
        Prop(p, "Falda_de_hojas_secas", new Vector3(0f, 11f, 0f), new Vector3(0.9f, 1.6f, 0.9f), M("Hoja_seca", 0.55f, 0.45f, 0.3f), false);
        Material leaf = M("Hoja_de_palmera", 0.25f, 0.45f, 0.22f);
        for (int k = 0; k < 7; k++)
        {
            float a = k * 360f / 7f;
            Transform f = AreaMicrocosmos.Box(p, "Hoja", new Vector3(0f, 12.1f, 0f), new Vector3(0.7f, 0.05f, 2.2f), Quaternion.Euler(-15f, a, 0f), leaf, false);
            f.localPosition += f.localRotation * new Vector3(0f, 0f, 1.0f);
        }
    }

    // EL APARCAMIENTO LLENO (coches de 2008 habituales en Los Ángeles: sedán ~4,8 × 1,8 × 1,47 m, todoterreno ~4,9 × 1,9 × 1,8, pickup
    // ~5,6 × 2,0 × 1,85; supuesto), en sus plazas (líneas cada 2,7 m) y dejando libre la del Roadster y alguna más.
    static void ParkedCars(Transform lot)
    {
        Color[] colors = { new Color(0.75f, 0.76f, 0.78f), new Color(0.1f, 0.1f, 0.12f), new Color(0.95f, 0.95f, 0.95f), new Color(0.2f, 0.25f, 0.45f), new Color(0.45f, 0.1f, 0.1f), new Color(0.5f, 0.5f, 0.48f) };
        Material glass = M("Vidrio_oscuro", 0.18f, 0.24f, 0.3f), tyre = M("Neumatico", 0.08f, 0.08f, 0.08f);
        int n = 0;
        for (int k = 0; k < 9; k++)
            for (int s = -1; s <= 1; s += 2)
            {
                if (s < 0 && k == 4) continue;   // la plaza del Roadster
                if ((k * 7 + s * 3 + 11) % 5 == 0) continue;   // alguna plaza libre
                int type = (k + (s > 0 ? 1 : 0)) % 3;
                float L = type == 0 ? 4.8f : type == 1 ? 4.9f : 5.6f, W = type == 0 ? 1.8f : type == 1 ? 1.9f : 2.0f, H = type == 0 ? 1.47f : type == 1 ? 1.8f : 1.85f;
                Material paint = M("Coche_" + n % colors.Length, colors[n % colors.Length].r, colors[n % colors.Length].g, colors[n % colors.Length].b);
                Transform c = new GameObject(type == 0 ? "Sedán aparcado" : type == 1 ? "Todoterreno aparcado" : "Pickup aparcada").transform;
                c.SetParent(lot, false); c.localPosition = new Vector3(s * 3.2f, 0f, -12f + (k + 0.5f) * 2.7f); c.localRotation = Quaternion.Euler(0f, s < 0 ? 90f : -90f, 0f);
                Prop(c, "Carroceria", new Vector3(0f, 0.3f + H * 0.25f, 0f), new Vector3(W, H * 0.5f, L), paint, true);
                if (type == 2)
                {
                    Prop(c, "Cabina", new Vector3(0f, H * 0.78f, L * 0.12f), new Vector3(W * 0.95f, H * 0.42f, L * 0.36f), paint, false);
                    Prop(c, "Caja_de_carga", new Vector3(0f, H * 0.62f, -L * 0.3f), new Vector3(W * 0.95f, 0.1f, L * 0.36f), tyre, false);
                }
                else Prop(c, "Cabina", new Vector3(0f, H * 0.78f, -L * 0.05f), new Vector3(W * 0.9f, H * 0.42f, L * (type == 1 ? 0.62f : 0.45f)), glass, false);
                foreach (var (x, z) in new[] { (-W * 0.45f, L * 0.32f), (W * 0.45f, L * 0.32f), (-W * 0.45f, -L * 0.32f), (W * 0.45f, -L * 0.32f) })
                    Prop(c, "Rueda", new Vector3(x, 0.33f, z), new Vector3(0.22f, 0.66f, 0.66f), tyre, false);
                n++;
            }
    }
}
