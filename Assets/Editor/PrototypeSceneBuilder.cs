using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace HollowHarvest.Editor
{
    public static class PrototypeSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/FarmPrototype.unity";
        private const string ArtFolder = "Assets/Art/Prototype";

        [MenuItem("HollowHarvest/Create Sprint 1 Farm Scene")]
        public static void CreateScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            // Repeated use opens the saved scene without destroying scene customizations.
            if (File.Exists(ScenePath))
            {
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    EditorSceneManager.OpenScene(ScenePath);
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            var actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(
                "Assets/Settings/InputSystem_Actions.inputactions");
            var material = AssetDatabase.LoadAssetAtPath<Material>(
                "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Lit-Default.mat");
            if (actions == null || actions.FindAction("Player/Move") == null || material == null)
                throw new System.InvalidOperationException("The project Move action or URP sprite material is missing.");

            Directory.CreateDirectory(ArtFolder);
            Directory.CreateDirectory("Assets/Scenes");
            var square = CreateSprite("Square", false);
            var farmer = CreateSprite("Farmer", true);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var farm = new GameObject("Farm").transform;

            Block("Ground", Vector2.zero, new Vector2(40, 30), new Color(0.28f, 0.38f, 0.23f), -20);
            Block("Main dirt path", new Vector2(0, -1), new Vector2(3, 24), new Color(0.51f, 0.41f, 0.27f), -19);
            Block("Cross path", new Vector2(0, 3), new Vector2(30, 2), new Color(0.51f, 0.41f, 0.27f), -19);
            var fenceColor = new Color(0.39f, 0.28f, 0.18f);
            Block("North boundary", new Vector2(0, 14), new Vector2(38, 0.5f), fenceColor, 0, true);
            Block("South boundary", new Vector2(0, -14), new Vector2(38, 0.5f), fenceColor, 0, true);
            Block("West boundary", new Vector2(-19, 0), new Vector2(0.5f, 28), fenceColor, 0, true);
            Block("East boundary", new Vector2(19, 0), new Vector2(0.5f, 28), fenceColor, 0, true);
            for (int x = -18; x <= 18; x += 2)
            {
                Block("Fence post", new Vector2(x, 14), new Vector2(0.25f, 0.9f), fenceColor * 1.2f, 1);
                Block("Fence post", new Vector2(x, -14), new Vector2(0.25f, 0.9f), fenceColor * 1.2f, 1);
            }
            Building("Farmhouse", new Vector2(-7, 7), new Vector2(6, 5), new Color(0.76f, 0.65f, 0.44f), new Color(0.39f, 0.2f, 0.18f));
            Building("Barn", new Vector2(8, 7), new Vector2(7, 5), new Color(0.62f, 0.25f, 0.2f), new Color(0.29f, 0.23f, 0.23f));
            Block("Well stone", new Vector2(5, -1), new Vector2(1.8f, 1.8f), Color.gray, 1, true);
            Block("Well water", new Vector2(5, -1), Vector2.one, new Color(0.15f, 0.3f, 0.4f), 2);
            for (int x = 0; x < 3; x++)
            for (int y = 0; y < 3; y++)
            {
                Vector2 p = new Vector2(-11 + x * 2.6f, -3 - y * 2.6f);
                Block("Empty tilled plot", p, new Vector2(2.2f, 2.2f), new Color(0.3f, 0.2f, 0.13f), -18);
                for (int row = -1; row <= 1; row++)
                    Block("Soil furrow", p + new Vector2(0, row * 0.6f), new Vector2(1.9f, 0.12f), new Color(0.4f, 0.28f, 0.17f), -17);
            }

            var player = new GameObject("Player");
            player.tag = "Player";
            var renderer = player.AddComponent<SpriteRenderer>();
            renderer.sprite = farmer;
            renderer.sharedMaterial = material;
            renderer.sortingOrder = 10;
            var body = player.AddComponent<Rigidbody2D>();
            body.gravityScale = 0;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            var collider = player.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(0.65f, 0.4f);
            collider.offset = new Vector2(0, -0.4f);
            Reference(player.AddComponent<PlayerMovement>(), "inputActions", actions);

            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0, 0, -10);
            var camera = cameraObject.GetComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 8;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.08f, 0.1f, 0.09f);
            cameraObject.AddComponent<UniversalAdditionalCameraData>();
            Reference(cameraObject.AddComponent<CameraFollow>(), "target", player.transform);

            var clock = new GameObject("Game Clock").AddComponent<GameClock>();
            var lightObject = new GameObject("Global Light 2D");
            var light = lightObject.AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Global;
            Reference(lightObject.AddComponent<DayNightLighting>(), "clock", clock);
            CreateHud(clock);

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) }
                .Concat(EditorBuildSettings.scenes.Where(s => s.path != ScenePath)).ToArray();
            AssetDatabase.SaveAssets();
            Selection.activeGameObject = player;
            Debug.Log("Sprint 1 farm is ready. Press Play. Adjust Game Clock > Minutes Per Second to test time.");

            GameObject Block(string name, Vector2 position, Vector2 size, Color color, int order, bool solid = false)
            {
                var go = new GameObject(name);
                go.transform.SetParent(farm);
                go.transform.position = position;
                go.transform.localScale = new Vector3(size.x, size.y, 1);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = square;
                sr.sharedMaterial = material;
                sr.color = color;
                sr.sortingOrder = order;
                if (solid) go.AddComponent<BoxCollider2D>();
                return go;
            }

            void Building(string name, Vector2 p, Vector2 size, Color walls, Color roof)
            {
                Block(name, p, size, walls, 1, true);
                Block(name + " roof", p + new Vector2(0, size.y * 0.25f), new Vector2(size.x + 0.5f, size.y * 0.6f), roof, 2);
                Block(name + " door", p - new Vector2(0, size.y * 0.3f), new Vector2(1.2f, 1.8f), new Color(0.2f, 0.15f, 0.12f), 3);
                foreach (float side in new[] { -1f, 1f })
                    Block(name + " window", p + new Vector2(side * size.x * 0.3f, -size.y * 0.25f), Vector2.one, new Color(0.85f, 0.76f, 0.43f), 3);
            }
        }

        private static Sprite CreateSprite(string name, bool character)
        {
            string path = ArtFolder + "/" + name + ".png";
            if (!File.Exists(path))
            {
                int width = character ? 16 : 1;
                int height = character ? 24 : 1;
                var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
                var pixels = new Color[width * height];
                if (!character) pixels[0] = Color.white;
                else
                {
                    Paint(4, 1, 7, 7, new Color(0.15f, 0.18f, 0.23f));
                    Paint(9, 1, 12, 7, new Color(0.15f, 0.18f, 0.23f));
                    Paint(3, 7, 13, 15, new Color(0.63f, 0.65f, 0.43f));
                    Paint(5, 15, 11, 20, new Color(0.88f, 0.67f, 0.45f));
                    Paint(2, 19, 14, 21, new Color(0.76f, 0.51f, 0.25f));
                    Paint(5, 21, 11, 23, new Color(0.76f, 0.51f, 0.25f));
                }
                texture.SetPixels(pixels);
                texture.Apply();
                File.WriteAllBytes(path, texture.EncodeToPNG());
                Object.DestroyImmediate(texture);
                void Paint(int x0, int y0, int x1, int y1, Color color)
                {
                    for (int y = y0; y < y1; y++)
                    for (int x = x0; x < x1; x++) pixels[y * width + x] = color;
                }
            }
            AssetDatabase.ImportAsset(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = character ? 16 : 1;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static void CreateHud(GameClock clock)
        {
            var root = new GameObject("HUD", typeof(Canvas), typeof(CanvasScaler));
            root.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.matchWidthOrHeight = 0.5f;
            var panel = new GameObject("Clock Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(root.transform, false);
            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(16, -16);
            rect.sizeDelta = new Vector2(430, 116);
            panel.GetComponent<Image>().color = new Color(0.06f, 0.08f, 0.06f, 0.9f);
            var textObject = new GameObject("Day and Time", typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(panel.transform, false);
            rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(16, 8);
            rect.offsetMax = new Vector2(-16, -8);
            var text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 22;
            text.color = new Color(0.94f, 0.9f, 0.74f);
            text.raycastTarget = false;
            text.text = "HOLLOW HARVEST\nDAY 01 / 30    06:00\nWASD / ARROWS  -  MOVE";
            var hud = root.AddComponent<ClockHud>();
            Reference(hud, "clock", clock);
            Reference(hud, "label", text);
        }

        private static void Reference(Object component, string field, Object value)
        {
            var serialized = new SerializedObject(component);
            serialized.FindProperty(field).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
