
using System;
using System.Linq;
using System.Reflection;
class P {
  static void Main() {
    var asm = Assembly.LoadFrom(@"G:\Steam\steamapps\common\OxygenNotIncluded\OxygenNotIncluded_Data\Managed\Assembly-CSharp.dll");
    var t = asm.GetType("DeathMonitor");
    Console.WriteLine("DeathMonitor fields: " + string.Join(",", t.GetFields(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance).Select(f=>f.Name)));
    var def = t.GetNestedType("Def");
    Console.WriteLine("Def fields: " + string.Join(",", def.GetFields(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance).Select(f=>f.Name + ":" + f.FieldType.Name)));
  }
}
