using UnityEngine;

/// <summary>
/// ÁREA DE CONSTRUCCIÓN (docs/construction-simulation.md): el «hub de estructuras» que repara y levanta tuberías,
/// electricidad, puertas, paredes, ventanas y tejados en las áreas humanas. Aquí se GUARDAN la maquinaria y el material:
/// miniexcavadora y minicargadora eléctricas, hormigonera, grúa pluma pequeña, andamio, carretillas; palés de ladrillo,
/// sacos de cemento, madera, vigas de acero, tubos, bobinas de cable, tejas, puertas y ventanas de repuesto; panel de
/// herramientas, banco de trabajo y la pizarra de avisos (tickets de reparación). La dirige Tofelite («Tote»), el esposo de Lizmibet.
/// Y la colocación del personal nuevo en sus áreas (<see cref="PlaceStaff"/>).
/// </summary>
public static partial class SampleSceneBuilder
{
    static readonly Color CYellow = new Color(0.95f, 0.75f, 0.15f), CDark = new Color(0.2f, 0.2f, 0.22f), CSteel = new Color(0.6f, 0.62f, 0.66f),
        CWood = new Color(0.72f, 0.55f, 0.36f), CBrick = new Color(0.72f, 0.36f, 0.26f), CConcrete = new Color(0.7f, 0.7f, 0.68f), COrange = new Color(0.95f, 0.45f, 0.15f);

