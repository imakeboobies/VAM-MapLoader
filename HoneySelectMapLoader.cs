using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace VAM_MapLoader
{
    class HoneySelectMapLoader : MapLoader
    {
        static string MAPKEY = "HoneySelect";
        static GameObject currentMapBase;              

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

            //check if there is a materials bundle as well, load it if it exists.           
            string materialBundleName = "mat_etc_" + Path.GetFileName(mapName.fileName);

            DirectoryInfo di = new DirectoryInfo(Path.GetDirectoryName(mapName.fileName));

            string matPath = Path.Combine(di.Parent.FullName, materialBundleName);

            if (File.Exists(matPath))
            {
                AssetBundle materialBundle = MapLoaderPlugin.getBundle(matPath);
                materialBundle.LoadAllAssets();
            }

            currentMapBase = new GameObject(mapName.displayName);

            if (asb != null && asb.GetAllAssetNames().Length > 0)
            {
                foreach (string prefabName in mapName.parameters)
                {
                    GameObject gom = asb.LoadAsset<GameObject>(prefabName);

                    if (gom != null)
                    {
                        GameObject prefabAdd = GameObject.Instantiate(gom);
                        prefabAdd.transform.SetParent(currentMapBase.transform, true);
                        
                    }

                }

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
                        string[] files = Directory.GetFiles(Path.GetFullPath(directory), "*.unity3d");


                        for (int i = 0; i < files.Length; i++)
                        {
                            string file = files[i];

                            AssetBundle mapContainer = MapLoaderPlugin.getBundle(file);

                            string[] asFiles = mapContainer.GetAllAssetNames();

                            Dictionary<string, AvailableMap> mapParams = new Dictionary<string, AvailableMap>();

                            foreach (string asFile in asFiles)
                            {
                                string lowerFile = asFile.ToLower();

                                if (lowerFile.Contains("prefabs") && !(lowerFile.Contains("cam") || lowerFile.Contains("haichi")))
                                {
                                    int lastSlash = asFile.LastIndexOf('/');

                                    string mapName = lastSlash > 8 ?asFile.Substring(lastSlash - 9, 9) : asFile;


                                    if (mapParams.ContainsKey(mapName))
                                        mapParams[mapName].parameters.Add(asFile);                                       
                                    else
                                    {
                                        List<string> param = new List<string>();
                                        param.Add(asFile);
                                        AvailableMap amap = new AvailableMap(file, mapName,param);
                                        mapParams.Add(mapName, amap);
                                    }

                                }
                             else if (lowerFile.Contains("map_hs") && !(lowerFile.Contains("cam") || lowerFile.Contains("haichi") || lowerFile.Contains("prefabs") || lowerFile.Contains("lightmap")))
                                {
                                    string mapName = Path.GetFileNameWithoutExtension(file);

                                    if (mapParams.ContainsKey(mapName))
                                        mapParams[mapName].parameters.Add(asFile);
                                    else
                                    {
                                        List<string> param = new List<string>();
                                        param.Add(asFile);
                                        AvailableMap amap = new AvailableMap(file, mapName, param);
                                        mapParams.Add(mapName, amap);
                                    }
                                }
                            }

                            foreach (KeyValuePair<string, AvailableMap> mapsToAdd in mapParams)
                            {
                                availableMaps.Add(mapsToAdd.Value);
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
