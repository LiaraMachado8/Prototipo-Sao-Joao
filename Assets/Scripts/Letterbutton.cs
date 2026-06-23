using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class LetterButton : MonoBehaviour
{
    [Header("Referências")]
    public TextMeshProUGUI textoLetra;
    public TextMeshProUGUI textoContador;  // pequeno texto abaixo da letra (ex: "×2")
    public Image imagemFundo;

    [Header("Cores")]
    public Color corAtiva = new Color(1f, 1f, 1f, 0.20f);
    public Color corUsada = new Color(1f, 1f, 1f, 0.05f);
    public Color corTextoAtivo = Color.white;
    public Color corTextoUsado = new Color(1f, 1f, 1f, 0.25f);
    public Color corContador = new Color(1f, 0.85f, 0f);  // dourado

    private Button botao;
    public string Letra { get; private set; }

    void Awake() => botao = GetComponent<Button>();

    public void Inicializar(string letra)
    {
        Letra = letra;
        if (textoLetra) textoLetra.text = letra;
        botao.onClick.AddListener(OnClicar);
        AtualizarEstadoComContador(false, false, 0);
    }

    /// <summary>
    /// desativado     — letra esgotou os usos permitidos
    /// mostrarContador — mostrar badge "×N" (só para letras que aparecem 2+ vezes)
    /// contadorValor  — quantos usos ainda restam
    /// </summary>
    public void AtualizarEstadoComContador(bool desativado, bool mostrarContador, int contadorValor)
    {
        botao.interactable = !desativado;
        if (imagemFundo) imagemFundo.color = desativado ? corUsada : corAtiva;
        if (textoLetra) textoLetra.color = desativado ? corTextoUsado : corTextoAtivo;

        if (textoContador != null)
        {
            textoContador.gameObject.SetActive(mostrarContador);
            if (mostrarContador)
            {
                textoContador.text = $"×{Mathf.Max(0, contadorValor)}";
                textoContador.color = corContador;
            }
        }
    }

    private void OnClicar() => GameManager.Instance.DigitarLetra(Letra);
}