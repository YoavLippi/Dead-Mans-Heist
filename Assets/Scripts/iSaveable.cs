namespace DefaultNamespace
{
    public interface iSaveable
    {
        void OnEnable()
        {
            WorldController.onSave += DoSave;
            WorldController.onLoad += DoLoad;
        }

        void OnDisable()
        {
            WorldController.onSave -= DoSave;
            WorldController.onLoad -= DoLoad;
        }
        void DoSave();
        void DoLoad();
    }
}