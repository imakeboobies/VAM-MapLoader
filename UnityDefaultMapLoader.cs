using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Web;


namespace VAM_MapLoader
{
    class UnityDefaultMapLoader : MapLoader
    {
        public static string MAPKEY = "Unity";

        public void init()
        {

        }

        public string Mapkey()
        {
            return MAPKEY;
        }

        public string loadMap(string mapName)
        {
            AssetBundle ab = null;
            string sceneName = "";
            try
            {
                if (MapLoaderPlugin.bundles.ContainsKey(mapName))
                    ab = MapLoaderPlugin.bundles[mapName];
                else
                {
                    ab = AssetBundle.LoadFromFile(mapName);
                    MapLoaderPlugin.bundles.Add(mapName, ab);
                }
            }

            catch (Exception ex) { }

            if (ab != null)
            {

                if (ab.GetAllScenePaths().Length > 0)
                {
                    sceneName = ab.GetAllScenePaths()[0];
                    UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName, UnityEngine.SceneManagement.LoadSceneMode.Additive);
                }

            }

            return sceneName;
        }

        public void unloadMap(string currentLoadedScene)
        {
            UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(currentLoadedScene);
        }

        public List<string> getAvailableMaps(Dictionary<string, List<string>> configDirectories)
        {
            List<string> availableMaps = new List<string>();            

            if (configDirectories.ContainsKey(MAPKEY))
            {
                foreach (string directory in configDirectories[MAPKEY])
                {
                    if (Directory.Exists(directory))
                    {
                        string[] files = Directory.GetFiles(Path.GetFullPath(directory), "*.scene");
                        foreach (string file in files)
                        {
                            availableMaps.Add(file);
                        }
                    }
                }
            }
            return availableMaps;
        }
    }
}
