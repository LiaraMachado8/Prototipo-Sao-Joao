using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AnswerSlot : MonoBehaviour
{
    [Header("Referências")]
    public TextMeshProUGUI textoLetra;
    public Image imagemFundo;

    private int meuIndice;
    private bool preenchido;

    /// Atualiza o conteúdo do slot
    public void Atualizar(string letra, bool estaPreenchido, int indice)
    {
        meuIndice = indice;
        preenchido = estaPreenchido;
        if (textoLetra) textoLetra.text = letra;
    }

    /// Atualiza a cor de fundo
    public void AtualizarCor(Color cor)
    {
        if (imagemFundo) imagemFundo.color = cor;
    }

    /// Chamado quando o jogador clica no slot para remover a letra
    public void OnClicar()
    {
        if (preenchido)
            GameManager.Instance.RemoverLetraDoSlot(meuIndice);
    }
}