using UnityEngine;

/// <summary>
/// La OBRA de un edificio griego que se alza con la piedra que traen los trabajadores (un <see cref="MicroLair"/>: cada
/// bloque entregado suma uno a su <c>stock</c>). Cada ETAPA (crepis, columnas, cella, entablamento, frontón) es un grupo
/// de piezas que aparece al llegar a su umbral de bloques. Modelo: el templo de Afaya en Egina (dórico, 6 × 12 columnas).
/// </summary>
[RequireComponent(typeof(MicroLair))]
public class GreekBuildSite : MonoBehaviour
{
    public string[] stageNames = { "Crepis", "Columnas", "Cella", "Entablamento", "Frontón y tejado" };
    [Tooltip("Bloques acumulados que necesita cada etapa.")]
    public int[] stageBlocks = { 10, 35, 50, 65, 80 };
    public GameObject[] stages = new GameObject[0];

    MicroLair _lair;
    public int Stage { get; private set; }
    public int Blocks => _lair != null ? _lair.stock : 0;
    public bool Done => Stage >= stages.Length;
    public string StageName => Done ? "terminado" : stageNames[Mathf.Min(Stage, stageNames.Length - 1)];
    public int NextBlocks => Done ? Blocks : stageBlocks[Mathf.Min(Stage, stageBlocks.Length - 1)];

    void Awake() { _lair = GetComponent<MicroLair>(); foreach (GameObject g in stages) if (g != null) g.SetActive(false); }

    void Update()
    {
        while (!Done && Blocks >= stageBlocks[Mathf.Min(Stage, stageBlocks.Length - 1)])
        {
            if (stages[Stage] != null) stages[Stage].SetActive(true);
            MythMission.Instance?.Say("", $"Se alza: {stageNames[Mathf.Min(Stage, stageNames.Length - 1)]}.");
            Stage++;
        }
    }
}
