/// <summary>
/// Config POST-NATAL por especie (docs/anima-dissolving-animal.md, etapa 5): las secuencias de crianza (etapas con
/// nido/rol paterno/presencia/alimentacion/transiciones). Antes un `_postNatalStages` estatico por clase; ahora DATA,
/// movida VERBATIM (sin reparsear valores). La lee `Animal.PostNatalStages` (que la pasa a `PostNatalManager`).
/// </summary>
public static class PostNatalProfile
{
    /// <summary>Las etapas post-natales de una especie, o null si no tiene.</summary>
    public static PostNatalStage[] Of(string species)
    {
        switch (species)
        {
            case "Bear": return new PostNatalStage[]
            {
        // Stage 0 — Nacimiento en letargo (madre semi-inconsciente)
        new PostNatalStage {
            label = "Nacimiento en letargo", durationDays = 1f,
            nestType = NestType.SnowDen, fatherRole = FatherRole.Absent,
            presencePattern = MotherPresencePattern.Continuous,
            feedingMethod = FeedingMethod.Nurse,
            entryActions = new[] { MotherAction.Clean, MotherAction.Stimulate,
                                   MotherAction.GuideTeat, MotherAction.FirstMilk },
            transitions = new[] { new TransitionCondition
                { kind = TransitionCondition.Kind.TimeElapsed, threshold = 1f } },
        },
        // Stage 1 — Madriguera / madre en letargo profundo (invierno)
        new PostNatalStage {
            label = "Madriguera letargo", durationDays = 60f,
            nestType = NestType.SnowDen, fatherRole = FatherRole.Absent,
            presencePattern = MotherPresencePattern.Continuous,
            feedingMethod = FeedingMethod.Nurse,
            transitions = new[] { new TransitionCondition
                { kind = TransitionCondition.Kind.TimeElapsed, threshold = 60f } },
        },
        // Stage 2 — Primera salida (primavera); madre consume fatReserves
        new PostNatalStage {
            label = "Primera salida", durationDays = 60f,
            nestType = NestType.Burrow, fatherRole = FatherRole.Absent,
            presencePattern = MotherPresencePattern.Continuous,
            feedingMethod = FeedingMethod.Nurse,
            entryActions = new[] { MotherAction.MarkHidingSpot },
            transitions = new[] { new TransitionCondition
                { kind = TransitionCondition.Kind.TimeElapsed, threshold = 60f } },
        },
        // Stage 3 — Aprendizaje activo (pesca/caza observada)
        new PostNatalStage {
            label = "Aprendizaje", durationDays = 120f,
            fatherRole = FatherRole.Absent,
            presencePattern = MotherPresencePattern.FrequentVisits,
            feedingMethod = FeedingMethod.Regurgitate,
            transitions = new[] {
                new TransitionCondition { kind = TransitionCondition.Kind.TimeElapsed, threshold = 120f },
                new TransitionCondition { kind = TransitionCondition.Kind.FirstSolidEaten },
            },
        },
        // Stage 4 — Independencia gradual (madre puede expulsar)
        new PostNatalStage {
            label = "Independencia", durationDays = 180f,
            fatherRole = FatherRole.Absent,
            presencePattern = MotherPresencePattern.MinimalVisits,
            weaningType = WeaningType.Gradual, feedingMethod = FeedingMethod.FoodItem,
            transitions = new[] {
                new TransitionCondition { kind = TransitionCondition.Kind.TimeElapsed, threshold = 180f },
                new TransitionCondition { kind = TransitionCondition.Kind.MotherFatReservesBelow, threshold = 20f },
            },
        },
    };
            case "Bunny": return new PostNatalStage[]
            {
        // Stage 0 — Nacimiento rápido; madre se va casi enseguida
        new PostNatalStage {
            label = "Nacimiento", durationDays = 0.5f,
            nestType = NestType.Burrow, fatherRole = FatherRole.Absent,
            presencePattern = MotherPresencePattern.MinimalVisits,
            feedingMethod = FeedingMethod.Nurse,
            entryActions = new[] { MotherAction.Clean, MotherAction.GuideTeat, MotherAction.FirstMilk },
            transitions = new[] { new TransitionCondition
                { kind = TransitionCondition.Kind.TimeElapsed, threshold = 0.5f } },
        },
        // Stage 1 — Nido solo; visitas nocturnas de 5 min
        new PostNatalStage {
            label = "Nido solo", durationDays = 7f,
            nestType = NestType.Burrow, fatherRole = FatherRole.Absent,
            presencePattern = MotherPresencePattern.MinimalVisits,
            feedingMethod = FeedingMethod.Nurse,
            transitions = new[] { new TransitionCondition
                { kind = TransitionCondition.Kind.TimeElapsed, threshold = 7f } },
        },
        // Stage 2 — Ojos abiertos; primera exploración del nido
        new PostNatalStage {
            label = "Ojos abiertos", durationDays = 7f,
            nestType = NestType.Burrow, fatherRole = FatherRole.Absent,
            presencePattern = MotherPresencePattern.MinimalVisits,
            feedingMethod = FeedingMethod.Nurse,
            transitions = new[] {
                new TransitionCondition { kind = TransitionCondition.Kind.TimeElapsed, threshold = 7f },
                new TransitionCondition { kind = TransitionCondition.Kind.FirstNestExit },
            },
        },
        // Stage 3 — Primeras salidas; empieza con sólidos
        new PostNatalStage {
            label = "Primeras salidas", durationDays = 10f,
            nestType = NestType.Burrow, fatherRole = FatherRole.Absent,
            presencePattern = MotherPresencePattern.MinimalVisits,
            weaningType = WeaningType.Gradual, feedingMethod = FeedingMethod.FoodItem,
            transitions = new[] {
                new TransitionCondition { kind = TransitionCondition.Kind.TimeElapsed, threshold = 10f },
                new TransitionCondition { kind = TransitionCondition.Kind.FirstSolidEaten },
            },
        },
        // Stage 4 — Independencia rápida (~3-4 semanas total)
        new PostNatalStage {
            label = "Independencia", durationDays = 7f,
            fatherRole = FatherRole.Absent,
            presencePattern = MotherPresencePattern.MinimalVisits,
            weaningType = WeaningType.Gradual, feedingMethod = FeedingMethod.FoodItem,
            transitions = new[] { new TransitionCondition
                { kind = TransitionCondition.Kind.TimeElapsed, threshold = 7f } },
        },
    };
            case "Deer": return new PostNatalStage[]
            {
        // Stage 0 — Nacimiento en campo abierto; madre come placenta para eliminar olores
        new PostNatalStage {
            label = "Nacimiento", durationDays = 0.5f,
            nestType = NestType.OpenField, fatherRole = FatherRole.Absent,
            presencePattern = MotherPresencePattern.Continuous,
            feedingMethod = FeedingMethod.Nurse,
            entryActions = new[] { MotherAction.Clean, MotherAction.Stimulate,
                                   MotherAction.GuideTeat, MotherAction.FirstMilk },
            transitions = new[] { new TransitionCondition
                { kind = TransitionCondition.Kind.TimeElapsed, threshold = 0.5f } },
        },
        // Stage 1 — Ocultamiento: cría quieta sola; madre pasta a distancia
        // MarkHidingSpot: madre actualiza HomeOrigin de la cría al dejarla
        new PostNatalStage {
            label = "Ocultamiento", durationDays = 14f,
            nestType = NestType.OpenField, fatherRole = FatherRole.Absent,
            presencePattern = MotherPresencePattern.FrequentVisits,
            feedingMethod = FeedingMethod.Nurse,
            entryActions = new[] { MotherAction.MarkHidingSpot },
            transitions = new[] { new TransitionCondition
                { kind = TransitionCondition.Kind.TimeElapsed, threshold = 14f } },
        },
        // Stage 2 — Sigue a la madre; ya camina bien
        new PostNatalStage {
            label = "Sigue a la madre", durationDays = 60f,
            nestType = NestType.OpenField, fatherRole = FatherRole.Absent,
            presencePattern = MotherPresencePattern.Continuous,
            feedingMethod = FeedingMethod.Nurse,
            transitions = new[] {
                new TransitionCondition { kind = TransitionCondition.Kind.TimeElapsed, threshold = 60f },
                new TransitionCondition { kind = TransitionCondition.Kind.FirstNestExit },
            },
        },
        // Stage 3 — Introducción a pastos; destete gradual
        new PostNatalStage {
            label = "Introducción sólidos", durationDays = 60f,
            fatherRole = FatherRole.Absent,
            presencePattern = MotherPresencePattern.FrequentVisits,
            weaningType = WeaningType.Gradual, feedingMethod = FeedingMethod.FoodItem,
            transitions = new[] {
                new TransitionCondition { kind = TransitionCondition.Kind.TimeElapsed, threshold = 60f },
                new TransitionCondition { kind = TransitionCondition.Kind.FirstSolidEaten },
            },
        },
        // Stage 4 — Independencia gradual
        new PostNatalStage {
            label = "Independencia", durationDays = 90f,
            fatherRole = FatherRole.Absent,
            presencePattern = MotherPresencePattern.MinimalVisits,
            weaningType = WeaningType.Gradual, feedingMethod = FeedingMethod.FoodItem,
            transitions = new[] { new TransitionCondition
                { kind = TransitionCondition.Kind.TimeElapsed, threshold = 90f } },
        },
    };
            case "Fox": return new PostNatalStage[]
            {
        // Stage 0 — Nacimiento en madriguera
        new PostNatalStage {
            label = "Nacimiento", durationDays = 0.5f,
            nestType = NestType.Burrow, fatherRole = FatherRole.Provider,
            presencePattern = MotherPresencePattern.Continuous,
            feedingMethod = FeedingMethod.Nurse,
            entryActions = new[] { MotherAction.Clean, MotherAction.Stimulate,
                                   MotherAction.GuideTeat, MotherAction.FirstMilk },
            transitions = new[] { new TransitionCondition
                { kind = TransitionCondition.Kind.TimeElapsed, threshold = 0.5f } },
        },
        // Stage 1 — Ciegos y sordos, dependientes de la madriguera (~3 semanas)
        new PostNatalStage {
            label = "Dependencia total", durationDays = 21f,
            nestType = NestType.Burrow, fatherRole = FatherRole.Provider,
            presencePattern = MotherPresencePattern.Continuous,
            feedingMethod = FeedingMethod.Nurse,
            transitions = new[] { new TransitionCondition
                { kind = TransitionCondition.Kind.TimeElapsed, threshold = 21f } },
        },
        // Stage 2 — Primeras salidas a la entrada de la madriguera
        new PostNatalStage {
            label = "Exploración de entrada", durationDays = 14f,
            nestType = NestType.Burrow, fatherRole = FatherRole.Provider,
            presencePattern = MotherPresencePattern.FrequentVisits,
            feedingMethod = FeedingMethod.Nurse,
            transitions = new[] {
                new TransitionCondition { kind = TransitionCondition.Kind.TimeElapsed, threshold = 14f },
                new TransitionCondition { kind = TransitionCondition.Kind.FirstNestExit },
            },
        },
        // Stage 3 — Regurgitación; ambos padres proveen comida sólida
        new PostNatalStage {
            label = "Regurgitación", durationDays = 30f,
            nestType = NestType.Burrow, fatherRole = FatherRole.Provider,
            presencePattern = MotherPresencePattern.FrequentVisits,
            feedingMethod = FeedingMethod.Regurgitate,
            transitions = new[] {
                new TransitionCondition { kind = TransitionCondition.Kind.TimeElapsed, threshold = 30f },
                new TransitionCondition { kind = TransitionCondition.Kind.FirstSolidEaten },
            },
        },
        // Stage 4 — Dispersión hacia la independencia (fin de verano/otoño)
        new PostNatalStage {
            label = "Independencia", durationDays = 90f,
            fatherRole = FatherRole.Absent,
            presencePattern = MotherPresencePattern.MinimalVisits,
            weaningType = WeaningType.Gradual, feedingMethod = FeedingMethod.FoodItem,
            transitions = new[] { new TransitionCondition
                { kind = TransitionCondition.Kind.TimeElapsed, threshold = 90f } },
        },
    };
            case "Malamute": return new PostNatalStage[]
            {
        // Stage 0 — Nacimiento; ciegos, sordos, totalmente dependientes
        new PostNatalStage {
            label = "Nacimiento", durationDays = 1f,
            nestType = NestType.Burrow, fatherRole = FatherRole.Absent,
            presencePattern = MotherPresencePattern.Continuous,
            feedingMethod = FeedingMethod.Nurse,
            entryActions = new[] { MotherAction.Clean, MotherAction.Stimulate,
                                   MotherAction.GuideTeat, MotherAction.FirstMilk },
            transitions = new[] { new TransitionCondition
                { kind = TransitionCondition.Kind.TimeElapsed, threshold = 1f } },
        },
        // Stage 1 — Neonatal: ojos y oídos cerrados (~10-14 días reales)
        new PostNatalStage {
            label = "Neonatal", durationDays = 14f,
            nestType = NestType.Burrow, fatherRole = FatherRole.Absent,
            presencePattern = MotherPresencePattern.Continuous,
            feedingMethod = FeedingMethod.Nurse,
            transitions = new[] { new TransitionCondition
                { kind = TransitionCondition.Kind.TimeElapsed, threshold = 14f } },
        },
        // Stage 2 — Transición: ojos/oídos abiertos, primeros pasos fuera del nido
        new PostNatalStage {
            label = "Transición", durationDays = 7f,
            nestType = NestType.Burrow, fatherRole = FatherRole.Absent,
            presencePattern = MotherPresencePattern.Continuous,
            feedingMethod = FeedingMethod.Nurse,
            transitions = new[] {
                new TransitionCondition { kind = TransitionCondition.Kind.TimeElapsed, threshold = 7f },
                new TransitionCondition { kind = TransitionCondition.Kind.FirstNestExit },
            },
        },
        // Stage 3 — Socialización temprana; destete gradual (~3-8 semanas reales)
        new PostNatalStage {
            label = "Socialización temprana", durationDays = 28f,
            nestType = NestType.Burrow, fatherRole = FatherRole.Absent,
            presencePattern = MotherPresencePattern.FrequentVisits,
            weaningType = WeaningType.Gradual, feedingMethod = FeedingMethod.FoodItem,
            transitions = new[] {
                new TransitionCondition { kind = TransitionCondition.Kind.TimeElapsed, threshold = 28f },
                new TransitionCondition { kind = TransitionCondition.Kind.FirstSolidEaten },
            },
        },
        // Stage 4 — Independencia (destete completo ~8 semanas)
        new PostNatalStage {
            label = "Independencia", durationDays = 10f,
            fatherRole = FatherRole.Absent,
            presencePattern = MotherPresencePattern.MinimalVisits,
            weaningType = WeaningType.Gradual, feedingMethod = FeedingMethod.FoodItem,
            transitions = new[] { new TransitionCondition
                { kind = TransitionCondition.Kind.TimeElapsed, threshold = 10f } },
        },
    };
            case "Seal": return new PostNatalStage[]
            {
        // Stage 0 — Nacimiento en playa; vínculo por olfato (crítico)
        new PostNatalStage {
            label = "Nacimiento", durationDays = 1f,
            nestType = NestType.Beach, fatherRole = FatherRole.Absent,
            presencePattern = MotherPresencePattern.Continuous,
            feedingMethod = FeedingMethod.Nurse,
            entryActions = new[] { MotherAction.Clean, MotherAction.Stimulate,
                                   MotherAction.GuideTeat, MotherAction.FirstMilk },
            transitions = new[] { new TransitionCondition
                { kind = TransitionCondition.Kind.TimeElapsed, threshold = 1f } },
        },
        // Stage 1 — Lactancia intensiva; madre casi no se mueve; cría engorda muy rápido.
        // Abandono emergente: cuando fatReserves < 15 (sea por tiempo normal o por interferencia
        // de depredadores que impidieron que la madre acumulara grasa antes del parto).
        new PostNatalStage {
            label = "Lactancia intensiva", durationDays = 12f,
            nestType = NestType.Beach, fatherRole = FatherRole.Absent,
            presencePattern = MotherPresencePattern.ProgrammedAbandonment,
            weaningType = WeaningType.Abrupt, feedingMethod = FeedingMethod.Nurse,
            transitions = new[] {
                new TransitionCondition
                    { kind = TransitionCondition.Kind.MotherFatReservesBelow, threshold = 15f },
            },
        },
        // Stage 2 — Cría sola; aprende a nadar por instinto (no hay más interacción de la madre)
        new PostNatalStage {
            label = "Separación definitiva", durationDays = 1f,
            fatherRole = FatherRole.Absent,
            presencePattern = MotherPresencePattern.ProgrammedAbandonment,
            feedingMethod = FeedingMethod.None,
            transitions = new[] { new TransitionCondition
                { kind = TransitionCondition.Kind.TimeElapsed, threshold = 1f } },
        },
    };
            case "Whale": return new PostNatalStage[]
            {
        // Stage 0 — Nacimiento en mar abierto; la cría nada por sí sola en minutos
        new PostNatalStage {
            label = "Nacimiento", durationDays = 1f,
            nestType = NestType.OpenWater, fatherRole = FatherRole.Absent,
            presencePattern = MotherPresencePattern.Continuous,
            feedingMethod = FeedingMethod.Nurse,
            entryActions = new[] { MotherAction.Clean, MotherAction.Stimulate,
                                   MotherAction.GuideTeat, MotherAction.FirstMilk },
            transitions = new[] { new TransitionCondition
                { kind = TransitionCondition.Kind.TimeElapsed, threshold = 1f } },
        },
        // Stage 1 — Lactancia extendida junto a la madre (~20 meses reales)
        new PostNatalStage {
            label = "Lactancia extendida", durationDays = 600f,
            nestType = NestType.OpenWater, fatherRole = FatherRole.Absent,
            presencePattern = MotherPresencePattern.Continuous,
            feedingMethod = FeedingMethod.Nurse,
            transitions = new[] { new TransitionCondition
                { kind = TransitionCondition.Kind.TimeElapsed, threshold = 600f } },
        },
        // Stage 2 — Destete gradual; aprende a pescar junto a la madre
        new PostNatalStage {
            label = "Destete gradual", durationDays = 365f,
            fatherRole = FatherRole.Absent,
            presencePattern = MotherPresencePattern.FrequentVisits,
            weaningType = WeaningType.Gradual, feedingMethod = FeedingMethod.FoodItem,
            transitions = new[] {
                new TransitionCondition { kind = TransitionCondition.Kind.TimeElapsed, threshold = 365f },
                new TransitionCondition { kind = TransitionCondition.Kind.FirstSolidEaten },
            },
        },
        // Stage 3 — Independencia gradual dentro de la manada/pod
        new PostNatalStage {
            label = "Independencia", durationDays = 365f,
            fatherRole = FatherRole.Absent,
            presencePattern = MotherPresencePattern.MinimalVisits,
            weaningType = WeaningType.Gradual, feedingMethod = FeedingMethod.FoodItem,
            transitions = new[] { new TransitionCondition
                { kind = TransitionCondition.Kind.TimeElapsed, threshold = 365f } },
        },
    };
            case "Wolf": return new PostNatalStage[]
            {
        // Stage 0 — Nacimiento: secuencia fija
        new PostNatalStage {
            label = "Nacimiento", durationDays = 1f,
            nestType = NestType.Burrow, fatherRole = FatherRole.Provider,
            presencePattern = MotherPresencePattern.Continuous,
            feedingMethod = FeedingMethod.Nurse,
            entryActions = new[] { MotherAction.Clean, MotherAction.Stimulate,
                                   MotherAction.GuideTeat, MotherAction.FirstMilk },
            transitions = new[] { new TransitionCondition
                { kind = TransitionCondition.Kind.TimeElapsed, threshold = 1f } },
        },
        // Stage 1 — Dependencia total (ojos cerrados)
        new PostNatalStage {
            label = "Dependencia total", durationDays = 14f,
            nestType = NestType.Burrow, fatherRole = FatherRole.Provider,
            presencePattern = MotherPresencePattern.Continuous,
            feedingMethod = FeedingMethod.Nurse,
            transitions = new[] { new TransitionCondition
                { kind = TransitionCondition.Kind.TimeElapsed, threshold = 14f } },
        },
        // Stage 2 — Exploración temprana y juego entre camada
        new PostNatalStage {
            label = "Exploración temprana", durationDays = 30f,
            nestType = NestType.Burrow, fatherRole = FatherRole.Provider,
            presencePattern = MotherPresencePattern.Continuous,
            feedingMethod = FeedingMethod.Nurse,
            transitions = new[] {
                new TransitionCondition { kind = TransitionCondition.Kind.TimeElapsed, threshold = 30f },
                new TransitionCondition { kind = TransitionCondition.Kind.FirstNestExit },
            },
        },
        // Stage 3 — Introducción a regurgitación
        new PostNatalStage {
            label = "Regurgitación", durationDays = 45f,
            nestType = NestType.Burrow, fatherRole = FatherRole.Provider,
            presencePattern = MotherPresencePattern.FrequentVisits,
            feedingMethod = FeedingMethod.Regurgitate,
            transitions = new[] {
                new TransitionCondition { kind = TransitionCondition.Kind.TimeElapsed, threshold = 45f },
                new TransitionCondition { kind = TransitionCondition.Kind.FirstSolidEaten },
            },
        },
        // Stage 4 — Integración a la manada / caza observada
        new PostNatalStage {
            label = "Integración manada", durationDays = 90f,
            nestType = NestType.Burrow, fatherRole = FatherRole.ActiveCaregiver,
            presencePattern = MotherPresencePattern.FrequentVisits,
            feedingMethod = FeedingMethod.FoodItem,
            transitions = new[] { new TransitionCondition
                { kind = TransitionCondition.Kind.TimeElapsed, threshold = 90f } },
        },
        // Stage 5 — Independencia gradual
        new PostNatalStage {
            label = "Independencia", durationDays = 120f,
            fatherRole = FatherRole.Absent,
            presencePattern = MotherPresencePattern.MinimalVisits,
            weaningType = WeaningType.Gradual, feedingMethod = FeedingMethod.FoodItem,
            transitions = new[] { new TransitionCondition
                { kind = TransitionCondition.Kind.TimeElapsed, threshold = 120f } },
        },
    };
            default: return null;
        }
    }
}
