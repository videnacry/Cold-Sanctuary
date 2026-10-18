using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// MODELO CORPORAL DE CUBOS (estilo Minecraft) — construye el cuerpo de un anyma como un GRUPO DE CUBOS, uno por
/// PARTE del cuerpo (`BodyPart`), y **cablea el `CreatureRig`** para que las **asanas/yoga** (que rotan partes) funcionen
/// sin modelos 3D. Hay un **plan** por tipo de cuerpo (bípedo humano / cuadrúpedo / hexápodo insecto / pez / ave) con
/// proporciones y posiciones **a escala** (normalizadas a ~1 de alto; el tamaño real lo pone `LifeStage`/`Physiognomy`
/// al Play, o el root scale). Los insectos llevan **6 patas** (LimbA–F), las aves alas, etc. docs/anyma-factory-and-control §1.
/// </summary>
public static class CubeBody
{
    struct Part { public BodyPart part; public Vector3 pos, scale; public Part(BodyPart p, Vector3 po, Vector3 s) { part = p; pos = po; scale = s; } }

    // Plan por especie (heurística por clave de catálogo). Default = bípedo (humanos/compañeros).
    public enum Plan { Biped, Quadruped, Insect, Fish, Bird }

    public static Plan PlanFor(string species)
    {
        if (string.IsNullOrEmpty(species)) return Plan.Biped;
        string s = species.ToLowerInvariant();
        if (s.Contains("ant") || s.Contains("aphid") || s.Contains("ladybug") || s.Contains("spider") || s.Contains("cricket") ||
            s.Contains("termite") || s.Contains("beetle") || s.Contains("antlion") || s.Contains("mantis") || s.Contains("grasshopper") ||
            s.Contains("bee")) return Plan.Insect;
        if (s.Contains("whale") || s.Contains("seal") || s.Contains("orca") || s.Contains("fish")) return Plan.Fish;
        if (s.Contains("penguin") || s.Contains("bird") || s.Contains("gallina")) return Plan.Bird;
        if (s.Contains("human") || s.Contains("meerkat") || s.Contains("mono") || s.Contains("panterilia") ||
            s.Contains("goluis") || s.Contains("irosene")) return Plan.Biped;
        return Plan.Quadruped;   // wolf/bunny/fox/deer/bear/malamute…
    }

