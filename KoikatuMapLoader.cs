using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

namespace VAM_MapLoader
{
    class KoikatuMapLoader : MapLoader
    {
        


        static string MAPKEY = "Koikatu";
        AssetBundle shaders;
        Dictionary<string, Shader> additionalShaders;
        public event MapInitialized onMapInit;
        GameObject mapRoot;
        AvailableMap currentLoadMap;
        public void init()
        {

            additionalShaders = new Dictionary<string, Shader>();
         /* XXX This should work to add shaders, but it doesnt. GG unity!
            if (shaders == null)
                shaders = AssetBundle.LoadFromFile("Shaders\\koikatu.shaders");

            
                        
            Shader[] shs = shaders.LoadAllAssets<Shader>();

            foreach (Shader sh in shs)
            {
                Shader shIn = Shader.Instantiate<Shader>(sh);                               
                additionalShaders.Add(shIn.name, shIn);
            }*/
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
            currentLoadMap = mapName;
            return mapName;
        }

        public void unloadMap(AvailableMap currentLoadedScene)
        {
            GameObject.Destroy(mapRoot);
        }

        public List<AvailableMap> getAvailableMaps(Dictionary<string, List<string>> configDirectories)
        {
            List<AvailableMap> availableMaps = new List<AvailableMap>();

            if (configDirectories.ContainsKey(MAPKEY))
            {

                foreach (string directory in configDirectories[MAPKEY])
                {
                    string[] files = Directory.GetFiles(directory, "*.unity3d");

                    foreach (string file in files)
                    {
                        availableMaps.Add(new AvailableMap(file, Path.GetFileNameWithoutExtension(file), MAPKEY));
                    }
                }
            }

            return availableMaps;
        }

        public void onSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {

            mapRoot = new GameObject("mapRoot");

            GameObject[] rootObjs = arg0.GetRootGameObjects();

            foreach (GameObject mapGameObject in rootObjs)
            {

                MeshRenderer[] tt = mapGameObject.GetComponentsInChildren<MeshRenderer>();
                foreach (MeshRenderer at in tt)
                {

                    foreach (Material mx in at.sharedMaterials)
                    {
                        if (mx != null)
                        {
                            if (Shader.Find(mx.shader.name) != null)
                                mx.shader = Shader.Find(mx.shader.name);
                            else if (additionalShaders.ContainsKey(mx.shader.name))
                                mx.shader = additionalShaders[mx.shader.name];
                            else
                                mx.shader = Shader.Find("Standard");
                        }

                    }

                }

                SkinnedMeshRenderer[] ttx = mapGameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
                foreach (SkinnedMeshRenderer at in ttx)
                {

                    foreach (Material mx in at.sharedMaterials)
                    {
                        if (mx != null)
                        {
                            if (Shader.Find(mx.shader.name) != null)
                                mx.shader = Shader.Find(mx.shader.name);
                            else if (additionalShaders.ContainsKey(mx.shader.name))
                                mx.shader = additionalShaders[mx.shader.name];
                            else
                                mx.shader = Shader.Find("Standard");
                        }

                    }

                }

                SpriteRenderer[] spr = mapGameObject.GetComponentsInChildren<SpriteRenderer>();
                foreach (SpriteRenderer at in spr)
                {

                    foreach (Material mx in at.sharedMaterials)
                    {
                        if (mx != null)
                        {
                            if (Shader.Find(mx.shader.name) != null)
                                mx.shader = Shader.Find(mx.shader.name);
                            else if (additionalShaders.ContainsKey(mx.shader.name))
                                mx.shader = additionalShaders[mx.shader.name];
                            else
                                mx.shader = Shader.Find("Standard");
                        }

                    }

                }

                ParticleSystemRenderer[] psr = mapGameObject.GetComponentsInChildren<ParticleSystemRenderer>();
                foreach (ParticleSystemRenderer at in psr)
                {

                    foreach (Material mx in at.sharedMaterials)
                    {
                        if (mx != null)
                        {
                            if (Shader.Find(mx.shader.name) != null)
                                mx.shader = Shader.Find(mx.shader.name);
                            else if (additionalShaders.ContainsKey(mx.shader.name))
                                mx.shader = additionalShaders[mx.shader.name];
                            else
                                mx.shader = Shader.Find("Particles/Additive");
                   

                        }

                    }

                }

                mapGameObject.transform.SetParent(mapRoot.transform, true);
            }

            onMapInit.Invoke(mapRoot,currentLoadMap);
        }
    }
}
