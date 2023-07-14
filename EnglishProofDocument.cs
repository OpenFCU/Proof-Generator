using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace ProofGenerator;

public class EnglishProofDocument : ProofDocument
{
    private readonly string _title;
    private readonly Student _model;
    private readonly byte[]? _stamp;
    public EnglishProofDocument(string title, Student model, byte[]? icon, byte[]? stamp) : base(icon)
    {
        _title = title;
        _model = model;
        _stamp = stamp;
    }

    protected override void ComposePage(PageDescriptor page)
    {
        ComposeContent(page.Content(), _title, _model);
        if (_stamp != null)
        {
            ComposeStamp(page.Foreground(), _stamp);
            ComposeFooter(page.Footer());
        }
    }

    private void ComposeStamp(IContainer container, byte[] stamp)
    {
        container.AlignBottom()
            .PaddingBottom(10, Unit.Millimetre)
            .MaxHeight(50, Unit.Millimetre)
            .MaxWidth(66, Unit.Millimetre)
            .Image(stamp)
            .FitArea();
    }

    private void ComposeContent(IContainer container, string title, Student student)
    {
        container
            .DefaultTextStyle(s => s.FontFamily("TW-Kai").FontSize(18).LineHeight(1.7f))
            .PaddingLeft(20, Unit.Millimetre)
            .Column(column =>
            {
                column.Item()
                    .PaddingTop(12, Unit.Millimetre)
                    .PaddingBottom(8, Unit.Millimetre)
                    .PaddingRight(20, Unit.Millimetre)
                    .AlignCenter()
                    .Text(title)
                    .FontSize(26)
                    .Bold();
                ComposeField(column.Item(), "Student Name", student.Name);
                ComposeField(column.Item(), "Student ID", student.Id);
                ComposeField(column.Item(), "Date of Birth", student.Birthday.ToString("MMM dd, yyyy"));
                ComposeField(column.Item(), "Nationality", student.Nationality);
                ComposeField(column.Item(), "Identity", student.Kind);
                ComposeField(column.Item(), "Year and Month of Admission", student.RegisterDate.ToString("MMMM, yyyy"), nameWidth: 90);
                ComposeField(column.Item(), "Degree", student.Degree.ToEnglishString());
                ComposeField(column.Item(), "Period of Study", student.Degree.GetEnglishStudyLimit());
                ComposeField(column.Item(), "The Program of Study", student.Department, nameWidth: 70);
                ComposeField(column.Item(), "", "", "");
                ComposeField(column.Item(), "Year of Study", student.Grade.ToString());
                ComposeField(column.Item(), "Current Semester", CurrentSemester);
                ComposeField(column.Item(), "Date Issued", DateOnly.FromDateTime(DateTime.Today).ToString("MMM dd, yyyy"));
            });
    }

    private void ComposeFooter(IContainer container)
    {
        container
            .DefaultTextStyle(s => s.FontFamily("TW-Kai").FontSize(20).LineHeight(1.5f))
            .PaddingLeft(20, Unit.Millimetre)
            .Column(column =>
            {
                column.Item().Text("Registration and Curriculum Section");
                column.Item().Text("Office of Academic Affairs");
                column.Item()
                    .PaddingBottom(20, Unit.Millimetre)
                    .Text("Feng Chia University");
            });
    }

    private void CurrentSemester(IContainer container)
    {
        var semester = Semester.Current();
        container
            .AlignLeft()
            .Column(column =>
            {
                column.Item()
                    .Text($"{semester.Year} {(semester.Value == 1 ? "Autumn" : "Spring")} Semester");
                column.Item()
                    .Text($"(from {semester.StartDate().ToString("MMM dd, yyyy")} to {semester.EndDate().ToString("MMM dd, yyyy")})")
                    .FontSize(14);
            });
    }

    private void ComposeField(IContainer container, string name, Action<IContainer> value, string splitter = ":", float nameWidth = 55)
    {
        container.AlignLeft().Row(row =>
        {
            row.ConstantItem(nameWidth, Unit.Millimetre)
                .AlignLeft()
                .Text(name)
                .Bold();
            row.ConstantItem(8, Unit.Millimetre)
                .AlignLeft()
                .Text(splitter);
            row.RelativeItem()
                .AlignLeft()
                .Element(value);
        });
    }

    private void ComposeField(IContainer container, string name, string value, string splitter = ":", float nameWidth = 55)
    {
        ComposeField(container, name, (c) => c.Text(value), splitter, nameWidth);
    }
}

public static class EnglishProofDocumentExt
{
    public static string MonthAbbrName(this DateOnly date) => date.ToString("MMM");
    public static string MonthFullName(this DateOnly date) => date.ToString("MMMM");

    public static string Ordinal(this int n)
    {
        if (n % 100 == 11) return "th";
        if (n % 100 == 12) return "th";
        if (n % 100 == 13) return "th";
        if (n % 10 == 1) return "st";
        if (n % 10 == 2) return "nd";
        if (n % 10 == 3) return "rd";
        return "th";
    }

    public static string ToEnglishString(this Degree degree)
    {
        return degree switch
        {
            Degree.Bachelor => "Bachelor",
            Degree.Master => "Master",
            Degree.Doctor => "Doctor",
            _ => throw new ArgumentOutOfRangeException(nameof(degree), degree, null)
        };
    }

    public static string GetEnglishStudyLimit(this Degree degree)
    {
        return degree switch
        {
            Degree.Bachelor => "4 ~ 6 Years",
            Degree.Master => "1 ~ 4 Years",
            Degree.Doctor => "2 ~ 7 Years",
            _ => throw new ArgumentOutOfRangeException(nameof(degree), degree, null)
        };
    }
}