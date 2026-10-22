/// <summary>Avisos de la planta atómica en pantalla (vía <see cref="PlaneMessenger"/>, que también los deja en el log).</summary>
public static class PlantNotice
{
    public static void Say(string text) => PlaneMessenger.Send("Planta", text, 9f);
}
