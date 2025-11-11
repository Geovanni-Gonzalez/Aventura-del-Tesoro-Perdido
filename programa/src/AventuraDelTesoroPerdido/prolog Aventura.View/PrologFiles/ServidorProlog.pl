% ==========================
% ServidorProlog.pl
% ==========================
% Servidor HTTP para conectar la lógica Prolog con C#
% Autor: Geovanni González
% Proyecto: Aventura del Tesoro Perdido

:- use_module(library(http/thread_httpd)).
:- use_module(library(http/http_dispatch)).
:- use_module(library(http/http_json)).

% --- Cargar los otros archivos de lógica ---
:- prolog_load_context(directory, Dir),
   working_directory(_, Dir).

:- consult('Estado.pl').
:- consult('BaseDeConocimiento.pl').
:- consult('Reglas.pl').

% ==========================
% Helpers
% ==========================
% Normaliza valores de JSON (string) a átomo para usarlos en reglas que esperan átomos.
normalize_json_atom(JsonVal, Atom) :-
    (   string(JsonVal)
    ->  atom_string(Atom, JsonVal)
    ;   Atom = JsonVal  % ya puede venir como átomo
    ).

% Convierte excepciones a texto
error_to_message(Error, Mensaje) :-
    catch(message_to_string(Error, Mensaje), _, Mensaje = 'Error interno del servidor').

% ==========================
% Puntos de entrada HTTP
% ==========================
:- http_handler(root(estado), obtener_estado, []).
:- http_handler(root(mover), mover_personaje, []).
:- http_handler(root(usar), usar_objeto, []).
:- http_handler(root(reiniciar), reiniciar_juego, []).
:- http_handler(root(visitados), visitados, []).
:- http_handler(root(objetos_lugar), objetos_lugar, []).
:- http_handler(root(caminos), caminos, []).
:- http_handler(root(tomar), tomar_objeto, []).

% ==========================
% Iniciar Servidor
% ==========================
iniciar_servidor(Port) :-
    http_server(http_dispatch, [port(Port)]),
    format('Servidor Prolog iniciado en puerto ~w~n', [Port]).

% ==========================
% Endpoints HTTP
% ==========================

tomar_objeto(Request) :-
    http_read_json_dict(Request, Data),
    (   _{ objeto: Obj0 } :< Data
    ->  normalize_json_atom(Obj0, Objeto),
        retractall(message(_)),
        catch(
            ( (tomar(Objeto) -> Tipo = ok ; Tipo = error),
              ( message(Mensaje) -> true ; Mensaje = "Sin mensaje" ),
              reply_json_dict(_{ resultado: Tipo, mensaje: Mensaje })
            ),
            Error,
            ( error_to_message(Error, MensajeErr),
              reply_json_dict(_{ resultado: "error", mensaje: MensajeErr })
            )
        )
    ;   reply_json_dict(_{ error: "Falta el parámetro 'objeto'" }, [status(400)])
    ).

% --- Caminos posibles desde la ubicación actual ---
caminos(_Request) :-
    jugador(UbicacionActual),
    findall(DestinoStr,
            ( conectado(UbicacionActual, Destino),
              atom_string(Destino, DestinoStr)  % <-- convierte átomo a string
            ),
            LugaresConectados),
    reply_json_dict(_{caminos: LugaresConectados}).

% --- Lugares visitados ---
visitados(_Request) :-
    findall(L, lugar_visitado(L), Lista),
    reply_json_dict(_{visitados: Lista}).

% --- Objetos en el lugar actual ---
objetos_lugar(_Request) :-
    jugador(Lugar),
    findall(O, objeto(O, Lugar), Objetos),
    reply_json_dict(_{objetos: Objetos}).

% --- Consultar estado actual ---
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
mover_personaje(Request) :-
    http_read_json_dict(Request, Data),
    (   _{ destino: Dest0 } :< Data
    ->  normalize_json_atom(Dest0, Destino),
        retractall(message(_)),
        catch(
            ( (mover(Destino) -> Tipo = ok ; Tipo = error),
              ( message(Mensaje) -> true ; Mensaje = "Sin mensaje" ),
              ( Tipo == ok -> catch(verifica_gane, _, true) ; true ),
              reply_json_dict(_{ resultado: Tipo, mensaje: Mensaje })
            ),
            Error,
            ( error_to_message(Error, MensajeErr),
              reply_json_dict(_{ resultado: "error", mensaje: MensajeErr })
            )
        )
    ;   reply_json_dict(_{ error: "Falta el parámetro 'destino'" }, [status(400)])
    ).

% --- Usar un objeto ---
usar_objeto(Request) :-
    http_read_json_dict(Request, Data),
    (   _{ objeto: Obj0 } :< Data
    ->  normalize_json_atom(Obj0, Objeto),
        retractall(message(_)),
        catch(
            ( (usar(Objeto) -> Tipo = ok ; Tipo = error),
              ( message(Mensaje) -> true ; Mensaje = "Sin mensaje" ),
              reply_json_dict(_{ resultado: Tipo, mensaje: Mensaje })
            ),
            Error,
            ( error_to_message(Error, MensajeErr),
              reply_json_dict(_{ resultado: "error", mensaje: MensajeErr })
            )
        )
    ;   reply_json_dict(_{ error: "Falta el parámetro 'objeto'" }, [status(400)])
    ).

% --- Reiniciar el juego ---
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
% Ejecución automática
% ==========================
:- initialization(iniciar_servidor(5000)).