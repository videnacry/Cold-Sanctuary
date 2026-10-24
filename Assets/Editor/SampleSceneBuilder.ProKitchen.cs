using UnityEngine;

/// <summary>
/// COCINA PROFESIONAL (docs/cocina-profesional.md): el equipo que piden la carta y sus elaboraciones (Gastronomy), como en
/// una cocina de restaurante con obrador propio. Dónde va cada máquina lo decide la planta de la cocina
/// (SampleSceneBuilder.KitchenPlan, docs/restaurant-plan.md); aquí está cómo es cada una.
/// Cada máquina se llama «Equipo_<nombre del recetario>» y lleva su cartel.
/// </summary>
public static partial class SampleSceneBuilder
{
    static readonly Color EqSteel = new Color(0.75f, 0.77f, 0.8f), EqDark = new Color(0.25f, 0.26f, 0.29f), EqGlass = new Color(0.6f, 0.75f, 0.8f, 0.45f),
        EqWood = new Color(0.55f, 0.4f, 0.28f), EqStone = new Color(0.6f, 0.58f, 0.55f), EqCopper = new Color(0.72f, 0.42f, 0.25f), EqLed = new Color(0.4f, 0.9f, 0.6f);

    static void ProKitchen(Transform room)
    {
        // El equipo ya está repartido por la planta (SampleSceneBuilder.KitchenPlan: cada puesto completo y los obradores;
        // la barra de sushi y la cafetera de cara al público, en el anillo del comedor). Aquí, la despensa/obrador que
        // planifica las elaboraciones (yogur, quesos, masas…).
        room.gameObject.AddComponent<KitchenLarder>();
    }

