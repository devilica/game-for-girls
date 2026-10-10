using System.Collections;
using System.Collections.Generic;
using DressUpGame.Character;
using DressUpGame.Data;
using DressUpGame.Pet;
using DressUpGame.Save;
using DressUpGame.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DressUpGame.MysteryBox
{
    public class MysteryBoxController : MonoBehaviour
    {
        public const string MysteryBoxSceneName = "MysteryBoxScene";

        private const int RefreshInterstitialEvery = 3;
        private const int BoxRewardedEvery = 10;

        [Header("Data")]
        [SerializeField] private MysteryLootRegistry lootRegistry;

        [Header("Characters")]
        [SerializeField] private CharacterCustomizer girlCustomizer;
        [SerializeField] private PetCustomizer petCustomizer;
        [SerializeField] private CharacterSaveManager girlSaveManager;
        [SerializeField] private PetSaveManager petSaveManager;

        [Header("Box art")]
        [SerializeField] private Sprite headerBannerSprite;
        [SerializeField] private Sprite girlBoxSprite;
        [SerializeField] private Sprite petBoxSprite;
        [SerializeField] private Sprite bestiesBoxSprite;

        [Header("UI")]
        [SerializeField] private MysteryBoxUI mysteryUi;

        [Header("Animation")]
        [SerializeField] private float shakeDuration = 0.45f;
        [SerializeField] private float shakeStrength = 12f;

        private MysteryRewardApplier rewardApplier;
        private readonly HashSet<MysteryBoxType> boxesOpening = new HashSet<MysteryBoxType>();
        private bool refreshInProgress;
        private bool revealSessionActive;
        private MysteryBoxType activeRevealBoxType;
        private CharacterSaveData girlRevealBaseline;
        private PetSaveData petRevealBaseline;
        private List<string> unlockRevealBaseline;
        private int revealSessionRefreshCount;
        private int sessionBoxOpenCount;

        private void Awake()
        {
            if (lootRegistry == null)
            {
                lootRegistry = Resources.Load<MysteryLootRegistry>(MysteryLootRegistry.ResourcePath);
            }

            if (girlCustomizer == null)
            {
                girlCustomizer = FindAnyObjectByType<CharacterCustomizer>();
            }

            if (petCustomizer == null)
            {
                petCustomizer = FindAnyObjectByType<PetCustomizer>();
            }

            if (girlSaveManager == null)
            {
                girlSaveManager = FindAnyObjectByType<CharacterSaveManager>();
            }

            if (petSaveManager == null)
            {
                petSaveManager = FindAnyObjectByType<PetSaveManager>();
            }

            if (mysteryUi == null)
            {
                mysteryUi = FindAnyObjectByType<MysteryBoxUI>();
            }

            GameCatalog gameCatalog = lootRegistry != null ? lootRegistry.GameCatalog : null;
            PetCatalog petCatalog = lootRegistry != null ? lootRegistry.PetCatalog : null;
            rewardApplier = new MysteryRewardApplier(
                girlCustomizer,
                petCustomizer,
                girlSaveManager,
                petSaveManager,
                gameCatalog,
                petCatalog);

            ResolveBoxSpritesFromUi();

            if (mysteryUi != null)
            {
                mysteryUi.SetBoxSprites(girlBoxSprite, petBoxSprite, bestiesBoxSprite);
                mysteryUi.BoxClicked += HandleBoxClicked;
                mysteryUi.ContinueClicked += HandleContinue;
                mysteryUi.RefreshClicked += HandleRefresh;
            }
        }

        private void Start()
        {
            sessionBoxOpenCount = 0;
            BreakInterstitialAdController.EnsureExists();
            RewardedAdController.EnsureExists();
            BottomBannerAdController.EnsureExists();
            BottomBannerAdController.RefreshVisibility();

            girlSaveManager?.LoadCharacter();
            petSaveManager?.LoadPet();
            MysteryBoxPreviewLayout previewLayout = FindAnyObjectByType<MysteryBoxPreviewLayout>();
            previewLayout?.ApplyLayout();
            Canvas canvas = FindAnyObjectByType<Canvas>();
            if (canvas != null)
            {
                MysteryBoxSceneLayout.EnsureOnCanvas(canvas);
                StartCoroutine(ApplyMysterySceneLayoutAfterFirstFrame(canvas));
            }
        }

        private IEnumerator ApplyMysterySceneLayoutAfterFirstFrame(Canvas canvas)
        {
            yield return null;
            if (canvas == null || !canvas.gameObject.activeInHierarchy)
            {
                yield break;
            }

            MysteryBoxSceneLayout layout = canvas.GetComponent<MysteryBoxSceneLayout>();
            if (layout != null && layout.isActiveAndEnabled)
            {
                layout.ApplyLayout();
            }
        }

        private void OnDestroy()
        {
            if (mysteryUi != null)
            {
                mysteryUi.BoxClicked -= HandleBoxClicked;
                mysteryUi.ContinueClicked -= HandleContinue;
                mysteryUi.RefreshClicked -= HandleRefresh;
            }
        }

        private void ResolveBoxSpritesFromUi()
        {
            if (mysteryUi == null)
            {
                return;
            }

            if (girlBoxSprite == null)
            {
                girlBoxSprite = mysteryUi.GetAssignedBoxSprite(MysteryBoxType.GirlFashion);
            }

            if (petBoxSprite == null)
            {
                petBoxSprite = mysteryUi.GetAssignedBoxSprite(MysteryBoxType.PetSurprise);
            }

            if (bestiesBoxSprite == null)
            {
                bestiesBoxSprite = mysteryUi.GetAssignedBoxSprite(MysteryBoxType.Besties);
            }

            Sprite resourcesBox = MysteryBoxArt.LoadBoxSprite();
            if (girlBoxSprite == null)
            {
                girlBoxSprite = resourcesBox;
            }

            if (petBoxSprite == null)
            {
                petBoxSprite = resourcesBox;
            }

            if (bestiesBoxSprite == null)
            {
                bestiesBoxSprite = resourcesBox;
            }
        }

        private void HandleBoxClicked(MysteryBoxType boxType)
        {
            if (refreshInProgress || boxesOpening.Contains(boxType))
            {
                return;
            }

            StartCoroutine(OpenBoxRoutine(boxType));
        }

        private void HandleContinue()
        {
            mysteryUi?.HideReveal();
            mysteryUi?.SetBoxButtonsInteractable(true);
            ClearRevealBaseline();
        }

        private void HandleRefresh()
        {
            if (refreshInProgress || boxesOpening.Count > 0 || !revealSessionActive)
            {
                return;
            }

            StartCoroutine(RefreshSameBoxRoutine());
        }

        private IEnumerator OpenBoxRoutine(MysteryBoxType boxType)
        {
            boxesOpening.Add(boxType);
            sessionBoxOpenCount++;
            if (sessionBoxOpenCount % BoxRewardedEvery == 0)
            {
                bool rewarded = false;
                yield return RewardedAdController.WaitForRewardRoutine(result => rewarded = result);
                GameplayPauseGuard.EnsureUnpaused();
                if (!rewarded)
                {
                    boxesOpening.Remove(boxType);
                    yield break;
                }
            }

            mysteryUi?.SetBoxButtonInteractable(boxType, false);
            mysteryUi?.HideReveal();

            if (!revealSessionActive)
            {
                CaptureRevealBaseline();
                revealSessionActive = true;
            }

            activeRevealBoxType = boxType;

            RectTransform boxRect = mysteryUi?.GetBoxRect(boxType);
            if (boxRect != null)
            {
                yield return ShakeRectRoutine(boxRect);
            }
            else
            {
                yield return new WaitForSeconds(0.2f);
            }

            yield return RollApplyAndShowRoutine(boxType);
            mysteryUi?.SetBoxButtonInteractable(boxType, true);
            boxesOpening.Remove(boxType);
        }

        private IEnumerator RefreshSameBoxRoutine()
        {
            refreshInProgress = true;
            revealSessionRefreshCount++;
            if (revealSessionRefreshCount % RefreshInterstitialEvery == 0)
            {
                yield return BreakInterstitialAdController.WaitForBreakAdRoutine();
                GameplayPauseGuard.EnsureUnpaused();
            }

            mysteryUi?.SetRefreshButtonVisible(false);
            mysteryUi?.SetBoxButtonInteractable(activeRevealBoxType, false);

            RestoreRevealBaseline();

            RectTransform boxRect = mysteryUi?.GetBoxRect(activeRevealBoxType);
            if (boxRect != null)
            {
                yield return ShakeRectRoutine(boxRect, shakeStrength * 0.65f);
            }

            yield return RollApplyAndShowRoutine(activeRevealBoxType);
            mysteryUi?.SetBoxButtonInteractable(activeRevealBoxType, true);
            refreshInProgress = false;
        }

        private IEnumerator RollApplyAndShowRoutine(MysteryBoxType boxType)
        {
            MysteryOpenResult result = MysteryRewardSelector.Roll(boxType, lootRegistry, null);
            if (result.Status == MysteryOpenStatus.Success)
            {
                GameCatalog gameCatalog = lootRegistry != null ? lootRegistry.GameCatalog : null;
                PetCatalog petCatalog = lootRegistry != null ? lootRegistry.PetCatalog : null;
                List<MysteryLootEntry> refreshedRewards =
                    MysteryLootItemResolver.RefreshRewards(result.Rewards, gameCatalog, petCatalog);
                result = MysteryOpenResult.Success(refreshedRewards);
                rewardApplier.ApplyRewards(refreshedRewards);
            }

            mysteryUi?.ShowReveal(result);
            yield return null;
        }

        private void CaptureRevealBaseline()
        {
            if (girlCustomizer != null)
            {
                girlRevealBaseline = CloneSaveData(girlCustomizer.CaptureSaveData());
            }

            if (petCustomizer != null)
            {
                petRevealBaseline = CloneSaveData(petCustomizer.CaptureSaveData());
            }

            unlockRevealBaseline = new List<string>(MysteryUnlockStore.LoadSet());
        }

        private void RestoreRevealBaseline()
        {
            if (girlRevealBaseline != null && girlCustomizer != null)
            {
                girlCustomizer.ApplySaveData(CloneSaveData(girlRevealBaseline));
            }

            if (petRevealBaseline != null && petCustomizer != null)
            {
                petCustomizer.ApplySaveData(CloneSaveData(petRevealBaseline));
            }

            if (unlockRevealBaseline != null)
            {
                MysteryUnlockStore.RestoreSet(unlockRevealBaseline);
            }
        }

        private void ClearRevealBaseline()
        {
            revealSessionActive = false;
            revealSessionRefreshCount = 0;
            girlRevealBaseline = null;
            petRevealBaseline = null;
            unlockRevealBaseline = null;
        }

        private static T CloneSaveData<T>(T data) where T : class
        {
            if (data == null)
            {
                return null;
            }

            return JsonUtility.FromJson<T>(JsonUtility.ToJson(data));
        }

        private IEnumerator ShakeRectRoutine(RectTransform target, float strengthOverride = -1f)
        {
            float strength = strengthOverride >= 0f ? strengthOverride : shakeStrength;
            Vector3 baseRotation = target.localEulerAngles;
            float elapsed = 0f;
            while (elapsed < shakeDuration)
            {
                elapsed += Time.deltaTime;
                float wobble = Mathf.Sin(elapsed * 40f) * strength;
                target.localEulerAngles = baseRotation + new Vector3(0f, 0f, wobble);
                yield return null;
            }

            target.localEulerAngles = baseRotation;
        }

        public bool TryHandleBackNavigation()
        {
            GameplayPauseGuard.EnsureUnpaused();
            SceneManager.LoadScene(MainMenuScreenController.MenuSceneName);
            return true;
        }
    }
}
