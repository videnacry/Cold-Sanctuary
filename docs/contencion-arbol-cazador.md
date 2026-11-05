# El árbol cazador, contenido (2026-11-05)

Berón: «el árbol cazador queda solo en un piso específico de investigación de modo que conserva todas sus características y se implementa normas de seguridad».

Código: `World/HunterCell.cs`, `SampleSceneBuilder.BuildContainmentLab`. Regla S16. Prueba: grupo 91.

## Dónde

**El Laboratorio de contención, en lo alto de Producción** (nivel 3, 108 m, en la isla). Sube un nivel a la torre porque su piso libre es el garaje del submarino. Ciencias está llena. Queda lejos de la Residencia (~130 m) y de los demás.

## Lo que conserva

Un solo árbol: **señuelo de 70 m, sopor letal de 12 m, cualquier presa**, incluidas personas, y crece hasta ×1,5.

- **El alcance es el entero.** En el bosque salía de su altura: los mayores, de ~52 m, llegaban a ~62 m y ~11 m; el ~70 y ~12 de un doc anterior era de un árbol de 60 m, el de la residencia. Se toma ese, el mayor.
- **El cuerpo es menor**: 13 m en vez de 29–52 m. Uno grande no cabe en un piso, con 22 m entre niveles. Con ×1,5 llega a unos 20 m, y la celda mide 21.
- **La celda lo aguanta** porque filtra por dentro: el radio no importa mientras esté sellada. En una brecha actúa entero: los 12 m letales llegan a la antesala, así que el puesto de trabajo está a más de 12 m del tronco. El señuelo alcanza los pisos de Producción, no la Residencia.

**Lo que se pierde sin panales** (un enjambre no se puede contener): las abejas y arañas que vivían en él, la miel de los cadáveres cercanos con su bucle de nutrientes, y las mudanzas de colmena a sus ramas. Su caza y su crecimiento no dependen de ellos. Los panales siguen en los abedules de la isla.

## Las normas, y de dónde salen

| Norma | Origen | Fuente |
|---|---|---|
| Acceso restringido y manual de prácticas | BL3-P | [Guías NIH, Apéndice P](https://webarchive.library.unt.edu/eot2008/20090131001310/http:/oba.od.nih.gov/oba/rac/guidelines_02/APPENDIX_P.htm) |
| Descontaminar lo que sale | BL3-P | La misma |
| Control de especies no deseadas: sin panales | BL3-P | La misma |
| Nadie comparte espacio con él | Contacto protegido | [Cheyenne Mountain Zoo](https://cmzoo.org/news/archive/tag/zoo-information/page/7) |
| Doble puerta | Zoos | [Cleveland Zoo](https://www.clevelandzoosociety.org/Z/2020/03/16/ask-the-expert-how-do-the-keepers-get-into-the-habitats-without-the-animals-coming-out) |
| Simulacros y código de escape | Zoos | [ASIS](https://www.asisonline.org/link/db5bf6ea36204592be7c784764586bf3.aspx) |

- **[verificar]**: la presión negativa no salió en las búsquedas, y los dos últimos datos de zoos son de fuentes indirectas. El simulacro semanal es elección mía: el dato que encontré es de cuatro al año.
- Un «árbol cazador» no existe, así que las normas se han adaptado.

## La celda

- De cristal, 18 × 16 × 21 m, con tapa: excepción de S6 por sala que debe aislarse.
- **Sellada**: su señuelo no sale y su sopor solo alcanza lo de dentro.
- Antesala: consola, filtros, portillo, guantes, ducha y equipo. Todo el trabajo es desde fuera.

## Misiones

Del sitio «Laboratorio de contención». No cuentan en el catálogo fijo.

| Misión | Efecto |
|---|---|
| `cuar_ronda` (obligatoria, 1 h) | Reinicia la cuenta de días sin ronda |
| `cuar_racion` | Crece como con una presa |
| `cuar_muestra` | Poda: lo mantiene dentro de la celda |
| `cuar_simulacro` (semanal) | Registro |
| `cuar_recaptura` y `_2` | Solo durante una brecha |

## La emergencia: código verde

- **Empieza** tras más de 3 días sin ronda, o por un fallo de sello, cada ~60 días desde el día 30.
- **Mientras dura**: el árbol alcanza a cualquiera, sin barrera. La pantalla de la sala de guardia se pone roja, y solo queda la recaptura.
- **Acaba** cuando firman **dos personas**, o al día siguiente, porque las compuertas cierran solas: la emergencia no se queda abierta.

## Riesgos

- Sin probar en vivo.
- Nadie huye solo: durante la brecha, quien pase cerca puede enfermar. No hay evacuación del piso programada.
- La ración no tiene hambre detrás: el árbol no se debilita si no come.
- La rotación puede tardar en mandar a alguien, porque es un sitio lejano y con misiones nuevas.