    static void FurnishConstruction(Transform area, Vector2 size)
    {
        Transform r = new GameObject("Construccion").transform; r.SetParent(area, false); r.localPosition = new Vector3(0f, 0.1f, 0f);
        // ALMACÉN DE MATERIAL (todo de plantas: guadua del bambusal, bahareque, micelio, cáñamo; docs/construccion-sostenible.md).
        Transform store = KitchenProps.Root(r, "Almacen_material", new Vector3(0f, 0f, 0f));
        store.gameObject.AddComponent<WorldLabel>().height = 3.6f;
        store.gameObject.AddComponent<MaterialStore>();
        float hx = size.x * 0.5f, hz = size.y * 0.5f;

        // ── Maquinaria (al fondo, −Z) ──
        Transform ex = CPiece(r, "Miniexcavadora", new Vector3(-hx + 1.07f, 0f, -hz + 2.4f), 20f);
        for (int s = -1; s <= 1; s += 2) KitchenProps.C(ex, new Vector3(s * 0.55f, 0.25f, 0f), new Vector3(0.3f, 0.5f, 1.8f), CDark, 0f, "Oruga");
        KitchenProps.C(ex, new Vector3(0f, 0.75f, 0f), new Vector3(1.2f, 0.5f, 1.4f), CYellow, 0f, "Chasis");
        KitchenProps.C(ex, new Vector3(-0.2f, 1.35f, -0.2f), new Vector3(0.8f, 0.8f, 0.8f), new Color(0.6f, 0.8f, 0.9f, 0.5f), 0f, "Cabina");
        KitchenProps.C(ex, new Vector3(0.35f, 1.4f, 0.9f), new Vector3(0.18f, 0.18f, 1.4f), CYellow, -25f, "Pluma").localRotation = Quaternion.Euler(-25f, 0f, 0f);
        KitchenProps.C(ex, new Vector3(0.35f, 1.2f, 1.75f), new Vector3(0.14f, 0.9f, 0.14f), CYellow, 0f, "Brazo");
        KitchenProps.C(ex, new Vector3(0.35f, 0.65f, 1.85f), new Vector3(0.5f, 0.3f, 0.35f), CDark, 0f, "Cazo");
        CLabel(ex, "Miniexcavadora (eléctrica)", 2.4f);

        Transform ld = CPiece(r, "Minicargadora", new Vector3(-hx + 4.94f, 0f, -hz + 2.2f), 0f);
        for (int i = 0; i < 4; i++) KitchenProps.C(ld, new Vector3(i % 2 == 0 ? -0.5f : 0.5f, 0.25f, i < 2 ? -0.45f : 0.45f), new Vector3(0.22f, 0.5f, 0.5f), CDark, 0f, "Rueda");
        KitchenProps.C(ld, new Vector3(0f, 0.7f, 0f), new Vector3(0.9f, 0.6f, 1.2f), CYellow, 0f, "Cuerpo");
        KitchenProps.C(ld, new Vector3(0f, 1.25f, -0.1f), new Vector3(0.8f, 0.6f, 0.8f), new Color(0.6f, 0.8f, 0.9f, 0.5f), 0f, "Cabina");
        KitchenProps.C(ld, new Vector3(0f, 0.35f, 0.95f), new Vector3(1.2f, 0.4f, 0.4f), CDark, 0f, "Pala");
        CLabel(ld, "Minicargadora (eléctrica)", 2f);

        Transform mx = CPiece(r, "Hormigonera", new Vector3(-hx + 8.25f, 0f, -hz + 2f), 0f);
        KitchenProps.C(mx, new Vector3(0f, 0.3f, 0f), new Vector3(0.8f, 0.1f, 0.6f), CDark, 0f, "Bastidor");
        KitchenProps.C(mx, new Vector3(0f, 0.85f, 0f), new Vector3(0.75f, 0.75f, 0.75f), COrange, 45f, "Cuba").localRotation = Quaternion.Euler(35f, 45f, 0f);
        KitchenProps.C(mx, new Vector3(0f, 1.25f, 0.25f), new Vector3(0.35f, 0.2f, 0.35f), COrange, 0f, "Boca");
        for (int s = -1; s <= 1; s += 2) KitchenProps.C(mx, new Vector3(s * 0.4f, 0.15f, 0f), new Vector3(0.08f, 0.3f, 0.3f), CDark, 0f, "Rueda");
        CLabel(mx, "Hormigonera", 1.6f);

        Transform crane = CPiece(r, "Grua_pluma", new Vector3(hx - 2.4f, 0f, -hz + 1.5f), 0f);
        KitchenProps.C(crane, new Vector3(0f, 0.2f, 0f), new Vector3(1.2f, 0.4f, 1.2f), CConcrete, 0f, "Base");
        for (int i = 0; i < 4; i++) KitchenProps.C(crane, new Vector3(i % 2 == 0 ? -0.2f : 0.2f, 3f, i < 2 ? -0.2f : 0.2f), new Vector3(0.06f, 5.2f, 0.06f), CYellow, 0f, "Mastil");
        for (int k = 0; k < 9; k++) KitchenProps.C(crane, new Vector3(0f, 0.8f + k * 0.55f, 0f), new Vector3(0.44f, 0.04f, 0.44f), CYellow, 0f, "Celosia");
        KitchenProps.C(crane, new Vector3(-1.8f, 5.6f, 0f), new Vector3(4.6f, 0.25f, 0.3f), CYellow, 0f, "Pluma");
        KitchenProps.C(crane, new Vector3(1.0f, 5.5f, 0f), new Vector3(0.6f, 0.5f, 0.5f), CConcrete, 0f, "Contrapeso");
        KitchenProps.C(crane, new Vector3(-3.4f, 4.4f, 0f), new Vector3(0.02f, 2.2f, 0.02f), CDark, 0f, "Cable");
        KitchenProps.C(crane, new Vector3(-3.4f, 3.25f, 0f), new Vector3(0.15f, 0.15f, 0.1f), COrange, 0f, "Gancho");
        CLabel(crane, "Grúa pluma", 6.2f);

        // ── Material (lado oeste, −X): palés y pilas EN LÍNEA contra la pared, un solo tramo (van pegados: FurnitureLine) que
        // guarda sus 2 m con lo demás. ──
        Transform west = new GameObject("Material_oeste").transform; west.SetParent(r, false); west.gameObject.AddComponent<FurnitureLine>();
        Transform bricks = CPiece(west, "Ladrillos", new Vector3(-hx + 2.4f, 0f, 2.85f), 0f);
        KitchenProps.C(bricks, new Vector3(0f, 0.06f, 0f), new Vector3(1.1f, 0.12f, 1.1f), CWood, 0f, "Pale");
        for (int y = 0; y < 5; y++) for (int i = 0; i < 4; i++)
            KitchenProps.C(bricks, new Vector3(-0.38f + i * 0.25f, 0.18f + y * 0.1f, 0f), new Vector3(0.22f, 0.09f, 1.0f), Color.Lerp(CBrick, CDark, (i + y) % 3 * 0.06f), 0f, "Ladrillo");
        CLabel(bricks, "Adobe (tierra y paja)", 1f);
        Transform cement = CPiece(west, "Cemento", new Vector3(-hx + 2.4f, 0f, 4.15f), 0f);
        KitchenProps.C(cement, new Vector3(0f, 0.06f, 0f), new Vector3(1.1f, 0.12f, 1.1f), CWood, 0f, "Pale");
        for (int y = 0; y < 4; y++) for (int i = 0; i < 2; i++)
            KitchenProps.C(cement, new Vector3(-0.25f + i * 0.5f, 0.2f + y * 0.15f, 0f), new Vector3(0.45f, 0.14f, 0.95f), new Color(0.86f, 0.84f, 0.78f), (y % 2) * 90f, "Saco");
        CLabel(cement, "Paneles de micelio", 0.9f);
        Transform wood = CPiece(west, "Madera", new Vector3(-hx + 0.85f, 0f, 4.6f), 0f);
        for (int y = 0; y < 4; y++) for (int i = 0; i < 6; i++)
            KitchenProps.C(wood, new Vector3(-0.5f + i * 0.2f, 0.1f + y * 0.12f, 0f), new Vector3(0.18f, 0.1f, 3f), Color.Lerp(CWood, Color.white, (i % 2) * 0.1f), 0f, "Tablon");
        CLabel(wood, "Guadua curada (bambú)", 0.8f);
        Transform steel = CPiece(west, "Vigas_acero", new Vector3(-hx + 0.85f, 0f, 1.2f), 0f);
        for (int i = 0; i < 4; i++)
        {
            float x = -0.6f + i * 0.4f;
            KitchenProps.C(steel, new Vector3(x, 0.12f, 0f), new Vector3(0.25f, 0.025f, 3.4f), CSteel, 0f, "Ala");
            KitchenProps.C(steel, new Vector3(x, 0.24f, 0f), new Vector3(0.025f, 0.22f, 3.4f), CSteel, 0f, "Alma");
            KitchenProps.C(steel, new Vector3(x, 0.36f, 0f), new Vector3(0.25f, 0.025f, 3.4f), CSteel, 0f, "Ala");
        }
        CLabel(steel, "Vigas de guadua laminada", 0.8f);
        Transform pipes = CPiece(west, "Tuberias", new Vector3(-hx + 2.4f, 0f, 0.8f), 0f);
        KitchenProps.C(pipes, new Vector3(0f, 0.5f, -1.2f), new Vector3(1.4f, 1f, 0.08f), CDark, 0f, "Soporte");
        KitchenProps.C(pipes, new Vector3(0f, 0.5f, 1.2f), new Vector3(1.4f, 1f, 0.08f), CDark, 0f, "Soporte");
        for (int y = 0; y < 3; y++) for (int i = 0; i < 5; i++)
            KitchenProps.C(pipes, new Vector3(-0.5f + i * 0.25f, 0.25f + y * 0.3f, 0f), new Vector3(0.12f, 0.12f, 2.6f), y == 0 ? new Color(0.85f, 0.85f, 0.88f) : y == 1 ? new Color(0.75f, 0.45f, 0.25f) : CSteel, 0f, "Tubo");
        CLabel(pipes, "Tuberías (agua y calefacción)", 1.4f);

        // ── Repuestos y electricidad (lado este, +X), también en línea contra la pared ──
        Transform east = new GameObject("Material_este").transform; east.SetParent(r, false); east.gameObject.AddComponent<FurnitureLine>();
        Transform cables = CPiece(east, "Bobinas_cable", new Vector3(hx - 1.4f, 0f, 2.6f), 0f);
        for (int i = 0; i < 3; i++)
        {
            KitchenProps.C(cables, new Vector3(-0.5f + i * 0.5f, 0.35f, 0f), new Vector3(0.12f, 0.7f, 0.7f), CWood, 0f, "Carrete");
            KitchenProps.C(cables, new Vector3(-0.5f + i * 0.5f, 0.35f, 0f), new Vector3(0.3f, 0.45f, 0.45f), i == 0 ? Color.black : i == 1 ? new Color(0.15f, 0.35f, 0.85f) : new Color(0.85f, 0.2f, 0.15f), 0f, "Cable");
        }
        CLabel(cables, "Cable eléctrico", 1.1f);
        Transform spares = CPiece(east, "Repuestos", new Vector3(hx - 1.2f, 0f, 0.2f), -90f);
        for (int i = 0; i < 3; i++) KitchenProps.C(spares, new Vector3(-0.6f + i * 0.6f, 1.05f, 0f), new Vector3(0.08f, 2.1f, 0.95f), new Color(0.55f, 0.42f, 0.3f), 0f, "Puerta_repuesto");
        for (int i = 0; i < 2; i++)
        {
            KitchenProps.C(spares, new Vector3(1.2f + i * 0.4f, 0.75f, 0f), new Vector3(0.08f, 1.2f, 1.0f), CSteel, 0f, "Marco_ventana");
            KitchenProps.C(spares, new Vector3(1.2f + i * 0.4f, 0.75f, 0f), new Vector3(0.09f, 1.05f, 0.85f), new Color(0.65f, 0.85f, 0.95f, 0.5f), 0f, "Cristal");
        }
        CLabel(spares, "Puertas y ventanas de repuesto", 2.3f);
        Transform tiles = CPiece(east, "Tejas", new Vector3(hx - 1.4f, 0f, -1.8f), 0f);
        KitchenProps.C(tiles, new Vector3(0f, 0.06f, 0f), new Vector3(1.1f, 0.12f, 1.1f), CWood, 0f, "Pale");
        for (int y = 0; y < 6; y++) KitchenProps.C(tiles, new Vector3(0f, 0.17f + y * 0.07f, 0f), new Vector3(1f, 0.06f, 1f), new Color(0.55f, 0.66f, 0.75f), 0f, "Teja");
        CLabel(tiles, "Tejas", 0.9f);

        // ── Taller (en medio, delante de la maquinaria; el borde +Z queda para el área de alfombras): banco de trabajo, panel de herramientas, andamio, carretillas y la pizarra de avisos ──
        Transform bench = KitchenProps.CounterRun(r, "Banco_trabajo", new Vector3(-1.5f, 0f, 0.93f), 3f, 180f, 0.8f);
        KitchenProps.C(bench, new Vector3(-1.1f, KitchenProps.CounterTop + 0.08f, 0f), new Vector3(0.3f, 0.16f, 0.2f), new Color(0.2f, 0.4f, 0.85f), 0f, "Taladro");
        KitchenProps.C(bench, new Vector3(-0.6f, KitchenProps.CounterTop + 0.05f, 0f), new Vector3(0.35f, 0.1f, 0.2f), COrange, 0f, "Sierra_circular");
        KitchenProps.C(bench, new Vector3(0.3f, KitchenProps.CounterTop + 0.1f, 0.1f), new Vector3(0.2f, 0.2f, 0.15f), CDark, 0f, "Tornillo_banco");
        KitchenProps.C(bench, new Vector3(0.9f, KitchenProps.CounterTop + 0.02f, 0f), new Vector3(1f, 0.02f, 0.06f), CYellow, 0f, "Nivel");
        CLabel(bench, "Banco de trabajo", 1.4f);
        Transform board = CPiece(r, "Panel_herramientas", new Vector3(-1.5f, 0f, 1.48f), 180f);
        KitchenProps.C(board, new Vector3(0f, 1.6f, 0f), new Vector3(3f, 1.2f, 0.04f), new Color(0.75f, 0.68f, 0.55f), 0f, "Tablero_perforado");
        string[] tools = { "Martillo", "Llave", "Alicates", "Destornillador", "Serrucho", "Paleta", "Metro", "Llana" };
        for (int i = 0; i < tools.Length; i++)
            KitchenProps.C(board, new Vector3(-1.3f + i * 0.37f, 1.6f + (i % 2) * 0.25f - 0.12f, -0.04f), new Vector3(0.06f, 0.34f, 0.03f), i % 3 == 0 ? COrange : i % 3 == 1 ? CSteel : CDark, i * 7f - 20f, tools[i]);
        Transform scaf = CPiece(r, "Andamio", new Vector3(3.05f, 0f, 0.94f), 0f);
        for (int i = 0; i < 4; i++) KitchenProps.C(scaf, new Vector3(i % 2 == 0 ? -0.9f : 0.9f, 1.5f, i < 2 ? -0.4f : 0.4f), new Vector3(0.05f, 3f, 0.05f), CSteel, 0f, "Tubo");
        for (int y = 0; y < 2; y++) KitchenProps.C(scaf, new Vector3(0f, 1.4f + y * 1.4f, 0f), new Vector3(1.9f, 0.05f, 0.85f), CWood, 0f, "Plataforma");
        for (int s = -1; s <= 1; s += 2) KitchenProps.C(scaf, new Vector3(0f, 0.75f, s * 0.42f), new Vector3(2.2f, 0.04f, 0.04f), CSteel, 0f, "Diagonal").localRotation = Quaternion.Euler(0f, 0f, 35f);
        CLabel(scaf, "Andamio", 3.2f);
        for (int k = 0; k < 2; k++)
        {
            Transform wb = CPiece(r, $"Carretilla_{k + 1}", new Vector3(5.4f, 0f, -0.2f + k * 1.1f), 90f);
            KitchenProps.C(wb, new Vector3(0f, 0.45f, 0.1f), new Vector3(0.6f, 0.3f, 0.8f), new Color(0.2f, 0.55f, 0.3f), 0f, "Cajon");
            KitchenProps.C(wb, new Vector3(0f, 0.2f, 0.55f), new Vector3(0.1f, 0.35f, 0.35f), CDark, 0f, "Rueda");
            for (int s = -1; s <= 1; s += 2) KitchenProps.C(wb, new Vector3(s * 0.25f, 0.45f, -0.55f), new Vector3(0.04f, 0.04f, 0.7f), CSteel, 0f, "Mango");
        }
        // Conos y casco del jefe de obra.
        for (int i = 0; i < 3; i++) KitchenProps.C(r, new Vector3(-hx + 0.6f + i * 0.6f, 0.2f, hz - 0.8f), new Vector3(0.25f, 0.4f, 0.25f), COrange, 45f, "Cono");
        KitchenProps.C(r, new Vector3(-0.3f, KitchenProps.CounterTop + 0.08f, 0.93f), new Vector3(0.28f, 0.14f, 0.3f), CYellow, 0f, "Casco");
    }

