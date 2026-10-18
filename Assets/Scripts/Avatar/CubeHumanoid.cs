using UnityEngine;

/// <summary>
/// CUERPO HUMANO DE CUBOS ARTICULADO (estilo Minecraft, sin .fbx ni esqueleto del modelo 3D): un cubo por segmento
/// —pelvis, torso, cuello, cabeza, brazo, antebrazo, mano, muslo, espinilla, pie— colgados en JERARQUÍA de sus
/// articulaciones (<see cref="CubeJoint"/>): girar el hombro mueve brazo+antebrazo+mano, girar la rodilla mueve
/// espinilla+pie. Cada articulación se registra en el <see cref="CreatureRig"/> con su <see cref="BodyPart"/>, así que
/// el yoga (<c>AsanaPoseSpell</c>), la emoción y la posesión mueven "la rodilla izquierda" sin saber qué cuerpo es.
///
/// <see cref="Dress"/> viste a un personaje existente (Kushal, la Magnate, compañeros): oculta su malla actual
/// (cápsula o .fbx), construye el cuerpo de cubos a su altura y cablea el rig. Funciona en editor (builder) y en Play.
/// </summary>
public static class CubeHumanoid
{
    public const string BodyName = "CuerpoCubos";
    const float NativeHeight = 1.9f;   // altura del cuerpo construido antes de escalar

    /// <summary>Oculta la malla actual de <paramref name="who"/> y le pone un cuerpo de cubos articulado a su altura.</summary>
    public static CreatureRig Dress(GameObject who, Color clothes, Color skin, float height = 0f)
    {
        CubeHair.Look look = CubeHair.For(who.name);
        skin = look.skin;                              // cada persona con su tono de piel…
        if (look.hasClothes) clothes = look.clothes;   // …su ropa (bata, chaqueta de cocina, tejido andino…)
        if (height <= 0f && look.height > 0f) height = look.height;   // …y su estatura (docs/diseno-personajes.md)
        Transform old = who.transform.Find(BodyName);
        // VOLVER A VESTIR: el cuerpo anterior dice dónde están los pies (la malla original ya está oculta).
        float? oldFeet = old != null ? old.localPosition.y * who.transform.lossyScale.y : (float?)null;
        if (old != null) Object.DestroyImmediate(old.gameObject);

        // Altura y suelo a partir de lo que se veía (cápsula centrada en el origen o .fbx con los pies en el origen).
        Bounds b = new Bounds(who.transform.position, Vector3.zero); bool any = false;
        foreach (Renderer r in who.GetComponentsInChildren<Renderer>())
        {
            if (!r.enabled || r is ParticleSystemRenderer) continue;
            if (!any) { b = r.bounds; any = true; } else b.Encapsulate(r.bounds);
            r.enabled = false;   // se queda el componente (por si se quiere volver a la malla); deja de verse
        }
        float h = height > 0f ? height : (any && b.size.y > 0.3f ? b.size.y : 1.75f);
        float feetY = oldFeet ?? (any ? b.min.y - who.transform.position.y : 0f);

        GameObject body = new GameObject(BodyName);
        body.transform.SetParent(who.transform, false);
        float s = h / NativeHeight / Mathf.Max(0.01f, who.transform.lossyScale.y);
        body.transform.localPosition = new Vector3(0f, feetY / Mathf.Max(0.01f, who.transform.lossyScale.y), 0f);
        body.transform.localScale = Vector3.one * s;

        CreatureRig rig = who.GetOrAdd<CreatureRig>();
        rig.humanoidAnimator = null;      // manda el cuerpo de cubos, no los huesos del .fbx
        rig.bindings.Clear();
        Build(body.transform, rig, clothes, skin);
        rig.Rebuild();
        if (who.GetComponent<CubeWalkCycle>() == null) who.AddComponent<CubeWalkCycle>();   // piernas y brazos al andar
        if (who.GetComponent<BodyShape>() == null && who.GetComponent<Magnate>() == null) who.AddComponent<BodyShape>();   // las stats físicas moldean el cuerpo
        return rig;
    }

    static Material Mat(Color c)
    {
        Shader sh = Shader.Find("Universal Render Pipeline/Lit");
        if (sh == null) sh = Shader.Find("Standard");
        var m = new Material(sh) { color = c };
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        return m;
    }

