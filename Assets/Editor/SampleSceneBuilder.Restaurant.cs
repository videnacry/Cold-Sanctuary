using UnityEngine;

/// <summary>
/// RESTAURANTE GOURMET y ESTUDIO TEXTIL (partes del constructor de la escena).
///  · La cocina pasa a ser una COCINA ABIERTA de restaurante gourmet: suelo de pizarra oscura, pared de azulejo verde
///    botella con remates de latón, pase de emplatado con lámparas de calor y comandas, botellero de cobre con cazos
///    colgados sobre la isla, plantas aromáticas. Al lado, en su propia plataforma elevada, el COMEDOR: mesas con mantel
///    blanco, bajoplato, copas, cubiertos, vela y flor; sillas tapizadas; lámparas colgantes; pared de vinos y plantas.
///  · ESTUDIO TEXTIL (el área de Irosene): telar de pie con su urdimbre, telar de cintura andino, rueca, ovillos de lana
///    por colores, tinas de tinte natural, mesa de corte, máquina de coser, maniquíes y un tendedero con telas.
/// </summary>
public static partial class SampleSceneBuilder
{
    // ── Cocina gourmet ────────────────────────────────────────────────────────────────────────────

    static void GourmetKitchen(Transform room)
    {
        Color slate = new Color(0.2f, 0.21f, 0.23f), tile = new Color(0.12f, 0.3f, 0.24f), brass = new Color(0.78f, 0.62f, 0.3f), copper = new Color(0.72f, 0.42f, 0.25f);
        Transform floor = room.Find("Suelo_Cocina");
        if (floor != null) floor.GetComponent<Renderer>().sharedMaterial = KitchenProps.Mat(slate);
        foreach (Transform t in room) if (t.name == "Junta") t.GetComponent<Renderer>().sharedMaterial = KitchenProps.Mat(new Color(0.14f, 0.15f, 0.17f));
        // Pared del fondo de azulejo verde botella con un friso de latón.
        KitchenProps.C(room, new Vector3(0f, 0.75f, 7.65f), new Vector3(19.2f, 1.5f, 0.05f), tile, 0f, "Pared_Azulejo");   // zócalo bajo: la cocina es abierta por los cuatro lados
        KitchenProps.C(room, new Vector3(0f, 1.05f, 7.6f), new Vector3(19.2f, 0.04f, 0.04f), brass, 0f, "Friso_Laton");
        for (int i = 0; i < 24; i++) KitchenProps.C(room, new Vector3(-9.4f + i * 0.82f, 0.75f, 7.62f), new Vector3(0.01f, 1.5f, 0.01f), new Color(0.08f, 0.2f, 0.16f), 0f, "Junta_Azulejo");
        // Sin nada colgado (no hay techo): ni botellero de cobre, ni lámparas, ni campanas extractoras. Pase: el riel de comandas.
        Transform pass = room.Find("Barra de servicio");
        if (pass != null)
        {
            KitchenProps.C(pass, new Vector3(0f, 1.45f, 0.45f), new Vector3(4.6f, 0.03f, 0.03f), KitchenProps.Steel, 0f, "Riel_comandas");
            for (int i = 0; i < 5; i++) KitchenProps.C(pass, new Vector3(-1.8f + i * 0.9f, 1.36f, 0.45f), new Vector3(0.1f, 0.15f, 0.005f), Color.white, 0f, "Comanda");
        }
        // Hierbas aromáticas frescas en la encimera (macetas de cubos).
        for (int i = 0; i < 5; i++)
        {
            Transform pot = KitchenProps.C(room, new Vector3(1.6f + i * 0.3f, KitchenProps.CounterTop + 0.08f, 0.7f), new Vector3(0.2f, 0.16f, 0.2f), new Color(0.75f, 0.45f, 0.3f), 0f, "Maceta_hierbas");
            KitchenProps.C(room, new Vector3(1.6f + i * 0.3f, KitchenProps.CounterTop + 0.24f, 0.7f), new Vector3(0.22f, 0.18f, 0.22f), new Color(0.3f + i * 0.04f, 0.6f, 0.3f), 0f, "Hierbas");
        }
    }

