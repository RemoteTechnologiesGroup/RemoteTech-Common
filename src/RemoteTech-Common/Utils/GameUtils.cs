namespace RemoteTech.Common.Utils
{
    public static class GameUtil
    {
        public static bool IsGameScenario
            =>
                HighLogic.CurrentGame != null &&
                (HighLogic.CurrentGame.Mode == Game.Modes.SCENARIO ||
                 HighLogic.CurrentGame.Mode == Game.Modes.SCENARIO_NON_RESUMABLE);

        public static int KSPMajorVersion
        {
            get { return Versioning.version_major; }
        }

        public static int KSPMinorVersion
        {
            get { return Versioning.version_minor; }
        }
    }
}