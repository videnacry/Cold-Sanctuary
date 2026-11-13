#!/usr/bin/env perl
# RUN LOGS RETRO (docs/qa-propuesta.md §10): uno por cada ronda que citan los casos migrados (registro.md), reconstruido
# desde las salidas del probador. USO: perl docs/qa/migracion/runs.pl <carpeta del probador>   (desde la raíz del repo)
# La carpeta del probador era scratchpad/tester (r*.out, suite*.out, runner*.log). El commit es el de claude/edificios justo
# antes de la hora del fichero (git rev-list -1 --before), salvo la suite 63, que lo dice su auditoría (d0542db).
use strict; use warnings; use utf8;
my $T = shift or die "falta la carpeta del probador";
sub slurp { my $f = shift; open my $h, '<:encoding(UTF-8)', $f or return ''; local $/; my $s = <$h>; close $h; $s =~ s/\r//g; $s }
my @runs = (   # [run log, ronda citada, modo, commit, ficheros, cómo se encontró]
 ['2026-11-12-1208-r63-retro.md', 63, 'suite', 'd0542db', ['suite63.out', 'suite63.groups'], 'suite 63 (la auditoría: «suite 63, d0542db»)'],
 ['2026-11-05-1544-r10-retro.md', 10, 'grupos', '479ebc1', ['round10.out'], 'por el número: el texto de la evidencia («inician los trabajadores, Magnate último recurso») NO está en round10.out'],
 ['2026-11-06-0127-r13-retro.md', 13, 'vivo', '5b9774a', ['r13a.out', 'r13b.out', 'r13c.out', 'r13d.out'], 'por el número; «la mariquita» está en r13c.out'],
 ['2026-11-08-1136-r23-retro.md', 23, 'vivo', 'b930954', ['r23.out'], 'por el número y el texto (rótulos sin nombres reales)'],
 ['2026-11-08-2045-r30-retro.md', 30, 'vivo', '2a36c30', ['r31.out'], 'por el texto «Kapilavastu: piso ×100 con muros», que está en r31.out (y r33.out), no en r30.out: la numeración de la auditoría no es la de los ficheros'],
 ['2026-11-09-1823-r31-retro.md', 31, 'vivo', '0932f95', ['runner41.log'], 'por el texto «Áreas no básicas en obra», que está en runner41.log (y runner43/45.log); no hay r31.out con él'],
 ['2026-11-10-0949-r34-retro.md', 34, 'vivo', 'bd58414', ['r44.out'], 'por el texto «59 microcosmos», que está en r44.out: la «ronda 34» de la auditoría es el fichero r44'],
 ['2026-11-12-2056-r66-retro.md', 66, 'vivo', '2ac2bbc', ['r66.out'], 'por el número y el texto (mapas a mano, Build Microcosmos de historias)'],
 ['2026-11-12-2144-r67-retro.md', 67, 'vivo', '2ac2bbc', ['r67.out'], 'por el número y el texto'],
 ['2026-11-12-2314-r68-retro.md', 68, 'vivo', '2ac2bbc', ['r68.out'], 'por el número y el texto'],
 ['2026-11-13-0426-r73-retro.md', 73, 'grupos', 'c8fcfd2', ['r73.out'], 'por el número y el texto (g117, la sonda de peticiones)'],
);
mkdir 'docs/qa/runs';
for my $r (@runs) {
    my ($name, $n, $mode, $sha, $files, $how) = @$r;
    my $all = join("\n", map { slurp("$T/$_") } @$files);
    my @tot = $all =~ /(TOTAL: \d+ PASS \/ \d+ FAIL[^\n]*)/g;
    my @units = grep { /PASS \/ \d+ FAIL/ && !/TOTAL/ } split /\n/, $all; @units = @units[0..($#units < 40 ? $#units : 39)] if @units;
    my @fails = $all =~ /(\[TEST\] FAIL · [^\n]*)/g; my %s; @fails = grep { !$s{$_}++ } @fails;
    my @to = $all =~ /(TIMEOUT[^\n]*)/g; %s = (); @to = grep { !$s{$_}++ } @to;
    my @fuga = $all =~ /(FUGA[^\n]*)/g;
    my @sobra = $all =~ /((?:! SOBRA|Hay \d+ \.cs que sobran|- BORRADO)[^\n]*)/g;
    my ($at) = $all =~ /atascado > 20 s: (\d+)/; my ($vu) = $all =~ /da vueltas en círculo: (\d+)/;
    my @pasos = $all =~ /^(== [^\n]*)/mg;
    my $res = !@tot ? 'no consta' : (grep { / [1-9]\d* FAIL/ } @tot) ? 'FAIL' : 'OK';
    my $cut = sub { my $x = shift; length($x) > 400 ? substr($x, 0, 400) . '…' : $x };
    my $o = "# r$n · retro · " . substr($name, 0, 10) . ' ' . substr($name, 11, 2) . ':' . substr($name, 13, 2) . " · retro\n"
      . "Commit: $sha (reconstruido) · Rama: claude/edificios · Fusionadas: no consta · Ejecutor: probador (agente)\n"
      . "Escena: no consta (run log reconstruido en la migración, 2026-11-13); microcosmos rehechos: ver «Pasos»\n"
      . "Preparación: modo original «$mode»; fuente: " . join(', ', @$files) . " ($how)\n"
      . "Tiempo: timeScale no consta · maxDelta no consta · FPS no consta · Unity no consta · duración no consta\n"
      . "Acción a mitad: no consta\n"
      . "Resultado: $res · " . (@tot ? join(' · ', @tot) : 'sin línea TOTAL') . " · TIMEOUT: " . (@to ? scalar(@to) : 0) . " · FUGA: " . (@fuga ? scalar(@fuga) : 0) . "\n"
      . "Contadores: [Fauna] atascado>20 s " . ($at // 'no consta') . " · da vueltas " . ($vu // 'no consta') . " · el resto, no consta\n"
      . "Sobrantes: " . (@sobra ? join(' · ', map { $cut->($_) } @sobra[0..($#sobra < 5 ? $#sobra : 5)]) : 'no consta') . "\n"
      . "Interrupciones: " . (@to ? join(' · ', map { $cut->($_) } @to[0..($#to < 3 ? $#to : 3)]) : 'no consta') . "\n"
      . "Casos: los de registro.md que enlazan este run log · Hallazgos: -\n"
      . "## Fallos (filas FAIL, tal cual)\n" . (@fails ? join("\n", map { '- ' . $cut->($_) } @fails) : '- ninguno en la fuente') . "\n"
      . "## Notas\n- **Run log retro** (migración al sistema de QA): se reconstruyó de la salida del probador; lo que no estaba en ella dice «no consta».\n"
      . "- Pasos de la cadena: " . (@pasos ? join(' · ', map { s/^== //r } @pasos[0..($#pasos < 14 ? $#pasos : 14)]) : 'no consta') . "\n"
      . (@units ? "- Unidades:\n" . join("\n", map { '  - ' . $cut->(s/^\s+//r) } @units) . "\n" : '');
    open my $h, '>:encoding(UTF-8)', "docs/qa/runs/$name" or die; print $h $o; close $h; print "escrito docs/qa/runs/$name\n";
}
