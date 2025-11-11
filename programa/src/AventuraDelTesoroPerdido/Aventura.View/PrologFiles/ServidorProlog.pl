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
:- http_handler(root(guardar_repeticion), guardar_repeticion_handler, []).
:- http_handler(root(reproducir_repeticion), reproducir_repeticion_handler, []).

% ==========================
% Iniciar Servidor
% ==========================
iniciar_servidor(Port) :-
    http_server(http_dispatch, [port(Port)]),
    format('✅ Servidor Prolog iniciado en puerto ~w~n', [Port]).

% ==========================
% Handlers HTTP
% ==========================

% Guardar repetición en archivo
guardar_repeticion_handler(_Request) :-
    findall(A, accion(A), Acciones),
    open('repeticion.txt', write, Stream),
    forall(member(M, Acciones), writeln(Stream, M)),
    close(Stream),
    reply_json_dict(_{ mensaje: 'Repetición guardada en repeticion.txt' }).

% Reproducir repetición desde archivo
reproducir_repeticion_handler(_Request) :-
    exists_file('repeticion.txt'),
    open('repeticion.txt', read, Stream),
    findall(Line, read_line_to_string(Stream, Line), Lineas),
    close(Stream),
    reply_json_dict(_{ repeticion: Lineas }).

como_gano_handler(_Request) :-
    retractall(message(_)),
    como_gano,
    findall(M, message(M), Mensajes),
    reply_json_dict(_{ mensajes: Mensajes }).

% --- Estado general ---
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
verifica_gane(_Request) :-
    retractall(message(_)),
    verifica_gane,
    ( message(MensajeAtom) -> atom_string(MensajeAtom, MensajeStr) ; MensajeStr = "Estado de gane no disponible." ),
    reply_json_dict(_{ mensaje: MensajeStr }).

% --- Donde estoy ---
donde_estoy_handler(_Request) :-
    retractall(message(_)),
    donde_estoy,
    ( message(MensajeAtom) -> atom_string(MensajeAtom, MensajeStr) ; MensajeStr = "Ubicación desconocida." ),
    reply_json_dict(_{ mensaje: MensajeStr }).

% --- Que tengo ---
que_tengo_handler(_Request) :-
    retractall(message(_)),
    que_tengo,
    ( message(MensajeAtom) -> atom_string(MensajeAtom, MensajeStr) ; MensajeStr = "Inventario vacío." ),
    reply_json_dict(_{ mensaje: MensajeStr }).

% --- Donde esta objeto ---
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
caminos(_Request) :-
    jugador(UbicacionActual),
    findall(DestinoStr,
        ( conectado(UbicacionActual, Destino),
          atom_string(Destino, DestinoStr)
        ),
        LugaresConectados),
    reply_json_dict(_{ caminos: LugaresConectados }).

% --- Lugares visitados ---
visitados(_Request) :-
    findall(L, lugar_visitado(L), Lista),
    reply_json_dict(_{ visitados: Lista }).

% --- Objetos en lugar ---
objetos_lugar(_Request) :-
    jugador(Lugar),
    findall(O, objeto(O, Lugar), Objetos),
    reply_json_dict(_{ objetos: Objetos }).

% --- Reiniciar juego ---
reiniciar_juego(_Request) :-
    reiniciar_estado,
    reply_json_dict(_{ resultado: "ok", mensaje: "Juego reiniciado correctamente." }).

% ==========================
% Reinicio del estado
% ==========================
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
:- initialization(iniciar_servidor(5000)).
