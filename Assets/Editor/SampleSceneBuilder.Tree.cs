using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EL ÁRBOL DE LOS CAMPOS v2 (Berón 2026-10-30; docs/edificio-arbol.md, medidas en <see cref="TreeSpec"/> y el modelo genérico
/// <see cref="HelixTower"/>, construido con SampleSceneBuilder.Helix.cs, el mismo que las torres): los campos de cultivo en un
/// edificio en HÉLICE, en el lóbulo Sur:Milpa.
/// - TRONCO CUADRADO de agua (22 m, pared de acrílico de 1 m, transparente como el tanque de virtualización: se ve nadar a la
///   gente), de 0 a 92 m, con 24 nervios de acero (CFST) y zunchos de canto cada 6 m. Una PUERTA por nivel con su campo
///   magnético (el agua no sale; el hueco es una campana de aire); abajo, la puerta del suelo; arriba, la del techo.
/// - HÉLICE de 2 niveles desde 64 m (por encima del árbol más alto del santuario): en cada nivel, 2 PISOS cuadrados de 32 m
///   pegados a dos caras opuestas (en molinete: no se tapan) y, en las otras dos caras, 2 RAMAS (tubos cuadrados de agua) a
///   un CAMPO DE PUNTA cuadrado de 16 m en la esquina libre, con una COLUMNA de agua (puerta al campo y puerta al ANILLO de
///   encima, con nidos, flores y frutales enanos). El nivel siguiente, girado un cuarto de vuelta.
/// - TECHO: plataforma plana de 22 m con las alfombras (de edificio y del santuario) y la puerta. Abajo, en el hielo, las
///   del suelo. Entre pisos, sin alfombras: por el tronco, nadando. El suelo bajo el árbol queda libre (sin pavimento). Gira.
/// </summary>
public static partial class SampleSceneBuilder
{
    static Transform _treeRoot;
    static bool _treePosed;
    static readonly List<TreeFloor> _treeFloors = new List<TreeFloor>();

    /// <summary>Levanta el árbol con el área del Huerto (<paramref name="garden"/>) en su piso 5. Devuelve la raíz.</summary>
    static Transform BuildTree(Transform parent, SanctuaryArea garden)
    {
        HelixTower h = TreeSpec.Helix;
        _treeFloors.Clear(); _treePosed = false;
        Helix hx = BeginHelix(parent, TreeSpec.Building, TreeSpec.RootName, TreeSpec.Site, 0f, h, rotates: true);
        hx.groundSide = TreeSpec.GroundSide; hx.padAngle = 45f; hx.padAngle2 = 69f;
        hx.floorTitle = k => GardenPlot.Title(TreeSpec.Guilds[k - 1]);
        hx.santuarioLabel = $"{TreeSpec.Building}: bosque comestible, bambusal, arrozal, milpa, huerto, chacra, rotación y nidos";
        hx.tipContent = (k, tip) => TreeTip(k, tip);
        _treeRoot = hx.root;
        for (int k = 1; k <= TreeSpec.Platforms; k++)
            if (!TreeSpec.Platform(k).tip) TrunkFloor(hx, k, garden);
        FinishHelix(hx);   // (el árbol no tiene muebles que esperar: sus puertas, en el sitio de siempre)
        Debug.Log($"[Árbol] «{TreeSpec.Building}»: {TreeSpec.Platforms} plataformas ({string.Join(" · ", System.Array.ConvertAll(TreeSpec.Guilds, g => GardenPlot.Title(g)))}), " +
                  $"el árbol más alto del santuario, {TreeSpec.TallestTree:0.#} m.");
        return hx.root;
    }

    // ── LOS PISOS PEGADOS AL TRONCO: sus cultivos ─────────────────────────────────────────────────────────────

    static Transform TrunkFloor(Helix hx, int k, SanctuaryArea garden)
    {
        HelixTower h = hx.h;
        GardenPlot.Guild guild = TreeSpec.Guilds[k - 1];
        float S = h.floorSize;
        Transform f = HelixFloor(hx, k, null, new Vector2(S, S), $"Piso_arbol_{k}_{guild}");
        // EL CULTIVO del piso: lejos de la puerta (el borde −Z, pegado al tronco) y de la rama que pasa por encima de esa franja.
        if (guild == GardenPlot.Guild.Hortalizas && garden != null)
        {
            garden.transform.SetParent(f, false);
            garden.transform.localPosition = new Vector3(0f, 0f, -6.5f);
            garden.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);   // su lado abierto (+Z del área) mira a la puerta
            GameObject go = MakePlot(garden.transform, guild, garden.transform.TransformPoint(new Vector3(-3f, 0.1f, 0f)), new Vector2(6f, 18f), TreeSpec.Seeds[k - 1], false);
            go.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);   // (sus filas, a lo largo del área: caben todas las del gremio)
        }
        else
        {
            GameObject go = MakePlot(f, guild, f.TransformPoint(new Vector3(1f, 0.06f, 4f)), new Vector2(22f, 18f), TreeSpec.Seeds[k - 1], false);
            go.transform.localRotation = Quaternion.identity;
            if (k == 6) go.GetComponent<GardenPlot>().speciesOffset = 5;   // el segundo bosque comestible: los otros árboles del dosel
            KitchenProps.C(go.transform, new Vector3(0f, -0.03f, 0f), new Vector3(24f, 0.06f, 20f), new Color(0.70f, 0.64f, 0.58f), 0f, "Suelo_Huerto");
        }
        _treeFloors.Add(f.GetComponent<TreeFloor>());
        return f;
    }

    /// <summary>Lo que crece en el campo de punta k: un bancal de 8,6 × 15 m a lo ancho del campo (lejos de la columna y de su
    /// puerta), con las filas a lo largo de los 15 m (caben todas las del gremio: la chacra, 8; la rotación, 6).</summary>
    static string TreeTip(int k, Transform tip)
    {
        GardenPlot.Guild guild = TreeSpec.Guilds[k - 1];
        tip.name = $"Campo_punta_{k}_{guild}";
        GameObject plot = MakePlot(tip, guild, tip.TransformPoint(new Vector3(0f, 0.06f, 3.3f)), new Vector2(8.6f, 15f), TreeSpec.Seeds[k - 1], false);
        plot.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
        KitchenProps.C(plot.transform, new Vector3(0f, -0.03f, 0f), new Vector3(9.2f, 0.06f, 15.6f),
            guild == GardenPlot.Guild.Arrozal ? new Color(0.62f, 0.78f, 0.84f) : new Color(0.70f, 0.64f, 0.58f), 0f, "Suelo_Huerto");
        _treeFloors.Add(tip.GetComponent<TreeFloor>());
        return GardenPlot.Title(guild);
    }

    /// <summary>Al final de <see cref="PlaceCarpetAreas"/>: el árbol v2 ya se construye en su forma (pisos planos, girados de 90 en
    /// 90°); solo queda anotarlo.</summary>
    static void PoseTree()
    {
        if (_treeRoot == null || _treePosed) return;
        _treePosed = true;
        Debug.Log($"[Árbol] Hélice lista: {_treeFloors.Count} plataformas (pisos pegados al tronco y campos de punta), lados alternos de un nivel al siguiente.");
    }
}