    static Transform CPiece(Transform parent, string name, Vector3 pos, float yaw)
    {
        Transform t = KitchenProps.Root(parent, name, pos, yaw);
        return t;
    }

    static void CLabel(Transform t, string text, float height)
    {
        KitchenProps.Fit(t);
        WorldLabel wl = t.GetComponent<WorldLabel>(); if (wl == null) wl = t.gameObject.AddComponent<WorldLabel>();
        wl.text = text; wl.height = height;
    }

    /// <summary>Lleva a cada miembro del personal nuevo a su área.</summary>
    static void PlaceStaff()
    {
        void Put(string who, string area, Vector3 local)
        {
            GameObject g = GameObject.Find(who), a = GameObject.Find(area);
            if (g != null && a != null) g.transform.position = a.transform.TransformPoint(local);
        }
        Put("Irosene", "YogaRoom_Area", new Vector3(9f, 1.5f, 0f));              // MAESTRA DE YOGA (2026-11-04; antes Andrea y Sadhguru): en la tarima de la sala de Hatha tradicional, junto al linga
        Put("Andrea", "Gym_Area", new Vector3(3f, 1f, 3f));                      // ENTRENADOR DEL GIMNASIO (2026-11-04: «el personaje con más fuerza es Andrea»)
        Put("JefeConstruccion", "Construction_Area", new Vector3(-1.5f, 1f, -0.3f)); // junto al banco de trabajo y la pizarra de avisos
        // La familia y los amigos de Berón (2026-11-04; docs/diseno-personajes.md §Familia y amigos). Los que heredan un papel de los
        // famosos que se fueron a sus microcosmos salen en su área (docs/personajes-mesocosmos.md §Reparto de papeles); los demás,
        // repartidos (la jornada los lleva a todas las áreas). La pareja (Amahuz y Karen) sale junta.
        Put("Nesvitor", "Kitchen_Area", new Vector3(-3f, 1f, 9f));                 // las cuentas del santuario, desde el restaurante
        Put("Ducristin", "VehicleWorkshop_Area", new Vector3(2f, 1f, 2f));         // la mecánica: vehículos y maquinaria
        Put("Maite", "Infirmary_Area", new Vector3(3f, 1f, -3f));                  // registros y vacunación del hospital
        Put("Alicita", "Garden_Area", new Vector3(2f, 1f, 3f));                    // el huerto: alimentar a los visitantes
        Put("Josedrilo", "Construction_Area", new Vector3(3f, 1f, 2f));            // la guardia, con Gohageneis (la torre está junto a las obras)
        Put("Amahuz", "TextileStudio_Area", new Vector3(2f, 1f, 2f));
        Put("Karen", "TextileStudio_Area", new Vector3(3f, 1f, 2f));
        Put("Shalva", "Gym_Area", new Vector3(-3f, 1f, 3f));
        Put("Adaguchi", "CubCare_Area", new Vector3(0f, 1f, 2f));
    }

