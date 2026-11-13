using UnityEngine;

// (En su propio archivo: Unity solo serializa en la escena un MonoBehaviour cuyo archivo se llama como la clase.)
/// <summary>
/// (2026-11-11, petición #152) Cada 15 s pasa a mate los materiales de los cuerpos (<see cref="CheapMaterials.CheapenBodies"/>): los que se crean en
/// juego al vestir o colorear a la gente y a los animales («Lit (Instance)», «Gris», «DarkGreen»…) no existían al cargar la escena.
/// </summary>
public class CheapMaterialsSweeper : MonoBehaviour
{
    float _next;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        if (FindAnyObjectByType<CheapMaterialsSweeper>() != null) return;
        var go = new GameObject("CheapMaterialsSweeper") { hideFlags = HideFlags.HideInHierarchy };
        DontDestroyOnLoad(go);
        go.AddComponent<CheapMaterialsSweeper>();
    }

    void Update()
    {
        if (Time.unscaledTime < _next) return;
        _next = Time.unscaledTime + 15f;
        // (2026-11-11) Solo los cuerpos NUEVOS (nacimientos, clones de la Magnate, crías): sus cubos se hacen con primitivas (el «Lit» por defecto) y
        // algunos se colorean en juego. Las escenas enteras, solo al cargar (CheapMaterials.OnLoad): recorrer todos los renderers cada minuto era un
        // tirón en este PC.
        CheapMaterials.CheapenBodies(true);
    }
}
