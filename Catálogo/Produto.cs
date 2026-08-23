using SQLite;

namespace Catalogo;

public class Produto
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string Descricao { get; set; } = string.Empty;

    public decimal Preco { get; set; }

    public string CaminhoImagem { get; set; } = string.Empty;

    public bool UsarEstoque { get; set; }

    public int QuantidadeEstoque { get; set; }
}
