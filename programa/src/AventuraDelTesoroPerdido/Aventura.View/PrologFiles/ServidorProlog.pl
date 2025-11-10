% ==========================
% ServidorProlog.pl
% ==========================
% Servidor HTTP para conectar la lógica Prolog con C#
% Autor: Geovanni González
% Proyecto: Aventura del Tesoro Perdido

:- use_module(library(http/thread_httpd)).
:- use_module(library(http/http_dispatch)).
:- use_module(library(http/http_json)).
:- use_module(library(http/json_convert)).

% --- Cargar los otros archivos de lógica ---
:- prolog_load_context(directory, Dir),
   working_directory(_, Dir).

:- consult('Estado.pl').
:- consult('BaseDeConocimiento.pl').
:- consult('Reglas.pl').

% ==========================
% Puntos de entrada HTTP
% ==========================
:- http_handler(root(estado), obtener_estado, []).
:- http_handler(root(mover), mover_personaje, []).
:- http_handler(root(usar), usar_objeto, []).
:- http_handler(root(reiniciar), reiniciar_juego, []).

% ==========================
% Iniciar Servidor
% ==========================
iniciar_servidor(Port) :-
    http_server(http_dispatch, [port(Port)]),
    format('? Servidor Prolog iniciado en puerto ~w~n', [Port]).

% ==========================
% Endpoints HTTP
% ==========================

% --- Consultar estado actual ---
obtener_estado(_Request) :-
    estado_actual(Estado),
    reply_json_dict(_{ estado: Estado }).

% --- Mover al jugador a otra ubicación ---
mover_personaje(Request) :-
    http_read_json_dict(Request, Data),
    (   _{ destino: Destino } :< Data ->
        (   mover_a(Destino, Resultado),
            reply_json_dict(_{ resultado: Resultado })
        )
    ;   reply_json_dict(_{ error: 'Falta el parámetro destino' }, [status(400)])
    ).

% --- Usar un objeto en el entorno ---
usar_objeto(Request) :-
    http_read_json_dict(Request, Data),
    (   _{ objeto: Objeto } :< Data ->
        (   usar(Objeto, Resultado),
            reply_json_dict(_{ resultado: Resultado })
        )
    ;   reply_json_dict(_{ error: 'Falta el parámetro objeto' }, [status(400)])
    ).

% --- Reiniciar el juego ---
reiniciar_juego(_Request) :-
    reiniciar_estado,
    reply_json_dict(_{ mensaje: 'Juego reiniciado correctamente.' }).

% ==========================
% Predicados base (si no existen en tus otros archivos)
% ==========================

% Si Estado.pl ya define estos, puedes omitir esta sección.

:- dynamic estado_actual/1.
estado_actual(inicio).

mover_a(Destino, ok) :-
    retractall(estado_actual(_)),
    asserta(estado_actual(Destino)), !.
mover_a(_, error).

usar(_Objeto, usado).

reiniciar_estado :-
    retractall(estado_actual(_)),
    asserta(estado_actual(inicio)).

% ==========================
% Ejecución automática
% ==========================
:- initialization(iniciar_servidor(5000)).
