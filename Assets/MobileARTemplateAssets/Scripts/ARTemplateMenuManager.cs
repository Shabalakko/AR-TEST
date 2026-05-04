using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;
using TMPro;

namespace UnityEngine.XR.Templates.AR
{
    /// <summary>
    /// Handles dismissing the object menu when clicking out the UI bounds, and showing the
    /// menu again when the create menu button is clicked after dismissal. Manages object deletion in the AR demo scene,
    /// and also handles the toggling between the object creation menu button and the delete button.
    /// </summary>
    public class ARTemplateMenuManager : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Button that opens the create menu.")]
        Button m_CreateButton;

        public Button createButton
        {
            get => m_CreateButton;
            set => m_CreateButton = value;
        }

        [SerializeField]
        [Tooltip("Button that opens the checklist for a selected object.")]
        Button m_ChecklistButton;

        public Button checklistButton
        {
            get => m_ChecklistButton;
            set => m_ChecklistButton = value;
        }

        [SerializeField]
        [Tooltip("The panel containing the checklist UI.")]
        GameObject m_ChecklistPanel;

        public GameObject checklistPanel
        {
            get => m_ChecklistPanel;
            set => m_ChecklistPanel = value;
        }

        [SerializeField]
        [Tooltip("Button that deletes a selected object.")]
        Button m_DeleteButton;

        public Button deleteButton
        {
            get => m_DeleteButton;
            set => m_DeleteButton = value;
        }

        [SerializeField]
        [Tooltip("The menu with all the creatable objects.")]
        GameObject m_ObjectMenu;

        public GameObject objectMenu
        {
            get => m_ObjectMenu;
            set => m_ObjectMenu = value;
        }

        [SerializeField]
        [Tooltip("The modal with debug options.")]
        GameObject m_ModalMenu;

        public GameObject modalMenu
        {
            get => m_ModalMenu;
            set => m_ModalMenu = value;
        }

        [SerializeField]
        [Tooltip("The animator for the object creation menu.")]
        Animator m_ObjectMenuAnimator;

        public Animator objectMenuAnimator
        {
            get => m_ObjectMenuAnimator;
            set => m_ObjectMenuAnimator = value;
        }

        [Header("Dynamic Menu Settings")]
        [SerializeField]
        [Tooltip("The prefab for the machine to be spawned.")]
        GameObject m_MachinePrefab;

        [SerializeField]
        [Tooltip("The prefab for the buttons in the menu.")]
        GameObject m_MenuButtonPrefab;

        [SerializeField]
        [Tooltip("The container for the menu buttons.")]
        Transform m_MenuListContent;

        [SerializeField]
        [Tooltip("The text to display on machine buttons.")]
        string m_MachineButtonLabel = "Macchina";

        [SerializeField]
        [Tooltip("The object spawner component in charge of spawning new objects.")]
        ObjectSpawner m_ObjectSpawner;

        public ObjectSpawner objectSpawner
        {
            get => m_ObjectSpawner;
            set => m_ObjectSpawner = value;
        }

        [SerializeField]
        [Tooltip("Button that closes the object creation menu.")]
        Button m_CancelButton;

        public Button cancelButton
        {
            get => m_CancelButton;
            set => m_CancelButton = value;
        }

        [SerializeField]
        [Tooltip("The interaction group for the AR demo scene.")]
        XRInteractionGroup m_InteractionGroup;

        public XRInteractionGroup interactionGroup
        {
            get => m_InteractionGroup;
            set => m_InteractionGroup = value;
        }

        [SerializeField]
        [Tooltip("The slider for activating plane debug visuals.")]
        DebugSlider m_DebugPlaneSlider;

        public DebugSlider debugPlaneSlider
        {
            get => m_DebugPlaneSlider;
            set => m_DebugPlaneSlider = value;
        }

        [SerializeField]
        [Tooltip("The plane manager in the AR demo scene.")]
        ARPlaneManager m_PlaneManager;

        public ARPlaneManager planeManager
        {
            get => m_PlaneManager;
            set => m_PlaneManager = value;
        }

        [SerializeField]
        [Tooltip("Determines whether or not to fade the AR Planes when visualization is toggled.")]
        bool m_UseARPlaneFading = true;

        public bool useARPlaneFading
        {
            get => m_UseARPlaneFading;
            set => m_UseARPlaneFading = value;
        }

        [SerializeField]
        [Tooltip("The AR debug menu.")]
        ARDebugMenu m_ARDebugMenu;

        public ARDebugMenu arDebugMenu
        {
            get => m_ARDebugMenu;
            set => m_ARDebugMenu = value;
        }

        [SerializeField]
        [Tooltip("The report manager component for the checklist data.")]
        ReportManager m_ReportManager;

        public ReportManager reportManager
        {
            get => m_ReportManager;
            set => m_ReportManager = value;
        }

        [SerializeField]
        [Tooltip("The slider for activating the debug menu.")]
        DebugSlider m_DebugMenuSlider;

        public DebugSlider debugMenuSlider
        {
            get => m_DebugMenuSlider;
            set => m_DebugMenuSlider = value;
        }

        [SerializeField]
        XRInputValueReader<Vector2> m_TapStartPositionInput = new XRInputValueReader<Vector2>("Tap Start Position");

