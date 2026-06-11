using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Níveis do Jogo")]
    public LevelData[] niveis; // Arraste seus LevelData aqui no Inspector

    [Header("Referências de UI")]
    public UIManager uiManager;

    // Estado interno
    private int nivelAtual = 0;
    private List<string> letrasDigitadas = new List<string>();
    private HashSet<string> letrasUsadas = new HashSet<string>();
    private int erros = 0;
    private int estrelas = 3;

    // ─── Unity lifecycle ───────────────────────────────────────────────────────

    void Awake()
    {
        // Singleton simples
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        CarregarNivel(0);
    }

    // ─── Nível ─────────────────────────────────────────────────────────────────

    public void CarregarNivel(int index)
    {
        if (index >= niveis.Length) { FinalizarJogo(); return; }

        nivelAtual = index;
        erros = 0;
        estrelas = 3;
        letrasDigitadas.Clear();
        letrasUsadas.Clear();

        LevelData nivel = niveis[nivelAtual];
        uiManager.AtualizarNivel(nivel, nivelAtual + 1, niveis.Length);
        uiManager.AtualizarEstrelas(estrelas);
        uiManager.AtualizarSlots(letrasDigitadas, nivel.TamanhoDaResposta);
        uiManager.AtualizarAlfabeto(letrasUsadas);
        uiManager.LimparFeedback();
    }

    public void ProximoNivel() => CarregarNivel(nivelAtual + 1);
    public void ReiniciarJogo() => CarregarNivel(0);

    // ─── Ações do jogador ──────────────────────────────────────────────────────

    /// Chamado pelo LetterButton quando o jogador clica numa letra
    public void DigitarLetra(string letra)
    {
        LevelData nivel = niveis[nivelAtual];
        if (letrasUsadas.Contains(letra)) return;
        if (letrasDigitadas.Count >= nivel.TamanhoDaResposta) return;

        letrasDigitadas.Add(letra);
        letrasUsadas.Add(letra);

        uiManager.AtualizarSlots(letrasDigitadas, nivel.TamanhoDaResposta);
        uiManager.AtualizarAlfabeto(letrasUsadas);
    }

    /// Chamado pelo AnswerSlot quando o jogador clica num quadrado preenchido
    public void RemoverLetraDoSlot(int indice)
    {
        if (indice < 0 || indice >= letrasDigitadas.Count) return;

        string letra = letrasDigitadas[indice];
        letrasDigitadas.RemoveAt(indice);
        letrasUsadas.Remove(letra);

        uiManager.AtualizarSlots(letrasDigitadas, niveis[nivelAtual].TamanhoDaResposta);
        uiManager.AtualizarAlfabeto(letrasUsadas);
    }

    /// Apaga todas as letras digitadas
    public void LimparTudo()
    {
        letrasDigitadas.Clear();
        letrasUsadas.Clear();
        uiManager.AtualizarSlots(letrasDigitadas, niveis[nivelAtual].TamanhoDaResposta);
        uiManager.AtualizarAlfabeto(letrasUsadas);
        uiManager.LimparFeedback();
    }

    /// Verifica a resposta do jogador
    public void ConfirmarResposta()
    {
        LevelData nivel = niveis[nivelAtual];

        if (letrasDigitadas.Count < nivel.TamanhoDaResposta)
        {
            uiManager.MostrarFeedback("Preencha todas as letras! 🤔", false);
            return;
        }

        string tentativa = string.Join("", letrasDigitadas);

        if (nivel.ValidarResposta(tentativa))
        {
            uiManager.AnimarSlotsCorretos();
            StartCoroutine(DelayMostrarVitoria());
        }
        else
        {
            erros++;
            estrelas = Mathf.Max(0, 3 - erros);
            uiManager.AnimarSlotsErrados();
            uiManager.AtualizarEstrelas(estrelas);
            uiManager.MostrarFeedback("Não foi dessa vez! Tenta de novo! 😅", false);
            StartCoroutine(DelayLimpar());
        }
    }

    // ─── Coroutines ────────────────────────────────────────────────────────────

    private IEnumerator DelayMostrarVitoria()
    {
        yield return new WaitForSeconds(0.6f);
        uiManager.MostrarPainelVitoria(nivelAtual + 1, niveis.Length, estrelas);
    }

    private IEnumerator DelayLimpar()
    {
        yield return new WaitForSeconds(0.9f);
        LimparTudo();
    }

    private void FinalizarJogo()
    {
        uiManager.MostrarPainelFimDeJogo();
    }

    // ─── Getters úteis para UI ─────────────────────────────────────────────────

    public LevelData NivelAtual => niveis[nivelAtual];
    public int NumeroNivelAtual => nivelAtual + 1;
    public int TotalNiveis => niveis.Length;
}
