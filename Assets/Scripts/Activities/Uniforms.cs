using UnityEngine;

/// <summary>
/// LOS UNIFORMES DE LA JORNADA (docs/actividades-y-uniformes.md §4; Berón 2026-11-04: «sería posible que los humanos se coloquen
/// uniformes para las tareas que vayan a realizar de la jornada?»). Al llegar a su tarea, cada uno se pone el uniforme de esa tarea
/// y al acabarla vuelve a su ropa: blanco de cocina con delantal y gorro, pijama de quirófano o bata blanca con fonendo, peto
/// vaquero y guantes de hortelano, chaleco reflectante y casco de obra, polo del gimnasio, bata de laboratorio con gafas, neopreno
/// para nadar… Se CONSERVA la identidad: el pelo, la cara y la piel no se tocan (solo se recolorean los cubos de la ropa y se añaden
/// piezas «Uniforme_…»). El insecto-humano se pone el delantal de su oficio, del tejido de su era (<see cref="CubeInsect.WearWorkApron"/>).
/// Lo aplica y lo quita <see cref="UniformWearer"/>; aquí está QUÉ uniforme toca y CÓMO es cada uno.
/// </summary>
public static class Uniforms
{
    public enum Kind { None, Cocina, Sala, Quirofano, BataMedica, Veterinaria, Huerto, Obra, Gimnasio, Laboratorio, Neopreno, Limpieza, Taller, Guardia, Textil, Nuclear, Informatica, Cria, Musica, Asamblea, Energia }

    public static string Title(Kind k)
    {
        switch (k)
        {
            case Kind.Cocina: return "chaquetilla blanca, delantal y gorro de cocina";
            case Kind.Sala: return "camisa blanca, chaleco y delantal corto de sala";
            case Kind.Quirofano: return "pijama de quirófano y gorro";
            case Kind.BataMedica: return "bata blanca y fonendo";
            case Kind.Veterinaria: return "pijama azul de veterinaria y fonendo";
            case Kind.Huerto: return "peto vaquero, guantes y sombrero de paja";
            case Kind.Obra: return "chaleco reflectante, casco, guantes y botas";
            case Kind.Gimnasio: return "polo del gimnasio";
            case Kind.Laboratorio: return "bata de laboratorio, gafas y guantes de nitrilo";
            case Kind.Neopreno: return "neopreno";
            case Kind.Limpieza: return "casaca de limpieza y guantes de goma";
            case Kind.Taller: return "mono de mecánico y cinturón de herramientas";
            case Kind.Guardia: return "camisa caqui y gorra de guardia";
            case Kind.Textil: return "delantal de taller y cinta métrica";
            case Kind.Nuclear: return "mono blanco, casco y dosímetro";
            case Kind.Informatica: return "polo oscuro con acreditación";
            case Kind.Asamblea: return "camisa clara con la insignia de la asamblea";
            case Kind.Musica: return "camiseta y pantalón negros de técnico de escenario";
            case Kind.Cria: return "camisa caqui y delantal de cuidador";
            case Kind.Energia: return "mono azul, casco, arnés y guantes aislantes";
            default: return "su ropa";
        }
    }

