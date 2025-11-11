% Estado.pl

% Este archivo declara que hechos que pueden cambiar
% y define el estado inicial del juego
% ------------------------------------------------

% ==== Declaraciones Dinamicas ====
% Le decimos a Prolog que estos hechos cambiarán
% durante el juego (serán 'assert'ados y 'retract'ados)

% Nombre: jugador/1
% Entrada: Un átomo que representa el lugar actual (p.ej. bosque)
% Salida: Verdadero si el hecho jugador(Lugar) está registrado
% Descripcion: Mantiene la ubicación actual del jugador en el mundo del juego.
:- dynamic jugador/1.

% Nombre: inventario/1
% Entrada: Una lista de átomos (objetos) que el jugador posee
% Salida: Verdadero si el hecho inventario(ListaObjetos) está registrado
% Descripcion: Representa el conjunto de objetos actualmente en posesión del jugador.
:- dynamic inventario/1.

% Nombre: objeto/2
% Entrada: Objeto (átomo), Lugar (átomo)
% Salida: Verdadero si objeto(Objeto, Lugar) está en la base de hechos
% Descripcion: Ubicación actual de un objeto en el mundo (antes de ser tomado).
:- dynamic objeto/2.

% Nombre: objeto_usado/1
% Entrada: Objeto (átomo)
% Salida: Verdadero si el objeto ya fue marcado como usado
% Descripcion: Indica qué objetos han sido empleados para cumplir requisitos.
:- dynamic objeto_usado/1.

% Nombre: lugar_visitado/1
% Entrada: Lugar (átomo)
% Salida: Verdadero si el jugador ha visitado ese lugar
% Descripcion: Historial de exploración del jugador para validar rutas y condiciones.
:- dynamic lugar_visitado/1.

% Nombre: message/1
% Entrada: Mensaje (string/átomo)
% Salida: Verdadero si el último mensaje fue almacenado
% Descripcion: Buffer del último mensaje generado por las reglas para consumo en CSharp.
:- dynamic message/1.

% ==== Estado Inicial del Juego ====
% Estos son los hechos con los que el juego comienza
% En 'reglas.pl' se modificarán estos hechos

% Nombre: jugador/1 (estado inicial)
% Entrada: bosque
% Salida: Verdadero (hecho base)
% Descripcion: El jugador inicia la partida en el lugar 'bosque'.
jugador(bosque).

% Nombre: inventario/1 (estado inicial)
% Entrada: []
% Salida: Verdadero (inventario vacío)
% Descripcion: El jugador comienza sin objetos en su inventario.
inventario([]).

% Nombre: lugar_visitado/1 (estado inicial)
% Entrada: bosque
% Salida: Verdadero (primer lugar marcado como visitado)
% Descripcion: Se registra el lugar inicial como visitado para futuras validaciones.
lugar_visitado(bosque).