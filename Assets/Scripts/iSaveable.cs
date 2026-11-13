namespace DefaultNamespace
{
    public interface iSaveable
    {
        void RegisterSaveEvents()
        {
            WorldController.onSave += DoSave;
            WorldController.onLoad += DoLoad;
        }

        void DeregisterSaveEvents()
        {
            WorldController.onSave -= DoSave;
            WorldController.onLoad -= DoLoad;
        }
        void DoSave();
        void DoLoad();
    }
}