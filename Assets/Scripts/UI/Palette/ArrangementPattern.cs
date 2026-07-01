using UnityEngine;

// Base para los patrones de disposición espacial de bloques.
// Compute() devuelve una posición de mundo por cada elemento dado.
// El jugador define la referencia de origen (posición + orientación).
// magnitude viene del PaletteResult y escala la estructura.
public abstract class ArrangementPattern
{
    public abstract Vector3[] Compute(int count, Transform player, float magnitude);
}

// Escalones ascendentes en dirección frontal del jugador.
// Útil para escalar muros o llegar a superficies elevadas.
[System.Serializable]
public class StairsPattern : ArrangementPattern
{
    public float stepDepth  = 0.7f;   // separación horizontal entre peldaños
    public float stepHeight = 0.25f;  // altura por peldaño (igual que blockScale.y recomendado)

    public override Vector3[] Compute(int count, Transform player, float magnitude)
    {
        Vector3[] pos = new Vector3[count];
        for (int i = 0; i < count; i++)
            pos[i] = player.position
                   + player.forward * stepDepth  * (i + 1) * magnitude
                   + Vector3.up     * stepHeight * i;
        return pos;
    }
}

// Muro perpendicular frente al jugador. Columnas → filas si hay más bloques.
// Útil como barrera ante un ataque o para obstruir un paso.
[System.Serializable]
public class BarrierPattern : ArrangementPattern
{
    public float distance    = 1.5f;  // metros frente al jugador
    public float blockWidth  = 1.1f;  // separación horizontal entre bloques
    public float blockHeight = 0.3f;  // separación vertical entre filas

    public override Vector3[] Compute(int count, Transform player, float magnitude)
    {
        Vector3[] pos  = new Vector3[count];
        int       cols = Mathf.Max(1, Mathf.CeilToInt(Mathf.Sqrt(count)));
        Vector3   center = player.position + player.forward * distance * magnitude;

        for (int i = 0; i < count; i++)
        {
            int col = i % cols;
            int row = i / cols;
            pos[i] = center
                   + player.right * (col - (cols - 1) * 0.5f) * blockWidth
                   + Vector3.up   * row * blockHeight;
        }
        return pos;
    }
}

// Grid horizontal elevado: plataforma para descansar o evitar depredadores.
[System.Serializable]
public class PlatformPattern : ArrangementPattern
{
    public float heightOffset = 3f;   // metros sobre el jugador
    public float spacing      = 1.1f; // separación entre bloques del grid

    public override Vector3[] Compute(int count, Transform player, float magnitude)
    {
        Vector3[] pos  = new Vector3[count];
        int       cols = Mathf.Max(1, Mathf.CeilToInt(Mathf.Sqrt(count)));
        int       rows = Mathf.CeilToInt((float)count / cols);
        Vector3   center = player.position + Vector3.up * heightOffset * magnitude;

        for (int i = 0; i < count; i++)
        {
            int col = i % cols;
            int row = i / cols;
            pos[i] = center
                   + player.right   * (col - (cols - 1) * 0.5f) * spacing
                   + player.forward * (row - (rows - 1) * 0.5f) * spacing;
        }
        return pos;
    }
}

// Cluster de bloques justo debajo del jugador: amortigua una caída libre.
// En caída libre, el jugador activa el hechizo y los bloques aparecen bajo sus pies.
[System.Serializable]
public class FallBreakerPattern : ArrangementPattern
{
    public float dropOffset = 2f;    // metros bajo el jugador al momento del hechizo
    public float spacing    = 0.6f;

    public override Vector3[] Compute(int count, Transform player, float magnitude)
    {
        Vector3[] pos  = new Vector3[count];
        int       cols = Mathf.Max(1, Mathf.CeilToInt(Mathf.Sqrt(count)));
        Vector3   center = player.position + Vector3.down * dropOffset;

        for (int i = 0; i < count; i++)
        {
            int col = i % cols;
            int row = i / cols;
            pos[i] = center
                   + player.right   * (col - (cols - 1) * 0.5f) * spacing
                   + player.forward * (row - (Mathf.CeilToInt((float)count / cols) - 1) * 0.5f) * spacing;
        }
        return pos;
    }
}