        public XRInputValueReader<Vector2> tapStartPositionInput
        {
            get => m_TapStartPositionInput;
            set => XRInputReaderUtility.SetInputProperty(ref m_TapStartPositionInput, value, this);
        }

        [SerializeField]
        XRInputValueReader<Vector2> m_DragCurrentPositionInput = new XRInputValueReader<Vector2>("Drag Current Position");

        public XRInputValueReader<Vector2> dragCurrentPositionInput
        {
            get => m_DragCurrentPositionInput;
            set => XRInputReaderUtility.SetInputProperty(ref m_DragCurrentPositionInput, value, this);
        }

        bool m_IsPointerOverUI;
        bool m_ShowObjectMenu;
        bool m_ShowOptionsModal;
        bool m_VisualizePlanes = true;
        bool m_ShowDebugMenu;
        bool m_InitializingDebugMenu;
        bool m_DynamicMenuInitialized;
        float m_DebugMenuPlanesButtonValue = 0f;
        Vector2 m_ObjectButtonOffset = Vector2.zero;
        Vector2 m_ObjectMenuOffset = Vector2.zero;
        readonly List<ARPlane> m_ARPlanes = new List<ARPlane>();
        readonly Dictionary<ARPlane, ARPlaneMeshVisualizer> m_ARPlaneMeshVisualizers = new Dictionary<ARPlane, ARPlaneMeshVisualizer>();
        readonly Dictionary<ARPlane, ARPlaneMeshVisualizerFader> m_ARPlaneMeshVisualizerFaders = new Dictionary<ARPlane, ARPlaneMeshVisualizerFader>();
        readonly List<GameObject> m_SpawnedMachineInstances = new List<GameObject>();
        readonly List<Button> m_MachineButtons = new List<Button>();
        readonly List<GameObject> m_MachineUIEntries = new List<GameObject>();
        readonly List<string> m_MachineCustomNames = new List<string>();

        TouchScreenKeyboard m_Keyboard;
        int m_RenamingIndex = -1;
        GameObject m_LastFocusedObject;

        /// <summary>
        /// Awake runs BEFORE the first frame is rendered. We disable the object menu
        /// immediately so the user never sees the original template menu (with its
        /// static shapes) on Android, where Start() may take longer to reach the
        /// dynamic menu initialization step.
        /// </summary>
        void Awake()
        {
            if (m_ObjectMenu != null)
                m_ObjectMenu.SetActive(false);
        }

        void OnEnable()
        {
            if (m_CreateButton != null) m_CreateButton.onClick.AddListener(ShowMenu);
            if (m_CancelButton != null) m_CancelButton.onClick.AddListener(HideMenu);
            if (m_DeleteButton != null) m_DeleteButton.onClick.AddListener(DeleteFocusedObject);
            if (m_ChecklistButton != null) m_ChecklistButton.onClick.AddListener(ToggleChecklist);
            if (m_PlaneManager != null) m_PlaneManager.trackablesChanged.AddListener(OnPlaneChanged);
            if (m_ObjectSpawner != null)
                m_ObjectSpawner.objectSpawned += HandleObjectSpawned;
        }

        void OnDisable()
        {
            m_ShowObjectMenu = false;
            if (m_CreateButton != null) m_CreateButton.onClick.RemoveListener(ShowMenu);
            if (m_CancelButton != null) m_CancelButton.onClick.RemoveListener(HideMenu);
            if (m_DeleteButton != null) m_DeleteButton.onClick.RemoveListener(DeleteFocusedObject);
            if (m_ChecklistButton != null) m_ChecklistButton.onClick.RemoveListener(ToggleChecklist);
            if (m_PlaneManager != null) m_PlaneManager.trackablesChanged.RemoveListener(OnPlaneChanged);
            if (m_ObjectSpawner != null)
                m_ObjectSpawner.objectSpawned -= HandleObjectSpawned;
        }

        IEnumerator Start()
        {
            // Pulisce Selectable, Animator e Animation con configurazioni rotte.
           

            // Pulisce SUBITO i prefab dello spawner del template (le forme di default).
            // Questo evita che, se qualcun altro li legge prima della nostra
            // InitializeDynamicMenu, mostri la lista vecchia.
            if (m_ObjectSpawner != null)
            {
                m_ObjectSpawner.objectPrefabs.Clear();
                m_ObjectSpawner.spawnEnabled = false;
            }

            // Auto turn on/off debug menu.
            if (m_ARDebugMenu != null)
            {
                m_ARDebugMenu.gameObject.SetActive(true);
                m_InitializingDebugMenu = true;
                InitializeDebugMenuOffsets();
            }

            // Aspetta qualche frame perché su Android l'inizializzazione UI è lenta.
            // Frame-based è più affidabile di WaitForSeconds (non dipende dal framerate).
            yield return null;
            yield return null;
            yield return new WaitForEndOfFrame();
            yield return null;

            // Per essere sicuri che il menu sia attivo durante l'inizializzazione
            // (così GetComponentInChildren su ScrollRect funzioni correttamente),
            // lo riattivo TEMPORANEAMENTE prima di operare sui figli.
            bool wasActive = m_ObjectMenu != null && m_ObjectMenu.activeSelf;
            if (m_ObjectMenu != null && !wasActive)
                m_ObjectMenu.SetActive(true);

            InitializeDynamicMenu();


            if (m_DebugMenuSlider != null) m_DebugMenuSlider.value = m_ShowDebugMenu ? 1 : 0;
            if (m_DebugPlaneSlider != null) m_DebugPlaneSlider.value = m_VisualizePlanes ? 1 : 0;

            m_DynamicMenuInitialized = true;
        }

