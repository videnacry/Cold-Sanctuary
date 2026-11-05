# Lugares de entrenamiento de cada época (microcosmos, 2026-11-04)

Berón pidió esto:

> «¿en ninguna época de microcosmos hay gimnasio? había escuchado que desde la antigua Grecia se entrenaba físicamente … quizás las investigaciones de las ciudades/pueblos de las historias del microcosmos no dan un mapa completo de las estructuras que existían? en ese caso se deben llenar con mapas actuales para cumplir con las estructuras de ocio/entrenamiento/trabajo/recuperación, se usarían mapas actuales solo para buscar posiciones pero se investigaría la época/lugar para recrear edificaciones, muebles y maquinaria nativos»

Este documento recoge lo que se investigó de cada época y lugar: cómo era el sitio donde se entrenaba y con qué aparatos. Las reglas que obligan a cumplirlo están en otros documentos:

- La regla está en [microcosmos-reglas-edificacion.md](microcosmos-reglas-edificacion.md) (regla 16) y en el manual (M35).
- Las posiciones que salen de un mapa de hoy, pueblo por pueblo, están en [microcosmos-pueblos-mapas.md](microcosmos-pueblos-mapas.md).
- Las etiquetas de actividad están en [microcosmos-actividades.md](microcosmos-actividades.md).
- Cómo se usan desde los planes de entrenamiento está en [gimnasio-maquinas.md](gimnasio-maquinas.md), en «Microcosmos: lugares de entrenar por época».

## Cómo está hecho en el código

Cada pieza del sistema está en este sitio:

| Pieza | Dónde |
|---|---|
| Categoría de actividad | `AreaFunctionSite.Entrenamiento` («entrenamiento»). Se aplica a las plantillas «Centro de entrenamiento» y «Dojo». Antes contaban como ocio. |
| Clase estable de cada sitio | `AreaFunctionSite.training`, que sale de `MicroStyles.TrainingKind(estilo, año)`. Es la etiqueta que leen los planes de entrenamiento. |
| Nombre de época | `MicroStyles.CivicName(Civic.Entrenamiento, …)` |
| Aparatos de cada clase | `CivicBuilder.TrainingKit(clase)`. Las piezas se construyen con `MicroFurniture.Training`. |
| Funciones del registro | `ent_entrenar`, `ent_ensenar`, `ent_lucha`, `ent_correr`, `ent_pesos`, `ent_golpear`, `ent_aparatos`, `ent_armas`, `ent_tiro` y `ent_pelota`. Cada una indica qué puestos (claves de mueble) la sirven. |

## Las clases (etiqueta estable → época y lugar → aparatos)

