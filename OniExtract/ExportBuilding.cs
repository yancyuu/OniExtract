using System.Collections.Generic;
using UnityEngine;
using OniExtract;
using System.Linq;

public class ExportBuilding : BaseExport
{
    public override string ExportFileName { get; set; } = "building";
    public List<BBuildingEntity> bBuildingDefList = new List<BBuildingEntity>();
    public List<BuildMenuCategory> buildMenuCategories = new List<BuildMenuCategory>();
    public Dictionary<string, List<KeyValuePair<string, string>>> buildingAndSubcategoryDataPairs = new Dictionary<string, List<KeyValuePair<string, string>>>();
    public List<Tag> roomConstraintTags= new List<Tag>();
    public Dictionary<string, string> requiredSkillPerkMap = new Dictionary<string, string>();

    public ExportBuilding()
    {
    }

    public void AddNewBuildingDef(BuildingDef buildingDef)
    {
        this.roomConstraintTags = RoomConstraints.ConstraintTags.AllTags;
    }

    // 调试用：把 BuildingDef 的所有实例字段（名字+类型+值）写进 debug 文件
    public static void DumpBuildingDefFields(string path)
    {
        var sb = new System.Text.StringBuilder();
        var seen = false;
        foreach (var def in Assets.BuildingDefs)
        {
            if (!seen)
            {
                foreach (var f in def.GetType().GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance))
                {
                    object v = null;
                    try { v = f.GetValue(def); } catch { }
                    sb.AppendLine("FIELD\t" + f.FieldType.Name + "\t" + f.Name + "\t" + (v == null ? "null" : v.ToString()));
                }
                foreach (var p in def.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance))
                {
                    object v = null;
                    try { v = p.GetValue(def, null); } catch { }
                    sb.AppendLine("PROP\t" + p.PropertyType.Name + "\t" + p.Name + "\t" + (v == null ? "null" : v.ToString()));
                }
                seen = true;
            }
            // 重点建筑的完整字段值
            if (def.Tag.Name == "ThermoAquatuner" || def.Tag.Name == "AirConditioner" || def.Tag.Name == "Electrolyzer" || def.Tag.Name == "SteamTurbine")
            {
                sb.AppendLine("=== " + def.Tag.Name + " ===");
                foreach (var f in def.GetType().GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance))
                {
                    object v = null;
                    try { v = f.GetValue(def); } catch { }
                    if (f.FieldType == typeof(float) || f.FieldType == typeof(int) || f.FieldType == typeof(bool))
                        sb.AppendLine("  " + f.Name + " = " + (v == null ? "null" : v.ToString()));
                }
            }
        }
        System.IO.File.WriteAllText(path, sb.ToString());
    }

    private static float ReadFloatField(object obj, params string[] names)
    {
        foreach (var n in names)
        {
            var f = obj.GetType().GetField(n, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (f != null && f.FieldType == typeof(float)) return (float)f.GetValue(obj);
            var p = obj.GetType().GetProperty(n, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (p != null && p.PropertyType == typeof(float)) return (float)p.GetValue(obj, null);
        }
        return -1f;
    }

    public void AddNewBuildingEntity(BuildingDef buildingDef)
    {
        GameObject go = buildingDef.BuildingComplete;
        KPrefabID prefabID = go.GetComponent<KPrefabID>();
        BBuildingEntity bBuild = new BBuildingEntity(buildingDef.Tag.Name, prefabID);
        bBuild.widthInCells = buildingDef.WidthInCells;
        bBuild.heightInCells = buildingDef.HeightInCells;
        bBuild.materialCategory = buildingDef.MaterialCategory;
        bBuild.materialMass = buildingDef.Mass;
        bBuild.isFoundation = buildingDef.IsFoundation;
        bBuild.isKAnimTile = buildingDef.isKAnimTile;
        bBuild.isUtility = buildingDef.isUtility;
        bBuild.dragBuild = buildingDef.DragBuild;
        bBuild.buildLocationRule = (int)buildingDef.BuildLocationRule;
        bBuild.permittedRotations = (int)buildingDef.PermittedRotations;
        bBuild.sceneLayer = (int)buildingDef.SceneLayer;
        bBuild.objectLayer = (int)buildingDef.ObjectLayer;
        bBuild.viewMode = buildingDef.ViewMode.ToString();
        bBuild.defaultAnimState = buildingDef.DefaultAnimState;
        bBuild.uiSpriteName = buildingDef.UISprite != null ? buildingDef.UISprite.name : null;
        bBuild.powerConsumption = ReadFloatField(buildingDef, "PowerConsumption", "powerConsumption");
        bBuild.energyConsumptionWhenActive = ReadFloatField(buildingDef, "EnergyConsumptionWhenActive", "energyConsumptionWhenActive");
        bBuild.selfHeatKilowatts = ReadFloatField(buildingDef, "SelfHeatKilowattsWhenActive", "SelfHeatKilowatts");
        bBuild.exhaustHeatKilowatts = ReadFloatField(buildingDef, "ExhaustKilowattsWhenActive", "ExhaustKilowatts");
        bBuild.overheatTemperature = ReadFloatField(buildingDef, "OverheatTemperature", "overheatTemperature");
        Wire wire = go.GetComponent<Wire>();
        if (wire != null)
        {
            try
            {
                object rating = wire.MaxWattageRating;
                // 版本间 API 名字不稳定，反射找 Wire 上把档位换算成瓦数的静态方法
                foreach (var m in typeof(Wire).GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static))
                {
                    var ps = m.GetParameters();
                    if (m.Name.Contains("Wattage") && ps.Length == 1 && ps[0].ParameterType == rating.GetType())
                    {
                        object val = m.Invoke(null, new object[] { rating });
                        bBuild.wireMaxWattage = System.Convert.ToSingle(val);
                        break;
                    }
                }
                if (bBuild.wireMaxWattage < 0f)
                {
                    // 兜底：档位名里带数字（如 Num1kW / Num2kW / Num20kW）
                    var n = rating.ToString();
                    var mm = System.Text.RegularExpressions.Regex.Match(n, @"(\d+)kW");
                    if (mm.Success) bBuild.wireMaxWattage = float.Parse(mm.Groups[1].Value) * 1000f;
                }
            }
            catch { }
        }
        EnergyGenerator energyGenerator = go.GetComponent<EnergyGenerator>();
        if (energyGenerator != null)
        {
            bBuild.energyGenerator = new OutEnergyGenerator(energyGenerator);
            bBuild.energyGenerator.baseWattageRating = buildingDef.GeneratorWattageRating;
        }
        else
        {
            bBuild.energyGenerator = null;
        }
        ConduitConsumer conduitConsumer = go.GetComponent<ConduitConsumer>();
        if (conduitConsumer != null)
        {
            bBuild.conduitConsumer = new OutConduitConsumer(conduitConsumer);
        }
        else
        {
            bBuild.conduitConsumer = null;
        }
        ConduitDispenser conduitDispenser = go.GetComponent<ConduitDispenser>();
        if (conduitDispenser != null)
        {
            bBuild.conduitDispenser = new OutConduitDispenser(conduitDispenser);
        }
        else
        {
            bBuild.conduitDispenser = null;
        }
        ElementConverter[] elementConverters = go.GetComponents<ElementConverter>();
        if (elementConverters != null && elementConverters.Length > 0)
        {
            foreach (var elementConverter in elementConverters)
            {
                bBuild.elementConverters.Add(new OutElementConverter(elementConverter));
            }
        }
        PlantablePlot plantablePlot = go.GetComponent<PlantablePlot>();
        if (plantablePlot != null)
        {
            bBuild.plantablePlot = new OutPlantablePlot(plantablePlot);
        }
        else
        {
            bBuild.plantablePlot = null;
        }
        ElementConsumer[] elementConsumers = go.GetComponents<ElementConsumer>();
        if (elementConsumers != null && elementConsumers.Length > 0)
        {
            foreach (var elementConsumer in elementConsumers)
            {
                bBuild.elementConsumers.Add(new OutElementConsumer(elementConsumer));
            }
        }
        PassiveElementConsumer[] passiveElementConsumers = go.GetComponents<PassiveElementConsumer>();
        if (passiveElementConsumers != null && passiveElementConsumers.Length > 0)
        {
            foreach (var passiveElementConsumer in passiveElementConsumers)
            {
                bBuild.passiveElementConsumers.Add(new OutPassiveElementConsumer(passiveElementConsumer));
            }
        }
        Storage storage = go.GetComponent<Storage>();
        if (storage != null)
        {
            bBuild.storage = new OutStorage(storage);
        }
        AttachableBuilding attachableBuilding = go.GetComponent<AttachableBuilding>();
        if (attachableBuilding != null)
        {
            bBuild.attachableBuilding = attachableBuilding;
        }
        BuildingAttachPoint buildingAttachPoint = go.GetComponent<BuildingAttachPoint>();
        if (buildingAttachPoint != null)
        {
            bBuild.buildingAttachPoint = buildingAttachPoint;
        }
        RocketModule rocketModule = go.GetComponent<RocketModule>();
        if (rocketModule != null)
        {
            bBuild.rocketModule = rocketModule;
        }
        ReorderableBuilding reorderableBuilding = go.GetComponent<ReorderableBuilding>();
        if (reorderableBuilding != null)
        {
            bBuild.reorderableBuilding = reorderableBuilding;
        }
        RocketEngineCluster rocketEngineCluster = go.GetComponent<RocketEngineCluster>();
        if (rocketEngineCluster != null)
        {
            bBuild.rocketEngineCluster = new OutRocketEngineCluster(rocketEngineCluster);
        }
        RocketModuleCluster rocketModuleCluster = go.GetComponent<RocketModuleCluster>();
        if (rocketModuleCluster != null)
        {
            bBuild.rocketModuleCluster = rocketModuleCluster;
        }
        RocketEngine rocketEngine = go.GetComponent<RocketEngine>();
        if (rocketEngine != null)
        {
            bBuild.rocketEngine = new OutRocketEngine(rocketEngine);
        }
        PassengerRocketModule passengerRocketModule = go.GetComponent<PassengerRocketModule>();
        if (passengerRocketModule != null)
        {
            bBuild.passengerRocketModule = passengerRocketModule;
        }
        CargoBay cargoBay = go.GetComponent<CargoBay>();
        if (cargoBay != null)
        {
            bBuild.cargoBay = new OutCargoBay(cargoBay);
        }
        CargoBayConduit cargoBayConduit = go.GetComponent<CargoBayConduit>();
        if (cargoBayConduit != null)
        {
            bBuild.cargoBayConduit = cargoBayConduit;
        }
        CargoBayCluster cargoBayCluster = go.GetComponent<CargoBayCluster>();
        if (cargoBayCluster != null)
        {
            bBuild.cargoBayCluster = new OutCargoBayCluster(cargoBayCluster);
        }
        TreeFilterable treeFilterable = go.GetComponent<TreeFilterable>();
        if (treeFilterable != null)
        {
            bBuild.treeFilterable = new OutTreeFilterable(treeFilterable);
        }
        Deconstructable deconstructable = go.GetComponent<Deconstructable>();
        if (deconstructable != null)
        {
            bBuild.deconstructable = deconstructable;
        }
        Demolishable demolishable = go.GetComponent<Demolishable>();
        if (demolishable != null)
        {
            bBuild.demolishable = demolishable;
        }   
        Workable[] workableComponents = go.GetComponents<Workable>();
        var derivedWorkables = workableComponents.Where(component => component.GetType() != typeof(Workable) && component.GetType().IsSubclassOf(typeof(Workable)));
        foreach (var workable in derivedWorkables)
        {
            if (workable != null && workable.requiredSkillPerk != null && workable.requiredSkillPerk != "")
            {
                this.requiredSkillPerkMap.Add(buildingDef.Tag.Name, workable.requiredSkillPerk);
            }
        }
        Battery battery = go.GetComponent<Battery>();
        if (battery != null)
        {
            bBuild.battery = new OutBattery(battery);
        }
        RoomTracker roomTracker = go.GetComponent<RoomTracker>();
        if (roomTracker != null)
        {
            bBuild.roomTracker = roomTracker;
        }
        RocketUsageRestriction.Def rocketUsage = go.GetDef<RocketUsageRestriction.Def>();
        if (rocketUsage != null)
        {
            bBuild.rocketUsageRestrictionDef = rocketUsage;
        }

        this.bBuildingDefList.Add(bBuild);
    }

    public void ExportBuildMenu()
    {
        foreach (var planOrder in TUNING.BUILDINGS.PLANORDER)
        {
            string icon_name = PlanScreen.IconNameMap[planOrder.category];
            string categoryName = HashCache.Get().Get(planOrder.category);
            this.buildMenuCategories.Add(new BuildMenuCategory()
            {
                category = planOrder.category.HashValue,
                categoryName = categoryName,
                categoryIcon = icon_name
            });
            this.buildingAndSubcategoryDataPairs[categoryName] = planOrder.buildingAndSubcategoryData;
        }
    }
}
