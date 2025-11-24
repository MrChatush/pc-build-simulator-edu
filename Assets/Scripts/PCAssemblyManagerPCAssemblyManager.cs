using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;
using EasyBuildSystem.Features.Runtime.Buildings.Manager;
using EasyBuildSystem.Features.Runtime.Buildings.Part;

public class PCAssemblyManager : MonoBehaviour
{
    [Header("PC Assembly Stages System")]
    public bool enablePCAssemblyStages = true;
    
    [SerializeField] List<PCAssemblyStage> assemblyStages = new List<PCAssemblyStage>();
    public List<PCAssemblyStage> AssemblyStages { get { return assemblyStages; } }

    [SerializeField] int currentStageIndex = 0;
    public int CurrentStageIndex { get { return currentStageIndex; } }

    [Header("Stage Navigation Keys")]
    public KeyCode nextStageKey = KeyCode.N;
    public KeyCode previousStageKey = KeyCode.B;
    public KeyCode switchPartKey = KeyCode.Tab;

    [Header("Debug")]
    public bool showDebugMessages = true;

    [System.Serializable]
    public class PCAssemblyStage
    {
        public string stageName;
        [TextArea] public string stageDescription;
        public List<BuildingPart> availableParts = new List<BuildingPart>();
        public bool isCompleted = false;
        public bool isLocked = false;
    }

    [System.Serializable]
    public class StageChangedEvent : UnityEvent<int, PCAssemblyStage> { }
    public StageChangedEvent OnStageChangedEvent = new StageChangedEvent();

    [System.Serializable]
    public class StageCompletedEvent : UnityEvent<int, PCAssemblyStage> { }
    public StageCompletedEvent OnStageCompletedEvent = new StageCompletedEvent();

    private BuildingManager buildingManager;

    void Start()
    {
        buildingManager = BuildingManager.Instance;
        
        if (enablePCAssemblyStages)
        {
            InitializePCAssemblyStages();
        }
    }

    void Update()
    {
        if (!enablePCAssemblyStages || !Application.isPlaying) return;

        HandleStageNavigation();
    }

    /// <summary>
    /// Initialize the PC Assembly Stages system
    /// </summary>
    void InitializePCAssemblyStages()
    {
        if (assemblyStages.Count == 0)
        {
            CreateDefaultStages();
        }

        SetCurrentStage(currentStageIndex);
    }

    /// <summary>
    /// Create default PC assembly stages
    /// </summary>
    void CreateDefaultStages()
    {
        assemblyStages = new List<PCAssemblyStage>
        {
            new PCAssemblyStage 
            { 
                stageName = "Системный блок", 
                stageDescription = "Выберите и установите корпус системного блока",
                isLocked = false
            },
            new PCAssemblyStage 
            { 
                stageName = "Материнская плата", 
                stageDescription = "Установите материнскую плату в корпус",
                isLocked = true
            },
            new PCAssemblyStage 
            { 
                stageName = "Процессор", 
                stageDescription = "Установите процессор в сокет материнской платы",
                isLocked = true
            },
            new PCAssemblyStage 
            { 
                stageName = "Оперативная память", 
                stageDescription = "Установите модули оперативной памяти в слоты",
                isLocked = true
            },
            new PCAssemblyStage 
            { 
                stageName = "Видеокарта", 
                stageDescription = "Установите видеокарту в PCI-E слот",
                isLocked = true
            },
            new PCAssemblyStage 
            { 
                stageName = "Блок питания", 
                stageDescription = "Установите блок питания в корпус",
                isLocked = true
            },
            new PCAssemblyStage 
            { 
                stageName = "Накопители", 
                stageDescription = "Установите SSD/HDD накопители",
                isLocked = true
            },
            new PCAssemblyStage 
            { 
                stageName = "Охлаждение", 
                stageDescription = "Установите систему охлаждения процессора",
                isLocked = true
            }
        };

        if (showDebugMessages)
            Debug.Log("Созданы стандартные этапы сборки ПК");
    }

    /// <summary>
    /// Handle stage navigation input
    /// </summary>
    void HandleStageNavigation()
    {
        if (Input.GetKeyDown(nextStageKey))
        {
            CompleteCurrentStage();
        }

        if (Input.GetKeyDown(previousStageKey))
        {
            PreviousStage();
        }

        if (Input.GetKeyDown(switchPartKey))
        {
            SwitchToNextBuildingPart();
        }
    }
    /// <summary>
    /// Get progress of current stage (0 to 1)
    /// </summary>
    public float GetCurrentStageProgress()
    {
        if (assemblyStages.Count == 0 || currentStageIndex >= assemblyStages.Count)
            return 0f;

        var currentStage = assemblyStages[currentStageIndex];

        // Если есть доступные детали - считаем прогресс по установленным деталям
        if (currentStage.availableParts.Count > 0)
        {
            // Здесь нужно добавить логику подсчета установленных деталей
            // Пока возвращаем 0 если этап не завершен, 1 если завершен
            return currentStage.isCompleted ? 1f : 0f;
        }
        else
        {
            // Если нет деталей - просто проверяем завершен ли этап
            return currentStage.isCompleted ? 1f : 0f;
        }
    }

    /// <summary>
    /// Switch to the next building part in current stage
    /// </summary>
    public void SwitchToNextBuildingPart()
    {
        if (assemblyStages.Count == 0) return;

        var currentStage = assemblyStages[currentStageIndex];
        if (currentStage.availableParts.Count == 0) return;

        if (showDebugMessages)
            Debug.Log($"Доступно деталей в текущем этапе: {currentStage.availableParts.Count}");
        
        // Здесь будет логика переключения между деталями
        // Пока просто выводим информацию
        foreach (var part in currentStage.availableParts)
        {
            if (part != null)
            {
                Debug.Log($"Доступная деталь: {part.GetGeneralSettings.Name}");
            }
        }
    }

