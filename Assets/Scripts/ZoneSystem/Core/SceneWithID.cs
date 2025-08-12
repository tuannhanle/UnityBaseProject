using UnityEngine;
using ZoneSystem.Interfaces;
using System;

namespace ZoneSystem.Core
{
    public abstract class SceneWithID : MonoBehaviour, IScene
    {
        [Header("Scene Identification")]
        [SerializeField] protected int sceneID;
        [SerializeField] protected string sceneName;
        [SerializeField] protected GameObject sceneGameObject;
        
        [Header("Position Settings")]
        [SerializeField] protected ScenePosition position;
        [SerializeField] protected Vector2 customPosition;
        [SerializeField] protected Vector2 sceneSize = Vector2.one;

        protected bool isInitialized = false;
        protected IZone parentZone;

        public int SceneID => sceneID;
        public string SceneName => sceneName;
        public ScenePosition Position => position;
        public IZone ParentZone 
        { 
            get => parentZone; 
            set => parentZone = value; 
        }
        public bool IsActive { get; protected set; }
        public GameObject SceneRoot => sceneGameObject != null ? sceneGameObject : gameObject;

        public event Action<IScene> OnSceneActivated;
        public event Action<IScene> OnSceneDeactivated;

        protected virtual void Awake()
        {
            if (string.IsNullOrEmpty(sceneName))
                sceneName = $"Scene_{sceneID}";

            if (sceneGameObject == null)
                sceneGameObject = gameObject;

            ValidateSceneID();
        }

        protected virtual void Start()
        {
            Initialize();
        }

        private void ValidateSceneID()
        {
            if (sceneID <= 0)
            {
                Debug.LogWarning($"Scene {gameObject.name} có SceneID không hợp lệ: {sceneID}. Nên sử dụng ID > 0.");
            }
        }

        public virtual void Initialize()
        {
            if (isInitialized) return;

            SetupPosition();
            OnInitializeInternal();
            
            isInitialized = true;
            
            Debug.Log($"Đã khởi tạo Scene ID: {sceneID}, Name: {sceneName}");
        }

        public virtual void Activate()
        {
            if (!isInitialized)
                Initialize();

            IsActive = true;
            SceneRoot.SetActive(true);
            
            OnActivateInternal();
            OnSceneActivated?.Invoke(this);
            
            Debug.Log($"Đã kích hoạt Scene {sceneName} (ID: {sceneID})");
        }

        public virtual void Deactivate()
        {
            IsActive = false;
            SceneRoot.SetActive(false);
            
            OnDeactivateInternal();
            OnSceneDeactivated?.Invoke(this);
            
            Debug.Log($"Đã tắt Scene {sceneName} (ID: {sceneID})");
        }

        public virtual void UpdateView<T>(T data)
        {
            if (!IsActive || parentZone?.State != ZoneState.Active) 
            {
                Debug.LogWarning($"Scene {sceneName} không active hoặc Zone không active - bỏ qua update");
                return;
            }

            OnUpdateViewInternal(data);
            Debug.Log($"Đã cập nhật view cho Scene {sceneName} (ID: {sceneID})");
        }

        public virtual void SetPosition(Vector2 position, Vector2 size)
        {
            customPosition = position;
            sceneSize = size;
            this.position = ScenePosition.Custom;
            
            ApplyPosition(position, size);
        }

        protected virtual void SetupPosition()
        {
            Vector2 targetPosition;
            
            switch (position)
            {
                case ScenePosition.TopLeft:
                    targetPosition = new Vector2(0, 1);
                    break;
                case ScenePosition.TopCenter:
                    targetPosition = new Vector2(0.5f, 1);
                    break;
                case ScenePosition.TopRight:
                    targetPosition = new Vector2(1, 1);
                    break;
                case ScenePosition.MiddleLeft:
                    targetPosition = new Vector2(0, 0.5f);
                    break;
                case ScenePosition.MiddleCenter:
                    targetPosition = new Vector2(0.5f, 0.5f);
                    break;
                case ScenePosition.MiddleRight:
                    targetPosition = new Vector2(1, 0.5f);
                    break;
                case ScenePosition.BottomLeft:
                    targetPosition = new Vector2(0, 0);
                    break;
                case ScenePosition.BottomCenter:
                    targetPosition = new Vector2(0.5f, 0);
                    break;
                case ScenePosition.BottomRight:
                    targetPosition = new Vector2(1, 0);
                    break;
                case ScenePosition.Custom:
                    targetPosition = customPosition;
                    break;
                default:
                    targetPosition = Vector2.zero;
                    break;
            }

            ApplyPosition(targetPosition, sceneSize);
        }

        protected virtual void ApplyPosition(Vector2 normalizedPosition, Vector2 size)
        {
            var rectTransform = SceneRoot.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                // Set anchor to position
                rectTransform.anchorMin = normalizedPosition;
                rectTransform.anchorMax = normalizedPosition;
                
                // Set size
                rectTransform.sizeDelta = new Vector2(
                    size.x * Screen.width, 
                    size.y * Screen.height
                );
                
                // Set pivot based on position
                Vector2 pivot = normalizedPosition;
                rectTransform.pivot = pivot;
                rectTransform.anchoredPosition = Vector2.zero;
            }
        }

        // Abstract methods để subclass implement
        protected abstract void OnInitializeInternal();
        protected abstract void OnActivateInternal();
        protected abstract void OnDeactivateInternal();
        protected abstract void OnUpdateViewInternal<T>(T data);

        // Utility methods
        public void SetSceneID(int newID)
        {
            if (newID > 0)
            {
                sceneID = newID;
                sceneName = $"Scene_{sceneID}";
            }
        }

        public void SetSceneGameObject(GameObject newGameObject)
        {
            if (newGameObject != null)
            {
                sceneGameObject = newGameObject;
            }
        }

        // Editor helper
        [ContextMenu("Generate Scene Name from ID")]
        private void GenerateSceneNameFromID()
        {
            sceneName = $"Scene_{sceneID}";
        }
    }
}