    // ── Comedor gourmet, en su propia plataforma elevada junto a la cocina ─────────────────────────

    static void BuildGourmetDining(Transform parent)
    {
        // COCINA EN EL CENTRO y el COMEDOR ALREDEDOR, en la misma plataforma: la cocina es abierta (sin paredes, con su
        // barra de pase, la barra de sushi y la cafetera hacia el público), y las mesas la rodean por los cuatro lados,
        // así cualquier comensal ve cocinar. 10 mesas de 4 = 40 cubiertos (base de 30 turistas + personal).
        GameObject kitchen = GameObject.Find("Kitchen_Area");
        if (kitchen == null) return;
        Vector2 size = new Vector2(40f, 38f);   // toda la plataforma (el suelo de la cocina queda encima, en el centro)
        GameObject go = new GameObject("Comedor_Gourmet");
        go.transform.SetParent(kitchen.transform, false);
        go.transform.localPosition = new Vector3(0f, 0.1f, 0f);
        Transform r = go.transform;
        Color wood = new Color(0.42f, 0.28f, 0.2f), linen = new Color(0.97f, 0.97f, 0.95f), gold = new Color(0.82f, 0.68f, 0.35f), velvet = new Color(0.32f, 0.12f, 0.18f);
        KitchenProps.C(r, new Vector3(0f, 0.02f, 0f), new Vector3(size.x, 0.04f, size.y), new Color(0.48f, 0.33f, 0.22f), 0f, "Parquet");
        for (int i = 0; i < 20; i++) KitchenProps.C(r, new Vector3(-size.x * 0.5f + (i + 0.5f) * size.x / 20f, 0.045f, 0f), new Vector3(0.01f, 0.005f, size.y), new Color(0.36f, 0.24f, 0.16f), 0f, "Tablilla");
        // Mesas: 4 delante (junto a la entrada) y 2 al fondo, en el suelo; las de los LADOS suben a dos GALERÍAS de 2,2 m
        // (docs/construcciones-elevadas.md §Pisos): 3 mesas en cada una, con vista a la cocina abierta, y abajo queda un
        // paso ancho alrededor de la cocina. 12 mesas de 4 = 48 cubiertos (base de 30 turistas + personal).
        int mesa = 0;
        void Table(Transform parent, Vector3 pos)
        {
            Transform t = KitchenProps.Root(parent, $"Mesa_{++mesa}", pos);
            KitchenProps.C(t, new Vector3(0f, 0.38f, 0f), new Vector3(0.1f, 0.76f, 0.1f), wood, 0f, "Pata");
            KitchenProps.C(t, new Vector3(0f, 0.765f, 0f), new Vector3(1.3f, 0.03f, 1.3f), linen, 0f, "Mantel");
            KitchenProps.C(t, new Vector3(0f, 0.6f, 0f), new Vector3(1.32f, 0.3f, 1.32f), linen, 0f, "Caida_mantel");
            for (int s = 0; s < 4; s++)   // cuatro cubiertos
            {
                Quaternion q = Quaternion.Euler(0f, s * 90f, 0f);
                Vector3 seat = q * new Vector3(0f, 0f, 0.42f);
                KitchenProps.C(t, seat + new Vector3(0f, 0.785f, 0f), new Vector3(0.3f, 0.01f, 0.3f), gold, s * 90f, "Bajoplato");
                KitchenProps.C(t, seat + new Vector3(0f, 0.795f, 0f), new Vector3(0.24f, 0.012f, 0.24f), Color.white, s * 90f, "Plato");
                KitchenProps.C(t, seat + q * new Vector3(0.2f, 0.785f, 0f), new Vector3(0.02f, 0.005f, 0.18f), KitchenProps.Steel, s * 90f, "Cuchillo");
                KitchenProps.C(t, seat + q * new Vector3(-0.2f, 0.785f, 0f), new Vector3(0.02f, 0.005f, 0.18f), KitchenProps.Steel, s * 90f, "Tenedor");
                KitchenProps.C(t, seat + q * new Vector3(0.18f, 0.84f, -0.18f), new Vector3(0.06f, 0.1f, 0.06f), new Color(0.85f, 0.95f, 1f, 0.35f), 0f, "Copa");
                // Silla tapizada
                Vector3 cp = q * new Vector3(0f, 0f, 0.95f);
                KitchenProps.C(t, cp + new Vector3(0f, 0.45f, 0f), new Vector3(0.45f, 0.08f, 0.45f), velvet, s * 90f, "Asiento");
                KitchenProps.C(t, cp + q * new Vector3(0f, 0.8f, 0.2f), new Vector3(0.45f, 0.65f, 0.06f), velvet, s * 90f, "Respaldo");
                for (int l = 0; l < 4; l++) KitchenProps.C(t, cp + q * new Vector3((l % 2 - 0.5f) * 0.38f, 0.2f, (l / 2 - 0.5f) * 0.38f), new Vector3(0.04f, 0.4f, 0.04f), wood, 0f, "Pata_silla");
            }
            KitchenProps.C(t, new Vector3(0f, 0.85f, 0f), new Vector3(0.05f, 0.14f, 0.05f), new Color(1f, 0.95f, 0.85f), 0f, "Vela");
            KitchenProps.C(t, new Vector3(0f, 0.94f, 0f), new Vector3(0.02f, 0.04f, 0.02f), new Color(1f, 0.75f, 0.3f), 0f, "Llama_vela");
            KitchenProps.C(t, new Vector3(0.18f, 0.84f, 0.12f), new Vector3(0.06f, 0.12f, 0.06f), new Color(0.85f, 0.95f, 1f, 0.4f), 0f, "Florero");
            KitchenProps.C(t, new Vector3(0.18f, 0.93f, 0.12f), new Vector3(0.09f, 0.07f, 0.09f), new Color(0.95f, 0.85f, 0.9f), 0f, "Flor");
        }
        foreach (Vector3 p in new[] { new Vector3(-13.5f, 0f, 12.5f), new Vector3(-4.5f, 0f, 13.5f), new Vector3(4.5f, 0f, 13.5f), new Vector3(13.5f, 0f, 12.5f),
                                      new Vector3(5f, 0f, -12.5f), new Vector3(13f, 0f, -12.5f) })
            Table(r, p);
        const float gx = 17.4f, gz = 9f;   // centro de cada galería en X; llegan de z = -9 a z = +9
        float g = MezzanineHeight, run = StairRun();
        for (int k = -1; k <= 1; k += 2)
        {
            Transform top = Deck(r, k < 0 ? "Galeria_comedor_oeste" : "Galeria_comedor_este", new Vector3(k * gx, 0f, 0f), new Vector2(4.6f, 2f * gz));
            for (int j = -1; j <= 1; j++) Table(top, new Vector3(0f, 0f, j * 6f));
            float inner = k * (gx - 2.3f);
            DeckRail(r, new Vector3(inner, g, -gz), new Vector3(inner, g, gz));                                   // hacia la cocina
            DeckRail(r, new Vector3(inner, g, -gz), new Vector3(k * 19.7f, g, -gz));                               // el fondo
            DeckRail(r, new Vector3(inner, g, gz), new Vector3(k * (gx - 0.8f), g, gz));                            // a los lados de donde llega la escalera
            DeckRail(r, new Vector3(k * (gx + 0.8f), g, gz), new Vector3(k * 19.7f, g, gz));
            InclinedStair(r, k < 0 ? "Escalera_comedor_oeste" : "Escalera_comedor_este", new Vector3(k * gx, 0f, gz + run), 180f);   // al borde de +Z
        }
        int up = 0; foreach (Transform t in r.GetComponentsInChildren<Transform>()) if (t.name.StartsWith("Mesa_") && t.position.y > r.position.y + 1.5f) up++;
        Debug.Log($"[Restaurante] comedor: {mesa - up} mesas en la planta y {up} en las dos galerías de {g:0.#} m, con sus escaleras inclinadas.");
        // Pared de vinos al fondo y plantas en las esquinas.
        Transform wine = KitchenProps.Root(r, "Pared_de_vinos", new Vector3(-size.x * 0.5f + 0.4f, 0f, -13f), 90f);   // en el lateral del fondo (delante queda la entrada)
        KitchenProps.C(wine, new Vector3(0f, 1.2f, 0f), new Vector3(6f, 2.4f, 0.4f), wood, 0f, "Botellero");
        for (int y = 0; y < 6; y++)
            for (int x = 0; x < 14; x++)
                KitchenProps.C(wine, new Vector3(-2.75f + x * 0.42f, 0.3f + y * 0.38f, -0.2f), new Vector3(0.09f, 0.09f, 0.04f), (x + y) % 3 == 0 ? new Color(0.35f, 0.08f, 0.12f) : new Color(0.18f, 0.28f, 0.15f), 0f, "Botella");
        for (int k = -1; k <= 1; k += 2)
        {
            Transform pl = KitchenProps.Root(r, "Planta", new Vector3(k * 2.6f, 0f, size.y * 0.5f - 0.8f));   // a los lados de la entrada
            KitchenProps.C(pl, new Vector3(0f, 0.25f, 0f), new Vector3(0.5f, 0.5f, 0.5f), new Color(0.3f, 0.3f, 0.32f), 0f, "Macetero");
            KitchenProps.C(pl, new Vector3(0f, 0.9f, 0f), new Vector3(0.8f, 0.9f, 0.8f), new Color(0.3f, 0.55f, 0.3f), 30f, "Follaje");
        }
        WorldLabel wl = go.AddComponent<WorldLabel>(); wl.text = "Restaurante (comedor para 30)"; wl.height = 3f;
        Aseos(r, new Vector3(-8f, 0f, -size.y * 0.5f + 0.9f), 0f);   // al fondo, entre la pared de vinos y las mesas
    }


