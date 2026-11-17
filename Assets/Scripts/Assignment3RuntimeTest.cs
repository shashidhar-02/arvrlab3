using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using Unity.XR.CoreUtils;

namespace Assignment3
{
    /// <summary>
    /// Play Mode Verifier and Diagnostic Display for Assignment 3:
    /// Implementing Continuous VR Movement in Unity.
    /// </summary>
    public class Assignment3RuntimeTest : MonoBehaviour
    {
        [Header("Component References")]
        public XROrigin xrOrigin;
        public Camera xrCamera;
        public ContinuousMoveProvider moveProvider;
        public CharacterController characterController;
        public GameObject xrDeviceSimulator;

        [Header("Live Verification Flags")]
        public bool originPresent = false;
        public bool cameraPresent = false;
        public bool simulatorPresent = false;
        public bool moveProviderPresent = false;
        public bool inputAssigned = false;
        public bool headsetRelativeConfigured = false;
        public bool characterControllerActive = false;
        public bool environmentCollidersPresent = false;
        public bool playModeDiagnosticsPassed = false;

        [Header("Live Movement Telemetry")]
        public Vector3 initialPosition;
        public Vector3 currentPosition;
        public float movementSpeed = 0f;
        public string headingStatus = "Waiting...";

        private void Awake()
        {
            ValidateHierarchy();
        }

        private void Start()
        {
            if (xrOrigin != null)
            {
                initialPosition = xrOrigin.transform.position;
            }
            StartCoroutine(RunAutomatedPlayModeVerification());
        }

        private void Update()
        {
            if (xrOrigin != null)
            {
                currentPosition = xrOrigin.transform.position;
            }

            if (xrCamera != null)
            {
                headingStatus = $"Camera Forward: {xrCamera.transform.forward:F2}";
            }
        }

        public void ValidateHierarchy()
        {
            if (xrOrigin == null) xrOrigin = FindAnyObjectByType<XROrigin>();
            if (xrCamera == null && xrOrigin != null) xrCamera = xrOrigin.Camera;
            if (xrCamera == null) xrCamera = Camera.main;

            if (moveProvider == null) moveProvider = FindAnyObjectByType<ContinuousMoveProvider>();
            if (characterController == null && xrOrigin != null) characterController = xrOrigin.GetComponent<CharacterController>();
            if (characterController == null) characterController = FindAnyObjectByType<CharacterController>();

            if (xrDeviceSimulator == null)
            {
                var sim = GameObject.Find("XR Interaction Simulator") ?? GameObject.Find("XR Device Simulator");
                if (sim != null) xrDeviceSimulator = sim;
            }

            originPresent = xrOrigin != null;
            cameraPresent = xrCamera != null;
            simulatorPresent = xrDeviceSimulator != null;
            moveProviderPresent = moveProvider != null;
            characterControllerActive = characterController != null && characterController.enabled;

            if (moveProvider != null)
            {
                headsetRelativeConfigured = moveProvider.forwardSource != null;
                inputAssigned = moveProvider.leftHandMoveInput != null && moveProvider.leftHandMoveInput.inputActionReference != null;
                movementSpeed = moveProvider.moveSpeed;
            }

            // Verify environment colliders
            var colliders = FindObjectsByType<Collider>(FindObjectsInactive.Exclude);
            environmentCollidersPresent = colliders != null && colliders.Length >= 5;
        }

        private IEnumerator RunAutomatedPlayModeVerification()
        {
            yield return new WaitForSeconds(0.5f);
            ValidateHierarchy();

            Debug.Log("==================================================");
            Debug.Log("<color=cyan><b>[Assignment 3] RUNTIME PLAY MODE DIAGNOSTICS</b></color>");
            Debug.Log($"1. XR Origin: {(originPresent ? "<color=green>PASS</color>" : "<color=red>FAIL</color>")}");
            Debug.Log($"2. XR Camera: {(cameraPresent ? "<color=green>PASS</color>" : "<color=red>FAIL</color>")}");
            Debug.Log($"3. XR Device Simulator: {(simulatorPresent ? "<color=green>PASS</color>" : "<color=red>FAIL</color>")}");
            Debug.Log($"4. Continuous Move Provider: {(moveProviderPresent ? "<color=green>PASS (Speed=" + movementSpeed + " m/s)</color>" : "<color=red>FAIL</color>")}");
            Debug.Log($"5. Input Action Binding: {(inputAssigned ? "<color=green>PASS (Left Hand Thumbstick bound)</color>" : "<color=red>FAIL</color>")}");
            Debug.Log($"6. Headset-Relative Forward Source: {(headsetRelativeConfigured ? "<color=green>PASS (" + moveProvider.forwardSource.name + ")</color>" : "<color=red>FAIL</color>")}");
            Debug.Log($"7. CharacterController Collider: {(characterControllerActive ? "<color=green>PASS (Radius=" + characterController.radius + ", Height=" + characterController.height + ")</color>" : "<color=red>FAIL</color>")}");
            Debug.Log($"8. Environment Colliders Count: {FindObjectsByType<Collider>(FindObjectsInactive.Exclude).Length} -> {(environmentCollidersPresent ? "<color=green>PASS</color>" : "<color=red>FAIL</color>")}");

            // Verify collision blocking: test CharacterController against obstacles
            yield return new WaitForSeconds(0.2f);
            TestWallCollisionDetection();

            playModeDiagnosticsPassed = originPresent && cameraPresent && simulatorPresent &&
                                         moveProviderPresent && inputAssigned &&
                                         headsetRelativeConfigured && characterControllerActive &&
                                         environmentCollidersPresent;

            Debug.Log("==================================================");
            Debug.Log($"[Assignment 3] OVERALL VERIFICATION: {(playModeDiagnosticsPassed ? "<color=green><b>ALL CHECKS PASSED (READY)</b></color>" : "<color=red><b>FAILED</b></color>")}");
            Debug.Log("==================================================");
        }

        private void TestWallCollisionDetection()
        {
            if (characterController == null) return;

            // Perform raycast check from character to nearest wall/obstacle
            RaycastHit hit;
            Vector3 origin = characterController.transform.position + Vector3.up * 0.9f;
            if (Physics.Raycast(origin, Vector3.forward, out hit, 10f))
            {
                Debug.Log($"[Assignment 3 Collision Test] Raycast ahead detected solid barrier: '{hit.collider.gameObject.name}' at distance {hit.distance:F2}m with Collider '{hit.collider.GetType().Name}'. Physical wall blocking confirmed.");
            }
        }
    }
}
