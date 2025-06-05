// Assets/_Project/Scripts/Core/SceneNavigation.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigation : MonoBehaviour
{
    private static SceneNavigation _instance;
    public static SceneNavigation Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<SceneNavigation>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("SceneNavigation");
                    _instance = go.AddComponent<SceneNavigation>();
                }
            }
            return _instance;
        }
    }

    // Nomes das Cenas (use constantes para evitar erros de digitação)
    public const string OpeningScene = "OpeningScene"; // Tela de abertura
    public const string TransitionScene = "TransitionScene"; // Tela de narrativa inicial
    public const string MainMenuScene = "MainMenuScene";
    public const string MapHubScene = "MapHubScene"; // Mapa/Hub inicial
    // Adicione nomes para cenas de áreas, mini-games, perfil, progresso, etc.
    // public const string ForestAreaScene = "ForestAreaScene";
    // public const string RunnerMinigameScene = "RunnerMinigameScene";
    // public const string UserProfileScene = "UserProfileScene";
    // public const string ProgressDashboardScene = "ProgressDashboardScene";


    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("SceneNavigation: Nome da cena não pode ser vazio!");
            return;
        }
        Debug.Log($"SceneNavigation: Loading scene - {sceneName}");
        // Adicionar lógica de tela de carregamento aqui se desejar (ex: ativar um painel de loading, carregar assincronamente)
        SceneManager.LoadScene(sceneName);

        // Se o UIManager for persistente e precisar encontrar elementos na nova cena:
        // UIManager.Instance?.FindGlobalUIElements();
    }

    // Métodos específicos para cada tela
    public void GoToOpeningScene() => LoadScene(OpeningScene);
    public void GoToTransitionScene() => LoadScene(TransitionScene);
    public void GoToMainMenu() => LoadScene(MainMenuScene);
    public void GoToMapHub() => LoadScene(MapHubScene);
    // public void GoToForestArea() => LoadScene(ForestAreaScene);
    // public void GoToUserProfile() => LoadScene(UserProfileScene);
    // public void GoToProgressDashboard() => LoadScene(ProgressDashboardScene);

    public void GoBack()
    {
        // Lógica simples de "voltar". Pode precisar ser mais inteligente
        // dependendo da complexidade da navegação (ex: usando um histórico de cenas).
        // Por enquanto, um exemplo: se não estiver no menu, volta para o MapHub. Se estiver no MapHub, vai para o MainMenu.
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene != MainMenuScene && currentScene != MapHubScene && currentScene != OpeningScene && currentScene != TransitionScene)
        {
            GoToMapHub(); // Se estiver em uma sub-área ou minigame, volta pro Hub
        }
        else if (currentScene == MapHubScene)
        {
            GoToMainMenu(); // Se estiver no Hub, volta pro Menu
        }
    }
}