    /// <summary>LA FAMILIA Y LOS AMIGOS DE BERÓN (2026-11-04, de sus fotos; docs/diseno-personajes.md §Familia y amigos): id, rótulo,
    /// x de salida, ropa y stats (fuerza, agilidad, resistencia, masa). Personas reales: stats cercanas a 1, sin suponer edades;
    /// Nesvitor, mayor (lo dice su foto), algo menos ágil. El rótulo lleva el papel heredado de los famosos, si lo tiene.</summary>
    public static readonly (string id, string label, float x, Color clothes, float s, float ag, float en, float m)[] FamilyCharacters =
    {
        ("Nesvitor", "Nesvitor · Néstor (cuentas del santuario)", 30f, new Color(0.1f, 0.1f, 0.11f), 0.95f, 0.9f, 1.05f, 1.0f),
        ("Amahuz", "Amahuz", 32f, new Color(0.1f, 0.12f, 0.22f), 1.0f, 1.05f, 1.0f, 0.9f),
        ("Karen", "Karen", 34f, new Color(0.97f, 0.96f, 0.95f), 0.9f, 1.05f, 1.0f, 0.95f),
        ("Alicita", "Alicita · Alicia (huerto: alimentar a los visitantes)", 36f, new Color(0.12f, 0.13f, 0.15f), 0.9f, 1.0f, 1.05f, 0.95f),
        ("Ducristin", "Ducristin (mecánica: vehículos y maquinaria)", 38f, new Color(0.6f, 0.78f, 0.95f), 1.0f, 1.0f, 1.0f, 1.05f),
        ("Josedrilo", "Josedrilo (guardia)", 40f, new Color(0.1f, 0.09f, 0.09f), 1.05f, 1.0f, 1.0f, 1.0f),
        ("Maite", "Maite (hospital: registros y vacunación)", 42f, new Color(0.36f, 0.33f, 0.24f), 0.9f, 1.05f, 1.0f, 0.95f),
        ("Shalva", "Shalva", 44f, new Color(0.3f, 0.33f, 0.15f), 1.05f, 1.0f, 1.05f, 1.0f),
        ("Adaguchi", "Adaguchi", 46f, new Color(0.08f, 0.08f, 0.09f), 1.05f, 1.0f, 1.0f, 1.05f),
    };