        /// <summary>
        /// Pulisce Selectable, Animator e Animation con configurazioni rotte.
        /// Eseguito una sola volta in Start.
        /// </summary>
        void PerformGlobalSanitization()
        {
            foreach (var selectable in FindObjectsOfType<Selectable>(true))
            {
                if (selectable.transition == Selectable.Transition.Animation)
                {
                    var anim = selectable.GetComponent<Animator>();
                    if (anim == null || anim.runtimeAnimatorController == null)
                    {
                        selectable.transition = Selectable.Transition.ColorTint;
                        if (anim != null)
                        {
                            anim.enabled = false;
                            DestroyImmediate(anim);
                        }
                    }
                }
            }

            foreach (var anim in FindObjectsOfType<Animator>(true))
            {
                if (anim != null && anim.runtimeAnimatorController == null)
                {
                    anim.enabled = false;
                    DestroyImmediate(anim);
                }
            }

            foreach (var anim in FindObjectsOfType<Animation>(true))
            {
                if (anim != null && anim.clip == null && anim.GetClipCount() == 0)
                {
                    anim.enabled = false;
                    DestroyImmediate(anim);
                }
            }
        }

        void InitializeDynamicMenu()
{
    if (m_MenuListContent == null && m_ObjectMenu != null)
    {
        var scrollRect = m_ObjectMenu.GetComponentInChildren<ScrollRect>(true);
        if (scrollRect != null)
            m_MenuListContent = scrollRect.content;
    }

    if (m_MenuListContent == null)
    {
        Debug.LogError("[ARTemplateMenuManager] Menu List Content not found.");
        return;
    }

    // Distruggi figli
    for (int i = m_MenuListContent.childCount - 1; i >= 0; i--)
    {
        Transform child = m_MenuListContent.GetChild(i);
        if (child != null)
            Destroy(child.gameObject);
    }

    // Reset dati
    if (m_ObjectSpawner != null)
        m_ObjectSpawner.objectPrefabs.Clear();

    m_SpawnedMachineInstances.Clear();
    m_MachineButtons.Clear();
    m_MachineUIEntries.Clear();
    m_MachineCustomNames.Clear();

    // IMPORTANTE: NON creare subito il bottone
    StartCoroutine(RebuildMenuNextFrame());
}
        IEnumerator RebuildMenuNextFrame()
        {
            // assicurati che sia attivo
            if (m_ObjectMenu != null && !m_ObjectMenu.activeSelf)
                m_ObjectMenu.SetActive(true);

            yield return null;

            CreateAddButton();

            if (m_MenuListContent.childCount == 0)
            {
                Debug.LogError("[ARTemplateMenuManager] AddButton non creato!");
                yield break;
            }

            LoadMachinesData();

            if (m_MenuListContent is RectTransform rt)
                LayoutRebuilder.ForceRebuildLayoutImmediate(rt);

            foreach (var selectable in m_MenuListContent.GetComponentsInChildren<Selectable>(true))
                SanitizeSelectable(selectable);

            // 🔵 opzionale: lo richiudi dopo aver creato tutto
            yield return null;

            HideMenu();
        }

        void SaveMachinesData()
        {
            PlayerPrefs.SetInt("AR_MachineCount", m_MachineCustomNames.Count);
            for (int i = 0; i < m_MachineCustomNames.Count; i++)
            {
                PlayerPrefs.SetString($"AR_MachineName_{i}", m_MachineCustomNames[i]);
            }
            PlayerPrefs.Save();
        }

        void LoadMachinesData()
        {
            if (PlayerPrefs.HasKey("AR_MachineCount"))
            {
                int count = PlayerPrefs.GetInt("AR_MachineCount");
                for (int i = 0; i < count; i++)
                {
                    string name = PlayerPrefs.GetString($"AR_MachineName_{i}", $"{m_MachineButtonLabel} {i + 1}");
                    AddMachineInternal(name);
                }
            }
        }
        void CreateAddButton()
        {
            if (m_MenuButtonPrefab == null)
            {
                Debug.LogError("[ARTemplateMenuManager] m_MenuButtonPrefab not assigned!");
                return;
            }
            if (m_MenuListContent == null)
            {
                Debug.LogError("[ARTemplateMenuManager] m_MenuListContent is null!");
                return;
            }

            GameObject go = Instantiate(m_MenuButtonPrefab, m_MenuListContent);
            go.name = "AddMachineButton";
            Button btn = go.GetComponentInChildren<Button>();
            if (btn != null)
            {
                SanitizeSelectable(btn);
                btn.onClick.AddListener(AddMachineEntry);

                var text = go.GetComponentInChildren<TMP_Text>();
                if (text != null) text.text = "+";

                var deleteBtn = go.transform.Find("DeleteButton")?.GetComponent<Button>();
                if (deleteBtn != null) deleteBtn.gameObject.SetActive(false);

                var renameBtn = go.transform.Find("RenameButton")?.GetComponent<Button>();
                if (renameBtn != null) renameBtn.gameObject.SetActive(false);
            }
            else
            {
                Debug.LogError("[ARTemplateMenuManager] m_MenuButtonPrefab non contiene un Button!");
            }
        }