    static void Build(Transform root, CreatureRig rig, Color clothes, Color skin)
    {
        CubeHair.Look look = CubeHair.For(root.parent != null ? root.parent.name : "");
        Material cloth = Mat(clothes), flesh = Mat(skin), dark = look.hasPants ? Mat(look.pants) : Mat(clothes * 0.6f + new Color(0, 0, 0, 1));
        Material forearm = look.hasSleeves ? Mat(look.sleeves) : flesh;   // manga larga (camiseta interior) o brazo al aire
        Material upperArm = look.hasUpperSleeves ? Mat(look.upperSleeves) : cloth;   // chaleco: las mangas son de la camiseta de debajo
        Material belly = look.hasWaist ? Mat(look.waist) : cloth;                    // corsé en la cintura
        Material shin = look.hasBoots ? Mat(look.boots) : look.shorts ? flesh : dark;   // botas altas / pantalón corto
        Material shoe = look.hasBoots ? Mat(look.boots) : look.hasShoes ? Mat(look.shoes) : look.shorts ? Mat(new Color(0.9f, 0.9f, 0.9f)) : dark;

        // Pelvis (cadera) a la altura de la cadera; todo cuelga de aquí.
        CubeJoint hips  = Joint(root, BodyPart.Hips,  new Vector3(0f, 0.95f, 0f), new Vector3(0.34f, 0.2f, 0.22f), Vector3.zero, dark, rig);
        // Columna en DOS tramos (lumbar = Back, torácica = Chest) para que flexiones y extensiones se repartan y se curven.
        CubeJoint lumbar = Joint(hips.transform, BodyPart.Back, new Vector3(0f, 0.1f, 0f), new Vector3(0.36f, 0.24f, 0.22f), new Vector3(0f, 0.12f, 0f), belly, rig);
        CubeJoint waist = Joint(lumbar.transform, BodyPart.Chest, new Vector3(0f, 0.24f, 0f), new Vector3(0.4f, 0.3f, 0.24f), new Vector3(0f, 0.15f, 0f), cloth, rig);
        CubeJoint neck  = Joint(waist.transform, BodyPart.Neck, new Vector3(0f, 0.28f, 0f), new Vector3(0.1f, 0.08f, 0.1f), new Vector3(0f, 0.04f, 0f), flesh, rig);
        CubeJoint head = Joint(neck.transform, BodyPart.Head, new Vector3(0f, 0.08f, 0f), new Vector3(0.24f, 0.26f, 0.24f), new Vector3(0f, 0.13f, 0f), flesh, rig);
        // Cara de cubos (ojos, párpados, cejas, nariz, mandíbula, comisuras) → expresiones faciales (CubeFace).
        CubeFace.Build(head.transform, new Vector3(0f, 0.13f, 0f), new Vector3(0.24f, 0.26f, 0.24f), 0f, skin, look.eyes, rig, null, null, look.hasLips ? look.lips : (Color?)null, look.feminine, Color.Lerp(look.hair, Color.black, 0.35f));
        CubeHair.Build(head.transform, new Vector3(0f, 0.13f, 0f), new Vector3(0.24f, 0.26f, 0.24f), look);   // cabello (y barba) de cubos

        for (int side = -1; side <= 1; side += 2)
        {
            bool left = side < 0;
            // Escápula (pivote junto a la base del cuello): rodar/encoger el hombro sin mover el brazo. En reposo es
            // transparente (rotación identidad), así que no altera las demás posturas; de ella cuelga el brazo.
            CubeJoint scap = Joint(waist.transform, left ? BodyPart.ScapulaLeft : BodyPart.ScapulaRight,
                new Vector3(0.08f * side, 0.26f, 0f), new Vector3(0.14f, 0.1f, 0.12f), new Vector3(0.09f * side, -0.02f, 0f), cloth, rig);
            // Brazo: escápula → hombro → codo → muñeca.
            CubeJoint sh = Joint(scap.transform, left ? BodyPart.ShoulderLeft : BodyPart.ShoulderRight,
                new Vector3(0.18f * side, -0.04f, 0f), new Vector3(0.1f, 0.3f, 0.1f), new Vector3(0f, -0.15f, 0f), upperArm, rig);
            CubeJoint el = Joint(sh.transform, left ? BodyPart.ForearmLeft : BodyPart.ForearmRight,
                new Vector3(0f, -0.3f, 0f), new Vector3(0.09f, 0.27f, 0.09f), new Vector3(0f, -0.135f, 0f), forearm, rig);
            if (look.tattoos && !look.hasSleeves) Tattoo(el.transform, side);
            Joint(el.transform, left ? BodyPart.HandLeft : BodyPart.HandRight,
                new Vector3(0f, -0.27f, 0f), new Vector3(0.08f, 0.1f, 0.05f), new Vector3(0f, -0.05f, 0f), flesh, rig);
            // Pierna: cadera → rodilla → tobillo.
            CubeJoint th = Joint(hips.transform, left ? BodyPart.ThighLeft : BodyPart.ThighRight,
                new Vector3(0.1f * side, -0.05f, 0f), new Vector3(0.14f, 0.42f, 0.14f), new Vector3(0f, -0.21f, 0f), dark, rig);
            CubeJoint kn = Joint(th.transform, left ? BodyPart.ShinLeft : BodyPart.ShinRight,
                new Vector3(0f, -0.42f, 0f), new Vector3(0.12f, 0.4f, 0.12f), new Vector3(0f, -0.2f, 0f), shin, rig);
            Joint(kn.transform, left ? BodyPart.FootLeft : BodyPart.FootRight,
                new Vector3(0f, -0.4f, 0f), new Vector3(0.12f, 0.06f, 0.24f), new Vector3(0f, -0.03f, 0.06f), shoe, rig);
        }

        Details(root, look);   // (el rig aún no tiene el mapa de huesos: se buscan por su articulación)
        foreach (CubeJoint j in root.GetComponentsInChildren<CubeJoint>()) j.Capture();
    }


