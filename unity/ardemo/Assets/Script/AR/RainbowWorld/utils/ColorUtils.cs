using System.Collections.Generic;

public class ColorUtils
{
    public static readonly EColorType[] Trichromatic = new EColorType[] { EColorType.R, EColorType.G, EColorType.B };
    public static readonly EColorType[] MixedColor = new EColorType[] { EColorType.Y, EColorType.M, EColorType.C };
    /// <summary>
    /// 判断是否是三基色
    /// </summary>
    /// <param name="color"></param>
    /// <returns></returns>
    public static bool IsTrichromatic(EColorType color)
    {
        switch (color)
        {
            case EColorType.R:
                return true;
            case EColorType.G:
                return true;
            case EColorType.B:
                return true;
        }
        return false;
    }
    /// <summary>
    /// 把三基色合成混合色
    /// </summary>
    /// <param name="color1"></param>
    /// <param name="color2"></param>
    /// <returns></returns>
    public static EColorType MixColor(EColorType color1, EColorType color2)
    {
        int mixColor = (byte)color1 | (byte)color2;
        return (EColorType)mixColor;
    }
    /// <summary>
    /// 分解颜色
    /// </summary>
    /// <param name="color"></param>
    /// <returns></returns>
    public static List<EColorType> GetDecomposeColor(EColorType color)
    {
        List<EColorType> list = new List<EColorType>();
        if ((byte)(color & EColorType.R) == (byte)EColorType.R)
        {
            list.Add(EColorType.R);
        }
        if ((byte)(color & EColorType.G) == (byte)EColorType.G)
        {
            list.Add(EColorType.G);
        }
        if ((byte)(color & EColorType.B) == (byte)EColorType.B)
        {
            list.Add(EColorType.B);
        }
        return list;
    }

    /// <summary>
    /// 判断trichromatic是不是组成mixedColor的基础色
    /// </summary>
    /// <param name="trichromatic"></param>
    /// <param name="mixedColor"></param>
    /// <returns></returns>
    public static bool IsBasicColorForMix(EColorType trichromatic, EColorType mixedColor)
    {
        if (((byte)trichromatic & (byte)mixedColor) != 0)
        {
            return true;
        }
        return false;
    }
}