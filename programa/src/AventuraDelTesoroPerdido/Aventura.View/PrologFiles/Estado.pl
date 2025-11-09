% Estado.pl

% Este archivo declara que hechos que pueden cambiar
% y define el estado inicial del juego
% ------------------------------------------------

% ==== Declaraciones Dinamicas ====
% Le decimos a Prolog que estos hechos cambiaran
% durante el juego (seran 'assert'ados y 'retract'ados)

% Donde esta el jugador
:- dynamic jugador/1.

% Que objetos tiene el jugador
:- dynamic inventario/1.

% Donde estan los objetos (para poder tomarlos/quitarlos del mundo)
:- dynamic objeto/2.

% Que objetos ha usado el jugador (para desbloquear puertas)
:- dynamic objeto_usado/1.

% Que lugares ha visitado el jugador (para 'requiereVisita' y 'verifica_gane')
:- dynamic lugar_visitado/1.

% Guarda el ultimo mensaje para C lo lea
:- dynamic message/1.

% ==== Estado Inicial del Juego ====
% Estos son los hechos con los que el juego comienza
% En 'reglas.pl' se modificara estos hechos

% El jugador comienza en el 'bosque'
jugador(bosque).

% El jugador comienza con el inventario vacio
inventario([]).

% El primer lugar visitado es el 'bosque' (donde empieza)
lugar_visitado(bosque).