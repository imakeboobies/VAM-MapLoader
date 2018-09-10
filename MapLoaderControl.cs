using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VAM_MapLoader
{
    class MapLoaderControl : JSONStorable
    {
        public AvailableMap am;
        public const string mapControlAtom = "MapLoaderControl";
        public Atom controlAtom;

        public void onAtomAdd(List<string> atoms)
        {
            if (controlAtom == null)
            {
                foreach (string atomname in atoms)
                {
                    if (atomname.Trim().Equals(mapControlAtom))
                    {
                        controlAtom = SuperController.singleton.GetAtomByUid(atomname);
                    }
                }
            }
        }

        protected override void Awake()
        {
            if (awakecalled)
                return;

            base.Awake();

            name = "MapLoaderControl";
            exclude = false;
            onlyStoreIfActive = false;

        }

        public void Start()
        {
            SuperController.singleton.onAtomUIDsChangedHandlers += onAtomAdd;

            controlAtom = SuperController.singleton.GetAtomByUid(mapControlAtom);

            if (controlAtom == null)
            {
                foreach (SuperController.AtomAsset atomm in SuperController.singleton.atomAssets)
                {
                    if (atomm.assetName.Equals("CustomUnityAsset"))
                    {
                        StartCoroutine(SuperController.singleton.LoadAtomFromBundleAsync(atomm, mapControlAtom, false));
                    }
                }
            }
        }



        public override JSONClass GetJSON(bool includePhysical = true, bool includeAppearance = true)
        {
            if (!awakecalled)
                Awake();

            JSONClass json = base.GetJSON(includePhysical, includeAppearance);
            json["loaderType"] = am.loaderType;
            json["fileName"] = am.fileName;
            json["displayName"] = am.displayName;

            int i = 0;
            foreach (string paras in am.parameters)
            {
                json["parameters"][i] = paras.TrimEnd('\r', '\n');
                i++;
            }

            needsStore = true;
            return json;
        }

        public override void RestoreFromJSON(JSONClass json, bool restorePhysical = true, bool restoreAppearance = true, JSONArray presetAtoms = null)
        {
            base.RestoreFromJSON(json, restorePhysical, restoreAppearance, presetAtoms);

            string loaderType = json["loaderType"];
            string fileName = json["fileName"];
            string displayName = json["displayName"];

            JSONArray para = json["parameters"].AsArray;

            List<string> parameters = new List<string>();

            if (para.Count > 0)
            {
                for (int i = 0; i < para.Count; i++)
                {
                    string val = para[i].ToString().TrimEnd('\r', '\n');
                    val = val.Replace("\"", "");
                    parameters.Add(val);
                }
            }

            if (loaderType != null && loaderType.Length > 0)
            {

                AvailableMap am = new AvailableMap(fileName.TrimEnd('\r', '\n'), displayName.TrimEnd('\r', '\n'), loaderType.TrimEnd('\r', '\n'), parameters);

                MapLoaderPlugin.Instance.LoadMap(am, loaderType);

            }



        }

    }
}
