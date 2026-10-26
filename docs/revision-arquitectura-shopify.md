# Revisión de arquitectura: qué aprender de la app Shopify «mahou-location-control»

> Revisión de solo lectura, 2026-10-26. La app se leyó desde `Downloads/mahou-location-control-main.zip`
> (descomprimida en un directorio temporal; no se ejecutó, instaló ni compiló nada). Las rutas de la app
> son relativas a su raíz. Las de Cold-Sanctuary, relativas a este repo.

## 1. La app Shopify: estructura

**Monorepo yarn 1.22 con workspaces** (`package.json` → `extensions/*`, `packages/*`, `web`), y cada pieza
con su propio `package.json`, sus dependencias y sus tests.

| Capa | Dónde | Qué hace |
|---|---|---|
| Entrada HTTP | `web/index.ts` (129 líneas) | Arranque de Express, rutas, crons, gestor de errores final |
| Rutas → controladores | `web/routes/*.ts` → `web/controllers/*.ts` | Rutas finas; el controlador traduce HTTP ↔ caso de uso |
| Casos de uso | `web/useCases/*.ts` (`CheckOrderAfterCreated`, `ProcessMahouOrder`…) | Una clase por acción de negocio, con `exec()` |
| Servicios | `web/services/*.ts` | Integraciones (Oracle, Google Sheets, email, PDF) |
| Datos | `web/models/*` (Mongoose, marcado *deprecated*) y `web/modules/*` («v2») | Dos generaciones que conviven |
| Webhooks | `web/webhooks/procesarWebhook.ts` + `orders.ts`/`locations.ts` | HMAC → tabla de handlers por topic → idempotencia por índice único |
| Paquete reutilizable | `packages/shopify-preorder-selling-plan/src/{domain,application,infra}` | Arquitectura limpia de verdad: entidades, objetos de valor, interfaces de repositorio y gateway, casos de uso, adaptadores Mongo/GraphQL |
| UI admin | `web/frontend/pages/**.jsx` (React 17 + Polaris, sin tipos) | Enrutado por ficheros |
| Extensiones | `extensions/<nombre>/` (15) | Checkout UI (Preact), Functions de validación, Cart Transform, Theme blocks |
| Núcleo común | `extensions/validation-core/src/index.ts` | Lo que las cuatro validaciones no pueden divergir (pasos, idioma, `Decision<Permite,Bloquea>`, huella) |

**Dirección de dependencias.** En el paquete de preventas está garantizada: `domain` no importa nada;
`application` depende de interfaces (`IPreorderSellingPlanRepository`, `IShopifyPreorderSellingPlanGateway`);
`infra` las implementa, y el cliente GraphQL es inyectable (`infra/shopify/IShopifyGraphQLClient.ts`), sin
depender del SDK de Shopify del anfitrión. En las Functions, el punto de entrada
(`stock-validation/src/cart_validations_generate_run.ts`) solo mapea el input generado a un `Contexto` y
llama a `decidir()` en `decision.ts`, que es pura. En `web/` la dirección es más blanda: los casos de uso
importan modelos y `core` directamente.

**Configuración.** `shopify.app.toml` por entorno, `shopify.extension.toml` por extensión (con `api_version`),
`.nvmrc` y `packageManager` fijan Node y yarn, lockfiles versionados y `--frozen-lockfile` en CI,
`tsconfig.strict.json` canónico que se extiende, configuración de negocio en metafields editables desde el
admin (`custom.fianza_config_v2`, `web/frontend/pages/fianza`) y ajustes del editor de checkout
(`stock_timeout_seconds`). Textos por idioma en `locales/*.json`.

