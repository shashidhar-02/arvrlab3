using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using Unity.XR.CoreUtils;
using Assignment3;

namespace Assignment3.Editor
{
    /// <summary>
    /// Automated Scene Builder and Verifier for Assignment 3:
    /// Implementing Continuous VR Movement in Unity.
    /// </summary>
    [InitializeOnLoad]
    public static class Assignment3SceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/SampleScene.unity";
        private const string XrOriginPrefabPath = "Assets/Samples/XR Interaction Toolkit/3.6.1/Starter Assets/Prefabs/XR Origin (XR Rig).prefab";
        private const string SimulatorPrefabPath = "Assets/Samples/XR Interaction Toolkit/3.6.1/XR Interaction Simulator/XR Interaction Simulator.prefab";
        private const string InputActionsPath = "Assets/Samples/XR Interaction Toolkit/3.6.1/Starter Assets/XRI Default Input Actions.inputactions";

        static Assignment3SceneBuilder()
        {
            EditorApplication.delayCall += () =>
            {
                if (!SessionState.GetBool("Assignment3_Built", false))
                {
                    SessionState.SetBool("Assignment3_Built", true);
                    BuildAssignment3Scene();
                }
            };
        }

        [MenuItem("Assignment 3/Build and Configure Scene", priority = 1)]
        public static void BuildAssignment3Scene()
        {
            Debug.Log("<color=green><b>[Assignment 3] Starting automated scene construction...</b></color>");

            // Ensure the scene is open
            Scene scene = EditorSceneManager.GetActiveScene();
            if (scene.path != ScenePath && File.Exists(ScenePath))
            {
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }

            // 1. Create or verify XR Interaction Manager
            var interactionManager = UnityEngine.Object.FindAnyObjectByType<XRInteractionManager>();
            if (interactionManager == null)
            {
                var mgrObj = new GameObject("XR Interaction Manager");
                interactionManager = mgrObj.AddComponent<XRInteractionManager>();
                Undo.RegisterCreatedObjectUndo(mgrObj, "Create XR Interaction Manager");
                Debug.Log("[Assignment 3] Created XR Interaction Manager.");
            }

            // 2. Remove standalone Main Camera outside XR Origin
            var allCameras = UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsInactive.Include);
            foreach (var cam in allCameras)
            {
                if (cam.gameObject.name == "Main Camera" && cam.GetComponentInParent<XROrigin>() == null)
                {
                    Debug.Log("[Assignment 3] Removing standalone template Main Camera.");
                    UnityEngine.Object.DestroyImmediate(cam.gameObject);
                }
            }

