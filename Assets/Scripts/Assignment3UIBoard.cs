using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using Unity.XR.CoreUtils;

namespace Assignment3
{
    /// <summary>
    /// In-world 3D Diagnostic and Instructions Board for Assignment 3.
    /// Rendered in World Space in front of the player spawn point.
    /// </summary>
    public class Assignment3UIBoard : MonoBehaviour
    {
        private Text _statusText;
        private Assignment3RuntimeTest _testRunner;
        private ContinuousMoveProvider _moveProvider;
        private Camera _cam;

        private void Start()
        {
            _testRunner = FindAnyObjectByType<Assignment3RuntimeTest>();
            _moveProvider = FindAnyObjectByType<ContinuousMoveProvider>();
            var origin = FindAnyObjectByType<XROrigin>();
            if (origin != null) _cam = origin.Camera;

            _statusText = GetComponentInChildren<Text>();
        }

        private void Update()
        {
            if (_statusText == null) return;

            string speedStr = _moveProvider != null ? $"{_moveProvider.moveSpeed:F1} m/s" : "N/A";
            string headStr = _cam != null ? $"{_cam.transform.forward:F2}" : "N/A";
            string playerPos = _moveProvider != null ? $"{_moveProvider.transform.position:F2}" : "N/A";

            _statusText.text =
                "<b><size=26><color=#44EE88>ASSIGNMENT 3: CONTINUOUS VR MOVEMENT</color></size></b>\n" +
                "=====================================================\n" +
                "<b>System Architecture:</b> OpenXR + XR Interaction Toolkit 3.6.1\n" +
                $"<b>Continuous Move Provider:</b> ACTIVE (Speed: {speedStr})\n" +
                "<b>Input Mapping:</b> XRI Left Locomotion/Move (Simulated Thumbstick)\n" +
                "<b>Heading Direction:</b> Headset Relative (XR Camera Forward)\n" +
                "<b>Collision Engine:</b> CharacterController + Solid Box Colliders\n" +
                "<b>Simulation:</b> XR Device Simulator (Desktop Editor Play Mode)\n" +
                "-----------------------------------------------------\n" +
                $"<b>Player Position:</b> {playerPos}\n" +
                $"<b>Headset Forward Heading:</b> {headStr}\n" +
                "-----------------------------------------------------\n" +
                "<b>DESKTOP SIMULATOR CONTROLS:</b>\n" +
                "• <b>Right Mouse Click + WASD:</b> Look & Move Simulated Headset\n" +
                "• <b>Left Controller Selection:</b> Left Thumbstick moves player\n" +
                "• <b>Smooth Locomotion:</b> Move Forward/Back/Left/Right\n" +
                "• <b>Obstacle Block:</b> Walls and pillars block movement physically\n" +
                "=====================================================\n" +
                "<b>STATUS:</b> <color=#44EE88><b>READY FOR PLAY MODE EVALUATION</b></color>";
        }

        public static GameObject CreateWorldSpaceBoard(Vector3 position, Quaternion rotation)
        {
            var existing = GameObject.Find("Assignment3_UI_Board");
            if (existing != null) Object.DestroyImmediate(existing);

            var boardObj = new GameObject("Assignment3_UI_Board");
            boardObj.transform.position = position;
            boardObj.transform.rotation = rotation;

            var canvas = boardObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            boardObj.AddComponent<CanvasScaler>();
            boardObj.AddComponent<GraphicRaycaster>();

            var rt = boardObj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(900, 560);
            rt.localScale = new Vector3(0.0035f, 0.0035f, 0.0035f);

            // Background panel
            var panelObj = new GameObject("Panel");
            panelObj.transform.SetParent(boardObj.transform, false);
            var panelImg = panelObj.AddComponent<Image>();
            panelImg.color = new Color(0.08f, 0.10f, 0.14f, 0.92f);
            var panelRt = panelObj.GetComponent<RectTransform>();
            panelRt.anchorMin = Vector2.zero;
            panelRt.anchorMax = Vector2.one;
            panelRt.sizeDelta = Vector2.zero;

            // Border
            var borderObj = new GameObject("Border");
            borderObj.transform.SetParent(boardObj.transform, false);
            var borderImg = borderObj.AddComponent<Image>();
            borderImg.color = new Color(0.2f, 0.6f, 0.9f, 0.4f);
            var borderRt = borderObj.GetComponent<RectTransform>();
            borderRt.anchorMin = Vector2.zero;
            borderRt.anchorMax = Vector2.one;
            borderRt.sizeDelta = new Vector2(4, 4);
            borderRt.SetAsFirstSibling();

            // Text
            var textObj = new GameObject("StatusText");
            textObj.transform.SetParent(boardObj.transform, false);
            var text = textObj.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = 20;
            text.color = Color.white;
            text.alignment = TextAnchor.UpperLeft;
            text.lineSpacing = 1.15f;
            var textRt = textObj.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(30, 20);
            textRt.offsetMax = new Vector2(-30, -20);

            boardObj.AddComponent<Assignment3UIBoard>();
            return boardObj;
        }
    }
}
