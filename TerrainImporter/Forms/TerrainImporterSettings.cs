using NWN2Toolset;
using NWN2Toolset.NWN2.Data.Blueprints;
using NWN2Toolset.NWN2.Data.Templates;
using OEIShared.IO;
using OEIShared.IO.TalkTable;
using OEIShared.IO.TwoDA;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using TerrainImporter.Model;

namespace TerrainImporter.Forms
{
    public partial class TerrainImporterSettings : Form
    {
        // Main map files
        public HeightMap heightMap;
        public WaterMap waterMap;
        public AttributeMap attributeMap;

        // Private members
        private bool canceled = true;

        private List<string> paintTextureNames = new List<string>();
        private ArrayList paintGrassNames;
        private ArrayList paintTreesList;
        private ImageList paintTexturesImageList = null;
        private ImageList paintGrassImageList = null;

        // Public members
        public Hashtable fullAttributeTable;
        public Hashtable paintTextures;
        public Hashtable paintGrass;
        public Hashtable paintTrees;

        // Public methods

        public TerrainImporterSettings()
        {
            InitializeComponent();
            loadStaticData();
        }

        public bool isCanceled()
        {
            return canceled;
        }

        // Private methods

        private void loadStaticData()
        {
            generatePaintTextureNamesList();
            generatePaintGrassNamesList();
            generatePaintTexturesList();
            generatePaintGrassList();
            generatePaintTreesList();
        }

        private void generatePaintTextureNamesList()
        {
            TwoDAColumn twoDaColumn1 = TwoDAManager.Instance.Get("terrainMaterials").Columns["Terrain"];
            for (int index = 0; index < twoDaColumn1.Count; ++index)
            {
                this.paintTextureNames.Add(twoDaColumn1[index]);
            }
        }

        private void generatePaintTexturesList()
        {
            this.paintTexturesImageList = new ImageList();
            this.paintTexturesImageList.ColorDepth = ColorDepth.Depth24Bit;
            this.paintTexturesImageList.ImageSize = new Size(64, 64);
            foreach (string paintTextureName in this.paintTextureNames)
            {
                string path = findPath(paintTextureName);
                if (path != null)
                {
                    Stream stream = (Stream)File.OpenRead(path);
                    this.paintTexturesImageList.Images.Add(paintTextureName, (Image)new Bitmap(stream));
                    stream.Close();
                    this.textureListView.Items.Add(paintTextureName, paintTextureName, this.paintTexturesImageList.Images.IndexOfKey(paintTextureName));
                }
            }

            this.textureListView.LargeImageList = this.paintTexturesImageList;
        }

        private void generatePaintGrassNamesList()
        {
            this.paintGrassNames = new ArrayList();
            TwoDAColumn twoDaColumn2 = ((TwoDAManager)TwoDAManager.Instance).Get("grass").Columns["Texture"];
            for (int index = 0; index < twoDaColumn2.Count; ++index)
            {
                this.paintGrassNames.Add(twoDaColumn2[index]);
            }
        }

        private void generatePaintTreesList()
        {
            this.paintTreesList = new ArrayList();
            foreach (NWN2TreeBlueprint nwN2TreeBlueprint in (CollectionBase)NWN2GlobalBlueprintManager.Instance.Trees)
            {
                this.paintTreesList.Add(nwN2TreeBlueprint);
                if (!this.treeToolBox.Nodes.ContainsKey(((NWN2TreeTemplate)nwN2TreeBlueprint).Classification))
                {
                    // I have no idea what this is and why there is an error but I'll try to fix it
                    string treeBlueprint = ((NWN2TreeTemplate)nwN2TreeBlueprint).Classification;
                    TalkTableElement talkTableElement = TalkTable.Instance[Convert.ToUInt32(treeBlueprint.Substring(1, treeBlueprint.Length - 2)), OEIShared.Utils.BWLanguages.Gender.Male];
                    this.treeToolBox.Nodes.Add(treeBlueprint, talkTableElement.String);
                }


                this.treeToolBox.Nodes[this.treeToolBox.Nodes.IndexOfKey(((NWN2TreeTemplate)nwN2TreeBlueprint).Classification)].Nodes.Add(new TreeNode(((NWN2TreeTemplate)nwN2TreeBlueprint).Name)
                {
                    Tag = nwN2TreeBlueprint
                });
            }
        }

