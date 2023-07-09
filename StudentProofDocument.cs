using ProofGenerator.Extension;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ProofGenerator;

public class StudentProofDocument : IDocument
{
    private readonly DocumentMetadata _metadata;
    private readonly string _title;
    private readonly Student _model;
    private readonly byte[]? _icon;
    private readonly byte[]? _stamp;
    public StudentProofDocument(string title, Student model, byte[]? icon, byte[]? stamp)
    {
        _title = title;
        _model = model;
        _icon = icon;
        _stamp = stamp;
        _metadata = new DocumentMetadata
        {
            Author = "Project Open FCU",
        };
    }

    public DocumentMetadata GetMetadata() => _metadata;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.PageColor(Colors.White);

            page.Header().ComposeHeader(_icon);
            page.Content().ComposeContent(_title, _model);
            if (_stamp != null)
            {
                page.Foreground().ComposeStamp(_stamp);
                page.Footer().ComposeFooter();
            }
        });
    }
}

public static class StudentProofDocumentExt
{

    public static void ComposeStamp(this IContainer container, byte[] stamp)
    {
        container.AlignBottom()
            .PaddingBottom(10, Unit.Millimetre)
            .MaxHeight(50, Unit.Millimetre)
            .MaxWidth(66, Unit.Millimetre)
            .Image(stamp)
            .FitArea();
    }
    public static void ComposeHeader(this IContainer container, byte[]? icon)
    {
        container
            .Height(35, Unit.Millimetre)
            .Background(Colors.Orange.Medium)
            .PaddingTop(3, Unit.Millimetre)
            .PaddingHorizontal(20, Unit.Millimetre)
            .DefaultTextStyle(s => s.FontFamily("Noto Sans TC").Medium().FontColor(Colors.White))
            .Row(row =>
            {
                var iconCotainer = row.ConstantItem(25, Unit.Millimetre)
                    .PaddingTop(2, Unit.Millimetre)
                    .Height(25, Unit.Millimetre)
                    .Width(25, Unit.Millimetre);
                if (icon != null)
                {
                    iconCotainer
                    .Image(icon)
                    .FitArea();
                }
                row.RelativeItem()
                    .ExtendVertical()
                    .AlignCenter()
                    .Column(col =>
                    {
                        col.Item()
                            .Text("逢甲大學 Feng Chia University")
                            .FontSize(20);
                        col.Item()
                            .Text("No. 100, Wenhua Rd., Xitun Dist., Taichung 407102, Taiwan (R.O.C.)")
                            .FontSize(12);
                        col.Item()
                            .Text("407102 臺中市西屯區文華路100號")
                            .FontSize(12);
                    });
            });
    }

    public static void ComposeContent(this IContainer container, string title, Student student)
    {
        container
            .DefaultTextStyle(s => s.FontFamily("TW-Kai").FontSize(20).LineHeight(1.5f))
            .PaddingLeft(20, Unit.Millimetre)
            .Column(column =>
            {
                column.Item()
                    .PaddingTop(12, Unit.Millimetre)
                    .PaddingBottom(8, Unit.Millimetre)
                    .PaddingRight(20, Unit.Millimetre)
                    .AlignCenter()
                    .Text(title)
                    .FontSize(26);
                column.Item().ComposeField("姓名", student.Name);
                column.Item().ComposeField("學號", student.Id);
                column.Item().ComposeField("出生日期", student.Birthday.ToChineseDate());
                column.Item().ComposeField("國籍", student.Nationality);
                column.Item().ComposeField("入學身分", student.Kind);
                column.Item().ComposeField("入學日期", $"{student.RegisterDate.Year} 年 {student.RegisterDate.Month} 月");
                column.Item().ComposeField("學制", student.Degree.ToChineseString());
                column.Item().ComposeField("修業年限", student.Degree.GetStudyLimit());
                column.Item().ComposeField("就讀系所", student.Department);

                column.Item()
                    .PaddingTop(10, Unit.Millimetre)
                    .ComposeField("就讀年級", $"{student.Grade.ToChinese()}年級");
                column.Item().ComposeField("就讀學期", CurrentSemester);
                column.Item().ComposeField("核發日期", DateOnly.FromDateTime(DateTime.Today).ToChineseDate());

            });
    }

    public static void ComposeFooter(this IContainer container)
    {
        container
            .DefaultTextStyle(s => s.FontFamily("TW-Kai").FontSize(20).LineHeight(1.5f))
            .PaddingLeft(20, Unit.Millimetre)
            .Column(column =>
            {
                column.Item()
                    .PaddingBottom(10, Unit.Millimetre)
                    .PaddingLeft(8, Unit.Millimetre)
                    .Text("特此證明");
                column.Item()
                    .PaddingBottom(30, Unit.Millimetre)
                    .PaddingLeft(8, Unit.Millimetre)
                    .Text("教務處註冊課務組").FontSize(24).LetterSpacing(0.5f);
            });
    }

    private static void CurrentSemester(this IContainer container)
    {
        var semester = Semester.Current();
        container
            .AlignLeft()
            .Column(column =>
            {
                column.Item()
                    .Text($"{semester.Year - 1911} 學年度第 {semester.Value} 學期");
                column.Item()
                    .Text($"({semester.StartDate().ToChineseDate()} - {semester.EndDate().ToChineseDate()})")
                    .FontSize(14);
            });
    }

    private static string ToChineseDate(this DateOnly date) =>
        $"{date.Year} 年 {date.Month} 月 {date.Day} 日";

    private static void ComposeField(this IContainer container, string name, Action<IContainer> value)
    {
        container.AlignLeft().Row(row =>
        {
            row.ConstantItem(30, Unit.Millimetre)
                .AlignLeft()
                .Text(name);
            row.ConstantItem(10, Unit.Millimetre)
                .AlignLeft()
                .Text("：");
            row.RelativeItem()
                .AlignLeft()
                .Element(value);
        });
    }

    private static void ComposeField(this IContainer container, string name, string value)
    {
        container.AlignLeft().Row(row =>
        {
            row.ConstantItem(30, Unit.Millimetre)
                .AlignLeft()
                .Text(name);
            row.ConstantItem(10, Unit.Millimetre)
                .AlignLeft()
                .Text("：");
            row.RelativeItem()
                .AlignLeft()
                .Text(value);
        });
    }
}