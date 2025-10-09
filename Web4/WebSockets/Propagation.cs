namespace Web4.WebSockets;

record struct Propagation(
    int CurrentID,
    int CurrentLevel,
    int SuppressID,
    int SuppressLevel
);