    // TATUAJES en el antebrazo: unos trazos de tinta sobre la piel (por fuera del brazo), que se mueven con él.
    static void Tattoo(Transform forearm, int side)
    {
        Material ink = Mat(new Color(0.16f, 0.17f, 0.22f));
        for (int i = 0; i < 4; i++)
        {
            GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
            g.name = "Tatuaje";
            Object.DestroyImmediate(g.GetComponent<Collider>());
            g.transform.SetParent(forearm, false);
            g.transform.localPosition = new Vector3(side * 0.046f, -0.06f - i * 0.045f, (i % 2 == 0 ? 0.012f : -0.012f));
            g.transform.localScale = new Vector3(0.004f, 0.028f, 0.022f);
            g.transform.localRotation = Quaternion.Euler(i * 25f, 0f, 0f);
            g.GetComponent<Renderer>().sharedMaterial = ink;
        }
    }

    // DETALLES de la ropa y complementos del diseño de cada personaje (cubos pegados a sus huesos: se mueven con ellos).
    static void Details(Transform root, CubeHair.Look look)
    {
        Transform Bone(BodyPart p) { foreach (CubeJoint j in root.GetComponentsInChildren<CubeJoint>()) if (j.part == p) return j.transform; return null; }
        Transform chest = Bone(BodyPart.Chest), back = Bone(BodyPart.Back);
        if (look.hasUpperSleeves && !look.hasWaist && chest != null)   // CHALECO abierto: se ve la camiseta delante y el borrego
        {
            Deco(chest, new Vector3(0f, 0.15f, 0.122f), new Vector3(0.14f, 0.3f, 0.004f), look.upperSleeves);
            if (back != null) Deco(back, new Vector3(0f, 0.12f, 0.112f), new Vector3(0.12f, 0.24f, 0.004f), look.upperSleeves);
            for (int s = -1; s <= 1; s += 2) Deco(chest, new Vector3(s * 0.085f, 0.24f, 0.124f), new Vector3(0.035f, 0.12f, 0.006f), new Color(0.35f, 0.28f, 0.24f));   // cuello de borrego
            Deco(chest, new Vector3(0.12f, 0.2f, 0.124f), new Vector3(0.022f, 0.022f, 0.004f), new Color(0.95f, 0.8f, 0.1f));   // chapas
            Deco(chest, new Vector3(0.09f, 0.21f, 0.124f), new Vector3(0.012f, 0.012f, 0.004f), new Color(0.85f, 0.1f, 0.1f));
        }
        if (look.hasWaist && chest != null)   // BLUSA de cuello alto y escote; ribete dorado del corsé
        {
            Deco(chest, new Vector3(0f, 0.31f, -0.02f), new Vector3(0.24f, 0.08f, 0.2f), look.clothes);
            if (back != null) Deco(back, new Vector3(0f, 0.235f, 0f), new Vector3(0.37f, 0.015f, 0.225f), new Color(0.85f, 0.68f, 0.3f));
            if (back != null) Deco(back, new Vector3(0f, 0.12f, 0.112f), new Vector3(0.06f, 0.2f, 0.004f), new Color(0.85f, 0.68f, 0.3f));
        }
        if (look.upperTattoo)
        {
            Transform sh = Bone(BodyPart.ShoulderRight);
            if (sh != null) { Deco(sh, new Vector3(0.051f, -0.22f, 0f), new Vector3(0.004f, 0.045f, 0.045f), new Color(0.15f, 0.15f, 0.2f), 45f); Deco(sh, new Vector3(0.053f, -0.22f, 0f), new Vector3(0.004f, 0.014f, 0.014f), new Color(0.15f, 0.15f, 0.2f)); }
        }
        if (look.wristband) { Transform fa = Bone(BodyPart.ForearmLeft); if (fa != null) Deco(fa, new Vector3(0f, -0.24f, 0f), new Vector3(0.1f, 0.05f, 0.1f), new Color(0.12f, 0.12f, 0.14f)); }
        if (look.chain)
        {
            Transform hips = Bone(BodyPart.Hips);
            if (hips != null) for (int i = 0; i < 6; i++) Deco(hips, new Vector3(0.175f, -0.05f - i * 0.035f, 0.03f - i * 0.012f), new Vector3(0.012f, 0.022f, 0.012f), new Color(0.65f, 0.67f, 0.7f));
        }
        if (look.rifle && chest != null)   // RIFLE colgado a la espalda, en diagonal, con su correa
        {
            Transform r = new GameObject("Rifle").transform; r.SetParent(chest, false);
            r.localPosition = new Vector3(0f, 0.12f, -0.15f); r.localRotation = Quaternion.Euler(0f, 0f, 35f);
            Deco(r, new Vector3(0f, -0.25f, 0f), new Vector3(0.05f, 0.35f, 0.04f), new Color(0.45f, 0.14f, 0.18f));   // culata
            Deco(r, new Vector3(0f, 0.18f, 0f), new Vector3(0.025f, 0.6f, 0.025f), new Color(0.2f, 0.2f, 0.22f));     // cañón
            Deco(r, new Vector3(0f, 0.02f, -0.035f), new Vector3(0.035f, 0.22f, 0.035f), new Color(0.15f, 0.15f, 0.17f)); // mira
            Deco(chest, new Vector3(0f, 0.14f, 0f), new Vector3(0.42f, 0.03f, 0.26f), new Color(0.2f, 0.2f, 0.2f), 0f, 35f);   // correa cruzada
        }
    }