        public void AddMachineEntry()
        {
            AddMachineInternal(null);
        }

        void AddMachineInternal(string loadedName)
        {
            if (m_MachinePrefab == null || m_MenuButtonPrefab == null || m_MenuListContent == null || m_ObjectSpawner == null)
            {
                Debug.LogWarning("[ARTemplateMenuManager] Missing references for AddMachineEntry.");
                return;
            }

            int index = m_ObjectSpawner.objectPrefabs.Count;
            m_ObjectSpawner.objectPrefabs.Add(m_MachinePrefab);

            GameObject go = Instantiate(m_MenuButtonPrefab, m_MenuListContent);

            MoveAddButtonToEnd();

            m_SpawnedMachineInstances.Add(null);

            string finalName = string.IsNullOrEmpty(loadedName) ? $"{m_MachineButtonLabel} {index + 1}" : loadedName;
            m_MachineCustomNames.Add(finalName);

            if (string.IsNullOrEmpty(loadedName))
            {
                SaveMachinesData();
            }

            Button machineBtn = go.GetComponentInChildren<Button>();
            if (machineBtn != null)
            {
                SanitizeSelectable(machineBtn);
                m_MachineButtons.Add(machineBtn);
                m_MachineUIEntries.Add(go);
                machineBtn.onClick.AddListener(() => SetObjectToSpawn(index));

                var text = go.GetComponentInChildren<TMP_Text>();
                if (text != null) text.text = m_MachineCustomNames[index];

                var deleteBtn = go.transform.Find("DeleteButton")?.GetComponent<Button>();
                if (deleteBtn != null)
                {
                    deleteBtn.gameObject.SetActive(true);
                    deleteBtn.onClick.AddListener(() => RemoveMachineEntry(machineBtn));
                }

                var renameBtn = go.transform.Find("RenameButton")?.GetComponent<Button>();
                if (renameBtn != null)
                {
                    renameBtn.gameObject.SetActive(true);
                    renameBtn.onClick.AddListener(() => StartRenameMachine(machineBtn));
                }

                if (m_MenuListContent is RectTransform rt)
                    LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
            }
        }

        public void StartRenameMachine(Button machineBtn)
        {
            int index = m_MachineButtons.IndexOf(machineBtn);
            if (index < 0) return;

            m_RenamingIndex = index;

            if (TouchScreenKeyboard.isSupported)
            {
                TouchScreenKeyboard.hideInput = false;
                m_Keyboard = TouchScreenKeyboard.Open(m_MachineCustomNames[index], TouchScreenKeyboardType.Default, true, false, false, false, "Inserisci nome macchina...");
            }
            else
            {
                m_MachineCustomNames[index] = $"{m_MachineCustomNames[index]} (Rinominata)";
                RefreshMachineButtons();
                SaveMachinesData();
                m_RenamingIndex = -1;
            }
        }

        public void RemoveMachineEntry(Button machineBtn)
        {
            int index = m_MachineButtons.IndexOf(machineBtn);
            if (index < 0) return;

            if (m_SpawnedMachineInstances[index] != null)
                Destroy(m_SpawnedMachineInstances[index]);

            GameObject uiEntry = m_MachineUIEntries[index];
            m_ObjectSpawner.objectPrefabs.RemoveAt(index);
            m_SpawnedMachineInstances.RemoveAt(index);
            m_MachineButtons.RemoveAt(index);
            m_MachineUIEntries.RemoveAt(index);
            m_MachineCustomNames.RemoveAt(index);

            if (m_ReportManager == null) m_ReportManager = Object.FindAnyObjectByType<ReportManager>();
            if (m_ReportManager != null)
            {
                string deletedId = $"Macchina_{index + 1}";
                m_ReportManager.DeleteDataForMachine(deletedId);

                for (int i = index; i < m_MachineButtons.Count; i++)
                {
                    string oldId = $"Macchina_{i + 2}";
                    string newId = $"Macchina_{i + 1}";
                    m_ReportManager.RenameDataForMachine(oldId, newId);
                }
            }

            Destroy(uiEntry);

            RefreshMachineButtons();

            if (m_ObjectSpawner.spawnOptionIndex == index)
            {
                m_ObjectSpawner.spawnEnabled = false;
                m_ObjectSpawner.spawnOptionIndex = -1;
            }
            else if (m_ObjectSpawner.spawnOptionIndex > index)
            {
                m_ObjectSpawner.spawnOptionIndex--;
            }

            if (m_MenuListContent is RectTransform rt)
                LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
                
            SaveMachinesData();
        }

