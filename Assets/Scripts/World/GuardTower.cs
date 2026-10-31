using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CASA DEL ÁRBOL DE GUARDIA (docs/spells-suprahechizo-and-thoughts.md §5b): el puesto de los vigías es una cabaña en lo
/// alto de uno de los árboles de hielo del santuario, pero ×2,5 de grande (40 m) y que NO caza. Se sube en ALFOMBRA
/// VOLADORA (una al pie del árbol y otra arriba; sin escalera ni barandilla). Arriba, una plataforma rodea el tronco y la
/// cabaña guarda el <see cref="GuardBoard"/> con las misiones de vigía. Todo de cubos.
/// </summary>
public static class GuardTower
{
    public const float BaseHeight = 83f;   // caso ÚNICO: el árbol de la guardia medía 83 m (los demás, 20–40 m)
    /// <summary>Su altura: la EXCEPCIÓN a «el primer piso de los edificios-árbol, por encima del árbol más alto» (Berón 2026-10-31: es
    /// un árbol modificado): crece tan alto como el edificio más alto (lo fija el constructor; si no, 83 m). Crece en alto, no en
    /// ancho (sus ramas, como las del de 83 m) y sus ramas esquivan los edificios (<see cref="Build"/>, blocked).</summary>
    public static float TreeHeight = BaseHeight;
    public const float DeckHeight = 11f;   // bajo el primer piso de ramas (≈15 m): la cabaña no queda atravesada

    public static Transform Build(Transform parent, Vector3 groundPos, System.Func<Vector3, bool> blocked = null)
    {
        GameObject treeGO = CubeTree.Build(parent, "CasaArbolGuardia", groundPos, 4242, TreeHeight, carnivorous: false, blocked: blocked, reachHeight: BaseHeight);
        Transform t = treeGO.transform;
        Color wood = new Color(0.62f, 0.52f, 0.42f), woodDark = new Color(0.48f, 0.4f, 0.33f), roof = new Color(0.55f, 0.66f, 0.75f),
              glass = new Color(0.75f, 0.88f, 0.95f), lamp = new Color(1f, 0.85f, 0.45f);
        float y = DeckHeight, half = 4.6f;

        // Plataforma alrededor del tronco, con vigas de apoyo hacia el tronco.
        Block(t, new Vector3(0f, y, 0f), new Vector3(half * 2f, 0.3f, half * 2f), woodDark, true, "Plataforma");
        for (int i = 0; i < 4; i++)
        {
            Quaternion q = Quaternion.Euler(0f, i * 90f + 45f, 0f);
            Transform beam = Block(t, new Vector3(0f, y - 1.6f, 0f) + q * Vector3.forward * half * 0.55f, new Vector3(0.35f, 0.35f, half * 1.3f), wood, false, "Viga");
            beam.localRotation = q * Quaternion.Euler(-35f, 0f, 0f);
        }
        // Sin barandilla ni escalera (Berón 2026-10-27, docs/alfombras-plan.md): se sube y se baja en alfombra voladora.

        // Cabaña al norte del tronco: paredes de tablones, puerta, ventanas y tejado escalonado.
        Vector3 c = new Vector3(0f, y, 2.6f);
        float cw = 6f, cd = 3.2f, ch = 2.6f;
        Block(t, c + new Vector3(0f, ch * 0.5f, cd * 0.5f), new Vector3(cw, ch, 0.15f), wood, true, "Pared_Norte");
        Block(t, c + new Vector3(-cw * 0.5f, ch * 0.5f, 0f), new Vector3(0.15f, ch, cd), wood, true, "Pared_Oeste");
        Block(t, c + new Vector3(cw * 0.5f, ch * 0.5f, 0f), new Vector3(0.15f, ch, cd), wood, true, "Pared_Este");
        Block(t, c + new Vector3(-2.1f, ch * 0.5f, -cd * 0.5f), new Vector3(1.8f, ch, 0.15f), wood, true, "Pared_Sur");
        Block(t, c + new Vector3(2.1f, ch * 0.5f, -cd * 0.5f), new Vector3(1.8f, ch, 0.15f), wood, true, "Pared_Sur");
        Block(t, c + new Vector3(0f, ch - 0.3f, -cd * 0.5f), new Vector3(2.4f, 0.6f, 0.15f), wood, true, "Dintel");
        for (int s = -1; s <= 1; s += 2)
            Block(t, c + new Vector3(s * cw * 0.5f, ch * 0.55f, 0f), new Vector3(0.17f, 0.7f, 1f), glass, false, "Ventana");
        for (int k = 0; k < 4; k++)
            Block(t, c + new Vector3(0f, ch + 0.15f + k * 0.3f, 0f), new Vector3(cw + 0.8f - k * 1.5f, 0.3f, cd + 0.8f - k * 0.7f), k % 2 == 0 ? roof : woodDark, false, "Tejado");
        Block(t, c + new Vector3(cw * 0.5f - 0.4f, ch - 0.4f, -cd * 0.5f - 0.2f), new Vector3(0.25f, 0.35f, 0.25f), lamp, false, "Farol");
        Block(t, c + new Vector3(0f, ch + 2.2f, 0f), new Vector3(0.06f, 1.6f, 0.06f), woodDark, false, "Mastil");
        Block(t, c + new Vector3(0.4f, ch + 2.7f, 0f), new Vector3(0.7f, 0.45f, 0.03f), new Color(0.75f, 0.65f, 0.88f), false, "Bandera");

        // Tablero de misiones dentro de la cabaña, contra la pared norte.
        Transform board = Block(t, c + new Vector3(0f, 1.3f, cd * 0.5f - 0.15f), new Vector3(1.6f, 1.1f, 0.08f), new Color(0.86f, 0.78f, 0.62f), true, "Tablero de guardia");
        board.gameObject.AddComponent<GuardBoard>();
        WorldLabel wl = board.gameObject.AddComponent<WorldLabel>(); wl.text = "Tablero de guardia"; wl.height = 0.9f;

        // ALFOMBRAS: una en el hielo, al pie del árbol (lado sur), y otra en la plataforma, junto al tronco.
        const string casa = "Casa del guarda";
        CarpetPad.BuildZone(t, t.TransformPoint(new Vector3(0f, 0.08f, -half - 2.5f)), t.rotation, CarpetPad.Kind.Edificio, casa, 0, casa + "#0", "Suelo (hielo)", 1, 3.2f, 3.6f);
        CarpetPad.BuildZone(t, t.TransformPoint(new Vector3(-2.9f, y + 0.2f, -2.6f)), t.rotation, CarpetPad.Kind.Edificio, casa, 1, casa + "#1", "Plataforma de guardia", 1, 3.2f, 3.6f);

        SoftGround.Plant(t, groundPos + Vector3.down * 0.1f, new Vector2(14f, 14f), 11);   // colchón de musgo bajo la casa del árbol
        WorldLabel tl = t.gameObject.AddComponent<WorldLabel>(); tl.text = "Casa del árbol de la guardia"; tl.height = y + 5f;
        return t;
    }

    static Transform Block(Transform parent, Vector3 local, Vector3 size, Color c, bool solid, string name)
    {
        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = name;
        if (!solid) { Collider col = g.GetComponent<Collider>(); if (Application.isPlaying) Object.Destroy(col); else Object.DestroyImmediate(col); }
        g.transform.SetParent(parent, false);
        g.transform.localPosition = local;
        g.transform.localScale = size;
        g.GetComponent<Renderer>().sharedMaterial = KitchenProps.Mat(c);
        return g.transform;
    }
}