    static void Deco(Transform parent, Vector3 pos, Vector3 size, Color c, float xRot = 0f, float zRot = 0f)
    {
        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = "Detalle";
        Object.DestroyImmediate(g.GetComponent<Collider>());
        g.transform.SetParent(parent, false);
        g.transform.localPosition = pos; g.transform.localScale = size;
        g.transform.localRotation = Quaternion.Euler(xRot, 0f, zRot);
        g.GetComponent<Renderer>().sharedMaterial = Mat(c);
    }
    static CubeJoint Joint(Transform parent, BodyPart part, Vector3 pivot, Vector3 size, Vector3 cubeOffset, Material mat, CreatureRig rig)
    {
        GameObject jointGO = new GameObject(part.ToString());
        jointGO.transform.SetParent(parent, false);
        jointGO.transform.localPosition = pivot;

        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = part + "_cubo";
        Object.DestroyImmediate(cube.GetComponent<Collider>());   // el cuerpo choca con su CharacterController/cápsula, no por partes
        cube.transform.SetParent(jointGO.transform, false);
        cube.transform.localPosition = cubeOffset;
        cube.transform.localScale = size;
        cube.GetComponent<Renderer>().sharedMaterial = mat;

        CubeJoint j = jointGO.AddComponent<CubeJoint>();
        j.part = part; j.segment = cube.transform;
        rig.bindings.Add(new CreatureRig.PartBinding { part = part, bone = jointGO.transform });
        return j;
    }
}
