using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Faixa.Servicos;

namespace Faixa;

/// <summary>Quem fez o programa e como falar com ele, mostrados no painel Sobre.</summary>
public static class Sobre
{
    public const string Email = "rmdiasvieira@icloud.com";
    public const string GitHub = "https://github.com/rmgladius";
    public const string GitHubTexto = "github.com/rmgladius";
    public const string Instagram = "https://www.instagram.com/rodrigodiasdj/";
    public const string InstagramTexto = "@rodrigodiasdj";
    public const string Problemas = Programa.UrlRepositorio + "/issues";
}

public partial class MainWindow
{
    /// <summary>Rótulos trocados por "copiado": o texto de antes e o relógio que o devolve.</summary>
    private readonly Dictionary<TextBlock, (string Original, DispatcherTimer Relogio)> _copiados = [];

    private bool SobreAberto => PainelSobre.Visibility == Visibility.Visible;

    private void Sobre_Click(object sender, RoutedEventArgs e) => AbrirSobre();

    private void FecharSobre_Click(object sender, RoutedEventArgs e) => FecharSobre();

    /// <summary>Clique no fundo escuro, fora do cartão, fecha.</summary>
    private void PainelSobre_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (ReferenceEquals(e.OriginalSource, PainelSobre))
            FecharSobre();
    }

    private void AbrirSobre()
    {
        SobreVersao.Text = _versaoNova is { } nova
            ? $"Versão {Atualizador.VersaoAtualTexto} · a {nova.Texto} já saiu"
            : $"Versão {Atualizador.VersaoAtualTexto}";
        MostrarPainel(PainelSobre, CartaoSobreDesloc);
    }

    private void FecharSobre()
    {
        PainelSobre.Visibility = Visibility.Collapsed;
        VoltarRotulos();
        CampoLink.Focus();
    }

    /// <summary>Sobre e Apoiar entram do mesmo jeito: o fundo escurece e o cartão sobe um pouco.</summary>
    private void MostrarPainel(UIElement painel, TranslateTransform deslocamento)
    {
        painel.Visibility = Visibility.Visible;
        painel.Focus();
        if (Capturando)
            return;

        var duracao = TimeSpan.FromMilliseconds(170);
        painel.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, duracao));
        deslocamento.BeginAnimation(TranslateTransform.YProperty,
            new DoubleAnimation(12, 0, duracao) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } });
    }

    /// <summary>GitHub, Instagram, código-fonte: o endereço vem no Tag do botão.</summary>
    private void Endereco_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: string endereco })
            AbrirArquivo(endereco);
    }

    /// <summary>
    /// Copia em vez de abrir um "mailto:": muita gente usa o e-mail no navegador e não tem
    /// programa de e-mail configurado no Windows, e aí o clique não faria nada.
    /// </summary>
    private void CopiarEmail_Click(object sender, RoutedEventArgs e) => Copiar(Sobre.Email, EmailRotulo, "E-mail copiado");

    /// <summary>Copia e troca o rótulo da linha por um aviso verde, que volta ao normal em 2,5 s.</summary>
    private void Copiar(string texto, TextBlock rotulo, string aviso)
    {
        try
        {
            if (!Capturando) // as capturas não mexem na área de transferência de quem roda
                Clipboard.SetText(texto);
        }
        catch
        {
            return; // área de transferência ocupada por outro programa
        }

        if (!_copiados.TryGetValue(rotulo, out var copiado))
        {
            copiado = (rotulo.Text, new DispatcherTimer { Interval = TimeSpan.FromSeconds(2.5) });
            copiado.Relogio.Tick += (_, _) => VoltarRotulo(rotulo);
            _copiados[rotulo] = copiado;
        }
        rotulo.Text = aviso;
        rotulo.Foreground = Pincel("Sucesso");
        copiado.Relogio.Stop();
        copiado.Relogio.Start();
    }

    private void VoltarRotulo(TextBlock rotulo)
    {
        if (!_copiados.Remove(rotulo, out var copiado))
            return;
        copiado.Relogio.Stop();
        rotulo.Text = copiado.Original;
        rotulo.Foreground = Pincel("TextoApagado");
    }

    private void VoltarRotulos()
    {
        foreach (var rotulo in _copiados.Keys.ToList())
            VoltarRotulo(rotulo);
    }
}
