using UnityEngine;

namespace Coconsole
{
    [Coco]
    public static class CoconsoleDebugCommand
    {
        [Cmd]
        public static void Clear()
        {
            if (!CoconsoleSingleton.instance) return;
            CoconsoleSingleton.instance.Flush();
        }
        
        [Cmd]
        public static void Hide()
        {
            if (!CoconsoleSingleton.instance) return;
            CoconsoleSingleton.instance.Hide();
        }
    }
}