
using System.Collections.Generic;
using UnityEngine.SceneManagement;


namespace VAM_MapLoader
{
    interface MapLoader
    {
        string Mapkey();
        void init();
        AvailableMap loadMap(AvailableMap mapName);
        void unloadMap(AvailableMap currentLoadedScene);
        List<AvailableMap> getAvailableMaps(Dictionary<string, List<string>> configDirectories);
        void onSceneLoaded(Scene scene, LoadSceneMode mode);
    }
}
