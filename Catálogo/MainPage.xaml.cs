using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Collections.ObjectModel;
using System.Linq;

namespace Catalogo;

public partial class MainPage : ContentPage
{
    private readonly DatabaseService _bancoDeDados = new DatabaseService();

    public ObservableCollection<Produto> ProdutosLista { get; set; } = new ObservableCollection<Produto>();

    public MainPage()
    {
        InitializeComponent();
        QuestPDF.Settings.License = LicenseType.Community;

        BindingContext = this;
        CarregarProdutosDoBancoAsync();
    }

    private async void CarregarProdutosDoBancoAsync()
    {
        var produtosDoBanco = await _bancoDeDados.ObterTodosProdutosAsync();

        ProdutosLista.Clear();
        foreach (var prod in produtosDoBanco)
        {
            ProdutosLista.Add(prod);
        }

        if (ProdutosLista.Count == 0)
        {
            var primeiroProduto = new Produto();
            await _bancoDeDados.SalvarProdutoAsync(primeiroProduto);
            ProdutosLista.Add(primeiroProduto);
        }

        ListaProdutosVisual.ItemsSource = ProdutosLista;
    }

    private async void OnAdicionarProdutoClicado(object? sender, EventArgs e)
    {
        var novoProd = new Produto();
        await _bancoDeDados.SalvarProdutoAsync(novoProd);
        ProdutosLista.Add(novoProd);

        ListaProdutosVisual.ScrollTo(novoProd);
    }

    private async void OnRemoverProdutoClicado(object? sender, EventArgs e)
    {
        if (sender is Button botao && botao.CommandParameter is Produto produtoParaRemover)
        {
            bool aceitou = await this.DisplayAlertAsync("Confirmação", $"Deseja mesmo remover este item do catálogo?", "Sim", "Não");
            if (!aceitou) return;

            await _bancoDeDados.DeletarProdutoAsync(produtoParaRemover);
            ProdutosLista.Remove(produtoParaRemover);

            if (ProdutosLista.Count == 0)
            {
                var blocoReserva = new Produto();
                await _bancoDeDados.SalvarProdutoAsync(blocoReserva);
                ProdutosLista.Add(blocoReserva);
            }
        }
    }

    private async void OnCampoMudouFoco(object? sender, FocusEventArgs e)
    {
        if (sender is Entry campo && campo.BindingContext is Produto produtoAtual)
        {
            await _bancoDeDados.SalvarProdutoAsync(produtoAtual);
        }
    }

    private async void OnSwitchEstoqueModificado(object? sender, ToggledEventArgs e)
    {
        if (sender is Switch chave && chave.BindingContext is Produto produtoAtual)
        {
            await _bancoDeDados.SalvarProdutoAsync(produtoAtual);
        }
    }