    /// <summary>EL UNIFORME DE UNA TAREA de la jornada: por la misión (su prefijo) y, si no la nombra, por el área. Comer, el
    /// entrenamiento de yoga, las clases del aula y el tiempo libre van con su ropa (None).</summary>
    public static Kind ForWork(string missionId, string area, string doing)
    {
        if (area == null) return Kind.None;
        if (doing != null && doing.StartsWith("Entrenamiento")) return area == "Gimnasio" ? Kind.Gimnasio : Kind.None;
        if (doing == "Comida") return Kind.None;
        string id = missionId ?? "";
        if (id == "hos_quirofano" || id == "hos_uci" || id == "hos_urgencias" || id == "enf_inventario") return Kind.Quirofano;
        if (id == "hos_veterinaria" || id == "enf_animales" || id == "hos_aves") return Kind.Veterinaria;
        if (id.StartsWith("enf_") || id.StartsWith("hos_")) return Kind.BataMedica;
        if (id == "coc_sala" || id == "coc_atender") return Kind.Sala;
        if (id == "coc_comedor") return Kind.Limpieza;
        if (id.StartsWith("coc_") || id.StartsWith("prep_")) return Kind.Cocina;
        if (id.StartsWith("hue_") || id == "mat_guadua") return Kind.Huerto;
        if (id.StartsWith("obra_") || id.StartsWith("con_") || id == "mat_bahareque") return Kind.Obra;
        if (id == "gym_limpieza" || id == "yog_sala" || id == "yog_aseos" || id.EndsWith("_limpieza") && !id.StartsWith("lab_")) return Kind.Limpieza;
        if (id.StartsWith("gym_") || id.StartsWith("mec_")) return Kind.Taller;
        if (id.StartsWith("lab_") || id.StartsWith("qui_") || id.StartsWith("bio_") || id.StartsWith("fis_") || id == "mat_micelio") return Kind.Laboratorio;
        if (id.StartsWith("pla_") || id.StartsWith("nuc_")) return Kind.Nuclear;
        if (id.StartsWith("tex_") || id == "mat_cuerda") return Kind.Textil;
        if (id.StartsWith("cri_")) return Kind.Cria;
        if (id.StartsWith("gua_")) return Kind.Guardia;
        if (id.StartsWith("ifo_")) return Kind.Informatica;
        if (id.StartsWith("asa_")) return Kind.Asamblea;
        if (id.StartsWith("son_")) return Kind.Musica;
        if (id.StartsWith("sol_") || id.StartsWith("ene_")) return Kind.Energia;
        if (id.StartsWith("yog_") || id.StartsWith("aul_")) return Kind.None;   // (el maestro de yoga y el aula, con su ropa)
        // Sin misión con nombre: por el área.
        switch (area)
        {
            case AreaWork.Hospital: return Kind.BataMedica;
            case "Cocina": return Kind.Cocina;
            case "Huerto": return Kind.Huerto;
            case "Construcción": return Kind.Obra;
            case "Mecánica": return Kind.Taller;
            case "Carne Cultivada": case "Química": case "Biología": return Kind.Laboratorio;
            case "Planta Atómica": case "Central Nuclear": return Kind.Nuclear;
            case "Estudio Textil": return Kind.Textil;
            case "Cría": return Kind.Cria;
            case AreaWork.Guard: case AreaWork.GuardRoom: return Kind.Guardia;
            case "Informática": return Kind.Informatica;
            case "Sala de asambleas": return Kind.Asamblea;
            case "Música": return Kind.Musica;
            case EnergyRule.Area: return Kind.Energia;
        }
        return Kind.None;
    }

    /// <summary>El uniforme de una función del registro (microcosmos): por su clase.</summary>
    public static Kind ForFunction(AreaFunctions.AreaFunction f)
    {
        if (f == null) return Kind.None;
        switch (f.kind)
        {
            case AreaFunctions.Kind.Cook: return Kind.Cocina;
            case AreaFunctions.Kind.Serve: return Kind.Sala;
            case AreaFunctions.Kind.Clean: return Kind.Limpieza;
            case AreaFunctions.Kind.Heal: return Kind.BataMedica;
            case AreaFunctions.Kind.Tend: return Kind.Huerto;
            case AreaFunctions.Kind.Build: return Kind.Obra;
            case AreaFunctions.Kind.Make: return Kind.Taller;
            case AreaFunctions.Kind.Operate: case AreaFunctions.Kind.Study: return f.area == "Laboratorio" || f.area == "Hospital" ? Kind.Laboratorio : Kind.None;
            case AreaFunctions.Kind.Guard: return Kind.Guardia;
            case AreaFunctions.Kind.Care: return f.area == "Casa" ? Kind.None : Kind.Cria;
            case AreaFunctions.Kind.Trade: return Kind.Sala;
            default: return Kind.None;   // practicar, meditar, rezar, descansar, comer, enseñar: con su ropa
        }
    }

