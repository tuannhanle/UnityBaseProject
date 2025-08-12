namespace ZoneSystem.MVP
{
    public interface IModel<T>
    {
        T Data { get; set; }
        void UpdateData(T newData);
        void ResetData();
    }

    public interface IView<T>
    {
        void UpdateView(T data);
        void Show();
        void Hide();
        void SetActive(bool active);
    }

    public interface IPresenter<T>
    {
        IModel<T> Model { get; }
        IView<T> View { get; }
        
        void Initialize();
        void UpdateData(T data);
        void ShowView();
        void HideView();
        void Cleanup();
    }
}