        private void generatePaintGrassList()
        {
            this.paintGrassImageList = new ImageList();
            this.paintGrassImageList.ColorDepth = ColorDepth.Depth24Bit;
            this.paintGrassImageList.ImageSize = new Size(64, 64);
            foreach (string paintGrassName in this.paintGrassNames)
            {
                string path = findPath(paintGrassName);
                if (path != null)
                {
                    Stream stream = (Stream)File.OpenRead(path);
                    this.paintGrassImageList.Images.Add(paintGrassName, (Image)new Bitmap(stream));
                    stream.Close();
                    this.grassListView.Items.Add(paintGrassName, paintGrassName, this.paintGrassImageList.Images.IndexOfKey(paintGrassName));
                }
            }

            this.grassListView.LargeImageList = this.paintGrassImageList;
        }

        private string findPath(string key)
        {
            List<string> pathList = new List<string>(6) 
            { 
                "\\NWN2Toolset\\Terrain\\", "\\NWN2Toolset_X1\\Terrain\\", "\\NWN2Toolset_X2\\Terrain\\" ,
                "\\NWN2Toolset\\Grass\\", "\\NWN2Toolset_X1\\Grass\\", "\\NWN2Toolset_X2\\Grass\\"
            };

            foreach (string path in pathList)
            {
                string checkpath = ResourceManager.Instance.BaseDirectory + path + key + ".bmp";
                if (File.Exists(checkpath))
                {
                    return checkpath;
                }
            }
            return null;
        }

        private void handleBrowseHeightmapImgFile()
        {
            Stream inf;
            string selectedFileName = this.heightMapOpenFileDialog.FileName;

            if ((FileStream)(inf = (Stream)File.Open(selectedFileName, FileMode.Open)) == null)
            {
                throw new FileNotFoundException(selectedFileName);
            }

            this.heightMap = (HeightMap)new BmpHeightMap(inf);
            this.upperLeftX.Maximum = this.lowerRightX.Maximum = this.lowerRightX.Value = (Decimal)this.heightMap.Width;
            this.upperLeftY.Maximum = this.lowerRightY.Maximum = this.lowerRightY.Value = (Decimal)this.heightMap.Height;
            inf.Close();
        }

        private void handleBrowseHeightmapProjFile()
        {
            Stream inStream1;
            XmlDocument projectFile;
            string projectFileName = this.heightMapOpenFileDialog.FileName;

            if ((inStream1 = this.heightMapOpenFileDialog.OpenFile()) != null)
            {
                //reset any lists for this new file
                this.fullAttributeTable = new Hashtable();
                this.terrainLandTypeComboBox.Items.Clear();
                this.grassLandTypeComboBox.Items.Clear();
                this.treeLandTypeComboBox.Items.Clear();

                //load L3DT project file xml in XmlDocument
                projectFile = new XmlDocument();
                projectFile.Load(inStream1);
                inStream1.Close();

                //find the heightmap and load it into this.heightMap
                importHeightMapSettings(projectFileName, projectFile);

                //find the watermap and load it into this.waterMap
                importWaterMapSettings(projectFileName, projectFile);

                //load the climate id list 
                importClimateSettings(projectFile);

                //load the attribute map
                importAttributeSettings(projectFileName, projectFile);

                //make sure the first element is selected in each combobox
                this.terrainLandTypeComboBox.SelectedIndex = 0;
                this.grassLandTypeComboBox.SelectedIndex = 0;
                this.treeLandTypeComboBox.SelectedIndex = 0;
            }
        }

