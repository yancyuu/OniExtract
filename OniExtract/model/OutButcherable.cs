using System.Collections.Generic;

namespace OniExtract
{
    // 宰杀/死亡掉落表（Tag -> kg）。原始 Butcherable 是 Unity 组件，序列化器会跳过，
    // 按工程惯例包成 Out* 模型导出（见 OutPrioritizable）。
    public class OutButcherable
    {
        public Dictionary<string, float> drops;

        public OutButcherable(Butcherable obj)
        {
            drops = new Dictionary<string, float>();
            if (obj.drops == null) return;
            foreach (var kv in obj.drops)
                drops[kv.Key.ToString()] = kv.Value;
        }
    }
}