**Versiones de la API de Shopify.** Están fijadas en un solo sitio por superficie: `web/core/shopify.ts`
(`ApiVersion.October26`; REST fijado a `2026-07` con un comentario que explica por qué), `api_version` en cada
`.toml`, y la de webhooks en el `.toml` de la app. Las fixtures de webhook llevan la versión en el nombre
(`orders-create.2026-10.qa-alta.json`), y el README de fixtures documenta qué cambió entre versiones.
`scripts/check-docs.sh` falla si la tabla de `ARCHITECTURE.md` no coincide con el `.toml`. Cada migración
tiene su documento con capturas de antes y después (`docs/migration/fan-5366-banner-reserve.md`).

**Errores.** Errores de dominio tipados con `code` (`packages/.../domain/errors.ts`), que el controlador
traduce a HTTP (`SellingPlanController.ts`, sync fallido → 502 y registro en la colección `integrations` vía
`IntegrationLogger`). Las decisiones *fail-open* o *fail-closed* son explícitas y tienen fixture propia
(`checkout-error-backend` deja pasar; `checkout-sin-metafield` bloquea a propósito, comentado en
`fixtures.test.ts`). Hay un gestor de errores final de Express y el arranque comprueba el `listen` y los
índices (`[CRITICAL]`).

**Logging.** `console.*` con prefijo de área (`[webhooks]`, `[carrierService]`, `[Mahou Sync]`) y `[CRITICAL]`
para fallos de arranque; warn para lo ignorable y error para lo que falla. Sin logger estructurado (282
llamadas a `console`). Los errores de integración se persisten en Mongo, no solo en el log.

**Tipos.** TypeScript estricto (`noUncheckedIndexedAccess`, `exactOptionalPropertyTypes`…). Tipos generados
del esquema GraphQL (`codegen` → `generated/api.ts`). Uniones discriminadas para los resultados
(`Decision`). El frontend JSX queda sin tipar.

**Nombres.** Dominio en español y coherente (`decidir`, `motivo`, `huella`, `PASOS_PAGO`); términos de
framework en inglés. Casos de uso en PascalCase con nombre de verbo y fichero = clase. Fixtures con el ID
del caso (`CK-37-real-barril.json`).

## 2. La app Shopify: pruebas

| Tipo | Cómo | Evidencia |
|---|---|---|
| Unitarias | Vitest en cada workspace (77 ficheros `*.test.*`) | `extensions/*/src/__tests__/decision.test.ts`, `web/utils/__tests__/*` |
| Dominio + dobles | Factorías y mocks tipados (`Mocked<IRepo>`) | `packages/.../__tests__/test-utils/{factories,mocks}.ts` |
| Integración | Mongo en memoria (`mongodb-memory-server`) | `MongoPreorderSellingPlanRepository.integration.test.ts` |
| Contrato con la plataforma | Las Functions se compilan a wasm y se ejecutan con el *runner* oficial contra cada fixture; el esquema valida input y output | `extensions/*-validation/tests/default.test.js` |
| Flujos de usuario | **No hay Playwright ni Cypress.** Los flujos se emulan con **148 fixtures JSON capturadas de checkouts y pedidos reales** (`CK-37-real-*.json`, `orders-create.*`), un script que crea pedidos de prueba (`scripts/testing/create-test-draft.sh`) y un catálogo de QA manual con IDs y capturas | `docs/qa/casos-de-uso.md`, `web/webhooks/__tests__/fixtures/README.md` |
| Guardas sobre las propias pruebas | El test falla si hay en disco una fixture no declarada; otro test busca DNI, teléfonos o emails en las fixtures (anonimización) | `stock-validation/src/__tests__/fixtures.test.ts`, `web/webhooks/__tests__/fixturesSinDatosPersonales.test.ts` |
| CI | `.github/workflows/tests.yml`: en PR y en push a la integradora; `permissions: read`, `concurrency` con cancelación, `timeout`, `TZ: UTC`; pasos `test` → `typecheck` → `check-docs` (cada uno con `!cancelled()` para que un fallo no oculte otro) y un job que construye la imagen Docker entera | `.github/workflows/tests.yml` |
| Puerta | `scripts/run-tests.sh` y `typecheck.sh` **declaran el conjunto esperado**: si desaparece un workspace, aparece uno sin declarar o alguno pierde su script de test, se pone en rojo (no se da por bueno con menos tests). Las exenciones se listan como deuda medida | `scripts/run-tests.sh`, `scripts/typecheck.sh` |
| Lint / formato / pre-commit | **No hay** ESLint, config de Prettier (solo la dependencia en `web/package.json`), Husky ni lint-staged | — |

