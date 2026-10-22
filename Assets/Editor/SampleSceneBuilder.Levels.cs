using UnityEngine;

/// <summary>
/// ÁREAS POR PISOS (docs/construcciones-elevadas.md §Pisos): en vez de una plataforma por área unida por puentes, una
/// misma base puede tener
/// - PLATAFORMAS HIJAS (entreplantas) de 3 m con ESCALERA INCLINADA, dentro de la plataforma: p. ej. las camas de la
///   enfermería en una galería en U, que deja el suelo libre para pasillos anchos;
/// - un PISO SUPERIOR a 10 m (no tapa la vista del de abajo), con columnas solo en el perímetro y escalera vertical
///   desde el piso de abajo: yoga + ballet, gimnasio + dojo, dos huertos en la misma base.
/// Así hay menos plataformas separadas y menos puentes.
/// </summary>
public static partial class SampleSceneBuilder
{
    public const float UpperFloorGap = 10f, MezzanineHeight = 3f;   // 3 m: debajo se pasa de pie sin chocar con la losa
    const float StairAngle = 38f;   // pendiente de las escaleras inclinadas (una escalera de casa ronda los 35–40°)
    const float SetBack = 3f;   // el piso de arriba es 3 m más corto por +Z: ahí queda el pie de su escalera

    /// <summary>Pone <paramref name="upper"/> como PISO SUPERIOR de <paramref name="lower"/> (ya elevada), 10 m más arriba.
    /// Devuelve el tamaño del piso (el de abajo, 3 m más corto por +Z, donde baja su escalera).</summary>
    static Vector2 StackOn(Transform upper, Transform lower, Vector2 lowerSize)
    {
        Vector2 size = new Vector2(lowerSize.x, lowerSize.y - SetBack);
        upper.SetPositionAndRotation(lower.TransformPoint(new Vector3(0f, 0f, -SetBack * 0.5f)), lower.rotation);
        // Su escalera, a un lado del eje (en el eje llega la del piso de abajo; a +X está la máquina de virtualización).
        Elevate(upper, size, UpperFloorGap, groundBelow: false, ladderX: -size.x * 0.3f);
        return size;
    }

    /// <summary>ENTREPLANTA: una plataforma hija de <paramref name="size"/> con su cara de arriba a <paramref name="h"/>,
    /// sobre postes finos en el perímetro (cada ~4 m). <paramref name="noPosts"/> = lado sin postes (x/y = −1/+1: el borde
    /// −X/+X o −Z/+Z), p. ej. el que da al borde de la plataforma, por donde puede llegar un puente.
    /// Devuelve el Transform a la altura de la cara de arriba.</summary>
    static Transform Deck(Transform parent, string name, Vector3 center, Vector2 size, float h = MezzanineHeight, Vector2Int noPosts = default)
    {
        Color wood = new Color(0.62f, 0.52f, 0.42f), woodDark = new Color(0.48f, 0.4f, 0.33f);
        Transform d = new GameObject(name).transform; d.SetParent(parent, false); d.localPosition = center;
        Solid(d, new Vector3(0f, h - 0.075f, 0f), new Vector3(size.x, 0.15f, size.y), woodDark, "Losa_entreplanta");
        float hx = size.x * 0.5f - 0.1f, hz = size.y * 0.5f - 0.1f;
        int nx = Mathf.Max(1, Mathf.CeilToInt(size.x / 4f)), nz = Mathf.Max(1, Mathf.CeilToInt(size.y / 4f));
        for (int i = 0; i <= nx; i++)
            for (int k = 0; k <= nz; k++)
            {
                if (i != 0 && i != nx && k != 0 && k != nz) continue;   // solo el perímetro: debajo queda libre
                if ((noPosts.x < 0 && i == 0) || (noPosts.x > 0 && i == nx) || (noPosts.y < 0 && k == 0) || (noPosts.y > 0 && k == nz)) continue;
                Solid(d, new Vector3(-hx + 2f * hx * i / nx, (h - 0.15f) * 0.5f, -hz + 2f * hz * k / nz), new Vector3(0.12f, h - 0.15f, 0.12f), wood, "Poste");
            }
        Transform top = new GameObject("Encima").transform; top.SetParent(d, false); top.localPosition = Vector3.up * h;
        return top;
    }

    /// <summary>ESCALERA INCLINADA (<see cref="StairAngle"/>) de <paramref name="h"/> m: el pie en <paramref name="foot"/> (en el suelo del padre)
    /// y sube hacia <paramref name="yaw"/>. Rampa invisible (para andar y para el NavMesh) + peldaños, zancas y pasamanos.</summary>
    static void InclinedStair(Transform parent, string name, Vector3 foot, float yaw, float h = MezzanineHeight, float width = 1.4f)
    {
        Color wood = new Color(0.62f, 0.52f, 0.42f), woodDark = new Color(0.48f, 0.4f, 0.33f);
        float run = h / Mathf.Tan(StairAngle * Mathf.Deg2Rad), len = Mathf.Sqrt(run * run + h * h);
        Transform s = new GameObject(name).transform; s.SetParent(parent, false);
        s.localPosition = foot; s.localRotation = Quaternion.Euler(0f, yaw, 0f);
        Transform ramp = Solid(s, new Vector3(0f, h * 0.5f, run * 0.5f), new Vector3(width, 0.05f, len), wood, "Rampa");
        ramp.localRotation = Quaternion.Euler(-StairAngle, 0f, 0f);
        ramp.GetComponent<Renderer>().enabled = false;
        int steps = Mathf.CeilToInt(h / 0.18f);
        for (int i = 0; i < steps; i++)
        {
            float y = (i + 1) * h / steps, z = (i + 0.5f) * run / steps;
            KitchenProps.C(s, new Vector3(0f, y - 0.04f, z), new Vector3(width, 0.08f, run / steps + 0.02f), wood, 0f, "Peldano");
        }
        for (int k = -1; k <= 1; k += 2)
        {
            Transform z1 = KitchenProps.C(s, new Vector3(k * width * 0.5f, h * 0.5f, run * 0.5f), new Vector3(0.08f, 0.25f, len), woodDark, 0f, "Zanca");
            z1.localRotation = Quaternion.Euler(-StairAngle, 0f, 0f);
            Transform rail = KitchenProps.C(s, new Vector3(k * width * 0.5f, h * 0.5f + 0.9f, run * 0.5f), new Vector3(0.05f, 0.05f, len), wood, 0f, "Pasamanos");
            rail.localRotation = Quaternion.Euler(-StairAngle, 0f, 0f);
        }
        WorldLabel wl = s.gameObject.AddComponent<WorldLabel>(); wl.text = "Escalera"; wl.height = h + 0.6f;
    }

    /// <summary>Largo (en planta) de una escalera inclinada de <paramref name="h"/> m.</summary>
    static float StairRun(float h = MezzanineHeight) => h / Mathf.Tan(StairAngle * Mathf.Deg2Rad);
}
