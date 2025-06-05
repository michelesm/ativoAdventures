// Assets/_Project/Scripts/UI/UIManager.cs
using UnityEngine;
using UnityEngine.UI; // Necessário para componentes de UI como Text, Button, Image
using System.Collections.Generic; // Para Dictionary
using TMPro; // Se estiver usando TextMeshPro (recomendado)

public class UIManager : MonoBehaviour
{
    private static UIManager _instance;
    public static UIManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<UIManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("UIManager");
                    _instance = go.AddComponent<UIManager>();
                }
            }
            return _instance;
        }
    }

    [Header("Player HUD Elements")]
    public TextMeshProUGUI energyText; // Arrastar o componente TextMeshProUGUI aqui
    public TextMeshProUGUI livesText;  // Arrastar o componente TextMeshProUGUI aqui
    public TextMeshProUGUI coinsText;  // Arrastar o componente TextMeshProUGUI aqui
    public Image[] lifeHearts; // Array de Imagens para os corações de vida

    [Header("Popups & Screens")]
    public GameObject healthConnectNotAvailablePopup; // Arrastar o painel/popup aqui
    public GameObject errorPopup; // Arrastar o painel/popup aqui
    public TextMeshProUGUI errorPopupText; // Texto dentro do errorPopup
    public GameObject permissionRequestPopup; // Popup para explicar por que as permissões são necessárias
    public Button requestPermissionButton; // Botão no permissionRequestPopup

    // Adicione referências para outras telas/elementos de UI conforme necessário

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        // DontDestroyOnLoad(gameObject); // UIManager pode ser específico da cena ou persistente.
        // Se for persistente, precisa de mais lógica para lidar com referências de UI entre cenas.
        // Para simplificar, vamos assumir que cada cena com UI significativa terá seu UIManager
        // ou que este UIManager é robusto o suficiente.
        // Se for um UIManager global, as referências (energyText, etc.) precisam ser encontradas
        // dinamicamente ou serem parte de um prefab de UI persistente.
        // Por ora, vamos torná-lo persistente e assumir que os elementos de HUD são consistentes ou
        // são atualizados/encontrados ao carregar novas cenas.
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        PlayerData.OnEnergyChanged += UpdateEnergyUI;
        PlayerData.OnLivesChanged += UpdateLivesUI;
        PlayerData.OnCoinsChanged += UpdateCoinsUI;
        HealthConnectManager.OnPermissionsResult += HandlePermissionUI;
    }

    void OnDisable()
    {
        PlayerData.OnEnergyChanged -= UpdateEnergyUI;
        PlayerData.OnLivesChanged -= UpdateLivesUI;
        PlayerData.OnCoinsChanged -= UpdateCoinsUI;
        HealthConnectManager.OnPermissionsResult -= HandlePermissionUI;
    }

    void Start()
    {
        // Inicializar UI com dados atuais (caso já estejam carregados)
        if (PlayerData.Instance != null)
        {
            UpdateEnergyUI(PlayerData.Instance.stats.currentEnergy, PlayerData.Instance.stats.maxEnergy);
            UpdateLivesUI(PlayerData.Instance.stats.currentLives, PlayerStats.MaxLives);
            UpdateCoinsUI(PlayerData.Instance.stats.coins);
        }

        // Esconder popups inicialmente
        if (healthConnectNotAvailablePopup) healthConnectNotAvailablePopup.SetActive(false);
        if (errorPopup) errorPopup.SetActive(false);
        if (permissionRequestPopup) permissionRequestPopup.SetActive(false);

        // Configurar listeners de botões (exemplo)
        if (requestPermissionButton)
        {
            requestPermissionButton.onClick.AddListener(() =>
            {
                HealthConnectManager.Instance.RequestHealthPermissions();
                if (permissionRequestPopup) permissionRequestPopup.SetActive(false);
            });
        }
    }

    public void FindGlobalUIElements()
    {
        // Chame isso após carregar uma nova cena se o UIManager for persistente
        // e os elementos de UI não forem filhos diretos ou parte de um prefab persistente.
        // Exemplo:
        // GameObject hudCanvas = GameObject.Find("PlayerHUDCanvas"); // Supondo que seu HUD tenha este nome
        // if (hudCanvas != null) {
        //    energyText = hudCanvas.transform.Find("EnergyText").GetComponent<TextMeshProUGUI>();
        //    // ... encontrar outros elementos
        // }
        // Esta abordagem é frágil. É melhor ter um prefab de HUD persistente ou
        // UIManagers por cena que se registram/desregistram.
        // Por simplicidade, vamos assumir que você arrasta os elementos persistentes no Inspector.
    }


    void UpdateEnergyUI(int current, int max)
    {
        if (energyText != null)
        {
            energyText.text = $"Energia: {current} / {max}";
        }
    }

    void UpdateLivesUI(int current, int max)
    {
        if (livesText != null)
        {
            livesText.text = $"Vidas: {current}";
        }
        if (lifeHearts != null)
        {
            for (int i = 0; i < lifeHearts.Length; i++)
            {
                lifeHearts[i].gameObject.SetActive(i < current);
            }
        }
    }

    void UpdateCoinsUI(int current)
    {
        if (coinsText != null)
        {
            coinsText.text = $"Moedas: {current}";
        }
    }

    public void ShowHealthConnectNotAvailablePopup()
    {
        if (healthConnectNotAvailablePopup)
        {
            healthConnectNotAvailablePopup.SetActive(true);
            // Adicionar um botão no popup para chamar HealthConnectManager.Instance.TryOpenHealthConnectInstall()
            Button installButton = healthConnectNotAvailablePopup.GetComponentInChildren<Button>(); // Exemplo
            if (installButton != null)
            {
                installButton.onClick.RemoveAllListeners(); // Evitar múltiplos listeners
                installButton.onClick.AddListener(() =>
                {
                    HealthConnectManager.Instance.TryOpenHealthConnectInstall();
                    healthConnectNotAvailablePopup.SetActive(false);
                });
            }
        }
    }

    public void ShowErrorPopup(string message)
    {
        if (errorPopup && errorPopupText)
        {
            errorPopupText.text = message;
            errorPopup.SetActive(true);
            // Adicionar um botão "OK" para fechar o popup
            Button okButton = errorPopup.GetComponentInChildren<Button>(); // Exemplo
            if (okButton != null)
            {
                okButton.onClick.RemoveAllListeners();
                okButton.onClick.AddListener(() => errorPopup.SetActive(false));
            }
        }
    }

    private void HandlePermissionUI(Dictionary<string, bool> permissionsStatus)
    {
        bool allGranted = true;
        bool showError = false;
        string missingPermissions = "Permissões necessárias: ";

        foreach (var kvp in permissionsStatus)
        {
            if (kvp.Key == "Error")
            {
                showError = true;
                break;
            }
            if (!kvp.Value)
            {
                allGranted = false;
                missingPermissions += kvp.Key + " "; // Adiciona o nome da permissão faltando
            }
        }

        if (showError)
        {
            ShowErrorPopup("Erro ao verificar permissões. Tente novamente.");
            if (permissionRequestPopup) permissionRequestPopup.SetActive(true); // Mostrar novamente o popup para tentar
            return;
        }

        if (!allGranted)
        {
            Debug.LogWarning("UIManager: Nem todas as permissões foram concedidas. " + missingPermissions);
            if (permissionRequestPopup)
            {
                // Atualizar texto do popup de permissão se necessário
                // TextMeshProUGUI permissionInfoText = permissionRequestPopup.transform.Find("InfoText").GetComponent<TextMeshProUGUI>();
                // if(permissionInfoText) permissionInfoText.text = "Para progredir, precisamos de acesso aos seus dados de atividade: " + missingPermissions;
                permissionRequestPopup.SetActive(true);
            }
        }
        else
        {
            Debug.Log("UIManager: Todas as permissões necessárias foram concedidas!");
            if (permissionRequestPopup) permissionRequestPopup.SetActive(false);
            // Agora que as permissões estão OK, podemos buscar dados
            HealthConnectManager.Instance.FetchTodaySteps();
            HealthConnectManager.Instance.FetchTodayDistance();
        }
    }


}