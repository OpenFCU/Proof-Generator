namespace ProofGenerator;

public record Student(
    string Name,
    string Id,
    DateOnly Birthday,
    DateOnly RegisterDate,
    Degree Degree,
    int Grade,
    string Department,
    string Nationality,
    string Kind
);
