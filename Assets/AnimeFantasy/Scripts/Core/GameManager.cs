using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace AnimeFantasy
{
    /// <summary>
    /// Zentrale Steuerung: baut Canvas und Hintergrund, wechselt zwischen Menü und Kampf
    /// und verwaltet den Spielstand.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        public SaveData Save { get; private set; }

        RectTransform canvasRoot;
        RectTransform safeArea;
        Image background;
        Sprite backgroundSprite;
        GameObject currentScreen;
        Rect lastSafeArea;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Screen.orientation = ScreenOrientation.Portrait;

            EnsureCamera();
            UIFactory.EnsureEventSystem();

            var canvas = UIFactory.CreateCanvas("GameCanvas");
            canvas.transform.SetParent(transform, false);
            canvasRoot = (RectTransform)canvas.transform;

            background = UIFactory.CreateImage("Background", canvasRoot, Color.white);
            CreateStars();

            safeArea = UIFactory.CreateRect("SafeArea", canvasRoot);
            ApplySafeArea();

            Save = SaveSystem.Load();
            ShowMenu();
        }

        void Update()
        {
            if (Screen.safeArea != lastSafeArea) ApplySafeArea();

            // Android-Zurück-Taste: im Kampf ins Menü, im Menü beenden
            if (BackPressed())
            {
                if (currentScreen != null && currentScreen.GetComponent<BattleView>() != null) ShowMenu();
                else Application.Quit();
            }
        }

        static bool BackPressed()
        {
#if ENABLE_INPUT_SYSTEM
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb != null && kb.escapeKey.wasPressedThisFrame) return true;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.Escape)) return true;
#endif
            return false;
        }

        static void EnsureCamera()
        {
            if (Camera.main != null) return;
            var cam = new GameObject("Main Camera", typeof(Camera)).GetComponent<Camera>();
            cam.tag = "MainCamera";
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            DontDestroyOnLoad(cam.gameObject);
        }

        void ApplySafeArea()
        {
            var sa = Screen.safeArea;
            lastSafeArea = sa;
            if (Screen.width <= 0 || Screen.height <= 0) return;
            var min = new Vector2(sa.xMin / Screen.width, sa.yMin / Screen.height);
            var max = new Vector2(sa.xMax / Screen.width, sa.yMax / Screen.height);
            safeArea.Stretch(min.x, min.y, max.x, max.y);
        }

        void CreateStars()
        {
            var layer = UIFactory.CreateRect("Stars", canvasRoot).Stretch(0f, 0f, 1f, 1f);
            for (int i = 0; i < 45; i++)
            {
                var star = UIFactory.CreateImage("Star", layer, new Color(1f, 1f, 1f, Random.Range(0.15f, 0.6f)), SpriteFactory.Circle);
                var rt = star.rectTransform;
                var p = new Vector2(Random.value, Random.Range(0.35f, 1f));
                rt.anchorMin = rt.anchorMax = p;
                float s = Random.Range(4f, 12f);
                rt.sizeDelta = new Vector2(s, s);
            }
        }

        public void SetBackground(Color top, Color bottom)
        {
            if (backgroundSprite != null)
            {
                Destroy(backgroundSprite.texture);
                Destroy(backgroundSprite);
            }
            backgroundSprite = SpriteFactory.VerticalGradient(top, bottom);
            background.sprite = backgroundSprite;
        }

        GameObject NewScreen(string name)
        {
            if (currentScreen != null) Destroy(currentScreen);
            Time.timeScale = 1f;
            var rt = UIFactory.CreateRect(name, safeArea).Stretch(0f, 0f, 1f, 1f);
            currentScreen = rt.gameObject;
            return currentScreen;
        }

        public void ShowMenu()
        {
            NewScreen("MenuScreen").AddComponent<MenuView>().Build(this);
        }

        public void NewGame()
        {
            Save = SaveSystem.Reset();
            ShowMenu();
        }

        public void StartBattle(int stageIndex, bool training)
        {
            int levelBonus;
            var stage = GameDatabase.GetStage(stageIndex, out levelBonus);

            var party = GameDatabase.HeroIds
                .Select(id => new BattleUnit(GameDatabase.GetCharacter(id), Save.GetHero(id).level, true))
                .ToList();
            var enemies = stage.Enemies
                .Select(id => new BattleUnit(GameDatabase.GetCharacter(id), stage.EnemyLevel + levelBonus, false))
                .ToList();

            SetBackground(stage.SkyTop, stage.SkyBottom);
            var screen = NewScreen("BattleScreen");
            var view = screen.AddComponent<BattleView>();
            var controller = screen.AddComponent<BattleController>();

            string title = training
                ? "Training: " + stage.Name
                : "Kapitel " + (stageIndex + 1) + ": " + stage.Name;

            controller.Begin(view, stage, title, party, enemies,
                (victory, xp) => OnBattleFinished(view, stageIndex, training, victory, xp));
        }

        void OnBattleFinished(BattleView view, int stageIndex, bool training, bool victory, int xp)
        {
            Time.timeScale = 1f;
            if (victory)
            {
                var messages = Progression.AddXp(Save, xp);
                bool newChapter = !training && stageIndex >= Save.stage;
                if (newChapter) Save.stage = stageIndex + 1;
                SaveSystem.Save(Save);

                string body = "Erhaltene EP: " + xp;
                if (messages.Count > 0) body += "\n\n" + string.Join("\n", messages.ToArray());
                if (newChapter && Save.stage % GameDatabase.StageCount == 0)
                    body += "\n\nDie Dunkelfürstin ist besiegt!\nEin noch härterer Durchlauf beginnt ...";

                UIFactory.ShowModal(view.transform, "Sieg!", body,
                    "Weiter", () => StartBattle(Save.stage, false),
                    "Menü", () => ShowMenu());
            }
            else
            {
                UIFactory.ShowModal(view.transform, "Niederlage", "Deine Gruppe wurde besiegt.\nTrainiere und versuche es erneut!",
                    "Nochmal", () => StartBattle(stageIndex, training),
                    "Menü", () => ShowMenu());
            }
        }
    }
}
