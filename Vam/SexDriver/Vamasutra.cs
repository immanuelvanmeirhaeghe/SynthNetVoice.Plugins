using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using SimpleJSON;
using VAM_ScriptEngine;

namespace VAMDeluxe
{
    public class Vamasutra
    {
        Dropdown dropdown;
        string[] scenes;

        public Vamasutra()
        {
            GameObject dropdownGO = GameObject.Find("Positions Dropdown");
            if (dropdownGO != null)
            {
                dropdown = dropdownGO.GetComponent<Dropdown>();
                dropdown.onValueChanged.RemoveAllListeners();
                dropdown.onValueChanged.AddListener(OnSelected);
            }

            string vamasutraPath = SexDriver.SAVEPATH + "/VAMasutra/";
            scenes =  Directory.GetFiles(vamasutraPath, "*.json");
            List<string> sceneNames = new List<string>();
            List<Sprite> sprites = new List<Sprite>();
            foreach(string scenePath in scenes)
            {
                string filename = Path.GetFileNameWithoutExtension(scenePath);
                string name = FirstLetterToUpper(filename);

                sceneNames.Add(name);
                string thumbnailPath = vamasutraPath + filename + ".jpg";
                if (File.Exists(thumbnailPath))
                {
                    byte[] data = File.ReadAllBytes(thumbnailPath);
                    Texture2D texture = new Texture2D(512, 512, TextureFormat.ARGB32, false);
                    texture.LoadImage(data);
                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0));
                    sprites.Add(sprite);
                }
                else
                {
                    sprites.Add(null);
                }
            }

            List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
            for(int i=0; i<sceneNames.Count; i++)
            {
                string name = sceneNames[i];
                Sprite sprite = sprites[i];

                Dropdown.OptionData data;
                if (sprite != null)
                {
                    data = new Dropdown.OptionData(name,sprite);
                }
                else
                {
                    data = new Dropdown.OptionData(name);
                }

                options.Add(data);
            }

            dropdown.ClearOptions();
            dropdown.AddOptions(options);

            string facialPath = SexDriver.SAVEPATH + "./VAMasutra";

            /*
            StreamReader jsonFile = new StreamReader(facialPath);
            string jsonString = jsonFile.ReadToEnd();
            jsonFile.Close();
            var loadJson = JSON.Parse(jsonString);
            */
        }

        private void OnSelected(int id){
            SelectPosition(scenes[id]);
        }

        private void SelectPosition(string saveName)
        {

            StreamReader streamReader = new StreamReader(saveName);
            string aJSON = streamReader.ReadToEnd();
            streamReader.Close();
            JSONNode jSONNode = JSON.Parse(aJSON);
            JSONArray asArray = jSONNode["atoms"].AsArray;

            for(int i = 0; i < asArray.Count; i++)
            {
                JSONClass asObject = asArray[i].AsObject;
                string id = asObject["id"];
                string type = asObject["type"];
                if (type == "Person" && Utils.GetAtom(id) !=null)
                {
                    Atom person = Utils.GetAtom(id);
                    person.PreRestore();
                    person.RestoreTransform(asObject);
                    person.Restore(asObject, true, false, false, asArray);
                    person.LateRestore(asObject, true, false, false);
                    person.PostRestore();
                }
            }
            if (SuperController.singleton != null)
            {
                SuperController.singleton.PauseSimulation(5, "Loading Position " + saveName);
            }
        }

        public string FirstLetterToUpper(string str)
        {
            if (str == null)
                return null;

            if (str.Length > 1)
                return char.ToUpper(str[0]) + str.Substring(1);

            return str.ToUpper();
        }

    }
}