| `training` | Dónde y cuándo | Nombre en el pueblo | Aparatos (claves) | Por qué así (fuentes) |
|---|---|---|---|---|
| `juegos_heroicos` | Egeo antes de −700 (Egina, −1250) | Dromos de los juegos (carrera, lucha y pugilato) | pista_carreras, foso_lucha, armero (discos de piedra y jabalinas) | En la Grecia heroica aún no había gimnasios. Se competía en juegos: los fúnebres de Patroclo tenían carrera, lucha, pugilato, disco (*solos*), jabalina y arco (Ilíada XXIII). |
| `gimnasio_griego` | Egeo desde el s. VI a. C. (Crotona, Alejandría) | Gimnasio (palestra y xisto) | pista_carreras (xisto), foso_lucha (skamma), halteres, saco (kórykos), armero (discos y jabalinas) | Vitruvio describe la palestra y el xisto con el *korykeion*, el *konisterion* y el *elaiothesion* (De architectura V.11). Los halteres eran pesos para el salto (Pausanias V.26–27; los de piedra y plomo de Olimpia). En Crotona entrenaba Milón, seis veces vencedor olímpico en lucha. |
| `palestra_romana` | Roma | Palestra de las termas | foso_lucha, halteres, saco, balon (pila, follis, harpastum) | Las termas tenían su palestra (las de Caracalla). Séneca se queja del ruido de los que levantan pesas bajo su casa (Ep. 56). |
| `patio_lucha_egipcio` | Egipto faraónico (Menfis) | Patio de lucha (como en Beni Hasan) | foso_lucha, armero (bastones de lucha con palo), piedras (sacos de arena de levantar) | Las tumbas del Reino Medio en Beni Hasan pintan cientos de llaves de lucha y el levantamiento de sacos. |
| `patio_lucha` | Mesopotamia (Uruk) | Patio de lucha (lucha de cinturón) | foso_lucha, piedras | Hay una estatuilla de bronce de luchadores de cinturón en Jafayah (~2600 a. C.), y Gilgamesh lucha con Enkidu. |
| `akhara` | Sur de Asia antigua y moderna (Kapilavastu, Patanjali, Velliangiri) | Akhāṛā (patio de lucha) | foso_lucha (tierra con aceite y cúrcuma, el Hanumān de la esquina), mazas (gada y mugdar), piedras (nal) | En el Lalitavistara, Siddhārtha compite en lucha y en arco en Kapilavastu. La akhāṛā de tierra, con la gada, el mugdar y el nal, es la tradición del kushti. |
| `furusiyya` | Islam abasí (Bagdad, ~820) | Maydān de furusiyya (caballos, arco y lucha) | pista_carreras (pista del maydān), diana (qabaq), armero (lanzas y sables), foso_lucha (koshti) | En la Bagdad abasí ya se escribían manuales de furusiyya (Ibn Akhī Ḥizām, s. IX). Se entrenaba en el maydān con caballo, arco y lanza. La **zurkhaneh** como institución está documentada más tarde (época safávida), así que no se pone en el 820. Lo que había era la furusiyya y la lucha persa (*koshti*). |
| `tekke_guresh` | Otomano (Escútari, 1854) | Tekke de luchadores (yağlı güreş) y campo de tiro | foso_lucha (lucha con aceite), mazas, diana | La lucha con aceite tenía sus tekkes de luchadores; Kırkpınar es la más famosa. Estambul tenía su campo de tiro, el Okmeydanı (desde 1453). |
| `juego_pelota` | Maya clásico (Palenque) | Juego de pelota | pista (cancha), armario (yugos), balon (pelota de hule) | Palenque tiene su juego de pelota. |
| `wuguan` | China tradicional y Hong Kong (Bruce Lee, la aldea de la montaña) | Escuela de artes marciales (wuguan) | tatami, muneco (mu ren zhuang), saco (sha bao), piedras (candados de piedra, *shi suo*), armero | La escuela de Yip Man en el Hong Kong de los años 50, con el muñeco de madera del Wing Chun, el saco de arena y los candados de piedra tradicionales. |
| `dojo` | Japón Edo y Meiji | Dojo | tatami, makiwara, armero (bokken y shinai), diana (mato de kyūdō) | |
| `sala_armas` | Europa medieval | Sala de armas | pista_esgrima, armero (espadas y rodelas), muneco (poste de armas) | |
| `prado_juegos` | Inglaterra rural, 1500–1836 (Woolsthorpe, Berkeley) | Prado de juegos (lucha, garrote y carreras) | foso_lucha (ruedo de lucha), armero (garrotes: quarterstaff y singlestick), pista_carreras, balon (camp-ball) | Un pueblo de Lincolnshire o de Gloucestershire no tenía sala de esgrima. Se entrenaba en el prado. Los Cotswold Olimpick Games de Robert Dover (desde 1612, Chipping Campden, Gloucestershire) tenían lucha, *singlestick*, *shin-kicking* y carreras. El camp-ball era el fútbol de campo del este de Inglaterra. |
| `garrote_coleo` | Hispanoamérica colonial y republicana (Angostura, 1819) | Patio del juego de garrote y manga de coleo | pista_carreras (manga de coleo), armero (garrotes) | El juego de garrote venezolano es una esgrima de palo de origen colonial, y el coleo de toros es la faena llanera. Ninguno de los dos necesitaba un edificio. |
| `turnplatz_schwingen` | Alpes (Sils Maria, 1881) | Turnplatz y ruedo de Schwingen | foso_lucha (serrín), piedras (Unspunnen), barra_fija, paralelas | El Schwingen (lucha suiza en un ruedo de serrín) y el lanzamiento de la piedra de Unspunnen (fiesta de 1805). La Federación Suiza de Gimnasia es de 1832. |
| `turnhalle` | Europa urbana, 1837–1959; Norteamérica, 1840–1919 (París 1898, Berna, la Planta Atómica, Varsovia, Dayton, Colorado Springs) | Gimnasio (sala de gimnasia), y en Norteamérica Gimnasio de la YMCA | barra_fija, paralelas, potro, anillas, mazas (mazas indias) | Jahn abrió el *Turnplatz* de la Hasenheide (Berlín) en 1811 y de ahí salieron las Turnhallen. Las mazas indias eran victorianas. Hacia 1900 las YMCA de Norteamérica ya tenían gimnasio. |
| `hebert` | Francia rural desde 1910 (Le Chambon, 1942) | Terreno de deporte (método natural de Hébert) | pista_carreras (parcours), barra_fija, potro (plinto), balon | Georges Hébert creó el método natural en el colegio de Reims (1913). En los años 30 y 40 lo usaban los colegios y los scouts franceses. |
| `futbol_tregua` | El frente, 1914–1918 (la Tregua) | Partido de la tregua (fútbol en la tierra de nadie) | campo (porterías de gorras y cascos), balon | En la Navidad de 1914 hubo partidos en la tierra de nadie según cartas de los dos lados, en Saint-Yvon y en otros puntos del frente. Los historiadores discuten cuántos fueron y dónde. |
| `club_deportivo` | Soviético, 1960–1991 (Chernóbil) | Club deportivo (normas GTO) | pesas, barra_fija, pista_carreras, saco | Las normas del complejo GTO son de 1931. Prípiat tenía su complejo deportivo. |
| `estadio` | Sahel, desde 1960 (Uagadugú) | Estadio y terreno de deporte | campo, pista_carreras, balon | Sankara promovió el *sport de masse* en los años 80. El Stade du 4-Août está en Uagadugú. |
| `gimnasio_moderno` | Occidente moderno y Norteamérica desde 1920 (Pasadena, Hawthorne, Albuquerque, Omaha, Prairie View) | Gimnasio | pesas, saco, barra_fija, balon (y canasta) | |