            // 3. Create or verify XR Interaction Simulator (XR Device Simulator)
            var simulator = GameObject.Find("XR Interaction Simulator") ?? GameObject.Find("XR Device Simulator");
            if (simulator == null)
            {
                var simPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SimulatorPrefabPath);
                if (simPrefab != null)
                {
                    simulator = (GameObject)PrefabUtility.InstantiatePrefab(simPrefab);
                    simulator.name = "XR Interaction Simulator";
                    Undo.RegisterCreatedObjectUndo(simulator, "Instantiate XR Interaction Simulator");
                    Debug.Log("[Assignment 3] Instantiated XR Interaction Simulator.");
                }
                else
                {
                    Debug.LogWarning("[Assignment 3] Simulator prefab not found at: " + SimulatorPrefabPath);
                }
            }

            // 4. Create or verify XR Origin (XR Rig)
            var xrOrigin = UnityEngine.Object.FindAnyObjectByType<XROrigin>();
            GameObject xrOriginObj = null;
            if (xrOrigin == null)
            {
                var rigPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(XrOriginPrefabPath);
                if (rigPrefab != null)
                {
                    xrOriginObj = (GameObject)PrefabUtility.InstantiatePrefab(rigPrefab);
                    xrOriginObj.name = "XR Origin (XR Rig)";
                    Undo.RegisterCreatedObjectUndo(xrOriginObj, "Instantiate XR Origin (XR Rig)");
                    xrOrigin = xrOriginObj.GetComponent<XROrigin>();
                    Debug.Log("[Assignment 3] Instantiated XR Origin (XR Rig) from Starter Assets.");
                }
                else
                {
                    Debug.LogError("[Assignment 3] XR Origin prefab not found at: " + XrOriginPrefabPath);
                    return;
                }
            }
            else
            {
                xrOriginObj = xrOrigin.gameObject;
            }

            // Position player at origin spawn point
            xrOriginObj.transform.position = new Vector3(0, 0, 0);
            xrOriginObj.transform.rotation = Quaternion.identity;

            // 5. Ensure CharacterController on XR Origin (Solid collision body)
            var charController = xrOriginObj.GetComponent<CharacterController>();
            if (charController == null)
            {
                charController = xrOriginObj.AddComponent<CharacterController>();
                charController.radius = 0.35f;
                charController.height = 1.8f;
                charController.center = new Vector3(0, 0.9f, 0);
                charController.minMoveDistance = 0f;
                charController.stepOffset = 0.35f;
                charController.slopeLimit = 45f;
                charController.skinWidth = 0.08f;
                Debug.Log("[Assignment 3] Added CharacterController to XR Origin.");
            }

            // Find camera inside XR Origin
            var xrCamera = xrOrigin.Camera;
            if (xrCamera == null)
            {
                xrCamera = xrOriginObj.GetComponentInChildren<Camera>();
            }

            // 6. Configure Continuous Move Provider
            var moveProvider = xrOriginObj.GetComponentInChildren<ContinuousMoveProvider>();
            if (moveProvider == null)
            {
                moveProvider = xrOriginObj.AddComponent<ContinuousMoveProvider>();
                Debug.Log("[Assignment 3] Added ContinuousMoveProvider to XR Origin.");
            }
            moveProvider.moveSpeed = 3.0f; // Sensible continuous movement speed
            moveProvider.enableStrafe = true; // Forward, backward, left, right strafe
            if (xrCamera != null)
            {
                moveProvider.forwardSource = xrCamera.transform; // Headset relative movement direction
                Debug.Log("[Assignment 3] Assigned XR Camera forward source to ContinuousMoveProvider.");
            }

            // 7. Wire simulated controller thumbstick/joystick input
            var inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);
            if (inputActions != null)
            {
                var moveAction = inputActions.FindAction("XRI Left Locomotion/Move") ?? inputActions.FindAction("Move");
                if (moveAction != null)
                {
                    var actionRef = InputActionReference.Create(moveAction);
                    var reader = new XRInputValueReader<Vector2>();
                    reader.inputActionReference = actionRef;
                    moveProvider.leftHandMoveInput = reader;
                    Debug.Log("[Assignment 3] Successfully bound Left Hand Thumbstick/Locomotion Move Action.");
                }
                else
                {
                    Debug.LogWarning("[Assignment 3] Could not find 'XRI Left Locomotion/Move' action in InputActionAsset.");
                }
            }

            // 8. Build Walkable VR Environment (Floor, Walls, Platforms, Obstacles)
            BuildWalkableEnvironment();

            // 9. Build World-Space 3D Status Board
            Assignment3UIBoard.CreateWorldSpaceBoard(new Vector3(0, 1.8f, 2.8f), Quaternion.identity);

            // 10. Attach Assignment3RuntimeTest for automated Play Mode validation
            var testObj = GameObject.Find("Assignment3_Runtime_Tester");
            if (testObj == null)
            {
                testObj = new GameObject("Assignment3_Runtime_Tester");
            }
            var runtimeTest = testObj.GetComponent<Assignment3RuntimeTest>();
            if (runtimeTest == null)
            {
                runtimeTest = testObj.AddComponent<Assignment3RuntimeTest>();
            }
            runtimeTest.ValidateHierarchy();

            // Save the scene
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("<color=green><b>[Assignment 3] Scene successfully built, configured, and saved to: " + ScenePath + "</b></color>");
        }

        private static void BuildWalkableEnvironment()
        {
            var envRoot = GameObject.Find("Environment");
            if (envRoot != null)
            {
                UnityEngine.Object.DestroyImmediate(envRoot);
            }
            envRoot = new GameObject("Environment");

            // Materials
            var floorMat = CreateColorMaterial("Mat_Floor", new Color(0.22f, 0.25f, 0.28f));
            var wallMat = CreateColorMaterial("Mat_Walls", new Color(0.35f, 0.38f, 0.44f));
            var platMat = CreateColorMaterial("Mat_Platform", new Color(0.18f, 0.45f, 0.55f));
            var obsMat = CreateColorMaterial("Mat_Obstacle", new Color(0.75f, 0.35f, 0.20f));

            // 1. Large Walkable Floor (40x40)
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor_Large";
            floor.transform.SetParent(envRoot.transform);
            floor.transform.position = new Vector3(0, -0.25f, 0);
            floor.transform.localScale = new Vector3(40f, 0.5f, 40f);
            if (floorMat != null) floor.GetComponent<Renderer>().sharedMaterial = floorMat;
            var floorCol = floor.GetComponent<BoxCollider>();
            if (floorCol == null) floorCol = floor.AddComponent<BoxCollider>();
            floorCol.isTrigger = false;

            // 2. Perimeter Walls (Prevent escaping, block movement)
            CreateWall("Wall_North", new Vector3(0, 2.5f, 20f), new Vector3(40f, 5f, 1f), envRoot.transform, wallMat);
            CreateWall("Wall_South", new Vector3(0, 2.5f, -20f), new Vector3(40f, 5f, 1f), envRoot.transform, wallMat);
            CreateWall("Wall_East", new Vector3(20f, 2.5f, 0), new Vector3(1f, 5f, 40f), envRoot.transform, wallMat);
            CreateWall("Wall_West", new Vector3(-20f, 2.5f, 0), new Vector3(1f, 5f, 40f), envRoot.transform, wallMat);

            // 3. Elevated Platforms & Walkable Ramps
            // Platform 1 (Height 1.2m)
            var plat1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plat1.name = "Platform_Elevated_East";
            plat1.transform.SetParent(envRoot.transform);
            plat1.transform.position = new Vector3(8f, 0.6f, 8f);
            plat1.transform.localScale = new Vector3(8f, 1.2f, 8f);
            if (platMat != null) plat1.GetComponent<Renderer>().sharedMaterial = platMat;

            // Walkable Ramp connecting floor to Platform 1 (slope ~ 15 deg)
            var ramp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ramp.name = "Ramp_Walkable_East";
            ramp.transform.SetParent(envRoot.transform);
            ramp.transform.position = new Vector3(8f, 0.6f, 1.8f);
            ramp.transform.rotation = Quaternion.Euler(15f, 0f, 0f);
            ramp.transform.localScale = new Vector3(4f, 0.2f, 4.8f);
            if (platMat != null) ramp.GetComponent<Renderer>().sharedMaterial = platMat;

            // Platform 2 (Height 0.8m)
            var plat2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plat2.name = "Platform_Low_West";
            plat2.transform.SetParent(envRoot.transform);
            plat2.transform.position = new Vector3(-8f, 0.4f, 8f);
            plat2.transform.localScale = new Vector3(6f, 0.8f, 6f);
            if (platMat != null) plat2.GetComponent<Renderer>().sharedMaterial = platMat;

            // 4. Solid Obstacles (Pillars, Barricades)
            CreateObstacle("Pillar_NE", new Vector3(4f, 2f, 4f), new Vector3(1.5f, 4f, 1.5f), envRoot.transform, obsMat);
            CreateObstacle("Pillar_NW", new Vector3(-4f, 2f, 4f), new Vector3(1.5f, 4f, 1.5f), envRoot.transform, obsMat);
            CreateObstacle("Pillar_SE", new Vector3(4f, 2f, -4f), new Vector3(1.5f, 4f, 1.5f), envRoot.transform, obsMat);
            CreateObstacle("Pillar_SW", new Vector3(-4f, 2f, -4f), new Vector3(1.5f, 4f, 1.5f), envRoot.transform, obsMat);

            CreateObstacle("Barrier_Center_North", new Vector3(0, 1.5f, 8f), new Vector3(6f, 3f, 1f), envRoot.transform, obsMat);
            CreateObstacle("Barrier_Maze_Wall", new Vector3(-6f, 1.5f, -2f), new Vector3(1f, 3f, 8f), envRoot.transform, obsMat);

            Debug.Log("[Assignment 3] Walkable environment built with colliders: large floor, 4 walls, 2 platforms, 1 ramp, 6 obstacles.");
        }

        private static void CreateWall(string name, Vector3 pos, Vector3 scale, Transform parent, Material mat)
        {
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.SetParent(parent);
            wall.transform.position = pos;
            wall.transform.localScale = scale;
            if (mat != null) wall.GetComponent<Renderer>().sharedMaterial = mat;
            var col = wall.GetComponent<BoxCollider>();
            if (col == null) col = wall.AddComponent<BoxCollider>();
            col.isTrigger = false;
        }

        private static void CreateObstacle(string name, Vector3 pos, Vector3 scale, Transform parent, Material mat)
        {
            var obs = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obs.name = name;
            obs.transform.SetParent(parent);
            obs.transform.position = pos;
            obs.transform.localScale = scale;
            if (mat != null) obs.GetComponent<Renderer>().sharedMaterial = mat;
            var col = obs.GetComponent<BoxCollider>();
            if (col == null) col = obs.AddComponent<BoxCollider>();
            col.isTrigger = false;
        }

        private static Material CreateColorMaterial(string matName, Color color)
        {
            string matFolder = "Assets/Settings/Materials";
            if (!Directory.Exists(matFolder))
            {
                Directory.CreateDirectory(matFolder);
            }
            string path = $"{matFolder}/{matName}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                mat = new Material(shader);
                mat.color = color;
                AssetDatabase.CreateAsset(mat, path);
            }
            else
            {
                mat.color = color;
                EditorUtility.SetDirty(mat);
            }
            return mat;
        }
    }
}
