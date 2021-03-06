﻿using IllusionPlugin;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;
using System;
using System.IO;
using SimpleJSON;
using System.Collections;
using UnityThreading;

namespace VAM_MapLoader
{
    public class AvailableMap
    {
        public string fileName;
        public string displayName;
        public List<string> parameters;
        public string loaderType;



        public AvailableMap(string fileName_, string displayName_, string loaderType_) : this(fileName_, displayName_, loaderType_, new List<string>())
        {

        }

        public AvailableMap(string fileName_, string displayName_, string loaderType_, List<string> parameters_)
        {
            fileName = fileName_;
            displayName = displayName_;
            loaderType = loaderType_;
            parameters = parameters_;
        }
    }

    class MapLoaderPlugin : IPlugin
    {

    
        Atom atomAttach;
        public AvailableMap currentLoadedScene;// = "";        
        MapLoader currentLoader;
        MapLoaderControl lm;

        public static Dictionary<string, AssetBundle> bundles = new Dictionary<string, AssetBundle>();
        Dictionary<string, MapLoader> loaders;
        Dictionary<AvailableMap, MapLoader> availableMaps;

        public void LoadMap(AvailableMap map, string loaderType)
        {

            if (loaders.ContainsKey(loaderType.Trim()))
            {

                MapLoader loader = loaders[loaderType];

                LoadMap(map, loader);
            }
        }

        public void LoadMap(AvailableMap map, MapLoader loader)
        {

            if (currentLoadedScene != null)
            {
                currentLoader.unloadMap(currentLoadedScene);
            }

            currentLoadedScene = loader.loadMap(map);
            currentLoader = loader;

            if (lm)
                lm.am = currentLoadedScene;
        }

        private void onMapLoadComplete(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            if (currentLoader != null)
            {
                fixLightsInScene();
                currentLoader.onSceneLoaded(scene, mode);
            }

        }

        Dictionary<string, List<string>> processConfig(string configFilePath)
        {

            Dictionary<string, List<string>> configDirectories = new Dictionary<string, List<string>>();
            string json = File.ReadAllText(configFilePath);
            JSONClass jx = (JSONClass)JSONNode.Parse(json);
            foreach (string tag in jx.Keys)
            {
                string directory = Path.GetFullPath(@jx[tag].Value);

                if (configDirectories.ContainsKey(tag))
                    configDirectories[tag].Add(directory);
                else
                    configDirectories.Add(tag, new List<string>(new string[] { directory }));
            }

            return configDirectories;
        }

        void OnMapInit(GameObject gom, AvailableMap amp)
        {

            Atom coreControl = SuperController.singleton.GetComponent<Atom>();
            lm = coreControl.GetComponent<MapLoaderControl>();

            if (lm.controlAtom != null)
                gom.transform.SetParent(getTransformByNameAndRoot("object", lm.controlAtom.transform), false);

            if (lm)
                lm.am = amp;

        }