    // ── Aseos (en cada área humana) ───────────────────────────────────────────────────────────────
    // Módulo de aseos: cabinas con puerta y INODORO SECO de compostaje (sin agua ni desagüe: en el hielo no hay
    // alcantarillado; la serrín/ceniza tapa y el compost acaba, maduro, en los huertos), una cabina ACCESIBLE más ancha,
    // lavabos con espejo y dispensador, y el cartel. `cabins` = cabinas normales (3 para un área de 30 personas).
    static Transform Aseos(Transform parent, Vector3 local, float yaw, int cabins = 3)
    {
        Transform r = KitchenProps.Root(parent, "Aseos", local, yaw);
        Color wall = new Color(0.86f, 0.9f, 0.92f), door = new Color(0.45f, 0.62f, 0.72f), white = KitchenProps.White, steel = KitchenProps.Steel, wood = new Color(0.62f, 0.5f, 0.38f);
        float cw = 0.95f, depth = 1.5f, h = 2.2f;
        float total = cw * cabins + 1.5f + 1.3f;   // cabinas + accesible + zona de lavabos
        KitchenProps.C(r, new Vector3(0f, h * 0.5f, -depth * 0.5f), new Vector3(total, h, 0.06f), wall, 0f, "Pared");
        KitchenProps.C(r, new Vector3(0f, h + 0.03f, 0f), new Vector3(total, 0.06f, depth), wall, 0f, "Techo");
        float x = -total * 0.5f;
        for (int i = 0; i <= cabins; i++)
        {
            bool acc = i == cabins;
            float w = acc ? 1.5f : cw;
            KitchenProps.C(r, new Vector3(x, h * 0.5f, 0f), new Vector3(0.04f, h, depth), wall, 0f, "Tabique");
            KitchenProps.C(r, new Vector3(x + w * 0.5f, h * 0.48f, depth * 0.5f), new Vector3(w - 0.06f, h * 0.9f, 0.04f), door, 0f, acc ? "Puerta_accesible" : "Puerta");
            KitchenProps.C(r, new Vector3(x + w * 0.5f + w * 0.3f, 1f, depth * 0.5f + 0.03f), new Vector3(0.03f, 0.12f, 0.03f), steel, 0f, "Pomo");
            // Inodoro seco de compostaje: caja de madera con asiento y su cubo de serrín al lado.
            KitchenProps.C(r, new Vector3(x + w * 0.5f, 0.22f, -depth * 0.25f), new Vector3(0.42f, 0.44f, 0.55f), wood, 0f, "Inodoro_seco");
            KitchenProps.C(r, new Vector3(x + w * 0.5f, 0.45f, -depth * 0.25f), new Vector3(0.38f, 0.03f, 0.45f), white, 0f, "Asiento");
            KitchenProps.C(r, new Vector3(x + w * 0.5f + 0.32f, 0.15f, -depth * 0.35f), new Vector3(0.2f, 0.3f, 0.2f), new Color(0.78f, 0.66f, 0.45f), 0f, "Cubo_serrin");
            if (acc) KitchenProps.C(r, new Vector3(x + w * 0.5f - 0.45f, 0.75f, -depth * 0.25f), new Vector3(0.04f, 0.04f, 0.6f), steel, 0f, "Barra_apoyo");
            x += w;
        }
        KitchenProps.C(r, new Vector3(x, h * 0.5f, 0f), new Vector3(0.04f, h, depth), wall, 0f, "Tabique");
        // Lavabos con espejo y dispensador de jabón.
        for (int k = 0; k < 2; k++)
        {
            float lx = x + 0.35f + k * 0.6f;
            KitchenProps.C(r, new Vector3(lx, 0.8f, -depth * 0.35f), new Vector3(0.5f, 0.12f, 0.4f), white, 0f, "Lavabo");
            KitchenProps.C(r, new Vector3(lx, 0.88f, -depth * 0.47f), new Vector3(0.04f, 0.16f, 0.04f), steel, 0f, "Grifo");
            KitchenProps.C(r, new Vector3(lx, 1.45f, -depth * 0.5f + 0.04f), new Vector3(0.45f, 0.6f, 0.02f), new Color(0.82f, 0.92f, 0.98f), 0f, "Espejo");
        }
        KitchenProps.C(r, new Vector3(x + 0.95f, 1.1f, -depth * 0.5f + 0.05f), new Vector3(0.1f, 0.18f, 0.06f), white, 0f, "Jabon");
        WorldLabel wl = r.gameObject.AddComponent<WorldLabel>(); wl.text = "Aseos (WC)"; wl.height = h + 0.5f;
        return r;
    }
    // ── Estudio textil ────────────────────────────────────────────────────────────────────────────

