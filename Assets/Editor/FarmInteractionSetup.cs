using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace HollowHarvest.Editor
{
    public static class FarmInteractionSetup
    {
        private const string ScenePath = "Assets/Scenes/FarmPrototype.unity";
        private const string RootName = "Cow and Building Systems";

        [MenuItem("HollowHarvest/Add Cow and Building Interactions")]
        public static void Setup()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!File.Exists(ScenePath))
                throw new System.InvalidOperationException("Create the Sprint 1 Farm Scene first.");
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.path != ScenePath) scene = EditorSceneManager.OpenScene(ScenePath);
            if (GameObject.Find(RootName) != null)
            {
                Debug.Log("Cow/building setup already exists. Existing Inspector settings were preserved.");
                return;
            }

            var player = Object.FindAnyObjectByType<PlayerMovement>();
            var camera = Object.FindAnyObjectByType<CameraFollow>();
            var clock = Object.FindAnyObjectByType<GameClock>();
            var house = GameObject.Find("Farmhouse");
            var barn = GameObject.Find("Barn");
            var square = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Prototype/Square.png");
            var material = AssetDatabase.LoadAssetAtPath<Material>(
                "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Lit-Default.mat");
            var actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/Settings/InputSystem_Actions.inputactions");
            if (player == null || camera == null || clock == null || house == null || barn == null
                || square == null || material == null || actions == null
                || house.GetComponent<BoxCollider2D>() == null || barn.GetComponent<BoxCollider2D>() == null
                || actions.FindAction("FarmInteraction/Inspect") == null)
                throw new System.InvalidOperationException("Expected Sprint 1 objects/assets are missing. Restore their names and refresh the Input Actions asset.");

            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Add cow and building interactions");
            var root = new GameObject(RootName);
            var ui = CreateCanvas("Interaction UI", root.transform, 50);
            var fadeCanvas = CreateCanvas("Transition Overlay", root.transform, 1000);
            var fade = ImageObject("Black Fade", fadeCanvas.transform, Color.black);
            Stretch(fade.rectTransform);
            var fadeGroup = fade.gameObject.AddComponent<CanvasGroup>();
            fadeGroup.alpha = 0;
            fadeGroup.blocksRaycasts = false;
            var transition = root.AddComponent<BuildingTransition>();
            Reference(transition, "player", player);
            Reference(transition, "cameraFollow", camera);
            Reference(transition, "fadeOverlay", fadeGroup);

            var prompt = Label("Interaction Prompt", ui.transform, "", 24);
            prompt.alignment = TextAnchor.MiddleCenter;
            var rect = prompt.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0);
            rect.pivot = new Vector2(0.5f, 0);
            rect.anchoredPosition = new Vector2(0, 32);
            rect.sizeDelta = new Vector2(600, 50);
            var shadow = prompt.gameObject.AddComponent<Shadow>();
            shadow.effectDistance = new Vector2(2, -2);

            var modal = ImageObject("Cow Status Modal", ui.transform, new Color(0, 0, 0, 0.55f));
            Stretch(modal.rectTransform);
            var panel = ImageObject("Status Panel", modal.transform, new Color(0.08f, 0.11f, 0.09f));
            panel.rectTransform.sizeDelta = new Vector2(600, 360);
            var details = Label("Animal Details", panel.transform, "Cow status", 23);
            Stretch(details.rectTransform);
            details.rectTransform.offsetMin = new Vector2(24, 80);
            details.rectTransform.offsetMax = new Vector2(-24, -24);
            var buttonImage = ImageObject("Close Button", panel.transform, new Color(0.3f, 0.4f, 0.3f));
            rect = buttonImage.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0);
            rect.anchoredPosition = new Vector2(0, 40);
            rect.sizeDelta = new Vector2(230, 48);
            var button = buttonImage.gameObject.AddComponent<Button>();
            button.targetGraphic = buttonImage;
            var buttonText = Label("Close Label", button.transform, "Close (Escape)", 22);
            Stretch(buttonText.rectTransform);
            buttonText.alignment = TextAnchor.MiddleCenter;
            var cowPanel = ui.gameObject.AddComponent<CowStatusPanel>();
            Reference(cowPanel, "player", player);
            Reference(cowPanel, "panelRoot", modal.gameObject);
            Reference(cowPanel, "details", details);
            Reference(cowPanel, "closeButton", button);
            modal.gameObject.SetActive(false);

            if (Object.FindAnyObjectByType<EventSystem>() == null)
            {
                var events = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                events.transform.SetParent(root.transform);
                var module = events.GetComponent<InputSystemUIInputModule>();
                module.actionsAsset = actions;
                module.point = UIAction("Point");
                module.leftClick = UIAction("Click");
                module.move = UIAction("Navigate");
                module.submit = UIAction("Submit");
                module.cancel = UIAction("Cancel");
                module.scrollWheel = UIAction("ScrollWheel");
            }

            var interaction = player.GetComponent<PlayerInteraction>();
            if (interaction == null) interaction = Undo.AddComponent<PlayerInteraction>(player.gameObject);
            else Undo.RecordObject(interaction, "Connect interaction references");
            Reference(interaction, "inputActions", actions);
            Reference(interaction, "cowPanel", cowPanel);
            Reference(interaction, "transitions", transition);
            Reference(interaction, "prompt", prompt);

            var pasture = Marker("Grazing Center", root.transform, new Vector2(10, -7));
            Block("Pasture", root.transform, pasture.position, new Vector2(10, 7), new Color(0.31f, 0.43f, 0.25f), -19);
            var cow = new GameObject("Cow - Clover");
            cow.transform.SetParent(root.transform);
            cow.transform.position = new Vector2(10, -7);
            var sr = cow.AddComponent<SpriteRenderer>();
            sr.sprite = CreateCowSprite();
            sr.sharedMaterial = material;
            sr.sortingOrder = 9;
            var body = cow.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            // Trigger avoids the cow pushing the player or being pushed beyond its pasture.
            var cowCollider = cow.AddComponent<BoxCollider2D>();
            cowCollider.size = new Vector2(1.8f, 1.2f);
            cowCollider.isTrigger = true;
            Reference(cow.AddComponent<AnimalStats>(), "clock", clock);
            Reference(cow.AddComponent<CowWander>(), "grazingCenter", pasture);
            cow.AddComponent<CowInteraction>();

            AddBuilding(house, new Vector2(100, 0), new Color(0.42f, 0.33f, 0.24f));
            AddBuilding(barn, new Vector2(200, 0), new Color(0.38f, 0.32f, 0.2f));
            Undo.RegisterCreatedObjectUndo(root, "Create cow and indoor areas");
            Undo.CollapseUndoOperations(group);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Selection.activeGameObject = cow;
            Debug.Log("Cow and building interactions added. Cow: southeast pasture. Doors: farmhouse and barn south faces.");

            InputActionReference UIAction(string actionName)
            {
                const string folder = "Assets/Settings/FarmInteractionUI";
                Directory.CreateDirectory(folder);
                AssetDatabase.Refresh();
                string path = folder + "/" + actionName + ".asset";
                var reference = AssetDatabase.LoadAssetAtPath<InputActionReference>(path);
                if (reference != null) return reference;
                reference = InputActionReference.Create(actions.FindAction("UI/" + actionName, true));
                AssetDatabase.CreateAsset(reference, path);
                return reference;
            }

            void AddBuilding(GameObject building, Vector2 origin, Color floorColor)
            {
                var room = new GameObject(building.name + " Interior");
                room.transform.SetParent(root.transform);
                room.transform.position = origin;
                Block("Floor", room.transform, origin, new Vector2(26, 22), floorColor, -20);
                Color wallColor = new Color(0.25f, 0.23f, 0.2f);
                Block("North Wall", room.transform, origin + new Vector2(0, 10), new Vector2(25, 0.7f), wallColor, 1, true);
                Block("South Wall", room.transform, origin + new Vector2(0, -10), new Vector2(25, 0.7f), wallColor, 1, true);
                Block("West Wall", room.transform, origin + new Vector2(-12, 0), new Vector2(0.7f, 20), wallColor, 1, true);
                Block("East Wall", room.transform, origin + new Vector2(12, 0), new Vector2(0.7f, 20), wallColor, 1, true);
                Block("Exit Door Visual", room.transform, origin + new Vector2(0, -9.5f), new Vector2(2, 0.7f), new Color(0.7f, 0.5f, 0.25f), 2);
                Block("Placeholder Table", room.transform, origin + new Vector2(4, 2), new Vector2(3, 2), new Color(0.52f, 0.35f, 0.2f), 1, true);
                var indoorSpawn = Marker("Indoor Entrance", room.transform, origin + new Vector2(0, -7));
                var exitZone = Marker("Exit Interaction Zone", room.transform, origin + new Vector2(0, -8));
                var exit = exitZone.gameObject.AddComponent<DoorInteraction>();
                SetBool(exit, "isExit", true);
                SetVector(exit, "zoneSize", new Vector2(3, 3));

                var collider = building.GetComponent<BoxCollider2D>();
                Vector3 center = building.transform.TransformPoint(collider.offset);
                float bottom = center.y - collider.size.y * Mathf.Abs(building.transform.lossyScale.y) * 0.5f;
                var outdoorSpawn = Marker(building.name + " Outdoor Entrance", root.transform, new Vector2(center.x, bottom - 1));
                var doorZone = Marker(building.name + " Door Zone", root.transform, outdoorSpawn.position);
                var entrance = doorZone.gameObject.AddComponent<DoorInteraction>();
                Reference(entrance, "destination", indoorSpawn);
                Reference(entrance, "destinationArea", room);
                Reference(exit, "destination", outdoorSpawn);
                Reference(exit, "departingArea", room);
                room.SetActive(false);
            }

            GameObject Block(string name, Transform parent, Vector2 position, Vector2 size, Color color, int order, bool solid = false)
            {
                var go = new GameObject(name);
                go.transform.SetParent(parent);
                go.transform.position = position;
                go.transform.localScale = new Vector3(size.x, size.y, 1);
                var renderer = go.AddComponent<SpriteRenderer>();
                renderer.sprite = square;
                renderer.sharedMaterial = material;
                renderer.color = color;
                renderer.sortingOrder = order;
                if (solid) go.AddComponent<BoxCollider2D>();
                return go;
            }
        }

        private static Transform Marker(string name, Transform parent, Vector2 position)
        {
            var marker = new GameObject(name).transform;
            marker.SetParent(parent);
            marker.position = position;
            return marker;
        }
        private static Canvas CreateCanvas(string name, Transform parent, int order)
        {
            var go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.transform.SetParent(parent, false);
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = order;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }
        private static Image ImageObject(string name, Transform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.color = color;
            return image;
        }
        private static Text Label(string name, Transform parent, string content, int size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var text = go.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.color = new Color(0.94f, 0.9f, 0.74f);
            text.raycastTarget = false;
            text.text = content;
            return text;
        }
        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
        private static void Reference(Object component, string field, Object value)
        {
            var serialized = new SerializedObject(component);
            serialized.FindProperty(field).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
        private static void SetBool(Object component, string field, bool value)
        {
            var serialized = new SerializedObject(component);
            serialized.FindProperty(field).boolValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
        private static void SetVector(Object component, string field, Vector2 value)
        {
            var serialized = new SerializedObject(component);
            serialized.FindProperty(field).vector2Value = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
        private static Sprite CreateCowSprite()
        {
            const string path = "Assets/Art/Prototype/Cow.png";
            if (!File.Exists(path))
            {
                var texture = new Texture2D(32, 24, TextureFormat.RGBA32, false);
                var pixels = new Color[32 * 24];
                Paint(5, 3, 9, 10, Color.white);
                Paint(20, 3, 24, 10, Color.white);
                Paint(5, 2, 9, 5, Color.black);
                Paint(20, 2, 24, 5, Color.black);
                Paint(3, 9, 25, 21, Color.white);
                Paint(7, 15, 14, 21, Color.black);
                Paint(17, 10, 23, 16, Color.black);
                Paint(23, 12, 31, 22, Color.white);
                Paint(24, 10, 32, 14, new Color(0.85f, 0.5f, 0.5f));
                Paint(28, 17, 30, 19, Color.black);
                Paint(22, 20, 25, 23, new Color(0.7f, 0.58f, 0.4f));
                Paint(1, 11, 3, 20, Color.black);
                texture.SetPixels(pixels);
                texture.Apply();
                File.WriteAllBytes(path, texture.EncodeToPNG());
                Object.DestroyImmediate(texture);
                void Paint(int x0, int y0, int x1, int y1, Color color)
                {
                    for (int y = y0; y < y1; y++)
                    for (int x = x0; x < x1; x++) pixels[y * 32 + x] = color;
                }
            }
            AssetDatabase.ImportAsset(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 16;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}

