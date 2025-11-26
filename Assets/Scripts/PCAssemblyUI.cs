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
    public float updateInterval = 0.1f;
    public bool autoRefresh = true;

    private PCAssemblyManager assemblyManager;
    private float lastUpdateTime;
    private int lastCompletedStages = -1;
    private int lastStageIndex = -1;

    void Start()
    {
        InitializeUI();
    }

    void InitializeUI()
    {
        assemblyManager = FindObjectOfType<PCAssemblyManager>();

        if (assemblyManager == null)
        {
            Debug.LogError("PCAssemblyManager не найден!");
            if (assemblyPanel != null)
                assemblyPanel.SetActive(false);
            return;
        }

        if (!assemblyManager.enablePCAssemblyStages)
        {
            Debug.LogWarning("PCAssemblyStages отключен!");
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

        RefreshAllUI();

        if (assemblyPanel != null)
            assemblyPanel.SetActive(true);

        Debug.Log("PCAssemblyUI успешно инициализирован!");
    }

    void Update()
    {
        if (autoRefresh && assemblyManager != null && Time.time - lastUpdateTime >= updateInterval)
        {
            RefreshProgress();
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
        // Обновляем текст
        if (stageTitleText != null)
            stageTitleText.text = $"{stageIndex + 1}. {stage.stageName}";

        if (stageDescriptionText != null)
            stageDescriptionText.text = stage.stageDescription;

        // ОБНОВЛЯЕМ ПРОГРЕСС - ИСПОЛЬЗУЕМ ПРАВИЛЬНЫЙ МЕТОД
        float progress = CalculateProgress(stageIndex, stage);

        if (progressSlider != null)
            progressSlider.value = progress;

        if (progressText != null)
            progressText.text = $"Прогресс: {progress * 100:F0}%";

        // Обновляем кнопки
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
                           $"Прогресс: {progress * 100:F0}%\n" +
                           $"Заблокирован: {stage.isLocked}\n" +
                           $"Завершен: {stage.isCompleted}";
        }
    }

    // ПРАВИЛЬНЫЙ РАСЧЕТ ПРОГРЕССА
    float CalculateProgress(int stageIndex, PCAssemblyManager.PCAssemblyStage stage)
    {
        if (assemblyManager == null) return 0f;

        // Вариант 1: Общий прогресс всей сборки
        float totalProgress = (float)GetCompletedStagesCount() / assemblyManager.AssemblyStages.Count;

        // Вариант 2: Прогресс текущего этапа (0% или 100%)
        float stageProgress = stage.isCompleted ? 1f : 0f;

        // Вариант 3: Комбинированный прогресс
        // Текущий этап считается как 50% если не завершен
        float combinedProgress = totalProgress;
        if (!stage.isCompleted && !stage.isLocked)
        {
            // Добавляем частичный прогресс для текущего этапа
            combinedProgress += (0.5f / assemblyManager.AssemblyStages.Count);
        }

        return combinedProgress;
    }

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

    void RefreshProgress()
    {
        if (assemblyManager == null) return;

        int currentCompleted = GetCompletedStagesCount();
        int currentStageIndex = assemblyManager.CurrentStageIndex;

        // Обновляем только если есть изменения
        bool needsUpdate = currentCompleted != lastCompletedStages ||
                          currentStageIndex != lastStageIndex;

        if (needsUpdate)
        {
            var currentStage = assemblyManager.GetCurrentStage();
            if (currentStage != null)
            {
                float progress = CalculateProgress(currentStageIndex, currentStage);

                if (progressSlider != null)
                    progressSlider.value = progress;

                if (progressText != null)
                    progressText.text = $"Прогресс: {progress * 100:F0}%";

                lastCompletedStages = currentCompleted;
                lastStageIndex = currentStageIndex;
            }
        }
    }

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

    // Метод для принудительного обновления UI
    public void RefreshUI()
    {
        RefreshAllUI();
    }

    // Метод для переключения на конкретный этап
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