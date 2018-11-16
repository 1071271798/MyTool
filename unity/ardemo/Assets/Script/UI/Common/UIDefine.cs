using UnityEngine;
using System.Collections;

namespace Game.UI
{
    public enum WindowsIndex : int
    {
        Main = 1,
        Connenct,
        Model,
        EditFrame,
        CreateActions,
    }

    public class UIDefine
    {
        public static readonly Vector2 sm_resolution = new Vector2(640, 960);
    }
}
