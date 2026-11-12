# Fauna fría de los santuarios marino y aéreo

Berón (2026-11-05): «la fauna la puedes reemplazar por fauna más nativa, de lo que entiendo estamos en un territorio frío en medio de polos y tierra».

Sustituye a la lista tropical y de montaña de [sanctuary-second-lap-and-fear.md](sanctuary-second-lap-and-fear.md) §5. Los números vienen de FishBase, NZ Birds Online, Animal Diversity Web, CCAMLR y artículos; WP = Wikipedia.

## Marino: grupos, sin ciclo de vida

Peces e invertebrados van como `Swarm`, igual que en el santuario 1 (regla A1). Baratos y sin NavMesh, unos 45 individuos.

| Especie | Clave | Medidas | Dónde vive | Notas |
|---|---|---|---|---|
| Pez de hielo, *Chaenocephalus aceratus* | `Icefish` | 72 cm, 3,7 kg | 5–770 m; Bouvet, mar de Scotia | Sin hemoglobina; el macho guarda la puesta |
| Austromerluza antártica, *Dissostichus mawsoni* | `Toothfish` | 2 m, 80 kg | 0–2 200 m, circumantártica | Madura a los 13–17 años; 48 años; caza pez de hielo |
| Calamar colosal, *Mesonychoteuthis hamiltoni* | `ColossalSquid` | Manto 2,5 m, 495 kg | Hasta ≥ 2 200 m | Largo total discutido (4–10 m). Ojos de 27–30 cm y brazos con garfios. **Sigue siendo un grupo de uno, no un `Animal`** |
| Araña de mar gigante, *Colossendeis megalonyx* | `SeaSpider` | 40–50 cm | 3–5 000 m | Come medusas |
| Cangrejo yeti, *Kiwa tyleri* | `YetiCrab` | 4,7–6,7 cm de caparazón | 2 400–2 600 m, fumarolas del arco de Scotia oriental | Cultiva bacterias en el pecho; más de 700/m² |

**Licencias:** el mar del juego llega a 500 m y el cangrejo vive a 2 400 m o más. El modelo de cangrejo y el de araña salen del kril. El calamar y las arañas no comen en el modelo.

## Aéreo: aves frías, `Animal` de verdad

Usan el vuelo del petrel de las nieves (`SnowPetrelLife`, `ColdBirds`). Presupuesto: 10 aves.

| Especie | Clave | Envergadura, masa | Cría | Vida | ¿De verdad fría? |
|---|---|---|---|---|---|
| Págalo polar, *Stercorarius maccormicki* | `PolarSkua` | 1,35 m, 1,3 kg | 1–2 huevos, 26–34 d, pollo 45–59 d | Cría a los 6–9 años; > 25 años | Sí: cría al sur de 60°S y migra al norte |
| Petrel damero, *Daption capense* | `CapePetrel` | 0,85 m, 0,45 kg | 1 huevo, 45 d, pollo 52 d | ≥ 5 años; > 40 años | Sí; la única muy gregaria |
| Albatros viajero, *Diomedea exulans* | `WanderingAlbatross` | 3 m, 9 kg | 1 huevo cada dos años, 11 semanas, pollo 7–10 meses | 10–15 años; 60+ | Subantártico, no antártico |
| Charrán ártico, *Sterna paradisaea* | `ArcticTern` | 0,8 m, 0,1 kg | 1–3 huevos, 20–24 d, pollo 21–24 d | 4 años; 31 años | Cría en el norte y solo visita el hielo del sur por temporadas |

**Descartado por ahora:** el petrel gigante. Su clave llevaría «ant» (regla E7), come sobre todo carroña y no hay carroña en el aire. Se puede añadir con otro nombre.

## Cómo se ponen

- **Marino:** `PlaceMarineColdFauna`; `BuildMarineScene` ya la llama.
- **Aéreo:** `SampleSceneBuilder.PlaceAirColdFauna(root, centro, radio, alturaDelAgua)`. **La escena aérea aún no existe:** nadie lo llama todavía.
- **Cadena alimenticia:** `FoodChain.Isolated` hace el balance de un santuario sin mezclarlo con el 1, y `BanksFor` dice cuántos bancos pedir. El marino no entra: sus grupos se rehacen solos.
- **Escenas de ciclo de vida:** dos más, `CicloVida_Pagalo` y `CicloVida_PetrelDamero`. Comparten la isla del petrel de las nieves. El albatros y el charrán no la llevan: sería repetir el mismo ciclo y gastar más.
- **Sin modelar:** que el págalo robe huevos y pollos, y la migración del charrán.
