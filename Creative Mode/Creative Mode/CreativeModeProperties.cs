using TShockAPI;

namespace CreativeMode
{
    public class CreativeModeProperties
    {
        public TSPlayer Player { get; set; }
        public bool Endless { get; set; } = false;
        public bool Shine { get; set; } = false;
        public bool NightOwl { get; set; } = false;
        public bool Builder { get; set; } = false;
        public bool Mining { get; set; } = false;
        public bool Panic { get; set; } = false;
        public bool WaterWalk { get; set; } = false;
        public bool Gills { get; set; } = false;
        public bool ObsidianSkin { get; set; } = false;
        public CreativeModeProperties(TSPlayer player)
        {
            Player = player;
        }

        public bool Toggle(bool justBuffs = false)
        {
            if (justBuffs == false)
                Endless = !Endless;

            Shine = !Shine;
            Panic = !Panic;
            WaterWalk = !WaterWalk;
            NightOwl = !NightOwl;
            Gills = !Gills;
            ObsidianSkin = !ObsidianSkin;
            Builder = !Builder;
            Mining = !Mining;
            return Mining;
        }

        public void AllOff(bool justBuffs = false)
        {
            if (justBuffs == false)
                Endless = false;

            Shine = false;
            Panic = false;
            WaterWalk = false;
            NightOwl = false;
            Gills = false;
            ObsidianSkin = false;
            Builder = false;
            Mining = false;
        }

        public void AllOn(bool justBuffs = false)
        {
            if (justBuffs == false)
                Endless = true;

            Shine = true;
            Panic = true;
            WaterWalk = true;
            NightOwl = true;
            Gills = true;
            ObsidianSkin = true;
            Builder = true;
            Mining = true;
        }
    }
}
