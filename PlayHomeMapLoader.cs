using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace VAM_MapLoader
{
    class PlayHomeMapLoader : MapLoader
    {
        static string MAPKEY = "PlayHome";
        GameObject currentMapBase;
        public event MapInitialized onMapInit;

        public void init()
        {           
        }

        public string Mapkey()
        {
            return MAPKEY;
        }

        public AvailableMap loadMap(AvailableMap mapName)
        {
            AssetBundle asb = MapLoaderPlugin.getBundle(mapName.fileName);

            if (asb != null && asb.GetAllAssetNames().Length > 0)
            {
                string asbN = asb.GetAllAssetNames()[0];

                GameObject gom = asb.LoadAsset<GameObject>(asbN);

                currentMapBase = GameObject.Instantiate(gom);

                MeshRenderer[] tt = currentMapBase.GetComponentsInChildren<MeshRenderer>();
                foreach (MeshRenderer at in tt)
                {
                    foreach (Material mx in at.sharedMaterials)
                    {
                        if (Shader.Find(mx.shader.name) != null)
                            mx.shader = Shader.Find(mx.shader.name);
                        else
                            mx.shader = Shader.Find("Standard");
                    }
                }

                SkinnedMeshRenderer[] ttx = currentMapBase.GetComponentsInChildren<SkinnedMeshRenderer>();
                foreach (SkinnedMeshRenderer at in ttx)
                {
                    foreach (Material mx in at.sharedMaterials)
                    {
                        if (Shader.Find(mx.shader.name) != null)
                            mx.shader = Shader.Find(mx.shader.name);
                        else
                            mx.shader = Shader.Find("Standard");
                    }
                }

                ParticleSystemRenderer[] psx = currentMapBase.GetComponentsInChildren<ParticleSystemRenderer>();
                foreach (ParticleSystemRenderer at in psx)
                {
                    foreach (Material mx in at.sharedMaterials)
                    {
                        if (Shader.Find(mx.shader.name) != null)
                            mx.shader = Shader.Find(mx.shader.name);
                        else
                            mx.shader = Shader.Find("Particles/Additive");
                    }
                }

            }
           
            onMapInit.Invoke(currentMapBase, mapName);

            return mapName;
        }

        public void unloadMap(AvailableMap currentLoadedScene)
        {
            if (currentMapBase != null)
            {
                GameObject.Destroy(currentMapBase);
            }
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
                        string[] files = Directory.GetFiles(directory);

                        foreach (string file in files)
                        {
                            string extension = Path.GetExtension(file);

                            if (extension.Length == 0)
                            {
                                availableMaps.Add(new AvailableMap(file, Path.GetFileNameWithoutExtension(file), MAPKEY));
                            }

                        }
                    }
                }
            }

            return availableMaps;
        }

        public void onSceneLoaded(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.LoadSceneMode arg1) { }

    }
}
