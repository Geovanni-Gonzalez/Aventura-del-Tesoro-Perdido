% ==========================
% ServidorProlog.pl
% ==========================
% Servidor HTTP para conectar la lógica Prolog con C#
% Autor: Geovanni González
% Proyecto: Aventura del Tesoro Perdido

:- use_module(library(http/thread_httpd)).
:- use_module(library(http/http_dispatch)).
:- use_module(library(http/json)).
:- use_module(library(http/http_json)).

% --- Cargar los archivos de lógica ---
:- prolog_load_context(directory, Dir),
   working_directory(_, Dir).

:- consult('Estado.pl').
:- consult('BaseDeConocimiento.pl').
:- consult('Reglas.pl').

% ==========================
% Puntos de entrada HTTP
% ==========================
% Nombre: http_handler/3 (multiples usos)
% Entrada: Ruta (root/Nombre), PredicadoHandler, Opciones
% Salida: Registro interno del dispatcher HTTP
% Descripcion: Asocia cada ruta HTTP con el predicado que construye la respuesta JSON.
:- http_handler(root(estado), obtener_estado, []).
:- http_handler(root(mover), mover_lugar, []).
:- http_handler(root(usar), usar_objeto, []).
:- http_handler(root(reiniciar), reiniciar_juego, []).
:- http_handler(root(visitados), visitados, []).
:- http_handler(root(objetos_lugar), objetos_lugar, []).
:- http_handler(root(caminos), caminos, []).
:- http_handler(root(tomar), tomar_objeto, []).
:- http_handler(root(verifica_gane), verifica_gane, []).
:- http_handler(root(donde_estoy), donde_estoy_handler, []).
:- http_handler(root(que_tengo), que_tengo_handler, []).
:- http_handler(root(donde_esta), donde_esta_handler, []).
:- http_handler(root(puedo_ir), puedo_ir_handler, []).
:- http_handler(root(como_gano), como_gano_handler, []).

% ==========================
% Iniciar Servidor
% ==========================
% Nombre: iniciar_servidor/1
% Entrada: Port (número entero)
% Salida: Servidor HTTP escuchando en el puerto indicado
% Descripcion: Inicializa el servidor HTTP usando el dispatcher de librería.
iniciar_servidor(Port) :-
    http_server(http_dispatch, [port(Port)]),
    format('Servidor Prolog iniciado en puerto ~w~n', [Port]).

% ==========================
% Handlers HTTP
% ==========================

% Nombre: como_gano_handler/1
% Entrada: _Request (diccionario de la petición HTTP)
% Salida: Respuesta JSON con lista de mensajes (mensajes)
% Descripcion: Invoca el predicado como_gano y devuelve todos los mensajes acumulados.
como_gano_handler(_Request) :-
    retractall(message(_)),
    como_gano,
    findall(M, message(M), Mensajes),
    reply_json_dict(_{ mensajes: Mensajes }).

% --- Estado general ---
% Nombre: obtener_estado/1
% Entrada: _Request
% Salida: JSON con ubicacion, inventario, visitados
% Descripcion: Expone el estado actual del jugador y lugares visitados.
obtener_estado(_Request) :-
    jugador(Lugar),
    inventario(Inv),
    findall(L, lugar_visitado(L), Visitados),
    reply_json_dict(_{
        ubicacion: Lugar,
        inventario: Inv,
        visitados: Visitados
    }).

% --- Mover al jugador ---
% Nombre: mover_lugar/1
% Entrada: Request (debe contener destino)
% Salida: JSON con resultado ("ok") y mensaje (texto)
% Descripcion: Convierte el destino recibido a átomo, ejecuta mover/1 y devuelve el mensaje.
mover_lugar(Request) :-
    http_read_json_dict(Request, Data),
    ( _{ destino: DestinoStr } :< Data ->
        atom_string(Destino, DestinoStr),
        retractall(message(_)),
        mover(Destino),
        ( message(MensajeAtom) -> atom_string(MensajeAtom, MensajeStr) ; MensajeStr = "Sin mensaje." ),
        reply_json_dict(_{ resultado: "ok", mensaje: MensajeStr })
    ; reply_json_dict(_{ error: "Falta el parámetro 'destino'" }, [status(400)])
    ).

