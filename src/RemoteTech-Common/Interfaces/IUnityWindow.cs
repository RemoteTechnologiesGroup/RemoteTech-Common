namespace RemoteTech.Common.Interfaces
{
    /// <summary>
    /// Used for unity interface system only.
    /// Please use Kerbal Space Program's own dialog service for complex interfaces
    /// To be used by CommonCore class's drawing service
    /// </summary>
    interface IUnityWindow
    {
        void Draw();
        void Dispose();
    }
}
