using UnityEditor;
using UnityEngine;

/// <summary>
/// EL MICROCOSMOS DE LA PLANTA ATÓMICA (docs/atomic-plant.md §Microcosmos): la planta ×10 (la piscina del reactor es
/// una montaña de hormigón) y, en el hueco de su suelo, tres grandes de la física a escala de hormiga, cada uno con su
/// experimento JUGABLE: Newton y su cañón (la órbita), Marie Curie y su electrómetro (la radiactividad), Einstein y su
/// balanza (E = mc² en la fisión). Se entra desde la máquina de virtualización de la planta. (Eratóstenes, el primer
/// tramo de la física, tiene su propio microcosmos.)
/// </summary>
public static class PhysicsSceneBuilder
{
    public const string SceneName = "Microcosmos_PlantaAtomica";

    [MenuItem("Tools/Cold Sanctuary/Build Planta Atómica (Newton, Curie, Einstein)")]
    public static void BuildAll()
    {
        MicrocosmosAreaSceneBuilder.BuildStoryScene("AtomicPlant_Area", "Planta Atómica", SceneName, new Color(0.8f, 0.82f, 0.78f), new Vector3(6000f, 0f, 11000f), Story);
        Debug.Log("[Planta] microcosmos de la física generado.");
    }

    static readonly Color Wood = new Color(0.55f, 0.42f, 0.3f), Brass = new Color(0.8f, 0.65f, 0.3f), Iron = new Color(0.3f, 0.3f, 0.32f);

