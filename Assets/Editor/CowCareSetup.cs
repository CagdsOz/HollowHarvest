using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace HollowHarvest.Editor
{
    public static class CowCareSetup
    {
        [MenuItem("HollowHarvest/Upgrade Cow Care (Sprint 3)")]
        public static void Setup()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.path != "Assets/Scenes/FarmPrototype.unity")
                scene = EditorSceneManager.OpenScene("Assets/Scenes/FarmPrototype.unity");
            var objects = scene.GetRootGameObjects();
            var panel = objects.SelectMany(o => o.GetComponentsInChildren<CowStatusPanel>(true)).SingleOrDefault();
            var clock = objects.SelectMany(o => o.GetComponentsInChildren<GameClock>(true)).SingleOrDefault();
            var cows = objects.SelectMany(o => o.GetComponentsInChildren<CowInteraction>(true)).ToArray();
            if (panel == null || clock == null || cows.Length == 0)
                throw new InvalidOperationException("Run Add Cow and Building Interactions first. One clock and cow panel are required.");
            var panelData = new SerializedObject(panel);
            var details = panelData.FindProperty("details").objectReferenceValue as Text;
            var close = panelData.FindProperty("closeButton").objectReferenceValue as Button;
            if (details == null || close == null)
                throw new InvalidOperationException("The existing cow panel has missing Details or Close Button references.");
            var background = details.transform.parent as RectTransform;
            var counter = objects.SelectMany(o => o.GetComponentsInChildren<MilkCounter>(true)).SingleOrDefault();

            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Upgrade cow care");
            if (counter == null) counter = Undo.AddComponent<MilkCounter>(clock.gameObject);
            foreach (var cow in cows)
            {
                var care = cow.GetComponent<CowCare>();
                if (care == null) care = Undo.AddComponent<CowCare>(cow.gameObject);
                Reference(care, "milkCounter", counter);
            }

            Undo.RecordObject(background, "Expand cow panel");
            background.sizeDelta = new Vector2(680, 560);
            Undo.RecordObject(details.rectTransform, "Layout cow stats");
            details.rectTransform.offsetMin = new Vector2(24, 235);
            details.rectTransform.offsetMax = new Vector2(-24, -20);
            Undo.RecordObject(details, "Size cow stats text");
            details.fontSize = 22;
            Reference(panel, "feedButton", CareButton("Feed", -210));
            Reference(panel, "petButton", CareButton("Pet", 0));
            Reference(panel, "milkButton", CareButton("Milk", 210));
            var feedback = FindOrCreateText("Care Feedback", background);
            Layout(feedback.rectTransform, new Vector2(0.5f, 0), new Vector2(0, 115), new Vector2(620, 80));
            Undo.RecordObject(feedback, "Set feedback alignment");
            feedback.alignment = TextAnchor.MiddleCenter;
            feedback.text = "";
            Reference(panel, "feedback", feedback);

            var milkLabel = FindOrCreateText("Milk Counter", panel.transform);
            Layout(milkLabel.rectTransform, Vector2.one, new Vector2(-160, -42), new Vector2(280, 48));
            Undo.RecordObject(milkLabel, "Set milk text");
            milkLabel.alignment = TextAnchor.MiddleRight;
            milkLabel.text = $"MILK: {counter.TotalMilk}";
            if (milkLabel.GetComponent<Shadow>() == null) Undo.AddComponent<Shadow>(milkLabel.gameObject);
            var hud = milkLabel.GetComponent<MilkCounterHud>();
            if (hud == null) hud = Undo.AddComponent<MilkCounterHud>(milkLabel.gameObject);
            Reference(hud, "counter", counter);
            Reference(hud, "label", milkLabel);

            Undo.CollapseUndoOperations(group);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Selection.activeGameObject = cows[0].gameObject;
            Debug.Log("Sprint 3 cow care connected. Press Play, approach a cow, and press E. Balance values are on Animal Stats and Cow Care.");

            Button CareButton(string title, float x)
            {
                var existing = background.Find(title + " Button");
                Button button;
                if (existing != null) button = existing.GetComponent<Button>();
                else
                {
                    var go = new GameObject(title + " Button", typeof(RectTransform), typeof(Image), typeof(Button));
                    go.transform.SetParent(background, false);
                    var image = go.GetComponent<Image>();
                    image.color = new Color(0.3f, 0.4f, 0.3f);
                    button = go.GetComponent<Button>();
                    button.targetGraphic = image;
                    Undo.RegisterCreatedObjectUndo(go, "Create care button");
                }
                Layout((RectTransform)button.transform, new Vector2(0.5f, 0), new Vector2(x, 195), new Vector2(190, 48));
                var label = FindOrCreateText("Label", button.transform);
                Undo.RecordObject(label.rectTransform, "Layout care label");
                label.rectTransform.anchorMin = Vector2.zero;
                label.rectTransform.anchorMax = Vector2.one;
                label.rectTransform.offsetMin = label.rectTransform.offsetMax = Vector2.zero;
                Undo.RecordObject(label, "Name care button");
                label.text = title;
                label.alignment = TextAnchor.MiddleCenter;
                return button;
            }
        }

        private static Text FindOrCreateText(string name, Transform parent)
        {
            var existing = parent.Find(name);
            if (existing != null) return existing.GetComponent<Text>();
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var label = go.GetComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = 22;
            label.color = new Color(0.94f, 0.9f, 0.74f);
            label.raycastTarget = false;
            Undo.RegisterCreatedObjectUndo(go, "Create care UI text");
            return label;
        }
        private static void Layout(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size)
        {
            Undo.RecordObject(rect, "Layout care UI");
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }
        private static void Reference(Object component, string field, Object value)
        {
            Undo.RecordObject(component, "Connect cow care");
            var serialized = new SerializedObject(component);
            serialized.FindProperty(field).objectReferenceValue = value;
            serialized.ApplyModifiedProperties();
        }
    }
}
