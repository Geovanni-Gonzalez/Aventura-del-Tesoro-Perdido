% ==========================
% Reglas.pl
% ==========================

% ---- Consultas ----

donde_estoy :-
    jugador(Lugar),
    atom_concat('Estas en ', Lugar, Mensaje),
    assertz(message(Mensaje)).

que_tengo :-
    inventario(Lista),
    ( Lista = [] -> Mensaje = 'No tienes ningun objeto.'
    ; Mensaje = Lista ),
    assertz(message(Mensaje)).

lugar_visitados :-
    findall(L, lugar_visitado(L), Lista),
    assertz(message(Lista)).

donde_esta(Objeto) :-
    inventario(Inv),
    member(Objeto, Inv), !,
    atom_concat(Objeto, ' esta en tu inventario.', Mensaje),
    assertz(message(Mensaje)).
donde_esta(Objeto) :-
    objeto(Objeto, Lugar), !,
    atom_concat(Objeto, ' esta en ', Temp),
    atom_concat(Temp, Lugar, Mensaje),
    assertz(message(Mensaje)).
donde_esta(Objeto) :-
    atom_concat(Objeto, ' no se encuentra en el juego.', Mensaje),
    assertz(message(Mensaje)),
    fail.

% ---- Verificación ----

puedo_ir(Destino) :-
    jugador(Desde),
    \+ conectado(Desde, Destino), !,
    atom_concat('No hay conexion directa hacia ', Destino, Mensaje),
    assertz(message(Mensaje)).

puedo_ir(Destino) :-
    requiere(Objeto, Destino),
    \+ objeto_usado(Objeto), !,
    atom_concat('Te falta usar el objeto: ', Objeto, Mensaje),
    assertz(message(Mensaje)).

puedo_ir(Destino) :-
    atom_concat('Puedes ir a ', Destino, Mensaje),
    assertz(message(Mensaje)).

% ---- Acciones ----

tomar(Objeto) :-
    jugador(Lugar),
    objeto(Objeto, Lugar),              
    retract(objeto(Objeto, Lugar)),
    inventario(Inv),
    retract(inventario(Inv)),
    assert(inventario([Objeto|Inv])),
    atom_concat('Tomaste el objeto: ', Objeto, Mensaje),
    assertz(message(Mensaje)), !.

tomar(Objeto) :-
    atom_concat(Objeto, ' no esta en este lugar.', Mensaje),
    assertz(message(Mensaje)).

usar(Objeto) :-
    inventario(Inv),
    \+ member(Objeto, Inv), !,
    atom_concat('No tienes el objeto: ', Objeto, Mensaje),
    assertz(message(Mensaje)).

usar(Objeto) :-
    objeto_usado(Objeto), !,
    atom_concat('Ya habias usado: ', Objeto, Mensaje),
    assertz(message(Mensaje)).

usar(Objeto) :-
    assert(objeto_usado(Objeto)),
    atom_concat('Usaste el objeto: ', Objeto, Mensaje),
    assertz(message(Mensaje)).

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
    (lugar_visitado(Destino) -> true ; assert(lugar_visitado(Destino))),
    atom_concat('Te moviste a ', Destino, Mensaje),
    assertz(message(Mensaje)).

% ---- Rutas y Victoria ----

ruta(Inicio, Fin, Camino) :-
    ruta_aux(Inicio, Fin, [Inicio], CaminoInv),
    reverse(CaminoInv, Camino).

ruta_aux(Fin, Fin, Visitados, Visitados).
ruta_aux(Actual, Fin, Visitados, Camino) :-
    conectado(Actual, Vecino),
    \+ member(Vecino, Visitados),
    ruta_aux(Vecino, Fin, [Vecino|Visitados], Camino).

como_gano :-
    jugador(LugarActual),
    forall(
        tesoro(LugarGane, ObjetoGane),
        (
            ( ruta(LugarActual, LugarGane, Camino) -> atomic_list_concat(Camino,' -> ',CaminoStr)
            ; CaminoStr = 'No se encontro ruta.'),
            format(atom(Mensaje),'Objetivo: Conseguir "~w" y llevarlo a "~w". Ruta sugerida: ~w',
                   [ObjetoGane,LugarGane,CaminoStr]),
            assertz(message(Mensaje))
        )
    ).

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