        #region Iplugin impl
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
                return "1.3";
            }
        }

        public void OnApplicationQuit()
        {

        }

        public void OnApplicationStart()
        {
            loaders = new Dictionary<string, MapLoader>();
            availableMaps = new Dictionary<AvailableMap, MapLoader>();

            Dictionary<string, List<string>> configDirectories = processConfig(Path.Combine(Directory.GetCurrentDirectory(), "MapLoaderConfig.json"));

            foreach (System.Type mytype in AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes()).Where(myType => myType.IsClass && !myType.IsAbstract && myType.GetInterfaces().Contains(typeof(MapLoader))))
            {
                try
                {
                    MapLoader mapLoaderImpl = (MapLoader)Activator.CreateInstance(mytype);
                    mapLoaderImpl.init();
                    mapLoaderImpl.onMapInit += OnMapInit;
                    loaders.Add(mapLoaderImpl.Mapkey(), mapLoaderImpl);
                }
                catch (Exception ex)
                {
                    SuperController.LogError("Failed to initialise map loader: " + mytype);
                }
            }

            foreach (KeyValuePair<string, List<string>> cfKey in configDirectories)
            {

                if (loaders.ContainsKey(cfKey.Key))
                {
                    List<AvailableMap> maps = loaders[cfKey.Key].getAvailableMaps(configDirectories);

                    foreach (AvailableMap avMap in maps)
                    {
                        availableMaps.Add(avMap, loaders[cfKey.Key]);
                    }
                }
                else
                    SuperController.LogError("Missing a map loader! " + cfKey.Key); //XXX
            }



            UnityEngine.SceneManagement.SceneManager.sceneLoaded += onMapLoadComplete;
        }

        public void OnFixedUpdate()
        {

        }

        public void OnLevelWasInitialized(int level)
        {
            //Make sure we're in the main VAM scenes
            if ((level == 1 || level == 6))
            {

                Atom coreControl = SuperController.singleton.GetAtomByUid("CoreControl");
                lm = coreControl.gameObject.GetComponent<MapLoaderControl>();

                if (!lm)
                {
                    lm = coreControl.gameObject.AddComponent<MapLoaderControl>();
                    coreControl.RegisterAdditionalStorable(lm);
                }
                Transform buttonPrefab = getTransformByNameAndRoot("Quit Button", SuperController.singleton.mainMenuUI);
                Transform HRButtonPrefab = getTransformByNameAndRoot("Hard Reset Button", SuperController.singleton.mainMenuUI);

                Button mapLoadButtonUI = createMenuButton("Load External Map");
                Button unloadButtonUI = createMenuButton("Unload External Map");

                RectTransform buttonPrefabRT = buttonPrefab.GetComponent<RectTransform>();
                //RectTransform hardResetRT = HRButtonPrefab.GetComponent<RectTransform>();
                RectTransform mapButtonRT = mapLoadButtonUI.gameObject.GetComponent<RectTransform>();
                RectTransform unloadButtonRT = unloadButtonUI.gameObject.GetComponent<RectTransform>();

                mapButtonRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, mapButtonRT.rect.width / 2f);
                mapButtonRT.anchoredPosition = new Vector2(mapButtonRT.anchoredPosition.x - (mapButtonRT.rect.width / 2f), mapButtonRT.anchoredPosition.y + mapButtonRT.rect.height + 80f);
                unloadButtonRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, unloadButtonRT.rect.width / 2f);
                unloadButtonRT.anchoredPosition = new Vector2(unloadButtonRT.anchoredPosition.x - (unloadButtonRT.rect.width / 2f), unloadButtonRT.anchoredPosition.y + 70f);


                ScrollRect scrollRect = createMapScrollRect();
                GameObject mapLoadPanelGO = scrollRect.gameObject;
                Scrollbar scrollBarMB = createScrollBar(mapLoadPanelGO);

                scrollRect.verticalScrollbar = scrollBarMB;
                scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
                scrollRect.verticalScrollbarSpacing = -3;

                mapLoadButtonUI.onClick.AddListener(() =>
                {
                    if (mapLoadPanelGO.activeSelf)
                        mapLoadPanelGO.SetActive(false);
                    else
                        mapLoadPanelGO.SetActive(true);
                });


                unloadButtonUI.onClick.AddListener(() =>
                {
                    if (currentLoader != null && currentLoadedScene != null)
                    {
                        currentLoader.unloadMap(currentLoadedScene);
                        currentLoader = null;
                        currentLoadedScene = null;

                    }
                });

                GameObject mapLoadContentGO = mapLoadPanelGO.GetComponent<ScrollRect>().content.gameObject;

                foreach (KeyValuePair<AvailableMap, MapLoader> map in availableMaps)
                {
                    createMapLoadButton(mapLoadContentGO, map.Key, map.Value);
                }

                mapLoadPanelGO.SetActive(false);
            }
        }

        public void OnLevelWasLoaded(int level)
        {

        }

        public void OnUpdate()
        {

        }
        #endregion

        #region Singleton impl
        private static MapLoaderPlugin instance = null;

        public static MapLoaderPlugin Instance
        {
            get
            {
                if (instance == null)
                {
                    List<IPlugin> plugins = (List<IPlugin>)IllusionInjector.PluginManager.Plugins;

                    foreach (IPlugin plug in plugins)
                    {
                        if (plug.GetType().Equals(typeof(MapLoaderPlugin)))
                        {
                            instance = (MapLoaderPlugin)plug;
                        }
                    }

                }
                return instance;
            }
        }
        #endregion

        #region UI Creation methods
        void createMapLoadButton(GameObject mapLoadContentGO, AvailableMap map, MapLoader loader)
        {
            Transform panelPrefab = getTransformByNameAndRoot("Panel", SuperController.singleton.mainMenuUI);
            Transform buttonPrefab = getTransformByNameAndRoot("Quit Button", SuperController.singleton.mainMenuUI);
            Transform textPrefab = getTransformByNameAndRoot("Text", SuperController.singleton.mainMenuUI);
            RectTransform buttonPrefabRT = buttonPrefab.GetComponent<RectTransform>();
            RectTransform textPrefabRT = textPrefab.GetComponent<RectTransform>();
            Image imagePrefab = buttonPrefab.GetComponent<Image>();
            string style = buttonPrefab.GetComponent<UIStyleButton>().styleName;
            RectTransform mapLoadContentRT = mapLoadContentGO.GetComponent<RectTransform>();

            GameObject mapLoadButtonGO = new GameObject("mapLoadButtonGO");
            mapLoadButtonGO.transform.localScale = panelPrefab.localScale;
            mapLoadButtonGO.transform.SetParent(mapLoadContentGO.transform, false);

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
            mapLoadContentRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, mapLoadContentRT.rect.height + mapButtonRT.rect.height);

            //Update new components with adjusted values.
            mapLoadText.text = loader.Mapkey() + " - " + map.displayName;

            mapLoadButtonUI.onClick.AddListener(() =>
            {
                LoadMap(map, loader);
            });
        }

        Scrollbar createScrollBar(GameObject mapLoadPanelGO)
        {
            Transform panelPrefab = getTransformByNameAndRoot("Panel", SuperController.singleton.mainMenuUI);
            Transform buttonPrefab = getTransformByNameAndRoot("Quit Button", SuperController.singleton.mainMenuUI);
            Image panelImagePrefab = panelPrefab.GetComponent<Image>();
            Image imagePrefab = buttonPrefab.GetComponent<Image>();
            RectTransform panelPrefabRT = panelPrefab.GetComponent<RectTransform>();
            RectTransform mapLoadPanelRT = mapLoadPanelGO.GetComponent<RectTransform>();

            //Scroll Bar
            GameObject hScrollBarPanel = new GameObject("hScrollBarPanel");
            hScrollBarPanel.transform.localScale = panelPrefab.localScale;
            hScrollBarPanel.transform.SetParent(mapLoadPanelGO.transform, false);

            Image hScrollBarImage = hScrollBarPanel.AddComponent<Image>();
            CopyImageValues(panelImagePrefab, hScrollBarImage);
            hScrollBarImage.color = Color.white;

            RectTransform hScrollBarRT = hScrollBarPanel.GetComponent<RectTransform>();
            CopyRectTransformValues(panelPrefabRT, hScrollBarRT);

            hScrollBarRT.anchorMax = new Vector2(0.5f, 0.5f);
            hScrollBarRT.anchorMin = new Vector2(0.5f, 0.5f);
            hScrollBarRT.pivot = new Vector2(0.5f, 0.5f);

            hScrollBarRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, panelPrefabRT.rect.height);
            hScrollBarRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 30f);
            hScrollBarRT.anchoredPosition = new Vector2((mapLoadPanelRT.rect.width / 2) - (hScrollBarRT.rect.width / 2), 0f);

            //Sliding Area
            GameObject hScrollBarSlidingArea = new GameObject("hScrollBarSlidingArea");
            hScrollBarSlidingArea.transform.localScale = panelPrefab.localScale;
            hScrollBarSlidingArea.transform.SetParent(hScrollBarPanel.transform, false);

            RectTransform hScrollBarSlidingAreaRT = hScrollBarSlidingArea.AddComponent<RectTransform>();
            hScrollBarSlidingAreaRT.anchorMax = new Vector2(1f, 1f);
            hScrollBarSlidingAreaRT.anchorMin = new Vector2(0f, 0f);
            hScrollBarSlidingAreaRT.pivot = new Vector2(0.5f, 0.5f);
            hScrollBarSlidingAreaRT.sizeDelta = new Vector2(-20f, -20f);


            //Scroll Handle
            GameObject hScrollBarHandle = new GameObject("hScrollBarHandle");
            hScrollBarHandle.transform.localScale = panelPrefab.localScale;
            hScrollBarHandle.transform.SetParent(hScrollBarSlidingArea.transform, false);

            Image hScrollBarHandleImage = hScrollBarHandle.AddComponent<Image>();
            CopyImageValues(imagePrefab, hScrollBarHandleImage);
            hScrollBarHandleImage.color = Color.white;

            RectTransform hScrollBarHandleRT = hScrollBarHandle.GetComponent<RectTransform>();
            hScrollBarHandleRT.anchorMax = new Vector2(0.5f, 0.5f);
            hScrollBarHandleRT.anchorMin = new Vector2(0.5f, 0.5f);
            hScrollBarHandleRT.pivot = new Vector2(0.5f, 0.5f);
            hScrollBarHandleRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0f);
            hScrollBarHandleRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 30f);


            Scrollbar scrollBarMB = hScrollBarPanel.AddComponent<Scrollbar>();
            scrollBarMB.handleRect = hScrollBarHandleRT;
            scrollBarMB.targetGraphic = hScrollBarHandleImage;
            scrollBarMB.direction = Scrollbar.Direction.BottomToTop;
            scrollBarMB.transition = Selectable.Transition.ColorTint;

            return scrollBarMB;
        }

        ScrollRect createMapScrollRect()
        {

            Transform panelPrefab = getTransformByNameAndRoot("Panel", SuperController.singleton.mainMenuUI);
            Transform buttonPrefab = getTransformByNameAndRoot("Quit Button", SuperController.singleton.mainMenuUI);
            Transform textPrefab = getTransformByNameAndRoot("Text", SuperController.singleton.mainMenuUI);
            Image panelImagePrefab = panelPrefab.GetComponent<Image>();
            Image imagePrefab = buttonPrefab.GetComponent<Image>();
            RectTransform panelPrefabRT = panelPrefab.GetComponent<RectTransform>();
            RectTransform buttonPrefabRT = buttonPrefab.GetComponent<RectTransform>();
            string style = buttonPrefab.GetComponent<UIStyleButton>().styleName;
            RectTransform textPrefabRT = textPrefab.GetComponent<RectTransform>();

            //Create new gameobjects and associated UI components.
            GameObject mapLoadPanelGO = new GameObject("MapLoadPanel");
            mapLoadPanelGO.transform.localScale = panelPrefab.localScale;
            mapLoadPanelGO.transform.SetParent(panelPrefab.parent, false);

            Image mapLoadImage = mapLoadPanelGO.AddComponent<Image>();
            CopyImageValues(panelImagePrefab, mapLoadImage);
            RectTransform mapLoadPanelRT = mapLoadPanelGO.GetComponent<RectTransform>();
            CopyRectTransformValues(panelPrefabRT, mapLoadPanelRT);
            mapLoadPanelRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, mapLoadPanelRT.rect.width / 2f);
            ScrollRect scrollRect = mapLoadPanelGO.AddComponent<ScrollRect>();
            
            //scrollrect>viewport(mask+image)>content(button*100)
            GameObject mapLoadViewPortGO = new GameObject("MapLoadViewPort");
            mapLoadViewPortGO.transform.localScale = panelPrefab.localScale;
            mapLoadViewPortGO.transform.SetParent(mapLoadPanelGO.transform, false);
            Image mapLoadVPImage = mapLoadViewPortGO.AddComponent<Image>();

            // CopyImageValues(panelImagePrefab, mapLoadVPImage);                            
            Mask mapLoadMask = mapLoadViewPortGO.AddComponent<Mask>();

            RectTransform mapLoadViewPortRT = mapLoadViewPortGO.GetComponent<RectTransform>();
            CopyRectTransformValues(panelPrefabRT, mapLoadViewPortRT);

            GameObject mapLoadContentGO = new GameObject("MapLoadContent");
            mapLoadContentGO.transform.localScale = panelPrefab.localScale;
            mapLoadContentGO.transform.SetParent(mapLoadViewPortGO.transform, false);

            VerticalLayoutGroup vlg = mapLoadContentGO.AddComponent<VerticalLayoutGroup>();
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;

            RectTransform mapLoadContentRT = mapLoadContentGO.GetComponent<RectTransform>();
            CopyRectTransformValues(panelPrefabRT, mapLoadContentRT);
            mapLoadContentRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0f);            
            scrollRect.horizontal = false;

            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.viewport = mapLoadViewPortGO.GetComponent<RectTransform>();
            scrollRect.content = mapLoadContentGO.GetComponent<RectTransform>();

            mapLoadPanelRT.anchoredPosition = new Vector2(mapLoadPanelRT.anchoredPosition.x + (mapLoadPanelRT.rect.width*1.5f), mapLoadPanelRT.anchoredPosition.y);

            return scrollRect;
        }

        Button createMenuButton(string text)
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
            mapLoadText.text = text;
            return mapLoadButtonUI;
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
        #endregion

        #region Utility and Bundle methods
        void fixLightsInScene()
        {
            Light[] lights = GameObject.FindObjectsOfType<Light>();

            foreach (Light light in lights)
            {
                NGSS_Directional ngLight = light.gameObject.AddComponent<NGSS_Directional>();

            }

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

        public static AssetBundle getBundle(string bundlePath)
        {
            AssetBundle asb = null;
            try
            {
                if (MapLoaderPlugin.bundles.ContainsKey(bundlePath))
                    asb = MapLoaderPlugin.bundles[bundlePath];
                else
                {
                    asb = AssetBundle.LoadFromFile(bundlePath);
                    MapLoaderPlugin.bundles.Add(bundlePath, asb);
                }
            }
            catch (Exception ex) { SuperController.LogError("Unable to load asset bundle " + bundlePath); }
            return asb;
        }

        public static void unloadBundle(string bundlePath)
        {
            AssetBundle asb = null;
            try
            {
                if (MapLoaderPlugin.bundles.ContainsKey(bundlePath))
                {
                    asb = MapLoaderPlugin.bundles[bundlePath];
                    asb.Unload(false);
                }
            }
            catch (Exception ex) { SuperController.LogError("Unable to unload asset bundle " + bundlePath); }

        }
        #endregion
    }


}



