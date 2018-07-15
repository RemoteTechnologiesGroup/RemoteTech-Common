namespace RemoteTech.Common.Interfaces
{
    /// <summary>
    /// Used for unity interface system only. 
    /// To be used by CommonCore class's drawing service
    /// </summary>
    interface IUnityWindow
    {
        void Draw();
        void Dispose();
    }
}
