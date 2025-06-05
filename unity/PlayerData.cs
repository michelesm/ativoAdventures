// Assets/_Project/Scripts/Core/PlayerData.cs
using UnityEngine;
using System;

[System.Serializable] // Para poder salvar/carregar facilmente se necessário no futuro
public class PlayerStats
{
    public int currentEnergy;
    public int maxEnergy = 1000; // Exemplo
    public int currentLives;
    public const int MaxLives = 5;
    public int coins;

    // Métricas de atividade física acumuladas (para desbloqueios)
    public long totalStepsEver;
    public double totalDistanceEver; // em metros
    public float totalExerciseMinutesEver;
    public int consecutiveActiveDays;

    // Itens e Roupas (poderiam ser listas de IDs ou objetos mais complexos)
    // public List<string> unlockedSkins = new List<string>();
    // public List<string> ownedItems = new List<string>();

    public PlayerStats()
    {
        currentLives = MaxLives;
        currentEnergy = 0;
        coins = 0;
        totalStepsEver = 0;
        totalDistanceEver = 0;
        totalExerciseMinutesEver = 0;
        consecutiveActiveDays = 0;
    }
}

public class PlayerData : MonoBehaviour
{
    private static PlayerData _instance;
    public static PlayerData Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<PlayerData>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("PlayerData");
                    _instance = go.AddComponent<PlayerData>();
                }
            }
            return _instance;
        }
    }

    public PlayerStats stats = new PlayerStats();

    // Eventos para UI e Game Logic
    public static event Action<int, int> OnEnergyChanged; // current, max
    public static event Action<int, int> OnLivesChanged;  // current, max
    public static event Action<int> OnCoinsChanged;

    // Constantes de conversão (BALANCEAR ESTES VALORES!)
    private const int ENERGY_PER_1000_STEPS = 50;
    private const float ENERGY_PER_KM_DISTANCE = 100; // 100 energia por km
    private const int LIVES_RECOVERY_THRESHOLD_STEPS = 5000; // Passos para recuperar 1 vida
    private bool canRecoverLifeToday = true; // Limite diário para recuperação de vida

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        LoadPlayerData(); // Implementar se quiser persistência
    }

    void OnEnable()
    {
        HealthConnectManager.OnStepsDataUpdated += HandleStepsData;
        HealthConnectManager.OnDistanceDataUpdated += HandleDistanceData;
        // Inscreva-se em outros eventos de dados (calorias, tempo de exercício)
    }

    void OnDisable()
    {
        HealthConnectManager.OnStepsDataUpdated -= HandleStepsData;
        HealthConnectManager.OnDistanceDataUpdated -= HandleDistanceData;
        // Desinscreva-se
    }

    private void HandleStepsData(long stepsToday)
    {
        if (stepsToday < 0) return; // Erro ou indisponível

        // Exemplo: Converter passos em energia
        // Esta é uma lógica simplista. Você pode querer apenas adicionar energia baseada nos passos *novos* desde a última sincronização.
        // Para este exemplo, vamos assumir que `stepsToday` é o total do dia e queremos adicionar uma porção disso.
        // Uma abordagem melhor: armazenar `lastSyncedSteps` e calcular a diferença.
        long newSteps = stepsToday - stats.totalStepsEver; // Simplificação, idealmente seria "passos desde a última sincronização HOJE"
        if (newSteps > 0)
        {
            AddEnergy((int)(newSteps / 1000.0f * ENERGY_PER_1000_STEPS));
            stats.totalStepsEver += newSteps; // Atualiza o total geral
        }

        // Exemplo: Recuperar vida com passos
        if (stats.currentLives < PlayerStats.MaxLives && canRecoverLifeToday)
        {
            // Esta lógica precisa ser mais robusta, talvez baseada em "passos feitos hoje"
            // e não no total. E um limite diário.
            if (stepsToday >= LIVES_RECOVERY_THRESHOLD_STEPS) // Exemplo muito simples
            {
                // RecoverOneLife();
                // canRecoverLifeToday = false; // Para limitar a uma recuperação por dia via passos
                // Você precisará de um sistema para resetar `canRecoverLifeToday` diariamente.
            }
        }
        Debug.Log($"PlayerData: Steps updated to {stepsToday}. Current Energy: {stats.currentEnergy}");
    }

    private void HandleDistanceData(double distanceTodayMetres)
    {
        if (distanceTodayMetres < 0) return;

        double newDistanceKm = (distanceTodayMetres / 1000.0) - (stats.totalDistanceEver / 1000.0);
        if (newDistanceKm > 0)
        {
            AddEnergy((int)(newDistanceKm * ENERGY_PER_KM_DISTANCE));
            stats.totalDistanceEver += (newDistanceKm * 1000.0); // Adiciona em metros
        }
        Debug.Log($"PlayerData: Distance updated to {distanceTodayMetres}m. Current Energy: {stats.currentEnergy}");
    }

    public void AddEnergy(int amount)
    {
        if (amount <= 0) return;
        stats.currentEnergy = Mathf.Min(stats.currentEnergy + amount, stats.maxEnergy);
        OnEnergyChanged?.Invoke(stats.currentEnergy, stats.maxEnergy);
        Debug.Log($"Energy added: {amount}. Total: {stats.currentEnergy}");
    }

    public bool UseEnergy(int amount)
    {
        if (amount <= 0) return false;
        if (stats.currentEnergy >= amount)
        {
            stats.currentEnergy -= amount;
            OnEnergyChanged?.Invoke(stats.currentEnergy, stats.maxEnergy);
            Debug.Log($"Energy used: {amount}. Remaining: {stats.currentEnergy}");
            return true;
        }
        Debug.Log($"Not enough energy. Have: {stats.currentEnergy}, Need: {amount}");
        return false;
    }

    public void AddLives(int amount)
    {
        if (amount <= 0) return;
        stats.currentLives = Mathf.Min(stats.currentLives + amount, PlayerStats.MaxLives);
        OnLivesChanged?.Invoke(stats.currentLives, PlayerStats.MaxLives);
    }

    public void LoseLife()
    {
        if (stats.currentLives > 0)
        {
            stats.currentLives--;
            OnLivesChanged?.Invoke(stats.currentLives, PlayerStats.MaxLives);
            if (stats.currentLives == 0)
            {
                // Lógica de Game Over ou tela para recuperar vidas
                Debug.Log("Game Over - No lives left");
            }
        }
    }

    // Método para recuperar vidas através de atividade física (chamado por alguma lógica de jogo)
    public void RecoverLifeWithActivity()
    {
        if (stats.currentLives < PlayerStats.MaxLives)
        {
            // Aqui você pode verificar se o jogador cumpriu uma meta de atividade recente
            // Por exemplo, se `canRecoverLifeToday` for verdadeiro e uma certa quantidade de passos foi dada.
            // Esta é uma simplificação.
            AddLives(1);
            Debug.Log("Life recovered through activity!");
            // Adicionar lógica para limitar a recuperação (ex: uma vez por dia)
        }
    }


    public void AddCoins(int amount)
    {
        if (amount <= 0) return;
        stats.coins += amount;
        OnCoinsChanged?.Invoke(stats.coins);
    }

    public bool UseCoins(int amount)
    {
        if (stats.coins >= amount)
        {
            stats.coins -= amount;
            OnCoinsChanged?.Invoke(stats.coins);
            return true;
        }
        return false;
    }

    // TODO: Implementar SavePlayerData() e LoadPlayerData() usando PlayerPrefs, JSON, ou outra solução de persistência.
    public void SavePlayerData()
    {
        string json = JsonUtility.ToJson(stats);
        PlayerPrefs.SetString("PlayerStatsData", json);
        PlayerPrefs.Save();
        Debug.Log("Player Data Saved!");
    }

    public void LoadPlayerData()
    {
        if (PlayerPrefs.HasKey("PlayerStatsData"))
        {
            string json = PlayerPrefs.GetString("PlayerStatsData");
            stats = JsonUtility.FromJson<PlayerStats>(json);
            Debug.Log("Player Data Loaded!");
        }
        else
        {
            stats = new PlayerStats(); // Inicia com valores padrão se não houver save
            Debug.Log("No player data found, initialized with defaults.");
        }
        // Disparar eventos para atualizar a UI com os dados carregados
        OnEnergyChanged?.Invoke(stats.currentEnergy, stats.maxEnergy);
        OnLivesChanged?.Invoke(stats.currentLives, PlayerStats.MaxLives);
        OnCoinsChanged?.Invoke(stats.coins);
    }

    // Chamar este método no início de um novo dia para resetar limites diários
    public void ProcessNewDay()
    {
        canRecoverLifeToday = true;
        // Lógica para verificar sequência de dias ativos
        // Se (dados de atividade do dia anterior > 0) stats.consecutiveActiveDays++;
        // Else stats.consecutiveActiveDays = 0; (se o dia anterior não teve atividade significativa)
        // Isso requer salvar a data da última atividade.
        SavePlayerData();
    }
}