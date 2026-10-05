# 修改说明（基于 cnctemaR/OniExtract2024 v1.7.0，GPL-2.0）

本仓库是 https://github.com/cnctemaR/OniExtract2024 的修改版，适配游戏 U59（水生行星包）。

## 修改内容
1. `model/OutEnergyGenerator.cs`：新增 `baseWattageRating` 字段
2. `ExportBuilding.cs`：从 BuildingDef 读取发电机瓦数；用反射读取建筑耗电（EnergyConsumptionWhenActive）、自热（SelfHeatKilowattsWhenActive）、排热（ExhaustKilowattsWhenActive）、过热温度（OverheatTemperature）；新增 BuildingDef 字段调试导出（debug_buildingdef.txt）
3. `model/BBuildingEntity.cs`：新增 powerConsumption / energyConsumptionWhenActive / selfHeatKilowatts / exhaustHeatKilowatts / overheatTemperature 字段
4. `OniExtract2024.csproj`：GameLibsFolder 指向本机游戏路径（按需自行修改）
5. `Patches.cs`：新增 BuildingDef 字段调试导出（debug_buildingdef.txt）和动物代谢 Def 调试导出（debug_critters.txt）
6. `ExportBuilding.cs` / `model/BBuildingEntity.cs`：新增电线负载上限导出（wireMaxWattage，反射兼容 API 改名）