    /// <summary>LOS FAMOSOS QUE SE FUERON A SUS MICROCOSMOS (Berón 2026-11-04: «los personajes famosos, elon musk, sadhguru, y demás
    /// sácalos del mesocosmos y crea sus microcosmos respectivos»). YA NO SE CONSTRUYEN EN EL SANTUARIO: quedan aquí sus datos (id,
    /// rótulo, ropa y stats: fuerza, agilidad, resistencia, masa) para sus microcosmos; su aspecto sigue en CubeHair.For (por el id).
    /// Sus papeles en el santuario los llevan otros: docs/personajes-mesocosmos.md §Reparto de papeles.</summary>
    public static readonly (string id, string label, float x, Color clothes, float s, float ag, float en, float m)[] MesoCharacters =
    {
        ("Sadhguru", "Sadhguru (maestro de yoga)", 14f, new Color(0.5f, 0.72f, 0.62f), 1.0f, 1.2f, 1.4f, 1.0f),
        ("ElonMusk", "Elon Musk (vehículos eléctricos y cohetes)", 18f, new Color(0.08f, 0.08f, 0.09f), 1.05f, 0.95f, 1.0f, 1.1f),
        ("Goku", "Goku (artes marciales)", 20f, new Color(0.95f, 0.47f, 0.12f), 1.6f, 1.5f, 1.6f, 1.1f),
        ("BillGates", "Bill Gates (software y salud pública)", 22f, new Color(0.22f, 0.32f, 0.55f), 0.95f, 0.95f, 1.0f, 1.0f),
        ("WarrenBuffett", "Warren Buffett (inversor)", 24f, new Color(0.42f, 0.43f, 0.46f), 0.85f, 0.8f, 0.9f, 1.05f),
        ("MacKenzieScott", "MacKenzie Scott (novelista y filántropa)", 26f, new Color(0.62f, 0.75f, 0.88f), 0.95f, 1.05f, 1.05f, 0.95f),
        ("IbrahimTraore", "Ibrahim Traoré (capitán)", 28f, new Color(0.33f, 0.36f, 0.24f), 1.25f, 1.2f, 1.3f, 1.05f),
    };
}
