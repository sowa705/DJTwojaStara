using CSCore.Streams.Effects;

namespace DJTwojaStara.Audio;

public static class EqualizerExtensions
{
    public static void UseEQPreset(this Equalizer equalizer, EQPreset preset)
    {
        float[] gains = new float[10]; // we always use 10 gains

        switch (preset)
        {
            case EQPreset.Normal:
                break; // we dont have to do anything
            case EQPreset.BassBoost:
                gains[0] = 10f;
                gains[1] = 8f;
                gains[2] = 5f;
                gains[3] = 1f;
                break;
            case EQPreset.Beyerdynamic:
                gains[0] = 10f;
                gains[1] = 8f;
                gains[2] = 5f;
                gains[3] = 1f;
                gains[8] = 15f; // lmao
                break;
            case EQPreset.Earrape:
                for (int i = 0; i < gains.Length; i++)
                {
                    gains[i] = 25f-i*0.5f;
                }
                break;
            case EQPreset.LaptopSpeakers:
                gains[0] = -50f;
                gains[1] = -40f;
                gains[2] = -20f;
                gains[3] = -10f;
                gains[8] = 5f;
                gains[8] = 10f;
                gains[9] = 20f;
                break;
        }

        for (int i = 0; i < gains.Length; i++)
        {
            equalizer.SampleFilters[i].Filters[0].GainDB = gains[i];
            equalizer.SampleFilters[i].Filters[1].GainDB = gains[i];
        }
    }
}

public enum EQPreset
{
    Normal,
    BassBoost,
    Earrape,
    LaptopSpeakers,
    Beyerdynamic
}