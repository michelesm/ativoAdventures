package com.meuestudiodejogos.ativoadventure.healthconnect; 

import android.app.Activity;
import android.content.Intent;
import android.net.Uri;
import android.os.Handler;
import android.os.Looper;
import android.util.Log;

import androidx.activity.result.ActivityResultLauncher;
import androidx.activity.result.contract.ActivityResultContract;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.appcompat.app.AppCompatActivity; 
import androidx.health.connect.client.HealthConnectClient;
import androidx.health.connect.client.PermissionController;
import androidx.health.connect.client.permission.HealthPermission;
import androidx.health.connect.client.records.DistanceRecord;
import androidx.health.connect.client.records.Record;
import androidx.health.connect.client.records.StepsRecord;
import androidx.health.connect.client.records.TotalCaloriesBurnedRecord;
import androidx.health.connect.client.request.ReadRecordsRequest;
import androidx.health.connect.client.response.ReadRecordsResponse;
import androidx.health.connect.client.time.TimeRangeFilter;

import com.unity3d.player.UnityPlayer; // Para enviar mensagens ao Unity

import java.io.IOException;
import java.time.Instant;
import java.time.ZonedDateTime;
import java.time.temporal.ChronoUnit;
import java.util.Arrays;
import java.util.HashSet;
import java.util.List;
import java.util.Set;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;

public class HealthPlugin {
    private static final String TAG = "HealthPlugin";
    private static final String UNITY_GAME_OBJECT_NAME = "HealthConnectManager"; // GameObject no Unity que receberá as mensagens

    private static HealthPlugin instance;
    private HealthConnectClient healthConnectClient;
    private Activity unityActivity; // Referência à atividade principal do Unity

    // Permissões que vamos solicitar
    // ATENÇÃO: Mapeie para os tipos de dados corretos (StepsRecord, DistanceRecord, etc.)
    private final Set<String> PERMISSIONS =
        new HashSet<>(Arrays.asList(
            HealthPermission.getReadPermission(StepsRecord.class),
            HealthPermission.getReadPermission(DistanceRecord.class),
            HealthPermission.getReadPermission(TotalCaloriesBurnedRecord.class)
            // Adicione HealthPermission.getReadPermission(ActiveMinutesRecord.class) se existir ou ActiveCaloriesBurnedRecord.class
            // Para "Tempo de exercício (em minutos)" -> pode ser necessário usar ActiveMinutesRecord ou uma combinação.
            // Para "Sequência de dias ativos" -> Isso exigirá lógica de agregação de dados diários.
        ));

    public static HealthPlugin getInstance() {
        if (instance == null) {
            instance = new HealthPlugin();
        }
        return instance;
    }

    // Método chamado pelo Unity para inicializar o plugin
    public void initialize(Activity activity) {
        this.unityActivity = activity;
        Log.d(TAG, "HealthPlugin Initialized. Unity Activity: " + activity);
        try {
             if (HealthConnectClient.isProviderAvailable(unityActivity)) {
                healthConnectClient = HealthConnectClient.getOrCreate(unityActivity);
                Log.d(TAG, "Health Connect Client created.");
            } else {
                Log.w(TAG, "Health Connect Provider is NOT available.");
                sendDataToUnity("OnHealthConnectNotAvailable", "");
            }
        } catch (Exception e) {
            Log.e(TAG, "Error initializing Health Connect Client: " + e.getMessage());
            sendDataToUnity("OnHealthConnectError", "InitError: " + e.getMessage());
        }
    }

    // Método chamado pelo Unity para verificar e solicitar permissões
    public void requestPermissions() {
        if (healthConnectClient == null || unityActivity == null) {
            Log.e(TAG, "HealthConnectClient or UnityActivity is null. Cannot request permissions.");
            sendDataToUnity("OnPermissionRequestResult", "Error:NotInitialized");
            return;
        }

        Intent intent = PermissionController.createRequestPermissionIntent(healthConnectClient, PERMISSIONS);
        if (unityActivity != null) {
             // Criar um contrato para ActivityResult
            ActivityResultContract<Set<String>, Set<String>> contract = PermissionController.createRequestPermissionResultContract();

            Intent permissionIntent = new Intent(unityActivity, PermissionActivity.class);
            permissionIntent.putExtra(PermissionActivity.EXTRA_PERMISSIONS, new HashSet<>(PERMISSIONS));
            unityActivity.startActivity(permissionIntent);
            // O resultado será enviado de PermissionActivity para Unity.
        } else {
             Log.e(TAG, "UnityActivity is null, cannot start PermissionActivity.");
             sendDataToUnity("OnPermissionRequestResult", "Error:UnityActivityNull");
        }
    }


    // Método para verificar as permissões já concedidas (chamado pelo Unity)
    public void checkGrantedPermissions() {
        if (healthConnectClient == null) {
            Log.e(TAG, "HealthConnectClient is null.");
            sendDataToUnity("OnPermissionsChecked", "Error:NotInitialized");
            return;
        }
        ExecutorService executor = Executors.newSingleThreadExecutor();
        executor.execute(() -> {
            try {
                Set<String> grantedPermissions = healthConnectClient.getPermissionController().getGrantedPermissions();
                StringBuilder result = new StringBuilder();
                for (String perm : PERMISSIONS) {
                    result.append(perm.substring(perm.lastIndexOf('.') + 1)) // Nome curto da permissão
                          .append(":")
                          .append(grantedPermissions.contains(perm) ? "Granted" : "Denied")
                          .append(";");
                }
                Log.d(TAG, "Checked Permissions: " + result.toString());
                sendDataToUnity("OnPermissionsChecked", result.toString());
            } catch (Exception e) {
                Log.e(TAG, "Error checking granted permissions: " + e.getMessage());
                sendDataToUnity("OnPermissionsChecked", "Error:" + e.getMessage());
            }
        });
    }


