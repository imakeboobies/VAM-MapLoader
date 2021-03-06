﻿using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Web;


namespace VAM_MapLoader
{
    class UnityDefaultMapLoader : MapLoader
    {
        public static string MAPKEY = "Unity";
        public event MapInitialized onMapInit;
        GameObject currentMapBase;
        AvailableMap currentAvailMapLoad;

        public void init()
        {

        }

        public string Mapkey()
        {
            return MAPKEY;
        }

        public AvailableMap loadMap(AvailableMap mapName)
        {
            AssetBundle ab = MapLoaderPlugin.getBundle(mapName.fileName);

            string sceneName = "";
            mapName.parameters = new List<string>();
            if (ab != null)
            {
                if (ab.GetAllScenePaths().Length > 0)
                {
                    sceneName = ab.GetAllScenePaths()[0];
                    UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, UnityEngine.SceneManagement.LoadSceneMode.Additive);
                }

            }
            mapName.parameters.Add(sceneName);
            currentAvailMapLoad = mapName;
            return mapName;
        }

        public void unloadMap(AvailableMap currentLoadedScene)
        {
            GameObject.Destroy(currentMapBase);

        /*    if (currentLoadedScene.parameters.Count > 0)
                UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(currentLoadedScene.parameters[0]);*/
        }

        public List<AvailableMap> getAvailableMaps(Dictionary<string, List<string>> configDirectories)
        {
            List<AvailableMap> availableMaps = new List<AvailableMap>();            

            if (configDirectories.ContainsKey(MAPKEY))
            {
                foreach (string directory in configDirectories[MAPKEY])
                {
                    if (Directory.Exists(directory))
                    {
                        string[] files = Directory.GetFiles(Path.GetFullPath(directory), "*.scene");
                        foreach (string file in files)
                        {
                            availableMaps.Add(new AvailableMap(file, Path.GetFileNameWithoutExtension(file), MAPKEY));
                        }
                    }
                }
            }
            return availableMaps;
        }

        public void onSceneLoaded(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.LoadSceneMode arg1)
        {
            currentMapBase = new GameObject("mapRoot");

            GameObject[] rootObjs = arg0.GetRootGameObjects();

            foreach (GameObject roots in rootObjs)
            {
                roots.transform.SetParent(currentMapBase.transform, true);
            }

            onMapInit.Invoke(currentMapBase, currentAvailMapLoad);
        }
    }
}
