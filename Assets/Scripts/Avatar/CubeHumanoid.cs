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

    // (2026-11-09) Uno por color para todas las personas (antes, uno nuevo por pieza del cuerpo y persona), mate y opaco.
    static Material Mat(Color c) => CheapMaterials.Opaque(c, "Persona");

    static void Build(Transform root, CreatureRig rig, Color clothes, Color skin)
    {
        CubeHair.Look look = CubeHair.For(root.parent != null ? root.parent.name : "");
        Material cloth = Mat(clothes), flesh = Mat(skin), dark = look.hasPants ? Mat(look.pants) : Mat(clothes * 0.6f + new Color(0, 0, 0, 1));
        Material forearm = look.hasSleeves ? Mat(look.sleeves) : flesh;   // manga larga (camiseta interior) o brazo al aire
        Material upperArm = look.sleeveless ? flesh : look.hasUpperSleeves ? Mat(look.upperSleeves) : cloth;   // sin mangas / chaleco: las mangas son de la camiseta de debajo
        Material belly = look.hasWaist ? Mat(look.waist) : cloth;                    // corsé en la cintura
        Material shin = look.hasBoots ? Mat(look.boots) : look.shorts ? flesh : dark;   // botas altas / pantalón corto
        Material shoe = look.hasBoots ? Mat(look.boots) : look.hasShoes ? Mat(look.shoes) : look.shorts ? Mat(new Color(0.9f, 0.9f, 0.9f)) : dark;
        Material thigh = look.hasSkirt ? flesh : dark;   // con falda, los muslos al aire (la falda va sobre la cadera: Details)

        // Pelvis (cadera) a la altura de la cadera; todo cuelga de aquí.
        CubeJoint hips  = Joint(root, BodyPart.Hips,  new Vector3(0f, 0.95f, 0f), new Vector3(0.34f, 0.2f, 0.22f), Vector3.zero, dark, rig);
        // Columna en DOS tramos (lumbar = Back, torácica = Chest) para que flexiones y extensiones se repartan y se curven.
        CubeJoint lumbar = Joint(hips.transform, BodyPart.Back, new Vector3(0f, 0.1f, 0f), new Vector3(0.36f, 0.24f, 0.22f), new Vector3(0f, 0.12f, 0f), belly, rig);
        CubeJoint waist = Joint(lumbar.transform, BodyPart.Chest, new Vector3(0f, 0.24f, 0f), new Vector3(0.4f, 0.3f, 0.24f), new Vector3(0f, 0.15f, 0f), cloth, rig);
        CubeJoint neck  = Joint(waist.transform, BodyPart.Neck, new Vector3(0f, 0.28f, 0f), new Vector3(0.1f, 0.08f, 0.1f), new Vector3(0f, 0.04f, 0f), flesh, rig);
        CubeJoint head = Joint(neck.transform, BodyPart.Head, new Vector3(0f, 0.08f, 0f), new Vector3(0.24f, 0.26f, 0.24f), new Vector3(0f, 0.13f, 0f), flesh, rig);
        // Cara de cubos (ojos, párpados, cejas, nariz, mandíbula, comisuras) → expresiones faciales (CubeFace).
        // Cara de UNA pieza (la cabeza es un solo cubo; los rasgos van encima).
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
                new Vector3(0.1f * side, -0.05f, 0f), new Vector3(0.14f, 0.42f, 0.14f), new Vector3(0f, -0.21f, 0f), thigh, rig);
            CubeJoint kn = Joint(th.transform, left ? BodyPart.ShinLeft : BodyPart.ShinRight,
                new Vector3(0f, -0.42f, 0f), new Vector3(0.12f, 0.4f, 0.12f), new Vector3(0f, -0.2f, 0f), shin, rig);
            Joint(kn.transform, left ? BodyPart.FootLeft : BodyPart.FootRight,
                new Vector3(0f, -0.4f, 0f), new Vector3(0.12f, 0.06f, 0.24f), new Vector3(0f, -0.03f, 0.06f), shoe, rig);
        }

        Details(root, look);   // (el rig aún no tiene el mapa de huesos: se buscan por su articulación)
        // (Cuerpo de CUBOS: así la separación entre miembros se nota menos y la forma es más amable. Las stats se ven en el
        // grosor de cada segmento y en los relieves de músculo y grasa de BodyShape.)
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
        if (look.wristband) { Transform fa = Bone(look.hasWatch ? BodyPart.ForearmRight : BodyPart.ForearmLeft); if (fa != null) Deco(fa, new Vector3(0f, -0.24f, 0f), new Vector3(0.1f, 0.05f, 0.1f), look.band.a > 0f ? look.band : new Color(0.12f, 0.12f, 0.14f)); }   // con reloj, en la otra muñeca
        if (look.chain)
        {
            Transform hips = Bone(BodyPart.Hips);
            if (hips != null) for (int i = 0; i < 6; i++) Deco(hips, new Vector3(0.175f, -0.05f - i * 0.035f, 0.03f - i * 0.012f), new Vector3(0.012f, 0.022f, 0.012f), new Color(0.65f, 0.67f, 0.7f));
        }
        if (look.hasStole && chest != null)   // CHAL/ESTOLA: tela cruzada del hombro izquierdo a la cadera derecha, por delante y por detrás
        {
            for (int k = 0; k < 2; k++)
            {
                float zf = k == 0 ? 0.125f : -0.125f;
                Deco(chest, new Vector3(0f, 0.12f, zf), new Vector3(0.1f, 0.42f, 0.012f), look.stole, 0f, 38f);
                Deco(chest, new Vector3(0.012f, 0.12f, zf + (k == 0 ? 0.002f : -0.002f)), new Vector3(0.02f, 0.42f, 0.012f), Color.Lerp(look.stole, new Color(0.2f, 0.6f, 0.6f), 0.7f), 0f, 38f);   // ribete turquesa
            }
            Deco(chest, new Vector3(-0.17f, 0.27f, 0f), new Vector3(0.1f, 0.05f, 0.26f), look.stole);   // sobre el hombro
            if (back != null) Deco(back, new Vector3(0.1f, 0.0f, 0.118f), new Vector3(0.22f, 0.12f, 0.012f), look.stole, 0f, 25f);   // caída sobre el regazo
        }
        if (look.hasNecklace && chest != null)   // COLLAR: cadena (con cruz) o guirnalda de cuentas/flores
        {
            int beads = look.necklaceCross ? 9 : 13;
            for (int i = 0; i < beads; i++)
            {
                float u = i / (float)(beads - 1) * 2f - 1f;   // −1…1 de un lado al otro
                float y = 0.29f - (1f - u * u) * (look.necklaceCross ? 0.1f : 0.2f);
                float bead = look.necklaceCross ? 0.008f : look.beads ? 0.012f : 0.02f;
                Color bc = look.necklaceCross || look.beads ? look.necklace : (i % 3 == 0 ? new Color(0.25f, 0.6f, 0.3f) : i % 3 == 1 ? look.necklace : new Color(0.97f, 0.97f, 0.93f));
                Deco(chest, new Vector3(u * 0.09f, y, 0.122f + (1f - u * u) * 0.004f), Vector3.one * bead, bc);
            }
            if (look.necklaceCross)
            {
                Deco(chest, new Vector3(0f, 0.165f, 0.126f), new Vector3(0.008f, 0.045f, 0.004f), look.necklace);
                Deco(chest, new Vector3(0f, 0.175f, 0.126f), new Vector3(0.028f, 0.008f, 0.004f), look.necklace);
            }
        }
        if (look.rings)   // ANILLOS: aros de metal en varios dedos de las dos manos
            foreach (BodyPart hp in new[] { BodyPart.HandLeft, BodyPart.HandRight })
            {
                Transform hand = Bone(hp); if (hand == null) continue;
                for (int i = 0; i < 2; i++) Deco(hand, new Vector3(-0.02f + i * 0.03f, -0.085f, 0f), new Vector3(0.022f, 0.012f, 0.056f), i == 0 ? new Color(0.85f, 0.7f, 0.3f) : new Color(0.78f, 0.78f, 0.82f));
            }
        if (look.earring && look.rings) { Transform hd = Bone(BodyPart.Head); if (hd != null) Deco(hd, new Vector3(0.132f, 0.1f, 0f), new Vector3(0.016f, 0.016f, 0.016f), new Color(0.85f, 0.85f, 0.88f)); }   // pendiente también en la otra oreja
        FamilyDetails(Bone, chest, back, look);
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

    // ROPA Y COMPLEMENTOS de la familia y los amigos de Berón (2026-11-04, docs/diseno-personajes.md §Familia y amigos):
    // chaqueta con cremallera, cuadros/rayas/canalé, cuello de camisa, mangas remangadas, volantes, capucha, gafas colgadas,
    // cinturón, bolso cruzado, falda de flores y reloj. Medidas de los segmentos: pecho 0,4 × 0,3 × 0,24 (de y 0 a 0,3),
    // vientre 0,36 × 0,24 × 0,22 (de y 0 a 0,24), cadera 0,34 × 0,2 × 0,22 (centrada).
    static void FamilyDetails(System.Func<BodyPart, Transform> Bone, Transform chest, Transform back, CubeHair.Look look)
    {
        if (chest == null) return;
        Color clothes = look.clothes, skin = look.skin;
        if (look.jacket)
        {
            Color edge = Color.Lerp(clothes, Color.black, 0.4f), zip = new Color(0.55f, 0.55f, 0.58f);
            if (look.jacketClosed)
            {
                Deco(chest, new Vector3(0f, 0.15f, 0.122f), new Vector3(0.008f, 0.3f, 0.004f), zip);
                if (back != null) Deco(back, new Vector3(0f, 0.12f, 0.112f), new Vector3(0.008f, 0.24f, 0.004f), zip);
                Deco(chest, new Vector3(0f, 0.275f, 0.123f), new Vector3(0.07f, 0.05f, 0.004f), look.inner);   // el cuello de la camiseta asoma
            }
            else
            {
                Deco(chest, new Vector3(0f, 0.15f, 0.122f), new Vector3(0.12f, 0.3f, 0.004f), look.inner);   // la camisa, entre las dos mitades
                if (back != null) Deco(back, new Vector3(0f, 0.12f, 0.112f), new Vector3(0.11f, 0.24f, 0.004f), look.inner);
                for (int s = -1; s <= 1; s += 2)
                {
                    Deco(chest, new Vector3(s * 0.062f, 0.15f, 0.123f), new Vector3(0.008f, 0.3f, 0.005f), edge);   // bordes con la cremallera
                    if (back != null) Deco(back, new Vector3(s * 0.057f, 0.12f, 0.113f), new Vector3(0.008f, 0.24f, 0.005f), edge);
                }
                if (look.weave != CubeHair.Weave.None)
                {
                    Material pm = Mat(look.pattern);
                    WeaveOn(chest, 0.15f, 0.12f, 0.3f, 0.1245f, pm, look.weave);
                    if (back != null) WeaveOn(back, 0.12f, 0.11f, 0.24f, 0.1145f, pm, look.weave);
                }
            }
            if (look.hasTee) Deco(chest, new Vector3(0f, 0.28f, 0.126f), new Vector3(0.05f, 0.035f, 0.004f), look.tee);   // la camiseta, en el cuello
            Deco(chest, new Vector3(0f, 0.31f, -0.005f), new Vector3(0.22f, 0.05f, 0.2f), clothes);   // cuello alto de la chaqueta
        }
        else if (look.weave != CubeHair.Weave.None)
        {
            Material pm = Mat(look.pattern);
            for (int f = -1; f <= 1; f += 2)
            {
                WeaveOn(chest, 0.15f, 0.4f, 0.3f, f * 0.1225f, pm, look.weave);
                if (back != null) WeaveOn(back, 0.12f, 0.36f, 0.24f, f * 0.1125f, pm, look.weave);
            }
            if (look.weave == CubeHair.Weave.Ribs)   // el canalé del jersey también en las mangas
                foreach (BodyPart arm in new[] { BodyPart.ShoulderLeft, BodyPart.ShoulderRight, BodyPart.ForearmLeft, BodyPart.ForearmRight })
                {
                    Transform a = Bone(arm); if (a == null) continue;
                    bool fore = arm == BodyPart.ForearmLeft || arm == BodyPart.ForearmRight;
                    if (fore && !look.hasSleeves) continue;
                    float len = fore ? 0.27f : 0.3f, half = fore ? 0.046f : 0.051f;
                    for (int f = -1; f <= 1; f += 2) WeaveOn(a, -len * 0.5f, half * 2f - 0.01f, len, f * half, pm, look.weave);
                }
        }
        if (look.collar && !look.jacket)   // CUELLO de camisa o de polo; abierto, deja ver la piel en pico
        {
            Color cc = Color.Lerp(clothes, Color.white, 0.12f);
            for (int s = -1; s <= 1; s += 2) Deco(chest, new Vector3(s * 0.05f, 0.285f, 0.126f), new Vector3(0.06f, 0.03f, 0.006f), cc, 0f, s * -25f);
            if (look.openCollar) Deco(chest, new Vector3(0f, 0.272f, 0.1235f), new Vector3(0.05f, 0.05f, 0.004f), skin, 0f, 45f);
            else for (int i = 0; i < 2; i++) Deco(chest, new Vector3(0f, 0.24f - i * 0.05f, 0.125f), new Vector3(0.014f, 0.014f, 0.004f), new Color(0.9f, 0.9f, 0.9f));   // botones del polo
        }
        if (look.rolledSleeves)   // MANGAS REMANGADAS: el puño doblado bajo el codo, el antebrazo al aire
            foreach (BodyPart arm in new[] { BodyPart.ForearmLeft, BodyPart.ForearmRight })
            {
                Transform a = Bone(arm); if (a != null) Deco(a, new Vector3(0f, -0.035f, 0f), new Vector3(0.105f, 0.06f, 0.105f), clothes);
            }
        if (look.ruffles)   // VOLANTES de la blusa, en filas sobre el pecho y el vientre
        {
            Color rc = Color.Lerp(clothes, Color.gray, 0.12f);
            for (int i = 0; i < 5; i++) Deco(chest, new Vector3(0f, 0.04f + i * 0.05f, 0.128f), new Vector3(0.16f, 0.03f, 0.014f), rc, -25f);
            if (back != null) for (int i = 0; i < 3; i++) Deco(back, new Vector3(0f, 0.04f + i * 0.07f, 0.118f), new Vector3(0.14f, 0.03f, 0.014f), rc, -25f);
        }
        if (look.hood)   // CAPUCHA caída a la espalda
        {
            Deco(chest, new Vector3(0f, 0.31f, -0.13f), new Vector3(0.26f, 0.07f, 0.07f), look.hoodColor);
            Deco(chest, new Vector3(0f, 0.22f, -0.138f), new Vector3(0.22f, 0.14f, 0.03f), Color.Lerp(look.hoodColor, Color.black, 0.15f));
        }
        // (2026-11-05, de las fotos de Berón) PIEZAS NUEVAS de la ropa.
        float printZ = look.jacket ? 0.1265f : 0.1235f;
        if (look.hasLogo)   // ESTAMPADO en el pecho
        {
            Deco(chest, new Vector3(0f, 0.16f, printZ), new Vector3(0.13f, 0.07f, 0.004f), look.logo);
            for (int s = -1; s <= 1; s += 2) Deco(chest, new Vector3(s * 0.035f, 0.135f, printZ + 0.002f), new Vector3(0.03f, 0.02f, 0.004f), look.logoDetail);
        }
        if (look.hasTie)   // CORBATA
        {
            Deco(chest, new Vector3(0f, 0.17f, printZ), new Vector3(0.032f, 0.18f, 0.004f), look.tie);
            Deco(chest, new Vector3(0f, 0.262f, printZ + 0.002f), new Vector3(0.04f, 0.026f, 0.006f), Color.Lerp(look.tie, Color.black, 0.15f));
        }
        if (look.highNeck) Deco(chest, new Vector3(0f, 0.325f, 0.02f), new Vector3(0.13f, 0.05f, 0.13f), look.inner);   // CUELLO ALTO de la blusa
        if (look.puffSleeves)   // MANGAS DE FAROL
            foreach (BodyPart arm in new[] { BodyPart.ShoulderLeft, BodyPart.ShoulderRight })
            {
                Transform a = Bone(arm); if (a != null) Deco(a, new Vector3(0f, -0.06f, 0f), new Vector3(0.14f, 0.13f, 0.14f), clothes);
            }
        if (look.hasBackpack)   // MOCHILA: dos correas por delante y el cuerpo a la espalda
        {
            for (int s = -1; s <= 1; s += 2) Deco(chest, new Vector3(s * 0.1f, 0.16f, 0.127f), new Vector3(0.03f, 0.28f, 0.006f), look.backpack);
            Deco(chest, new Vector3(0f, 0.14f, -0.18f), new Vector3(0.26f, 0.3f, 0.11f), look.backpack);
            Deco(chest, new Vector3(0f, 0.08f, -0.24f), new Vector3(0.18f, 0.12f, 0.02f), Color.Lerp(look.backpack, Color.white, 0.12f));
        }
        Transform pelvis = Bone(BodyPart.Hips);
        if (look.hasTote && pelvis != null)   // BOLSA DE TELA colgada del hombro
        {
            Deco(chest, new Vector3(-0.185f, 0.08f, 0.1f), new Vector3(0.012f, 0.36f, 0.012f), look.tote);
            Deco(pelvis, new Vector3(-0.22f, -0.16f, 0.03f), new Vector3(0.03f, 0.3f, 0.26f), look.tote);
            Deco(pelvis, new Vector3(-0.237f, -0.16f, 0.03f), new Vector3(0.004f, 0.12f, 0.12f), look.totePrint);
        }
        if (look.longCoat)   // ABRIGO LARGO: faldones abiertos por delante, hasta la rodilla, y botones grandes
        {
            Color lap = Color.Lerp(clothes, Color.black, 0.06f), btn = Color.Lerp(clothes, Color.white, 0.12f);
            Deco(chest, new Vector3(-0.085f, 0.1f, 0.127f), new Vector3(0.028f, 0.028f, 0.008f), btn);
            Deco(chest, new Vector3(-0.085f, 0.02f, 0.127f), new Vector3(0.028f, 0.028f, 0.008f), btn);
            if (pelvis != null)
            {
                Deco(pelvis, new Vector3(0f, -0.2f, -0.132f), new Vector3(0.38f, 0.56f, 0.03f), lap);
                for (int s = -1; s <= 1; s += 2)
                {
                    Deco(pelvis, new Vector3(s * 0.135f, -0.2f, 0.132f), new Vector3(0.11f, 0.56f, 0.03f), lap);
                    Deco(pelvis, new Vector3(s * 0.19f, -0.2f, 0f), new Vector3(0.03f, 0.56f, 0.27f), lap);
                }
                Deco(pelvis, new Vector3(-0.135f, 0f, 0.15f), new Vector3(0.028f, 0.028f, 0.008f), btn);
            }
        }
        if (look.hasHangingGlasses)   // GAFAS colgadas del bolsillo de la camisa
        {
            float gx = look.jacket ? 0.02f : 0.08f, gz = look.jacket ? 0.128f : 0.126f;
            Color lens = Color.Lerp(Color.white, look.hangingGlasses, 0.15f);
            for (int s = -1; s <= 1; s += 2)
            {
                Deco(chest, new Vector3(gx + s * 0.022f, 0.07f, gz), new Vector3(0.034f, 0.028f, 0.004f), lens);
                Deco(chest, new Vector3(gx + s * 0.022f, 0.086f, gz + 0.002f), new Vector3(0.036f, 0.005f, 0.004f), look.hangingGlasses);
            }
            Deco(chest, new Vector3(gx, 0.078f, gz + 0.002f), new Vector3(0.012f, 0.004f, 0.004f), look.hangingGlasses);
            Deco(chest, new Vector3(gx - 0.035f, 0.11f, gz + 0.002f), new Vector3(0.004f, 0.05f, 0.004f), look.hangingGlasses, 0f, 20f);   // patilla enganchada
        }
        Transform hips = Bone(BodyPart.Hips);
        if (look.hasBelt && hips != null)
        {
            Deco(hips, new Vector3(0f, 0.075f, 0f), new Vector3(0.355f, 0.04f, 0.235f), look.belt);
            Deco(hips, new Vector3(0f, 0.075f, 0.12f), new Vector3(0.04f, 0.03f, 0.006f), new Color(0.75f, 0.75f, 0.78f));   // hebilla
        }
        if (look.hasBag)   // BOLSO CRUZADO: correa del hombro derecho a la cadera izquierda y el bolso de lado
        {
            Deco(chest, new Vector3(0f, 0.14f, 0f), new Vector3(0.42f, 0.025f, 0.26f), look.bag, 0f, -38f);
            if (hips != null)
            {
                Deco(hips, new Vector3(0.2f, -0.08f, 0.02f), new Vector3(0.07f, 0.22f, 0.26f), look.bag);
                Deco(hips, new Vector3(0.237f, -0.02f, 0.02f), new Vector3(0.006f, 0.08f, 0.24f), Color.Lerp(look.bag, Color.white, 0.12f));   // solapa
            }
        }
        if (look.hasSkirt && hips != null && look.tieredSkirt)   // FALDA DE VOLANTES, en tres pisos
            for (int i = 0; i < 3; i++) Deco(hips, new Vector3(0f, -0.05f - i * 0.085f, 0f), new Vector3(0.38f + i * 0.04f, 0.09f, 0.27f + i * 0.04f), Color.Lerp(look.skirt, Color.gray, i * 0.06f));
        else if (look.hasSkirt && hips != null && look.plainSkirt)   // FALDA LISA del vestido, hasta la rodilla
        {
            Deco(hips, new Vector3(0f, -0.18f, 0f), new Vector3(0.4f, 0.4f, 0.28f), look.skirt);
            Deco(hips, new Vector3(0f, -0.36f, 0f), new Vector3(0.44f, 0.06f, 0.32f), Color.Lerp(look.skirt, Color.gray, 0.08f));
        }
        else if (look.hasSkirt && hips != null)   // FALDA por encima de la rodilla, con vuelo y flores estampadas
        {
            Deco(hips, new Vector3(0f, -0.1f, 0f), new Vector3(0.4f, 0.24f, 0.28f), look.skirt);
            Deco(hips, new Vector3(0f, -0.24f, 0f), new Vector3(0.44f, 0.08f, 0.32f), Color.Lerp(look.skirt, Color.white, 0.06f));   // vuelo del bajo
            Color leaf = new Color(0.45f, 0.62f, 0.5f), light = Color.Lerp(look.skirtPrint, Color.white, 0.35f);
            for (int f = -1; f <= 1; f += 2)
                for (int i = 0; i < 9; i++)
                {
                    float x = -0.17f + i * 0.042f, y = -0.03f - ((i * 7) % 5) * 0.045f;
                    float z = f * (y < -0.2f ? 0.161f : 0.141f);
                    Deco(hips, new Vector3(x, y, z), new Vector3(0.024f, 0.024f, 0.004f), i % 3 == 0 ? light : look.skirtPrint);
                    if (i % 2 == 0) Deco(hips, new Vector3(x + 0.016f, y - 0.012f, z), new Vector3(0.012f, 0.008f, 0.004f), leaf);
                }
        }
        if (look.hasWatch)   // RELOJ en la muñeca izquierda
        {
            Transform fa = Bone(BodyPart.ForearmLeft);
            if (fa != null)
            {
                Deco(fa, new Vector3(0f, -0.235f, 0f), new Vector3(0.1f, 0.035f, 0.1f), look.watch);
                Deco(fa, new Vector3(0f, -0.235f, 0.052f), new Vector3(0.04f, 0.04f, 0.006f), look.watchFace);
            }
        }
    }

    // CUADROS, RAYAS o CANALÉ sobre una cara de un segmento (centro en y = cy, ancho w, alto hgt, en z): tiras finas de un
    // solo material compartido.
    static void WeaveOn(Transform bone, float cy, float w, float hgt, float z, Material m, CubeHair.Weave kind)
    {
        float step = kind == CubeHair.Weave.Stripes ? 0.02f : kind == CubeHair.Weave.Ribs ? 0.03f : 0.035f;
        float thick = kind == CubeHair.Weave.Stripes ? 0.004f : kind == CubeHair.Weave.Ribs ? 0.01f : 0.006f;
        float dz = kind == CubeHair.Weave.Ribs ? 0.008f : 0.003f;
        int nx = Mathf.Max(1, Mathf.FloorToInt(w / step));
        for (int i = 0; i < nx; i++)
            DecoM(bone, new Vector3(-w * 0.5f + (i + 0.5f) * w / nx, cy, z), new Vector3(thick, hgt, dz), m);
        if (kind != CubeHair.Weave.Checks) return;
        int ny = Mathf.Max(1, Mathf.FloorToInt(hgt / step));
        for (int j = 0; j < ny; j++)
            DecoM(bone, new Vector3(0f, cy - hgt * 0.5f + (j + 0.5f) * hgt / ny, z), new Vector3(w, thick, dz), m);
    }

    static void DecoM(Transform parent, Vector3 pos, Vector3 size, Material m)
    {
        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = "Detalle";
        Object.DestroyImmediate(g.GetComponent<Collider>());
        g.transform.SetParent(parent, false);
        g.transform.localPosition = pos; g.transform.localScale = size;
        g.GetComponent<Renderer>().sharedMaterial = m;
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

    // FORMA DE LA CARA: el cráneo (arriba, ancho) y la parte de abajo (mejillas → mandíbula → mentón) más estrecha, en vez
    // de un cubo: la cabeza humana es más estrecha en la mandíbula que en los pómulos. `look.jaw` da el ancho de la
    // mandíbula (cara redonda ≈ 0,9; ovalada ≈ 0,78; en corazón/afilada ≈ 0,68) y `look.cheeks` marca los pómulos.
    static void FaceShape(CubeJoint head, CubeHair.Look look, Material flesh)
    {
        Transform seg = head.segment; if (seg == null) return;
        Vector3 s = seg.localScale;   // 0,24 × 0,26 × 0,24
        float upper = s.y * 0.6f;
        seg.localScale = new Vector3(s.x, upper, s.z);
        seg.localPosition = new Vector3(0f, s.y - upper * 0.5f, 0f);
        float jaw = look.jaw > 0f ? look.jaw : 0.8f, lowH = s.y - upper;
        Part(head.transform, "Cara_media", new Vector3(0f, lowH * 0.72f, 0.004f), new Vector3(s.x * Mathf.Lerp(jaw, 1f, 0.6f), lowH * 0.56f, s.z * 0.97f), flesh);   // mejillas
        Part(head.transform, "Mandibula_forma", new Vector3(0f, lowH * 0.24f, 0.012f), new Vector3(s.x * jaw, lowH * 0.48f, s.z * 0.9f), flesh);              // mandíbula
        if (look.cheeks > 0f)
            for (int k = -1; k <= 1; k += 2)
                Part(head.transform, "Pomulo", new Vector3(k * s.x * 0.36f, upper * 0.0f + lowH * 0.95f, s.z * 0.42f), new Vector3(s.x * 0.18f, s.y * 0.07f, s.z * 0.14f * look.cheeks), flesh);
    }

    static void Part(Transform parent, string name, Vector3 local, Vector3 size, Material m)
    {
        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = name;
        Object.DestroyImmediate(g.GetComponent<Collider>());
        g.transform.SetParent(parent, false);
        g.transform.localPosition = local; g.transform.localScale = size;
        g.GetComponent<Renderer>().sharedMaterial = m;
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
