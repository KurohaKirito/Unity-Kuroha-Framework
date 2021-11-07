#if UNITY_EDITOR

namespace Kuroha.Tool.Release.ParticleEffectProfiler.Scripts
{
    /// <summary>
    /// 处理单个特效的像素点
    /// </summary>
    public class SingleEffectOverdraw
    {
        private readonly EffectOverdrawData[] effectOverdrawData;
        private int qualityIndex;
        private static readonly string[] qualities = {
            "High", "Middle", "Low"
        };
    
        public SingleEffectOverdraw(int qualityIndex)
        {
            effectOverdrawData = new EffectOverdrawData[qualities.Length];
            for (var i = 0; i < effectOverdrawData.Length; i++)
            {
                effectOverdrawData[i] = new EffectOverdrawData {
                    quality = qualities[i]
                };
            }

            ChangeQuality(qualityIndex);
        }

        public void UpdateOneData(int pixDraw, int pixActualDraw)
        {
            if (pixDraw <= 0 && pixActualDraw <= 0)
            {
                return;
            }
            EffectOverdrawData effectOverdrawData = GetEffectEvlaData();
            effectOverdrawData.pixTotal = effectOverdrawData.pixTotal + pixDraw;
            effectOverdrawData.pixActualDrawTotal = effectOverdrawData.pixActualDrawTotal + pixActualDraw;
            effectOverdrawData.pixDrawTimes = effectOverdrawData.pixDrawTimes + 1;
        }

        public EffectOverdrawData GetEffectEvlaData()
        {
            return effectOverdrawData[qualityIndex-1];
        }

        public EffectOverdrawData[] GetEffectEvlaDatas()
        {
            return effectOverdrawData;
        }

        public void ChangeQuality(int qualityIndex)
        {
            this.qualityIndex = qualityIndex;
        }
    }
}
#endif