    private async void OnTirarFotoItemClicado(object? sender, EventArgs e)
    {
        if (sender is Button botao && botao.CommandParameter is Produto produtoAtual)
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
                        using Stream fluxoDestino = File.Create(caminhoFinal);
                        await fluxoOrigem.CopyToAsync(fluxoDestino);

                        produtoAtual.CaminhoImagem = caminhoFinal;
                        await _bancoDeDados.SalvarProdutoAsync(produtoAtual);

                        CarregarProdutosDoBancoAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                await this.DisplayAlertAsync("Erro", $"Falha na câmera: {ex.Message}", "OK");
            }
        }
    }

    private async void OnEscolherFotoItemClicado(object? sender, EventArgs e)
    {
        if (sender is Button botao && botao.CommandParameter is Produto produtoAtual)
        {
            try
            {
                var resultados = await MediaPicker.Default.PickPhotosAsync(new MediaPickerOptions { SelectionLimit = 1 });
                FileResult? foto = resultados?.FirstOrDefault();

                if (foto != null)
                {
                    string pastaDoApp = FileSystem.AppDataDirectory;
                    string caminhoFinal = Path.Combine(pastaDoApp, foto.FileName);

                    using Stream fluxoOrigem = await foto.OpenReadAsync();
                    using Stream fluxoDestino = File.Create(caminhoFinal);
                    await fluxoOrigem.CopyToAsync(fluxoDestino);

                    produtoAtual.CaminhoImagem = caminhoFinal;
                    await _bancoDeDados.SalvarProdutoAsync(produtoAtual);

                    CarregarProdutosDoBancoAsync();
                }
            }
            catch (Exception ex)
            {
                await this.DisplayAlertAsync("Erro", $"Falha na galeria: {ex.Message}", "OK");
            }
        }
    }

    private async void OnGerarPdfFinalClicado(object? sender, EventArgs e)
    {
        List<Produto> listaFiltrada = ProdutosLista.Where(p => !string.IsNullOrWhiteSpace(p.Nome)).ToList();

        if (listaFiltrada.Count == 0)
        {
            await this.DisplayAlertAsync("Atenção", "Preencha pelo menos o Nome de um produto para gerar o catálogo.", "OK");
            return;
        }

        string nomeArquivoPdf = $"Catalogo_Produtos_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
        string caminhoPdf = Path.Combine(FileSystem.CacheDirectory, nomeArquivoPdf);

        CriarLayoutPdf(caminhoPdf, listaFiltrada);
        await CompartilharArquivoPdf(caminhoPdf, "Catálogo Digital");
    }

    private void CriarLayoutPdf(string caminhoSalvar, List<Produto> produtos)
    {
        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(0);

                page.Content().Column(coluna =>
                {
                    for (int i = 0; i < produtos.Count; i++)
                    {
                        var prod = produtos[i];

                        if (i > 0)
                        {
                            coluna.Item().PageBreak();
                        }

                        if (i == 0)
                        {
                            coluna.Item()
                                .Width(PageSizes.A4.Width)
                                .Height(PageSizes.A4.Height)
                                .Background("#FFFFFF")
                                .Padding(45)
                                .Border(1)
                                .BorderColor("#B9A47A")
                                .Padding(35)
                                .AlignCenter()
                                .AlignMiddle()
                                .Column(capa =>
                                {
                                    if (!string.IsNullOrWhiteSpace(prod.CaminhoImagem) &&
                                        File.Exists(prod.CaminhoImagem))
                                    {
                                        capa.Item()
                                            .Width(430)
                                            .Height(600)
                                            .Image(prod.CaminhoImagem)
                                            .FitArea();
                                    }
                                    else
                                    {
                                        capa.Item()
                                            .Width(430)
                                            .Height(600)
                                            .Background("#F7F7F7")
                                            .AlignCenter()
                                            .AlignMiddle()
                                            .Text("[ Sem Foto ]")
                                            .FontFamily("Arial")
                                            .FontSize(14)
                                            .Italic()
                                            .FontColor("#777777")
                                            .AlignCenter();
                                    }
                                });
                        }
                        else
                        {
                            coluna.Item()
                                .Width(PageSizes.A4.Width)
                                .Height(PageSizes.A4.Height)
                                .Background("#FFFFFF")
                                .Padding(45)
                                .Border(1)
                                .BorderColor("#B9A47A")
                                .Padding(35)
                                .Column(pagina =>
                                {
                                    pagina.Item()
                                        .PaddingTop(25)
                                        .PaddingBottom(25)
                                        .Text(prod.Nome)
                                        .FontFamily("Georgia")
                                        .FontSize(34)
                                        .Bold()
                                        .FontColor("#542020")
                                        .AlignCenter();

                                    if (!string.IsNullOrWhiteSpace(prod.CaminhoImagem) &&
                                        File.Exists(prod.CaminhoImagem))
                                    {
                                        pagina.Item()
                                            .AlignCenter()
                                            .Width(390)
                                            .Height(480)
                                            .Image(prod.CaminhoImagem)
                                            .FitArea();
                                    }
                                    else
                                    {
                                        pagina.Item()
                                            .AlignCenter()
                                            .Width(390)
                                            .Height(480)
                                            .Background("#F7F7F7")
                                            .AlignCenter()
                                            .AlignMiddle()
                                            .Text("[ Sem Foto ]")
                                            .FontFamily("Arial")
                                            .FontSize(14)
                                            .Italic()
                                            .FontColor("#777777")
                                            .AlignCenter();
                                    }

                                    pagina.Item()
                                        .Width(390)
                                        .PaddingTop(15)
                                        .AlignRight()
                                        .Text($"R$ {prod.Preco:N2}")
                                        .FontFamily("Georgia")
                                        .FontSize(22)
                                        .Bold()
                                        .FontColor("#542020");
                                });
                        }
                    }
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
}
