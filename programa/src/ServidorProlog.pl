:- use_module(library(socket)).

start_server(Port) :-
    tcp_socket(Socket),
    tcp_bind(Socket, Port),
    tcp_listen(Socket, 5),
    tcp_open_socket(Socket, AcceptFd, _),
    format("Servidor Prolog activo en puerto ~w~n", [Port]),
    accept_loop(AcceptFd).

accept_loop(AcceptFd) :-
    tcp_accept(AcceptFd, Socket, _),
    setup_call_cleanup(
        tcp_open_socket(Socket, In, Out),
        handle_client(In, Out),
        close_connection(In, Out)
    ),
    accept_loop(AcceptFd).  

handle_client(In, Out) :-
    read_line_to_string(In, Command),
    (   catch(term_string(Term, Command), _, fail),
        call(Term)
    ->  Result = "ok"
    ;   Result = "error"
    ),
    format(Out, "~w~n", [Result]),
    flush_output(Out).

close_connection(In, Out) :-
    close(In), close(Out).
