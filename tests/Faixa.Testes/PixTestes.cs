using ZXing;
using ZXing.Common;
using ZXing.QrCode;

namespace Faixa.Testes;

public class PixTestes
{
    [Fact]
    public void Crc16_e_o_ccitt_que_o_banco_central_usa() =>
        Assert.Equal(0x29B1, Pix.Crc16("123456789"));

    /// <summary>O exemplo de QR estático do manual do BR Code, letra por letra.</summary>
    [Fact]
    public void Gera_o_mesmo_codigo_do_exemplo_do_banco_central() =>
        Assert.Equal(
            "00020126580014br.gov.bcb.pix0136123e4567-e12b-12d1-a456-4266554400005204000053039865802BR"
            + "5913Fulano de Tal6008BRASILIA62070503***63041D3D",
            Pix.CopiaECola("123e4567-e12b-12d1-a456-426655440000", "Fulano de Tal", "BRASILIA"));

    [Fact]
    public void Codigo_do_projeto_nao_muda_sem_querer() =>
        Assert.Equal(
            "00020126580014br.gov.bcb.pix0136561cdaea-b433-4980-b2fd-8f4578e6e37d5204000053039865802BR"
            + "5912Rodrigo Dias6013Florianopolis62070503***6304F35F",
            Apoio.CopiaECola);

    [Theory]
    [InlineData("Florianópolis", 15, "Florianopolis")]
    [InlineData("São João da Boa Vista", 15, "Sao Joao da Boa")]
    [InlineData("  Conceição  ", 25, "Conceicao")]
    [InlineData("Ação ✓ 🎵", 25, "Acao")]
    public void Nome_e_cidade_vao_sem_acento_e_no_tamanho_do_campo(string texto, int maximo, string esperado) =>
        Assert.Equal(esperado, Pix.Ascii(texto, maximo));

    [Fact]
    public void Recusa_campo_vazio_ou_grande_demais()
    {
        Assert.Throws<ArgumentException>(() => Pix.Campo("01", ""));
        Assert.Throws<ArgumentException>(() => Pix.Campo("01", new string('a', 100)));
        Assert.Equal("0199" + new string('a', 99), Pix.Campo("01", new string('a', 99)));
    }

    /// <summary>Desenha o QR como na tela e lê de volta: tem de sair o mesmo copia e cola, sem nada a mais.</summary>
    [Fact]
    public void Qr_le_de_volta_o_mesmo_codigo()
    {
        var codigo = Apoio.CopiaECola;
        var matriz = Pix.Matriz(codigo);
        var lado = matriz.GetLength(0);
        Assert.Equal(lado, matriz.GetLength(1));

        // Margem branca de 4 módulos em volta.
        for (var i = 0; i < lado; i++)
            for (var m = 0; m < 4; m++)
                Assert.False(matriz[m, i] || matriz[lado - 1 - m, i] || matriz[i, m] || matriz[i, lado - 1 - m]);

        const int escala = 4;
        var tamanho = lado * escala;
        var pixels = new byte[tamanho * tamanho];
        for (var y = 0; y < tamanho; y++)
            for (var x = 0; x < tamanho; x++)
                pixels[y * tamanho + x] = matriz[y / escala, x / escala] ? (byte)0 : (byte)255;

        var imagem = new BinaryBitmap(new HybridBinarizer(
            new RGBLuminanceSource(pixels, tamanho, tamanho, RGBLuminanceSource.BitmapFormat.Gray8)));
        var lido = new QRCodeReader().decode(imagem);

        Assert.NotNull(lido);
        Assert.Equal(codigo, lido.Text);
    }
}
