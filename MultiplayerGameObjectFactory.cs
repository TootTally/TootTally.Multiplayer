using TMPro;
using TootTally.Graphics;
using UnityEngine;
using UnityEngine.UI;

namespace TootTally.Multiplayer
{
    public static class MultiplayerGameObjectFactory
    {
        private static TMP_InputField _inputFieldPrefab;

        private static bool _isInitialized;

        public static void Initialize()
        {
            if (_isInitialized) return;

            SetInputFieldPrefab();
            _isInitialized = true;
        }

        private static void SetInputFieldPrefab()
        {
            var inputHolder = new GameObject("InputFieldHolder");
            var rectHolder = inputHolder.AddComponent<RectTransform>();
            rectHolder.anchoredPosition = Vector2.zero;
            rectHolder.sizeDelta = new Vector2(350, 50);
            var inputImageHolder = GameObject.Instantiate(inputHolder, inputHolder.transform);
            var inputTextHolder = GameObject.Instantiate(inputImageHolder, inputHolder.transform);
            inputImageHolder.name = "Image";
            inputTextHolder.name = "Text";

            _inputFieldPrefab = inputHolder.AddComponent<TMP_InputField>();

            rectHolder.anchorMax = rectHolder.anchorMin = Vector2.zero;

            _inputFieldPrefab.image = inputImageHolder.AddComponent<Image>();
            RectTransform rectImage = inputImageHolder.GetComponent<RectTransform>();

            rectImage.anchorMin = rectImage.anchorMax = rectImage.pivot = Vector2.zero;
            rectImage.anchoredPosition = new Vector2(0, 4);
            rectImage.sizeDelta = new Vector2(350, 2);

            RectTransform rectText = inputTextHolder.GetComponent<RectTransform>();
            rectText.anchoredPosition = rectText.anchorMin = rectText.anchorMax = rectText.pivot = Vector2.zero;
            rectText.sizeDelta = new Vector2(350, 50);

            _inputFieldPrefab.textComponent = GameObjectFactory.CreateSingleText(inputTextHolder.transform, $"TextLabel", "", GameTheme.themeColors.leaderboard.text);
            _inputFieldPrefab.textComponent.rectTransform.pivot = new Vector2(0, 0.5f);
            _inputFieldPrefab.textComponent.alignment = TextAlignmentOptions.Left;
            _inputFieldPrefab.textComponent.margin = new Vector4(5, 0, 0, 0);
            _inputFieldPrefab.textComponent.enableWordWrapping = true;
            _inputFieldPrefab.textViewport = _inputFieldPrefab.textComponent.rectTransform;

            GameObject.DontDestroyOnLoad(_inputFieldPrefab);
        }

        public static TMP_InputField CreateInputField(Transform canvasTransform, string name, Vector2 size, float fontSize, string text, bool isPassword)
        {
            var inputField = GameObject.Instantiate(_inputFieldPrefab, canvasTransform);
            inputField.name = name;
            inputField.GetComponent<RectTransform>().sizeDelta = size;
            inputField.transform.Find("Image").GetComponent<RectTransform>().sizeDelta = new Vector2(size.x, 2);
            inputField.transform.Find("Text").GetComponent<RectTransform>().sizeDelta = size;
            inputField.textComponent.GetComponent<RectTransform>().sizeDelta = size;
            inputField.textComponent.fontSize = fontSize;
            inputField.text = text;
            inputField.inputType = isPassword ? TMP_InputField.InputType.Password : TMP_InputField.InputType.Standard;

            return inputField;
        }
    }
}
