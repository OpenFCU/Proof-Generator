namespace ProofGenerator;

public record Request(
    Student Student,
    string Title,
    string Icon,
    string Stamp
);

public record Response(
    string path
);