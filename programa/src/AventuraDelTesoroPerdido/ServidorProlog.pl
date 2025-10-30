% ==== Lugares ====
lugar(bosque, "Un denso bosque lleno de sonidos extraños.").
lugar(templo, "Un antiguo templo cubierto de musgo.").
lugar(cueva, "Una cueva misteriosa y oscura.").

% ==== Conexiones ====
conectado(bosque, templo).
conectado(templo, bosque).
conectado(bosque, cueva).
conectado(cueva, bosque).
conectado(cueva, templo).
conectado(templo, cueva).

% ==== Objetos ====
:- dynamic objeto/2.       % Para permitir tomar objetos
objeto(llave, bosque).
objeto(antorcha, templo).
objeto(oro, cueva).

% ==== Inventario ====
:- dynamic inventario/1.
inventario([]).

% ==== Jugador ====
:- dynamic jugador/1.
jugador(bosque).

% ==== Predicados de consulta ====
donde_esta :-
    jugador(Lugar),
    write('Estás en '), write(Lugar), nl.

que_tengo :-
    inventario(Lista),
    ( Lista = [] -> write('No tienes ningún objeto.')
    ; write('Tienes: '), write(Lista)
    ), nl.

puedo_ir(Destino) :-
    jugador(Desde),
    conectado(Desde, Destino),
    write('Puedes ir de '), write(Desde), write(' a '), write(Destino), nl.

% ==== Acciones ====
mover(Destino) :-
    jugador(Desde),
    conectado(Desde, Destino),
    retract(jugador(Desde)),
    assert(jugador(Destino)),
    write('Te moviste a '), write(Destino), nl.

tomar(Objeto) :-
    jugador(Lugar),
    objeto(Objeto, Lugar),
    retract(objeto(Objeto, Lugar)),
    inventario(Inv),
    retract(inventario(Inv)),
    assert(inventario([Objeto|Inv])),
    write('Tomaste el objeto: '), write(Objeto), nl.

usar(Objeto) :-
    inventario(Inv),
    member(Objeto, Inv),
    write('Usaste el objeto: '), write(Objeto), nl.

% ==== Tesoro y condición de gane ====
tesoro(cueva, oro).

verifica_gane :-
    jugador(Lugar),
    inventario(Inv),
    tesoro(Lugar, Objeto),
    member(Objeto, Inv),
    write('¡Felicidades! Ganaste el juego.'), nl,
    write('Lugar: '), write(Lugar), nl,
    write('Inventario: '), write(Inv), nl.
