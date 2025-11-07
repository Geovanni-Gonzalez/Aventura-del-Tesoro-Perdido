% Archivo principal que carga todos los modulos del juego.
% ------------------------------------------------

% --- 1. Carga la base de conocimiento (el mundo) ---
:- consult('baseDeConocimiento.pl').

% --- 2. Carga el estado inicial y dinamico ---
:- consult('estado.pl').

% --- 3. Carga las reglas y la logica del juego ---
:- consult('reglas.pl').
