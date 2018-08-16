using IllusionPlugin;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

namespace VAM_MapLoader
{
    class MapLoaderPlugin : IPlugin
    {
        string currentFileBrowserFormat = "json";
        bool currentFileBrowserShowFiles = false;
        bool sceneLoaded = false;
        string currentLoadedScene = "";
        string defaultLoadPath;
     
        Dictionary<string, AssetBundle> bundles = new Dictionary<string, AssetBundle>();

        public string Name
        {
            get
            {
                return "MapLoaderPlugin";
            }
        }

        public string Version
        {
            get
            {
                return "1.0";
            }
        }

        public void OnApplicationQuit()
        {

        }

        public void OnApplicationStart()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += onMapLoadComplete;
        }

        public void OnFixedUpdate()
        {

        }

        public void OnLevelWasInitialized(int level)
        {

        }

        void openSceneSelect()
        {

            SuperController.singleton.mainMenuUI.gameObject.SetActive(false);

            currentFileBrowserFormat = SuperController.singleton.fileBrowserUI.fileFormat;
            currentFileBrowserShowFiles = SuperController.singleton.fileBrowserUI.showFiles;
            SuperController.singleton.fileBrowserUI.fileFormat = "scene";
            SuperController.singleton.fileBrowserUI.showFiles = true;
            SuperController.singleton.fileBrowserUI.defaultPath = defaultLoadPath;
            SuperController.singleton.fileBrowserUI.Show(openScene);

        }

        private void openScene(string path)
        {

            try
            {

                try
                {
                    if (sceneLoaded && currentLoadedScene.Length > 0)
                    {                        
                        UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(currentLoadedScene);
                    }
                }
                catch (Exception e)
                {
                    //this is a horrible way of handling logic flow but unity doesn't expose anything to tell you what scenes are currently loaded. Might be able to re-work this with unloadscene.
                }

                    AssetBundle ab;
                if (bundles.ContainsKey(path))
                    ab = bundles[path];
                else
                { 
                    ab = AssetBundle.LoadFromFile(path);
                    bundles.Add(path, ab);
                }

                if (ab != null)
                {
                    if(ab.GetAllScenePaths().Length > 0)
                    {
                        string asb = ab.GetAllScenePaths()[0];                      
                        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(asb, UnityEngine.SceneManagement.LoadSceneMode.Additive);
                        currentLoadedScene = asb;
                        sceneLoaded = true;
                    
                    }
                }


             

            }
            catch (Exception e) { SuperController.LogError("VAM-MapLoader-MapLoaderPlugin: unable to load map from " + path); }

            SuperController.singleton.fileBrowserUI.fileFormat = currentFileBrowserFormat;
            SuperController.singleton.fileBrowserUI.showFiles = currentFileBrowserShowFiles;
        }

        private void onMapLoadComplete(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            if(scene.path.Equals(currentLoadedScene))
            {
                fixLightsInScene();
            }
            
        }

