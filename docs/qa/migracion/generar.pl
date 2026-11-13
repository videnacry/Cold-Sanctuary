#!/usr/bin/env perl
# MIGRACIÓN AL SISTEMA DE QA (docs/qa-propuesta.md §10). Genera, sin escribir nada a mano:
#   docs/qa/peticiones.md, docs/qa/casos.md (casos de las PB y de las reglas del manual) y docs/qa/registro.md (sus estados)
# a partir de docs/auditoria-peticiones.md, docs/manual-de-diseno.md y los tests (Assets/**/*.cs con «int Group =>»).
# Lo que no sale de esas fuentes (los CV, los OP, los B-nn y HC-nn, los run logs) va en las tablas de este script, con su fuente.
# USO (desde la raíz del repo):  perl docs/qa/migracion/generar.pl
# YA NO SE VUELVE A CORRER (2026-11-13): sobrescribiría los tres ficheros y movería los IDs de los casos de reglas. Desde la migración,
# el mantenimiento es a mano, fila a fila (docs/qa/README.md). Se guarda como registro de cómo se hizo.
# Fallo conocido: el número de grupo seguido de coma («95, `check-personajes.sh`») no se leía, y quedaron sin caso M30, P7, E10, E15
# y P19; se añadieron a mano (MIC-83, PER-23, PER-24, FAU-31, FAU-32). Abajo está corregido, por si alguien lo estudia.
use strict; use warnings; use utf8;
binmode STDOUT, ':encoding(UTF-8)';
sub slurp { my $f = shift; open my $h, '<:encoding(UTF-8)', $f or die "no puedo leer $f"; local $/; my $s = <$h>; close $h; $s =~ s/\r//g; $s }
sub spit { my ($f, $s) = @_; open my $h, '>:encoding(UTF-8)', $f or die "no puedo escribir $f"; print $h $s; close $h; print "escrito $f\n" }
sub cell { my $s = shift // ''; $s =~ s/\|/\//g; $s =~ s/\s+/ /g; $s =~ s/^\s+|\s+$//g; $s eq '' ? '—' : $s }

# ── 1. Tests: grupo → unidad, y si existía en la suite 63 (d0542db) ─────────────────────────────────────────────────
my (%unitOf, %inSuite63);
for my $f (split /\n/, `git ls-files "Assets/*.cs"`) {
    next unless -f $f; my $s = slurp($f);
    next unless $s =~ /int Group => (\d+)/; my $g = $1; (my $u = $f) =~ s{.*/|\.cs$}{}g;
    $unitOf{$g} //= $u;
    $inSuite63{$g} = (system("git cat-file -e d0542db:\"$f\" 2>/dev/null") == 0) ? 1 : 0;
}

# ── 2. Áreas: por grupo (propuesta §3, ampliada a todos los grupos), por letra de regla y por palabras ──────────────
my %areaOfGroup;
my %byArea = (
    FAU => [1,2,3,4,5,6,7,8,9,10,11,12,13,14,16,23,29,71,85,99,102,105,106,107,108,109,111],
    JOR => [28,41,45,46,50,65,89,92,97,104,113],
    YOG => [15,42,51,64], GYM => [49,87],
    ARB => [25,27,53,54,55,56,57,70,73,74,75,80,91,98,103],
    MIC => [43,44,58,59,60,61,62,63,66,67,68,69,76,77,78,79,86,115,116],
    HOS => [81,82,83,84], NUC => [90,94,100,110,114], ENE => [101],
    PER => [21,22,24,26,52,88,95], UI => [93], REN => [0,17,20,112,117], PX => [40,96],
);
for my $a (keys %byArea) { $areaOfGroup{$_} = $a for @{$byArea{$a}} }
my %areaOfLetter = (S=>'ARB', T=>'ARB', C=>'ARB', G=>'ARB', M=>'MIC', F=>'MIC', J=>'JOR', H=>'HOS', E=>'FAU', A=>'FAU', P=>'PER');
my @areaWords = (   # [regex sobre la petición, área] — solo si no hay grupo ni regla; las primeras, más concretas
    [qr/microcosmos|pueblo|historia|plantilla|muros hist|insecto-humano|figuras/i, 'MIC'],
    [qr/deslumbr/i, 'REN'], [qr/yoga|Ashtanga|Consol/i, 'YOG'], [qr/nombres reales/i, 'PER'],
    [qr/cama|litera|actividades|estructura con su interacci/i, 'JOR'], [qr/insectos en el santuario/i, 'FAU'],
    [qr/\bmicros?\b|insectos-humanos|vegetaci|Bolívar|mapas/i, 'MIC'], [qr/Santuario Marino|mar profundo/i, 'NUC'],
    [qr/puertas|ramas|en obra|terminado|muebles|camilla/i, 'ARB'],
    [qr/fauna|animal|depredador|oso|aves|morsa|pingüino|icebergs/i, 'FAU'],
    [qr/árbol|edificio|piso|alfombra|anillo|torre|columna|plataforma|cristal|rampa/i, 'ARB'],
    [qr/hospital|sangre|ballena/i, 'HOS'],
    [qr/nuclear|refugio|lecho|hielo|lanzadera|santuario 3/i, 'NUC'],
    [qr/solar|energ|turbina/i, 'ENE'],
    [qr/jornada|reparto|tareas|iniciaci|horario|guardería|Neo\b/i, 'JOR'],
    [qr/personaje|frases|bonusPack/i, 'PER'],
    [qr/sombras|opaco|brill|pesa más|focos/i, 'REN'],
    [qr/docs|checklist/i, 'OP'],
);

# ── 3. Reglas del manual: ID → grupos de «Se comprueba en» ──────────────────────────────────────────────────────────
my $manual = slurp('docs/manual-de-diseno.md');
my (%rule, @ruleOrder);
for my $l (split /\n/, $manual) {
    next unless $l =~ /^\| ([A-Z]\d+) \|/; my @c = split / \| /, $l; my $id = $1;
    next if exists $rule{$id};   # (J31 está dos veces: vale la primera)
    my $chk = $c[3] // ''; my @g;
    unless ($chk =~ /solo en docs/i && $chk !~ /\d/) {
        while ($chk =~ /`(\w+Test|\w+Checks|\w+Auto)`/g) { my $u = $1; for my $g (keys %unitOf) { push @g, $g if $unitOf{$g} eq $u } }
        while ($chk =~ /(?<![\d,.])(\d{1,3})(?!\d|[,.]\d|\s*(?:m\b|s\b|%|cm|min|h\b))/g) { push @g, $1 if exists $unitOf{$1} }
    }
    my %seen; @g = grep { !$seen{$_}++ } @g;
    $rule{$id} = { groups => \@g, text => $c[1] // '' }; push @ruleOrder, $id;
}

# ── 4. Rondas citadas → run log reconstruido (fichero del probador buscado por el texto de la evidencia) ──────────
my %run = (   # ronda citada en la auditoría → [nombre del run log, commit (git rev-list -1 --before=<hora del fichero>), fecha]
    63 => ['2026-11-12-1208-r63-retro.md', 'd0542db', '2026-11-12'],
    10 => ['2026-11-05-1544-r10-retro.md', '479ebc1', '2026-11-05'],
    13 => ['2026-11-06-0127-r13-retro.md', '5b9774a', '2026-11-06'],
    23 => ['2026-11-08-1136-r23-retro.md', 'b930954', '2026-11-08'],
    30 => ['2026-11-08-2045-r30-retro.md', '2a36c30', '2026-11-08'],
    31 => ['2026-11-09-1823-r31-retro.md', '0932f95', '2026-11-09'],
    34 => ['2026-11-10-0949-r34-retro.md', 'bd58414', '2026-11-10'],
    66 => ['2026-11-12-2056-r66-retro.md', '2ac2bbc', '2026-11-12'],
    67 => ['2026-11-12-2144-r67-retro.md', '2ac2bbc', '2026-11-12'],
    68 => ['2026-11-12-2314-r68-retro.md', '2ac2bbc', '2026-11-12'],
    73 => ['2026-11-13-0426-r73-retro.md', 'c8fcfd2', '2026-11-13'],
);

# ── 5. Peticiones de la auditoría ─────────────────────────────────────────────────────────────────────────────────
my @pb;
for my $l (split /\n/, slurp('docs/auditoria-peticiones.md')) {
    next unless $l =~ /^\| (\d+) \|/; (my $r = $l) =~ s/^\| //; $r =~ s/ ?\|\s*$//;
    my @c = split / \| /, $r; my $n = shift @c; my $f = shift @c; my $ev = pop @c; my $st = pop @c; my $p = join(' / ', @c);
    my ($s, $d) = $st =~ /^\*\*([^*]+)\*\*\s*(?:\(([^)]*)\))?/ ? ($1, $2 // '') : ($st, '');
    (my $evt = $ev) =~ s/\]\(https:[^)]*\)/]/g; $evt =~ s/\*\*//g; $p =~ s/\*\*//g;
    push @pb, { n => $n, fecha => $f, pet => $p, clase => $s, cuando => $d, ev => $evt };
}

