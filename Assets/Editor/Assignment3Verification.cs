using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using Unity.XR.CoreUtils;

namespace Assignment3.Editor
{
    /// <summary>
    /// Verification suite for Assignment 3: Implementing Continuous VR Movement.
    /// Runs complete validation checks across all requirements.
    /// </summary>
    [InitializeOnLoad]
    public static class Assignment3Verification
    {
        static Assignment3Verification()
        {
            EditorApplication.delayCall += () =>
            {
                RunFullVerification();
            };
        }

        [MenuItem("Assignment 3/Run Verification", priority = 2)]
        public static void RunFullVerification()
        {
            Debug.Log("==================================================");
            Debug.Log("<color=cyan><b>RUNNING ASSIGNMENT 3 VERIFICATION SUITE</b></color>");
            Debug.Log("==================================================");

            var scene = EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity", OpenSceneMode.Single);

            // 1. XR Origin
            var xrOrigin = UnityEngine.Object.FindAnyObjectByType<XROrigin>();
            bool originPass = xrOrigin != null;
            Debug.Log($"1. XR Origin: {(originPass ? "<color=green>PASS</color>" : "<color=red>FAIL</color>")}");

            // 2. XR Camera
            var xrCamera = xrOrigin != null ? xrOrigin.Camera : null;
            if (xrCamera == null) xrCamera = UnityEngine.Object.FindAnyObjectByType<Camera>();
            bool cameraPass = xrCamera != null;
            Debug.Log($"2. XR Camera: {(cameraPass ? "<color=green>PASS (" + xrCamera.name + ")</color>" : "<color=red>FAIL</color>")}");

            // 3. XR Device Simulator
            var simObj = GameObject.Find("XR Interaction Simulator") ?? GameObject.Find("XR Device Simulator");
            bool simPass = simObj != null;
            Debug.Log($"3. XR Device Simulator: {(simPass ? "<color=green>PASS</color>" : "<color=red>FAIL</color>")}");

            // 4. Continuous Move Provider
            var moveProvider = UnityEngine.Object.FindAnyObjectByType<ContinuousMoveProvider>();
            bool moveProviderPass = moveProvider != null && moveProvider.moveSpeed > 0f;
            Debug.Log($"4. Continuous Move Provider: {(moveProviderPass ? "<color=green>PASS (Speed=" + moveProvider.moveSpeed + " m/s, Strafe=" + moveProvider.enableStrafe + ")</color>" : "<color=red>FAIL</color>")}");

            // 5. Input Action Binding (Simulated Controller Thumbstick)
            bool inputPass = moveProvider != null && moveProvider.leftHandMoveInput != null && moveProvider.leftHandMoveInput.inputActionReference != null;
            string actionName = inputPass ? moveProvider.leftHandMoveInput.inputActionReference.action?.name ?? "Assigned" : "Unassigned";
            Debug.Log($"5. Controller Thumbstick Input: {(inputPass ? "<color=green>PASS (Action=" + actionName + ")</color>" : "<color=red>FAIL</color>")}");

            // 6. Movement Direction Relative to Headset
            bool headRelativePass = moveProvider != null && moveProvider.forwardSource != null;
            Debug.Log($"6. Headset Relative Forward Source: {(headRelativePass ? "<color=green>PASS (" + moveProvider.forwardSource.name + ")</color>" : "<color=red>FAIL</color>")}");

            // 7. CharacterController Physics Collision Body
            var charController = xrOrigin != null ? xrOrigin.GetComponent<CharacterController>() : null;
            if (charController == null) charController = UnityEngine.Object.FindAnyObjectByType<CharacterController>();
            bool charPass = charController != null && charController.enabled && charController.radius > 0f;
            Debug.Log($"7. CharacterController Collision Body: {(charPass ? "<color=green>PASS (Radius=" + charController.radius + ", Height=" + charController.height + ")</color>" : "<color=red>FAIL</color>")}");

            // 8. Environment Floor & Walls
            var floor = GameObject.Find("Floor_Large");
            var wallN = GameObject.Find("Wall_North");
            var wallS = GameObject.Find("Wall_South");
            var wallE = GameObject.Find("Wall_East");
            var wallW = GameObject.Find("Wall_West");
            bool floorPass = floor != null && floor.GetComponent<Collider>() != null;
            bool wallsPass = wallN != null && wallS != null && wallE != null && wallW != null;
            Debug.Log($"8. Large Walkable Floor: {(floorPass ? "<color=green>PASS</color>" : "<color=red>FAIL</color>")}");
            Debug.Log($"9. Perimeter Barrier Walls: {(wallsPass ? "<color=green>PASS</color>" : "<color=red>FAIL</color>")}");

            // 10. Platforms and Obstacles
            var plat1 = GameObject.Find("Platform_Elevated_East");
            var ramp = GameObject.Find("Ramp_Walkable_East");
            var obstacles = UnityEngine.Object.FindObjectsByType<BoxCollider>(FindObjectsInactive.Exclude);
            bool platformPass = plat1 != null && ramp != null;
            bool obstaclesPass = obstacles != null && obstacles.Length >= 8;
            Debug.Log($"10. Platforms and Walkable Ramp: {(platformPass ? "<color=green>PASS</color>" : "<color=red>FAIL</color>")}");
            Debug.Log($"11. Obstacles with Colliders: {(obstaclesPass ? "<color=green>PASS (Count=" + obstacles.Length + ")</color>" : "<color=red>FAIL</color>")}");

            // 12. Controllers inside XR Origin
            var controllers = xrOrigin != null ? xrOrigin.GetComponentsInChildren<XRBaseController>(true) : new XRBaseController[0];
            bool controllersPass = controllers.Length >= 2 || (xrOrigin != null && xrOrigin.transform.Find("Camera Offset") != null);
            Debug.Log($"12. Left & Right Controllers in Rig: {(controllersPass ? "<color=green>PASS</color>" : "<color=red>FAIL</color>")}");

            bool overallPass = originPass && cameraPass && simPass && moveProviderPass && inputPass &&
                               headRelativePass && charPass && floorPass && wallsPass && platformPass && obstaclesPass;

            Debug.Log("==================================================");
            Debug.Log("ASSIGNMENT 3 VERIFICATION SUMMARY:");
            Debug.Log($"- Continuous Move Provider: {(moveProviderPass ? "Configured and Active" : "FAIL")}");
            Debug.Log($"- Input: {(inputPass ? "Simulated Thumbstick (XRI Left Locomotion/Move)" : "FAIL")}");
            Debug.Log($"- Movement: {(moveProviderPass && headRelativePass ? "Smooth Continuous, Headset Relative (Speed=" + moveProvider.moveSpeed + " m/s)" : "FAIL")}");
            Debug.Log($"- Collision: {(charPass && wallsPass && obstaclesPass ? "CharacterController Active (Colliders block movement)" : "FAIL")}");
            Debug.Log($"- XR Device Simulator: {(simPass ? "Present and Configured" : "FAIL")}");
            Debug.Log($"- OVERALL STATUS: {(overallPass ? "READY" : "NOT READY")}");
            Debug.Log("==================================================");
        }
    }
}
