namespace ProofGenerator;

public record Request(
    Student Student,
    string Icon,
    string Stamp
);

public record Response(
    string path
);