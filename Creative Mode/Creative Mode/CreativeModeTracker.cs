using System.Collections.Generic;
using System.Linq;
using TShockAPI;

namespace CreativeMode
{
    public class CreativeModeTracker
    {
        public static Dictionary<string, CreativeModeProperties> CreativeModeProperties = new();

        public static CreativeModeProperties GetProperties(string name)
        {
            if (CreativeModeProperties.ContainsKey(name))
                return CreativeModeProperties[name];
            else
                return SetProperties(name, new CreativeModeProperties(TShock.Players.Where(x => x.Name == name).FirstOrDefault()));
        }
        public static CreativeModeProperties SetProperties(string name, CreativeModeProperties properties)
        {
            if (CreativeModeProperties.ContainsKey(name))
                CreativeModeProperties[name] = properties;
            else
                CreativeModeProperties.Add(name, properties);

            return CreativeModeProperties[name];
        }
        public static void RemoveProperties(string name)
        {
            if (CreativeModeProperties.ContainsKey(name))
                CreativeModeProperties.Remove(name);
        }
        public static void ApplyBuffs()
        {
            foreach (var p in CreativeModeProperties.Values)
            {

                if (p.Shine) p.Player.SetBuff(11, 300, true);
                if (p.Panic) p.Player.SetBuff(63, 300, true);
                if (p.WaterWalk) p.Player.SetBuff(15, 300, true);
                if (p.NightOwl) p.Player.SetBuff(12, 300, true);
                if (p.Gills) p.Player.SetBuff(4, 300, true);
                if (p.ObsidianSkin) p.Player.SetBuff(1, 300, true);
                if (p.Builder) p.Player.SetBuff(107, 300, true);
                if (p.Mining) p.Player.SetBuff(104, 300, true);
                if (p.Builder) p.Player.SetBuff(107, 300, true);
            }
        }
        
        public static List<CreativeModeProperties> GetPlayers()
        {
            return CreativeModeProperties.Values.ToList();
        }
    }
}
