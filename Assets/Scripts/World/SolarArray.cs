using UnityEngine;

/// <summary>
/// PANELES SOLARES (el santuario no usa gas ni combustibles: toda su energía es solar). Una fila de paneles inclinados
/// sobre postes —elevados, como todo lo construido, para no quitar suelo— que de DÍA genera energía para el santuario
/// (SanctuaryResources.Energy), más al mediodía y nada de noche. Alimenta cocinas de inducción, hornos, máquinas de la
/// enfermería y los tanques de virtualización. Todo de cubos (marco, células, postes, caja del inversor y baterías).
/// </summary>
public class SolarArray : MonoBehaviour
{
    [Tooltip("Potencia pico de la fila (vatios = julios por segundo de juego a mediodía).")]
    public float peakWatts = 6000f;
    public int panels = 6;

    float _acc;

    void Update()
    {
        TimeController tc = TimeController.timeController;
        float hour = tc != null ? tc.Hour : 12f;
        // Curva solar: 0 de noche, máximo a las 12 h (seno entre las 6 y las 18).
        float sun = hour > 6f && hour < 18f ? Mathf.Sin((hour - 6f) / 12f * Mathf.PI) : 0f;
        if (sun <= 0f) return;
        _acc += peakWatts * sun * Time.deltaTime;
        if (_acc < 1000f) return;   // abona por kJ para no saturar el registro
        SanctuaryResources.Instance.Add(SanctuaryResource.Energy, _acc);
        _acc = 0f;
    }

    /// <summary>Fila de paneles a <paramref name="height"/> m sobre postes, orientada al sol (+Z = sur del santuario).</summary>
    public static SolarArray Build(Transform parent, string name, Vector3 groundPos, float yaw, int panels = 6, float height = 3f)
    {
        Transform t = new GameObject(name).transform;
        t.SetParent(parent, true);
        t.position = groundPos;
        t.rotation = Quaternion.Euler(0f, yaw, 0f);
        Color frame = new Color(0.72f, 0.74f, 0.78f), cell = new Color(0.16f, 0.26f, 0.45f), grid = new Color(0.55f, 0.62f, 0.72f), post = new Color(0.55f, 0.56f, 0.6f);
        float w = 1.1f, len = panels * w;
        for (int i = 0; i <= panels; i += 2)
        {
            float x = -len * 0.5f + i * w;
            KitchenProps.C(t, new Vector3(x, height * 0.5f, -0.4f), new Vector3(0.08f, height, 0.08f), post, 0f, "Poste");
            KitchenProps.C(t, new Vector3(x, (height + 0.6f) * 0.5f, 0.4f), new Vector3(0.08f, height + 0.6f, 0.08f), post, 0f, "Poste");
        }
        Transform rack = new GameObject("Bastidor").transform;
        rack.SetParent(t, false);
        rack.localPosition = new Vector3(0f, height + 0.3f, 0f);
        rack.localRotation = Quaternion.Euler(-30f, 0f, 0f);   // inclinados 30° hacia el sol
        for (int i = 0; i < panels; i++)
        {
            float x = -len * 0.5f + (i + 0.5f) * w;
            KitchenProps.C(rack, new Vector3(x, 0f, 0f), new Vector3(w - 0.04f, 0.05f, 1.7f), frame, 0f, "Marco");
            KitchenProps.C(rack, new Vector3(x, 0.03f, 0f), new Vector3(w - 0.12f, 0.02f, 1.6f), cell, 0f, "Celulas");
            for (int g = 1; g < 6; g++) KitchenProps.C(rack, new Vector3(x, 0.042f, -0.8f + g * 0.27f), new Vector3(w - 0.12f, 0.004f, 0.012f), grid, 0f, "Rejilla");
        }
        KitchenProps.C(t, new Vector3(len * 0.5f + 0.6f, 1.6f, 0f), new Vector3(0.5f, 0.7f, 0.3f), new Color(0.9f, 0.9f, 0.88f), 0f, "Inversor");
        KitchenProps.C(t, new Vector3(len * 0.5f + 0.6f, 0.9f, 0f), new Vector3(0.6f, 0.5f, 0.4f), new Color(0.3f, 0.45f, 0.35f), 0f, "Baterias");
        SolarArray s = t.gameObject.AddComponent<SolarArray>();
        s.panels = panels; s.peakWatts = panels * 400f;
        WorldLabel wl = t.gameObject.AddComponent<WorldLabel>(); wl.text = "Paneles solares"; wl.height = height + 1.6f;
        return s;
    }
}
