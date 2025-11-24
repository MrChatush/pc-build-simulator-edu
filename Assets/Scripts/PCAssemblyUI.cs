using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PCAssemblyUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI stageTitleText;
    public TextMeshProUGUI stageDescriptionText;
    public Slider progressSlider;
    public Button nextStageButton;
    public Button previousStageButton;
    public TextMeshProUGUI progressText;
    public Button completeStageButton;

    [Header("Optional References")]
    public GameObject assemblyPanel;
    public TextMeshProUGUI debugText;

    [Header("Update Settings")]
    public float updateInterval = 0.1f; // Частота обновления в секундах
    public bool autoRefresh = true;

    private PCAssemblyManager assemblyManager;
    private float lastUpdateTime;
    private int lastCompletedStages = -1;
    private int lastStageIndex = -1;
    private string lastStageName = "";

    void Start()
    {
        InitializeUI();
    }

    void InitializeUI()
    {
        // Ищем PCAssemblyManager в сцене
        assemblyManager = FindObjectOfType<PCAssemblyManager>();

        if (assemblyManager == null)
        {
            Debug.LogError("PCAssemblyManager не найден в сцене! Создай GameObject с PCAssemblyManager.");

            // Отключаем UI если менеджер не найден
            if (assemblyPanel != null)
                assemblyPanel.SetActive(false);

            return;
        }

        // Проверяем включена ли система этапов
        if (!assemblyManager.enablePCAssemblyStages)
        {
            Debug.LogWarning("PCAssemblyStages отключен в PCAssemblyManager!");

            if (assemblyPanel != null)
                assemblyPanel.SetActive(false);

            return;
        }

        // Подписываемся на события
        assemblyManager.OnStageChangedEvent.AddListener(OnStageChanged);
        assemblyManager.OnStageCompletedEvent.AddListener(OnStageCompleted);

        // Настраиваем кнопки
        if (nextStageButton != null)
            nextStageButton.onClick.AddListener(OnNextStageClicked);

        if (previousStageButton != null)
            previousStageButton.onClick.AddListener(OnPreviousStageClicked);

        if (completeStageButton != null)
            completeStageButton.onClick.AddListener(OnCompleteStageClicked);

        // Инициализируем UI
        RefreshAllUI();

        // Включаем панель если она есть
        if (assemblyPanel != null)
            assemblyPanel.SetActive(true);

        Debug.Log("PCAssemblyUI успешно инициализирован!");
    }

    void Update()
    {
        // Автоматическое обновление UI с интервалом
        if (autoRefresh && assemblyManager != null && Time.time - lastUpdateTime >= updateInterval)
        {
            RefreshProgress(); // Обновляем только прогресс для производительности
            lastUpdateTime = Time.time;
        }
    }

    void OnStageChanged(int stageIndex, PCAssemblyManager.PCAssemblyStage stage)
    {
        RefreshAllUI();
        Debug.Log($"Этап изменен: {stage.stageName}");
    }

    void OnStageCompleted(int stageIndex, PCAssemblyManager.PCAssemblyStage stage)
    {
        RefreshAllUI();
        Debug.Log($"Этап завершен: {stage.stageName}");
    }

    void OnNextStageClicked()
    {
        if (assemblyManager != null)
        {
            assemblyManager.NextStage();
            RefreshAllUI();
        }
    }

    void OnPreviousStageClicked()
    {
        if (assemblyManager != null)
        {
            assemblyManager.PreviousStage();
            RefreshAllUI();
        }
    }

    void OnCompleteStageClicked()
    {
        if (assemblyManager != null)
        {
            assemblyManager.CompleteCurrentStage();
            RefreshAllUI();
        }
    }

    void UpdateUI(int stageIndex, PCAssemblyManager.PCAssemblyStage stage)
    {
        // Проверяем все ссылки чтобы избежать NullReferenceException
        if (stageTitleText != null)
            stageTitleText.text = $"{stageIndex + 1}. {stage.stageName}";

        if (stageDescriptionText != null)
            stageDescriptionText.text = stage.stageDescription;

        if (progressSlider != null && assemblyManager != null)
            progressSlider.value = assemblyManager.GetCurrentStageProgress();

        if (progressText != null && assemblyManager != null)
        {
            float progress = assemblyManager.GetCurrentStageProgress() * 100f;
            progressText.text = $"Прогресс этапа: {progress:F0}%";
        }

        // Обновляем состояние кнопок
        if (previousStageButton != null)
            previousStageButton.interactable = stageIndex > 0;

        if (nextStageButton != null && assemblyManager != null)
            nextStageButton.interactable = stageIndex < assemblyManager.AssemblyStages.Count - 1 && !stage.isLocked;

        if (completeStageButton != null)
            completeStageButton.interactable = !stage.isCompleted && !stage.isLocked;

        // Отладочная информация
        if (debugText != null && assemblyManager != null)
        {
            int completedStages = GetCompletedStagesCount();
            debugText.text = $"Этап: {stageIndex + 1}/{assemblyManager.AssemblyStages.Count}\n" +
                           $"Завершено: {completedStages}/{assemblyManager.AssemblyStages.Count}\n" +
                           $"Заблокирован: {stage.isLocked}\n" +
                           $"Завершен: {stage.isCompleted}";
        }
    }

    // Основной метод обновления всего UI
    void RefreshAllUI()
    {
        if (assemblyManager != null)
        {
            var currentStage = assemblyManager.GetCurrentStage();
            if (currentStage != null)
            {
                UpdateUI(assemblyManager.CurrentStageIndex, currentStage);
            }
        }
    }

    // Быстрое обновление только прогресса (для Update)
    void RefreshProgress()
    {
        if (assemblyManager == null) return;

        // Проверяем изменился ли прогресс
        int currentCompleted = GetCompletedStagesCount();
        int currentStageIndex = assemblyManager.CurrentStageIndex;
        var currentStage = assemblyManager.GetCurrentStage();
        string currentStageName = currentStage != null ? currentStage.stageName : "";

        // Обновляем только если есть изменения
        bool needsUpdate = currentCompleted != lastCompletedStages ||
                          currentStageIndex != lastStageIndex ||
                          currentStageName != lastStageName;

        if (needsUpdate)
        {
            if (progressSlider != null)
                progressSlider.value = assemblyManager.GetCurrentStageProgress();

            if (progressText != null)
            {
                float progress = assemblyManager.GetAssemblyProgress() * 100f;
                progressText.text = $"Прогресс: {progress:F0}%";
            }

            // Сохраняем текущие значения для сравнения
            lastCompletedStages = currentCompleted;
            lastStageIndex = currentStageIndex;
            lastStageName = currentStageName;
        }
    }


    // Вспомогательный метод для подсчета завершенных этапов
    int GetCompletedStagesCount()
    {
        if (assemblyManager == null || assemblyManager.AssemblyStages == null) return 0;

        int count = 0;
        foreach (var stage in assemblyManager.AssemblyStages)
        {
            if (stage.isCompleted) count++;
        }
        return count;
    }

    // Метод для принудительного обновления UI (можно вызвать из других скриптов)
    public void RefreshUI()
    {
        RefreshAllUI();
    }

    // Метод для переключения на конкретный этап (для дебага)
    public void SetStage(int stageIndex)
    {
        if (assemblyManager != null)
        {
            assemblyManager.SetCurrentStage(stageIndex);
            RefreshAllUI();
        }
    }

    // Метод для сброса всех этапов
    public void ResetStages()
    {
        if (assemblyManager != null)
        {
            assemblyManager.ResetStages();
            RefreshAllUI();
        }
    }

    // Включить/выключить автоматическое обновление
    public void SetAutoRefresh(bool enabled)
    {
        autoRefresh = enabled;
    }

    // Установить интервал обновления
    public void SetUpdateInterval(float interval)
    {
        updateInterval = Mathf.Max(0.01f, interval);
    }
}