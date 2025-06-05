// Assets/_Project/Scripts/Core/HealthConnectManager.cs
using UnityEngine;
using System; // Para Action
using System.Collections.Generic; // Para Dictionary

public class HealthConnectManager : MonoBehaviour
{
    private static HealthConnectManager _instance;
    public static HealthConnectManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<HealthConnectManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("HealthConnectManager");
                    _instance = go.AddComponent<HealthConnectManager>();
                }
            }
            return _instance;
        }
    }

    private AndroidJavaObject healthPluginInstance;
    private AndroidJavaObject unityActivity;

    public bool IsInitialized { get; private set; } = false;
    public bool IsHealthConnectAvailable { get; private set; } = false;

    // Eventos para notificar outras partes do jogo
    public static event Action OnHealthConnectInitialized;
    public static event Action OnHealthConnectNotAvailableEvent; // Renomeado para evitar conflito
    public static event Action<string> OnHealthConnectErrorEvent; // Renomeado
    public static event Action<Dictionary<string, bool>> OnPermissionsResult;
    public static event Action<long> OnStepsDataUpdated;
    public static event Action<double> OnDistanceDataUpdated;
    // Adicione eventos para outros dados (calorias, tempo de exercício)

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject); // Mantém o manager entre as cenas

        InitializePlugin();
    }

    void InitializePlugin()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            try
            {
                // Obter a atividade atual do Unity
                using (AndroidJavaClass unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    unityActivity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
                }

                // Obter a instância do plugin
                using (AndroidJavaClass pluginClass = new AndroidJavaClass("com.meuestudiodejogos.ativoadventure.healthconnect.HealthPlugin")) // SEU PACKAGE NAME DO PLUGIN + HealthPlugin
                {
                    if (pluginClass != null)
                    {
                        healthPluginInstance = pluginClass.CallStatic<AndroidJavaObject>("getInstance");
                        if (healthPluginInstance != null && unityActivity != null)
                        {
                            healthPluginInstance.Call("initialize", unityActivity);
                            IsInitialized = true; // Marcamos como inicializado aqui, mas OnHealthConnectInitialized será disparado pelo callback
                            Debug.Log("HealthConnectManager: Plugin instance obtained and initialize called.");
                        }
                        else
                        {
                            Debug.LogError("HealthConnectManager: Failed to get plugin instance or unityActivity is null.");
                            OnHealthConnectErrorEvent?.Invoke("PluginInstanceNull");
                        }
                    }
                    else
                    {
                        Debug.LogError("HealthConnectManager: Failed to get plugin class.");
                        OnHealthConnectErrorEvent?.Invoke("PluginClassNotFound");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("HealthConnectManager: Error initializing plugin: " + e.Message);
                OnHealthConnectErrorEvent?.Invoke("InitializationException: " + e.Message);
                IsInitialized = false;
            }
        }
        else
        {
            Debug.Log("HealthConnectManager: Not running on Android. Plugin not initialized.");
            IsInitialized = false;
        }
    }

    // Chamado pelo plugin Java quando a inicialização do Health Connect termina
    public void OnHealthConnectInitializedByPlugin(string message) // Nome do método como no Java (pode ser sem o ByPlugin)
    {
        Debug.Log("HealthConnectManager: OnHealthConnectInitializedByPlugin received from Java: " + message);
        IsHealthConnectAvailable = true; // Assumindo que se inicializou, está disponível
        OnHealthConnectInitialized?.Invoke();
    }

    public void OnHealthConnectNotAvailable(string message) // Chamado pelo Java
    {
        Debug.LogWarning("HealthConnectManager: Health Connect app is not available on the device. Message: " + message);
        IsHealthConnectAvailable = false;
        OnHealthConnectNotAvailableEvent?.Invoke();
    }

    public void OnHealthConnectError(string errorMessage) // Chamado pelo Java
    {
        Debug.LogError("HealthConnectManager: Health Connect Error from Plugin: " + errorMessage);
        OnHealthConnectErrorEvent?.Invoke(errorMessage);
    }


    public void RequestHealthPermissions()
    {
        if (IsInitialized && healthPluginInstance != null)
        {
            Debug.Log("HealthConnectManager: Requesting health permissions...");
            healthPluginInstance.Call("requestPermissions");
        }
        else
        {
            Debug.LogError("HealthConnectManager: Plugin not initialized. Cannot request permissions.");
            OnPermissionsResult?.Invoke(new Dictionary<string, bool> { { "Error", false } });
        }
    }

    // Chamado pelo plugin Java com o resultado da solicitação de permissão
    public void OnPermissionRequestResult(string message)
    {
        Debug.Log("HealthConnectManager: OnPermissionRequestResult received: " + message);
        // Exemplo de mensagem: "StepsRecord:Granted;DistanceRecord:Denied;"
        Dictionary<string, bool> permissionsStatus = ParsePermissionMessage(message);
        OnPermissionsResult?.Invoke(permissionsStatus);

        // Log para verificar
        foreach(var kvp in permissionsStatus)
        {
            Debug.Log($"Permission: {kvp.Key}, Status: {kvp.Value}");
        }
    }

    // Chamado pelo plugin Java com o resultado da verificação de permissões
    public void OnPermissionsChecked(string message)
    {
        Debug.Log("HealthConnectManager: OnPermissionsChecked received: " + message);
        Dictionary<string, bool> permissionsStatus = ParsePermissionMessage(message);
        OnPermissionsResult?.Invoke(permissionsStatus); // Pode usar o mesmo evento ou um dedicado
    }

    private Dictionary<string, bool> ParsePermissionMessage(string message)
    {
        var permissions = new Dictionary<string, bool>();
        if (message.StartsWith("Error:"))
        {
            permissions["Error"] = false;
            Debug.LogError("Permission message indicates error: " + message);
            return permissions;
        }

        string[] pairs = message.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string pair in pairs)
        {
            string[] keyValue = pair.Split(':');
            if (keyValue.Length == 2)
            {
                permissions[keyValue[0]] = keyValue[1].ToLower() == "granted";
            }
        }
        return permissions;
    }


    public void FetchTodaySteps()
    {
        if (IsInitialized && healthPluginInstance != null && IsHealthConnectAvailable)
        {
            Debug.Log("HealthConnectManager: Fetching today's steps...");
            healthPluginInstance.Call("readTodaySteps");
        }
        else
        {
            Debug.LogWarning("HealthConnectManager: Cannot fetch steps. Initialized: " + IsInitialized + ", Available: " + IsHealthConnectAvailable);
            OnStepsDataUpdated?.Invoke(-1); // Indica erro ou indisponibilidade
        }
    }

    // Chamado pelo plugin Java com os dados de passos
    public void OnStepsDataReceived(string stepsData)
    {
        Debug.Log("HealthConnectManager: OnStepsDataReceived: " + stepsData);
        if (long.TryParse(stepsData, out long steps))
        {
            OnStepsDataUpdated?.Invoke(steps);
        }
        else
        {
            Debug.LogError("HealthConnectManager: Failed to parse steps data: " + stepsData);
            OnStepsDataUpdated?.Invoke(-1); // Indica erro
            if (stepsData.ToLower().Contains("error:notinitialized") || stepsData.ToLower().Contains("error:healthconnectclient is null")) {
                 Debug.LogError("Health Connect client might not be initialized in the plugin. Check plugin logs.");
            } else if (stepsData.ToLower().Contains("error")) {
                 // Outro tipo de erro vindo do plugin
                 OnHealthConnectErrorEvent?.Invoke("StepsDataError: " + stepsData);
            }
        }
    }

    public void FetchTodayDistance()
    {
        if (IsInitialized && healthPluginInstance != null && IsHealthConnectAvailable)
        {
            Debug.Log("HealthConnectManager: Fetching today's distance...");
            healthPluginInstance.Call("readTodayDistance");
        }
        else
        {
            Debug.LogWarning("HealthConnectManager: Cannot fetch distance. Initialized: " + IsInitialized + ", Available: " + IsHealthConnectAvailable);
            OnDistanceDataUpdated?.Invoke(-1.0);
        }
    }

    // Chamado pelo plugin Java com os dados de distância
    public void OnDistanceDataReceived(string distanceData)
    {
        Debug.Log("HealthConnectManager: OnDistanceDataReceived: " + distanceData);
        if (double.TryParse(distanceData, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double distance))
        {
            OnDistanceDataUpdated?.Invoke(distance);
        }
        else
        {
            Debug.LogError("HealthConnectManager: Failed to parse distance data: " + distanceData);
            OnDistanceDataUpdated?.Invoke(-1.0);
             if (distanceData.ToLower().Contains("error")) {
                 OnHealthConnectErrorEvent?.Invoke("DistanceDataError: " + distanceData);
            }
        }
    }

    public void TryOpenHealthConnectInstall()
    {
        if (IsInitialized && healthPluginInstance != null)
        {
            Debug.Log("HealthConnectManager: Attempting to open Health Connect install page...");
            healthPluginInstance.Call("openHealthConnectInstall");
        }
        else
        {
            Debug.LogWarning("HealthConnectManager: Plugin not initialized. Cannot open install page.");
        }
    }

     public void OnHealthConnectInstallAction(string message) // Chamado pelo Java
    {
        Debug.Log("HealthConnectManager: OnHealthConnectInstallAction: " + message);
       
    }

}