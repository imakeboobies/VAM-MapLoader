using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VAM_MapLoader
{
    interface MapLoader
    {
        string Mapkey();
        string loadMap(string mapName);
        void unloadMap(string currentLoadedScene);
        List<string> getAvailableMaps(Dictionary<string, List<string>> configDirectories);
    }
}
