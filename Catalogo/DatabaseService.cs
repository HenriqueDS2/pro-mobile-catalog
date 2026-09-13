using SQLite;

namespace Catalogo;

public class DatabaseService
{
    private SQLiteAsyncConnection? _conexao;

    private async Task InicializarBancoAsync()
    {
        if (_conexao != null) return;

        string caminhoBanco = Path.Combine(FileSystem.AppDataDirectory, "ProdutosDB.db3");

        _conexao = new SQLiteAsyncConnection(caminhoBanco);

        await _conexao.CreateTableAsync<Produto>();
    }

    public async Task SalvarProdutoAsync(Produto produto)
    {
        await InicializarBancoAsync();

        if (_conexao is not null)
        {
            if (produto.Id != 0)
            {
                await _conexao.UpdateAsync(produto);
            }
            else
            {
                await _conexao.InsertAsync(produto);
            }
        }
    }

    public async Task<List<Produto>> ObterTodosProdutosAsync()
    {
        await InicializarBancoAsync();

        if (_conexao is not null)
        {
            return await _conexao.Table<Produto>().ToListAsync();
        }

        return new List<Produto>();
    }

    public async Task DeletarProdutoAsync(Produto produto)
    {
        await InicializarBancoAsync();

        if (_conexao is not null)
        {
            await _conexao.DeleteAsync(produto);
        }
    }
}
