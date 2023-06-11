namespace ProofGenerator;

public enum Degree
{
    Bachelor = 1,
    Master = 2,
    Doctor = 3
}

public static class DegreeExt
{
    public static string ToChineseString(this Degree degree)
    {
        return degree switch
        {
            Degree.Bachelor => "學士班",
            Degree.Master => "碩士班",
            Degree.Doctor => "博士班",
            _ => throw new ArgumentOutOfRangeException(nameof(degree), degree, null)
        };
    }

    public static string GetStudyLimit(this Degree degree)
    {
        return degree switch
        {
            Degree.Bachelor => "4 至 6 年",
            Degree.Master => "1 至 4 年",
            Degree.Doctor => "2 至 7 年",
            _ => throw new ArgumentOutOfRangeException(nameof(degree), degree, null)
        };
    }
}