    static void FurnishTextile(Transform area, Vector2 size)
    {
        Transform r = new GameObject("Estudio_Textil").transform; r.SetParent(area, false);
        Color wood = new Color(0.6f, 0.45f, 0.32f), dark = new Color(0.42f, 0.3f, 0.2f);
        Color[] wool = { new Color(0.85f, 0.2f, 0.2f), new Color(0.95f, 0.7f, 0.15f), new Color(0.2f, 0.45f, 0.75f), new Color(0.25f, 0.6f, 0.35f),
                         new Color(0.6f, 0.3f, 0.6f), new Color(0.95f, 0.95f, 0.92f), new Color(0.45f, 0.3f, 0.2f), new Color(0.95f, 0.45f, 0.55f) };
        void L(Transform t, string s, float h) { WorldLabel w = t.gameObject.AddComponent<WorldLabel>(); w.text = s; w.height = h; }
        float hx = size.x * 0.5f, hz = size.y * 0.5f;

        // Telar de pie (de bastidor) con la urdimbre de colores y la tela ya tejida.
        Transform loom = KitchenProps.Root(r, "Telar_de_pie", new Vector3(-hx + 1.6f, 0f, -hz + 1.4f));
        for (int s = -1; s <= 1; s += 2) for (int f = -1; f <= 1; f += 2) KitchenProps.C(loom, new Vector3(s * 0.75f, 0.75f, f * 0.55f), new Vector3(0.08f, 1.5f, 0.08f), wood);
        KitchenProps.C(loom, new Vector3(0f, 1.5f, 0f), new Vector3(1.6f, 0.08f, 1.2f), dark, 0f, "Bastidor");
        for (int i = 0; i < 16; i++) KitchenProps.C(loom, new Vector3(-0.6f + i * 0.08f, 0.95f, 0f), new Vector3(0.01f, 0.9f, 0.01f), wool[i % wool.Length], 0f, "Urdimbre");
        KitchenProps.C(loom, new Vector3(0f, 0.65f, 0.15f), new Vector3(1.3f, 0.3f, 0.02f), new Color(0.75f, 0.25f, 0.3f), 0f, "Tejido");
        KitchenProps.C(loom, new Vector3(0f, 0.5f, -0.45f), new Vector3(1.2f, 0.06f, 0.35f), wood, 0f, "Banco");
        L(loom, "Telar de pie", 1.9f);
        // Telar de cintura andino (Irosene es ecuatoriana): urdimbre tensa entre un poste y la tejedora.
        Transform back = KitchenProps.Root(r, "Telar_de_cintura", new Vector3(hx - 1.4f, 0f, -hz + 1.2f));
        KitchenProps.C(back, new Vector3(0f, 0.6f, 0.9f), new Vector3(0.1f, 1.2f, 0.1f), dark, 0f, "Poste");
        for (int i = 0; i < 10; i++) KitchenProps.C(back, new Vector3(-0.25f + i * 0.055f, 0.45f, 0.35f), new Vector3(0.012f, 0.012f, 1.1f), wool[(i + 3) % wool.Length], -8f, "Hilo");
        KitchenProps.C(back, new Vector3(0f, 0.42f, -0.15f), new Vector3(0.6f, 0.03f, 0.25f), new Color(0.9f, 0.4f, 0.2f), 0f, "Faja_tejida");
        L(back, "Telar de cintura andino", 1.4f);
        // Rueca (hilar la lana).
        Transform wheel = KitchenProps.Root(r, "Rueca", new Vector3(-hx + 1.2f, 0f, 0.6f));
        KitchenProps.C(wheel, new Vector3(0f, 0.3f, 0f), new Vector3(0.6f, 0.08f, 0.3f), wood);
        for (int a = 0; a < 8; a++) { Transform sp = KitchenProps.C(wheel, new Vector3(0f, 0.75f, 0f), new Vector3(0.02f, 0.7f, 0.02f), dark); sp.localRotation = Quaternion.Euler(a * 22.5f, 0f, 0f); }
        KitchenProps.C(wheel, new Vector3(0f, 0.75f, 0f), new Vector3(0.05f, 0.75f, 0.75f), new Color(0.6f, 0.45f, 0.32f, 0.35f), 0f, "Rueda");
        KitchenProps.C(wheel, new Vector3(0.25f, 0.95f, 0f), new Vector3(0.05f, 0.05f, 0.25f), new Color(0.95f, 0.95f, 0.9f), 0f, "Huso");
        L(wheel, "Rueca", 1.4f);
        // Estantería de ovillos por colores.
        Transform shelf = KitchenProps.Root(r, "Ovillos", new Vector3(0f, 0f, -hz + 0.35f));
        KitchenProps.C(shelf, new Vector3(0f, 1f, 0f), new Vector3(3f, 2f, 0.4f), dark, 0f, "Estanteria");
        for (int y = 0; y < 4; y++) for (int x = 0; x < 9; x++)
            KitchenProps.C(shelf, new Vector3(-1.3f + x * 0.32f, 0.3f + y * 0.45f, 0.12f), new Vector3(0.22f, 0.2f, 0.22f), wool[(x + y * 3) % wool.Length], 0f, "Ovillo");
        L(shelf, "Lanas (alpaca, oveja)", 2.3f);
        // Tinas de tinte natural (cochinilla, añil, nogal, cúrcuma).
        Color[] dye = { new Color(0.75f, 0.1f, 0.25f), new Color(0.15f, 0.2f, 0.6f), new Color(0.4f, 0.28f, 0.15f), new Color(0.95f, 0.75f, 0.1f) };
        string[] dn = { "Cochinilla", "Añil", "Nogal", "Cúrcuma" };
        for (int i = 0; i < 4; i++)
        {
            Transform v = KitchenProps.Root(r, $"Tina_{dn[i]}", new Vector3(hx - 0.7f, 0f, -0.8f + i * 0.8f));
            KitchenProps.C(v, new Vector3(0f, 0.3f, 0f), new Vector3(0.6f, 0.6f, 0.6f), KitchenProps.Steel, 0f, "Tina");
            KitchenProps.C(v, new Vector3(0f, 0.58f, 0f), new Vector3(0.52f, 0.02f, 0.52f), dye[i], 0f, "Tinte");
            L(v, $"Tinte de {dn[i].ToLower()}", 1f);
        }
        // Mesa de corte con tela, tijeras y cinta; máquina de coser (eléctrica: energía solar).
        Transform cut = KitchenProps.Root(r, "Mesa_de_corte", new Vector3(0f, 0f, 0.4f));
        KitchenProps.C(cut, new Vector3(0f, 0.85f, 0f), new Vector3(2.2f, 0.06f, 1.1f), wood, 0f, "Tablero");
        for (int s = -1; s <= 1; s += 2) for (int f = -1; f <= 1; f += 2) KitchenProps.C(cut, new Vector3(s * 1f, 0.42f, f * 0.48f), new Vector3(0.07f, 0.84f, 0.07f), dark);
        KitchenProps.C(cut, new Vector3(-0.3f, 0.885f, 0f), new Vector3(1.2f, 0.01f, 0.8f), new Color(0.2f, 0.45f, 0.75f), 0f, "Tela");
        KitchenProps.C(cut, new Vector3(0.6f, 0.9f, 0.2f), new Vector3(0.18f, 0.02f, 0.06f), KitchenProps.Steel, 30f, "Tijeras");
        KitchenProps.C(cut, new Vector3(0.6f, 0.89f, -0.25f), new Vector3(0.6f, 0.005f, 0.03f), new Color(0.95f, 0.85f, 0.3f), 0f, "Cinta_metrica");
        L(cut, "Mesa de corte", 1.3f);
        Transform sew = KitchenProps.Root(r, "Maquina_de_coser", new Vector3(-1.5f, 0f, hz - 1.2f));
        KitchenProps.C(sew, new Vector3(0f, 0.38f, 0f), new Vector3(1f, 0.76f, 0.55f), wood, 0f, "Mesa");
        KitchenProps.C(sew, new Vector3(0f, 0.86f, 0f), new Vector3(0.45f, 0.18f, 0.18f), Color.white, 0f, "Cuerpo");
        KitchenProps.C(sew, new Vector3(0.15f, 0.98f, 0f), new Vector3(0.15f, 0.25f, 0.15f), Color.white, 0f, "Brazo");
        KitchenProps.C(sew, new Vector3(-0.18f, 0.9f, 0f), new Vector3(0.03f, 0.12f, 0.03f), KitchenProps.Steel, 0f, "Aguja");
        L(sew, "Máquina de coser", 1.3f);
        // Maniquíes con prendas y un tendedero con telas teñidas.
        for (int m = 0; m < 2; m++)
        {
            Transform mq = KitchenProps.Root(r, $"Maniqui_{m + 1}", new Vector3(1.8f + m * 1.1f, 0f, hz - 1.0f));
            KitchenProps.C(mq, new Vector3(0f, 0.6f, 0f), new Vector3(0.05f, 1.2f, 0.05f), dark);
            KitchenProps.C(mq, new Vector3(0f, 1.35f, 0f), new Vector3(0.38f, 0.5f, 0.22f), wool[m * 3 + 1], 0f, "Prenda");
            KitchenProps.C(mq, new Vector3(0f, 1.05f, 0f), new Vector3(0.42f, 0.25f, 0.25f), wool[m * 3 + 2], 0f, "Falda");
        }
        Transform line = KitchenProps.Root(r, "Tendedero", new Vector3(0f, 0f, hz - 0.3f));
        for (int s = -1; s <= 1; s += 2) KitchenProps.C(line, new Vector3(s * 1.4f, 0.9f, 0f), new Vector3(0.05f, 1.8f, 0.05f), dark);
        KitchenProps.C(line, new Vector3(0f, 1.75f, 0f), new Vector3(2.8f, 0.01f, 0.01f), Color.white, 0f, "Cuerda");
        for (int i = 0; i < 5; i++) KitchenProps.C(line, new Vector3(-1.1f + i * 0.55f, 1.4f, 0f), new Vector3(0.45f, 0.7f, 0.01f), dye[i % dye.Length], 0f, "Tela_tendida");
    }
}