    // Método para ler dados (ex: passos de hoje)
    public void readTodaySteps() {
        if (healthConnectClient == null) {
            Log.e(TAG, "HealthConnectClient is null. Cannot read steps.");
            sendDataToUnity("OnStepsDataReceived", "Error:NotInitialized");
            return;
        }

        ExecutorService executor = Executors.newSingleThreadExecutor();
        executor.execute(() -> {
            try {
                Instant endTime = Instant.now();
                Instant startTime = ZonedDateTime.now().truncatedTo(ChronoUnit.DAYS).toInstant(); // Início do dia de hoje

                ReadRecordsRequest<StepsRecord> request = new ReadRecordsRequest<>(
                        StepsRecord.class,
                        new TimeRangeFilter.Builder()
                                .setStartTime(startTime)
                                .setEndTime(endTime)
                                .build()
                );
                ReadRecordsResponse<StepsRecord> response = healthConnectClient.readRecords(request);
                long totalSteps = 0;
                for (StepsRecord stepsRecord : response.getRecords()) {
                    totalSteps += stepsRecord.getCount();
                }
                Log.d(TAG, "Total steps today: " + totalSteps);
                sendDataToUnity("OnStepsDataReceived", String.valueOf(totalSteps));
            } catch (Exception e) {
                Log.e(TAG, "Error reading steps: " + e.getMessage());
                 sendDataToUnity("OnStepsDataReceived", "Error:" + e.getMessage());
            }
        });
    }

    // Método para ler distância total de hoje
    public void readTodayDistance() {
        if (healthConnectClient == null) {
            Log.e(TAG, "HealthConnectClient is null. Cannot read distance.");
            sendDataToUnity("OnDistanceDataReceived", "Error:NotInitialized");
            return;
        }
        ExecutorService executor = Executors.newSingleThreadExecutor();
        executor.execute(() -> {
            try {
                Instant endTime = Instant.now();
                Instant startTime = ZonedDateTime.now().truncatedTo(ChronoUnit.DAYS).toInstant();

                ReadRecordsRequest<DistanceRecord> request = new ReadRecordsRequest<>(
                        DistanceRecord.class,
                        new TimeRangeFilter.Builder().setStartTime(startTime).setEndTime(endTime).build()
                );
                ReadRecordsResponse<DistanceRecord> response = healthConnectClient.readRecords(request);
                double totalDistanceMeters = 0;
                for (DistanceRecord distanceRecord : response.getRecords()) {
                    totalDistanceMeters += distanceRecord.getDistance().getInMeters();
                }
                Log.d(TAG, "Total distance today (meters): " + totalDistanceMeters);
                sendDataToUnity("OnDistanceDataReceived", String.valueOf(totalDistanceMeters));
            } catch (Exception e) {
                Log.e(TAG, "Error reading distance: " + e.getMessage());
                sendDataToUnity("OnDistanceDataReceived", "Error:" + e.getMessage());
            }
        });
    }


    // Método utilitário para enviar dados de volta para o Unity
    private void sendDataToUnity(String methodName, String message) {
        Log.d(TAG, "Sending to Unity - GameObject: " + UNITY_GAME_OBJECT_NAME + ", Method: " + methodName + ", Message: " + message);
         if (unityActivity != null) {
            // Garante que a chamada para UnityPlayer.UnitySendMessage seja feita na thread principal
            new Handler(Looper.getMainLooper()).post(() -> {
                try {
                    UnityPlayer.UnitySendMessage(UNITY_GAME_OBJECT_NAME, methodName, message);
                } catch (UnsatisfiedLinkError ule) {
                    Log.e(TAG, "UnitySendMessage failed (UnsatisfiedLinkError): " + ule.getMessage() + ". This can happen if Unity environment is not fully set up or if called too early.");
                } catch (Exception e) {
                    Log.e(TAG, "UnitySendMessage failed: " + e.getMessage());
                }
            });
        } else {
            Log.w(TAG, "UnityActivity is null, cannot send message to Unity: " + methodName);
        }
    }

    // Método para redirecionar para a Play Store se o Health Connect não estiver instalado
    public void openHealthConnectInstall() {
        if (unityActivity == null) {
            Log.e(TAG, "UnityActivity is null, cannot open Play Store.");
            return;
        }
        Intent intent = new Intent(Intent.ACTION_VIEW, Uri.parse("market://details?id=com.google.android.apps.healthdata&url=healthconnect%3A%2F%2Fonboarding"));
        intent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK); // Necessário se chamado de um contexto não-Activity
        if (intent.resolveActivity(unityActivity.getPackageManager()) != null) {
            unityActivity.startActivity(intent);
        } else {
            // Se a Play Store não estiver disponível, abrir no navegador
            Intent webIntent = new Intent(Intent.ACTION_VIEW, Uri.parse("https://play.google.com/store/apps/details?id=com.google.android.apps.healthdata"));
            webIntent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
            if (webIntent.resolveActivity(unityActivity.getPackageManager()) != null) {
                unityActivity.startActivity(webIntent);
            } else {
                Log.e(TAG, "Cannot open Play Store or Web Browser for Health Connect.");
                sendDataToUnity("OnHealthConnectInstallAction", "Error:NoActivityToHandleIntent");
            }
        }
    }
}