        void RefreshMachineButtons()
        {
            for (int i = 0; i < m_MachineButtons.Count; i++)
            {
                int index = i;
                var btn = m_MachineButtons[i];

                var text = btn.GetComponentInChildren<TMP_Text>();
                if (text != null) text.text = m_MachineCustomNames[i];

                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => SetObjectToSpawn(index));

                var deleteBtn = m_MachineUIEntries[i].transform.Find("DeleteButton")?.GetComponent<Button>();
                if (deleteBtn != null)
                {
                    deleteBtn.onClick.RemoveAllListeners();
                    deleteBtn.onClick.AddListener(() => RemoveMachineEntry(btn));
                }

                var renameBtn = m_MachineUIEntries[i].transform.Find("RenameButton")?.GetComponent<Button>();
                if (renameBtn != null)
                {
                    renameBtn.onClick.RemoveAllListeners();
                    renameBtn.onClick.AddListener(() => StartRenameMachine(btn));
                }
            }
        }

        void MoveAddButtonToEnd()
        {
            if (m_MenuListContent == null) return;

            foreach (Transform child in m_MenuListContent)
            {
                if (child.name == "AddMachineButton")
                {
                    child.SetAsLastSibling();
                    return;
                }

                var text = child.GetComponentInChildren<TMP_Text>();
                if (text != null && text.text == "+")
                {
                    child.SetAsLastSibling();
                    return;
                }
            }
        }

        void Update()
        {
            // Mobile keyboard handling
            if (m_Keyboard != null && TouchScreenKeyboard.isSupported)
            {
                try
                {
                    // Aggiornamento in tempo reale del testo sul bottone
                    if (m_RenamingIndex >= 0 && m_RenamingIndex < m_MachineCustomNames.Count)
                    {
                        if (m_MachineCustomNames[m_RenamingIndex] != m_Keyboard.text)
                        {
                            m_MachineCustomNames[m_RenamingIndex] = m_Keyboard.text;
                            RefreshMachineButtons();
                        }
                    }

                    if (m_Keyboard.status == TouchScreenKeyboard.Status.Done)
                    {
                        if (m_RenamingIndex >= 0 && m_RenamingIndex < m_MachineCustomNames.Count)
                        {
                            if (string.IsNullOrEmpty(m_Keyboard.text))
                                m_MachineCustomNames[m_RenamingIndex] = $"{m_MachineButtonLabel} {m_RenamingIndex + 1}"; // Fallback se vuoto
                            else
                                m_MachineCustomNames[m_RenamingIndex] = m_Keyboard.text;
                                
                            RefreshMachineButtons();
                            SaveMachinesData();
                        }
                        m_Keyboard = null;
                        m_RenamingIndex = -1;
                    }
                    else if (m_Keyboard.status == TouchScreenKeyboard.Status.Canceled)
                    {
                        // In caso di annullamento potremmo ripristinare il vecchio nome,
                        // ma se abbiamo fatto l'aggiornamento in tempo reale, teniamo quello inserito
                        // o lo salviamo. Facciamo il salvataggio comunque.
                        if (m_RenamingIndex >= 0 && m_RenamingIndex < m_MachineCustomNames.Count)
                        {
                            if (string.IsNullOrEmpty(m_Keyboard.text))
                                m_MachineCustomNames[m_RenamingIndex] = $"{m_MachineButtonLabel} {m_RenamingIndex + 1}";
                            RefreshMachineButtons();
                            SaveMachinesData();
                        }
                        m_Keyboard = null;
                        m_RenamingIndex = -1;
                    }
                }
                catch
                {
                    m_Keyboard = null;
                    m_RenamingIndex = -1;
                }
            }

            if (m_InitializingDebugMenu)
            {
                m_ARDebugMenu.gameObject.SetActive(false);
                m_InitializingDebugMenu = false;
            }

            // Update m_IsPointerOverUI at the START of the frame
            m_IsPointerOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(-1);

            if (m_ShowObjectMenu || m_ShowOptionsModal)
            {
                if (!m_IsPointerOverUI && (m_TapStartPositionInput.TryReadValue(out _) || m_DragCurrentPositionInput.TryReadValue(out _)))
                {
                    if (m_ShowObjectMenu)
                        HideMenu();

                    if (m_ShowOptionsModal && m_ModalMenu != null)
                        m_ModalMenu.SetActive(false);
                }

                if (m_ShowObjectMenu)
                {
                    if (m_DeleteButton != null) m_DeleteButton.gameObject.SetActive(false);
                    if (m_ChecklistButton != null) m_ChecklistButton.gameObject.SetActive(false);
                }
                else
                {
                    var hasFocus = m_InteractionGroup?.focusInteractable != null;
                    if (m_DeleteButton != null) m_DeleteButton.gameObject.SetActive(hasFocus);
                    if (m_ChecklistButton != null) m_ChecklistButton.gameObject.SetActive(hasFocus);

                    if (!hasFocus && m_ChecklistPanel != null && m_ChecklistPanel.activeSelf)
                        m_ChecklistPanel.SetActive(false);
                }
            }
            else
            {
                if (m_CreateButton != null) m_CreateButton.gameObject.SetActive(true);
                var hasFocus = m_InteractionGroup?.focusInteractable != null;
                if (m_DeleteButton != null) m_DeleteButton.gameObject.SetActive(hasFocus);
                if (m_ChecklistButton != null) m_ChecklistButton.gameObject.SetActive(hasFocus);

                if (!hasFocus && m_ChecklistPanel != null && m_ChecklistPanel.activeSelf)
                    m_ChecklistPanel.SetActive(false);
            }

            for (int i = 0; i < m_SpawnedMachineInstances.Count; i++)
            {
                if (m_SpawnedMachineInstances[i] == null && !m_MachineButtons[i].interactable)
                    m_MachineButtons[i].interactable = true;
            }

            // --- Gestione globale Highlight Macchina Selezionata ---
            GameObject currentFocused = null;
            if (m_InteractionGroup != null && m_InteractionGroup.focusInteractable != null)
            {
                currentFocused = m_InteractionGroup.focusInteractable.transform.gameObject;
            }

            // Se l'oggetto focalizzato è cambiato rispetto all'ultimo frame
            if (m_LastFocusedObject != currentFocused)
            {
                // Spegni il vecchio (controllando che non sia stato distrutto)
                if (m_LastFocusedObject != null)
                {
                    var oldHighlighter = m_LastFocusedObject.GetComponent<MachineHighlighter>();
                    if (oldHighlighter != null) oldHighlighter.TurnOffHighlight();
                }

                // Accendi il nuovo
                if (currentFocused != null)
                {
                    var newHighlighter = currentFocused.GetComponent<MachineHighlighter>();
                    if (newHighlighter != null) newHighlighter.TurnOnHighlight();
                }

                m_LastFocusedObject = currentFocused;
            }
        }

        public void SetObjectToSpawn(int objectIndex)
        {
            if (m_ObjectSpawner == null)
            {
                Debug.LogWarning("[ARTemplateMenuManager] ObjectSpawner not configured.");
            }
            else if (m_ObjectSpawner.objectPrefabs.Count > objectIndex)
            {
                m_ObjectSpawner.spawnOptionIndex = objectIndex;
                m_ObjectSpawner.spawnEnabled = true;
            }
            else
            {
                Debug.LogWarning("[ARTemplateMenuManager] Object index out of range.");
            }

            HideMenu();
        }

        void ShowMenu()
        {
            // Su Android, se l'utente preme Create prima che Start abbia finito
            // l'inizializzazione, blocchiamo l'apertura per evitare di mostrare un menu vecchio.
            if (!m_DynamicMenuInitialized)
            {
                Debug.LogWarning("[ARTemplateMenuManager] ShowMenu chiamato prima dell'inizializzazione. Ignoro.");
                return;
            }

            m_ShowObjectMenu = true;
            if (m_ObjectMenu != null)
                m_ObjectMenu.SetActive(true);

            if (m_ObjectMenuAnimator != null && !m_ObjectMenuAnimator.GetBool("Show"))
                m_ObjectMenuAnimator.SetBool("Show", true);

            AdjustARDebugMenuPosition();
        }

        public void ShowHideModal()
        {
            if (m_ModalMenu == null) return;

            if (m_ModalMenu.activeSelf)
            {
                m_ShowOptionsModal = false;
                m_ModalMenu.SetActive(false);
            }
            else
            {
                m_ShowOptionsModal = true;
                m_ModalMenu.SetActive(true);
            }
        }

        public void ShowHideDebugPlane()
        {
            m_VisualizePlanes = !m_VisualizePlanes;
            if (m_DebugPlaneSlider != null) m_DebugPlaneSlider.value = m_VisualizePlanes ? 1 : 0;
            ChangePlaneVisibility(m_VisualizePlanes);
        }

        public void ShowHideDebugMenu()
        {
            m_ShowDebugMenu = !m_ShowDebugMenu;
            if (m_DebugMenuSlider != null) m_DebugMenuSlider.value = m_ShowDebugMenu ? 1 : 0;

            if (m_ShowDebugMenu)
            {
                m_ARDebugMenu.gameObject.SetActive(true);
                AdjustARDebugMenuPosition();
                if (m_ARDebugMenu.showPlanesButton.value != m_DebugMenuPlanesButtonValue)
                    m_ARDebugMenu.showPlanesButton.value = m_DebugMenuPlanesButtonValue;
            }
            else
            {
                m_DebugMenuPlanesButtonValue = m_ARDebugMenu.showPlanesButton.value;
                if (m_DebugMenuPlanesButtonValue == 1f)
                    m_ARDebugMenu.showPlanesButton.value = 0f;

                m_ARDebugMenu.gameObject.SetActive(false);
            }
        }

        public void ClearAllObjects()
        {
            if (m_ObjectSpawner == null) return;

            foreach (Transform child in m_ObjectSpawner.transform)
                Destroy(child.gameObject);

            for (int i = 0; i < m_SpawnedMachineInstances.Count; i++)
            {
                m_SpawnedMachineInstances[i] = null;
                m_MachineButtons[i].interactable = true;
            }

            if (m_SpawnedMachineInstances.Count > 0)
            {
                m_ObjectSpawner.spawnOptionIndex = 0;
                m_ObjectSpawner.spawnEnabled = true;
            }
            else
            {
                m_ObjectSpawner.spawnEnabled = false;
            }
        }

        public void HideMenu()
        {
            if (m_ObjectMenuAnimator != null && m_ObjectMenuAnimator.runtimeAnimatorController != null)
            {
                m_ObjectMenuAnimator.SetBool("Show", false);
            }
            else
            {
                if (m_ObjectMenu != null)
                    m_ObjectMenu.SetActive(false);
            }

            m_ShowObjectMenu = false;
            AdjustARDebugMenuPosition();
        }

        public void ToggleChecklist()
        {
            if (m_ChecklistPanel == null) return;

            bool willBeActive = !m_ChecklistPanel.activeSelf;

            if (willBeActive && m_InteractionGroup != null && m_InteractionGroup.focusInteractable != null)
            {
                GameObject focusedObj = m_InteractionGroup.focusInteractable.transform.gameObject;
                int index = m_SpawnedMachineInstances.IndexOf(focusedObj);

                if (index >= 0)
                {
                    string machineId = $"Macchina_{index + 1}";
                    if (m_ReportManager == null)
                        m_ReportManager = Object.FindAnyObjectByType<ReportManager>();

                    if (m_ReportManager != null)
                        m_ReportManager.InitializeForMachine(machineId);
                    else
                        Debug.LogWarning("[ARTemplateMenuManager] ReportManager not found.");
                }
            }

            m_ChecklistPanel.SetActive(willBeActive);
        }

        void ChangePlaneVisibility(bool setVisible)
        {
            foreach (var plane in m_ARPlanes)
            {
                if (m_ARPlaneMeshVisualizers.TryGetValue(plane, out var visualizer))
                    visualizer.enabled = m_UseARPlaneFading ? true : setVisible;

                if (m_ARPlaneMeshVisualizerFaders.TryGetValue(plane, out var fader))
                {
                    if (m_UseARPlaneFading)
                        fader.visualizeSurfaces = setVisible;
                    else
                        fader.SetVisualsImmediate(1f);
                }
            }
        }

        void DeleteFocusedObject()
        {
            var currentFocusedObject = m_InteractionGroup.focusInteractable;
            if (currentFocusedObject == null) return;

            GameObject obj = currentFocusedObject.transform.gameObject;

            int index = m_SpawnedMachineInstances.IndexOf(obj);

            Destroy(obj);

            if (index >= 0)
            {
                m_SpawnedMachineInstances[index] = null;
                m_MachineButtons[index].interactable = true;

                SetObjectToSpawn(index);
            }
        }

        void InitializeDebugMenuOffsets()
        {
            if (m_CreateButton.TryGetComponent<RectTransform>(out var buttonRect))
                m_ObjectButtonOffset = new Vector2(0f, buttonRect.anchoredPosition.y + buttonRect.rect.height + 10f);
            else
                m_ObjectButtonOffset = new Vector2(0f, 200f);

            if (m_ObjectMenu.TryGetComponent<RectTransform>(out var menuRect))
                m_ObjectMenuOffset = new Vector2(0f, menuRect.anchoredPosition.y + menuRect.rect.height + 10f);
            else
                m_ObjectMenuOffset = new Vector2(0f, 345f);
        }

        void AdjustARDebugMenuPosition()
        {
            if (m_ARDebugMenu == null)
                return;

            float screenWidthInInches = Screen.width / Screen.dpi;

            if (screenWidthInInches < 5)
            {
                Vector2 menuOffset = m_ShowObjectMenu ? m_ObjectMenuOffset : m_ObjectButtonOffset;

                if (m_ARDebugMenu.toolbar.TryGetComponent<RectTransform>(out var rect))
                {
                    rect.anchorMin = new Vector2(0.5f, 0);
                    rect.anchorMax = new Vector2(0.5f, 0);
                    rect.eulerAngles = new Vector3(rect.eulerAngles.x, rect.eulerAngles.y, 90);
                    rect.anchoredPosition = new Vector2(0, 20) + menuOffset;
                }

                if (m_ARDebugMenu.displayInfoMenuButton.TryGetComponent<RectTransform>(out var infoMenuButtonRect))
                    infoMenuButtonRect.localEulerAngles = new Vector3(infoMenuButtonRect.localEulerAngles.x, infoMenuButtonRect.localEulerAngles.y, -90);

                if (m_ARDebugMenu.displayConfigurationsMenuButton.TryGetComponent<RectTransform>(out var configurationsMenuButtonRect))
                    configurationsMenuButtonRect.localEulerAngles = new Vector3(configurationsMenuButtonRect.localEulerAngles.x, configurationsMenuButtonRect.localEulerAngles.y, -90);

                if (m_ARDebugMenu.displayCameraConfigurationsMenuButton.TryGetComponent<RectTransform>(out var cameraConfigurationsMenuButtonRect))
                    cameraConfigurationsMenuButtonRect.localEulerAngles = new Vector3(cameraConfigurationsMenuButtonRect.localEulerAngles.x, cameraConfigurationsMenuButtonRect.localEulerAngles.y, -90);

                if (m_ARDebugMenu.displayDebugOptionsMenuButton.TryGetComponent<RectTransform>(out var debugOptionsMenuButtonRect))
                    debugOptionsMenuButtonRect.localEulerAngles = new Vector3(debugOptionsMenuButtonRect.localEulerAngles.x, debugOptionsMenuButtonRect.localEulerAngles.y, -90);

                if (m_ARDebugMenu.infoMenu.TryGetComponent<RectTransform>(out var infoMenuRect))
                {
                    infoMenuRect.anchorMin = new Vector2(0.5f, 0);
                    infoMenuRect.anchorMax = new Vector2(0.5f, 0);
                    infoMenuRect.pivot = new Vector2(0.5f, 0);
                    infoMenuRect.anchoredPosition = new Vector2(0, 150) + menuOffset;
                }

                if (m_ARDebugMenu.configurationMenu.TryGetComponent<RectTransform>(out var configurationsMenuRect))
                {
                    configurationsMenuRect.anchorMin = new Vector2(0.5f, 0);
                    configurationsMenuRect.anchorMax = new Vector2(0.5f, 0);
                    configurationsMenuRect.pivot = new Vector2(0.5f, 0);
                    configurationsMenuRect.anchoredPosition = new Vector2(0, 150) + menuOffset;
                }

                if (m_ARDebugMenu.cameraConfigurationMenu.TryGetComponent<RectTransform>(out var cameraConfigurationsMenuRect))
                {
                    cameraConfigurationsMenuRect.anchorMin = new Vector2(0.5f, 0);
                    cameraConfigurationsMenuRect.anchorMax = new Vector2(0.5f, 0);
                    cameraConfigurationsMenuRect.pivot = new Vector2(0.5f, 0);
                    cameraConfigurationsMenuRect.anchoredPosition = new Vector2(0, 150) + menuOffset;
                }

                if (m_ARDebugMenu.debugOptionsMenu.TryGetComponent<RectTransform>(out var debugOptionsMenuRect))
                {
                    debugOptionsMenuRect.anchorMin = new Vector2(0.5f, 0);
                    debugOptionsMenuRect.anchorMax = new Vector2(0.5f, 0);
                    debugOptionsMenuRect.pivot = new Vector2(0.5f, 0);
                    debugOptionsMenuRect.anchoredPosition = new Vector2(0, 150) + menuOffset;
                }
            }
        }

        void OnPlaneChanged(ARTrackablesChangedEventArgs<ARPlane> eventArgs)
        {
            if (eventArgs.added.Count > 0)
            {
                foreach (var plane in eventArgs.added)
                {
                    m_ARPlanes.Add(plane);
                    if (plane.TryGetComponent<ARPlaneMeshVisualizer>(out var vizualizer))
                    {
                        m_ARPlaneMeshVisualizers.Add(plane, vizualizer);
                        if (!m_UseARPlaneFading)
                            vizualizer.enabled = m_VisualizePlanes;
                    }

                    if (!plane.TryGetComponent<ARPlaneMeshVisualizerFader>(out var visualizer))
                        visualizer = plane.gameObject.AddComponent<ARPlaneMeshVisualizerFader>();
                    m_ARPlaneMeshVisualizerFaders.Add(plane, visualizer);
                    visualizer.visualizeSurfaces = m_VisualizePlanes;
                }
            }

            if (eventArgs.removed.Count > 0)
            {
                foreach (var plane in eventArgs.removed)
                {
                    var planeGameObject = plane.Value;
                    if (planeGameObject == null)
                        continue;

                    if (m_ARPlanes.Contains(planeGameObject))
                        m_ARPlanes.Remove(planeGameObject);

                    if (m_ARPlaneMeshVisualizers.ContainsKey(planeGameObject))
                        m_ARPlaneMeshVisualizers.Remove(planeGameObject);

                    if (m_ARPlaneMeshVisualizerFaders.ContainsKey(planeGameObject))
                        m_ARPlaneMeshVisualizerFaders.Remove(planeGameObject);
                }
            }

            if (m_PlaneManager.trackables.count != m_ARPlanes.Count)
            {
                m_ARPlanes.Clear();
                m_ARPlaneMeshVisualizers.Clear();
                m_ARPlaneMeshVisualizerFaders.Clear();

                foreach (var plane in m_PlaneManager.trackables)
                {
                    m_ARPlanes.Add(plane);
                    if (plane.TryGetComponent<ARPlaneMeshVisualizer>(out var vizualizer))
                    {
                        m_ARPlaneMeshVisualizers.Add(plane, vizualizer);
                        if (!m_UseARPlaneFading)
                            vizualizer.enabled = m_VisualizePlanes;
                    }

                    if (!plane.TryGetComponent<ARPlaneMeshVisualizerFader>(out var fader))
                        fader = plane.gameObject.AddComponent<ARPlaneMeshVisualizerFader>();
                    m_ARPlaneMeshVisualizerFaders.Add(plane, fader);
                    fader.visualizeSurfaces = m_VisualizePlanes;
                }
            }
        }

        void HandleObjectSpawned(GameObject spawnedObject)
        {
            int index = m_ObjectSpawner.spawnOptionIndex;
            if (index >= 0 && index < m_SpawnedMachineInstances.Count)
            {
                m_SpawnedMachineInstances[index] = spawnedObject;
                m_MachineButtons[index].interactable = false;

                m_ObjectSpawner.spawnOptionIndex = -1;
                m_ObjectSpawner.spawnEnabled = false;
            }
        }

        void SanitizeSelectable(Selectable selectable)
        {
            if (selectable == null) return;

            foreach (var anim in selectable.GetComponentsInChildren<Animator>(true))
                SanitizeAnimator(anim);

            if (selectable.transition == Selectable.Transition.Animation)
            {
                var animator = selectable.GetComponent<Animator>();
                if (animator == null || animator.runtimeAnimatorController == null)
                {
                    selectable.transition = Selectable.Transition.ColorTint;
                    if (animator != null)
                        SanitizeAnimator(animator);
                }
            }
        }

        void SanitizeAnimator(Animator animator)
        {
            if (animator == null) return;

            if (animator.runtimeAnimatorController == null)
            {
                animator.enabled = false;
                Destroy(animator);
            }
        }

        void SanitizeAnimation(Animation animation)
        {
            if (animation == null) return;
            if (animation.clip == null && animation.GetClipCount() == 0)
            {
                animation.enabled = false;
                Destroy(animation);
            }
        }
    }
}
