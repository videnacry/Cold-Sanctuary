using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// EL BLOQUEO DE HUMANOS (docs/microcosmos-civilizaciones.md §3; Berón 2026-11-01: «en el microcosmos todos los personajes
/// son insectos, debería haber un bloqueo para evitar meter humanos»). ÚNICO guardián: en un microcosmos solo hay cuerpos
/// de insecto. Quien entra con cuerpo humano (el de cubos, <see cref="CubeHumanoid"/>, o un .fbx humanoide) se CONVIERTE en
/// su insecto (<see cref="InsectCast"/>, a su estatura, con cara humana y pelo: <see cref="CubeInsect.DressFolk"/>); si no se
/// puede, se le RECHAZA (se desactiva y se avisa). Al salir, <see cref="Release"/> le devuelve su cuerpo.
/// Lo llaman: <c>MobWorldLoader</c> (el jugador al entrar y al salir), <see cref="MicroEra"/> (la raíz de todo microcosmos: al
/// cargar y cada 2 s, para los personajes y lo que aparezca después) y los builders (antes de guardar la escena).
/// Prueba: <c>InsectOnlyTest</c>.
/// </summary>
public static class InsectOnly
{
    public enum Result { AlreadyInsect, Converted, Refused }

    /// <summary>¿Es un mundo de insectos (un microcosmos)? Su raíz lleva <see cref="MicroEra"/> (la plantilla la pone siempre)
    /// o la escena se llama Microcosmos_* / MobWorld_*.</summary>
    public static bool IsInsectWorld(Scene s)
    {
        if (!s.IsValid()) return false;
        if (s.name.StartsWith("Microcosmos_") || s.name.StartsWith("MobWorld_")) return true;
        if (!s.isLoaded) return false;
        foreach (GameObject g in s.GetRootGameObjects()) if (g.GetComponentInChildren<MicroEra>(true) != null) return true;
        return false;
    }

    /// <summary>¿Se ve con cuerpo humano? (su cuerpo de cubos humano con algo visible, o un Animator humanoide visible sin cuerpo de insecto)</summary>
    public static bool IsHuman(GameObject go)
    {
        if (go == null) return false;
        Transform body = go.transform.Find(CubeHumanoid.BodyName);
        if (body != null && body.gameObject.activeInHierarchy)
            foreach (Renderer r in body.GetComponentsInChildren<Renderer>()) if (r.enabled) return true;
        Animator an = go.GetComponent<Animator>();
        if (an != null && an.isHuman && go.transform.Find("Insecto") == null)
            foreach (Renderer r in go.GetComponentsInChildren<Renderer>()) if (r.enabled && !(r is ParticleSystemRenderer)) return true;
        return false;
    }

    /// <summary>Hace pasar a <paramref name="go"/> por el bloqueo: si es humano, lo convierte en su insecto (o lo rechaza).</summary>
    public static Result Admit(GameObject go)
    {
        if (go == null) return Result.Refused;
        if (!IsHuman(go)) return Result.AlreadyInsect;
        try
        {
            InsectForm form = go.GetComponent<InsectForm>() ?? go.AddComponent<InsectForm>();
            float h = form.HideHuman();
            Transform body = go.transform.Find(CubeHumanoid.BodyName);
            string sp = InsectCast.SpeciesFor(go.CompareTag("Player") ? "player" : go.name);
            CubeInsect ci = CubeInsect.DressFolk(go, sp, h, go.CompareTag("Player") ? "Kushal" : go.name);
            Transform root = go.transform.Find("Insecto");
            if (root != null && body != null) root.localPosition += new Vector3(0f, body.localPosition.y, 0f);   // los pies donde los tenía el cuerpo humano
            form.species = sp;
            if (ci == null || root == null) throw new System.Exception("sin cuerpo de insecto");
            Debug.Log($"[Microcosmos] «{go.name}» entra como {sp} ({InsectCast.Why(go.CompareTag("Player") ? "player" : go.name)}).");
            return Result.Converted;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[Microcosmos] «{go.name}» no puede entrar con cuerpo humano y no se pudo convertir ({e.Message}): queda fuera.");
            go.SetActive(false);
            return Result.Refused;
        }
    }

    /// <summary>Le devuelve su cuerpo humano (al salir del microcosmos).</summary>
    public static void Release(GameObject go)
    {
        if (go == null) return;
        InsectForm form = go.GetComponent<InsectForm>();
        if (form != null) form.Restore();
    }

    /// <summary>Pasa el bloqueo a todos los de la escena <paramref name="s"/> (en juego). Devuelve cuántos convirtió.</summary>
    public static int EnforceScene(Scene s)
    {
        int n = 0;
        foreach (CreatureRig rig in Object.FindObjectsByType<CreatureRig>(FindObjectsSortMode.None))
            if (rig != null && rig.gameObject.scene == s && Admit(rig.gameObject) == Result.Converted) n++;
        foreach (Animator an in Object.FindObjectsByType<Animator>(FindObjectsSortMode.None))
            if (an != null && an.gameObject.scene == s && an.isHuman && Admit(an.gameObject) == Result.Converted) n++;
        return n;
    }

    /// <summary>Lo mismo bajo una raíz (los builders, antes de guardar la escena: ninguna se guarda con humanos).</summary>
    public static int EnforceRoot(Transform root)
    {
        int n = 0;
        var owners = new List<GameObject>();
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true)) if (t.name == CubeHumanoid.BodyName && t.parent != null) owners.Add(t.parent.gameObject);
        foreach (GameObject g in owners) if (Admit(g) == Result.Converted) n++;
        if (n > 0) Debug.Log($"[Microcosmos] {root.name}: {n} cuerpos humanos convertidos en insectos (bloqueo de humanos).");
        return n;
    }
}

/// <summary>Recuerda qué se ocultó del cuerpo humano para devolvérselo (<see cref="InsectOnly.Release"/>).</summary>
public class InsectForm : MonoBehaviour
{
    public string species;
    [SerializeField] List<Renderer> _hidden = new List<Renderer>();

    /// <summary>Oculta todo lo visible (menos los rótulos y lo que ya sea insecto) y devuelve su estatura.</summary>
    public float HideHuman()
    {
        bool any = false; Bounds b = default;
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            if (!r.enabled || r is ParticleSystemRenderer || r.GetComponent<TextMesh>() != null) continue;
            if (r.transform.IsChildOf(transform) && r.GetComponentInParent<CubeInsect>() != null && r.transform.parent != null && IsUnderInsect(r.transform)) continue;
            if (!any) { b = r.bounds; any = true; } else b.Encapsulate(r.bounds);
            r.enabled = false; _hidden.Add(r);
        }
        float h = any ? b.size.y : 1.75f;
        return Mathf.Clamp(h, 1.4f, 2.1f);
    }

    bool IsUnderInsect(Transform t) { for (Transform p = t; p != null && p != transform; p = p.parent) if (p.name == "Insecto") return true; return false; }

    public void Restore()
    {
        Transform ins = transform.Find("Insecto");
        if (ins != null) { if (Application.isPlaying) Destroy(ins.gameObject); else DestroyImmediate(ins.gameObject); }
        CubeInsect ci = GetComponent<CubeInsect>();
        if (ci != null) { if (Application.isPlaying) Destroy(ci); else DestroyImmediate(ci); }
        foreach (Renderer r in _hidden) if (r != null) r.enabled = true;
        _hidden.Clear();
        if (Application.isPlaying) Destroy(this); else DestroyImmediate(this);
    }
}
