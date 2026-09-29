using UnityEngine;
using UnityEngine.SceneManagement;

namespace DressUpGame.UI
{
    /// <summary>
    /// Full-screen room background behind the Girl character (world-space sprite).
    /// </summary>
    [ExecuteAlways]
    public class GameBackgroundDisplay : MonoBehaviour
    {
        public const string DefaultResourcePath = "UI/background";
        public const string DefaultEditorAssetPath = "Assets/Resources/UI/background.png";
        public const string PetResourcePath = "UI/menu/background_2";
        public const string PetEditorAssetPath = "Assets/Resources/UI/menu/background_2.png";
        public const float PetLightenAmount = 0.32f;

        private const int SortingOrder = -100;
        private const string LightenShaderName = "DressUpGame/BackgroundLightenSprite";

        [SerializeField] private string resourcePath = DefaultResourcePath;
        [SerializeField] private string editorAssetPath = DefaultEditorAssetPath;
        [SerializeField] [Range(0f, 0.35f)] private float lightenAmount = 0.12f;

        private Material lightenMaterial;
        private Sprite cachedSprite;
        private string cachedResourcePath;
        private SpriteRenderer spriteRenderer;
        private float lastAspect = -1f;
        private float lastOrthoSize = -1f;
        private int lastScreenWidth = -1;
        private int lastScreenHeight = -1;

        public static GameBackgroundDisplay EnsureExists(Camera camera)
        {
            GameBackgroundDisplay existing = FindAnyObjectByType<GameBackgroundDisplay>();
            if (existing == null)
            {
                GameObject backgroundObject = new GameObject("GameBackground");
                existing = backgroundObject.AddComponent<GameBackgroundDisplay>();
                if (IsPetGameplayScene())
                {
                    existing.UsePetBackground();
                }
            }

            existing.Apply(camera);
            return existing;
        }

        public void UsePetBackground()
        {
            SetBackgroundSources(PetResourcePath, PetEditorAssetPath);
            lightenAmount = PetLightenAmount;
        }

        public void SetBackgroundSources(string resourcesPath, string editorPath)
        {
            resourcePath = resourcesPath;
            editorAssetPath = editorPath ?? string.Empty;
            cachedSprite = null;
            cachedResourcePath = null;
        }

        private static bool IsPetGameplayScene()
        {
            return SceneManager.GetActiveScene().name == "PetScene";
        }

        private void OnEnable()
        {
            if (IsPetGameplayScene())
            {
                if (resourcePath == DefaultResourcePath)
                {
                    UsePetBackground();
                }
                else if (resourcePath == PetResourcePath)
                {
                    lightenAmount = PetLightenAmount;
                }
            }

            Apply(Camera.main);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (spriteRenderer != null)
            {
                ApplyLightenMaterial();
            }
        }
#endif

        private void LateUpdate()
        {
            Camera camera = Camera.main;
            if (camera == null || !camera.orthographic)
            {
                return;
            }

            if (Mathf.Approximately(lastAspect, camera.aspect)
                && Mathf.Approximately(lastOrthoSize, camera.orthographicSize)
                && lastScreenWidth == Screen.width
                && lastScreenHeight == Screen.height)
            {
                return;
            }

            Apply(camera);
        }

        public void Apply(Camera camera)
        {
            if (camera == null || !camera.orthographic)
            {
                return;
            }

            Sprite sprite = LoadSprite();
            if (sprite == null)
            {
                return;
            }

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
                if (spriteRenderer == null)
                {
                    spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
                }
            }

            spriteRenderer.sprite = sprite;
            spriteRenderer.sortingOrder = SortingOrder;
            spriteRenderer.color = Color.white;
            ApplyLightenMaterial();

            GetCameraWorldBounds(camera, out float cameraWidth, out float cameraHeight, out Vector3 cameraCenter);
            Vector2 spriteSize = sprite.bounds.size;
            if (spriteSize.x <= 0.001f || spriteSize.y <= 0.001f)
            {
                return;
            }

            // Cover the full viewport and pin to the top so UI (e.g. sound toggle) sits on the room art.
            float scaleX = cameraWidth / spriteSize.x;
            float scaleY = cameraHeight / spriteSize.y;
            float scale = Mathf.Max(scaleX, scaleY);

            transform.localScale = Vector3.one * scale;
            float scaledHeight = spriteSize.y * scale;
            float cameraTop = cameraCenter.y + (cameraHeight * 0.5f);
            transform.position = new Vector3(cameraCenter.x, cameraTop - (scaledHeight * 0.5f), 0f);
            transform.rotation = Quaternion.identity;

            lastAspect = camera.aspect;
            lastOrthoSize = camera.orthographicSize;
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
        }

        private static void GetCameraWorldBounds(Camera camera, out float width, out float height, out Vector3 center)
        {
            float depth = Mathf.Abs(camera.transform.position.z);
            Vector3 bottomLeft = camera.ViewportToWorldPoint(new Vector3(0f, 0f, depth));
            Vector3 topRight = camera.ViewportToWorldPoint(new Vector3(1f, 1f, depth));
            width = topRight.x - bottomLeft.x;
            height = topRight.y - bottomLeft.y;
            center = new Vector3(
                (bottomLeft.x + topRight.x) * 0.5f,
                (bottomLeft.y + topRight.y) * 0.5f,
                0f);
        }

        private void ApplyLightenMaterial()
        {
            Material material = GetLightenMaterial();
            if (material == null)
            {
                return;
            }

            float amount = resourcePath == PetResourcePath ? PetLightenAmount : lightenAmount;
            material.SetFloat("_Lighten", amount);
            spriteRenderer.sharedMaterial = material;
        }

        private Material GetLightenMaterial()
        {
            if (lightenMaterial != null)
            {
                return lightenMaterial;
            }

            Shader shader = Shader.Find(LightenShaderName);
            if (shader == null)
            {
                return null;
            }

            lightenMaterial = new Material(shader);
            return lightenMaterial;
        }

        private Sprite LoadSprite()
        {
            if (cachedSprite != null && cachedResourcePath == resourcePath)
            {
                return cachedSprite;
            }

            cachedResourcePath = resourcePath;
            cachedSprite = Resources.Load<Sprite>(resourcePath);
#if UNITY_EDITOR
            if (cachedSprite == null && !string.IsNullOrEmpty(editorAssetPath))
            {
                cachedSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(editorAssetPath);
            }
#endif
            return cachedSprite;
        }
    }
}
