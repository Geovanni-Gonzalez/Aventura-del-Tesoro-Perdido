% Reglas.pl

% Este archivo se definen la logica, las acciones y las consultas del juego.
% ------------------------------------------------

% ==== Consultas ====
% Informa la ubicacion actual del jugador.
donde_estoy :-
    jugador(Lugar),
    atom_concat('Estas en ', Lugar, Mensaje), % Une 'Estas en ' + 'bosque'
    write(Mensaje), nl,
    assertz(message(Mensaje)). % Guarda el mensaje para C#

% Informa el inventario actual del jugador.
que_tengo :-
    inventario(Lista),
    (   Lista = [] ->
        Mensaje = 'No tienes ningun objeto.'
    ;   
        % Guardamos la lista y C# le pondrá el "Tienes: "
        Mensaje = Lista 
    ),
    (   Lista = [] -> write(Mensaje) ; write('Tienes: '), write(Lista) ),
    nl,
    assertz(message(Mensaje)). % Guarda el mensaje/lista para C#

% Informa los lugares que el jugador ha visitado.
lugar_visitados :-
    findall(L, lugar_visitado(L), Lista),
    write('Has visitado: '), write(Lista), nl,
    assertz(message(Lista)). % Guarda la lista para C#


% Informa donde esta un objeto (en el mundo o inventario).
donde_esta(Objeto) :-
    inventario(Inv),
    member(Objeto, Inv), !,
    atom_concat(Objeto, ' esta en tu inventario.', Mensaje),
    write(Mensaje), nl,
    assertz(message(Mensaje)).
donde_esta(Objeto) :-
    objeto(Objeto, Lugar), !,
    atom_concat(Objeto, ' esta en ', Temp),
    atom_concat(Temp, Lugar, Mensaje), % Une 'llave' + ' esta en ' + 'bosque'
    write(Mensaje), nl,
    assertz(message(Mensaje)).
donde_esta(Objeto) :-
    atom_concat(Objeto, ' no se encuentra en el juego.', Mensaje),
    write(Mensaje), nl,
    assertz(message(Mensaje)),
    fail.


% ==== Verificacion ====
% Verifica si el jugador puede moverse a un lugar.
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

puedo_ir(Destino) :-                     % Caso exitoso
    atom_concat('Sí puedes ir a ', Destino, Mensaje),
    assertz(message(Mensaje)).


% ==== acciones ====
% Permite al jugador tomar un objeto del lugar actual
tomar(Objeto) :-
    jugador(Lugar),
    objeto(Objeto, Lugar),              % Si el objeto está en el lugar
    retract(objeto(Objeto, Lugar)),
    inventario(Inv),
    retract(inventario(Inv)),
    assert(inventario([Objeto|Inv])),
    atom_concat('Tomaste el objeto: ', Objeto, Mensaje),
    assertz(message(Mensaje)), !.

tomar(Objeto) :-                        % Caso objeto no está en lugar
    atom_concat(Objeto, ' no está en este lugar.', Mensaje),
    assertz(message(Mensaje)).


% Permite al jugador usar un objeto de su inventario.
usar(Objeto) :-
    inventario(Inv),
    \+ member(Objeto, Inv),            % Si no está en inventario
    atom_concat('No tienes el objeto: ', Objeto, Mensaje),
    assertz(message(Mensaje)), !.

usar(Objeto) :-
    objeto_usado(Objeto),              % Si ya se usó
    atom_concat('Ya habías usado: ', Objeto, Mensaje),
    assertz(message(Mensaje)), !.

usar(Objeto) :-                          % Caso exitoso
    assert(objeto_usado(Objeto)),
    atom_concat('Usaste el objeto: ', Objeto, Mensaje),
    assertz(message(Mensaje)).


% Mueve al jugador a un nuevo lugar si cumple los requisitos.
mover(Destino) :-
    jugador(Desde),
    \+ conectado(Desde, Destino),
    Mensaje = 'No hay conexion directa.',
    assertz(message(Mensaje)), !.

mover(Destino) :-
    requiere(Objeto, Destino),
    \+ objeto_usado(Objeto),
    atom_concat('Necesitas USAR el objeto: ', Objeto, Mensaje),
    assertz(message(Mensaje)), !.

mover(Destino) :-
    jugador(Desde),
    retract(jugador(Desde)),
    assert(jugador(Destino)),
    (lugar_visitado(Destino) -> true ; assert(lugar_visitado(Destino))),
    atom_concat('Te moviste a ', Destino, Mensaje),
    assertz(message(Mensaje)).


% ==== LOGICA DE RUTA Y VICTORIA ====
% (Esta parte la dejamos igual porque 'write' es suficiente)

ruta(Inicio, Fin, Camino) :-
    ruta_aux(Inicio, Fin, [Inicio], CaminoInvertido),
    reverse(CaminoInvertido, Camino).

ruta_aux(Fin, Fin, Visitados, Visitados).
ruta_aux(Actual, Fin, Visitados, Camino) :-
    conectado(Actual, Vecino),
    \+ member(Vecino, Visitados),
    ruta_aux(Vecino, Fin, [Vecino|Visitados], Camino).

como_gano :-
    tesoro(LugarGane, ObjetoGane),
    jugador(LugarActual),
    write('--- Opcion de Victoria ---'), nl,
    write('Objetivo: Conseguir '), write(ObjetoGane), write(' y llevarlo a '), write(LugarGane), nl,
    write('Ruta simple (no valida requisitos): '),
    (   ruta(LugarActual, LugarGane, Camino) ->
        write(Camino), nl
    ;   write('No se encontro una ruta simple.'), nl
    ),
    write('--------------------------'), nl,
    % Este predicado solo informa, no necesita pasar mensaje a C#
    assertz(message('Ruta de gane mostrada en consola.')).

verifica_gane :-
    jugador(Lugar),
    inventario(Inv),
    tesoro(Lugar, Objeto),
    member(Objeto, Inv), !,
    findall(Visitado, lugar_visitado(Visitado), Camino),
    % Es muy complejo unir todo esto sin 'format', así que
    % simplemente guardamos un mensaje de éxito.
    Mensaje = 'Felicidades! Ganaste el juego.',
    write(Mensaje), nl,
    write('Lugar: '), write(Lugar), nl,
    write('Inventario: '), write(Inv), nl,
    write('Camino: '), write(Camino), nl,
    assertz(message(Mensaje)).
verifica_gane :-
    Mensaje = 'Aun no cumples las condiciones para ganar.',
    write(Mensaje), nl,
    assertz(message(Mensaje)),
    fail.