`sala_esgrima` (floretes y espadas) existe en la lista de aparatos, pero ningún pueblo la usa todavía.

## La tanda 3 (2026-11-05, [microcosmos-historias-3.md](microcosmos-historias-3.md))

| Clase | Época y lugar | Nombre | Aparatos | Por qué |
|---|---|---|---|---|
| `palin` | Mapuche | Paliwe (cancha de palín) | campo (paliwe), armero (wiño), balon (pali) | El palín, con su palo curvo y su bola de cuero, se juega por equipos en una cancha larga; es juego y rito (Revista Patrimonio; Univ. de Chile; López von Vriessen). |
| `warachikuy` | Inca | Campo del warachikuy | pista_carreras, diana (honda), armero | Las pruebas de los jóvenes (carrera al cerro Huanacauri, honda) y los relevos de chasquis (Markham; cronistas; efdeportes). |
| `tewaarathon` | Haudenosaunee | Campo de tewaarathon | campo, armero (palos de nogal), balon | El lacrosse, el «juego del Creador» (Two Row Times; Vennum 1994). |
| `pelota_nacional` | Andes republicanos | Cancha de pelota nacional | campo, armario (tablas), balon | La pelota de tabla ecuatoriana; su antigüedad es dudosa (de la fuente: [verificar] que se jugaba en Quito en 1908). |

Reutilizan: `gimnasio_griego` (Alejandría de Cleopatra: el Gimnasio de Estrabón), `sala_esgrima` (Viena y Leiden, cuya Universidad tuvo escuela de esgrima), `estadio` (San Pablo: el fútbol que
cuentan los diarios), `gimnasio_moderno` (Langley, Madrid, la Feria) y `turnhalle` (Gettysburg, en un patio de gimnasia propio: los Turnverein llegaron a EE. UU. en 1848).

## Qué falta (para la investigación siguiente)

- Las posiciones marcadas como «posición aproximada» en [microcosmos-pueblos-mapas.md](microcosmos-pueblos-mapas.md) hay que afinarlas con el mapa (OpenStreetMap) cuando se haga el mapa a mano de cada historia (manual M9).
- Los planes de entrenamiento de los insectos-humanos por época los escribe el hilo «entreno». Usan la clase `training` y las funciones `ent_*`; ver [gimnasio-maquinas.md](gimnasio-maquinas.md), en «Microcosmos: lugares de entrenar por época».
