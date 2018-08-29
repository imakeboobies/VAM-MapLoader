using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VAM_MapLoader
{
    class KoikatuMapLoader : MapLoader
    {
        static string MAPKEY = "Koikatu";
        AssetBundle shaders;
       
        public void init()
        {
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;

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
                    string[] files = Directory.GetFiles(directory, "*.unity3d");
                   
                    foreach (string file in files)
                    {
                        availableMaps.Add(file);
                    }
                }
            }

            return availableMaps;
        }

        private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            GameObject[] rootObjs = arg0.GetRootGameObjects();

            foreach (GameObject currentMapBase in rootObjs)
            {             
                MeshRenderer[] tt = currentMapBase.GetComponentsInChildren<MeshRenderer>();
                foreach (MeshRenderer at in tt)
                {

                    foreach (Material mx in at.sharedMaterials)
                    {
                        if (mx != null)
                        {
                            if (Shader.Find(mx.shader.name) != null)
                                mx.shader = Shader.Find(mx.shader.name);
                            else
                                mx.shader = Shader.Find("Standard");
                        }

                    }

                }

                SkinnedMeshRenderer[] ttx = currentMapBase.GetComponentsInChildren<SkinnedMeshRenderer>();
                foreach (SkinnedMeshRenderer at in ttx)
                {

                    foreach (Material mx in at.sharedMaterials)
                    {
                        if (mx != null)
                        {
                            if (Shader.Find(mx.shader.name) != null)
                                mx.shader = Shader.Find(mx.shader.name);
                            else
                                mx.shader = Shader.Find("Standard");
                        }

                    }

                }

                SpriteRenderer[] spr = currentMapBase.GetComponentsInChildren<SpriteRenderer>();
                foreach (SpriteRenderer at in spr)
                {

                    foreach (Material mx in at.sharedMaterials)
                    {
                        if (mx != null)
                        {
                            if (Shader.Find(mx.shader.name) != null)
                                mx.shader = Shader.Find(mx.shader.name);
                            else
                                mx.shader = Shader.Find("Standard");
                        }

                    }

                }

                ParticleSystemRenderer[] psr = currentMapBase.GetComponentsInChildren<ParticleSystemRenderer>();
                foreach (ParticleSystemRenderer at in psr)
                {

                    foreach (Material mx in at.sharedMaterials)
                    {
                        if (mx != null)
                        {
                            if (Shader.Find(mx.shader.name) != null)
                                mx.shader = Shader.Find(mx.shader.name);
                            else
                                mx.shader = Shader.Find("Standard");
                        }

                    }

                }

            }

        }
    }
}