% --- Usar objeto ---
% Nombre: usar_objeto/1
% Entrada: Request (debe contener objeto)
% Salida: JSON con resultado ("ok") y mensaje
% Descripcion: Ejecuta usar/1 para el objeto indicado y devuelve el mensaje generado.
usar_objeto(Request) :-
    http_read_json_dict(Request, Data),
    ( _{ objeto: ObjetoStr } :< Data ->
        atom_string(Objeto, ObjetoStr),
        retractall(message(_)),
        usar(Objeto),
        ( message(MensajeAtom) -> atom_string(MensajeAtom, MensajeStr) ; MensajeStr = "Sin mensaje." ),
        reply_json_dict(_{ resultado: "ok", mensaje: MensajeStr })
    ; reply_json_dict(_{ error: "Falta el parámetro 'objeto'" }, [status(400)])
    ).

% --- Tomar objeto ---
% Nombre: tomar_objeto/1
% Entrada: Request (debe contener objeto)
% Salida: JSON con resultado ("ok") y mensaje
% Descripcion: Toma un objeto del lugar actual si existe y responde con el resultado.
tomar_objeto(Request) :-
    http_read_json_dict(Request, Data),
    ( _{ objeto: ObjetoStr } :< Data ->
        atom_string(Objeto, ObjetoStr),
        retractall(message(_)),
        tomar(Objeto),
        ( message(MensajeAtom) -> atom_string(MensajeAtom, MensajeStr) ; MensajeStr = "Sin mensaje." ),
        reply_json_dict(_{ resultado: "ok", mensaje: MensajeStr })
    ; reply_json_dict(_{ error: "Falta el parámetro 'objeto'" }, [status(400)])
    ).

% --- Verificar gane ---
% Nombre: verifica_gane/1
% Entrada: _Request
% Salida: JSON con mensaje (estado de victoria)
% Descripcion: Ejecuta verifica_gane/0 para confirmar si se alcanzó la condición de victoria.
verifica_gane(_Request) :-
    retractall(message(_)),
    verifica_gane,
    ( message(MensajeAtom) -> atom_string(MensajeAtom, MensajeStr) ; MensajeStr = "Estado de gane no disponible." ),
    reply_json_dict(_{ mensaje: MensajeStr }).

% --- Donde estoy ---
% Nombre: donde_estoy_handler/1
% Entrada: _Request
% Salida: JSON con mensaje (ubicación actual)
% Descripcion: Devuelve el lugar actual del jugador usando donde_estoy/0.
donde_estoy_handler(_Request) :-
    retractall(message(_)),
    donde_estoy,
    ( message(MensajeAtom) -> atom_string(MensajeAtom, MensajeStr) ; MensajeStr = "Ubicación desconocida." ),
    reply_json_dict(_{ mensaje: MensajeStr }).

% --- Que tengo ---
% Nombre: que_tengo_handler/1
% Entrada: _Request
% Salida: JSON con mensaje (inventario)
% Descripcion: Informa el inventario del jugador mediante que_tengo/0.
que_tengo_handler(_Request) :-
    retractall(message(_)),
    que_tengo,
    ( message(MensajeAtom) -> atom_string(MensajeAtom, MensajeStr) ; MensajeStr = "Inventario vacío." ),
    reply_json_dict(_{ mensaje: MensajeStr }).

