using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Towerdefence
{
    internal static class ResourceManager
    {
       static List<GameObject> allGameObjects = new List<GameObject>();

        public static int GetGameObjectsCount() { return allGameObjects.Count; }
        public static void SetGO(GameObject go) { allGameObjects.Add(go); }
        public static GameObject GetGameObject(int id) { return allGameObjects[id]; }
    }
}
