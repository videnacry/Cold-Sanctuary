using System.Collections.Generic;
using UnityEngine;

// (En su propio archivo: Unity solo serializa en la escena un MonoBehaviour cuyo archivo se llama como la clase; si no, en Play queda «Missing Script».)
/// <summary>Hace salir malas hierbas en un bancal, poco a poco y hasta un tope. <see cref="GardenPlot"/> lo añade al generarse.</summary>
public class WeedPatch : MonoBehaviour
{
    public const int Cap = 5;
    public float everySeconds = 240f;
    GardenPlot _plot;
    System.Random _rnd;
    float _next;

    void Start()
    {
        _plot = GetComponent<GardenPlot>();
        _rnd = new System.Random(_plot != null ? _plot.seed * 31 + 7 : 7);
        for (int i = 0; i < 3; i++) Sprout();
        _next = Time.time + everySeconds;
    }

    void Update()
    {
        if (Time.time < _next) return;
        _next = Time.time + everySeconds;
        int n = 0; foreach (GardenWeed w in GardenWeed.All) if (w != null && w.plot == _plot) n++;
        if (n < Cap) Sprout();
    }

    public void Sprout()
    {
        if (_plot == null) return;
        WildPlants.Species s = WildPlants.All[_rnd.Next(WildPlants.All.Length)];
        Vector2 half = _plot.size * 0.5f;
        Vector3 local = new Vector3(((float)_rnd.NextDouble() - 0.5f) * 2f * half.x * 0.9f, 0.1f, ((float)_rnd.NextDouble() - 0.5f) * 2f * half.y * 0.9f);
        var g = new GameObject("Mala_hierba_" + s.key); g.transform.SetParent(transform, false); g.transform.localPosition = local;
        GardenWeed w = g.AddComponent<GardenWeed>(); w.species = s; w.plot = _plot;
        for (int i = 0; i < 5; i++)
        {
            GameObject b = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Object.Destroy(b.GetComponent<Collider>());
            b.name = "Brizna"; b.transform.SetParent(g.transform, false);
            b.transform.localRotation = Quaternion.Euler((float)_rnd.NextDouble() * 30f - 15f, i * 72f, (float)_rnd.NextDouble() * 30f - 15f);
            b.transform.localPosition = b.transform.localRotation * new Vector3(0f, 0.07f, 0f);
            b.transform.localScale = new Vector3(0.02f, 0.15f, 0.012f);
            b.GetComponent<Renderer>().sharedMaterial = KitchenProps.Mat(s.color);
        }
    }
}
