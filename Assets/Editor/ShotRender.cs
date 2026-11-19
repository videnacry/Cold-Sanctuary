using UnityEditor;
using UnityEngine;

/// <summary>
/// (2026-11-19, piloto6) UNA FOTO SIN CIAN. En el editor, Unity compila los sombreadores en segundo plano y, mientras tanto, dibuja
/// cada variante que aún no tiene con un sombreador provisional CIAN liso. Un <c>cam.Render()</c> de una sola vez a una
/// RenderTexture la pilla a medias: la foto «fabrica» de Hawthorne con el corte forzado (otras sombras, otra mezcla de luces → variantes
/// nuevas) salía cian del suelo, las lámparas y las vigas, mientras los rótulos y el cielo (variantes ya hechas) salían bien.
/// Aquí la foto se hace con la compilación EN EL ACTO (<see cref="ShaderUtil.allowAsyncCompilation"/> = false) y se devuelve como estaba.
/// Úsese en toda foto de prueba: <c>ShotRender.Render(cam)</c> en lugar de <c>cam.Render()</c>.
/// </summary>
public static class ShotRender
{
    public static void Render(Camera cam)
    {
        if (cam == null) return;
        bool was = ShaderUtil.allowAsyncCompilation;
        ShaderUtil.allowAsyncCompilation = false;
        try { cam.Render(); }
        finally { ShaderUtil.allowAsyncCompilation = was; }
    }
}
