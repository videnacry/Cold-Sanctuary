using UnityEngine;

/// <summary>
/// IK ANALÍTICA de 2 huesos (ley de cosenos) para "alcance": coloca el extremo (`end` = mano/pie) lo más cerca posible
/// de un objetivo rotando el hueso raíz (`root` = hombro/cadera) y el medio (`mid` = codo/rodilla). Así las posturas de
/// yoga pueden EXIGIR que la mano toque el suelo junto al pie o que las palmas se junten, en vez de depender de que los
/// ángulos "caigan" justos. `pole` orienta hacia dónde dobla el codo/rodilla; `weight` mezcla (0 = nada, 1 = pleno) para
/// entrar suave con la respiración. Se aplica DESPUÉS de la cinemática directa (la postura), que deja un buen punto de
/// partida. Formulación analítica estándar (dos huesos, un objetivo).
/// </summary>
public static class TwoBoneIK
{
    public static void Solve(Transform root, Transform mid, Transform end, Vector3 targetWorld, Vector3 poleWorld, float weight)
    {
        if (root == null || mid == null || end == null || weight <= 0f) return;

        Vector3 a = root.position, b = mid.position, c = end.position;
        float lab = Vector3.Distance(a, b);
        float lcb = Vector3.Distance(b, c);
        if (lab < 1e-5f || lcb < 1e-5f) return;

        Vector3 ab = b - a, bc = c - b, ac = c - a, at = targetWorld - a;
        float lat = Mathf.Clamp(at.magnitude, 0.001f, lab + lcb - 0.001f);
        if (ac.sqrMagnitude < 1e-8f || at.sqrMagnitude < 1e-8f) return;

        // Ángulos actuales y deseados en el hombro (a) y el codo (b).
        float ac_ab_0 = Mathf.Acos(Mathf.Clamp(Vector3.Dot(ac.normalized, ab.normalized), -1f, 1f));
        float ba_bc_0 = Mathf.Acos(Mathf.Clamp(Vector3.Dot((-ab).normalized, bc.normalized), -1f, 1f));
        float ac_at_0 = Mathf.Acos(Mathf.Clamp(Vector3.Dot(ac.normalized, at.normalized), -1f, 1f));
        float ac_ab_1 = Mathf.Acos(Mathf.Clamp((lcb * lcb - lab * lab - lat * lat) / (-2f * lab * lat), -1f, 1f));
        float ba_bc_1 = Mathf.Acos(Mathf.Clamp((lat * lat - lab * lab - lcb * lcb) / (-2f * lab * lcb), -1f, 1f));

        // Eje de flexión del codo (a partir del pole) y eje para apuntar hacia el objetivo.
        Vector3 axisBend = Vector3.Cross(ac, poleWorld - a);
        if (axisBend.sqrMagnitude < 1e-8f) axisBend = Vector3.Cross(ac, ab);
        if (axisBend.sqrMagnitude < 1e-8f) return;
        axisBend.Normalize();
        Vector3 axisAim = Vector3.Cross(ac, at);
        if (axisAim.sqrMagnitude < 1e-8f) axisAim = axisBend;
        axisAim.Normalize();

        // Objetivo de rotación del hombro: abrir el ángulo del codo + apuntar el brazo hacia el objetivo.
        Quaternion r0 = Quaternion.AngleAxis((ac_ab_1 - ac_ab_0) * Mathf.Rad2Deg, axisBend);
        Quaternion aim = Quaternion.AngleAxis(ac_at_0 * Mathf.Rad2Deg, axisAim);
        Quaternion rootWorldTarget = aim * r0 * root.rotation;
        Quaternion rootLocalTarget = Quaternion.Inverse(root.parent != null ? root.parent.rotation : Quaternion.identity) * rootWorldTarget;
        root.localRotation = Quaternion.Slerp(root.localRotation, rootLocalTarget, weight);

        // Objetivo de rotación del codo (tras mover el hombro).
        Quaternion e0 = Quaternion.AngleAxis((ba_bc_1 - ba_bc_0) * Mathf.Rad2Deg, axisBend);
        Quaternion midWorldTarget = e0 * mid.rotation;
        Quaternion midLocalTarget = Quaternion.Inverse(mid.parent != null ? mid.parent.rotation : Quaternion.identity) * midWorldTarget;
        mid.localRotation = Quaternion.Slerp(mid.localRotation, midLocalTarget, weight);
    }
}
