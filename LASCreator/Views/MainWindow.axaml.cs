using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Avalonia.Controls;

namespace LASCreator.Views
{
    public partial class MainWindow : Window
    {
        public static MainWindow? Instance { get; private set; }

        public static readonly List<FileDialogFilter> modelFilter = new() { new FileDialogFilter() { Name = "Diesel Model", Extensions = new() { "model" } } };
        public static readonly List<FileDialogFilter> textureFilter = new() { new FileDialogFilter() { Name = "Texture", Extensions = new() { "dds" } } };

        private readonly Dictionary<int, Material> materials = new();

        private int newKey = 0;

        public MainWindow()
        {
            InitializeComponent();

            CanResize = false;
            Width = 1200;
            Height = 680;
            MinWidth = Width;
            MaxWidth = Width;
            MinHeight = Height;
            MaxHeight = Height;

            TppModel.Filters = modelFilter;
            FppModel.Filters = modelFilter;

            Instance = this;
        }

        private void Add()
        {
            Material material = new(newKey);
            materials.Add(newKey, material);
            MaterialsList.Children.Insert(MaterialsList.Children.Count - 1, material);
            newKey++;
        }

        public void Remove(int key)
        {
            materials.Remove(key);
            foreach (Material material in MaterialsList.Children)
            {
                if (material.Key == key)
                {
                    MaterialsList.Children.Remove(material);
                    break;
                }
            }
        }