        private void importHeightMapSettings(string projectFileName, XmlDocument projectFile)
        {
            string hfName = projectFile.SelectSingleNode("/varlist[@name='ProjectData']/varlist[@name='Maps']/varlist[@name='HF']/string[@name='Filename']").InnerText;
            string heightMapFileName = projectFileName.Substring(0, projectFileName.LastIndexOf('\\') + 1) + hfName;
            loadHeightMap(heightMapFileName);

            this.upperLeftX.Maximum = this.lowerRightX.Maximum = this.lowerRightX.Value = (Decimal)this.heightMap.Width;
            this.upperLeftY.Maximum = this.lowerRightY.Maximum = this.lowerRightY.Value = (Decimal)this.heightMap.Height;
        }

        private void loadHeightMap(string heightMapFileName)
        {
            if (heightMapFileName.EndsWith("hfz") || heightMapFileName.EndsWith("hf2.gz") || heightMapFileName.EndsWith("hf2"))
            {
                this.heightMap = (HeightMap)new Hf2HeightMap(heightMapFileName);
            }
            else if (heightMapFileName.EndsWith("hff"))
            {
                this.heightMap = (HeightMap)new HffHeightMap(heightMapFileName);
            }
        }

        private void importWaterMapSettings(string projectFileName, XmlDocument projectFile)
        {
            string waterMapFileName = projectFileName.Substring(0, projectFileName.LastIndexOf('\\') + 1) + projectFile.SelectSingleNode("/varlist[@name='ProjectData']/varlist[@name='Maps']/varlist[@name='WM']/string[@name='Filename']").InnerText;
            this.waterMap = new WmfWaterMap(waterMapFileName);
        }

        private void importClimateSettings(XmlDocument projectFile)
        {
            byte climateId = 0;
            foreach (XmlNode xmlNode in projectFile.SelectNodes("/varlist[@name='ProjectData']/varlist[@name='Climates']/varlist"))
            {
                Stream climateFileStream = (Stream)File.Open(xmlNode.InnerText, FileMode.Open);
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(climateFileStream);
                climateFileStream.Close();

                byte landTypeId = 1;
                foreach (XmlElement xmlElement in xmlDocument.SelectNodes("/varlist/varlist[@name='LandTypeList']/varlist"))
                {
                    string key = climateId + "-" + landTypeId;
                    if (this.fullAttributeTable[key] == null)
                    {
                        this.fullAttributeTable.Add(key, xmlElement.Attributes["name"].Value);
                    }
                    landTypeId++;
                }
                climateId++;
            }
        }

        private void importAttributeSettings(string projectFileName, XmlDocument projectFile)
        {
            string filename3 = projectFileName.Substring(0, projectFileName.LastIndexOf('\\') + 1) + projectFile.SelectSingleNode("/varlist[@name='ProjectData']/varlist[@name='Maps']/varlist[@name='AM']/string[@name='Filename']").InnerText;
            if (filename3.EndsWith(".amf") || filename3.EndsWith(".amf.gz"))
            {
                this.attributeMap = new AmfAttributeMap(filename3);
                populatePixelAttributes();
            }
        }

        private void populatePixelAttributes()
        {
            AmfAttributeMap amfAttributeMap = (AmfAttributeMap)this.attributeMap;
            ArrayList arrayList = new ArrayList();
            this.paintTextures = new Hashtable();
            this.paintGrass = new Hashtable();
            this.paintTrees = new Hashtable();

            List<string> landTypes = amfAttributeMap.getAllLandTypes();
            foreach (string landType in landTypes)
            {
                if (this.fullAttributeTable[landType] != null && this.paintTextures[this.fullAttributeTable[landType]] == null)
                {
                    this.paintTextures.Add(this.fullAttributeTable[landType], this.paintTextureNames[0]);
                    this.paintGrass.Add(this.fullAttributeTable[landType], createGrassSettings());
                    this.paintTrees.Add(this.fullAttributeTable[landType], createTreeSettings());
                    this.terrainLandTypeComboBox.Items.Add(this.fullAttributeTable[landType]);
                    this.grassLandTypeComboBox.Items.Add(this.fullAttributeTable[landType]);
                    this.treeLandTypeComboBox.Items.Add(this.fullAttributeTable[landType]);
                }
            }
        }

