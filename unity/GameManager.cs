// Assets/_Project/Scripts/Core/GameManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public enum GameState
{
    Initializing,
    MainMenu,
    Playing, // No mapa principal ou em um mini-game
    Paused,
    GameOver
}

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("GameManager");
                    _instance = go.AddComponent<GameManager>();
                }
            }
            return _instance;
        }
    }

    public GameState CurrentState { get; private set; }
    public static event Action<GameState> OnGameStateChanged;

    // Referências para outros managers (podem ser atribuídas via Inspector ou encontradas no Awake)
    public HealthConnectManager healthManager;
    public PlayerData playerData;
    public UIManager uiManager;

    // Configurações do Core Loop
    private float dataSyncInterval = 300f; // Sincronizar dados a cada 5 minutos (300s)
    private float lastSyncTime;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        // Encontrar ou garantir que os outros singletons existam
        healthManager = HealthConnectManager.Instance;
        playerData = PlayerData.Instance;
        // uiManager será encontrado ou criado por si mesmo se seguir o padrão singleton

        lastSyncTime = Time.time;
    }

    void Start()
    {
        UpdateGameState(GameState.Initializing);
        // A inicialização do HealthConnect é assíncrona.
        // Podemos esperar pelo evento HealthConnectManager.OnHealthConnectInitialized
        // ou HealthConnectManager.OnHealthConnectNotAvailableEvent
        HealthConnectManager.OnHealthConnectInitialized += HandleHealthConnectReady;
        HealthConnectManager.OnHealthConnectNotAvailableEvent += HandleHealthConnectNotReady;
        HealthConnectManager.OnHealthConnectErrorEvent += HandleHealthConnectError;

        // Carregar a cena do menu principal após a inicialização
        // (ou deixar a cena de inicialização lidar com isso)
    }

    void Update()
    {
        // Core Loop - Sincronização de dados periódica
        if (CurrentState == GameState.Playing || CurrentState == GameState.MainMenu) // Sincronizar mesmo no menu
        {
            if (Time.time - lastSyncTime > dataSyncInterval)
            {
                if (healthManager != null && healthManager.IsHealthConnectAvailable)
                {
                    Debug.Log("GameManager: Performing periodic data sync.");
                    healthManager.FetchTodaySteps();
                    healthManager.FetchTodayDistance();
                    // Adicionar fetches para outros dados
                }
                lastSyncTime = Time.time;
            }
        }
    }

    private void HandleHealthConnectReady()
    {
        Debug.Log("GameManager: Health Connect is Ready. Checking permissions or fetching data.");
        // Após HC estar pronto, podemos verificar permissões ou buscar dados iniciais
        healthManager.RequestHealthPermissions(); // Ou healthManager.checkGrantedPermissions();
        // A resposta de permissão irá disparar OnPermissionsResult no HealthConnectManager
        // Podemos nos inscrever nesse evento aqui também, se necessário, ou deixar o PlayerData/UIManager lidar.

        // Exemplo: Após HC estar pronto, buscar dados iniciais se as permissões já estiverem OK
        // Isso pode ser feito após o callback de OnPermissionsResult
        // healthManager.FetchTodaySteps();
        // healthManager.FetchTodayDistance();

        // Mudar para o menu principal se estivermos em Inicializando
        if(CurrentState == GameState.Initializing)
        {
             // Se a cena atual não for o menu, carregue-o
            if (SceneManager.GetActiveScene().name != "MainMenuScene") // Substitua pelo nome da sua cena de menu
            {
                LoadScene("MainMenuScene"); // Implemente LoadScene para mudar para o menu
            }
            UpdateGameState(GameState.MainMenu);
        }
    }

    private void HandleHealthConnectNotReady()
    {
        Debug.LogWarning("GameManager: Health Connect is NOT Ready/Available.");
        // Informar o UIManager para mostrar uma mensagem/botão para instalar o HC
        UIManager.Instance?.ShowHealthConnectNotAvailablePopup();

        if(CurrentState == GameState.Initializing)
        {
            if (SceneManager.GetActiveScene().name != "MainMenuScene")
            {
                LoadScene("MainMenuScene");
            }
            UpdateGameState(GameState.MainMenu); // Ainda vai para o menu, mas com funcionalidade limitada
        }
    }
    private void HandleHealthConnectError(string error)
    {
        Debug.LogError($"GameManager: Health Connect Error: {error}");
        UIManager.Instance?.ShowErrorPopup("Health Connect Error: " + error);

        if(CurrentState == GameState.Initializing)
        {
             if (SceneManager.GetActiveScene().name != "MainMenuScene")
            {
                LoadScene("MainMenuScene");
            }
            UpdateGameState(GameState.MainMenu);
        }
    }


    public void UpdateGameState(GameState newState)
    {
        if (CurrentState == newState) return;

        CurrentState = newState;
        OnGameStateChanged?.Invoke(newState);

        switch (newState)
        {
            case GameState.Initializing:
                // Lógica de inicialização
                break;
            case GameState.MainMenu:
                // Lógica ao entrar no menu principal
                Time.timeScale = 1f; // Garantir que o tempo está normal
                break;
            case GameState.Playing:
                // Lógica ao iniciar/retomar o jogo
                Time.timeScale = 1f;
                break;
            case GameState.Paused:
                Time.timeScale = 0f; // Pausa o jogo
                break;
            case GameState.GameOver:
                // Lógica de game over
                Time.timeScale = 1f; // Ou 0f dependendo do que a tela de game over faz
                // Ex: LoadScene("GameOverScene");
                break;
        }
        Debug.Log($"Game State changed to: {newState}");
    }

    // Funções de navegação de cena (podem ser movidas para um SceneNavigation dedicado)
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
        // Pode adicionar lógica de tela de carregamento aqui
    }

    public void StartNewGame()
    {
        // Resetar dados do jogador para um novo jogo (ou parte deles)
        PlayerData.Instance.stats = new PlayerStats(); // Cuidado: isso apaga tudo
        PlayerData.Instance.SavePlayerData(); // Salva o estado resetado
        PlayerData.Instance.LoadPlayerData(); // Recarrega e dispara eventos para UI

        // Carregar a primeira cena do jogo/mapa
        LoadScene("MapHubScene"); // Substitua pelo nome da sua cena de mapa/hub
        UpdateGameState(GameState.Playing);
    }

    public void ContinueGame()
    {
        // PlayerData já deve estar carregado. Apenas carrega a cena apropriada.
        // Você pode querer salvar a última cena visitada pelo jogador.
        LoadScene("MapHubScene"); // Ou a última cena salva
        UpdateGameState(GameState.Playing);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        PlayerData.Instance.SavePlayerData(); // Salvar dados antes de sair
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Para sair do modo Play no Editor
        #endif
    }

    void OnApplicationQuit()
    {
        // Certifique-se de que os dados são salvos ao fechar o app
        PlayerData.Instance.SavePlayerData();
    }

     void OnApplicationPause(bool pauseStatus)
    {
        // No mobile, OnApplicationPause é chamado quando o app vai para segundo plano
        if (pauseStatus)
        {
            PlayerData.Instance.SavePlayerData();
        } else {
            // App voltou para primeiro plano, talvez sincronizar dados?
            if (healthManager != null && healthManager.IsHealthConnectAvailable)
            {
                Debug.Log("GameManager: App resumed, performing data sync.");
                healthManager.FetchTodaySteps();
                healthManager.FetchTodayDistance();
            }
        }
    }
}