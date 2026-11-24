using UnityEngine;

namespace Chastota885
{
    // Простейший рабочий интерфейс на встроенном IMGUI.
    // Он нужен для проверки логики до создания красивой сцены и Canvas.
    public sealed class RadioGameUI : MonoBehaviour
    {
        private string frequencyInput = "88.50";
        private string status = "Загрузка...";
        private SignalData visibleSignal;
        private EndingCondition visibleEnding;
        private Vector2 scroll;

        private void OnEnable()
        {
            if (GameManager.Instance == null) return;
            GameManager.Instance.SignalFound += HandleSignalFound;
            GameManager.Instance.StatusChanged += HandleStatus;
            GameManager.Instance.GameEnded += HandleGameEnded;
            GameManager.Instance.StateChanged += RepaintState;
        }

        private void Start()
        {
            Debug.Log("RadioGameUI запущен!");

            if (GameManager.Instance != null)
            {
                visibleSignal = GameManager.Instance.CurrentSignal;
                status = "День " + GameManager.Instance.State.CurrentDay + ". Настройте радио.";
            }
        }

        private void OnDisable()
        {
            if (GameManager.Instance == null) return;
            GameManager.Instance.SignalFound -= HandleSignalFound;
            GameManager.Instance.StatusChanged -= HandleStatus;
            GameManager.Instance.GameEnded -= HandleGameEnded;
            GameManager.Instance.StateChanged -= RepaintState;
        }

        private void HandleSignalFound(SignalData signal) { visibleSignal = signal; visibleEnding = null; }
        private void HandleStatus(string message) { status = message; }
        private void HandleGameEnded(EndingCondition ending) { visibleEnding = ending; visibleSignal = null; status = "Игра завершена."; }
        private void RepaintState() { }

        private void OnGUI()
        {
            if (GameManager.Instance == null || GameManager.Instance.State == null)
            {
                GUILayout.Label("GameManager не найден. Добавьте его на сцену.");
                return;
            }

            GameState state = GameManager.Instance.State;
            GUILayout.BeginArea(new Rect(20, 20, Mathf.Min(Screen.width - 40, 650), Screen.height - 40), GUI.skin.box);
            scroll = GUILayout.BeginScrollView(scroll);

            GUILayout.Label("ЧАСТОТА 88.5", HeaderStyle());
            GUILayout.Label("День: " + state.CurrentDay);
            GUILayout.Label("Связь: " + state.Connection + " / 100");
            GUILayout.Label("Паранойя: " + state.Paranoia + " / 100");
            GUILayout.Label("Батарея: " + state.Battery + " / 100");
            GUILayout.Space(12);

            GUILayout.Label("НАСТРОЙКА РАДИО", SectionStyle());
            GUILayout.BeginHorizontal();
            GUILayout.Label("Частота (MHz):", GUILayout.Width(120));
            frequencyInput = GUILayout.TextField(frequencyInput, GUILayout.Width(120));
            if (GUILayout.Button("Поймать сигнал", GUILayout.Width(150)))
            {
                if (float.TryParse(frequencyInput.Replace(',', '.'), System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out float frequency))
                    GameManager.Instance.TuneFrequency(frequency);
                else status = "Введите корректное число частоты.";
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(12);
            if (visibleSignal != null)
            {
                GUILayout.Label(visibleSignal.title, SectionStyle());
                GUILayout.Label(visibleSignal.body, GUILayout.ExpandHeight(false));
                GUILayout.Space(8);
                GUILayout.Label("Ваш ответ:", SectionStyle());

                foreach (ResponseData response in visibleSignal.responses)
                {
                    string caption = response.text + "  (расход батареи: " + response.batteryCost + ")";
                    if (GUILayout.Button(caption, GUILayout.MinHeight(35)))
                        GameManager.Instance.SelectResponse(response.id);
                }
            }
            else if (visibleEnding != null)
            {
                GUILayout.Label(visibleEnding.title, SectionStyle());
                GUILayout.Label(visibleEnding.body);
                if (GUILayout.Button("Начать заново", GUILayout.Height(35)))
                {
                    visibleEnding = null;
                    GameManager.Instance.StartNewGame();
                }
            }
            else if (state.HasAnsweredToday)
            {
                GUILayout.Label("Сегодняшний сигнал обработан.");
                if (GUILayout.Button("Перейти к следующему дню", GUILayout.Height(35)))
                    GameManager.Instance.AdvanceToNextDay();
            }

            GUILayout.Space(15);
            GUILayout.Label("Состояние эфира:", SectionStyle());
            GUILayout.Label(status, GUILayout.MinHeight(40));
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private GUIStyle HeaderStyle()
        {
            return new GUIStyle(GUI.skin.label) { fontSize = 24, fontStyle = FontStyle.Bold };
        }

        private GUIStyle SectionStyle()
        {
            return new GUIStyle(GUI.skin.label) { fontSize = 16, fontStyle = FontStyle.Bold };
        }
    }
}