## 3. Qué la hace limpia, sostenible y extensible (y qué no)

**Prácticas que se pueden copiar**
1. **Decisión pura y adaptador fino.** `decision.ts` no conoce Shopify, y el punto de entrada solo mapea datos.
   Se prueba sin plataforma.
2. **Núcleo compartido para lo que no puede divergir** (`validation-core`), en vez de copiarlo.
3. **Puertos y adaptadores donde hay reutilización real** (paquete de preventas: interfaces en `domain/`).
4. **Conjunto de pruebas declarado y verificado**: perder un test se nota.
5. **Fixtures reales con nombre de caso** (`<ID>-real-<desc>`) y el catálogo `casos-de-uso.md` que une caso →
   test → fixture → captura. Un ID nunca se reutiliza.
6. **La documentación se comprueba en CI** (`check-docs.sh`).
7. **Comentarios de por qué**, no de qué: cada guarda o número mágico explica el incidente que la motivó
   (`MAXIMO_LINEAS_HUELLA`, `nohoist`).
8. **Entorno fijado** (`.nvmrc`, `packageManager`, lockfiles, `TZ`).
9. **Errores tipados con código**, y decisiones *fail-open* o *fail-closed* explícitas y probadas.

**Lo que no es tan limpio** (para no copiarlo)
- En el zip van `.env`, `.env.location-control-prod`, `web/.env`, `pass-dev.txt`, `pass-prod.txt` y `cert.pem`
  (no se abrieron). El propio `.gitignore` admite que algunos `.env` siguen en el repo.
- Capas duplicadas a medio migrar: `models/` (*deprecated*) junto a `modules/`, `routes/` junto a `router-v2/`.
- No hay lint ni formateador ni *hooks*, el log no es estructurado y conviven tres majors de Vitest.
- `web/services/sendEmail.ts` tiene 611 líneas y el frontend no tiene tipos. Nueve workspaces no hacen
  *typecheck*; están listados como deuda.

## 4. Cold-Sanctuary hoy (lo relevante para la comparación)

| Aspecto | Estado | Evidencia |
|---|---|---|
| Tamaño | 609 tipos, ~38 k líneas de C# | `docs/CODEMAP.md` |
| Capas | Todo en el ensamblado por defecto, sin namespaces (salvo `ColdSanctuary.TestRunner`, por CS0434); 0 asmdef | `docs/conventions.md`, `Assets/Scripts/TestRunner.cs` |
| Builder | `SampleSceneBuilder.cs` 4.314 líneas (+12 parciales, 2.622); `Build()` encadena unos 60 pasos; 387 `new Vector3(` con posiciones a mano | `Assets/Editor/SampleSceneBuilder.cs:52-190` |
| Registro de pruebas | El builder añade las `ITestUnit` (27 clases) **a mano** con `Group` fijo; si una no se añade, no corre y nadie lo nota | `SampleSceneBuilder.cs:86-113` |
| Pruebas | `TestProbe` (PASS/FAIL en `Editor.log`) + `TestRunner` en Play, solo si existe `Temp/run-tests`. Unity Test Framework 1.7.0 **instalado y sin usar** | `Assets/Scripts/TestProbe.cs`, `COLD-SANCTUARY/Packages/manifest.json` |
| Cobertura | 255 casos de uso, 179 con prueba (70 %); muchas solo comprueban que algo existe | `docs/casos-de-uso.md` |
| Emulación del jugador | `SpellConsole.Enter(string)` permite inyectar órdenes escritas; WASD y teclas se leen directamente de `Input` en 13 ficheros | `Assets/Scripts/Control/SpellConsole.cs:86` |
| CI | Solo `codemap.yml` (comprueba que el CODEMAP está regenerado). **No compila ni prueba** | `.github/workflows/codemap.yml` |
| Proyecto | El repo solo versiona `.cs`; el proyecto Unity es otra carpeta (Plastic) que se sincroniza con `tools/sync-to-unity.sh` | `CLAUDE.md` |
| Logging | 602 `Debug.Log*`, **153 etiquetas distintas** (`[Cocina]`, `[Restaurante]`, `[Diag]`, `[{vendorName}]`…), en dos idiomas | grep sobre `Assets/` |
| Datos | 7 ScriptableObjects (`Asana`, `ItemData`, `SanctuaryMission`…), pero las tablas grandes son código: `CookingRecipe.All`, `Gastronomy.Menu`, `AreaWork.Catalog`, `YogaLibrary.All`, `Era.Table` | `Assets/Scripts/Kitchen/`, `Progression/AreaWork.cs` |
| Acoplamiento a escena | 23 `GameObject.Find("…")` y 97 `FindObjects*Type` en runtime (sin contar tests) | grep |

