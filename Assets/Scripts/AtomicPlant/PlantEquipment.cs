using UnityEngine;

/// <summary>
/// LOS EQUIPOS DE LA PLANTA que no son la consola, cada uno con su trabajo real (F):
/// - <b>Puente de la piscina</b>: miras al núcleo, 7 m bajo el agua. El brillo azul es la radiación de CHERENKOV:
///   electrones más rápidos que la luz EN EL AGUA (c/1,33). Brilla según la potencia. El agua es refrigerante,
///   moderador y blindaje a la vez.
/// - <b>Purificación</b>: cambiar las resinas de intercambio iónico cuando la conductividad pasa de 2 µS/cm (el agua
///   pura no corroe el aluminio ni se activa).
/// - <b>Bomba del primario e intercambiador</b>: la arranca o la para; muestra el calor que saca al agua de mar.
/// - <b>Torre de refrigeración</b> (secundario): purga y revisión del ventilador.
/// - <b>Cámara de niebla</b>: ver las trazas del fondo natural (alfa gordas y cortas, beta finas y torcidas, muones rectos).
/// - <b>Contador Geiger</b>: medir la tasa de dosis donde estás (y probar a acercarte o alejarte: 1/r²).
/// </summary>
public class PlantEquipment : MonoBehaviour, IInteractable
{
    public enum Kind { PoolBridge, Purification, PrimaryPump, CoolingTower, CloudChamber, GeigerCounter }

    public Kind kind;

    float _towerServiced = -999f;

    public string InteractLabel
    {
        get
        {
            switch (kind)
            {
                case Kind.PoolBridge: return "Mirar al núcleo desde el puente";
                case Kind.Purification: return "Purificación del agua (resinas)";
                case Kind.PrimaryPump: return R != null && R.pumpOn ? "Parar la bomba del primario" : "Arrancar la bomba del primario";
                case Kind.CoolingTower: return "Revisar la torre de refrigeración";
                case Kind.CloudChamber: return "Mirar la cámara de niebla";
                default: return "Medir con el contador Geiger";
            }
        }
    }

    public bool CanInteract => true;

    static ResearchReactor R => ResearchReactor.Instance;

    static GameObject Player => GameObject.FindGameObjectWithTag("Player");

    bool NeedsDosimeter()
    {
        GameObject p = Player;
        if (p != null && p.GetComponent<PersonalDosimeter>() != null) return false;
        PlantNotice.Say("Zona controlada: coge antes el dosímetro en la taquilla de la entrada.");
        return true;
    }

    public void Interact()
    {
        switch (kind)
        {
            case Kind.PoolBridge:
                if (NeedsDosimeter() || R == null) return;
                if (R.power < 100f) PlantNotice.Say($"El núcleo, a 7 m bajo el agua, apenas se ve ({ResearchReactor.FormatPower(R.power)}). El agua lo enfría, frena los neutrones (moderador) y te protege (blindaje).");
                else PlantNotice.Say($"Brillo AZUL a {ResearchReactor.FormatPower(R.power)}: radiación de Cherenkov. Los electrones van más rápido que la luz EN EL AGUA (que allí va a c/1,33) y emiten luz, como un avión supersónico emite su estampido.");
                if (Random.value < 0.15f) { PersonalDosimeter d = Player != null ? Player.GetComponent<PersonalDosimeter>() : null; if (d != null) d.contaminated = true; }
                return;
            case Kind.Purification:
                if (NeedsDosimeter() || R == null) return;
                if (R.conductivity <= 2f) { PlantNotice.Say($"Conductividad {R.conductivity:0.00} µS/cm: el agua está bien. Se cambian las resinas por encima de 2 µS/cm."); return; }
                R.conductivity = 0.8f;
                PlantNotice.Say("Resinas cambiadas: el agua vuelve a 0,8 µS/cm. Los iones disueltos corroen el revestimiento y se activan con los neutrones; el agua pura, no.");
                return;
            case Kind.PrimaryPump:
                if (R == null) return;
                R.pumpOn = !R.pumpOn;
                float q = R.pumpOn ? R.exchangerUA * Mathf.Max(0f, R.poolTemp - R.seaWaterTemp) : 0f;
                PlantNotice.Say(R.pumpOn ? $"Bomba en marcha: el intercambiador saca hasta {ResearchReactor.FormatPower(Mathf.Min(q, 2e6f))} al agua de mar ({R.seaWaterTemp:0} °C). Los dos circuitos nunca se mezclan."
                                         : "Bomba parada: la piscina solo se enfría por su superficie. A potencia alta se calentará (~3 °C/h por MW).");
                return;
            case Kind.CoolingTower:
                if (Time.time - _towerServiced < 600f) { PlantNotice.Say("La torre ya está revisada hoy."); return; }
                _towerServiced = Time.time;
                PlantNotice.Say("Torre revisada: ventilador engrasado y purga hecha (el agua que se evapora deja sales: hay que renovarla). Es el circuito SECUNDARIO: aquí no llega nada radiactivo.");
                return;
            case Kind.CloudChamber:
                PlantNotice.Say("Cámara de niebla (Wilson, 1911): vapor de alcohol sobresaturado. Trazas gordas y cortas = alfa; finas y en zigzag = beta; largas y rectas = muones de los rayos cósmicos. La radiación siempre estuvo ahí.");
                return;
            default:
                GameObject p = Player;
                float rate = RadiationSource.TotalRateAt((p != null ? p.transform.position : transform.position) + Vector3.up);
                PlantNotice.Say($"Geiger: {rate:0.00} µSv/h (fondo natural ≈ 0,1). Al doble de distancia de una fuente, la cuarta parte: 1/r².");
                return;
        }
    }
}
