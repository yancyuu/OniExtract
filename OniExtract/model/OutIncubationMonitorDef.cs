
namespace OniExtract
{
    public class OutIncubationMonitorDef
    {
        public float baseIncubationRate;
        public Tag spawnedCreature;

        public OutIncubationMonitorDef(IncubationMonitor.Def obj)
        {
            this.baseIncubationRate = obj.baseIncubationRate;
            this.spawnedCreature = obj.spawnedCreature;
        }
    }
}
