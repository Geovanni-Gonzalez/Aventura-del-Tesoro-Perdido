% ==========================
% Reglas.pl
% ==========================

% ---- Consultas ----

% Nombre: donde_estoy/0
% Entrada: (ninguna)
% Salida: Registra message(Mensaje) con la ubicación actual del jugador
% Descripcion: Obtiene el lugar actual del jugador y construye un mensaje descriptivo.
donde_estoy :-
    jugador(Lugar),
    atom_concat('Estas en ', Lugar, Mensaje),
    assertz(message(Mensaje)).

% Nombre: que_tengo/0
% Entrada: (ninguna)
% Salida: Registra message(Mensaje) con el inventario (lista) o un texto si está vacío
% Descripcion: Informa el contenido actual del inventario del jugador.
que_tengo :-
    inventario(Lista),
    ( Lista = [] -> Mensaje = 'No tienes ningun objeto.'
    ; Mensaje = Lista ),
    assertz(message(Mensaje)).

% Nombre: lugar_visitados/0
% Entrada: (ninguna)
% Salida: Registra message(Lista) con todos los lugares visitados
% Descripcion: Devuelve la lista de lugares que el jugador ha visitado.
lugar_visitados :-
    findall(L, lugar_visitado(L), Lista),
    assertz(message(Lista)).

% Nombre: donde_esta/1
% Entrada: Objeto (átomo)
% Salida: Registra message(Mensaje) con la localización del objeto; falla si no existe
% Descripcion: Indica si el objeto está en el inventario, en algún lugar del mapa o no existe.
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

% Nombre: puedo_ir/1
% Entrada: Destino (átomo)
% Salida: Registra message(Mensaje) con el resultado de la verificación
% Descripcion: Valida si existe conexión, requisitos de uso de objetos, etc., para ir al destino.
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

% Nombre: tomar/1
% Entrada: Objeto (átomo)
% Salida: Registra message(Mensaje) con el resultado de tomar; actualiza inventario/objeto
% Descripcion: Toma un objeto del lugar actual, lo agrega al inventario y lo quita del mapa.
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

% Nombre: usar/1
% Entrada: Objeto (átomo)
% Salida: Registra message(Mensaje); marca objeto_usado/1 cuando aplica
% Descripcion: Usa un objeto del inventario para habilitar accesos o cumplir requisitos.
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

% Nombre: mover/1
% Entrada: Destino (átomo)
% Salida: Registra message(Mensaje); actualiza jugador/1 y lugar_visitado/1
% Descripcion: Mueve al jugador al destino si hay conexión y se cumplen requisitos.
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

% Nombre: ruta/3
% Entrada: Inicio (átomo), Fin (átomo), Camino (lista de átomos - salida)
% Salida: Camino con la secuencia de lugares desde Inicio hasta Fin (si existe)
% Descripcion: Busca una ruta simple usando backtracking sobre conexiones.
ruta(Inicio, Fin, Camino) :-
    ruta_aux(Inicio, Fin, [Inicio], CaminoInv),
    reverse(CaminoInv, Camino).

% Nombre: ruta_aux/4
% Entrada: Actual (átomo), Fin (átomo), Visitados (lista), Camino (lista - salida)
% Salida: Camino acumulado en orden inverso
% Descripcion: DFS recursivo que evita ciclos con la lista de visitados.
ruta_aux(Fin, Fin, Visitados, Visitados).
ruta_aux(Actual, Fin, Visitados, Camino) :-
    conectado(Actual, Vecino),
    \+ member(Vecino, Visitados),
    ruta_aux(Vecino, Fin, [Vecino|Visitados], Camino).

% Nombre: como_gano/0
% Entrada: (ninguna)
% Salida: Registra message(Mensaje) con el objetivo y ruta sugerida para cada tesoro
% Descripcion: Informa condiciones de victoria y una ruta sugerida (si existe) desde la ubicación actual.
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

% Nombre: verifica_gane/0
% Entrada: (ninguna)
% Salida: Registra message(Mensaje) indicando éxito o estado pendiente
% Descripcion: Comprueba si el jugador cumple las condiciones de victoria (lugar y objeto requerido).
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
    
