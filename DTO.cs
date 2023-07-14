namespace ProofGenerator;

public record Request(
    string Lang,
    string Title,
    string Icon,
    string Stamp,
    Student Student
);

public record Response(
    string path
);