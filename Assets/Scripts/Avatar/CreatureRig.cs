using System.Collections.Generic;
using UnityEngine;

/// <summary>Partes lógicas de un ser (humano/insecto/quimera). No todo cuerpo tiene todas.</summary>
public enum BodyPart
{
    Head, Neck, Chest, Hips,
    ShoulderLeft, ShoulderRight,
    HandLeft, HandRight,
    FootLeft, FootRight,
    // insecto / quimera (rig Generic):
    AntennaLeft, AntennaRight, Tail,
    LimbA, LimbB, LimbC, LimbD, LimbE, LimbF
}

/// <summary>
/// Mapa CENTRAL de un ser (docs/stats-as-truth.md §Composición): **parte lógica → hueso (`Transform`)**, para
/// que cualquier sistema (yoga/emoción/posesión/combate/emotes) mueva "el cuello" o "la antena L" **sin saber
/// qué modelo es**. Para **humanoides** se auto-rellena desde el Avatar estándar de Unity
/// (`Animator.GetBoneTransform(HumanBodyBones…)`); para **insectos/quimeras** (rig Generic) se asigna a mano.
/// Es solo el **esqueleto móvil**; la **composición** (peinado/ropa/adornos con stats) es un sistema aparte.
/// Un mismo motor de yoga/emoción sirve para cualquier cuerpo: solo cambia este mapeo.
/// </summary>
public class CreatureRig : MonoBehaviour
{
    [System.Serializable]
    public class PartBinding { public BodyPart part; public Transform bone; }

    [Tooltip("Si hay Animator humanoide, se auto-rellenan las partes estándar desde el Avatar de Unity.")]
    public Animator humanoidAnimator;
    [Tooltip("Bindings manuales (insectos/quimeras, o para sobreescribir el humanoide).")]
    public List<PartBinding> bindings = new List<PartBinding>();

    readonly Dictionary<BodyPart, Transform> _map = new Dictionary<BodyPart, Transform>();

    void Awake() { Rebuild(); }

    /// <summary>(Re)construye el mapa: primero el humanoide estándar, luego los bindings manuales (prioridad).</summary>
    public void Rebuild()
    {
        _map.Clear();
        if (humanoidAnimator != null && humanoidAnimator.isHuman)
        {
            TryHuman(BodyPart.Head, HumanBodyBones.Head);
            TryHuman(BodyPart.Neck, HumanBodyBones.Neck);
            TryHuman(BodyPart.Chest, HumanBodyBones.Chest);
            TryHuman(BodyPart.Hips, HumanBodyBones.Hips);
            TryHuman(BodyPart.ShoulderLeft, HumanBodyBones.LeftShoulder);
            TryHuman(BodyPart.ShoulderRight, HumanBodyBones.RightShoulder);
            TryHuman(BodyPart.HandLeft, HumanBodyBones.LeftHand);
            TryHuman(BodyPart.HandRight, HumanBodyBones.RightHand);
            TryHuman(BodyPart.FootLeft, HumanBodyBones.LeftFoot);
            TryHuman(BodyPart.FootRight, HumanBodyBones.RightFoot);
        }
        foreach (PartBinding b in bindings)   // manuales: prioridad + añaden partes no-humanas (antenas, patas…)
            if (b != null && b.bone != null) _map[b.part] = b.bone;
    }

    void TryHuman(BodyPart part, HumanBodyBones bone)
    {
        Transform t = humanoidAnimator.GetBoneTransform(bone);
        if (t != null) _map[part] = t;
    }

    /// <summary>El hueso de esa parte, o null si este cuerpo no la tiene (una hormiga no tiene "Neck").</summary>
    public Transform Get(BodyPart part) => _map.TryGetValue(part, out Transform t) ? t : null;

    public bool Has(BodyPart part) => _map.ContainsKey(part);
}
