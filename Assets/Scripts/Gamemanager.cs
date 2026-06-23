using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Níveis do Jogo")]
    public LevelData[] niveis;

    [Header("Referências de UI")]
    public UIManager uiManager;

    private int nivelAtual = 0;
    private List<string> letrasDigitadas = new List<string>();
    // Quantas vezes cada letra já foi usada nesta tentativa
    private Dictionary<string, int> contagemUsadas = new Dictionary<string, int>();
    private int erros = 0;
    private int estrelas = 3;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start() => CarregarNivel(0);

    // ─── Nível ─────────────────────────────────────────────────────────────────

    public void CarregarNivel(int index)
    {
        if (index >= niveis.Length) { FinalizarJogo(); return; }

        nivelAtual = index;
        erros = 0;
        estrelas = 3;
        letrasDigitadas.Clear();
        contagemUsadas.Clear();

        LevelData nivel = niveis[nivelAtual];
        uiManager.AtualizarNivel(nivel, nivelAtual + 1, niveis.Length);
        uiManager.AtualizarEstrelas(estrelas);
        uiManager.AtualizarSlots(letrasDigitadas, nivel.TamanhoDaResposta);
        uiManager.AtualizarAlfabeto(ContarNaResposta(nivel.resposta), contagemUsadas);
        uiManager.LimparFeedback();
    }

    public void ProximoNivel() => CarregarNivel(nivelAtual + 1);
    public void ReiniciarJogo() => CarregarNivel(0);

    // ─── Helpers de contagem ───────────────────────────────────────────────────

    /// Quantas vezes cada letra aparece na resposta correta
    public static Dictionary<string, int> ContarNaResposta(string resposta)
    {
        var map = new Dictionary<string, int>();
        foreach (char c in resposta.ToUpper())
        {
            string s = c.ToString();
            map[s] = map.ContainsKey(s) ? map[s] + 1 : 1;
        }
        return map;
    }

    /// Quantos usos restam para a letra (null = letra não está na resposta)
    private int? UsosRestantes(string letra)
    {
        var naResposta = ContarNaResposta(niveis[nivelAtual].resposta);
        if (!naResposta.ContainsKey(letra)) return null; // não está na resposta
        int jaUsados = contagemUsadas.ContainsKey(letra) ? contagemUsadas[letra] : 0;
        return naResposta[letra] - jaUsados;
    }

    // ─── Ações do jogador ──────────────────────────────────────────────────────

    public void DigitarLetra(string letra)
    {
        LevelData nivel = niveis[nivelAtual];
        if (letrasDigitadas.Count >= nivel.TamanhoDaResposta) return;

        // Letras fora da resposta: só podem aparecer uma vez (comportamento original)
        // Letras na resposta: limitadas pela quantidade de ocorrências
        int? restantes = UsosRestantes(letra);
        if (restantes == null)
        {
            // Letra não está na resposta — permite usar mas só uma vez
            if (contagemUsadas.ContainsKey(letra) && contagemUsadas[letra] > 0) return;
        }
        else if (restantes <= 0)
        {
            return; // esgotou os usos desta letra
        }

        letrasDigitadas.Add(letra);
        contagemUsadas[letra] = (contagemUsadas.ContainsKey(letra) ? contagemUsadas[letra] : 0) + 1;

        uiManager.AtualizarSlots(letrasDigitadas, nivel.TamanhoDaResposta);
        uiManager.AtualizarAlfabeto(ContarNaResposta(nivel.resposta), contagemUsadas);
    }

    public void RemoverLetraDoSlot(int indice)
    {
        if (indice < 0 || indice >= letrasDigitadas.Count) return;

        string letra = letrasDigitadas[indice];
        letrasDigitadas.RemoveAt(indice);

        if (contagemUsadas.ContainsKey(letra))
            contagemUsadas[letra] = Mathf.Max(0, contagemUsadas[letra] - 1);

        uiManager.AtualizarSlots(letrasDigitadas, niveis[nivelAtual].TamanhoDaResposta);
        uiManager.AtualizarAlfabeto(ContarNaResposta(niveis[nivelAtual].resposta), contagemUsadas);
    }

    public void LimparTudo()
    {
        letrasDigitadas.Clear();
        contagemUsadas.Clear();
        uiManager.AtualizarSlots(letrasDigitadas, niveis[nivelAtual].TamanhoDaResposta);
        uiManager.AtualizarAlfabeto(ContarNaResposta(niveis[nivelAtual].resposta), contagemUsadas);
        uiManager.LimparFeedback();
    }

    public void ConfirmarResposta()
    {
        LevelData nivel = niveis[nivelAtual];
        if (letrasDigitadas.Count < nivel.TamanhoDaResposta)
        {
            uiManager.MostrarFeedback("Preencha todas as letras!", false);
            return;
        }

        string tentativa = string.Join("", letrasDigitadas);
        if (nivel.ValidarResposta(tentativa))
        {
            uiManager.AnimarSlotsCorretos();
            SoundManager.Instance.PlaySound2D("Winnning11");
            StartCoroutine(DelayMostrarVitoria());
        }
        else
        {
            erros++;
            estrelas = Mathf.Max(0, 3 - erros);
            uiManager.AnimarSlotsErrados();
            uiManager.AtualizarEstrelas(estrelas);
            uiManager.MostrarFeedback("Não foi dessa vez! Tenta de novo!", false);
            StartCoroutine(DelayLimpar());
        }
    }

    private IEnumerator DelayMostrarVitoria()
    {
        yield return new WaitForSeconds(5f);
        yield return new WaitForSeconds(5f);
        uiManager.MostrarPainelVitoria(nivelAtual + 1, niveis.Length, estrelas);
    }

    private IEnumerator DelayLimpar()
    {
        yield return new WaitForSeconds(0.9f);
        LimparTudo();
    }

    private void FinalizarJogo() => uiManager.MostrarPainelFimDeJogo();

    public LevelData NivelAtual => niveis[nivelAtual];
    public int NumeroNivelAtual => nivelAtual + 1;
    public int TotalNiveis => niveis.Length;
}