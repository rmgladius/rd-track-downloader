using System.Globalization;
using System.Text;
using QRCoder;

namespace Faixa.Servicos;

/// <summary>
/// Código Pix para doações: o texto do "Pix copia e cola" e o QR com o mesmo texto.
/// Segue o BR Code do Banco Central, sem valor fixo (quem paga escolhe quanto).
/// </summary>
public static class Pix
{
    /// <summary>
    /// Monta o código. Cada campo é ID (2 dígitos) + tamanho (2 dígitos) + valor, e o último
    /// guarda o CRC16 de tudo o que vem antes. Nome e cidade vão sem acento: o padrão só aceita ASCII.
    /// </summary>
    public static string CopiaECola(string chave, string nome, string cidade)
    {
        var conta = Campo("00", "br.gov.bcb.pix") + Campo("01", chave.Trim());
        var semCrc = Campo("00", "01")             // versão do formato
                     + Campo("26", conta)          // onde o dinheiro cai
                     + Campo("52", "0000")         // categoria do comércio: não se aplica
                     + Campo("53", "986")          // real
                     + Campo("58", "BR")
                     + Campo("59", Ascii(nome, 25))
                     + Campo("60", Ascii(cidade, 15))
                     + Campo("62", Campo("05", "***")) // sem identificador: o mesmo código serve para sempre
                     + "6304";
        return semCrc + Crc16(semCrc).ToString("X4");
    }

    /// <summary>
    /// Os quadradinhos do QR, já com a margem branca de 4 módulos que os leitores precisam.
    /// [linha, coluna]; true é escuro. Correção de erro média, para ler bem na tela.
    /// </summary>
    public static bool[,] Matriz(string texto)
    {
        using var gerador = new QRCodeGenerator();
        using var dados = gerador.CreateQrCode(texto, QRCodeGenerator.ECCLevel.M);
        var linhas = dados.ModuleMatrix;
        var lado = linhas.Count;
        var matriz = new bool[lado, lado];
        for (var y = 0; y < lado; y++)
            for (var x = 0; x < lado; x++)
                matriz[y, x] = linhas[y][x];
        return matriz;
    }

    internal static string Campo(string id, string valor)
    {
        if (valor.Length is 0 or > 99)
            throw new ArgumentException($"O campo {id} do Pix precisa ter de 1 a 99 caracteres.", nameof(valor));
        return id + valor.Length.ToString("D2", CultureInfo.InvariantCulture) + valor;
    }

    /// <summary>Tira acentos e o que não for ASCII visível, e corta no tamanho máximo do campo.</summary>
    internal static string Ascii(string texto, int maximo)
    {
        var sb = new StringBuilder();
        foreach (var c in texto.Normalize(NormalizationForm.FormD))
            if (c is >= ' ' and <= '~')
                sb.Append(c);
        var limpo = sb.ToString().Trim();
        return limpo.Length > maximo ? limpo[..maximo].TrimEnd() : limpo;
    }

    /// <summary>CRC16-CCITT (polinômio 0x1021, começando em 0xFFFF), o que o BR Code usa.</summary>
    internal static ushort Crc16(string texto)
    {
        ushort crc = 0xFFFF;
        foreach (var b in Encoding.ASCII.GetBytes(texto))
        {
            crc ^= (ushort)(b << 8);
            for (var i = 0; i < 8; i++)
                crc = (crc & 0x8000) != 0 ? (ushort)((crc << 1) ^ 0x1021) : (ushort)(crc << 1);
        }
        return crc;
    }
}

/// <summary>O Pix de quem mantém o projeto, mostrado no painel Apoiar.</summary>
public static class Apoio
{
    /// <summary>Chave aleatória: não expõe CPF, e-mail nem telefone.</summary>
    public const string Chave = "561cdaea-b433-4980-b2fd-8f4578e6e37d";
    public const string Nome = "Rodrigo Dias";
    public const string Cidade = "Florianópolis";

    public static string CopiaECola => Pix.CopiaECola(Chave, Nome, Cidade);
}
