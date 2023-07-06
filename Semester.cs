namespace ProofGenerator;

public record Semester(int Year, int Value)
{
    public static Semester Current()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var delimit1 = new DateOnly(today.Year, 2, 1);
        var delimit2 = new DateOnly(today.Year, 8, 1);
        if (today < delimit1)
        {
            return new Semester(today.Year - 1, 1);
        }

        return today < delimit2 ? new Semester(today.Year - 1, 2) : new Semester(today.Year, 1);
    }

    public DateOnly StartDate() => Value == 1 ? new DateOnly(Year, 8, 1) : new DateOnly(Year + 1, 2, 1);
    public DateOnly EndDate() => Value == 1 ? new DateOnly(Year + 1, 1, 31) : new DateOnly(Year + 1, 7, 31);
}