    // ── Cómo es cada uno (cuerpo de cubos humano) ───────────────────────────────────────────────────────────────

    public enum Hat { None, Toque, ScrubCap, Helmet, Straw, Cap }

    public struct Spec
    {
        public Color top, sleeves, pants, shoes;
        public Color? forearms, gloves;        // null = como las mangas / la mano de siempre
        public bool shortSleeves, shorts;      // manga corta (antebrazo al aire) · pantalón corto (espinilla al aire)
        public Hat hat; public Color hatColor;
        public bool apron, shortApron, coatTails, stethoscope, hiVis, goggles, toolBelt, collar, badge, tape, dosimeter, wetStripes;
        public Color apronColor;
    }

    static Color H(string hex) { ColorUtility.TryParseHtmlString("#" + hex, out Color c); return c; }

    public static Spec SpecOf(Kind k)
    {
        Color white = H("F3F3EF"), black = H("2B2B2E"), denim = H("3E5F8A"), khaki = H("A89A6A");
        switch (k)
        {
            case Kind.Cocina: return new Spec { top = white, sleeves = white, pants = H("4A4A4F"), shoes = black, hat = Hat.Toque, hatColor = white, apron = true, apronColor = white };
            case Kind.Sala: return new Spec { top = white, sleeves = white, pants = black, shoes = black, shortApron = true, apronColor = black, badge = true };
            case Kind.Quirofano: return new Spec { top = H("4FA59B"), sleeves = H("4FA59B"), shortSleeves = true, pants = H("4FA59B"), shoes = white, hat = Hat.ScrubCap, hatColor = H("4FA59B"), gloves = H("DDE7F0") };
            case Kind.BataMedica: return new Spec { top = white, sleeves = white, pants = H("8A9099"), shoes = H("5A4A3E"), coatTails = true, stethoscope = true, badge = true };
            case Kind.Veterinaria: return new Spec { top = H("3F6FB0"), sleeves = H("3F6FB0"), shortSleeves = true, pants = H("3F6FB0"), shoes = H("3A3A3A"), stethoscope = true };
            case Kind.Huerto: return new Spec { top = H("B04A3A"), sleeves = H("B04A3A"), pants = denim, shoes = H("5A4632"), gloves = H("5E8C4A"), hat = Hat.Straw, hatColor = H("D9C27A"), apron = true, apronColor = denim };
            case Kind.Obra: return new Spec { top = H("F08A24"), sleeves = H("6F7680"), pants = H("3F4652"), shoes = H("6B4A2E"), gloves = H("C9A35A"), hat = Hat.Helmet, hatColor = H("F2C230"), hiVis = true, toolBelt = true };
            case Kind.Gimnasio: return new Spec { top = H("1F4E8C"), sleeves = H("1F4E8C"), shortSleeves = true, pants = black, shorts = true, shoes = white, collar = true, badge = true };
            case Kind.Laboratorio: return new Spec { top = white, sleeves = white, pants = H("6D7480"), shoes = black, coatTails = true, goggles = true, gloves = H("6FA0E0"), badge = true };
            case Kind.Neopreno: return new Spec { top = H("16181C"), sleeves = H("16181C"), pants = H("16181C"), shoes = H("16181C"), wetStripes = true };
            case Kind.Limpieza: return new Spec { top = H("8FC1E3"), sleeves = H("8FC1E3"), shortSleeves = true, pants = H("8FC1E3"), shoes = white, gloves = H("F2D23A") };
            case Kind.Taller: return new Spec { top = H("38506E"), sleeves = H("38506E"), pants = H("38506E"), shoes = black, toolBelt = true, gloves = H("2E2E30") };
            case Kind.Guardia: return new Spec { top = khaki, sleeves = khaki, pants = H("4E5A3A"), shoes = black, hat = Hat.Cap, hatColor = H("4E5A3A"), badge = true };
            case Kind.Textil: return new Spec { top = H("7A5C8E"), sleeves = H("7A5C8E"), pants = H("3C3C44"), shoes = black, apron = true, apronColor = H("C8B08A"), tape = true };
            case Kind.Nuclear: return new Spec { top = white, sleeves = white, pants = white, shoes = H("F2C230"), hat = Hat.Helmet, hatColor = white, dosimeter = true, gloves = H("F2C230") };
            case Kind.Informatica: return new Spec { top = H("2E3440"), sleeves = H("2E3440"), shortSleeves = true, pants = H("3B4252"), shoes = black, collar = true, badge = true };
            case Kind.Asamblea: return new Spec { top = H("D9D2C2"), sleeves = H("D9D2C2"), shortSleeves = false, pants = H("4A4440"), shoes = black, badge = true };
            case Kind.Musica: return new Spec { top = H("1E1E22"), sleeves = H("1E1E22"), shortSleeves = true, pants = H("1E1E22"), shoes = black, badge = true };
            case Kind.Cria: return new Spec { top = khaki, sleeves = khaki, shortSleeves = true, pants = H("5B6B3A"), shoes = H("5A4632"), apron = true, apronColor = H("6B8E5A") };
            case Kind.Energia: return new Spec { top = H("2F6DB5"), sleeves = H("2F6DB5"), pants = H("2F6DB5"), shoes = black, hat = Hat.Helmet, hatColor = H("F2C230"), gloves = H("E8A317"), hiVis = true, toolBelt = true };
            default: return new Spec();
        }
    }

