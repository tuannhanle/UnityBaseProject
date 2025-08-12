using UnityEngine;

namespace ZoneSystem.MVP
{
    // Base Model class
    public abstract class ModelBase<T> : IModel<T>
    {
        protected T data;

        public T Data 
        { 
            get => data; 
            set => data = value; 
        }

        public virtual void UpdateData(T newData)
        {
            data = newData;
            OnDataUpdated(newData);
        }

        public virtual void ResetData()
        {
            data = default(T);
            OnDataReset();
        }

        protected virtual void OnDataUpdated(T newData) { }
        protected virtual void OnDataReset() { }
    }

    // Base View class
    public abstract class ViewBase<T> : MonoBehaviour, IView<T>
    {
        [SerializeField] protected GameObject viewRoot;
        [SerializeField] protected bool startVisible = true;

        protected virtual void Awake()
        {
            if (viewRoot == null)
                viewRoot = gameObject;
        }

        protected virtual void Start()
        {
            SetActive(startVisible);
        }

        public abstract void UpdateView(T data);

        public virtual void Show()
        {
            SetActive(true);
            OnShow();
        }

        public virtual void Hide()
        {
            SetActive(false);
            OnHide();
        }

        public virtual void SetActive(bool active)
        {
            if (viewRoot != null)
                viewRoot.SetActive(active);
        }

        protected virtual void OnShow() { }
        protected virtual void OnHide() { }
    }

    // Base Presenter class
    public abstract class PresenterBase<T> : IPresenter<T>
    {
        protected IModel<T> model;
        protected IView<T> view;
        protected bool isInitialized = false;

        public IModel<T> Model => model;
        public IView<T> View => view;

        public PresenterBase(IModel<T> model, IView<T> view)
        {
            this.model = model;
            this.view = view;
        }

        public virtual void Initialize()
        {
            if (isInitialized) return;

            OnInitialize();
            isInitialized = true;
        }

        public virtual void UpdateData(T data)
        {
            if (!isInitialized)
                Initialize();

            model.UpdateData(data);
            view.UpdateView(model.Data);
            OnDataUpdated(data);
        }

        public virtual void ShowView()
        {
            view.Show();
            OnViewShown();
        }

        public virtual void HideView()
        {
            view.Hide();
            OnViewHidden();
        }

        public virtual void Cleanup()
        {
            OnCleanup();
        }

        protected virtual void OnInitialize() { }
        protected virtual void OnDataUpdated(T data) { }
        protected virtual void OnViewShown() { }
        protected virtual void OnViewHidden() { }
        protected virtual void OnCleanup() { }
    }
}
