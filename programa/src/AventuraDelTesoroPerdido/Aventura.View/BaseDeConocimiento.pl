% Base de conocimiento.pl

% Este archivo define el mundo del juego
% Contiene todos los hechos que no cambian
% ------------------------------------------------

% ==== Lugares ====
% lugar(Nombre, Descripcion)

lugar(bosque, "Un denso bosque lleno de sonidos extraños.").
lugar(templo, "Un antiguo templo abandonado y cubierto de musgo.").
lugar(cueva, "Una cueva misteriosa y oscura.").

% ==== Conexiones ====
% conectado(Lugar1, Lugar2).

conectado(bosque, templo).
conectado(templo, bosque).
conectado(bosque, cueva).
conectado(cueva, bosque).
conectado(cueva, templo).
conectado(templo, cueva).

% ==== Objetos ====
% objeto(Objeto, LugarDondeEsta).

objeto(llave, bosque).
objeto(antorcha, templo).
objeto(oro, cueva).

% ==== Reglas de Acceso (REQUISITOS) ====
% requiere(Objeto, IngresoLugar)

requiere(llave, templo).
requiere(antorcha, cueva).

% ==== Requisitos de Visita ====
% requiereVisita(LugarDestino, LugarVisitado)

requiereVisita(cueva, bosque).

% ==== Condición de Victoria (Tesoro) ====
% tesoro(LugarDondeEstar, ObjetoATener).

tesoro(cueva, oro).