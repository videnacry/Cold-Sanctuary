using System.Collections.Generic;
using UnityEngine;

// (En su propio archivo: Unity solo serializa en la escena un MonoBehaviour cuyo archivo se llama como la clase; si no, en Play queda «Missing Script».)
/// <summary>EL LABORATORIO DE CONTENCIÓN, abajo: solo se ve. Como <see cref="MarineBase"/>, existe mientras la escena está cargada, y arriba solo se le mira por <see cref="Instance"/>.</summary>
public class HunterLab : MonoBehaviour
{
    public static HunterLab Instance { get; private set; }
    public IceCarnivorousTree tree;
    public List<GameObject> hives = new List<GameObject>();
    [Tooltip("La caja de la celda, en coordenadas de este objeto.")] public Bounds cell;

    void OnEnable()
    {
        Instance = this;
        if (HunterCell.Instance != null) Show(HunterCell.Instance.loop.Scale, HunterCell.Instance.loop.Nests);
    }
    void OnDisable() { if (Instance == this) Instance = null; }

    public bool Contains(Vector3 world) => cell.Contains(transform.InverseTransformPoint(world));
    /// <summary>Más nutrientes, más ramas y más nidos.</summary>
    public void Show(float scale, int nests)
    {
        if (tree == null) return;
        tree.transform.localScale = Vector3.one * scale;
        for (int i = 0; i < hives.Count; i++) if (hives[i] != null) hives[i].SetActive(i < nests);
    }
}
