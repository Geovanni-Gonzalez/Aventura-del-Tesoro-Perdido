% Base de conocimiento.pl
% Define el mundo estático del juego (hechos que no cambian durante la partida)
% ------------------------------------------------

% Nombre: lugar/2
% Entrada: Nombre (átomo), Descripcion (string/átomo)
% Salida: true si el lugar existe en la base de conocimiento
% Descripcion: Representa un lugar del mundo y un texto descriptivo mostrado al jugador.
lugar(bosque, "Un denso bosque lleno de sonidos extraños.").
lugar(templo, "Un antiguo templo abandonado y cubierto de musgo.").
lugar(cueva, "Una cueva misteriosa y oscura.").

% Nombre: conectado/2
% Entrada: Lugar1 (átomo), Lugar2 (átomo)
% Salida: true si existe conexión directa entre Lugar1 y Lugar2
% Descripcion: Define conexiones bidireccionales (cuando se declaran ambas) que permiten movimiento entre lugares.
conectado(bosque, templo).
conectado(templo, bosque).
conectado(bosque, cueva).
conectado(cueva, bosque).
conectado(cueva, templo).
conectado(templo, cueva).

% Nombre: objeto/2
% Entrada: Objeto (átomo), LugarDondeEsta (átomo)
% Salida: true si el objeto se encuentra inicialmente en el lugar indicado
% Descripcion: Indica la ubicación inicial de cada objeto antes de ser tomado por el jugador.
objeto(llave, bosque).
objeto(antorcha, templo).
objeto(oro, cueva).

% Nombre: requiere/2
% Entrada: Objeto (átomo), IngresoLugar (átomo)
% Salida: true si para ingresar a IngresoLugar se necesita el Objeto
% Descripcion: Establece requisitos de posesión/uso de objetos para acceder a ciertos lugares.
requiere(llave, templo).
requiere(antorcha, cueva).

% Nombre: requiereVisita/2
% Entrada: LugarDestino (átomo), LugarVisitado (átomo)
% Salida: true si se exige haber visitado LugarVisitado antes de ir a LugarDestino
% Descripcion: Define dependencias de exploración (orden mínimo de visita) entre lugares.
requiereVisita(cueva, bosque).

% Nombre: tesoro/2
% Entrada: LugarDondeEstar (átomo), ObjetoATener (átomo)
% Salida: true si el objeto especificado es el requerido en el lugar para ganar
% Descripcion: Condición de victoria: poseer ObjetoATener estando en LugarDondeEstar.
tesoro(cueva, oro).
