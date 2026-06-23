using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Pergunta")]
    public TextMeshProUGUI textoPergunta;
    public TextMeshProUGUI textoDica;
    public TextMeshProUGUI textoNivel;

    [Header("Grade de Letras")]
    public Transform containerAlfabeto;
    public GameObject prefabLetraBotao;

    [Header("Slots de Resposta")]
    public Transform containerSlots;
    public GameObject prefabSlot;

    [Header("Feedback e Estrelas")]
    public TextMeshProUGUI textoFeedback;
    public Image[] imagensEstrelas;

    [Header("Painéis")]
    public GameObject painelVitoria;
    public TextMeshProUGUI textoVitoriaEstrelas;
    public TextMeshProUGUI textoVitoriaNivel;
    public Button botaoProximoNivel;
    public Button botaoReiniciar;
    public GameObject painelFimDeJogo;

    [Header("Cores")]
    public Color corSlotVazio = new Color(1f, 1f, 1f, 0.15f);
    public Color corSlotPreenchido = new Color(1f, 0.85f, 0.2f, 0.4f);
    public Color corSlotCorreto = new Color(0.3f, 0.85f, 0.4f, 0.5f);
    public Color corSlotErrado = new Color(0.9f, 0.2f, 0.2f, 0.5f);
    public Color corEstrelaAtiva = new Color(1f, 0.85f, 0f);
    public Color corEstrelaInativa = new Color(0.4f, 0.4f, 0.4f);

    private List<LetterButton> botoesLetras = new List<LetterButton>();
    private List<AnswerSlot> slotsResposta = new List<AnswerSlot>();

    void Start()
    {
        ConstruirAlfabeto();
        if (painelVitoria) painelVitoria.SetActive(false);
        if (painelFimDeJogo) painelFimDeJogo.SetActive(false);
        if (botaoProximoNivel) botaoProximoNivel.onClick.AddListener(GameManager.Instance.ProximoNivel);
        if (botaoReiniciar) botaoReiniciar.onClick.AddListener(GameManager.Instance.ReiniciarJogo);
    }

    private void ConstruirAlfabeto()
    {
        foreach (Transform filho in containerAlfabeto) Destroy(filho.gameObject);
        botoesLetras.Clear();

        foreach (char c in "ABCDEFGHIJLMNOPQRSTUVZ")
        {
            GameObject obj = Instantiate(prefabLetraBotao, containerAlfabeto);
            LetterButton btn = obj.GetComponent<LetterButton>();
            btn.Inicializar(c.ToString());
            botoesLetras.Add(btn);
        }
    }

    // ─── Atualização de estado ─────────────────────────────────────────────────

    public void AtualizarNivel(LevelData nivel, int numero, int total)
    {
        if (textoPergunta) textoPergunta.text = nivel.pergunta;
        if (textoDica) textoDica.text = nivel.dica;
        if (textoNivel) textoNivel.text = $"NÍVEL {numero} DE {total}";
        if (painelVitoria) painelVitoria.SetActive(false);
        if (painelFimDeJogo) painelFimDeJogo.SetActive(false);
    }

    /// <summary>
    /// Atualiza o estado visual de cada botão de letra.
    /// naResposta  = quantas vezes cada letra aparece na resposta correta
    /// jaUsadas    = quantas vezes cada letra já foi digitada pelo jogador
    /// </summary>
    public void AtualizarAlfabeto(
        Dictionary<string, int> naResposta,
        Dictionary<string, int> jaUsadas)
    {
        foreach (var btn in botoesLetras)
        {
            string letra = btn.Letra;

            if (naResposta.ContainsKey(letra))
            {
                // Letra está na resposta: calcula usos restantes
                int usados = jaUsadas.ContainsKey(letra) ? jaUsadas[letra] : 0;
                int restantes = naResposta[letra] - usados;
                int total = naResposta[letra];

                btn.AtualizarEstadoComContador(
                    desativado: restantes <= 0,
                    mostrarContador: total >= 2,
                    contadorValor: restantes
                );
            }
            else
            {
                // Letra não está na resposta: desativa após usar uma vez
                bool usada = jaUsadas.ContainsKey(letra) && jaUsadas[letra] > 0;
                btn.AtualizarEstadoComContador(desativado: usada, mostrarContador: false, contadorValor: 0);
            }
        }
    }

    public void AtualizarSlots(List<string> letrasDigitadas, int tamanho)
    {
        if (slotsResposta.Count != tamanho) CriarSlots(tamanho);

        for (int i = 0; i < slotsResposta.Count; i++)
        {
            bool preenchido = i < letrasDigitadas.Count;
            string letra = preenchido ? letrasDigitadas[i] : "";
            slotsResposta[i].Atualizar(letra, preenchido, i);
            slotsResposta[i].AtualizarCor(preenchido ? corSlotPreenchido : corSlotVazio);
        }
    }

    private void CriarSlots(int quantidade)
    {
        foreach (Transform filho in containerSlots) Destroy(filho.gameObject);
        slotsResposta.Clear();
        for (int i = 0; i < quantidade; i++)
        {
            GameObject obj = Instantiate(prefabSlot, containerSlots);
            AnswerSlot slot = obj.GetComponent<AnswerSlot>();
            slotsResposta.Add(slot);
        }
    }

    public void AtualizarEstrelas(int quantidade)
    {
        if (imagensEstrelas == null) return;
        for (int i = 0; i < imagensEstrelas.Length; i++)
            if (imagensEstrelas[i])
                imagensEstrelas[i].color = i < quantidade ? corEstrelaAtiva : corEstrelaInativa;
    }

    // ─── Feedback ──────────────────────────────────────────────────────────────

    public void MostrarFeedback(string mensagem, bool sucesso)
    {
        if (!textoFeedback) return;
        textoFeedback.text = mensagem;
        textoFeedback.color = sucesso ? corSlotCorreto : corSlotErrado;
    }

    public void LimparFeedback()
    {
        if (textoFeedback) textoFeedback.text = "";
    }

    public void AnimarSlotsCorretos()
    {
        foreach (var slot in slotsResposta) slot.AtualizarCor(corSlotCorreto);
        MostrarFeedback("Correto! Muito bem! 🎉", true);
    }

    public void AnimarSlotsErrados() => StartCoroutine(AnimacaoErro());

    private IEnumerator AnimacaoErro()
    {
        foreach (var slot in slotsResposta) slot.AtualizarCor(corSlotErrado);
        yield return new WaitForSeconds(0.3f);
        if (containerSlots)
        {
            Vector3 pos = containerSlots.localPosition;
            for (int i = 0; i < 4; i++)
            {
                containerSlots.localPosition = pos + new Vector3(i % 2 == 0 ? 8f : -8f, 0, 0);
                yield return new WaitForSeconds(0.05f);
            }
            containerSlots.localPosition = pos;
        }
    }

    // ─── Painéis ───────────────────────────────────────────────────────────────

    public void MostrarPainelVitoria(int nivelAtual, int totalNiveis, int estrelas)
    {
        if (!painelVitoria) return;
        painelVitoria.SetActive(true);
        bool ultimoNivel = nivelAtual >= totalNiveis;

        if (textoVitoriaEstrelas)
        {
            string s = "";
            for (int i = 0; i < 3; i++) s += i < estrelas ? "⭐" : "☆";
            textoVitoriaEstrelas.text = s;
        }
        if (textoVitoriaNivel)
            textoVitoriaNivel.text = ultimoNivel
                ? "Você completou todos os níveis! 🏆"
                : $"Nível {nivelAtual} concluído!";

        if (botaoProximoNivel) botaoProximoNivel.gameObject.SetActive(!ultimoNivel);
        if (botaoReiniciar) botaoReiniciar.gameObject.SetActive(ultimoNivel);
    }

    public void MostrarPainelFimDeJogo()
    {
        if (painelFimDeJogo) painelFimDeJogo.SetActive(true);
    }
}