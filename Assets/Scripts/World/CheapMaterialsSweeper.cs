using UnityEngine;

// (En su propio archivo: Unity solo serializa en la escena un MonoBehaviour cuyo archivo se llama como la clase.)
/// <summary>
/// (2026-11-11, petición #152) Cada 15 s pasa a mate los materiales de los cuerpos (<see cref="CheapMaterials.CheapenBodies"/>): los que se crean en
/// juego al vestir o colorear a la gente y a los animales («Lit (Instance)», «Gris», «DarkGreen»…) no existían al cargar la escena.
/// </summary>
public class CheapMaterialsSweeper : MonoBehaviour
{
    float _next, _nextScene = 60f;

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
        CheapMaterials.CheapenBodies();
        // (ronda 72) Y cada minuto, las escenas enteras: quedaban 15 «Lit» y «Lit (Instance)» que no eran de ningún cuerpo; dice de quién eran.
        if (Time.unscaledTime >= _nextScene)
        {
            _nextScene = Time.unscaledTime + 60f;
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++) CheapMaterials.CheapenScene(UnityEngine.SceneManagement.SceneManager.GetSceneAt(i), true);
        }
    }
}
