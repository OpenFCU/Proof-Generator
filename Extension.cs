using System.Text;

namespace ProofGenerator.Extension;

public static class Extension
{
    public static string ToChinese(this int i) => i switch
    {
        0 => "零",
        1 => "一",
        2 => "二",
        3 => "三",
        4 => "四",
        5 => "五",
        6 => "六",
        7 => "七",
        8 => "八",
        9 => "九",
        _ => throw new ArgumentOutOfRangeException(nameof(i), i, null)
    };

}