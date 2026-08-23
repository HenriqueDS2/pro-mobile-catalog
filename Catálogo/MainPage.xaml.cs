using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Catalogo;

public partial class MainPage : ContentPage
{
    private readonly DatabaseService _bancoDeDados = new DatabaseService();
    private string _caminhoFotoTemporaria = "";

    public MainPage()
    {
        InitializeComponent();
        QuestPDF.Settings.License = LicenseType.Community;
    }

    private void OnChaveEstoqueModificada(object? sender, ToggledEventArgs e)
    {
        PainelQuantidade.IsVisible = e.Value;
    }

    private async void OnTirarFotoClicado(object? sender, EventArgs e)
    {
        try
        {
            if (MediaPicker.Default.IsCaptureSupported)
            {
                FileResult? foto = await MediaPicker.Default.CapturePhotoAsync();

                if (foto != null)
                {
                    string pastaDoApp = FileSystem.AppDataDirectory;
                    string caminhoFinal = Path.Combine(pastaDoApp, foto.FileName);

                    using Stream fluxoOrigem = await foto.OpenReadAsync();
                    using FileStream fluxoDestino = File.OpenWrite(caminhoFinal);
                    await fluxoOrigem.CopyToAsync(fluxoDestino);

                    _caminhoFotoTemporaria = caminhoFinal;
                    LabelCaminhoFoto.Text = "Foto capturada com sucesso!";
                    LabelCaminhoFoto.TextColor = Microsoft.Maui.Graphics.Colors.Green;
                }
            }
            else
            {
                await this.DisplayAlertAsync("Erro", "Câmera não suportada neste aparelho.", "OK");
            }
        }
        catch (Exception ex)
        {
            await this.DisplayAlertAsync("Erro", $"Falha ao abrir câmera: {ex.Message}", "OK");
        }
    }

    private async void OnSalvarEGerarPdfClicado(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(CampoNome.Text) || string.IsNullOrWhiteSpace(CampoPreco.Text))
        {
            await this.DisplayAlertAsync("Atenção", "Preencha o Nome e o Preço para continuar.", "OK");
            return;
        }

        decimal.TryParse(CampoPreco.Text, out decimal precoConvertido);
        int.TryParse(CampoQuantidade.Text, out int quantidadeConvertida);

        var produtoCadastro = new Produto
        {
            Nome = CampoNome.Text,
            Descricao = CampoDescricao.Text ?? "",
            Preco = precoConvertido,
            CaminhoImagem = _caminhoFotoTemporaria,
            UsarEstoque = ChaveEstoque.IsToggled,
            QuantidadeEstoque = quantidadeConvertida
        };

        await _bancoDeDados.SalvarProdutoAsync(produtoCadastro);

        string nomeArquivoPdf = $"Produto_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
        string caminhoPdf = Path.Combine(FileSystem.CacheDirectory, nomeArquivoPdf);

        CriarLayoutPdf(caminhoPdf, produtoCadastro);

        await CompartilharArquivoPdf(caminhoPdf, produtoCadastro.Nome);

        LimparFormulario();
    }

    private void CriarLayoutPdf(string caminhoSalvar, Produto produto)
    {
        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);
                page.PageColor("#F4EFE6");

                page.Content().Border(1, Unit.Point).BorderColor("#D4C5B3").Padding(30).Column(col =>
                {
                    col.Item().Text(produto.Nome)
                        .FontFamily("Georgia")
                        .FontSize(28)
                        .Bold()
                        .FontColor("#3A1E1E")
                        .AlignCenter();

                    string textoDetalhes = produto.Descricao;
                    if (produto.UsarEstoque)
                    {
                        textoDetalhes += $" • Disponível: {produto.QuantidadeEstoque} un";
                    }

                    if (!string.IsNullOrWhiteSpace(textoDetalhes))
                    {
                        col.Item().PaddingTop(5).Text(textoDetalhes)
                            .FontFamily("Arial")
                            .FontSize(14)
                            .FontColor("#5A4A42")
                            .AlignCenter();
                    }

                    if (!string.IsNullOrWhiteSpace(produto.CaminhoImagem) && File.Exists(produto.CaminhoImagem))
                    {
                        col.Item().PaddingTop(25).PaddingBottom(25).AlignCenter().Width(350).Height(400).Image(produto.CaminhoImagem);
                    }
                    else
                    {
                        col.Item().PaddingTop(50).PaddingBottom(50).AlignCenter().Text("[ Sem Foto do Produto ]").FontColor("#5A4A42").Italic();
                    }

                    // CORRIGIDO LINHA 132: Estrutura refinada para aplicar fundo colorido e bordas perfeitamente
                    col.Item().AlignCenter().Background("#D4AF37").PaddingVertical(8).PaddingHorizontal(25).Column(precoCol =>
                    {
                        precoCol.Item().Text($"R$ {produto.Preco:N2}")
                            .FontFamily("Arial")
                            .FontSize(18)
                            .Bold()
                            .FontColor("#FFFFFF")
                            .AlignCenter();
                    });
                });
            });
        }).GeneratePdf(caminhoSalvar);
    }

    private async Task CompartilharArquivoPdf(string caminhoPdf, string nomeProduto)
    {
        if (File.Exists(caminhoPdf))
        {
            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = $"Enviar Catálogo: {nomeProduto}",
                File = new ShareFile(caminhoPdf)
            });
        }
    }

    private void LimparFormulario()
    {
        CampoNome.Text = "";
        CampoDescricao.Text = "";
        CampoPreco.Text = "";
        CampoQuantidade.Text = "";
        ChaveEstoque.IsToggled = false;
        _caminhoFotoTemporaria = "";
        LabelCaminhoFoto.Text = "Nenhuma foto selecionada";
        LabelCaminhoFoto.TextColor = Microsoft.Maui.Graphics.Colors.Gray;
    }
}
