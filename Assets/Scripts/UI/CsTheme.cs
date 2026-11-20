using UnityEngine;

/// <summary>
/// EL TEMA DE LOS MENÚS (2026-11-19, Berón: «allí [en CodeShip] los menús son muy estéticos, ¿sería posible hacer algo similar aquí?»).
/// Una sola piel IMGUI con la paleta de CodeShip (src/ui/root-menu.css, action-pad.css): cristal azul petróleo oscuro, bordes y acentos
/// cian, verde para lo activo y naranja para lo que pide atención, esquinas redondeadas. Todos los menús del juego son IMGUI y crean sus
/// estilos a partir de <c>GUI.skin</c>, así que basta con llamar a <see cref="Use"/> al principio de su <c>OnGUI</c> para que hereden el
/// tema sin tocar su dibujo. Las texturas se generan en código (el repo solo versiona .cs): no hay assets que montar en Unity.
/// Las piezas a medida de CodeShip (ventanas, hojas, filas, mandos) están en <see cref="CsUi"/>, que dibuja con <see cref="Shape"/>.
/// </summary>
public static class CsTheme
{
    /// <summary>Apagar para volver a la piel por defecto de Unity (comparar, o si algún menú se ve mal).</summary>
    public static bool Enabled = true;

    // ── La paleta (los mismos valores que el CSS de CodeShip) ───────────────────────────────────────────────────────────────────
    public static readonly Color Glass = new Color32(0x0b, 0x2a, 0x36, 0xdc);        // #0b2a36cc: el fondo de botones y ventanas
    public static readonly Color GlassHover = new Color32(0x13, 0x40, 0x52, 0xe6);
    public static readonly Color Deep = new Color32(0x05, 0x14, 0x1c, 0xe6);         // #05141ce6: el fondo de las ventanas grandes y campos
    public static readonly Color Cyan = new Color32(0x7f, 0xdc, 0xff, 0xff);         // #7fdcff: el acento
    public static readonly Color CyanSoft = new Color32(0x7f, 0xdc, 0xff, 0x66);     // #7fdcff66: los bordes
    public static readonly Color Text = new Color32(0xd8, 0xf6, 0xff, 0xff);         // #d8f6ff: el texto
    public static readonly Color DeepText = new Color32(0x00, 0x10, 0x18, 0xff);     // #001018: el texto sobre cian
    public static readonly Color Green = new Color32(0x7d, 0xff, 0xb0, 0xff);        // #7dffb0: lo que está activo
    public static readonly Color Orange = new Color32(0xff, 0x9a, 0x2e, 0xff);       // #ff9a2e: la acción que pide atención
    public static readonly Color OrangeLight = new Color32(0xff, 0xd2, 0xa0, 0xff);  // #ffd2a0
    public static readonly Color Ink = new Color32(0x1d, 0x0d, 0x00, 0xff);          // #1d0d00: el texto sobre naranja

    static GUISkin _skin;
    static int _skinFor = -1;

    /// <summary>Pone el tema en este <c>OnGUI</c> (Unity devuelve la piel por defecto en cada llamada a <c>OnGUI</c>). En el móvil, la letra y
    /// los márgenes van a la medida de CodeShip (<see cref="CsUi.Dp"/>: 13 px CSS), también en los menús que aún no usan <see cref="CsUi"/>.</summary>
    public static void Use()
    {
        if (!Enabled) return;
        int key = Mathf.RoundToInt(CsUi.Dp * 100f);
        if (_skin == null || key != _skinFor)
        {
            if (_skin != null) Object.Destroy(_skin);
            _skin = Build(GUI.skin);
            _skinFor = key;
            if (CsUi.Mobile)
                foreach (GUIStyle st in new[] { _skin.label, _skin.button, _skin.box, _skin.toggle, _skin.textField, _skin.textArea, _skin.window })
                    st.fontSize = CsUi.Fs(13f);
        }
        GUI.skin = _skin;
    }

    static GUISkin Build(GUISkin from)
    {
        GUISkin s = Object.Instantiate(from);
        s.hideFlags = HideFlags.HideAndDontSave;
        const int r = 10;
        var border = new RectOffset(r, r, r, r);

        Texture2D panel = Rounded(Glass, CyanSoft, r), deep = Rounded(Deep, CyanSoft, r);
        Style(s.box, panel, null, null, Text, border);
        s.box.padding = new RectOffset(10, 10, 8, 8);
        Style(s.window, deep, null, null, Text, border);
        s.window.onNormal.background = deep;
        s.window.padding = new RectOffset(12, 12, 26, 10);

        Style(s.button, Rounded(Glass, CyanSoft, r), Rounded(GlassHover, Cyan, r), Rounded(Cyan, Cyan, r), Text, border);
        s.button.active.textColor = DeepText;
        s.button.onNormal.background = Rounded(Glass, Green, r); s.button.onNormal.textColor = Green;
        s.button.onHover.background = Rounded(GlassHover, Green, r); s.button.onHover.textColor = Green;
        s.button.onActive.background = s.button.active.background; s.button.onActive.textColor = DeepText;
        s.button.padding = new RectOffset(12, 12, 6, 6);

        Texture2D field = Rounded(Deep, CyanSoft, 6), fieldOn = Rounded(Deep, Cyan, 6);
        foreach (GUIStyle f in new[] { s.textField, s.textArea })
        {
            Style(f, field, field, fieldOn, Text, new RectOffset(6, 6, 6, 6));
            f.focused.background = fieldOn; f.focused.textColor = Text;
        }
        s.settings.cursorColor = Cyan;
        s.settings.selectionColor = new Color(Cyan.r, Cyan.g, Cyan.b, 0.35f);

        s.label.normal.textColor = Text;
        s.toggle.normal.textColor = s.toggle.onNormal.textColor = Text;
        s.toggle.hover.textColor = s.toggle.onHover.textColor = Cyan;

        Texture2D track = Rounded(new Color(0f, 0f, 0f, 0.25f), new Color(0f, 0f, 0f, 0f), 4), thumb = Rounded(CyanSoft, Cyan, 4);
        foreach (GUIStyle t in new[] { s.verticalScrollbar, s.horizontalScrollbar }) { t.normal.background = track; t.border = new RectOffset(4, 4, 4, 4); }
        foreach (GUIStyle t in new[] { s.verticalScrollbarThumb, s.horizontalScrollbarThumb }) { t.normal.background = thumb; t.border = new RectOffset(4, 4, 4, 4); }
        return s;
    }

