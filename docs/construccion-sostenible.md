# Construcción sostenible: materiales y tiempos

El santuario es autosustentable: lo que se construye sale de plantas cultivadas aquí y, cuando es de animales, sin hacerles daño.

Código:
- `World/MaterialStore.cs`: almacén de Construcción.
- `World/ConstructionProject.cs`: obras.
- `Progression/AreaWork.cs`: misiones.
- `Farming/GardenPlot.cs`: el `Bambusal`.

## Materiales (decisión por defecto)

| Material | De dónde sale | Proceso y tiempo real | En el juego |
|---|---|---|---|
| **Guadua** (bambú estructural, *Guadua angustifolia*) | Bambusal | Cada mata es un rizoma que echa cañas nuevas cada año. La caña crece a su altura en unos meses y madura para construir a los 3–5 años. Se corta la caña madura sobre el primer nudo; el rizoma rebrota. Corta sostenible: cerca de un tercio de las maduras al año. Se **cura** (avinagrado en pie unas semanas), se **trata** con bórax por inmersión contra insectos y hongos, y se seca. | Misión «Cortar guadua» (2 h): 6 cañas que tardan **3 días** en estar tratadas. El bambusal rebrota 1,5 cañas/día. |
| **Bahareque** | Tierra del sitio y paja de trigo y arroz (los huertos) | Pared tradicional andina: esterilla de guadua rellena y revocada con barro y paja. | Misión «Preparar barro con paja» (1 h). |
| **Paneles de micelio** | Paja y cañamiza inoculadas con micelio de hongo | Crecen en moldes 5–7 días; se secan con calor para parar el crecimiento. Aislamiento ligero, compostable. | Misión en el laboratorio (1 h); listos en **5 días**. |
| **Cuerda de cáñamo** | Borde de cáñamo del bambusal | Fibra del tallo; ata las uniones de guadua. | Misión en el textil (1 h). |
| **Acabado** | Aceite de linaza (del lino de los huertos) y cera de abeja | La cera es la que sobra en el panal; nunca se vacía la colmena. | Protege la guadua. |

Las uniones de guadua son de boca de pez con pasadores y cuerda. La guadua no toca el suelo: va sobre un zócalo para que no se pudra.

## Un día de obra

- Gasta **4 guaduas tratadas, 2 de barro con paja, 2 paneles de micelio y 1 cuerda**.
- La misión «Llevar el material» lo saca del almacén. Si falta material, la obra no avanza ese día y el log lo dice.
- Con la misión «Trabajar la obra» las paredes suben y el cartel cuenta los días.
- Duración de las obras: ballet 5 días, dojo 6 y calistenia 4. Son pabellones pequeños de guadua y bahareque; en la vida real, con una cuadrilla de 4, serían unas 2–4 semanas, comprimidas al calendario del juego.
- El catálogo del día añade misiones para reponer lo que falte. Siempre deja dos días de obra en reserva por cada obra abierta.

## Pendiente

- Visual del almacén (pilas que crecen y bajan).
- Paja como subproducto real de la cosecha de trigo y arroz.
- Teja de guadua o de micelio para los tejados.
