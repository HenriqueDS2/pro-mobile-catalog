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
                page.Margin(1.5f, Unit.Centimetre);
                page.PageColor("#F4EFE6");

                page.Content().PaddingVertical(10).Column(col =>
                {
                    for (int i = 0; i < produtos.Count; i++)
                    {
                        var prod = produtos[i];

                        if (i > 0)
                        {
                            col.Item().PageBreak();
                        }

                        col.Item().PaddingBottom(10).AlignCenter().Width(450).Border(1, Unit.Point).BorderColor("#D4C5B3").Background("#FFFFFF").Padding(25).Column(card =>
                        {
                            card.Item().Text(prod.Nome)
                                .FontFamily("Georgia").FontSize(24).Bold().FontColor("#3A1E1E").AlignCenter();

                            string textoDetalhes = prod.Descricao;
                            if (prod.UsarEstoque)
                            {
                                textoDetalhes += $" • Disponível: {prod.QuantidadeEstoque} un";
                            }

                            if (!string.IsNullOrWhiteSpace(textoDetalhes))
                            {
                                card.Item().PaddingTop(5).Text(textoDetalhes)
                                  .FontFamily("Arial").FontSize(14).FontColor("#5A4A42").AlignCenter();
                            }

                            if (!string.IsNullOrWhiteSpace(prod.CaminhoImagem) && File.Exists(prod.CaminhoImagem))
                            {
                                card.Item().PaddingTop(20).PaddingBottom(20).AlignCenter().Width(300).Height(300).Image(prod.CaminhoImagem);
                            }
                            else
                            {
                                card.Item().PaddingTop(40).PaddingBottom(40).AlignCenter().Text("[ Sem Foto ]").FontColor("#5A4A42").Italic();
                            }

                            card.Item().AlignCenter().Background("#D4AF37").PaddingVertical(8).PaddingHorizontal(25).Column(precoCol =>
                            {
                                precoCol.Item().Text($"R$ {prod.Preco:N2}")
                                    .FontFamily("Arial").FontSize(16).Bold().FontColor("#FFFFFF").AlignCenter();
                            });
                        });
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