    static void Style(GUIStyle st, Texture2D normal, Texture2D hover, Texture2D active, Color text, RectOffset border)
    {
        st.normal.background = normal; st.normal.textColor = text;
        st.hover.background = hover ?? normal; st.hover.textColor = text;
        st.active.background = active ?? hover ?? normal; st.active.textColor = text;
        st.focused.background = hover ?? normal; st.focused.textColor = text;
        st.border = border;
    }

    // ── Las texturas, generadas (con borde suavizado) ───────────────────────────────────────────────────────────────────────────

    /// <summary>Un rectángulo redondeado de radio <paramref name="radius"/> px, para estirar en 9 trozos (<c>border = radius</c>).</summary>
    public static Texture2D Rounded(Color fill, Color rim, int radius, int rimWidth = 2)
    {
        int n = radius * 2 + 2;
        return Shape(n, n, radius, (u, v) => fill, rim, rimWidth);
    }

    /// <summary>
    /// Una forma redondeada de <paramref name="w"/>×<paramref name="h"/> px con radio <paramref name="radius"/> (h/2 = píldora; w = h y radio
    /// w/2 = círculo), su relleno por punto (<paramref name="fill"/> recibe u, v en 0..1, v = 0 ARRIBA, como el CSS) y su aro.
    /// </summary>
    public static Texture2D Shape(int w, int h, float radius, System.Func<float, float, Color> fill, Color rim, float rimWidth)
    {
        return Make(w, h, (x, y) => RoundedDist(x, y, w, h, radius), (x, y) => fill((x + 0.5f) / w, 1f - (y + 0.5f) / h), rim, rimWidth);
    }

    /// <summary>
    /// El <c>radial-gradient(circle at cx cy, a, b stop)</c> del CSS: el color en (u, v) de una caja cuadrada. CodeShip lo usa en los botones
    /// del mando (at 40% 35%, hasta el 70 %) y en los joysticks. El radio de referencia es el de CSS: hasta la esquina más lejana.
    /// </summary>
    public static System.Func<float, float, Color> Radial(float cx, float cy, Color a, Color b, float stop, float aspect = 1f)
    {
        float fx = Mathf.Max(cx, 1f - cx) * aspect, fy = Mathf.Max(cy, 1f - cy);
        float far = Mathf.Sqrt(fx * fx + fy * fy) * stop;
        return (u, v) => Color.Lerp(a, b, Mathf.Clamp01(Mathf.Sqrt((u - cx) * (u - cx) * aspect * aspect + (v - cy) * (v - cy)) / far));
    }

    /// <summary>Distancia (px) desde un punto al borde de un rectángulo redondeado, positiva dentro.</summary>
    static float RoundedDist(float x, float y, int w, int h, float r)
    {
        float px = x + 0.5f, py = y + 0.5f;
        float dx = Mathf.Max(r - px, px - (w - r), 0f), dy = Mathf.Max(r - py, py - (h - r), 0f);
        if (dx > 0f && dy > 0f) return r - Mathf.Sqrt(dx * dx + dy * dy);
        return Mathf.Min(Mathf.Min(px, w - px), Mathf.Min(py, h - py));
    }

    static Texture2D Make(int w, int h, System.Func<int, int, float> inside, System.Func<int, int, Color> fill, Color rim, float rimWidth)
    {
        var t = new Texture2D(w, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear, hideFlags = HideFlags.HideAndDontSave };
        var px = new Color[w * h];
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float d = inside(x, y);                                   // > 0 dentro, en píxeles
                float a = Mathf.Clamp01(d + 0.5f);                         // borde exterior suavizado
                float rimK = rim.a > 0f && rimWidth > 0f ? Mathf.Clamp01(rimWidth - d + 0.5f) : 0f;   // 1 en el aro, 0 dentro
                Color f = fill(x, y);
                // El aro va ENCIMA del relleno, como el border del CSS sobre su fondo (composición «source-over»).
                float ra = rim.a * rimK, oa = ra + f.a * (1f - ra);
                Color c = oa <= 0f ? new Color(0f, 0f, 0f, 0f)
                    : new Color((rim.r * ra + f.r * f.a * (1f - ra)) / oa, (rim.g * ra + f.g * f.a * (1f - ra)) / oa, (rim.b * ra + f.b * f.a * (1f - ra)) / oa, oa);
                c.a *= a;
                px[y * w + x] = c;
            }
        t.SetPixels(px);
        t.Apply();
        return t;
    }
}
