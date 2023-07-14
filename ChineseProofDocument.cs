using ProofGenerator.Extension;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ProofGenerator;

public class ChineseProofDocument : ProofDocument
{
    private readonly string _title;
    private readonly Student _model;
    private readonly byte[]? _stamp;
    public ChineseProofDocument(string title, Student model, byte[]? icon, byte[]? stamp) : base(icon)
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
                ComposeField(column.Item(), "姓名", student.Name);
                ComposeField(column.Item(), "學號", student.Id);
                ComposeField(column.Item(), "出生日期", student.Birthday.ToString("yyyy 年 MM 月 dd 日"));
                ComposeField(column.Item(), "國籍", student.Nationality);
                ComposeField(column.Item(), "入學身分", student.Kind);
                ComposeField(column.Item(), "入學日期", $"{student.RegisterDate.Year} 年 {student.RegisterDate.Month} 月");
                ComposeField(column.Item(), "學制", student.Degree.ToChineseString());
                ComposeField(column.Item(), "修業年限", student.Degree.GetChineseStudyLimit());
                ComposeField(column.Item(), "就讀系所", student.Department);
                ComposeField(column.Item(), "", "");
                ComposeField(column.Item(), "就讀年級", $"{student.Grade.ToChinese()}年級");
                ComposeField(column.Item(), "就讀學期", CurrentSemester);
                ComposeField(column.Item(), "核發日期", DateOnly.FromDateTime(DateTime.Today).ToString("yyyy 年 MM 月 dd 日"));
            });
    }

    private void ComposeFooter(IContainer container)
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

    private void CurrentSemester(IContainer container)
    {
        var semester = Semester.Current();
        container
            .AlignLeft()
            .Column(column =>
            {
                column.Item()
                    .Text($"{semester.Year - 1911} 學年度第 {semester.Value} 學期");
                column.Item()
                    .Text($"({semester.StartDate().ToString("yyyy 年 MM 月 dd 日")} - {semester.EndDate().ToString("yyyy 年 MM 月 dd 日")})")
                    .FontSize(14);
            });
    }



    private void ComposeField(IContainer container, string name, Action<IContainer> value)
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

    private void ComposeField(IContainer container, string name, string value, string splitter = "：")
    {
        container.AlignLeft().Row(row =>
        {
            row.ConstantItem(30, Unit.Millimetre)
                .AlignLeft()
                .Text(name);
            row.ConstantItem(10, Unit.Millimetre)
                .AlignLeft()
                .Text(splitter);
            row.RelativeItem()
                .AlignLeft()
                .Text(value);
        });
    }
}

public static class ChineseProofDocumentExt
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

    public static string GetChineseStudyLimit(this Degree degree)
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