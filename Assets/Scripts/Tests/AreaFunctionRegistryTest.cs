using System.Collections;
using System.Linq;
using UnityEngine;

/// <summary>
/// TEST DEL REGISTRO DE FUNCIONES DE ÁREA (docs/microcosmos-civilizaciones.md §2). El catálogo de la jornada del santuario
/// (<see cref="AreaWork.Catalog"/>) ahora sale de <see cref="AreaFunctions"/>: debe ser EXACTAMENTE el de antes (mismos ids en
/// el mismo orden, horas, obligatorias, grupos, quién dirige, «también va a…» y la que recorre las áreas), los efectos de
/// terminar una misión deben estar en el registro, y los edificios de los microcosmos deben filtrar por época (sin vacunas
/// antes de 1796, sin cocina antes del fuego). Datos puros: sin escena.
/// </summary>
public class AreaFunctionRegistryTest : MonoBehaviour, ITestUnit
{
    public int Group => 61;
    public bool ParallelSafe => true;

    // El catálogo de AreaWork tal como estaba antes del registro (2026-11-01), en su orden.
    static readonly string[] Ids =
    {
        "enf_iniciacion", "enf_visita", "enf_animales", "enf_inventario", "hos_urgencias", "hos_quirofano", "hos_uci", "hos_veterinaria",
        "coc_servicio", "coc_limpieza", "coc_obrador", "coc_sala", "hue_riego", "hue_cosecha", "hue_visitantes",
        "yog_guia", "yog_demo", "yog_corrige_1", "yog_corrige_2", "coc_atender", "yog_sala", "mec_vehiculos", "mec_maquinas",
        "tex_tejer", "tex_coser", "lab_biorreactor", "lab_iniciar", "lab_cosechar", "lab_limpieza", "coc_comedor", "yog_aseos",
        "tex_limpieza", "mec_limpieza", "con_almacen", "cri_tomas", "cri_recintos", "cri_vinculo", "gym_limpieza", "gym_mantener",
        "pla_operar", "pla_isotopos", "pla_ronda", "pla_ciclotron", "fis_experimentos", "qui_sintesis", "qui_residuos",
        "bio_cultivo", "bio_pcr", "nuc_turno", "gua_carnivoros", "gua_sala", "gua_alimentador", "gua_alimentador_2", "hue_transplante",
        // (2026-11-04) los pisos nuevos: Informática (el clúster de IA) y el Aula (docs/piso-informatica.md, docs/piso-aula.md)
        "ifo_sysadmin", "ifo_refrigeracion", "ifo_energia", "aul_clase", "aul_estudio",
        // (2026-11-05) la asamblea y la música (docs/piso-asambleas.md, docs/piso-musica.md)
        "asa_orden", "asa_acta", "asa_mediar", "son_grabar", "son_ensayo", "son_equipo",
        // (2026-11-04/05) las profesiones (docs/personajes-mesocosmos.md §Profesiones)
        "aul_legal", "aul_lenguas", "coc_cuentas", "con_proyectos", "con_proyectos2", "coc_avisos", "yog_ashtanga", "aul_letras", "hos_plantas", "hos_pediatria", "mec_alfombras", "lan_control",
        // (2026-11-08) el Huerto vertical de la torre de Producción (docs/produccion-maquinaria.md)
        "huv_siembra", "huv_cosecha", "huv_solucion",
    };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<AreaFunctionRegistryTest>() == null) tr.gameObject.AddComponent<AreaFunctionRegistryTest>();
    }

    public IEnumerator Run()
    {
        TestProbe.Begin("Registro de funciones de área: el catálogo del santuario igual que antes, efectos y edificios por época");
        var c = AreaWork.Catalog;
        string order = "";
        for (int i = 0; i < Mathf.Max(c.Length, Ids.Length); i++)
            if (i >= c.Length || i >= Ids.Length || c[i].id != Ids[i]) { order = $"posición {i + 1}: «{(i < c.Length ? c[i].id : "—")}» en vez de «{(i < Ids.Length ? Ids[i] : "—")}»"; break; }
        TestProbe.Check("mismas 80 misiones (el turno de la sala de guardia + 50 de antes + 5 de Informática y el Aula + 3 del alimentador y el trasplante + 3 de la asamblea + 3 de la música + 12 de las profesiones + 3 del Huerto vertical; las de energía y nuclear salen por día, no aquí) y en el mismo orden", order.Length == 0 && c.Length == Ids.Length, order.Length > 0 ? order : $"{c.Length}");
        TestProbe.Near("mismas horas en total (89,5 h)", c.Sum(m => m.hours), 89.5f, 0.01f);
        TestProbe.Check("mismas obligatorias (21)", c.Count(m => m.mandatory) == 21, $"{c.Count(m => m.mandatory)}");
        TestProbe.Check("dirigen: iniciación, quirófano, guiar y demostrar la clase", c.Where(m => m.lead).Select(m => m.id).OrderBy(x => x).SequenceEqual(new[] { "enf_iniciacion", "hos_quirofano", "yog_demo", "yog_guia" }));
        TestProbe.Check("la clase de yoga es un grupo de 4", c.Count(m => m.group == "clase_yoga") == 4);
        TestProbe.Check("solo «Reparar maquinaria» recorre las áreas", c.Where(m => m.roams).Select(m => m.id).SequenceEqual(new[] { "mec_maquinas" }));
        WorkRoster.Mission M(string id) => c.First(m => m.id == id);
        TestProbe.Check("«también va a…» igual (cosecha → Cocina; animales → Cría; lote → Cocina y Cría; isótopos → Hospital)",
            M("hue_cosecha").also.SequenceEqual(new[] { "Cocina" }) && M("enf_animales").also.SequenceEqual(new[] { "Cría" }) &&
            M("lab_cosechar").also.SequenceEqual(new[] { "Cocina", "Cría" }) && M("pla_isotopos").also.SequenceEqual(new[] { AreaWork.Hospital }) && M("pla_ciclotron").also.SequenceEqual(new[] { AreaWork.Hospital }));

        // Los efectos (antes, el switch de WorkdayDirector.Apply) están en el registro.
        foreach (string id in new[] { "mat_guadua", "mat_bahareque", "mat_micelio", "mat_cuerda", "lab_iniciar", "lab_cosechar", "coc_servicio" })
            TestProbe.Check($"«{id}» trae su efecto", AreaFunctions.Get(id) != null && AreaFunctions.Get(id).effect != null);
        TestProbe.Check("las obras van por prefijo (una obra que no existe no hace nada)", AreaFunctions.Run("obra_material_(prueba inexistente)", "Prueba"));
        TestProbe.Check("una misión sin efecto no hace nada", !AreaFunctions.Run("yog_sala", "Prueba"));

        // Los edificios de los microcosmos, por época.
        var scutari = AreaFunctions.ForSite("Hospital", 1854).Select(f => f.id).ToList();
        TestProbe.Check("hospital de 1854: pasar visita, lavandería y vacunar (vacunas desde 1796)", scutari.Contains("enf_visita") && scutari.Contains("hos_lavanderia") && scutari.Contains("hos_vacunar"));
        TestProbe.Check("hospital de Kapilavastu (−530): sin vacunas", !AreaFunctions.ForSite("Hospital", -530).Any(f => f.id == "hos_vacunar"));
        TestProbe.Check("antes del fuego no se cocina", AreaFunctions.ForSite("Cocina", -1000000).Count == AreaFunctions.ForSite("Cocina", -1000000).Count(f => f.kind != AreaFunctions.Kind.Cook));
        TestProbe.Check("el dojo de 1957 reutiliza sus funciones (clase, guardia, muñeco, combate)", AreaFunctions.ForSite("Dojo", 1957).Count == 4);
        TestProbe.Check("el restaurante reutiliza el servicio de la carta del santuario", AreaFunctions.ForSite("Restaurante", 1964).Any(f => f.id == "coc_servicio" && f.sanctuary));
        string noStation = string.Join(", ", AreaFunctions.Buildings.SelectMany(b => b.Value).Distinct().Where(id => AreaFunctions.Get(id) == null || AreaFunctions.Get(id).stations.Length == 0));
        TestProbe.Check("cada función de edificio existe y dice qué puesto necesita", noStation.Length == 0, noStation);
        TestProbe.End();
        yield break;
    }
}