    static int Story(Transform t)
    {
        int n = 0;

        // ── NEWTON: una montaña con el cañón y, delante, la Tierra a escala para ver la trayectoria ──
        Transform nw = Spot(t, "Newton · la gravitación", new Vector3(-16f, 0f, 8f));
        Material rock = AreaMicrocosmos.Mat("Fis_Roca", new Color(0.55f, 0.5f, 0.45f));
        AreaMicrocosmos.Box(nw, "Pedestal", new Vector3(0f, 1.8f, 3f), new Vector3(2f, 3.6f, 2f), rock, true);
        GameObject earthGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        earthGo.name = "Tierra_a_escala"; earthGo.transform.SetParent(nw, false);
        earthGo.transform.localPosition = new Vector3(0f, 7.5f, 3f); earthGo.transform.localScale = Vector3.one * 8f;
        Object.DestroyImmediate(earthGo.GetComponent<Collider>());
        earthGo.GetComponent<Renderer>().sharedMaterial = AreaMicrocosmos.Mat("Fis_Tierra", new Color(0.25f, 0.45f, 0.75f));
        AreaMicrocosmos.Box(earthGo.transform, "Montaña_de_Newton", new Vector3(0f, 0.52f, 0f), new Vector3(0.06f, 0.06f, 0.06f), rock, false);
        GameObject newton = AreaMicrocosmos.Ant(t, "Isaac Newton", nw.localPosition + new Vector3(2.2f, 0f, 0.5f), new Color(0.4f, 0.27f, 0.18f), 1.8f, 1f);
        CubeInsect.Wear1940s(newton, new Color(0.55f, 0.15f, 0.15f), CubeInsect.Hat.None); n++;   // casaca roja (el cuerpo de cubos aún no tiene peluca)
        NewtonCannonLesson nl = nw.gameObject.AddComponent<NewtonCannonLesson>();
        nl.teacher = "Isaac Newton"; nl.topic = "la gravedad: ¿por qué no se cae la Luna?"; nl.teacherBody = newton.transform;
        nl.earth = earthGo.transform; nl.earthRadiusModel = 4f;
        Label(nw, "Newton: el cañón en la montaña (1687)", 12.5f);

        // ── CURIE: el cobertizo (mesa, condensador, electrómetro, cuarzo con su pesa y cronómetro) ──
        Transform cu = Spot(t, "Curie · la radiactividad", new Vector3(16f, 0f, 8f));
        Material wood = AreaMicrocosmos.Mat("Fis_Madera", Wood), brass = AreaMicrocosmos.Mat("Fis_Laton", Brass), iron = AreaMicrocosmos.Mat("Fis_Hierro", Iron);
        AreaMicrocosmos.Box(cu, "Suelo_cobertizo", new Vector3(0f, 0.05f, 2f), new Vector3(7f, 0.1f, 5f), wood, true);
        AreaMicrocosmos.Box(cu, "Tejado_cobertizo", new Vector3(0f, 3.6f, 2f), new Vector3(7.4f, 0.15f, 5.4f), Quaternion.Euler(-6f, 0f, 0f), AreaMicrocosmos.Mat("Fis_Cristal", new Color(0.75f, 0.85f, 0.9f)), false);
        for (int s = -1; s <= 1; s += 2) AreaMicrocosmos.Box(cu, "Poste", new Vector3(s * 3.4f, 1.8f, 4.3f), new Vector3(0.2f, 3.6f, 0.2f), wood, true);
        AreaMicrocosmos.Box(cu, "Mesa", new Vector3(0f, 0.45f, 2.5f), new Vector3(3f, 0.9f, 1.2f), wood, true);
        AreaMicrocosmos.Box(cu, "Condensador", new Vector3(-0.9f, 1.05f, 2.5f), new Vector3(0.5f, 0.3f, 0.5f), brass, false);
        AreaMicrocosmos.Box(cu, "Electrometro", new Vector3(0f, 1.15f, 2.5f), new Vector3(0.45f, 0.5f, 0.45f), brass, false);
        AreaMicrocosmos.Box(cu, "Cuarzo_piezoelectrico", new Vector3(0.9f, 1.3f, 2.5f), new Vector3(0.1f, 0.8f, 0.1f), AreaMicrocosmos.Mat("Fis_Cuarzo", new Color(0.92f, 0.92f, 0.95f)), false);
        AreaMicrocosmos.Box(cu, "Pesa", new Vector3(0.9f, 0.95f, 2.5f), new Vector3(0.25f, 0.15f, 0.25f), iron, false);
        AreaMicrocosmos.Box(cu, "Regla_luz", new Vector3(0f, 1.6f, 3f), new Vector3(1.4f, 0.12f, 0.04f), AreaMicrocosmos.Mat("Fis_Regla", new Color(0.95f, 0.92f, 0.8f)), false);
        GameObject marie = AreaMicrocosmos.Ant(t, "Marie Curie", cu.localPosition + new Vector3(0f, 0f, 0.6f), new Color(0.38f, 0.25f, 0.17f), 1.7f, 0.95f);
        CubeInsect.Wear1940s(marie, new Color(0.12f, 0.12f, 0.14f), CubeInsect.Hat.None); n++;   // su vestido oscuro de laboratorio
        GameObject pierre = AreaMicrocosmos.Ant(t, "Pierre Curie", cu.localPosition + new Vector3(2.4f, 0f, 1f), new Color(0.4f, 0.27f, 0.18f), 1.8f, 1f);
        CubeInsect.Wear1940s(pierre, new Color(0.18f, 0.18f, 0.2f), CubeInsect.Hat.None); n++;
        CurieElectrometerLesson cl = cu.gameObject.AddComponent<CurieElectrometerLesson>();
        cl.teacher = "Marie Curie"; cl.topic = "la radiactividad: medir lo invisible"; cl.teacherBody = marie.transform;
        Label(cu, "El cobertizo de los Curie (1898)", 4.4f);

        // ── EINSTEIN: la balanza de la fisión ──
        Transform ei = Spot(t, "Einstein · E = mc²", new Vector3(0f, 0f, 22f));
        AreaMicrocosmos.Box(ei, "Pie_balanza", new Vector3(0f, 1.2f, 0f), new Vector3(0.3f, 2.4f, 0.3f), brass, true);
        AreaMicrocosmos.Box(ei, "Base_balanza", new Vector3(0f, 0.1f, 0f), new Vector3(1.6f, 0.2f, 1f), wood, true);
        Transform beam = new GameObject("Brazo").transform; beam.SetParent(ei, false); beam.localPosition = new Vector3(0f, 2.45f, 0f);
        AreaMicrocosmos.Box(beam, "Brazo_barra", Vector3.zero, new Vector3(4f, 0.1f, 0.1f), brass, false);
        Transform left = Pan(beam, "Platillo_antes", -1.9f, brass), right = Pan(beam, "Platillo_despues", 1.9f, brass);
        GameObject einstein = AreaMicrocosmos.Ant(t, "Albert Einstein", ei.localPosition + new Vector3(0f, 0f, -2.4f), new Color(0.4f, 0.27f, 0.18f), 1.75f, 1f);
        CubeInsect.Wear1940s(einstein, new Color(0.45f, 0.42f, 0.38f), CubeInsect.Hat.None); n++;
        EinsteinMassLesson el = ei.gameObject.AddComponent<EinsteinMassLesson>();
        el.teacher = "Albert Einstein"; el.topic = "E = mc²: de dónde sale la energía del átomo"; el.teacherBody = einstein.transform;
        el.beam = beam; el.leftPan = left; el.rightPan = right;
        Label(ei, "La balanza de Einstein (1905)", 4f);

        // Los mirmidones de la planta (el elenco de la enfermería renacido) escuchan a los maestros.
        string[] cast = { "Sakshi", "Ambrosio", "Medea", "Momo", "Ruth", "Héspero" };
        for (int i = 0; i < cast.Length; i++)
        {
            float a = (210f + i * 24f) * Mathf.Deg2Rad;
            AreaMicrocosmos.Ant(t, cast[i], new Vector3(Mathf.Cos(a) * 9f, 0f, 8f + Mathf.Sin(a) * 9f), new Color(0.4f, 0.26f, 0.18f), 1.75f, 1f); n++;
        }

        var card = t.gameObject.AddComponent<HistoryCard>();
        var m = t.gameObject.AddComponent<PhysicsHistoryMission>();
        m.lessons = new PhysicsLesson[] { nl, cl, el };
        card.title = m.title;
        card.intro = "La planta atómica, vista desde el suelo: la piscina del reactor es una montaña de hormigón. Aquí te esperan tres " +
                     "personas que cambiaron lo que sabemos de la materia y la energía. Acércate a cada una y pulsa E: Newton te dará un " +
                     "cañón; Marie Curie, su electrómetro; Einstein, una balanza. Hazlo tú y entenderás por qué esta planta funciona.";
        card.history = new[]
        {
            "Isaac Newton (1643–1727) publicó en 1687 los «Principia»: la misma fuerza que hace caer una manzana mantiene a la Luna en su órbita, y disminuye con el cuadrado de la distancia. La Luna está a unos 60 radios terrestres, así que cae 3600 veces menos que una piedra. En su «Tratado del sistema del mundo» imaginó el cañón en la montaña: si la bala va lo bastante rápido, ya no vuelve al suelo.",
            "En 1896 Henri Becquerel vio que las sales de uranio velaban placas fotográficas sin luz. Marie Skłodowska Curie (1867–1934) decidió MEDIRLO: con el cuarzo piezoeléctrico que Pierre y Jacques Curie habían descubierto en 1880, midió corrientes de 10⁻¹¹ amperios. En 1898 vio que la pecblenda era más activa que el propio uranio, y con Pierre encontró dos elementos nuevos: el polonio y el radio. Ella inventó la palabra «radiactividad».",
            "Marie Curie fue la primera mujer en ganar un Nobel (Física, 1903, con Pierre y Becquerel) y la primera persona en ganar dos (Química, 1911). Murió en 1934 de una anemia aplásica, probablemente por la radiación que recibió toda su vida: entonces no se conocía el riesgo. Por eso hoy, en esta planta, todos llevan dosímetro.",
            "Albert Einstein (1879–1955) publicó en 1905 que la masa es una forma de energía: E = mc². A finales de 1938 Otto Hahn y Fritz Strassmann partieron el uranio con neutrones, y Lise Meitner y Otto Frisch explicaron la fisión y calcularon con la fórmula de Einstein la energía liberada: unos 200 MeV por núcleo. Esa energía es la que calienta el agua de la piscina.",
            "La planta del santuario es un reactor de investigación de piscina, como los TRIGA de las universidades (de 1 MW): no da electricidad, da neutrones para fabricar isótopos médicos para el hospital, analizar muestras y enseñar. Su combustible (uranio en hidruro de circonio) frena la reacción solo si se calienta: es su seguridad inherente.",
        };
        card.sources = new[]
        {
            "M. Skłodowska Curie, «Rayons émis par les composés de l'uranium et du thorium», Comptes Rendus 126, 1101–1103 (1898); trad. C. Giunta (Le Moyne College, «Elements and Atoms», cap. 18).",
            "Musée Curie / Google Arts & Culture: «How did the Curies measure radioactivity?» y «The Curies and the discovery of radium».",
            "A. Einstein, «Ist die Trägheit eines Körpers von seinem Energieinhalt abhängig?», Annalen der Physik 18, 639–641 (1905). L. Meitner y O. R. Frisch, Nature 143, 239–240 (1939).",
            "I. Newton, Philosophiæ Naturalis Principia Mathematica (1687) y A Treatise of the System of the World (1728).",
            "UT-Austin NETL (TRIGA Mark II, 1,1 MW) y Penn State Breazeale Reactor (TRIGA, 1 MW): fichas de las instalaciones. Masas atómicas: AME2020.",
        };
        return n;
    }

    static Transform Pan(Transform beam, string name, float x, Material m)
    {
        Transform p = new GameObject(name).transform; p.SetParent(beam, false); p.localPosition = new Vector3(x, -0.9f, 0f);
        AreaMicrocosmos.Box(p, "Cuerda", new Vector3(0f, 0.45f, 0f), new Vector3(0.03f, 0.9f, 0.03f), m, false);
        AreaMicrocosmos.Box(p, "Plato", Vector3.zero, new Vector3(1.2f, 0.06f, 1.2f), m, false);
        return p;
    }

    static Transform Spot(Transform parent, string name, Vector3 local)
    {
        Transform s = new GameObject(name).transform; s.SetParent(parent, false); s.localPosition = local; return s;
    }

    static void Label(Transform t, string text, float height)
    {
        WorldLabel wl = t.gameObject.AddComponent<WorldLabel>(); wl.text = text; wl.height = height;
    }
}
