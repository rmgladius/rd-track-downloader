using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Faixa.Servicos;

namespace Faixa;

/// <summary>Painel Apoiar: o Pix do projeto em QR, copia e cola e chave.</summary>
public partial class MainWindow
{
    private bool ApoioAberto => PainelApoio.Visibility == Visibility.Visible;

    /// <summary>Sobre ou Apoiar por cima da janela: Esc fecha, e arrastar link não vale.</summary>
    private bool PainelAberto => SobreAberto || ApoioAberto;

    private void Apoiar_Click(object sender, RoutedEventArgs e) => AbrirApoio();

    private void FecharApoio_Click(object sender, RoutedEventArgs e) => FecharApoio();

    private void PainelApoio_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (ReferenceEquals(e.OriginalSource, PainelApoio))
            FecharApoio();
    }

    private void CopiarCodigoPix_Click(object sender, RoutedEventArgs e) =>
        Copiar(Apoio.CopiaECola, PixCodigoRotulo, "Código copiado");

    private void CopiarChavePix_Click(object sender, RoutedEventArgs e) =>
        Copiar(Apoio.Chave, PixChaveRotulo, "Chave copiada");

    /// <summary>Também abre pelo link no rodapé do Sobre: aí um painel dá lugar ao outro.</summary>
    private void AbrirApoio()
    {
        if (SobreAberto)
        {
            PainelSobre.Visibility = Visibility.Collapsed;
            VoltarRotulos();
        }
        QrPix.Source ??= DesenharQr(Pix.Matriz(Apoio.CopiaECola));
        MostrarPainel(PainelApoio, CartaoApoioDesloc);
    }

    private void FecharApoio()
    {
        PainelApoio.Visibility = Visibility.Collapsed;
        VoltarRotulos();
        CampoLink.Focus();
    }

    private void FecharPaineis()
    {
        if (SobreAberto)
            FecharSobre();
        if (ApoioAberto)
            FecharApoio();
    }

    /// <summary>
    /// O QR em vetor, nítido em qualquer escala do Windows. Só os módulos escuros, sem a margem:
    /// o branco e a margem vêm da moldura em volta. Módulos vizinhos na mesma linha viram um retângulo só.
    /// </summary>
    private static DrawingImage DesenharQr(bool[,] matriz)
    {
        const int margem = 4; // a que Pix.Matriz já traz
        var lado = matriz.GetLength(0) - 2 * margem;
        var escuros = new StreamGeometry();
        using (var g = escuros.Open())
        {
            for (var y = 0; y < lado; y++)
                for (var x = 0; x < lado; x++)
                {
                    if (!matriz[y + margem, x + margem])
                        continue;
                    var inicio = x;
                    while (x + 1 < lado && matriz[y + margem, x + 1 + margem])
                        x++;
                    g.BeginFigure(new Point(inicio, y), isFilled: true, isClosed: true);
                    g.PolyLineTo([new Point(x + 1, y), new Point(x + 1, y + 1), new Point(inicio, y + 1)],
                        isStroked: false, isSmoothJoin: false);
                }
        }

        var desenho = new DrawingGroup();
        // Fixa o tamanho no quadrado inteiro, mesmo que a borda de algum lado seja clara.
        desenho.Children.Add(new GeometryDrawing(Brushes.Transparent, null, new RectangleGeometry(new Rect(0, 0, lado, lado))));
        desenho.Children.Add(new GeometryDrawing(Brushes.Black, null, escuros));
        var imagem = new DrawingImage(desenho);
        imagem.Freeze();
        return imagem;
    }
}