    // ── Insecto-humano: el delantal de su oficio, del tejido de su era ──────────────────────────────────────────

    /// <summary>Viste (o desviste, con None) al insecto-humano: delantal del color del oficio y de la época, y el tocado.</summary>
    public static void DressInsect(GameObject host, Kind k, int year)
    {
        if (k == Kind.None) { CubeInsect.RemoveWorkUniform(host); return; }
        bool old = year < 1800, modern = year >= 1900;
        Color linen = H("D8CDB0"), leather = H("7A5434"), cotton = H("F1F0EA"), strap = old ? H("5A3E26") : H("3A3A3A");
        switch (k)
        {
            case Kind.Cocina: CubeInsect.WearWorkApron(host, old ? linen : cotton, strap, CubeInsect.Hat.None, false, old ? linen : cotton); break;
            case Kind.Sala: CubeInsect.WearWorkApron(host, modern ? H("2B2B2E") : linen, strap, CubeInsect.Hat.None); break;
            case Kind.BataMedica: case Kind.Quirofano: case Kind.Veterinaria:
                CubeInsect.WearWorkApron(host, old ? H("4A3A30") : cotton, strap, year >= 1850 && year < 1950 ? CubeInsect.Hat.Bonnet : CubeInsect.Hat.None, sleeves: !old); break;   // (antes de Lister, 1867, el cirujano llevaba un mandil oscuro)
            case Kind.Huerto: CubeInsect.WearWorkApron(host, old ? leather : H("3E5F8A"), strap, CubeInsect.Hat.Brimmed); break;
            case Kind.Obra: case Kind.Taller: CubeInsect.WearWorkApron(host, leather, strap, modern && k == Kind.Obra ? CubeInsect.Hat.Cap : CubeInsect.Hat.None); break;
            case Kind.Laboratorio: CubeInsect.WearWorkApron(host, cotton, strap, CubeInsect.Hat.None, sleeves: true); break;
            case Kind.Limpieza: CubeInsect.WearWorkApron(host, old ? linen : H("8FC1E3"), strap, CubeInsect.Hat.None); break;
            case Kind.Guardia: CubeInsect.WearWorkApron(host, H("4E5A3A"), strap, CubeInsect.Hat.Cap); break;
            default: CubeInsect.WearWorkApron(host, old ? linen : H("7A8C5A"), strap, CubeInsect.Hat.None); break;
        }
    }
}
