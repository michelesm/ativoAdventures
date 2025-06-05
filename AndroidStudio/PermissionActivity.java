// PermissionActivity.java
package com.meuestudiodejogos.ativoadventure.healthconnect;

import android.app.Activity;
import android.content.Intent;
import android.os.Bundle;
import android.util.Log;
import android.widget.Toast;

import androidx.activity.result.ActivityResultCallback;
import androidx.activity.result.ActivityResultLauncher;
import androidx.appcompat.app.AppCompatActivity; // Importante
import androidx.health.connect.client.HealthConnectClient;
import androidx.health.connect.client.PermissionController;

import com.unity3d.player.UnityPlayer;

import java.util.HashSet;
import java.util.Set;

public class PermissionActivity extends AppCompatActivity { // Deve ser AppCompatActivity ou ComponentActivity
    private static final String TAG = "PermissionActivity";
    public static final String EXTRA_PERMISSIONS = "extra_permissions";
    private static final String UNITY_GAME_OBJECT_NAME = "HealthConnectManager";

    private ActivityResultLauncher<Set<String>> requestPermissionActivityContract;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        Log.d(TAG, "PermissionActivity onCreate");

        // Registrar o callback para o resultado da permissão
        requestPermissionActivityContract = registerForActivityResult(
            new PermissionController.RequestPermissions(), // Contrato do Health Connect
            new ActivityResultCallback<Set<String>>() {
                @Override
                public void onActivityResult(Set<String> grantedPermissions) {
                    Log.d(TAG, "Permission result received. Granted: " + grantedPermissions.size());
                    StringBuilder resultStr = new StringBuilder();
                    // Supondo que PERMISSIONS venha do intent original ou de uma fonte comum
                    Set<String> requestedPermissions = (Set<String>) getIntent().getSerializableExtra(EXTRA_PERMISSIONS);
                    if (requestedPermissions == null) requestedPermissions = new HashSet<>();

                    for (String perm : requestedPermissions) {
                         resultStr.append(perm.substring(perm.lastIndexOf('.') + 1)) // Nome curto
                               .append(":")
                               .append(grantedPermissions.contains(perm) ? "Granted" : "Denied")
                               .append(";");
                    }
                    sendDataToUnity("OnPermissionRequestResult", resultStr.toString());
                    finish(); // Fecha esta activity transparente
                }
            }
        );

        // Obter as permissões do Intent e lançar a solicitação
        Intent intent = getIntent();
        if (intent != null && intent.hasExtra(EXTRA_PERMISSIONS)) {
            @SuppressWarnings("unchecked") // Sabemos que é um HashSet<String>
            HashSet<String> permissions = (HashSet<String>) intent.getSerializableExtra(EXTRA_PERMISSIONS);
            if (permissions != null && !permissions.isEmpty()) {
                Log.d(TAG, "Requesting permissions: " + permissions.toString());
                requestPermissionActivityContract.launch(permissions);
            } else {
                Log.e(TAG, "No permissions passed to PermissionActivity.");
                sendDataToUnity("OnPermissionRequestResult", "Error:NoPermissionsInIntent");
                finish();
            }
        } else {
            Log.e(TAG, "Intent or extras missing in PermissionActivity.");
            sendDataToUnity("OnPermissionRequestResult", "Error:IntentOrExtrasMissing");
            finish();
        }
    }

    private void sendDataToUnity(String methodName, String message) {
        Log.d(TAG, "Sending to Unity from PermissionActivity - Method: " + methodName + ", Message: " + message);
        try {
            UnityPlayer.UnitySendMessage(UNITY_GAME_OBJECT_NAME, methodName, message);
        } catch (Exception e) {
            Log.e(TAG, "UnitySendMessage failed from PermissionActivity: " + e.getMessage());
        }
    }
}


build.gradle (:HealthConnectPlugin)
dependencies {
    implementation("androidx.health.connect:connect-client:1.1.0-alpha07")
    implementation("androidx.appcompat:appcompat:1.6.1") // Para AppCompatActivity
    implementation("androidx.activity:activity-ktx:1.8.0") // Para registerForActivityResult (use a versão mais recente)
    
}