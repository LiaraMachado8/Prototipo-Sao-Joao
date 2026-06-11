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
    public Transform containerAlfabeto;    // GridLayoutGroup aqui
    public GameObject prefabLetraBotao;    // Prefab do LetterButton

    [Header("Slots de Resposta")]
    public Transform containerSlots;       // HorizontalLayoutGroup aqui
    public GameObject prefabSlot;          // Prefab do AnswerSlot

    [Header("Feedback e Estrelas")]
    public TextMeshProUGUI textoFeedback;
    public Image[] imagensEstrelas;        // 3 imagens de estrela

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

    // Listas de objetos instanciados
    private List<LetterButton> botoesLetras = new List<LetterButton>();
    private List<AnswerSlot> slotsResposta = new List<AnswerSlot>();

    // ─── Inicialização ─────────────────────────────────────────────────────────

    void Start()
    {
        ConstruirAlfabeto();

        if (painelVitoria) painelVitoria.SetActive(false);
        if (painelFimDeJogo) painelFimDeJogo.SetActive(false);

        // Conectar botões dos painéis
        if (botaoProximoNivel) botaoProximoNivel.onClick.AddListener(GameManager.Instance.ProximoNivel);
        if (botaoReiniciar) botaoReiniciar.onClick.AddListener(GameManager.Instance.ReiniciarJogo);
    }

    private void ConstruirAlfabeto()
    {
        // Limpa o que tiver
        foreach (Transform filho in containerAlfabeto) Destroy(filho.gameObject);
        botoesLetras.Clear();

        string alfabeto = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        foreach (char c in alfabeto)
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

    public void AtualizarAlfabeto(HashSet<string> letrasUsadas)
    {
        foreach (var btn in botoesLetras)
            btn.AtualizarEstado(letrasUsadas.Contains(btn.Letra));
    }

    public void AtualizarSlots(List<string> letrasDigitadas, int tamanho)
    {
        // Recria os slots se o tamanho mudou
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

    // ─── Animações de Slots ────────────────────────────────────────────────────

    public void AnimarSlotsCorretos()
    {
        foreach (var slot in slotsResposta)
            slot.AtualizarCor(corSlotCorreto);
        MostrarFeedback("Correto! Muito bem! 🎉", true);
    }

    public void AnimarSlotsErrados()
    {
        StartCoroutine(AnimacaoErro());
    }

    private IEnumerator AnimacaoErro()
    {
        foreach (var slot in slotsResposta) slot.AtualizarCor(corSlotErrado);
        yield return new WaitForSeconds(0.3f);
        // Shake simples: move o container para direita e esquerda
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