    /// <summary>
    /// Move to the next assembly stage
    /// </summary>
    public void NextStage()
    {
        if (currentStageIndex < assemblyStages.Count - 1)
        {
            SetCurrentStage(currentStageIndex + 1);
        }
        else
        {
            if (showDebugMessages)
                Debug.Log("Сборка ПК завершена! Все этапы пройдены.");
        }
    }

    /// <summary>
    /// Move to the previous assembly stage
    /// </summary>
    public void PreviousStage()
    {
        if (currentStageIndex > 0)
        {
            SetCurrentStage(currentStageIndex - 1);
        }
    }

    /// <summary>
    /// Set the current assembly stage
    /// </summary>
    public void SetCurrentStage(int stageIndex)
    {
        if (stageIndex < 0 || stageIndex >= assemblyStages.Count)
        {
            Debug.LogError("Неверный индекс этапа: " + stageIndex);
            return;
        }

        // Check if stage is locked
        if (assemblyStages[stageIndex].isLocked)
        {
            if (showDebugMessages)
                Debug.LogWarning($"Этап '{assemblyStages[stageIndex].stageName}' заблокирован! Завершите предыдущие этапы сначала.");
            return;
        }

        currentStageIndex = stageIndex;
        var currentStage = assemblyStages[currentStageIndex];

        // Update available building parts for this stage
        UpdateAvailableBuildingParts(currentStage);

        // Trigger stage changed event
        OnStageChangedEvent.Invoke(currentStageIndex, currentStage);

        if (showDebugMessages)
            Debug.Log($"Текущий этап: {currentStage.stageName} - {currentStage.stageDescription}");
    }

    /// <summary>
    /// Update available building parts for the current stage
    /// </summary>
    void UpdateAvailableBuildingParts(PCAssemblyStage stage)
    {
        if (buildingManager == null) return;

        // Clear current building parts in BuildingManager
        buildingManager.BuildingPartReferences.Clear();

        // Add parts available for this stage
        if (stage.availableParts.Count > 0)
        {
            buildingManager.BuildingPartReferences.AddRange(stage.availableParts);
        }

        if (showDebugMessages)
            Debug.Log($"Этап '{stage.stageName}' имеет {stage.availableParts.Count} доступных деталей");
    }

    /// <summary>
    /// Complete the current stage and unlock the next one
    /// </summary>
    public void CompleteCurrentStage()
    {
        if (currentStageIndex < assemblyStages.Count)
        {
            assemblyStages[currentStageIndex].isCompleted = true;
            
            // Unlock next stage if exists
            if (currentStageIndex < assemblyStages.Count - 1)
            {
                assemblyStages[currentStageIndex + 1].isLocked = false;
                
                if (showDebugMessages)
                    Debug.Log($"Этап {currentStageIndex + 1} разблокирован: {assemblyStages[currentStageIndex + 1].stageName}");
            }

            OnStageCompletedEvent.Invoke(currentStageIndex, assemblyStages[currentStageIndex]);
            
            if (showDebugMessages)
                Debug.Log($"Этап '{assemblyStages[currentStageIndex].stageName}' завершен!");
            
            // Auto-advance to next stage
            NextStage();
        }
    }

    /// <summary>
    /// Get the current assembly stage
    /// </summary>
    public PCAssemblyStage GetCurrentStage()
    {
        if (currentStageIndex < assemblyStages.Count)
        {
            return assemblyStages[currentStageIndex];
        }
        return null;
    }

    /// <summary>
    /// Check if all assembly stages are completed
    /// </summary>
    public bool IsAssemblyComplete()
    {
        return assemblyStages.All(stage => stage.isCompleted);
    }

    /// <summary>
    /// Manually unlock a specific stage
    /// </summary>
    public void UnlockStage(int stageIndex)
    {
        if (stageIndex >= 0 && stageIndex < assemblyStages.Count)
        {
            assemblyStages[stageIndex].isLocked = false;
            
            if (showDebugMessages)
                Debug.Log($"Этап {stageIndex} разблокирован: {assemblyStages[stageIndex].stageName}");
        }
    }

    /// <summary>
    /// Add a part to a specific stage
    /// </summary>
    public void AddPartToStage(int stageIndex, BuildingPart part)
    {
        if (stageIndex >= 0 && stageIndex < assemblyStages.Count && part != null)
        {
            assemblyStages[stageIndex].availableParts.Add(part);
            
            if (showDebugMessages)
                Debug.Log($"Деталь '{part.GetGeneralSettings.Name}' добавлена в этап {stageIndex}");
        }
    }

    /// <summary>
    /// Get progress of assembly (0 to 1)
    /// </summary>
    public float GetAssemblyProgress()
    {
        if (assemblyStages.Count == 0) return 0f;
        
        int completedStages = assemblyStages.Count(stage => stage.isCompleted);
        return (float)completedStages / assemblyStages.Count;
    }

    /// <summary>
    /// Reset all stages to initial state
    /// </summary>
    public void ResetStages()
    {
        foreach (var stage in assemblyStages)
        {
            stage.isCompleted = false;
            stage.isLocked = true;
        }
        
        // Unlock first stage
        if (assemblyStages.Count > 0)
        {
            assemblyStages[0].isLocked = false;
        }
        
        currentStageIndex = 0;
        SetCurrentStage(0);
        
        if (showDebugMessages)
            Debug.Log("Все этапы сброшены");
    }
}