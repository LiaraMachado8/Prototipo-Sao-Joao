using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class LetterButton : MonoBehaviour
{
    [Header("Referências")]
    public TextMeshProUGUI textoLetra;
    public Image imagemFundo;

    [Header("Cores")]
    public Color corAtiva = new Color(1f, 1f, 1f, 0.2f);
    public Color corUsada = new Color(1f, 1f, 1f, 0.05f);
    public Color corTextoAtivo = Color.white;
    public Color corTextoUsado = new Color(1f, 1f, 1f, 0.25f);

    private Button botao;
    public string Letra { get; private set; }

    void Awake()
    {
        botao = GetComponent<Button>();
    }

    /// Chamado pelo UIManager ao construir a grade
    public void Inicializar(string letra)
    {
        Letra = letra;
        if (textoLetra) textoLetra.text = letra;
        botao.onClick.AddListener(OnClicar);
        AtualizarEstado(false);
    }

    /// Atualiza visual conforme letra já foi usada ou não
    public void AtualizarEstado(bool usada)
    {
        botao.interactable = !usada;
        if (imagemFundo) imagemFundo.color = usada ? corUsada : corAtiva;
        if (textoLetra) textoLetra.color = usada ? corTextoUsado : corTextoAtivo;
    }

    private void OnClicar()
    {
        GameManager.Instance.DigitarLetra(Letra);
    }
}