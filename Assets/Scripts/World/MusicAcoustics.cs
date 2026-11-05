using UnityEngine;

/// <summary>
/// CUÁNTO SONIDO PARA EL CRISTAL (Berón 2026-11-05: «se pueden aprovechar las paredes transparentes para bloquear sonido»; docs/piso-musica.md §Acústica).
/// El juego no simula el sonido: esto es la CUENTA con la que se dimensionaron las salas, y la prueba 86 la comprueba. Un paño suelto, con la puerta abierta
/// y sin tapa no aísla casi nada: por eso cada sala lleva DOBLE cristal laminado con cámara y tapa de cristal, y sus puertas se cierran solas (<see cref="SoundDoor"/>).
/// Cifras (Rw, dB): cristal laminado de 12,5 mm ~40; doble acristalamiento acústico de catálogo 44–52, y en obra hasta 8 menos; la meta entre salas de ensayo
/// es ~STC 50 (55 para uso silencioso) y STC 40 en las puertas de sala de música. La ley de masas de la pared compuesta es física de libro.
/// </summary>
public static class MusicAcoustics
{
    /// <summary>Cámara de aire entre los dos cristales (m). 200 mm es el doble acristalamiento clásico de ventana de estudio.</summary>
    public const float Cavity = 0.2f;
    /// <summary>Rw de diseño de una pared de doble cristal laminado con cámara ancha, ya descontada la obra (dB).</summary>
    public const float DoubleGlassRw = 45f;
    /// <summary>Rw de la puerta sónica cerrada (dB).</summary>
    public const float DoorRw = 40f;
    /// <summary>Lo que hace falta entre dos salas de ensayo en uso normal (dB).</summary>
    public const float TargetBetweenRooms = 50f;
    /// <summary>Lo que suma la segunda sala: entre dos salas vecinas hay DOS paredes dobles, cada una con su aire. Se cuenta a la baja, por los flancos del suelo (dB).</summary>
    public const float SecondWallBonus = 8f;

    /// <summary>Aislamiento de una pared hecha de varias partes (ley de la pared compuesta): manda la parte más débil, por pequeña que sea.
    /// Cada parte: su superficie (m²) y su Rw (dB; 0 = un hueco abierto).</summary>
    public static float Composite(params (float area, float rw)[] parts)
    {
        float total = 0f, leak = 0f;
        foreach (var (area, rw) in parts) { total += area; leak += area * Mathf.Pow(10f, -rw / 10f); }
        return total <= 0f ? 0f : -10f * Mathf.Log10(leak / total);
    }

    /// <summary>Aislamiento de una sala de <paramref name="size"/> (x, z): cuatro paredes y tapa de doble cristal, con su puerta cerrada o abierta.
    /// Sin <paramref name="lid"/>, la abertura de arriba cuenta como un hueco.</summary>
    public static float Room(Vector2 size, bool doorOpen, bool lid = true)
    {
        float door = GlassWall.DoorWidth * GlassWall.DoorHeight, walls = 2f * (size.x + size.y) * GlassWall.Height - door, top = size.x * size.y;
        return Composite((walls, DoubleGlassRw), (door, doorOpen ? 0f : DoorRw), (top, lid ? DoubleGlassRw : 0f));
    }

    /// <summary>Aislamiento entre dos salas vecinas, cada una con su doble cristal y su puerta cerrada.</summary>
    public static float BetweenRooms(Vector2 size) => Room(size, false) + SecondWallBonus;
}
