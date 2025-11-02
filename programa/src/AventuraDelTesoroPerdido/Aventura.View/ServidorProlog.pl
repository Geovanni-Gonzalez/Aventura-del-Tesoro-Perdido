% ==========================================================
% 🌍 AVENTURA DEL TESORO PERDIDO (versión con mensajes estándar)
% ==========================================================

% ==== Lugares ====
lugar(bosque, "Un denso bosque lleno de sonidos extraños.").
lugar(templo, "Un antiguo templo cubierto de musgo.").
lugar(cueva, "Una cueva misteriosa y oscura.").
lugar(mar, "Una playa tranquila con arena dorada y olas suaves.").

% ==== Conexiones ====
conectado(bosque, templo).
conectado(templo, bosque).
conectado(bosque, cueva).
conectado(cueva, bosque).
conectado(cueva, templo).
conectado(templo, cueva).
conectado(templo, mar).
conectado(mar, templo).

% ==== Requisitos de acceso ====
requiere(templo, llave).
requiere(cueva, antorcha).

% ==== Objetos ====
:- dynamic objeto/2.
objeto(llave, mar).
objeto(antorcha, bosque).
objeto(oro, cueva).

% ==== Inventario ====
:- dynamic inventario/1.
inventario([]).

% ==== Jugador ====
:- dynamic jugador/1.
jugador(bosque).

% ==== Lugares visitados ====
:- dynamic lugar_visitado/1.
lugar_visitado(bosque).

% ==== Uso de objetos ====
:- dynamic usado/1.

% ==========================================================
% 🔍 Consultas
% ==========================================================

donde_esta(Objeto) :-
    objeto(Objeto, Lugar),
    write('ok: El objeto '), write(Objeto),
    write(' se encuentra en '), write(Lugar), !.
donde_esta(_) :-
    write('warn: Ese objeto no existe o ya fue tomado.').

que_tengo :-
    inventario(Inv),
    ( Inv = [] ->
        write('warn: No tienes ningún objeto.')
    ; write('ok: Inventario actual: '), write(Inv)
    ), !.

lugares_visitados :-
    findall(L, lugar_visitado(L), Lista),
    write('ok: Lugares visitados: '), write(Lista), !.

% ==========================================================
% 🧭 Movimiento y validaciones
% ==========================================================

puedo_ir(Destino) :-
    jugador(Desde),
    (   \+ lugar(Destino, _) ->
        write('error: El lugar no existe.'), !, fail
    ;   \+ conectado(Desde, Destino) ->
        write('warn: No hay un camino directo desde '), write(Desde),
        write(' hasta '), write(Destino), !, fail
    ;   requiere(Destino, Objeto),
        \+ (inventario(Inv), member(Objeto, Inv), usado(Objeto)) ->
        write('warn: No puedes entrar a '), write(Destino),
        write(' sin usar el objeto requerido: '), write(Objeto), !, fail
    ;   write('ok: Puedes ir de '), write(Desde), write(' a '), write(Destino)
    ).

mover(Destino) :-
    puedo_ir(Destino),
    jugador(Desde),
    retract(jugador(Desde)),
    assert(jugador(Destino)),
    ( \+ lugar_visitado(Destino) -> assert(lugar_visitado(Destino)) ; true ),
    write('ok: Te moviste a '), write(Destino),
    verifica_gane, !.
mover(_) :-
    write('error: Movimiento no permitido o destino inválido.').

% ==========================================================
% 🎒 Objetos
% ==========================================================

tomar(Objeto) :-
    jugador(Lugar),
    objeto(Objeto, Lugar),
    retract(objeto(Objeto, Lugar)),
    inventario(Inv),
    retract(inventario(Inv)),
    assert(inventario([Objeto|Inv])),
    write('ok: Tomaste el objeto '), write(Objeto), !.
tomar(Objeto) :-
    jugador(Lugar),
    \+ objeto(Objeto, Lugar),
    write('warn: No hay ningún objeto llamado '), write(Objeto),
    write(' en '), write(Lugar).

usar(Objeto) :-
    inventario(Inv),
    member(Objeto, Inv),
    ( \+ usado(Objeto) ->
        assert(usado(Objeto)),
        write('ok: Has usado el objeto '), write(Objeto)
    ; write('warn: Ya habías usado el objeto '), write(Objeto)
    ), !.
usar(Objeto) :-
    write('warn: No tienes el objeto '), write(Objeto).

% ==========================================================
% 🧩 Rutas lógicas
% ==========================================================

ruta(Inicio, Fin, Camino) :-
    ruta_aux(Inicio, Fin, [Inicio], CaminoInverso),
    reverse(CaminoInverso, Camino),
    write('ok: Ruta encontrada: '), write(Camino), !.
ruta(_, _, _) :-
    write('warn: No hay ruta posible entre esos lugares.').

ruta_aux(Fin, Fin, Camino, Camino).
ruta_aux(Inicio, Fin, Visitados, Camino) :-
    conectado(Inicio, Siguiente),
    \+ member(Siguiente, Visitados),
    ruta_aux(Siguiente, Fin, [Siguiente|Visitados], Camino).

% ==========================================================
% 🏆 Gane
% ==========================================================

tesoro(cueva, oro).

verifica_gane :-
    jugador(Lugar),
    inventario(Inv),
    tesoro(Lugar, Objeto),
    member(Objeto, Inv),
    nl, write('ok: ¡Has ganado el juego! Lugar: '), write(Lugar),
    write(', Objeto: '), write(Objeto), !.
verifica_gane :- true.

% ==========================================================
% 🔄 Reinicio
% ==========================================================
reiniciar :-
    retractall(jugador(_)), assert(jugador(bosque)),
    retractall(inventario(_)), assert(inventario([])),
    retractall(objeto(_, _)),
    assert(objeto(llave, mar)),
    assert(objeto(antorcha, bosque)),
    assert(objeto(oro, cueva)),
    retractall(lugar_visitado(_)),
    assert(lugar_visitado(bosque)),
    retractall(usado(_)),
    write('ok: Juego reiniciado, estás en el bosque.').


lugares(L) :- findall(Nombre, lugar(Nombre, _), L).
objetos(O) :- findall(Nombre, objeto(Nombre, _), O).
descripcion_lugar(Lugar, Descripcion) :- lugar(Lugar, Descripcion).
conexion(Desde, Hacia) :- conectado(Desde, Hacia).