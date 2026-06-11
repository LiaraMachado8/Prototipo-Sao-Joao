using UnityEngine;

// Clique com botão direito na pasta Assets > Create > São João > Level Data
[CreateAssetMenu(fileName = "Level", menuName = "São João/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Conteúdo do Nível")]
    [TextArea(2, 4)]
    public string pergunta = "Escreva aqui a pergunta...";

    public string resposta = "PALAVRA"; // sempre em MAIÚSCULAS

    [TextArea(1, 2)]
    public string dica = "💡 Dica opcional aqui";

    [Header("Configurações do Nível")]
    public int numeroDoNivel = 1;
    public Sprite imagemDecoratica; // opcional

    // Valida a resposta do jogador (ignora maiúsculas/minúsculas)
    public bool ValidarResposta(string tentativa)
    {
        return tentativa.ToUpper().Trim() == resposta.ToUpper().Trim();
    }

    public int TamanhoDaResposta => resposta.Length;
}