    public static void Build(GameObject root, string species)
    {
        List<Part> parts = PartsOf(PlanFor(species));
        CreatureRig rig = root.GetOrAdd<CreatureRig>();
        foreach (Part p in parts)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = p.part.ToString();
            cube.transform.SetParent(root.transform, false);
            cube.transform.localPosition = p.pos;
            cube.transform.localScale = p.scale;
            rig.bindings.Add(new CreatureRig.PartBinding { part = p.part, bone = cube.transform });
        }
        rig.Rebuild();
        Differentiate(root, species, rig);   // silueta propia de la especie (orejas, volumen, hocico…)
    }

    // Formas REPRESENTATIVAS por especie (sobre el plan): cada factory tiene su "receta" para parecerse al animal real
    // aun sin modelos 3D (osos voluminosos, conejos con orejotas, zorros con hocico, etc.). La escala global la pone
    // Physiognomy/LifeStage; aquí ajustamos PROPORCIONES y añadimos partes (orejas) a los mamíferos.
    static void Differentiate(GameObject root, string species, CreatureRig rig)
    {
        string s = (species ?? "").ToLowerInvariant();
        Transform chest = rig.Get(BodyPart.Chest);
        Transform head = rig.Get(BodyPart.Head);

        // Volumen del tronco (bulk) y tamaño de cabeza por especie.
        float bulk = 1f, headMul = 1f, earW = 0f, earH = 0f, earZ = 0f;
        if (s.Contains("bear"))      { bulk = 1.4f; headMul = 1.2f; earW = 0.1f; earH = 0.1f; }   // oso: voluminoso, orejas redondas
        else if (s.Contains("wolf") || s.Contains("malamute")) { bulk = 0.95f; earW = 0.08f; earH = 0.12f; }  // lobo: estilizado, orejas puntiagudas
        else if (s.Contains("fox"))  { bulk = 0.8f; headMul = 0.9f; earW = 0.09f; earH = 0.15f; }   // zorro: fino, orejas grandes
        else if (s.Contains("bunny")){ bulk = 0.8f; headMul = 0.9f; earW = 0.07f; earH = 0.3f; earZ = 0.05f; } // conejo: orejotas
        else if (s.Contains("deer")) { bulk = 0.9f; earW = 0.07f; earH = 0.18f; }                    // ciervo: orejas altas
        else if (s.Contains("meerkat")) { bulk = 0.75f; headMul = 1.05f; earW = 0.06f; earH = 0.08f; }

        if (chest != null) chest.localScale = Vector3.Scale(chest.localScale, new Vector3(bulk, 1f, Mathf.Lerp(1f, 1.2f, bulk - 1f + 0.3f)));
        if (head != null)  head.localScale *= headMul;

        // Orejas (solo mamíferos con earH>0): dos cubos pequeños sobre la cabeza.
        if (head != null && earH > 0.001f)
        {
            Vector3 hp = head.localPosition;
            AddEar(root, rig, BodyPart.EarLeft,  new Vector3(hp.x - 0.1f, hp.y + 0.15f, hp.z + earZ), new Vector3(earW, earH, earW * 0.6f));
            AddEar(root, rig, BodyPart.EarRight, new Vector3(hp.x + 0.1f, hp.y + 0.15f, hp.z + earZ), new Vector3(earW, earH, earW * 0.6f));
        }
    }

    static void AddEar(GameObject root, CreatureRig rig, BodyPart part, Vector3 pos, Vector3 scale)
    {
        GameObject ear = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ear.name = part.ToString(); ear.transform.SetParent(root.transform, false);
        ear.transform.localPosition = pos; ear.transform.localScale = scale;
        rig.bindings.Add(new CreatureRig.PartBinding { part = part, bone = ear.transform });
    }

    static List<Part> PartsOf(Plan plan)
    {
        switch (plan)
        {
            case Plan.Insect: return new List<Part> {
                new Part(BodyPart.Head,  new Vector3(0f, 0.28f, 0.16f), new Vector3(0.14f, 0.12f, 0.12f)),
                new Part(BodyPart.AntennaLeft,  new Vector3(-0.05f, 0.4f, 0.22f), new Vector3(0.02f, 0.14f, 0.02f)),
                new Part(BodyPart.AntennaRight, new Vector3( 0.05f, 0.4f, 0.22f), new Vector3(0.02f, 0.14f, 0.02f)),
                new Part(BodyPart.Chest, new Vector3(0f, 0.26f, 0f),   new Vector3(0.18f, 0.14f, 0.2f)),   // tórax
                new Part(BodyPart.Tail,  new Vector3(0f, 0.24f, -0.22f), new Vector3(0.16f, 0.14f, 0.26f)), // abdomen
                new Part(BodyPart.LimbA, new Vector3(-0.14f, 0.12f, 0.08f), new Vector3(0.03f, 0.2f, 0.03f)),
                new Part(BodyPart.LimbB, new Vector3( 0.14f, 0.12f, 0.08f), new Vector3(0.03f, 0.2f, 0.03f)),
                new Part(BodyPart.LimbC, new Vector3(-0.15f, 0.12f, 0f),    new Vector3(0.03f, 0.2f, 0.03f)),
                new Part(BodyPart.LimbD, new Vector3( 0.15f, 0.12f, 0f),    new Vector3(0.03f, 0.2f, 0.03f)),
                new Part(BodyPart.LimbE, new Vector3(-0.14f, 0.12f, -0.08f),new Vector3(0.03f, 0.2f, 0.03f)),
                new Part(BodyPart.LimbF, new Vector3( 0.14f, 0.12f, -0.08f),new Vector3(0.03f, 0.2f, 0.03f)),
            };
            case Plan.Fish: return new List<Part> {
                new Part(BodyPart.Head,  new Vector3(0f, 0.5f, 0.6f),  new Vector3(0.4f, 0.4f, 0.4f)),
                new Part(BodyPart.Chest, new Vector3(0f, 0.5f, 0f),    new Vector3(0.5f, 0.5f, 1.0f)),   // cuerpo
                new Part(BodyPart.Tail,  new Vector3(0f, 0.5f, -0.8f), new Vector3(0.1f, 0.5f, 0.4f)),   // cola
                new Part(BodyPart.LimbA, new Vector3(-0.3f, 0.5f, 0.1f), new Vector3(0.3f, 0.05f, 0.25f)), // aleta L
                new Part(BodyPart.LimbB, new Vector3( 0.3f, 0.5f, 0.1f), new Vector3(0.3f, 0.05f, 0.25f)), // aleta R
            };
            case Plan.Bird: return new List<Part> {
                new Part(BodyPart.Head,  new Vector3(0f, 0.95f, 0.1f), new Vector3(0.22f, 0.22f, 0.22f)),
                new Part(BodyPart.Chest, new Vector3(0f, 0.6f, 0f),    new Vector3(0.3f, 0.45f, 0.4f)),
                new Part(BodyPart.ShoulderLeft,  new Vector3(-0.22f, 0.6f, 0f), new Vector3(0.1f, 0.3f, 0.25f)), // ala L
                new Part(BodyPart.ShoulderRight, new Vector3( 0.22f, 0.6f, 0f), new Vector3(0.1f, 0.3f, 0.25f)), // ala R
                new Part(BodyPart.FootLeft,  new Vector3(-0.08f, 0.15f, 0f), new Vector3(0.06f, 0.3f, 0.06f)),
                new Part(BodyPart.FootRight, new Vector3( 0.08f, 0.15f, 0f), new Vector3(0.06f, 0.3f, 0.06f)),
                new Part(BodyPart.Tail,  new Vector3(0f, 0.55f, -0.28f), new Vector3(0.2f, 0.08f, 0.25f)),
            };
            case Plan.Quadruped: return new List<Part> {
                new Part(BodyPart.Head,  new Vector3(0f, 0.75f, 0.5f),  new Vector3(0.28f, 0.28f, 0.3f)),
                new Part(BodyPart.Chest, new Vector3(0f, 0.6f, 0f),     new Vector3(0.4f, 0.4f, 0.9f)),   // tronco
                new Part(BodyPart.Tail,  new Vector3(0f, 0.6f, -0.6f),  new Vector3(0.08f, 0.08f, 0.4f)),
                new Part(BodyPart.LimbA, new Vector3(-0.18f, 0.25f, 0.35f), new Vector3(0.1f, 0.5f, 0.1f)), // patas
                new Part(BodyPart.LimbB, new Vector3( 0.18f, 0.25f, 0.35f), new Vector3(0.1f, 0.5f, 0.1f)),
                new Part(BodyPart.LimbC, new Vector3(-0.18f, 0.25f, -0.35f),new Vector3(0.1f, 0.5f, 0.1f)),
                new Part(BodyPart.LimbD, new Vector3( 0.18f, 0.25f, -0.35f),new Vector3(0.1f, 0.5f, 0.1f)),
            };
            default: return new List<Part> {   // Biped (humano) — proporciones ~1.7 de alto, con partes para asanas
                new Part(BodyPart.Head,  new Vector3(0f, 1.55f, 0f), new Vector3(0.25f, 0.25f, 0.25f)),
                new Part(BodyPart.Chest, new Vector3(0f, 1.2f, 0f),  new Vector3(0.4f, 0.5f, 0.25f)),
                new Part(BodyPart.Hips,  new Vector3(0f, 0.85f, 0f), new Vector3(0.35f, 0.25f, 0.25f)),
                new Part(BodyPart.ShoulderLeft,  new Vector3(-0.3f, 1.1f, 0f), new Vector3(0.12f, 0.45f, 0.12f)), // brazo L
                new Part(BodyPart.ShoulderRight, new Vector3( 0.3f, 1.1f, 0f), new Vector3(0.12f, 0.45f, 0.12f)), // brazo R
                new Part(BodyPart.HandLeft,  new Vector3(-0.3f, 0.82f, 0f), new Vector3(0.1f, 0.1f, 0.1f)),
                new Part(BodyPart.HandRight, new Vector3( 0.3f, 0.82f, 0f), new Vector3(0.1f, 0.1f, 0.1f)),
                new Part(BodyPart.FootLeft,  new Vector3(-0.12f, 0.35f, 0f), new Vector3(0.14f, 0.7f, 0.14f)), // pierna L
                new Part(BodyPart.FootRight, new Vector3( 0.12f, 0.35f, 0f), new Vector3(0.14f, 0.7f, 0.14f)), // pierna R
            };
        }
    }
}