        private Hashtable createTreeSettings()
        {
            Hashtable treeSettings = new Hashtable();
            treeSettings["density"] = new Decimal(5);
            treeSettings["doTrees"] = false;
            treeSettings["treeSettings"] = new ArrayList();
            return treeSettings;
        }

        private Hashtable createGrassSettings()
        {
            Hashtable attributeSettings = new Hashtable();
            attributeSettings["textures"] = new ArrayList() { this.paintGrassNames[0] };
            attributeSettings["innerRadius"] = new Decimal(1);
            attributeSettings["outerRadius"] = new Decimal(6);
            attributeSettings["pressure"] = 0.15f;
            attributeSettings["bladeSize"] = 1.5f;
            attributeSettings["bladeSizeVariation"] = 0.0f;
            attributeSettings["doPaint"] = false;
            return attributeSettings;
        }

        private void enableTabs()
        {
            this.terrainTab.Enabled = true;
        }

        private void toggleControls(Control control, bool state)
        {
            foreach (Control c in control.Controls)
            {
                c.Enabled = state;
                if (c is Control)
                {
                    toggleControls(c, state);
                }
            }
        }

        private void clearTreeInfo()
        {
            this.treeSeedTextBox.Text = "";
            this.treeWeightTextBox.Text = "";
            this.treeMinX.Text = "";
            this.treeMinY.Text = "";
            this.treeMinZ.Text = "";
            this.treeMaxX.Text = "";
            this.treeMaxY.Text = "";
            this.treeMaxZ.Text = "";

            this.treeSeedTextBox.Enabled = false;
            this.treeWeightTextBox.Enabled = false;
            this.treeMinX.Enabled = false;
            this.treeMinY.Enabled = false;
            this.treeMinZ.Enabled = false;
            this.treeMaxX.Enabled = false;
            this.treeMaxY.Enabled = false;
            this.treeMaxZ.Enabled = false;
        }

        // Control actions

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.canceled = true;
            this.Close();
        }

        private void importButton_Click(object sender, EventArgs e)
        {
            this.canceled = false;
            this.Close();
        }