## 5. Cambios propuestos (por prioridad)

Esfuerzo: **S** ≤ 1 día · **M** 2–5 días · **L** > 1 semana. Riesgo: probabilidad de romper escenas o el flujo actual.

| # | Cambio | Por qué (evidencia Shopify) | Ficheros de Cold-Sanctuary | Esf. | Riesgo |
|---|---|---|---|---|---|
| 1 | **Registro de pruebas verificado.** El builder descubre las `ITestUnit` con `TypeCache.GetTypesDerivedFrom<ITestUnit>()` en vez de añadirlas una a una. Una comprobación al arrancar el `TestRunner` falla si una clase `ITestUnit` no está en `TestRunner_AUTO` o si dos comparten `Group` sin querer. | `run-tests.sh` (`WORKSPACES_ESPERADOS`, `SIN_TESTS_PERMITIDO`) y `fixtures.test.ts` («declara todas las fixtures del directorio»): perder un test pone la puerta en rojo | `Assets/Editor/SampleSceneBuilder.cs:86-113`, `Assets/Scripts/TestRunner.cs`, `ITestUnit.cs` | S | Bajo |
| 2 | **Versionar el proyecto Unity entero en git**: `Packages/manifest.json` + `packages-lock.json`, `ProjectSettings/`, los `.meta` y las escenas generadas (LFS para binarios). Se retira `sync-to-unity.sh`. | Lockfiles versionados, `--frozen-lockfile`, `.nvmrc`/`packageManager`; la imagen Docker se construye completa en CI. Sin esto no hay CI posible | `.gitignore`, `.gitattributes`, `tools/sync-to-unity.sh`, `CLAUDE.md`, `docs/conventions.md` | M | Medio-alto (migrar desde Plastic; acordarlo con el compañero de Unity) |
| 3 | **CI con GameCI**: un workflow `tests.yml` con `game-ci/unity-test-runner@v4` (EditMode y PlayMode), caché de `Library/`, licencia en secretos, `concurrency`, `permissions: read`, `timeout`, y que `codemap` y el resto de pasos se ejecuten con `if: !cancelled()`. Más adelante, `unity-builder` para un *build* de Windows. | `.github/workflows/tests.yml`: puerta en PR y en la integradora, pasos independientes que no se tapan entre sí | `.github/workflows/tests.yml` (nuevo), `codemap.yml` | M | Medio (licencia y tiempo de importación); depende de #2 |
| 4 | **Assembly definitions por módulo**: `ColdSanctuary.Core` (Anima, Soul, Time, Chemistry), `.Fauna` (Animals, Microcosmos), `.Gameplay` (Kitchen, Farming, Progression, Mission…), `.UI`, `.Editor` (builders, solo Editor) y `.Tests.EditMode`/`.Tests.PlayMode`. Empezar por Editor y Tests (S) y separar el runtime poco a poco, cortando ciclos con interfaces. | Workspaces con dependencias propias; en el paquete de preventas, `domain` no importa `infra` y el cliente se inyecta (`IShopifyGraphQLClient`) | `Assets/**/*.asmdef` (nuevos); los ciclos aparecerán en `Anima`↔`Mind`↔`Control` | L | Alto (sin namespaces y con referencias globales cruzadas; hacerlo después de #2 para que los `.meta` viajen) |
| 5 | **Unity Test Framework (EditMode) para la lógica pura**, junto al `TestRunner` del juego (que se queda para las pruebas de escena). Primeras: `FallImpact.Land`, `TraceField`, `SoulMath`, `Era.Table`, `CookingRecipe`/`Gastronomy`, `WorkRoster`. Los `TestProbe.Check` de lógica pura pasan a `Assert`. | `decision.test.ts` y los tests de dominio del paquete corren en milisegundos y sin la plataforma; el *runner* de wasm se reserva para el contrato | `Assets/Tests/EditMode/*` (nuevo), `Assets/Scripts/Tests/*`, `TraceFieldTest.cs` | M | Bajo (el paquete ya está instalado) |
| 6 | **Separar la decisión pura del `MonoBehaviour`** (*humble object*): la regla en una clase estática o `struct` sin `UnityEngine` (p. ej. `PredationRules.Decide(ctx)`, `VolitionRules`, `RosterRules`) que devuelve un resultado tipado con motivo (`Decision<Motivo>`); el `MonoBehaviour` solo lee la escena y aplica. | `decision.ts` + `cart_validations_generate_run.ts` (adaptador de ~30 líneas); `validation-core` comparte el tipo `Decision<Permite,Bloquea>` | `Assets/Scripts/Transformation/Predation.cs`, `Assets/Animals/Volition.cs`, `Assets/Scripts/Progression/WorkdayDirector.cs` (539), `Control/SpellConsole.cs` (987) | M | Bajo-medio (refactor interno; cubrir antes con #5) |
| 7 | **Partir el builder y sacar el *layout* a datos.** `SampleSceneBuilder.cs` queda como orquestador (<300 líneas) y cada `BuildX` va a su parcial o a una clase `IAreaBuilder` registrada en una tabla, como ya se hace con `TestScenes`. Las posiciones de áreas, puentes y plataformas pasan a un `SanctuaryLayout` (SO o JSON). Una prueba compara un *snapshot* de la jerarquía generada antes y después (nombres + posiciones redondeadas). | Ficheros de una responsabilidad (`web/useCases/*`, extensiones de una función); la configuración vive fuera del código (metafield `fianza_config_v2`, `.toml`) | `Assets/Editor/SampleSceneBuilder*.cs`, `ResidentialBuilder.cs`, `HeadlessVerify.cs` | M | Medio (riesgo de mover el santuario: lo cubre el *snapshot*) |
| 8 | **Arnés E2E de «experiencia del jugador»**: escenarios PlayMode como lista de pasos (`Teclear("walkTo({destiny:\"Nevera\"})")`, `Pulsar(Key.E)`, `MoverHacia(destino)`, `EsperarHasta(cond, timeout)`, `Comprobar(...)`). Requiere una capa `IPlayerInput` (real o *scripted*) en los 13 ficheros que leen `Input`. Cada escenario lleva el ID de `casos-de-uso.md`; los primeros, las «10 pruebas que más faltan» (CU-HOS-04, CU-COC-09, CU-JUG-12…). | No usan Playwright: emulan el flujo con **entradas reales capturadas** (`CK-37-real-*.json`) y lo atan al catálogo de QA por ID. Aquí la «captura» sería un guion de órdenes grabado en una partida real | `Assets/Tests/PlayMode/Scenarios/*` (nuevo), `Control/SpellConsole.cs` (`Enter`), `Player/PlayerController.cs`, `PlayerCore`, `docs/casos-de-uso.md` | L | Medio (la capa de input toca el control del jugador) |
| 9 | **Configuración guiada por datos**: `CookingRecipe.All`, `Gastronomy.Menu`, `AreaWork.Catalog`, `YogaLibrary.All` y `Era.Table` pasan a ScriptableObjects (o JSON en `Resources/` mientras no se versionen los `.asset`). Un test de contrato comprueba IDs únicos, áreas existentes y referencias resueltas. | Metafield editable desde el admin, `locales/*.json`, ajustes de la extensión en el editor; `check-docs.sh` como contrato entre datos y documentación | `Assets/Scripts/Kitchen/{CookingRecipe,Gastronomy}.cs`, `Progression/AreaWork.cs`, `Asana/YogaLibrary.cs`, `Progression/Era.cs` | M | Medio (si los `.asset` no se versionan, se pierden: hacer después de #2 o usar JSON) |
| 10 | **Convención de logging**: una clase estática `Log` con canal `enum` (`Log.Info(Canal.Cocina, …)`, `Log.Warn`, `Log.Critical`), un idioma para las etiquetas, `[TEST]`/`[VERIFY]`/`[LIVE]` reservadas, y canales ruidosos tras `[Conditional("CS_VERBOSE")]`. Nada de etiquetas interpoladas como `[{vendorName}]`. | Prefijo de área estable, `[CRITICAL]` para lo que rompe el arranque y warn frente a error con criterio; los errores importantes se persisten (`IntegrationLogger`) | `Assets/Scripts/Log.cs` (nuevo); migración progresiva de los 602 `Debug.Log*` | S–M | Bajo |
| 11 | **Documentación comprobada por CI**: un `tools/check_casos.py` que extrae los nombres de `TestProbe.Check(...)` y verifica que cada fila «cubierta» de `casos-de-uso.md` existe en el código, y que ningún ID `CU-*` se repite ni se reutiliza. Junto al CODEMAP. | `check-docs.sh` (tabla de `ARCHITECTURE.md` frente a los `.toml`); «un ID no se reutiliza» en `docs/qa/casos-de-uso.md` | `tools/check_casos.py` (nuevo), `docs/casos-de-uso.md`, `.github/workflows/codemap.yml` | S | Bajo |
| 12 | **Nombres y namespaces**: `namespace ColdSanctuary.<Módulo>` alineado con el asmdef (#4), lo que elimina la excepción del `TestRunner`. Sufijos fijos: `*Test` para `ITestUnit`, `*Sandbox` para demos y `*Builder` para el Editor (hoy se mezclan `*Auto`, `Checklist*` y `*Test`). Dominio en español y términos de Unity en inglés, como hace la app. | Fichero = clase, casos de uso con verbo, vocabulario de dominio coherente (`decidir`, `motivo`, `huella`) | Todo `Assets/`; `tools/gen_codemap.py` (agrupar por namespace) | M | Medio (renombrar sin `.meta` versionados rompe referencias: hacerlo después de #2) |

### Orden sugerido

1. **Ya, sin dependencias:** #1, #10 y #11 (S, riesgo bajo).
2. **Base:** #2, y después #3 (sin proyecto versionado no hay CI), y #5 con un primer asmdef de Tests.
3. **Refactor con red:** #6 y #7, protegidos por #5 y por el *snapshot* del builder.
4. **Estructura:** #4 y #12 juntos, módulo a módulo.
5. **Experiencia del jugador:** #8 y #9, empezando por las «10 pruebas que más faltan».

### Lo que no conviene copiar

Secretos en el repo; dos generaciones de capas a medio migrar (`models/` frente a `modules/`); no tener lint
ni *hooks*. Para Cold-Sanctuary, un `.editorconfig` con reglas de análisis de Roslyn y un *pre-commit* que
regenere el CODEMAP cuestan poco y cubren ese hueco.