    /// <summary>Una máquina del recetario: cuerpo con su silueta propia, detalles y cartel.</summary>
    static Transform Eq(Transform room, string name, Vector3 pos, float yaw)
    {
        Transform t = KitchenProps.Root(room, "Equipo_" + name, pos, yaw);
        float top = 0.9f;
        switch (name)
        {
            case "horno mixto":   // horno combinado vapor-convección de 10 bandejas sobre soporte
                KitchenProps.C(t, new Vector3(0f, 0.3f, 0f), new Vector3(0.9f, 0.6f, 0.8f), EqSteel, 0f, "Soporte");
                KitchenProps.C(t, new Vector3(0f, 1.15f, 0f), new Vector3(0.9f, 1.1f, 0.85f), EqSteel, 0f, "Cuerpo");
                KitchenProps.C(t, new Vector3(-0.08f, 1.15f, -0.43f), new Vector3(0.62f, 0.9f, 0.02f), EqGlass, 0f, "Puerta_cristal");
                KitchenProps.C(t, new Vector3(0.36f, 1.35f, -0.43f), new Vector3(0.14f, 0.3f, 0.02f), EqDark, 0f, "Pantalla");
                for (int i = 0; i < 6; i++) KitchenProps.C(t, new Vector3(-0.08f, 0.8f + i * 0.13f, -0.1f), new Vector3(0.55f, 0.01f, 0.6f), EqDark, 0f, "Bandeja_GN");
                top = 1.7f; break;
            case "horno de pizza (eléctrico)":   // horno de piedra refractaria: cúpula y boca baja
                KitchenProps.C(t, new Vector3(0f, 0.45f, 0f), new Vector3(1.3f, 0.9f, 1.2f), EqDark, 0f, "Base");
                KitchenProps.C(t, new Vector3(0f, 1.15f, 0f), new Vector3(1.3f, 0.5f, 1.2f), new Color(0.65f, 0.3f, 0.22f), 0f, "Cupula");
                KitchenProps.C(t, new Vector3(0f, 1.48f, 0f), new Vector3(0.9f, 0.18f, 0.8f), new Color(0.65f, 0.3f, 0.22f), 0f, "Cupula_alta");
                KitchenProps.C(t, new Vector3(0f, 1.05f, -0.61f), new Vector3(0.6f, 0.25f, 0.02f), new Color(1f, 0.5f, 0.15f), 0f, "Boca_brasa");
                KitchenProps.C(t, new Vector3(0.7f, 1.3f, -0.3f), new Vector3(0.04f, 0.04f, 1.6f), EqWood, 0f, "Pala");
                top = 1.6f; break;
            case "wok de inducción": case "paellero de inducción": case "fogón de inducción": case "comal":
                KitchenProps.C(t, new Vector3(0f, 0.45f, 0f), new Vector3(1.1f, 0.9f, 0.8f), EqSteel, 0f, "Cuerpo");
                KitchenProps.C(t, new Vector3(0f, 0.905f, 0f), new Vector3(1.05f, 0.01f, 0.75f), new Color(0.08f, 0.08f, 0.1f), 0f, "Vitro");
                if (name.StartsWith("wok")) { KitchenProps.C(t, new Vector3(0f, 0.98f, 0f), new Vector3(0.45f, 0.12f, 0.45f), EqDark, 45f, "Wok"); KitchenProps.C(t, new Vector3(0.38f, 1.0f, 0f), new Vector3(0.35f, 0.03f, 0.04f), EqWood, 0f, "Mango"); }
                else if (name.StartsWith("paellero")) { KitchenProps.C(t, new Vector3(0f, 0.93f, 0f), new Vector3(0.8f, 0.05f, 0.6f), new Color(0.3f, 0.3f, 0.32f), 0f, "Paellera"); KitchenProps.C(t, new Vector3(0f, 0.96f, 0f), new Vector3(0.7f, 0.02f, 0.5f), new Color(0.95f, 0.75f, 0.3f), 0f, "Arroz_azafran"); }
                else if (name == "comal") KitchenProps.C(t, new Vector3(0f, 0.93f, 0f), new Vector3(0.85f, 0.03f, 0.6f), new Color(0.2f, 0.18f, 0.17f), 0f, "Plancha_comal");
                else for (int i = 0; i < 2; i++) KitchenProps.C(t, new Vector3(-0.25f + i * 0.5f, 1.0f, 0f), new Vector3(0.3f, 0.18f, 0.3f), EqSteel, 0f, "Olla");
                break;
            case "trompo vertical (inducción)":
                KitchenProps.C(t, new Vector3(0f, 0.45f, 0f), new Vector3(0.8f, 0.9f, 0.7f), EqSteel, 0f, "Cuerpo");
                KitchenProps.C(t, new Vector3(0f, 1.25f, 0.1f), new Vector3(0.05f, 0.7f, 0.05f), EqSteel, 0f, "Espada");
                KitchenProps.C(t, new Vector3(0f, 1.2f, 0.1f), new Vector3(0.28f, 0.45f, 0.28f), new Color(0.75f, 0.35f, 0.2f), 0f, "Pastor");
                KitchenProps.C(t, new Vector3(0f, 1.52f, 0.1f), new Vector3(0.16f, 0.1f, 0.16f), new Color(0.95f, 0.85f, 0.3f), 0f, "Pina");
                KitchenProps.C(t, new Vector3(0f, 1.2f, 0.33f), new Vector3(0.5f, 0.7f, 0.04f), new Color(1f, 0.5f, 0.2f), 0f, "Resistencia");
                top = 1.7f; break;
            case "barra de sushi":   // barra con vitrina refrigerada de pescado y tabla de hinoki
                KitchenProps.C(t, new Vector3(0f, 0.5f, 0f), new Vector3(2.4f, 1f, 0.7f), EqWood, 0f, "Barra");
                KitchenProps.C(t, new Vector3(0f, 1.0f, 0f), new Vector3(2.45f, 0.04f, 0.75f), new Color(0.92f, 0.85f, 0.7f), 0f, "Hinoki");
                KitchenProps.C(t, new Vector3(0f, 1.15f, -0.15f), new Vector3(2.2f, 0.25f, 0.35f), EqGlass, 0f, "Vitrina_neta");
                for (int i = 0; i < 4; i++) KitchenProps.C(t, new Vector3(-0.8f + i * 0.55f, 1.06f, -0.15f), new Vector3(0.35f, 0.06f, 0.15f), i % 2 == 0 ? new Color(0.98f, 0.55f, 0.4f) : new Color(0.75f, 0.2f, 0.25f), 0f, "Neta");
                KitchenProps.C(t, new Vector3(0.9f, 1.08f, 0.15f), new Vector3(0.35f, 0.12f, 0.35f), new Color(0.75f, 0.6f, 0.4f), 0f, "Hangiri");
                break;
            case "cafetera espresso":
                KitchenProps.C(t, new Vector3(0f, 0.5f, 0f), new Vector3(1.2f, 1f, 0.7f), EqWood, 0f, "Mueble");
                KitchenProps.C(t, new Vector3(0f, 1.25f, 0.05f), new Vector3(0.8f, 0.5f, 0.5f), new Color(0.75f, 0.2f, 0.18f), 0f, "Maquina");
                for (int i = 0; i < 2; i++) KitchenProps.C(t, new Vector3(-0.2f + i * 0.4f, 1.05f, -0.22f), new Vector3(0.08f, 0.08f, 0.12f), EqSteel, 0f, "Grupo");
                KitchenProps.C(t, new Vector3(0.45f, 1.2f, 0.1f), new Vector3(0.16f, 0.4f, 0.16f), EqDark, 0f, "Molinillo");
                break;
            case "molino de piedra":
                KitchenProps.C(t, new Vector3(0f, 0.4f, 0f), new Vector3(1f, 0.8f, 1f), EqWood, 0f, "Bancada");
                KitchenProps.C(t, new Vector3(0f, 0.9f, 0f), new Vector3(0.8f, 0.2f, 0.8f), EqStone, 45f, "Muela");
                KitchenProps.C(t, new Vector3(0f, 1.2f, 0f), new Vector3(0.4f, 0.4f, 0.4f), EqWood, 0f, "Tolva");
                KitchenProps.C(t, new Vector3(0f, 0.55f, -0.55f), new Vector3(0.4f, 0.2f, 0.2f), new Color(0.95f, 0.92f, 0.85f), 0f, "Harina");
                break;
            case "trapiche y paila":
                KitchenProps.C(t, new Vector3(0.3f, 0.6f, 0f), new Vector3(0.5f, 1.2f, 0.6f), EqDark, 0f, "Trapiche");
                for (int i = 0; i < 3; i++) KitchenProps.C(t, new Vector3(0.3f, 0.5f + i * 0.25f, -0.32f), new Vector3(0.45f, 0.2f, 0.05f), EqSteel, 0f, "Rodillo");
                KitchenProps.C(t, new Vector3(-0.45f, 0.3f, 0f), new Vector3(0.7f, 0.6f, 0.7f), EqDark, 0f, "Hornilla");
                KitchenProps.C(t, new Vector3(-0.45f, 0.68f, 0f), new Vector3(0.75f, 0.16f, 0.75f), EqCopper, 0f, "Paila");
                KitchenProps.C(t, new Vector3(-0.45f, 0.74f, 0f), new Vector3(0.6f, 0.04f, 0.6f), new Color(0.55f, 0.32f, 0.12f), 0f, "Panela");
                break;
            case "almazara":   // molino de aceite: empiedro, batidora y decantador
                KitchenProps.C(t, new Vector3(0f, 0.35f, 0f), new Vector3(1.2f, 0.7f, 0.8f), EqSteel, 0f, "Batidora");
                KitchenProps.C(t, new Vector3(0f, 0.85f, 0f), new Vector3(0.6f, 0.3f, 0.6f), EqStone, 0f, "Empiedro");
                KitchenProps.C(t, new Vector3(0.5f, 0.9f, 0.2f), new Vector3(0.2f, 0.4f, 0.2f), new Color(0.75f, 0.7f, 0.25f, 0.6f), 0f, "Aceite");
                break;
            case "barricas de fermentación":   // shoyu, miso, vinagre, gari: barricas de madera con sus fechas
                for (int i = 0; i < 4; i++)
                {
                    float x = -0.6f + i * 0.4f;
                    KitchenProps.C(t, new Vector3(x, 0.4f, 0f), new Vector3(0.36f, 0.8f, 0.36f), EqWood, 45f, "Barrica");
                    KitchenProps.C(t, new Vector3(x, 0.25f, 0f), new Vector3(0.38f, 0.04f, 0.38f), EqDark, 45f, "Aro");
                    KitchenProps.C(t, new Vector3(x, 0.6f, 0f), new Vector3(0.38f, 0.04f, 0.38f), EqDark, 45f, "Aro");
                }
                break;
            case "cava de maduración":   // armario climatizado de quesos (12 °C, 85 % HR) con ruedas de parmesano
                KitchenProps.C(t, new Vector3(0f, 1.0f, 0f), new Vector3(1.4f, 2f, 0.8f), new Color(0.35f, 0.3f, 0.26f), 0f, "Cava");
                KitchenProps.C(t, new Vector3(0f, 1.0f, -0.41f), new Vector3(1.2f, 1.7f, 0.02f), EqGlass, 0f, "Puerta_cristal");
                for (int r = 0; r < 4; r++) for (int c = 0; c < 3; c++)
                    KitchenProps.C(t, new Vector3(-0.4f + c * 0.4f, 0.35f + r * 0.42f, 0f), new Vector3(0.32f, 0.12f, 0.32f), new Color(0.95f, 0.8f, 0.45f), 45f, "Rueda_queso");
                KitchenProps.C(t, new Vector3(0.5f, 1.85f, -0.42f), new Vector3(0.2f, 0.08f, 0.01f), EqLed, 0f, "Termostato");
                top = 2.1f; break;
            case "extrusora y secadero":
                KitchenProps.C(t, new Vector3(-0.35f, 0.5f, 0f), new Vector3(0.6f, 1f, 0.6f), EqSteel, 0f, "Extrusora");
                KitchenProps.C(t, new Vector3(-0.35f, 1.15f, 0f), new Vector3(0.35f, 0.3f, 0.35f), EqSteel, 0f, "Tolva");
                KitchenProps.C(t, new Vector3(0.35f, 0.9f, 0f), new Vector3(0.7f, 1.8f, 0.6f), EqWood, 0f, "Secadero");
                for (int i = 0; i < 4; i++) for (int k = 0; k < 6; k++)
                    KitchenProps.C(t, new Vector3(0.1f + k * 0.1f, 0.6f + i * 0.35f, -0.2f), new Vector3(0.01f, 0.28f, 0.01f), new Color(0.96f, 0.88f, 0.6f), 0f, "Espagueti");
                top = 1.9f; break;
            case "nixtamalizadora y molino":
                KitchenProps.C(t, new Vector3(-0.3f, 0.45f, 0f), new Vector3(0.7f, 0.9f, 0.7f), EqSteel, 0f, "Tina_cal");
                KitchenProps.C(t, new Vector3(-0.3f, 0.88f, 0f), new Vector3(0.6f, 0.04f, 0.6f), new Color(0.95f, 0.85f, 0.4f), 0f, "Maiz");
                KitchenProps.C(t, new Vector3(0.4f, 0.55f, 0f), new Vector3(0.45f, 1.1f, 0.45f), new Color(0.3f, 0.45f, 0.55f), 0f, "Molino_nixtamal");
                break;
            case "cámara de fermentación":   // fermentadora controlada para masa madre, pan y masa de pizza
            case "incubadora de tempeh":
            case "abatidor":
                KitchenProps.C(t, new Vector3(0f, 1.0f, 0f), new Vector3(0.85f, 2f, 0.8f), EqSteel, 0f, "Armario");
                KitchenProps.C(t, new Vector3(0f, 1.0f, -0.41f), new Vector3(0.75f, 1.8f, 0.02f), name == "abatidor" ? EqSteel : EqGlass, 0f, "Puerta");
                KitchenProps.C(t, new Vector3(0.25f, 1.9f, -0.43f), new Vector3(0.2f, 0.08f, 0.01f), name == "abatidor" ? new Color(0.4f, 0.7f, 1f) : EqLed, 0f, "Display");
                if (name != "abatidor") for (int i = 0; i < 5; i++)
                    KitchenProps.C(t, new Vector3(0f, 0.3f + i * 0.33f, 0f), new Vector3(0.6f, 0.1f, 0.55f), name == "incubadora de tempeh" ? new Color(0.95f, 0.93f, 0.85f) : new Color(0.92f, 0.82f, 0.62f), 0f, "Bandeja");
                top = 2.1f; break;
            case "yogurtera (incubadora)":
                KitchenProps.C(t, new Vector3(0f, 0.45f, 0f), new Vector3(1f, 0.9f, 0.7f), EqSteel, 0f, "Mesa");
                KitchenProps.C(t, new Vector3(0f, 1.1f, 0f), new Vector3(0.8f, 0.4f, 0.5f), EqSteel, 0f, "Incubadora");
                KitchenProps.C(t, new Vector3(0f, 1.1f, -0.26f), new Vector3(0.6f, 0.3f, 0.02f), EqGlass, 0f, "Ventana");
                for (int i = 0; i < 5; i++) KitchenProps.C(t, new Vector3(-0.25f + i * 0.125f, 1.0f, 0f), new Vector3(0.09f, 0.14f, 0.09f), new Color(0.97f, 0.97f, 0.95f), 0f, "Tarro");
                KitchenProps.C(t, new Vector3(0.3f, 1.25f, -0.27f), new Vector3(0.12f, 0.06f, 0.01f), EqLed, 0f, "43C");
                break;
            case "cuba de quesería":   // cuba de doble pared con liras de corte y moldes
                KitchenProps.C(t, new Vector3(0f, 0.5f, 0f), new Vector3(1.3f, 0.6f, 0.8f), EqSteel, 0f, "Cuba");
                KitchenProps.C(t, new Vector3(0f, 0.78f, 0f), new Vector3(1.2f, 0.02f, 0.7f), new Color(0.98f, 0.96f, 0.88f), 0f, "Cuajada");
                for (int i = 0; i < 4; i++) KitchenProps.C(t, new Vector3(-0.5f + i * 0.33f, 0.2f, 0f), new Vector3(0.05f, 0.4f, 0.05f), EqSteel, 0f, "Pata");
                KitchenProps.C(t, new Vector3(0.75f, 0.95f, 0f), new Vector3(0.04f, 0.6f, 0.4f), EqSteel, 0f, "Lira");
                for (int i = 0; i < 3; i++) KitchenProps.C(t, new Vector3(-0.4f + i * 0.4f, 0.9f, 0.5f), new Vector3(0.2f, 0.15f, 0.2f), new Color(0.9f, 0.9f, 0.92f), 0f, "Molde");
                break;
            case "prensa de tofu":
                KitchenProps.C(t, new Vector3(0f, 0.45f, 0f), new Vector3(0.9f, 0.9f, 0.7f), EqSteel, 0f, "Mesa");
                KitchenProps.C(t, new Vector3(0f, 1.02f, 0f), new Vector3(0.5f, 0.24f, 0.4f), EqWood, 0f, "Caja_prensa");
                KitchenProps.C(t, new Vector3(0f, 1.3f, 0f), new Vector3(0.06f, 0.4f, 0.06f), EqSteel, 0f, "Husillo");
                KitchenProps.C(t, new Vector3(0f, 1.5f, 0f), new Vector3(0.4f, 0.04f, 0.04f), EqSteel, 0f, "Manivela");
                KitchenProps.C(t, new Vector3(0.32f, 0.96f, 0f), new Vector3(0.2f, 0.1f, 0.2f), new Color(0.97f, 0.95f, 0.88f), 0f, "Tofu");
                break;
            case "marmita": case "marmita a presión":   // marmita basculante con su volante
                KitchenProps.C(t, new Vector3(0f, 0.55f, 0f), new Vector3(0.9f, 0.6f, 0.9f), EqSteel, 45f, "Marmita");
                KitchenProps.C(t, new Vector3(0f, 0.2f, 0f), new Vector3(0.9f, 0.4f, 0.5f), EqSteel, 0f, "Base");
                KitchenProps.C(t, new Vector3(0.55f, 0.6f, 0f), new Vector3(0.05f, 0.3f, 0.3f), EqDark, 0f, "Volante");
                if (name == "marmita a presión") { KitchenProps.C(t, new Vector3(0f, 0.9f, 0f), new Vector3(0.85f, 0.1f, 0.85f), EqSteel, 45f, "Tapa_hermetica"); KitchenProps.C(t, new Vector3(0f, 1.0f, 0f), new Vector3(0.08f, 0.12f, 0.08f), new Color(0.9f, 0.2f, 0.2f), 0f, "Valvula"); }
                break;
            case "amasadora espiral": case "batidora planetaria":
                KitchenProps.C(t, new Vector3(0f, 0.4f, 0f), new Vector3(0.6f, 0.8f, 0.6f), EqSteel, 0f, "Base");
                KitchenProps.C(t, new Vector3(0f, 0.95f, 0f), new Vector3(0.45f, 0.3f, 0.45f), EqSteel, 0f, "Cuba");
                KitchenProps.C(t, new Vector3(0f, 1.25f, 0.2f), new Vector3(0.35f, 0.5f, 0.2f), name.StartsWith("amasadora") ? new Color(0.85f, 0.3f, 0.25f) : EqSteel, 0f, "Cabezal");
                KitchenProps.C(t, new Vector3(0f, 1.0f, 0f), new Vector3(0.05f, 0.35f, 0.05f), EqDark, 0f, name.StartsWith("amasadora") ? "Espiral" : "Varilla");
                top = 1.5f; break;
            case "laminadora de pasta":
                KitchenProps.C(t, new Vector3(0f, 0.45f, 0f), new Vector3(1.2f, 0.9f, 0.7f), EqSteel, 0f, "Mesa");
                KitchenProps.C(t, new Vector3(0f, 1.05f, 0f), new Vector3(0.4f, 0.3f, 0.35f), EqSteel, 0f, "Rodillos");
                KitchenProps.C(t, new Vector3(-0.4f, 0.92f, 0f), new Vector3(0.4f, 0.01f, 0.3f), new Color(0.97f, 0.88f, 0.55f), 0f, "Lamina_pasta");
                KitchenProps.C(t, new Vector3(0.4f, 0.92f, 0f), new Vector3(0.4f, 0.01f, 0.3f), new Color(0.97f, 0.88f, 0.55f), 0f, "Lamina_pasta");
                break;
            case "arrocera": case "vaporera":
                KitchenProps.C(t, new Vector3(0f, 0.45f, 0f), new Vector3(0.8f, 0.9f, 0.7f), EqSteel, 0f, "Mesa");
                if (name == "arrocera") { KitchenProps.C(t, new Vector3(0f, 1.05f, 0f), new Vector3(0.45f, 0.3f, 0.45f), new Color(0.95f, 0.95f, 0.93f), 0f, "Arrocera"); KitchenProps.C(t, new Vector3(0.12f, 1.1f, -0.23f), new Vector3(0.08f, 0.05f, 0.01f), EqLed, 0f, "Piloto"); }
                else for (int i = 0; i < 3; i++) KitchenProps.C(t, new Vector3(0f, 0.97f + i * 0.13f, 0f), new Vector3(0.45f, 0.12f, 0.45f), new Color(0.8f, 0.68f, 0.45f), 45f, "Cesta_bambu");
                break;
            default:
                KitchenProps.C(t, new Vector3(0f, 0.45f, 0f), new Vector3(0.8f, 0.9f, 0.7f), EqSteel, 0f, "Cuerpo");
                break;
        }
        KitchenProps.Fit(t);
        WorldLabel wl = t.gameObject.AddComponent<WorldLabel>(); wl.text = char.ToUpper(name[0]) + name.Substring(1); wl.height = top - 0.3f;
        return t;
    }
}
