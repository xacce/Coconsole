using UnityEngine;

namespace Coconsole
{
    [Coco]
    public static class CoconsoleButtonCommands
    {
        [Cmd]
        public static void Cmd1()
        {
            Debug.Log("Cmd1");
        }
        
        [Cmd]
        public static void Cmd2()
        {
            Debug.Log("Cmd2");
        }
    }
}