        public void OnLevelWasLoaded(int level)
        {
            //Make sure we're in the main VAM scenes
            if ((level == 1 || level == 6))
            {

                //Get existing UI components.
                Transform panelPrefab = getTransformByNameAndRoot("Panel", SuperController.singleton.mainMenuUI);
                Transform buttonPrefab = getTransformByNameAndRoot("Quit Button", SuperController.singleton.mainMenuUI);
                Transform textPrefab = getTransformByNameAndRoot("Text", SuperController.singleton.mainMenuUI);
                Image imagePrefab = buttonPrefab.GetComponent<Image>();
                RectTransform buttonPrefabRT = buttonPrefab.GetComponent<RectTransform>();
                string style = buttonPrefab.GetComponent<UIStyleButton>().styleName;
                RectTransform textPrefabRT = textPrefab.GetComponent<RectTransform>();

                //Create new gameobjects and associated UI components.
                GameObject mapLoadButtonGO = new GameObject("MapLoadButton");
                mapLoadButtonGO.transform.localScale = buttonPrefab.localScale;
                mapLoadButtonGO.transform.SetParent(panelPrefab, false);
                Button mapLoadButtonUI = mapLoadButtonGO.AddComponent<Button>();
                Image mapLoadButtonImage = mapLoadButtonGO.AddComponent<Image>();
                UIStyleButton mplbStyle = mapLoadButtonGO.AddComponent<UIStyleButton>();
                UIStyleImage mpliStyle = mapLoadButtonGO.AddComponent<UIStyleImage>();
                RectTransform mapButtonRT = mapLoadButtonGO.GetComponent<RectTransform>();
                //Copy values from existing components.
                CopyImageValues(imagePrefab, mapLoadButtonImage);
                mplbStyle.styleName = style;
                mpliStyle.styleName = style;
                CopyRectTransformValues(buttonPrefabRT, mapButtonRT);

                //Adjust position of button to fit into menu.
                buttonPrefabRT.anchoredPosition = new Vector2(buttonPrefabRT.anchoredPosition.x, buttonPrefabRT.anchoredPosition.y - 40f);
                mapButtonRT.anchoredPosition = new Vector2(mapButtonRT.anchoredPosition.x, mapButtonRT.anchoredPosition.y + mapButtonRT.rect.height);

                //Create new gameobject and ui for button text
                GameObject mapLoadTextGO = new GameObject("MapLoadText");
                mapLoadTextGO.transform.localScale = textPrefab.localScale;
                mapLoadTextGO.transform.SetParent(mapLoadButtonGO.transform, false);
                Text mapLoadText = mapLoadTextGO.AddComponent<Text>();
                UIStyleText mpltStyle = mapLoadButtonGO.AddComponent<UIStyleText>();
                RectTransform mapTextRT = mapLoadTextGO.GetComponent<RectTransform>();

                //Copy values from existing components.
                CopyRectTransformValues(textPrefabRT, mapTextRT);
                CopyTextValues(textPrefab.GetComponent<Text>(), mapLoadText);
                mpltStyle.styleName = style;

                //Update new components with adjusted values.
                mapLoadText.text = "Load External Map";
                mapLoadButtonUI.onClick.AddListener(openSceneSelect);

                //set default path to find scenes.
                string path = Application.dataPath;
                int lt = path.LastIndexOf("/");
                defaultLoadPath = path.Substring(0, lt);
            }
        }

        //Deep copy image
        void CopyImageValues(Image input, Image output)
        {
            output.type = input.type;
            output.sprite = input.sprite;
            output.color = input.color;
            output.material = input.material;
        }

        //Deep copy text
        void CopyTextValues(Text input, Text output)
        {
            output.alignByGeometry = input.alignByGeometry;
            output.alignment = input.alignment;
            output.color = input.color;
            output.font = input.font;
            output.fontSize = input.fontSize;
            output.fontStyle = input.fontStyle;
            output.horizontalOverflow = input.horizontalOverflow;
            output.lineSpacing = input.lineSpacing;
            output.maskable = input.maskable;
            output.material = input.material;
            output.resizeTextForBestFit = input.resizeTextForBestFit;
            output.resizeTextMaxSize = input.resizeTextMaxSize;
            output.resizeTextMinSize = input.resizeTextMinSize;
            output.supportRichText = input.supportRichText;
            output.useGUILayout = input.useGUILayout;
            output.verticalOverflow = input.verticalOverflow;
        }

        //Deep copy rect transform
        void CopyRectTransformValues(RectTransform br1, RectTransform br2)
        {

            br2.anchorMax = br1.anchorMax;
            br2.anchorMin = br1.anchorMin;
            br2.offsetMax = br1.offsetMax;
            br2.offsetMin = br1.offsetMin;
            br2.pivot = br1.pivot;
            br2.rotation = br1.rotation;
            br2.sizeDelta = br1.sizeDelta;
            br2.anchoredPosition = br1.anchoredPosition;
            br2.anchoredPosition3D = br1.anchoredPosition3D;
        }

        void fixLightsInScene()
        {
           Light[] lights = GameObject.FindObjectsOfType<Light>();

            foreach(Light light in lights)
            {                
                NGSS_Directional ngLight =  light.gameObject.AddComponent<NGSS_Directional>();
                
            }
        }


        public void OnUpdate()
        {

        }


        public static Transform getTransformByNameAndRoot(string name_, Transform parent_)
        {
            Transform[] mrs = parent_.GetComponentsInChildren<Transform>();

            for (int i = 0; i < mrs.Length; i++)
            {

                if (mrs[i].name.Equals(name_))
                    return mrs[i];
            }

            return null;
        }

    }


}



