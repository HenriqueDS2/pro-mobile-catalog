using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

#if ANDROID
using Android.Content;
using Android.Print;
using Microsoft.Maui.Platform;
#endif

namespace Catalogo;

public partial class MainPage : ContentPage
{
    private readonly DatabaseService _bancoDeDados = new DatabaseService();

    public ObservableCollection<Produto> ProdutosLista { get; set; } = new ObservableCollection<Produto>();

    public MainPage()
    {
        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
        Shell.SetNavBarIsVisible(this, false);
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
            produtoAtual.UsarEstoque = e.Value;

            await _bancoDeDados.SalvarProdutoAsync(produtoAtual);

            CarregarProdutosDoBancoAsync();
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
        List<Produto> listaFiltrada = ProdutosLista
            .Where(p => !string.IsNullOrWhiteSpace(p.Nome) || (!string.IsNullOrWhiteSpace(p.CaminhoImagem) && File.Exists(p.CaminhoImagem)))
            .ToList();

        if (listaFiltrada.Count == 0)
        {
            await this.DisplayAlertAsync("Atenção", "Adicione pelo menos um produto com nome ou foto para gerar o catálogo.", "OK");
            return;
        }

        CriarLayoutPdfNativoAndroid(listaFiltrada);
    }

    private void CriarLayoutPdfNativoAndroid(List<Produto> produtos)
    {
        var html = new StringBuilder();
        html.Append("<!DOCTYPE html><html><head><meta charset='utf-8'/><style>");
        html.Append("body { font-family: 'Open Sans', 'Segoe UI', sans-serif; background-color: #FFFFFF; margin: 0; padding: 0; }");
        html.Append(".pagina { width: 100%; height: 100vh; box-sizing: border-box; padding: 45px; page-break-after: always; display: flex; flex-direction: column; justify-content: center; align-items: center; background: #FFFFFF; }");
        html.Append(".moldura { border: 2px solid #B9A47A; padding: 35px; width: 100%; height: 100%; box-sizing: border-box; display: flex; flex-direction: column; justify-content: space-between; align-items: center; }");
        html.Append(".titulo { font-size: 34px; color: #542020; font-weight: bold; font-family: 'Georgia', serif; text-align: center; margin-top: 15px; width: 100%; }");
        html.Append(".descricao { font-size: 23px; color: #666666; font-style: italic; font-family: 'Georgia', serif; text-align: center; margin-top: 10px; padding: 0 10px; width: 100%; }");

        // AJUSTE 1: max-height de 75% e margin: auto 0; faz a foto da capa centralizar verticalmente perfeita
        html.Append(".container-foto { width: 100%; flex-grow: 1; display: flex; justify-content: center; align-items: center; margin: auto 0; max-height: 75%; overflow: hidden; }");
        html.Append(".foto { max-width: 100%; max-height: 100%; object-fit: contain; }");
        html.Append(".sem-foto { width: 100%; height: 480px; background: #F7F7F7; display: flex; align-items: center; justify-content: center; color: #777777; font-style: italic; font-size: 16px; border: 1px dashed #CCCCCC; margin: auto 0; }");

        html.Append(".preco { font-size: 24px; color: #542020; font-weight: bold; font-family: 'Georgia', serif; text-align: right; width: 100%; margin-bottom: 15px; }");
        html.Append("</style></head><body>");

        for (int i = 0; i < produtos.Count; i++)
        {
            var prod = produtos[i];
            html.Append("<div class='pagina'><div class='moldura'>");

            if (i == 0)
            {
                // Capa do Catálogo (Sem títulos, apenas a imagem centralizada verticalmente)
                html.Append("<div class='container-foto'>");
                if (!string.IsNullOrWhiteSpace(prod.CaminhoImagem) && File.Exists(prod.CaminhoImagem))
                    html.Append($"<img class='foto' src='file://{prod.CaminhoImagem}' />");
                else
                    html.Append("<div class='sem-foto'>[ Sem Foto ]</div>");
                html.Append("</div>");
            }
            else
            {
                // Páginas Internas de Produtos
                if (!string.IsNullOrWhiteSpace(prod.Nome))
                    html.Append($"<div class='titulo'>{prod.Nome}</div>");
                else
                    html.Append("<div class='titulo'>&nbsp;</div>");

                if (!string.IsNullOrWhiteSpace(prod.Descricao))
                    html.Append($"<div class='descricao'>{prod.Descricao}</div>");
                else
                    html.Append("<div class='descricao'>&nbsp;</div>");

                html.Append("<div class='container-foto'>");
                if (!string.IsNullOrWhiteSpace(prod.CaminhoImagem) && File.Exists(prod.CaminhoImagem))
                    html.Append($"<img class='foto' src='file://{prod.CaminhoImagem}' />");
                else
                    html.Append("<div class='sem-foto'>[ Sem Foto ]</div>");
                html.Append("</div>");
                html.Append($"<div class='preco'>R$ {prod.Preco:N2}</div>");
            }

            html.Append("</div></div>");
        }

        html.Append("</body></html>");

#if ANDROID
        var contexto = Platform.CurrentActivity;
        if (contexto != null)
        {
            contexto.RunOnUiThread(() =>
            {
                var webView = new Android.Webkit.WebView(contexto);
                webView.Settings.AllowFileAccess = true;
                webView.Settings.AllowContentAccess = true;

                webView.SetWebViewClient(new GenericPrintWebViewClient(contexto, "Catálogo Digital"));
                webView.LoadDataWithBaseURL("file:///", html.ToString(), "text/html", "utf-8", null);
            });
        }
#endif
    }
}

#if ANDROID
public class GenericPrintWebViewClient : Android.Webkit.WebViewClient
{
    private readonly Context _contexto;
    private readonly string _nomeDoc;

    public GenericPrintWebViewClient(Context contexto, string nomeDoc)
    {
        _contexto = contexto;
        _nomeDoc = nomeDoc;
    }

    public override void OnPageFinished(Android.Webkit.WebView? view, string? url)
    {
        if (view == null) return;

        var printManager = (PrintManager?)_contexto.GetSystemService(Context.PrintService);
        var printAdapter = view.CreatePrintDocumentAdapter(_nomeDoc);

        if (printManager != null)
        {
            printManager.Print(_nomeDoc, printAdapter, new PrintAttributes.Builder().Build());
        }
    }
}
#endif
