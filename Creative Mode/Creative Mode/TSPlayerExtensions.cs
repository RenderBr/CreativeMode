using TShockAPI;

namespace CreativeMode
{
    public static class TSPlayerExtensions
    {
        public static CreativeModeProperties GetProperties(this TSPlayer player)
        {
            return CreativeModeTracker.GetProperties(player.Name);
        }

        public static void SetProperties(this TSPlayer player)
        {
            CreativeModeTracker.SetProperties(player.Name, new(player));
        }

        public static void SendToggleMessage(this TSPlayer plr, string prop, bool toggle)
        {
            plr.SendInfoMessage($"{prop.FirstCharToUpper()} is now {(toggle ? "enabled" : "disabled")}");
        }
    }
}