        private void Create()
        {
            string internalName = InternalName.Text;
            string outfitName = OutfitName.Text;
            string outfitDescription = OutfitDescription.Text;
            string modName = ModName.Text;
            string tppModel = TppModel.Path;
            string fppModel = FppModel.Path;
            string icon = Icon.Path;
            string saveTo = SaveTo.Path;

            if (internalName == null || internalName.Length == 0 ||
                outfitName == null || outfitName.Length == 0 ||
                outfitDescription == null || outfitDescription.Length == 0 ||
                modName == null || modName.Length == 0 ||
                tppModel == null || tppModel.Length == 0 ||
                TppObjects.Text == null || TppObjects.Text.Length == 0 ||
                saveTo == null || saveTo.Length == 0)
            {
                new PopUp("Error", "Make sure to fill every non optional field").ShowDialog(this);
                return;
            }

            string[] tppObjects = TppObjects.Text.Trim().Split(',');
            string[] fppObjects = Array.Empty<string>();

            if (fppModel.Length > 0)
            {
                if (FppObjects.Text == null || FppObjects.Text.Length == 0)
                {
                    new PopUp("Error", "Make sure to fill FPP Model Objects").ShowDialog(this);
                    return;
                }
                fppObjects = FppObjects.Text.Trim().Split(',');
            }

            if (File.Exists($"{saveTo}/main.xml"))
            {
                Directory.Delete(saveTo, true);
                Directory.CreateDirectory(saveTo);
            }

            #region main.xml
            Dictionary<string, string> textures = new();

            int index = 0;

            foreach (Material material in materials.Values)
            {
                if (material.MaterialName.Text == null || material.MaterialName.Text.Length == 0)
                {
                    new PopUp("Error", $"Material name is missing").ShowDialog(this);
                    return;
                }

                if (material.MaterialDiffuse.Path == null || material.MaterialDiffuse.Path.Length == 0)
                {
                    new PopUp("Error", $"Material {material.MaterialName.Text} is missing a diffuse map").ShowDialog(this);
                    return;
                }

                if (!textures.ContainsKey(material.MaterialDiffuse.Path)) textures.Add(material.MaterialDiffuse.Path, $"texture_df_{index}");
                if (!textures.ContainsKey(material.MaterialNormal.Path) && material.MaterialNormal.Path.Length > 0) textures.Add(material.MaterialNormal.Path, $"texture_nm_{index}");
                if (!textures.ContainsKey(material.MaterialIllumination.Path) && material.MaterialIllumination.Path.Length > 0) textures.Add(material.MaterialIllumination.Path, $"texture_il_{index}");
                
                index++;
            }

            string addFiles = "";

            foreach (string texture in textures.Values)
            {
                addFiles += $"<dds path=\"units/{internalName}/armor_skins/{internalName}/{texture}\"/>\n";
            }

            string mainIcon = "";
            if (icon.Length > 0)
            {
                string iconPath = $"guis/dlcs/{internalName}/armor_skins";
                Directory.CreateDirectory($"{saveTo}/assets/{iconPath}");
                File.Copy(icon, $"{saveTo}/assets/{iconPath}/{internalName}.dds");
                mainIcon = $"{iconPath}/{internalName}";
                addFiles += $"<dds path=\"{iconPath}/{internalName}\"/>\n";
            }

            addFiles += $"<unit_mat path=\"units/{internalName}/armor_skins/{internalName}/{internalName}\"/>";

            if (fppModel.Length > 0) addFiles += $"\n<unit_mat path=\"units/{internalName}/armor_skins/{internalName}/{internalName}_fpp\"/>";

            StreamWriter main = new($"{saveTo}/main.xml");
            StreamReader stream = new("Sources/main.xml");
            main.Write(stream.ReadToEnd(), modName, mainIcon, addFiles);
            stream.Close();
            main.Close();
            #endregion

            #region english.txt
            Directory.CreateDirectory($"{saveTo}/localization");

            StreamWriter english = new($"{saveTo}/localization/english.txt");
            stream = new("Sources/english.txt");
            english.Write(stream.ReadToEnd(), internalName, outfitName, outfitDescription);
            stream.Close();
            english.Close();
            #endregion

            #region hook.lua
            Directory.CreateDirectory($"{saveTo}/hooks");

            string fpp = "";
            if (fppModel.Length > 0) fpp = $"fps_unit = \"units/{internalName}/armor_skins/{internalName}/{internalName}_fpp\",\nfps_hide_armor = true,\nfps_replace_body = true";

            StreamWriter hook = new($"{saveTo}/hooks/hook.lua");
            stream = new("Sources/hook.lua");
            hook.Write(stream.ReadToEnd(), internalName, $"units/{internalName}/armor_skins/{internalName}/{internalName}", fpp);
            stream.Close();
            hook.Close();
            #endregion

            #region copy textures
            string unitPath = $"{saveTo}/assets/units/{internalName}/armor_skins/{internalName}";
            Directory.CreateDirectory(unitPath);

            foreach (KeyValuePair<string, string> texture in textures)
                File.Copy(texture.Key, $"{unitPath}/{texture.Value}.dds");
            #endregion

            #region copy models
            File.Copy(tppModel, $"{unitPath}/{internalName}.model");
            if (fppModel.Length > 0) File.Copy(fppModel, $"{unitPath}/{internalName}_fpp.model");
            #endregion

            #region character.unit
            stream = new("Sources/character.unit");
            string text = stream.ReadToEnd();
            stream.Close();

            StreamWriter characterUnit = new($"{unitPath}/{internalName}.unit");
            characterUnit.Write(text, internalName, "");
            if (fppModel.Length > 0)
            {
                StreamWriter characterFppUnit = new($"{unitPath}/{internalName}_fpp.unit");
                characterFppUnit.Write(text, internalName, "_fpp");
                characterFppUnit.Close();
            }
            characterUnit.Close();
            #endregion

            #region character.object
            stream = new("Sources/character.object");
            text = stream.ReadToEnd();
            stream.Close();

            StreamWriter characterObject = new($"{unitPath}/{internalName}.object");
            string tppLines = "";
            foreach (string item in tppObjects)
                tppLines += $"<object name=\"{item}\" enabled=\"true\" shadow_caster=\"true\"/>\n";
            characterObject.Write(text, internalName, "", tppLines);
            characterObject.Close();

            if (fppModel.Length > 0)
            {
                StreamWriter characterFppObject = new($"{unitPath}/{internalName}_fpp.object");
                string fppLines = "";
                foreach (string item in fppObjects)
                    fppLines += $"<object name=\"{item}\" enabled=\"true\"/>\n";
                characterFppObject.Write(text, internalName, "_fpp", fppLines);
                characterFppObject.Close();
            }
            #endregion

            #region character.material_config
            string mat = "<material name=\"{0}\" render_template=\"generic:CONTOUR:DIFFUSE_TEXTURE:NORMALMAP:SKINNED_3WEIGHTS\" unique=\"true\" version=\"2\">\n<diffuse_texture file=\"{1}\"/>\n<bump_normal_texture file=\"{2}\"/>\n<variable type=\"vector3\" name=\"contour_color\" value=\"1 1 1\"/>\n<variable type=\"scalar\" name=\"contour_opacity\" value=\"0\"/>\n</material>\n";
            string ilMat = "<material name=\"{0}\" render_template=\"generic:CONTOUR:DIFFUSE_TEXTURE:NORMALMAP:SELF_ILLUMINATION:SELF_ILLUMINATION_BLOOM:SKINNED_3WEIGHTS\" unique=\"true\" version=\"2\">\n<diffuse_texture file=\"{1}\"/>\n<bump_normal_texture file=\"{2}\"/>\n<self_illumination_texture file=\"{3}\"/>\n<variable type=\"vector3\" name=\"contour_color\" value=\"1 1 1\"/>\n<variable type=\"scalar\" name=\"contour_opacity\" value=\"0\"/>\n<variable type=\"scalar\" name=\"il_bloom\" value=\"10\"/>\n<variable type=\"scalar\" name=\"il_multiplier\" value=\"1\"/>\n</material>\n";

            string matFpp = "<material name=\"{0}\" render_template=\"generic:DEPTH_SCALING:DIFFUSE_TEXTURE:NORMALMAP:SKINNED_3WEIGHTS\" unique=\"true\" version=\"2\">\n<diffuse_texture file=\"{1}\"/>\n<bump_normal_texture file=\"{2}\"/>\n</material>\n";
            string ilMatFpp = "<material name=\"{0}\" render_template=\"generic:DEPTH_SCALING:DIFFUSE_TEXTURE:NORMALMAP:SELF_ILLUMINATION:SELF_ILLUMINATION_BLOOM:SKINNED_3WEIGHTS\r\n\" unique=\"true\" version=\"2\">\n<diffuse_texture file=\"{1}\"/>\n<bump_normal_texture file=\"{2}\"/>\n<self_illumination_texture file=\"{3}\"/>\n<variable type=\"scalar\" name=\"il_bloom\" value=\"10\"/>\n<variable type=\"scalar\" name=\"il_multiplier\" value=\"1\"/>\n</material>\n";

            string mats = "";
            string matsFpp = "";

            foreach (Material material in materials.Values)
            {
                string template = mat;

                if (material.FppCheck.IsChecked != null && (bool)material.FppCheck.IsChecked)
                {
                    if (material.MaterialIllumination.Path.Length == 0) template = matFpp;
                    else template = ilMatFpp;
                }
                else
                {
                    if (material.MaterialIllumination.Path.Length == 0) template = mat;
                    else template = ilMat;
                }

                string addTo = "";
                string normal = "units/dev_tools/tangent_test/blank_nm";

                if (material.MaterialNormal.Path.Length != 0) normal = $"units/{internalName}/armor_skins/{internalName}/{textures[material.MaterialNormal.Path]}";
                
                if (material.MaterialIllumination.Path.Length == 0)
                    addTo = string.Format(
                        template,
                        material.MaterialName.Text,
                        $"units/{internalName}/armor_skins/{internalName}/{textures[material.MaterialDiffuse.Path]}",
                        normal
                        );
                else
                    addTo = string.Format(
                        template,
                        material.MaterialName.Text,
                        $"units/{internalName}/armor_skins/{internalName}/{textures[material.MaterialDiffuse.Path]}",
                        normal,
                        $"units/{internalName}/armor_skins/{internalName}/{textures[material.MaterialIllumination.Path]}"
                        );

                if (material.FppCheck.IsChecked != null && (bool)material.FppCheck.IsChecked) matsFpp += addTo;
                else mats += addTo;
            }

            stream = new("Sources/character.material_config");

            StreamWriter characterMaterialConfig = new($"{unitPath}/{internalName}.material_config");
            characterMaterialConfig.Write(stream.ReadToEnd(), mats);
            characterMaterialConfig.Close();

            if (fppModel.Length > 0)
            {
                stream.DiscardBufferedData();
                stream.BaseStream.Seek(0, SeekOrigin.Begin);
                StreamWriter characterFppMaterialConfig = new($"{unitPath}/{internalName}_fpp.material_config");
                characterFppMaterialConfig.Write(stream.ReadToEnd(), matsFpp);
                characterFppMaterialConfig.Close();
            }

            stream.Close();
            #endregion

            new PopUp("Done", "Mod created successfully").ShowDialog(this);
        }
    }
}
