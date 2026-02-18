using UnityEditor;

namespace zFramework.AppBuilder
{
    public enum Platform
    {
        None = 0,
        Windows = 5,
        Windows64 = 19,
        Android = 13,
        IOS = 9,
        WebGL = (int)BuildTarget.WebGL,
        MacOS = 2,
    }
}
