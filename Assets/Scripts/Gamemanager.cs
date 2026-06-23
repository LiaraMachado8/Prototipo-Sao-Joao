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

    // ── Nomes dos sons (devem bater com o groupID na SoundLibrary) ──────────
    [Header("Sound Effect IDs")]
    public string somLetraClicada = "letra_clicada";   // clique numa letra
    public string somLetraRemovida = "letra_removida";  // clique num slot para remover
    public string somApagar = "apagar";          // botão Apagar (limpar tudo)
    public string somErro = "erro";            // resposta errada
    public string somAcerto = "acerto";          // resposta certa
    public string somProximoNivel = "proximo_nivel";   // ao avançar de nível

    private int nivelAtual = 0;
    private List<string> letrasDigitadas = new List<string>();
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

    public void ProximoNivel()
    {
        PlaySom(somProximoNivel);
        CarregarNivel(nivelAtual + 1);
    }

    public void ReiniciarJogo() => CarregarNivel(0);

    // ─── Helpers ───────────────────────────────────────────────────────────────

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

    private int? UsosRestantes(string letra)
    {
        var naResposta = ContarNaResposta(niveis[nivelAtual].resposta);
        if (!naResposta.ContainsKey(letra)) return null;
        int jaUsados = contagemUsadas.ContainsKey(letra) ? contagemUsadas[letra] : 0;
        return naResposta[letra] - jaUsados;
    }

    // ─── Ações do jogador ──────────────────────────────────────────────────────

    public void DigitarLetra(string letra)
    {
        LevelData nivel = niveis[nivelAtual];
        if (letrasDigitadas.Count >= nivel.TamanhoDaResposta) return;

        int? restantes = UsosRestantes(letra);
        if (restantes == null)
        {
            if (contagemUsadas.ContainsKey(letra) && contagemUsadas[letra] > 0) return;
        }
        else if (restantes <= 0) return;

        letrasDigitadas.Add(letra);
        contagemUsadas[letra] = (contagemUsadas.ContainsKey(letra) ? contagemUsadas[letra] : 0) + 1;

        // 🔊 Som de letra clicada
        PlaySom(somLetraClicada);

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

        // 🔊 Som de letra removida
        PlaySom(somLetraRemovida);

        uiManager.AtualizarSlots(letrasDigitadas, niveis[nivelAtual].TamanhoDaResposta);
        uiManager.AtualizarAlfabeto(ContarNaResposta(niveis[nivelAtual].resposta), contagemUsadas);
    }

    public void LimparTudo()
    {
        if (letrasDigitadas.Count > 0)
            PlaySom(somApagar); // 🔊 Som de apagar (só toca se tinha algo)

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
            PlaySom(somAcerto); // 🔊 Som de acerto
            uiManager.AnimarSlotsCorretos();
            StartCoroutine(DelayMostrarVitoria());
        }
        else
        {
            erros++;
            estrelas = Mathf.Max(0, 3 - erros);
            PlaySom(somErro); // 🔊 Som de erro
            uiManager.AnimarSlotsErrados();
            uiManager.AtualizarEstrelas(estrelas);
            uiManager.MostrarFeedback("Não foi dessa vez! Tenta de novo!", false);
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

    private void FinalizarJogo() => uiManager.MostrarPainelFimDeJogo();

    // ─── Utilitário de som ─────────────────────────────────────────────────────

    private void PlaySom(string id)
    {
        if (SoundManager.Instance != null && !string.IsNullOrEmpty(id))
            SoundManager.Instance.PlaySound2D(id);
    }

    // ─── Getters ───────────────────────────────────────────────────────────────

    public LevelData NivelAtual => niveis[nivelAtual];
    public int NumeroNivelAtual => nivelAtual + 1;
    public int TotalNiveis => niveis.Length;
}