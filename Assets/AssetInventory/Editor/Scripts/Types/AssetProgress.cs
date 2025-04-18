namespace AssetInventory
{
    public abstract class AssetProgress
    {
        public static string CurrentMain { get; protected set; }
        public static int MainCount { get; protected set; }
        public static int MainProgress { get; protected set; }
        public static string CurrentSub { get; protected set; }
        public static int SubCount { get; protected set; }
        public static int SubProgress { get; protected set; }
        public static bool CancellationRequested { get; set; }
        public static bool Running { get; set; }
        public static bool ReadOnly { get; protected set; }
        protected static Cooldown Cooldown;
        protected static MemoryObserver MemoryObserver;

        public static void ResetState(bool done)
        {
            Running = !done;
            CurrentMain = null;
            CurrentSub = null;
            MainCount = 0;
            MainProgress = 0;
            SubCount = 0;
            SubProgress = 0;
            ReadOnly = false;

            Cooldown = new Cooldown(AI.Config.cooldownInterval, AI.Config.cooldownDuration);
            Cooldown.Enabled = AI.Config.useCooldown;

            MemoryObserver = new MemoryObserver(AI.Config.memoryLimit);
            MemoryObserver.Enabled = true;
        }
    }
}