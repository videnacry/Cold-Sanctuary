using UnityEngine;

/// <summary>
/// Posturas para CUERPOS COPIADOS (clones de la Magnate, turistas virtuales): no tienen Anima ni CreatureRig, así que la
/// postura (las mismas de DailyPoses / ExamPoses) se aplica directamente a sus articulaciones (CubeJoint) por hueso.
/// </summary>
public static class ClonePose
{
    public static void Rest(Transform body)
    {
        foreach (CubeJoint j in body.GetComponentsInChildren<CubeJoint>(true)) { j.transform.localRotation = j.RestRotation; j.transform.localPosition = j.RestPosition; }
    }

    public static void Apply(Transform body, AsanaPoseSpell pose)
    {
        if (body == null || pose == null) return;
        Rest(body);
        foreach (CubeJoint j in body.GetComponentsInChildren<CubeJoint>(true))
            foreach (var p in pose.parts)
                if (p.part == j.part) j.transform.localRotation = Quaternion.Euler(p.euler);
        if (pose.rootHeight >= 0f)
            foreach (CubeJoint j in body.GetComponentsInChildren<CubeJoint>(true))
                if (j.part == BodyPart.Hips) { Vector3 r = j.RestPosition; j.transform.localPosition = new Vector3(r.x, pose.rootHeight, r.z); }
    }
}