% --- Donde esta objeto ---
% Nombre: donde_esta_handler/1
% Entrada: Request (debe contener objeto)
% Salida: JSON con mensaje (localización del objeto)
% Descripcion: Indica dónde se encuentra un objeto (inventario o mapa).
donde_esta_handler(Request) :-
    http_read_json_dict(Request, Data),
    ( _{ objeto: ObjetoStr } :< Data ->
        atom_string(Objeto, ObjetoStr),
        retractall(message(_)),
        donde_esta(Objeto),
        ( message(MensajeAtom) -> atom_string(MensajeAtom, MensajeStr) ; MensajeStr = "Objeto desconocido." ),
        reply_json_dict(_{ mensaje: MensajeStr })
    ; reply_json_dict(_{ error: "Falta el parámetro 'objeto'" }, [status(400)])
    ).

% --- Puedo ir ---
% Nombre: puedo_ir_handler/1
% Entrada: Request (debe contener destino)
% Salida: JSON con mensaje (resultado verificación)
% Descripcion: Valida si el jugador puede moverse a un destino según reglas y requisitos.
puedo_ir_handler(Request) :-
    http_read_json_dict(Request, Data),
    ( _{ destino: DestinoStr } :< Data ->
        atom_string(Destino, DestinoStr),
        retractall(message(_)),
        puedo_ir(Destino),
        ( message(MensajeAtom) -> atom_string(MensajeAtom, MensajeStr) ; MensajeStr = "No se puede verificar." ),
        reply_json_dict(_{ mensaje: MensajeStr })
    ; reply_json_dict(_{ error: "Falta el parámetro 'destino'" }, [status(400)])
    ).

% --- Caminos ---
% Nombre: caminos/1
% Entrada: _Request
% Salida: JSON con lista de caminos (caminos)
% Descripcion: Devuelve los destinos directamente conectados al lugar actual del jugador.
caminos(_Request) :-
    jugador(UbicacionActual),
    findall(DestinoStr,
        ( conectado(UbicacionActual, Destino),
          atom_string(Destino, DestinoStr)
        ),
        LugaresConectados),
    reply_json_dict(_{ caminos: LugaresConectados }).

% --- Lugares visitados ---
% Nombre: visitados/1
% Entrada: _Request
% Salida: JSON con lista de lugares visitados (visitados)
% Descripcion: Lista todos los lugares que el jugador ha visitado hasta el momento.
visitados(_Request) :-
    findall(L, lugar_visitado(L), Lista),
    reply_json_dict(_{ visitados: Lista }).

% --- Objetos en lugar ---
% Nombre: objetos_lugar/1
% Entrada: _Request
% Salida: JSON con lista de objetos en el lugar actual (objetos)
% Descripcion: Devuelve los objetos situados en la ubicación actual del jugador.
objetos_lugar(_Request) :-
    jugador(Lugar),
    findall(O, objeto(O, Lugar), Objetos),
    reply_json_dict(_{ objetos: Objetos }).

% --- Reiniciar juego ---
% Nombre: reiniciar_juego/1
% Entrada: _Request
% Salida: JSON con resultado y mensaje
% Descripcion: Restablece el estado del juego a sus valores iniciales.
reiniciar_juego(_Request) :-
    reiniciar_estado,
    reply_json_dict(_{ resultado: "ok", mensaje: "Juego reiniciado correctamente." }).

% ==========================
% Reinicio del estado
% ==========================
% Nombre: reiniciar_estado/0
% Entrada: (ninguna)
% Salida: Estado interno reiniciado (hechos dinámicos restaurados)
% Descripcion: Limpia todos los hechos mutables y reestablece la posición e inventario inicial.
reiniciar_estado :-
    retractall(jugador(_)),
    retractall(inventario(_)),
    retractall(objeto_usado(_)),
    retractall(lugar_visitado(_)),
    assert(jugador(bosque)),
    assert(inventario([])),
    assert(lugar_visitado(bosque)),
    retractall(message(_)),
    assert(message("Estado reiniciado.")).

% ==========================
% Inicialización
% ==========================
% Nombre: initialization/1
% Entrada: Goal (iniciar_servidor(5000))
% Salida: Ejecución de Goal al cargar el archivo
% Descripcion: Arranca automáticamente el servidor HTTP en el puerto 5000.
:- initialization(iniciar_servidor(5000)).
