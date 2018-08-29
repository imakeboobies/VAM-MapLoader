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

        public void init()
        {

        }

        public string Mapkey()
        {
            return MAPKEY;
        }

        public string loadMap(string mapName)
        {
            AssetBundle asb = null;
            try
            {
                if (MapLoaderPlugin.bundles.ContainsKey(mapName))
                    asb = MapLoaderPlugin.bundles[mapName];
                else
                {
                    asb = AssetBundle.LoadFromFile(mapName);
                    MapLoaderPlugin.bundles.Add(mapName, asb);
                }
            }
            catch (Exception ex) { }
            

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
                            mx.shader = Shader.Find("Standard");
                    }
                }

            }

            return mapName;
        }

        public void unloadMap(string currentLoadedScene)
        {
            if (currentMapBase != null)
            {
                GameObject.Destroy(currentMapBase);
            }
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
                        string[] files = Directory.GetFiles(directory);

                        foreach (string file in files)
                        {
                            string extension = Path.GetExtension(file);

                            if (extension.Length == 0)
                            {
                                availableMaps.Add(file);
                            }

                        }
                    }
                }
            }

            return availableMaps;
        }
    }
}
