% ======================================================
% Reglas.pl - Lógica y acciones del juego Aventura
% ======================================================

% ----------------------
% Consultas
% ----------------------

% Ubicación actual del jugador
donde_estoy :-
    jugador(Lugar),
    atom_concat('Estas en ', Lugar, Mensaje),
    assertz(message(Mensaje)).

% Inventario actual del jugador
que_tengo :-
    inventario(Inv),
    ( Inv = [] ->
        Mensaje = 'No tienes ningun objeto.'
    ;
        atomic_list_concat(Inv, ', ', ListaStr),
        atom_concat('Tienes: ', ListaStr, Mensaje)
    ),
    assertz(message(Mensaje)).

% Lugares visitados
lugar_visitados :-
    findall(L, lugar_visitado(L), Lista),
    atomic_list_concat(Lista, ', ', ListaStr),
    atom_concat('Has visitado: ', ListaStr, Mensaje),
    assertz(message(Mensaje)).

% Dónde está un objeto
donde_esta(Objeto) :-
    inventario(Inv),
    member(Objeto, Inv), !,
    atom_concat(Objeto, ' está en tu inventario.', Mensaje),
    assertz(message(Mensaje)).
donde_esta(Objeto) :-
    objeto(Objeto, Lugar), !,
    atom_concat(Objeto, ' está en ', Temp),
    atom_concat(Temp, Lugar, Mensaje),
    assertz(message(Mensaje)).
donde_esta(Objeto) :-
    atom_concat(Objeto, ' no se encuentra en el juego.', Mensaje),
    assertz(message(Mensaje)).

% ----------------------
% Verificaciones
% ----------------------

% Verifica si el jugador puede ir a un destino
puedo_ir(Destino) :-
    jugador(Desde),
    \+ conectado(Desde, Destino), !,
    atom_concat('No hay conexión directa hacia ', Destino, Mensaje),
    assertz(message(Mensaje)).

puedo_ir(Destino) :-
    requiere(Objeto, Destino),
    \+ objeto_usado(Objeto), !,
    atom_concat('Te falta usar el objeto: ', Objeto, Mensaje),
    assertz(message(Mensaje)).

puedo_ir(Destino) :-
    atom_concat('Sí puedes ir a ', Destino, Mensaje),
    assertz(message(Mensaje)).

% ----------------------
% Acciones
% ----------------------

% Tomar un objeto del lugar actual
tomar(Objeto) :-
    jugador(Lugar),
    objeto(Objeto, Lugar), !,
    retract(objeto(Objeto, Lugar)),
    inventario(Inv),
    retract(inventario(Inv)),
    assert(inventario([Objeto|Inv])),
    atom_concat('Tomaste el objeto: ', Objeto, Mensaje),
    assertz(message(Mensaje)).

tomar(Objeto) :-
    atom_concat(Objeto, ' no está en este lugar.', Mensaje),
    assertz(message(Mensaje)).

% Usar un objeto
usar(Objeto) :-
    inventario(Inv),
    \+ member(Objeto, Inv), !,
    atom_concat('No tienes el objeto: ', Objeto, Mensaje),
    assertz(message(Mensaje)).

usar(Objeto) :-
    objeto_usado(Objeto), !,
    atom_concat('Ya habías usado: ', Objeto, Mensaje),
    assertz(message(Mensaje)).

usar(Objeto) :-
    assert(objeto_usado(Objeto)),
    atom_concat('Usaste el objeto: ', Objeto, Mensaje),
    assertz(message(Mensaje)).

% Mover jugador
mover(Destino) :-
    jugador(Desde),
    \+ conectado(Desde, Destino), !,
    atom_concat('No hay conexion directa hacia ', Destino, Mensaje),
    assertz(message(Mensaje)).

mover(Destino) :-
    requiere(Objeto, Destino),
    \+ objeto_usado(Objeto), !,
    atom_concat('Necesitas USAR el objeto: ', Objeto, Mensaje),
    assertz(message(Mensaje)).

mover(Destino) :-
    jugador(Desde),
    retract(jugador(Desde)),
    assert(jugador(Destino)),
    ( lugar_visitado(Destino) -> true ; assert(lugar_visitado(Destino)) ),
    atom_concat('Te moviste a ', Destino, Mensaje),
    assertz(message(Mensaje)).

% ----------------------
% Lógica de rutas y victoria
% ----------------------

ruta(Inicio, Fin, Camino) :-
    ruta_aux(Inicio, Fin, [Inicio], CaminoInv),
    reverse(CaminoInv, Camino).

ruta_aux(Fin, Fin, Visitados, Visitados).
ruta_aux(Actual, Fin, Visitados, Camino) :-
    conectado(Actual, Vecino),
    \+ member(Vecino, Visitados),
    ruta_aux(Vecino, Fin, [Vecino|Visitados], Camino).

como_gano :-
    tesoro(LugarGane, ObjetoGane),
    jugador(LugarActual),
    ( ruta(LugarActual, LugarGane, Camino) -> true ; Camino = [] ),
    assertz(message('Ruta de gane mostrada en consola.')).

verifica_gane :-
    jugador(Lugar),
    inventario(Inv),
    tesoro(Lugar, Objeto),
    member(Objeto, Inv), !,
    findall(Visitado, lugar_visitado(Visitado), Camino),
    Mensaje = 'Felicidades! Ganaste el juego.',
    assertz(message(Mensaje)),
    retractall(message_debug(_)),
    assertz(message_debug(lugar(Lugar))),
    assertz(message_debug(inventario(Inv))),
    assertz(message_debug(camino(Camino))).

verifica_gane :-
    Mensaje = 'Aun no cumples las condiciones para ganar.',
    assertz(message(Mensaje)).