        private void heightMapBrowseButton_Click(object sender, EventArgs e)
        {
            if (this.heightMapOpenFileDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            string selectedFileName = this.heightMapOpenFileDialog.FileName;
            this.heightMapFileNameTxtBox.Text = selectedFileName;
            if (selectedFileName.EndsWith(".proj"))
            {

                toggleControls(this.importSettingsTabControl.TabPages[0], true);
                toggleControls(this.importSettingsTabControl.TabPages[1], true);
                toggleControls(this.importSettingsTabControl.TabPages[2], true);
                toggleControls(this.importSettingsTabControl.TabPages[3], true);
                this.importButton.Enabled = true;
                handleBrowseHeightmapProjFile();
            }
            else
            {
                toggleControls(this.importSettingsTabControl.TabPages[0], true);
                this.importButton.Enabled = true;
                handleBrowseHeightmapImgFile();
            }
        }

        private void textureListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.textureListView.SelectedIndices.Count <= 0)
            {
                return;
            }
            string selectedTexture = this.textureListView.SelectedItems[0].Text;
            this.paintTextures[this.terrainLandTypeComboBox.Text] = selectedTexture;
            this.bigTextureBox.Image = this.paintTexturesImageList.Images[selectedTexture];
        }

        private void terrainLandTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.terrainLandTypeComboBox.SelectedIndex <= -1)
            {
                return;
            }
            string landType = this.terrainLandTypeComboBox.Text;
            this.textureListView.Focus();
            this.textureListView.Items[(string)this.paintTextures[landType]].Selected = true;
            this.bigTextureBox.Image = this.paintTexturesImageList.Images[(string)this.paintTextures[landType]];
        }

        private void grassLandTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.grassLandTypeComboBox.SelectedIndex <= -1)
            {
                return;
            }

            string landType = this.grassLandTypeComboBox.Text;
            this.grassListView.ItemSelectionChanged -= this.grassListView_ItemSelectionChanged;
            this.grassListView.SelectedItems.Clear();
            
            Hashtable paintGrassTable = (Hashtable)this.paintGrass[landType];
            ArrayList selectedTextures = (ArrayList)paintGrassTable["textures"];
            for (int i = 0; i < selectedTextures.Count; i++)
            {
                string index = (string)selectedTextures[i];
                this.grassListView.Items[index].Selected = true;
            }
            this.grassListView.ItemSelectionChanged += new ListViewItemSelectionChangedEventHandler(this.grassListView_ItemSelectionChanged);

            this.grassInnerRadius.Value = (Decimal)paintGrassTable["innerRadius"];
            this.grassOuterRadius.Value = (Decimal)paintGrassTable["outerRadius"];
            this.grassPressure.Value = Convert.ToDecimal((float)paintGrassTable["pressure"] * 100f);
            this.bladeSizeTextBox.Text = Convert.ToString(paintGrassTable["bladeSize"]);
            this.bladeSizeTrackBar.Value = Convert.ToInt32(Math.Round(((double)(float)paintGrassTable["bladeSize"] * 100)));
            this.bladeSizeVariationTextBox.Text = Convert.ToString(paintGrassTable["bladeSizeVariation"]);
            this.bladeSizeVariationTrackBar.Value = Convert.ToInt32(Math.Round((double)(float)paintGrassTable["bladeSizeVariation"] * 20.0));
            this.paintGrassCheckbox.Checked = (bool)paintGrassTable["doPaint"];
        }

        private void grassListView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            string landType = this.grassLandTypeComboBox.Text;
            if (this.grassListView.SelectedIndices.Count > 0 && this.grassListView.SelectedIndices.Count < 4)
            {
                MessageBox.Show(e.ToString());
                Hashtable paintGrassTable = (Hashtable)this.paintGrass[landType];
                ArrayList selectedGrassTextures = (ArrayList)paintGrassTable["textures"];
                selectedGrassTextures.Clear();
                foreach (ListViewItem listViewItem in this.grassListView.SelectedItems)
                {
                    selectedGrassTextures.Add(listViewItem.Text);
                }
                paintGrassTable["textures"] = selectedGrassTextures;
            }
            else
            {
                if (this.grassListView.SelectedIndices.Count <= 3)
                {
                    return;
                }
                MessageBox.Show("You cannot have more than 3 grasses selected for any given attribute type");
                e.Item.Selected = false;
            }            
        }        

        private void grassInnerRadius_ValueChanged(object sender, EventArgs e)
        {
            string landType = this.grassLandTypeComboBox.Text;
            ((Hashtable)this.paintGrass[landType])["innerRadius"] = this.grassInnerRadius.Value;
        }

        private void grassOuterRadius_ValueChanged(object sender, EventArgs e)
        {
            string landType = this.grassLandTypeComboBox.Text;
            ((Hashtable)this.paintGrass[landType])["outerRadius"] = this.grassOuterRadius.Value;
        }

        private void grassPressure_ValueChanged(object sender, EventArgs e)
        {
            string landType = this.grassLandTypeComboBox.Text;
            ((Hashtable)this.paintGrass[landType])["pressure"] = (float)((double)Convert.ToSingle(this.grassPressure.Value) / 100.0);
        }

        private void bladeSizeTrackBar_Scroll(object sender, EventArgs e)
        {
            string landType = this.grassLandTypeComboBox.Text;
            ((Hashtable)this.paintGrass[landType])["bladeSize"] = (float)((double)this.bladeSizeTrackBar.Value / 100);
            this.bladeSizeTextBox.Text = Convert.ToString(((Hashtable)this.paintGrass[landType])["bladeSize"]);
        }

        private void bladeSizeVariationTrackBar_Scroll(object sender, EventArgs e)
        {
            string landType = this.grassLandTypeComboBox.Text;
            ((Hashtable)this.paintGrass[landType])["bladeSizeVariation"] = (float)((double)this.bladeSizeVariationTrackBar.Value / 20.0);
            this.bladeSizeVariationTextBox.Text = Convert.ToString(((Hashtable)this.paintGrass[landType])["bladeSizeVariation"]);
        }

        private void paintGrassCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            string landType = this.grassLandTypeComboBox.Text;
            ((Hashtable)this.paintGrass[landType])["doPaint"] = (bool)(this.paintGrassCheckbox.Checked ? true : false);
        }

        private void treeLandTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.treeLandTypeComboBox.SelectedIndex <= -1)
            {
                return;
            }

            string landType = this.treeLandTypeComboBox.Text;
            Hashtable treesForLandType = ((Hashtable)this.paintTrees[landType]);
            this.selectedTreesBox.Items.Clear();

            clearTreeInfo();

            ArrayList selectedTrees = (ArrayList)treesForLandType["treeSettings"];
            for (int i = 0; i < selectedTrees.Count; i++)
            {
                TreeSetting treeSetting = (TreeSetting)selectedTrees[i];
                this.selectedTreesBox.Items.Add(((NWN2TreeTemplate)treeSetting.Tree).Name);
            }

            if (this.selectedTreesBox.Items.Count > 0)
            {
                this.treeRemoveButton.Enabled = true;
            }
            else
            {
                this.treeRemoveButton.Enabled = false;
            }

            this.treeDensity.Value = (Decimal)((Hashtable)this.paintTrees[landType])["density"];
            this.paintTreeCheckBox.Checked = (bool)((Hashtable)this.paintTrees[landType])["doTrees"];
        }

        private void treeDensity_ValueChanged(object sender, EventArgs e)
        {
            string landType = this.treeLandTypeComboBox.Text;
            ((Hashtable)this.paintTrees[landType])["density"] = this.treeDensity.Value;
        }

        private void paintTreeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            string landType = this.treeLandTypeComboBox.Text;
            ((Hashtable)this.paintTrees[landType])["doTrees"] = (bool)(this.paintTreeCheckBox.Checked ? true : false);
        }

        private void treeToolBox_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (this.treeToolBox.SelectedNode.Level == 0)
            {
                this.treeAddButton.Enabled = false;
            }
            else
            {
                this.treeAddButton.Enabled = true;
            }
        }

        private void treeAddButton_Click(object sender, EventArgs e)
        {
            string landType = this.treeLandTypeComboBox.Text;
            TreeSetting treeSetting = new TreeSetting();
            treeSetting.Tree = NWN2TreeBlueprint.CreateFromBlueprint((NWN2TreeBlueprint)this.treeToolBox.SelectedNode.Tag, (IResourceRepository)NWN2ToolsetMainForm.App.Module.Repository, false);
            treeSetting.Weight = (byte)1;
            for (int index = 0; index < 3; ++index)
            {
                treeSetting.Scale[index, 0] = 0.8f;
                treeSetting.Scale[index, 1] = 1.2f;
            }
            this.selectedTreesBox.Items.Add(((NWN2TreeTemplate)treeSetting.Tree).Name);
            ((ArrayList)((Hashtable)this.paintTrees[landType])["treeSettings"]).Add(treeSetting);
            this.treeRemoveButton.Enabled = true;

        }

        private void treeRemoveButton_Click(object sender, EventArgs e)
        {
            string landType = this.treeLandTypeComboBox.Text;
            ((ArrayList)((Hashtable)this.paintTrees[landType])["treeSettings"]).RemoveAt(this.selectedTreesBox.SelectedIndex);
            this.selectedTreesBox.Items.RemoveAt(this.selectedTreesBox.SelectedIndex);
            if (this.selectedTreesBox.Items.Count >= 1)
            {
                return;
            }
            this.treeRemoveButton.Enabled = false;
        }

        private void selectedTreesBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            string landType = this.treeLandTypeComboBox.Text;
            if (this.selectedTreesBox.SelectedIndex > -1)
            {
                TreeSetting treeSetting = (TreeSetting)((ArrayList)((Hashtable)this.paintTrees[landType])["treeSettings"])[this.selectedTreesBox.SelectedIndex];
                this.treeSeedTextBox.Enabled = true;
                this.treeSeedTextBox.Text = ((NWN2TreeTemplate)treeSetting.Tree).Seed.ToString();
                this.treeWeightTextBox.Enabled = true;
                this.treeWeightTextBox.Text = treeSetting.Weight.ToString();
                this.treeMinX.Enabled = this.treeMinY.Enabled = this.treeMinZ.Enabled = this.treeMaxX.Enabled = this.treeMaxY.Enabled = this.treeMaxZ.Enabled = true;

                this.treeMinX.Text = Convert.ToString(treeSetting.Scale[0, 0]);
                this.treeMaxX.Text = Convert.ToString(treeSetting.Scale[0, 1]);
                this.treeMinY.Text = Convert.ToString(treeSetting.Scale[1, 0]);
                this.treeMaxY.Text = Convert.ToString(treeSetting.Scale[1, 1]);
                this.treeMinZ.Text = Convert.ToString(treeSetting.Scale[2, 0]);
                this.treeMaxZ.Text = Convert.ToString(treeSetting.Scale[2, 1]);
            }
            else
            {
                this.treeSeedTextBox.Enabled = false;
                this.treeSeedTextBox.Text = "";
                this.treeWeightTextBox.Enabled = false;
                this.treeWeightTextBox.Text = "";
                this.treeMinX.Enabled = this.treeMinY.Enabled = this.treeMinZ.Enabled = this.treeMaxX.Enabled = this.treeMaxY.Enabled = this.treeMaxZ.Enabled = false;
            }
        }

        private void treeSeedTextBox_TextChanged(object sender, EventArgs e)
        {
            string landType = this.treeLandTypeComboBox.Text;
            if (this.selectedTreesBox.SelectedIndex <= -1 || !(this.treeSeedTextBox.Text != "-") || !(this.treeSeedTextBox.Text != "."))
            {
                return;
            }

            try
            {
                ((NWN2TreeTemplate)((TreeSetting)((ArrayList)((Hashtable)this.paintTrees[landType])["treeSettings"])[this.selectedTreesBox.SelectedIndex]).Tree).Seed = Convert.ToUInt32(this.treeSeedTextBox.Text);
            }
            catch (OverflowException)
            {
                MessageBox.Show("You have entered an invalid value.\r\nValue must be an integer value between " + 0 + " and " + uint.MaxValue + ".", "Bad Value", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                this.treeSeedTextBox.Text = ((NWN2TreeTemplate)((TreeSetting)((ArrayList)((Hashtable)this.paintTrees[landType])["treeSettings"])[this.selectedTreesBox.SelectedIndex]).Tree).Seed.ToString();
            }
            catch (FormatException)
            {
                MessageBox.Show("This field requires a numeric value.", "Non-numeric Value", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                this.treeSeedTextBox.Text = ((NWN2TreeTemplate)((TreeSetting)((ArrayList)((Hashtable)this.paintTrees[landType])["treeSettings"])[this.selectedTreesBox.SelectedIndex]).Tree).Seed.ToString();
            }
        }

        private void treeWeightTextBox_TextChanged(object sender, EventArgs e)
        {
            string landType = this.treeLandTypeComboBox.Text;
            if (this.selectedTreesBox.SelectedIndex <= -1 || !(this.treeWeightTextBox.Text != "-") || !(this.treeWeightTextBox.Text != "."))
            {
                return;
            }

            try
            {
                ((TreeSetting)((ArrayList)((Hashtable)this.paintTrees[landType])["treeSettings"])[this.selectedTreesBox.SelectedIndex]).Weight = Convert.ToByte(this.treeWeightTextBox.Text);
            }
            catch (OverflowException)
            {
                MessageBox.Show("You have entered an invalid value.\r\nValue must be an integer value between " + 0 + " and " + byte.MaxValue + ".", "Bad Value", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                this.treeWeightTextBox.Text = ((NWN2TreeTemplate)((TreeSetting)((ArrayList)((Hashtable)this.paintTrees[landType])["treeSettings"])[this.selectedTreesBox.SelectedIndex]).Tree).Seed.ToString();
            }
            catch (FormatException)
            {
                MessageBox.Show("This field requires a numeric value.", "Non-numeric Value", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                this.treeWeightTextBox.Text = ((TreeSetting)((ArrayList)((Hashtable)this.paintTrees[landType])["treeSettings"])[this.selectedTreesBox.SelectedIndex]).Weight.ToString();
            }
        }

        private void treeMinX_TextChanged(object sender, EventArgs e)
        {
            if (this.selectedTreesBox.SelectedIndex <= -1 || !(this.treeMinX.Text != "-") || !(this.treeMinX.Text != "."))
            {
                return;
            }
            this.treeMinX.Text = this.setMinMaxValue(this.treeMinX.Text, 0, 0);
        }

        private void treeMaxX_TextChanged(object sender, EventArgs e)
        {
            if (this.selectedTreesBox.SelectedIndex <= -1 || !(this.treeMaxX.Text != "-") || !(this.treeMaxX.Text != "."))
            {
                return;
            }
            this.treeMaxX.Text = this.setMinMaxValue(this.treeMaxX.Text, 0, 1);
        }

        private void treeMinY_TextChanged(object sender, EventArgs e)
        {
            if (this.selectedTreesBox.SelectedIndex <= -1 || !(this.treeMinY.Text != "-") || !(this.treeMinY.Text != "."))
            {
                return;
            }
            this.treeMinY.Text = this.setMinMaxValue(this.treeMinY.Text, 1, 0);
        }

        private void treeMaxY_TextChanged(object sender, EventArgs e)
        {
            if (this.selectedTreesBox.SelectedIndex <= -1 || !(this.treeMaxY.Text != "-") || !(this.treeMaxY.Text != "."))
            {
                return;
            }
            this.treeMaxY.Text = this.setMinMaxValue(this.treeMaxY.Text, 1, 1);
        }

        private void treeMinZ_TextChanged(object sender, EventArgs e)
        {
            if (this.selectedTreesBox.SelectedIndex <= -1 || !(this.treeMinZ.Text != "-") || !(this.treeMinZ.Text != "."))
            {
                return;
            }
            this.treeMinZ.Text = this.setMinMaxValue(this.treeMinZ.Text, 2, 0);
        }

        private void treeMaxZ_TextChanged(object sender, EventArgs e)
        {
            if (this.selectedTreesBox.SelectedIndex <= -1 || !(this.treeMaxZ.Text != "-") || !(this.treeMaxZ.Text != "."))
            {
                return;
            }
            this.treeMaxZ.Text = this.setMinMaxValue(this.treeMaxZ.Text, 2, 1);
        }

        private string setMinMaxValue(string text, int x, int y)
        {
            string minMaxValue;
            string landType = this.treeLandTypeComboBox.Text;
            try
            {
                ((TreeSetting)((ArrayList)((Hashtable)this.paintTrees[landType])["treeSettings"])[this.selectedTreesBox.SelectedIndex]).Scale[x, y] = Convert.ToSingle(text);
                minMaxValue = ((TreeSetting)((ArrayList)((Hashtable)this.paintTrees[landType])["treeSettings"])[this.selectedTreesBox.SelectedIndex]).Scale[x, y].ToString();
            }
            catch (OverflowException)
            {
                MessageBox.Show("You have entered an invalid value.\r\nValue must be a floating point value between " + float.MinValue + " and " + float.MaxValue + ".", "Bad Value", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                minMaxValue = ((TreeSetting)((ArrayList)((Hashtable)this.paintTrees[landType])["treeSettings"])[this.selectedTreesBox.SelectedIndex]).Scale[x, y].ToString();
            }
            catch (FormatException)
            {
                MessageBox.Show("This field requires a numeric value.", "Non-numeric Value", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                minMaxValue = ((TreeSetting)((ArrayList)((Hashtable)this.paintTrees[landType])["treeSettings"])[this.selectedTreesBox.SelectedIndex]).Scale[x, y].ToString();
            }
            return minMaxValue;
        }
    }
}
