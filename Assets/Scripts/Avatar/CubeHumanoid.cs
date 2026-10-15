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
        Transform old = who.transform.Find(BodyName);
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
        float feetY = any ? b.min.y - who.transform.position.y : 0f;

        GameObject body = new GameObject(BodyName);
        body.transform.SetParent(who.transform, false);
        float s = h / NativeHeight / Mathf.Max(0.01f, who.transform.lossyScale.y);
        body.transform.localPosition = new Vector3(0f, feetY / Mathf.Max(0.01f, who.transform.lossyScale.y), 0f);
        body.transform.localScale = Vector3.one * s;

        CreatureRig rig = who.GetComponent<CreatureRig>() ?? who.AddComponent<CreatureRig>();
        rig.humanoidAnimator = null;      // manda el cuerpo de cubos, no los huesos del .fbx
        rig.bindings.Clear();
        Build(body.transform, rig, clothes, skin);
        rig.Rebuild();
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
        Material cloth = Mat(clothes), flesh = Mat(skin), dark = Mat(clothes * 0.6f + new Color(0, 0, 0, 1));

        // Pelvis (cadera) a la altura de la cadera; todo cuelga de aquí.
        CubeJoint hips  = Joint(root, BodyPart.Hips,  new Vector3(0f, 0.95f, 0f), new Vector3(0.34f, 0.2f, 0.22f), Vector3.zero, dark, rig);
        CubeJoint waist = Joint(hips.transform, BodyPart.Chest, new Vector3(0f, 0.1f, 0f), new Vector3(0.4f, 0.5f, 0.24f), new Vector3(0f, 0.25f, 0f), cloth, rig);
        CubeJoint neck  = Joint(waist.transform, BodyPart.Neck, new Vector3(0f, 0.52f, 0f), new Vector3(0.1f, 0.08f, 0.1f), new Vector3(0f, 0.04f, 0f), flesh, rig);
        Joint(neck.transform, BodyPart.Head, new Vector3(0f, 0.08f, 0f), new Vector3(0.24f, 0.26f, 0.24f), new Vector3(0f, 0.13f, 0f), flesh, rig);

        for (int side = -1; side <= 1; side += 2)
        {
            bool left = side < 0;
            // Brazo: hombro → codo → muñeca.
            CubeJoint sh = Joint(waist.transform, left ? BodyPart.ShoulderLeft : BodyPart.ShoulderRight,
                new Vector3(0.26f * side, 0.46f, 0f), new Vector3(0.1f, 0.3f, 0.1f), new Vector3(0f, -0.15f, 0f), cloth, rig);
            CubeJoint el = Joint(sh.transform, left ? BodyPart.ForearmLeft : BodyPart.ForearmRight,
                new Vector3(0f, -0.3f, 0f), new Vector3(0.09f, 0.27f, 0.09f), new Vector3(0f, -0.135f, 0f), flesh, rig);
            Joint(el.transform, left ? BodyPart.HandLeft : BodyPart.HandRight,
                new Vector3(0f, -0.27f, 0f), new Vector3(0.08f, 0.1f, 0.05f), new Vector3(0f, -0.05f, 0f), flesh, rig);
            // Pierna: cadera → rodilla → tobillo.
            CubeJoint th = Joint(hips.transform, left ? BodyPart.ThighLeft : BodyPart.ThighRight,
                new Vector3(0.1f * side, -0.05f, 0f), new Vector3(0.14f, 0.42f, 0.14f), new Vector3(0f, -0.21f, 0f), dark, rig);
            CubeJoint kn = Joint(th.transform, left ? BodyPart.ShinLeft : BodyPart.ShinRight,
                new Vector3(0f, -0.42f, 0f), new Vector3(0.12f, 0.4f, 0.12f), new Vector3(0f, -0.2f, 0f), dark, rig);
            Joint(kn.transform, left ? BodyPart.FootLeft : BodyPart.FootRight,
                new Vector3(0f, -0.4f, 0f), new Vector3(0.12f, 0.06f, 0.24f), new Vector3(0f, -0.03f, 0.06f), dark, rig);
        }

        foreach (CubeJoint j in root.GetComponentsInChildren<CubeJoint>()) j.Capture();
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
