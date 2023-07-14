

using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ProofGenerator;

public abstract class ProofDocument : IDocument
{
    private readonly DocumentMetadata _metadata;
    private readonly byte[]? _icon;
    public ProofDocument(byte[]? icon)
    {
        _icon = icon;
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

            ComposeHeader(page.Header(), _icon);
            ComposePage(page);
        });
    }

    protected void ComposeHeader(IContainer container, byte[]? icon)
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

    protected abstract void ComposePage(PageDescriptor page);
}