my %HC = (   # huecos y decisiones pendientes (propuesta §3 y §10)
    'HC-01' => ['PB-140', 'Valores de los bonusPacks de Irosene, Lizmibet y Linduarda: los decide Berón', 'Berón'],
    'HC-02' => ['PB-082', 'Afinar con OpenStreetMap los huecos históricos (la parte abierta de PB-082, L282)', 'sin dueño'],
    'HC-03' => ['—', '¿«Alicia» es el nombre real de Alicita? Se quitó del rótulo por defecto (P24); docs/personajes/alicita.md', 'Berón'],
    'HC-04' => ['—', 'Hora de la suite diaria (nivel N4) acordada con Berón (Unity es compartido)', 'Berón'],
);

# ── 6. Casos de las PB ────────────────────────────────────────────────────────────────────────────────────────────
my (%next, @cases, %reg, @unmapped); $next{OP} = 9;   # OP-01…OP-08 son las de la propuesta §7 (abajo)
sub newId { my $a = shift; $next{$a} //= 1; sprintf('%s-%02d', $a, $next{$a}++) }
for my $x (@pb) {
    my $id3 = sprintf('PB-%03d', $x->{n}); $x->{id} = $id3;
    my $text = "$x->{pet} $x->{ev}";
    my @g; while ($text =~ /\bg(\d{1,3})\b|grupos? (\d{1,3})\b/g) { my $g = $1 // $2; push @g, $g if exists $unitOf{$g} }
    my %s; @g = grep { !$s{$_}++ } @g;
    my @r; while ($text =~ /\b([SFTCGHMAJWEP]\d{1,2})\b/g) { push @r, $1 if exists $rule{$1} }
    %s = (); @r = grep { !$s{$_}++ } @r; $x->{rules} = \@r;
    my @L = $x->{ev} =~ /\[(L\d+)\]/g; %s = (); @L = grep { !$s{$_}++ } @L;
    if ($x->{clase} eq 'Hecho (doc)') { $x->{estado} = 'Doc'; $x->{cases} = []; next }
    if ($x->{n} == 140) { $x->{estado} = 'Abierta'; $x->{cases} = []; $x->{hc} = 'HC-01'; next }
    my $area; for my $g (grep { $_ != 117 } @g) { if ($areaOfGroup{$g}) { $area = $areaOfGroup{$g}; last } }
    if (!$area) { for my $r (@r) { my ($le) = $r =~ /^([A-Z])/; if ($r =~ /^W1[2-5]$/) { $area = 'REN'; last } if ($areaOfLetter{$le}) { $area = $areaOfLetter{$le}; last } } }
    if (!$area) { for my $w (@areaWords) { if ($x->{pet} =~ $w->[0]) { $area = $w->[1]; last } } }
    if (!$area) { $area = 'PX'; push @unmapped, $id3 }
    my $cid = newId($area); $x->{cases} = [$cid];
    my $g0 = $g[0]; my @otros = @g > 1 ? map { "g$_" } @g[1..$#g] : ();
    push @cases, { id => $cid, area => $area, que => $x->{pet}, unidad => defined $g0 ? $unitOf{$g0} : '—', grupo => defined $g0 ? $g0 : '—',
                   reglas => join(', ', @r) || '—', pb => $id3, origen => join(', ', "auditoría #$x->{n}", @L, (@otros ? 'también ' . join(', ', @otros) : ())) };
    # Registro
    my $c = $x->{clase};
    if ($c eq 'Verificado en vivo') {
        my $rn = 63;
        if ($x->{ev} =~ /ronda (\d+), 2026/) { $rn = $1 }                       # sonda g117 (ronda 73)
        elsif ($x->{ev} !~ /suite 63/ && $x->{ev} =~ /rondas? (\d+)/) { $rn = $1 }
        my @extra; unless ($rn == 73) { while ($x->{ev} =~ /rondas? (\d+)/g) { push @extra, $1 if $1 != $rn && $run{$1} } if ($x->{ev} =~ /rondas 66, 67 y 68/) { push @extra, 67, 68 } }
        %s = (); @extra = grep { !$s{$_}++ && $_ != $rn } @extra;
        my $rl = $run{$rn} or die "ronda $rn sin run log ($id3)";
        my $nota = "auditoría: verificado en vivo" . ($x->{cuando} ? " $x->{cuando}" : "") . (@extra ? '; también ' . join(', ', map { "runs/$run{$_}[0]" } @extra) : '');
        $reg{$cid} = ['Probado (retro)', $x->{cuando} || $rl->[2], $rl->[1], "runs/$rl->[0]", '—', $nota];
        $x->{estado} = 'Hecha';
    } elsif ($c eq 'Abierto') {
        $reg{$cid} = ['Sin probar', '—', '—', '—', '—', 'abierta en la auditoría: falta código'];
        $x->{estado} = 'Abierta';
    } elsif ($c eq 'Sin anotar') {
        my $code = $x->{ev} =~ /`[^`]+`|\.cs\b|\b[0-9a-f]{7}\b|[Gg]ancho/ ? 1 : 0;
        $reg{$cid} = [$code ? 'Solo código' : 'Sin probar', '—', '—', '—', '—', 'sin anotar en el checklist (auditoría)' . ($code ? '; hay código' : '; sin código localizado')];
        $x->{estado} = 'Sin verificar';
    } else {   # Sin verificar en vivo, una parte abierta, Hecho (doc); sin verificar
        my $hc = $x->{n} == 82 ? 'HC-02' : '—';
        $reg{$cid} = ['Solo código', '—', '—', '—', $hc, "auditoría: $c"];
        $x->{estado} = $hc ne '—' ? 'Abierta' : 'Sin verificar'; $x->{hc} = $hc if $hc ne '—';
    }
}
# El caso de PB-082 abre HC-02; la PB queda Abierta (propuesta §3: «Abierta si tiene un HC»).

# ── 7. Casos de las reglas del manual con «Se comprueba en» un grupo ─────────────────────────────────────────────
my %ruleCase; my $suiteFail = qr/^(50)$/;   # suite 63: 4477 PASS / 4 FAIL, todos en g50
for my $id (@ruleOrder) {
    my @g = @{$rule{$id}{groups}}; next unless @g;
    my ($le) = $id =~ /^([A-Z])/; my $area = $id =~ /^W1[2-5]$/ ? 'REN' : $areaOfLetter{$le} // $areaOfGroup{$g[0]} // 'PX';
    my $cid = newId($area); $ruleCase{$id} = $cid; my $g0 = $g[0];
    (my $t = $rule{$id}{text}) =~ s/\*\*//g; $t =~ s/\[([^\]]*)\]\([^)]*\)/$1/g; $t = substr($t, 0, 220) . (length($t) > 220 ? '…' : '');
    push @cases, { id => $cid, area => $area, que => "Regla $id: $t", unidad => $unitOf{$g0}, grupo => $g0, reglas => $id, pb => '—',
                   origen => join(', ', 'manual §«Se comprueba en»', (@g > 1 ? 'también ' . join(', ', map { "g$_" } @g[1..$#g]) : ())) };
    my @fails = grep { /$suiteFail/ } @g; my @nuevos = grep { !$inSuite63{$_} } @g;
    if (!$inSuite63{$g0}) { $reg{$cid} = ['Sin probar', '—', '—', '—', '—', "g$g0 no existía en la suite 63"] }
    elsif (@fails)       { $reg{$cid} = ['Parcial', '2026-11-12', 'd0542db', "runs/$run{63}[0]", 'B-01', 'g50 con 4 FAIL en la suite 63 (fauna atascada, B-01, y las misiones de Kushal)'] }
    else                 { $reg{$cid} = ['Probado (retro)', '2026-11-12', 'd0542db', "runs/$run{63}[0]", '—', 'su grupo en verde en la suite 63' . (@nuevos ? '; ' . join(', ', map { "g$_" } @nuevos) . ' aún no existía' : '')] }
}

# ── 8. CV (propuesta §10, paso 3) y OP (§7) ───────────────────────────────────────────────────────────────────────
my @cv = (
 ['CV-01','IFastGameTime (maxDelta 1) × puertas, disparadores y agentes','Ningún paso grande hace que un agente se salte una puerta o un disparador; el grupo que lo usa y los siguientes, igual de verdes','JornadaTest',50,'5347100, g50 y g40'],
 ['CV-02','maxDelta 0,04 de GymUsabilityTest × el grupo siguiente','TestRunner.Sanear lo restaura; 0 «FUGA» de tiempo','GymUsabilityTest',49,'13a213a, g49'],
 ['CV-03','Warp de la fauna × viaje en marcha','Quitar o cambiar el Warp no sube «atascado > 20 s» respecto a la media de las 3 rondas anteriores (contadores.md)','JornadaTest',50,'52d2949; r76–r77 dieron 27 y 34'],
 ['CV-04','Pausa de otra conducta × velocidad de un viaje','Un viaje en marcha nunca queda con velocidad «pedida 0,0»','FaunaChecks',1,'52d2949'],
 ['CV-05','Habit × Volition (deseo) × ciclo de vida × viajes','El cuerpo tiene un solo dueño (el deseo que gana); el hábito no cuenta como atasco','FaunaChecks',1,'d44f7e8, 5d89067'],
 ['CV-06','TreeRotator × NavMesh (StaleNavCarver) × crías del recinto (PenWalker)','La raíz vuelve a tallar el NavMesh tras cada paso; las crías del recinto andan sin NavMesh y no se quedan quietas','TreeOfFieldsTest',53,'2847127, 76db8a8, 4990e1c'],
 ['CV-07','WorkdayDirector × ronda de frutales (FruitGardener) × detector de «dar vueltas»','La ronda de frutales no es «dar vueltas»','JornadaTest',50,'f64fad3'],
 ['CV-08','WorkdayDirector × clase de yoga','Las conductas que dejó la jornada se pausan en la clase','YogaClassTest',51,'e2c0c7c'],
 ['CV-09','Jornada × horario libre (16 h) × cena (19 h, MealRitual)','La barra se vuelve a llenar a las 19 h; nadie se queda sin franja','HorarioLibreTest',65,'2d9a0aa, 31df64e'],
 ['CV-10','WorkRoster: reparto × opcionales × clones de la Magnate × iniciaciones','Primero las obligadas; si nadie está libre, las opcionales no se hacen ni piden clon; antes una iniciación que un clon','RepartoSantuariosTest',113,'16cb73e, 6a8b400, 19740d1'],
 ['CV-11','Clon de la Magnate × misión con herramienta (alimentador)','El clon con el alimentador da de comer','AlimentadorTest',105,'0cd78a8, 4299722'],
 ['CV-12','PopulationPlan × crías, icebergs y otras especies','Vivos ≤ K × humanos en el total que cuenta; el [ECO] lo dice','RepartoSantuariosTest',113,'PB-151, g113'],
 ['CV-13','CheapMaterialsPass (editor) × CheapMaterialsSweeper (juego) × constructores de microcosmos × emisión','Mates también en juego y tras reconstruir un microcosmos','PeticionesSondaTest',117,'46f92ea, 42b0def, PB-152'],
 ['CV-14','NavAgentSafe × alfombra, Línea, muelle aéreo, en brazos','El agente vuelve a encenderse sobre el NavMesh','JornadaTest',50,'63de4fd'],
 ['CV-15','RemoteStaff (santuario 2) × TestRunner × jornada','Con las pruebas en marcha no hay nadie apagado; sin pruebas, la jornada manda','JornadaTest',50,'g50, Lizmibet y Linduarda'],
);
for my $c (@cv) { push @cases, { id => $c->[0], area => 'CV', que => "$c->[1]: $c->[2]", unidad => $c->[3], grupo => $c->[4], reglas => '—', pb => '—', origen => $c->[5] };
                  $reg{$c->[0]} = ['Sin probar', '—', '—', '—', '—', 'caso nuevo: falta el Check con [' . $c->[0] . '] (lo añade quien toque la prueba)'] }
my @op = (['OP-01','W3'],['OP-02','—'],['OP-03','W7'],['OP-04','—'],['OP-05','—'],['OP-06','W5, W6'],['OP-07','—'],['OP-08','W3']);
for my $o (@op) { push @cases, { id => $o->[0], area => 'OP', que => "Regla operativa $o->[0] (operacion.md)", unidad => '—', grupo => '—', reglas => $o->[1], pb => '—', origen => 'qa-propuesta.md §7' };
                  $reg{$o->[0]} = ['No aplica', '—', '—', '—', '—', 'regla de proceso: la mira el probador en cada run log'] }

# ── 9. Escribir ───────────────────────────────────────────────────────────────────────────────────────────────────
my %casesOfRule; for my $c (@cases) { next if $c->{reglas} eq '—'; push @{$casesOfRule{$_}}, $c->{id} for split /, /, $c->{reglas} }
my $hdr = "<!-- GENERADO por docs/qa/migracion/generar.pl el 2026-11-13 desde docs/auditoria-peticiones.md, docs/manual-de-diseno.md y los tests. Desde aquí se mantiene a mano. -->\n";

my $o = "# Peticiones de Berón (PB)\n\n$hdr\nCada petición de Berón, con su número fijo. **Una petición nueva entra el mismo día** como la siguiente PB libre (ver [README.md](README.md) §Cómo entra una petición).\n"
      . "El **estado no se escribe a mano**: sale de sus casos en [registro.md](registro.md) (lo comprueba el comprobador):\n"
      . "`Hecha` si todos sus casos están `Probado (…)`; `Doc` si solo pedía un doc o una regla; `Sin verificar` si alguno está en `Solo código` o `Sin probar`; `Abierta` si tiene un `HC` o le falta código.\n\n"
      . "Las PB-001…PB-155 vienen de [auditoria-peticiones.md](../auditoria-peticiones.md) (el número es el mismo). La siguiente libre es **PB-156**.\n\n"
      . "| PB | Fecha | Petición | Reglas | Casos | Estado | Origen |\n|---|---|---|---|---|---|---|\n";
my %cnt;
for my $x (@pb) { $cnt{$x->{estado}}++; my $casos = @{$x->{cases}} ? join(', ', @{$x->{cases}}) : ($x->{hc} // '—'); $casos .= ", $x->{hc}" if $x->{hc} && @{$x->{cases}};
  $o .= '| ' . join(' | ', $x->{id}, cell("2026-$x->{fecha}"), cell($x->{pet}), cell(join(', ', @{$x->{rules}})), cell($casos), $x->{estado}, cell("auditoría #$x->{n}: $x->{clase}" . ($x->{cuando} ? " ($x->{cuando})" : ''))) . " |\n" }
$o .= "\n**Recuento:** " . join(' · ', map { "$_ $cnt{$_}" } sort keys %cnt) . " · total " . scalar(@pb) . ".\n";
spit('docs/qa/peticiones.md', $o);

my %areaName = (FAU=>'Fauna', JOR=>'Jornada y horario libre', YOG=>'Yoga', GYM=>'Gimnasio', ARB=>'Árboles, edificios y alfombras', MIC=>'Microcosmos', HOS=>'Hospital',
                NUC=>'Nuclear, refugio y santuarios 2 y 3', ENE=>'Energía', PER=>'Personajes y charlas', UI=>'Interfaz', REN=>'Rendimiento y pruebas', PX=>'Recorridos en vivo',
                CV=>'Convivencia de procesos (entrelazamiento.md)', OP=>'Reglas operativas (operacion.md)');
$o = "# Catálogo de casos\n\n$hdr\nIDs estables: **no se renumeran ni se borran** (un caso que ya no vale se tacha y pasa a `Retirado` en el registro). Formato del ID: 2–3 letras de área + 2 cifras.\n"
   . "El estado de cada caso está en [registro.md](registro.md). «Unidad» y «Grupo» son la prueba del `TestRunner` que lo comprueba (`int Group =>`); «—» si aún no tiene.\n"
   . "En la prueba, el ID va al principio del texto del `TestProbe.Check`: `\"[CV-03] …\"`.\n\n";
for my $a (qw(FAU JOR YOG GYM ARB MIC HOS NUC ENE PER UI REN PX CV OP)) {
    my @c = grep { $_->{area} eq $a } @cases; next unless @c;
    $o .= "## $a · $areaName{$a}\n\n| ID | Área | Qué tiene que pasar | Unidad | Grupo | Reglas | PB | Origen |\n|---|---|---|---|---|---|---|---|\n";
    $o .= '| ' . join(' | ', $_->{id}, $_->{area}, cell($_->{que}), cell($_->{unidad}), cell($_->{grupo}), cell($_->{reglas}), cell($_->{pb}), cell($_->{origen})) . " |\n" for @c;
    $o .= "\n";
}
$o .= "## Bugs y huecos\n\n`B-nn` = fallo con dueño; `HC-nn` = hueco o decisión pendiente. Salen de un `Hn` de [hallazgos.md](hallazgos.md).\n\n"
   . "| ID | Tipo | Qué | Casos | Dueño | Estado | Hallazgo |\n|---|---|---|---|---|---|---|\n"
   . "| B-01 | Fallo | g50: fauna atascada > 20 s, dando vueltas o volcada. Falla en todas las rondas que corrieron g50 desde r71 (crónico) | CV-03, CV-05, " . join(', ', grep { $reg{$_}[4] eq 'B-01' } map { $_->{id} } @cases) . " | probador → trabajador de fauna | Abierto | H1 |\n"
   . "| B-02 | Fallo | El script de marcas vació docs/checklist.md y docs/auditoria-peticiones.md (82878fc): leía un temporal que no existía | OP-04 | probador | Arreglado en 1bdfe1c; falta el aviso automático (QaCheck §6.9) | H2 |\n"
   . "| B-03 | Fallo | Ficheros sueltos en Unity tras cambiar de rama: TmpCheck y .cs que ya no están en git | OP-05 | probador | Arreglado con sync --prune (r83, r84); falta el aviso automático (QaCheck §6.10) | H3 |\n";
for my $h (sort keys %HC) { $o .= "| $h | Hueco | $HC{$h}[1] | $HC{$h}[0] | $HC{$h}[2] | Abierto | — |\n" }
spit('docs/qa/casos.md', $o);

$o = "# Registro de estados\n\n$hdr\nUna fila por caso de [casos.md](casos.md). **Sin run log no hay «Probado»**. Estados:\n"
   . "`Probado (suite gN)` · `Probado (en vivo)` · `Probado (retro)` (solo de la migración, con run log reconstruido) · `Parcial` · `FAIL` · `Inestable` (pasa y falla en el mismo commit) · "
   . "`Crónico` (el mismo check falla en ≥ 3 rondas seguidas: pide `B-nn` y dueño) · `Solo test` · `Solo código` (programado y compilado, sin ejecutar) · `Sin probar` · `No aplica` · `Retirado`.\n\n"
   . "Una evidencia **caduca** si `TestImpact` dice que un commit posterior toca su grupo: el comprobador la marca «a revisar».\n\n"
   . "| ID | Estado | Fecha | Commit | Run log | B/HC | Nota |\n|---|---|---|---|---|---|---|\n";
my %rc;
for my $c (@cases) { my $r = $reg{$c->{id}} or die "sin registro $c->{id}"; $rc{$r->[0]}++;
  $o .= '| ' . join(' | ', $c->{id}, $r->[0], cell($r->[1]), cell($r->[2]), $r->[3] eq '—' ? '—' : "[$r->[3]]($r->[3])", cell($r->[4]), cell($r->[5])) . " |\n" }
$o .= "\n**Recuento:** " . join(' · ', map { "$_ $rc{$_}" } sort keys %rc) . " · total " . scalar(@cases) . ".\n\n"
   . "## Inestables y crónicos\n\nChecks concretos (el texto del `TestProbe.Check`), no casos enteros. Se mira en [contadores.md](contadores.md) y en los run logs.\n\n"
   . "| Check | Grupo | Estado | Rondas | B/HC | Dueño | Nota |\n|---|---|---|---|---|---|---|\n"
   . "| fauna: ningún animal atascado > 20 s, dando vueltas o volcado | 50 | Crónico | r71–r80, r82, r83 (12 rondas con g50; r81 no corrió g50) | B-01 | probador → trabajador de fauna | contadores.md: 4, 12, 11, 9, 17, 27, 34, 5, 6, 10, 7, 14 |\n"
   . "| fauna: ningún animal dentro de muebles, en el agua sin ser acuático ni cayendo | 50 | Inestable | FAIL en r73, r78, r80, r82; pasa en r74–r77, r79, r83 | — | sin dueño | H6: falló y pasó en commits distintos; falta repetirlo en un mismo commit para confirmarlo |\n"
   . "| un paseo sin salto posible descansa a propósito / …y prueba otra dirección (gira ≥ 90°) | 1 | Inestable | FAIL en r77 (6 líneas); pasa en r73–r75, r78, r79, r82, r83 | — | sin dueño | H5: falló y pasó en commits distintos; falta repetirlo en un mismo commit |\n"
   . "| día 1/2: Kushal completa TODAS sus misiones | 50 | Inestable | FAIL en la suite 63 (d0542db); pasa en la suite r71 y en r72–r83 | — | sin dueño | runs/$run{63}[0] |\n";
spit('docs/qa/registro.md', $o);

print "PB: " . scalar(@pb) . " · casos: " . scalar(@cases) . " (de PB " . scalar(grep { $_->{pb} ne '—' } @cases) . ", de reglas " . scalar(keys %ruleCase) . ", CV " . scalar(@cv) . ", OP " . scalar(@op) . ")\n";
print "PB sin área clara (van a PX): @unmapped\n" if @unmapped;
print "PB por estado: " . join(' · ', map { "$_ $cnt{$_}" } sort keys %cnt) . "\n";
print "registro: " . join(' · ', map { "$_ $rc{$_}" } sort keys %rc) . "\n";
