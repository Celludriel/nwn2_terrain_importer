using Microsoft.DirectX;
using NWN2Toolset;
using NWN2Toolset.NWN2.Data.Instances;
using NWN2Toolset.NWN2.Data.Templates;
using NWN2Toolset.NWN2.NetDisplay;
using NWN2Toolset.NWN2.Views;
using NWN2Toolset.Plugins;
using OEIShared.NetDisplay;
using OEIShared.OEIMath;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using TD.SandBar;
using TerrainImporter.Forms;
using TerrainImporter.Model;

namespace TerrainImporter
{
    public class TerrainImporter : INWN2Plugin
    {
        private Random rng = new Random();
        private MenuButtonItem menuItem;

        public MenuButtonItem PluginMenuItem
        {
            get
            {
                return this.menuItem;
            }
        }

        public string DisplayName
        {
            get
            {
                return "Terrain Importer";
            }
        }

        public string MenuName
        {
            get
            {
                return "Terrain Importer";
            }
        }

        public object Preferences
        {
            get
            {
                return null;
            }
            set
            {
            }
        }

        public string Name
        {
            get
            {
                return "TerrainImporter";
            }
        }

        public void Startup(INWN2PluginHost Host)
        {
            this.menuItem = Host.GetMenuForPlugin(this);
            this.menuItem.Activate += new EventHandler(this.TerrainImporter_Activate);
        }

        public void Load(INWN2PluginHost Host)
        {
        }

        public void Unload(INWN2PluginHost Host)
        {
        }

        public void Shutdown(INWN2PluginHost Host)
        {
        }

        private void TerrainImporter_Activate(object sender, EventArgs e)
        {
            if (NWN2NetDisplayManager.Instance.Scenes.Count > 0)
            {
                TerrainImporterSettings importerSettings = new TerrainImporterSettings();
                importerSettings.ShowDialog();

                if (importerSettings.isCanceled())
                {
                    return;
                }

                float texturePressure = Convert.ToSingle(importerSettings.texturePressure.Value) / 100f;
                float textureInnerRadius = Convert.ToSingle(importerSettings.textureInnerRadius.Value);
                float textureOuterRadius = Convert.ToSingle(importerSettings.textureOuterRadius.Value);

                TerrainImporterProgress importerProgress = new TerrainImporterProgress();
                importerProgress.Show();

                List<NWN2AreaViewer> allAreaViewers = NWN2ToolsetMainForm.App.GetAllAreaViewers();
                NWN2AreaViewer nwN2AreaViewer = allAreaViewers[allAreaViewers.IndexOf((NWN2AreaViewer)NWN2ToolsetMainForm.App.GetActiveViewer())];
                NWN2TerrainEditorForm terrainEditor = NWN2ToolsetMainForm.App.TerrainEditor;
                BoundingBox3 boundsOfArea = nwN2AreaViewer.Area.GetBoundsOfArea();
                SynchronousNetDisplayManager netDisplayManager = NWN2NetDisplayManager.Instance;

                float coordinateIncrement = 1.666667f;
                float outerRight = boundsOfArea.Right + 160f + coordinateIncrement;
                float outerTop = boundsOfArea.Top + 160f + coordinateIncrement;

                int heightMapUpperLeftX = Convert.ToInt32(importerSettings.upperLeftX.Value);
                int heightMapUpperLeftY = Convert.ToInt32(importerSettings.upperLeftY.Value);
                float areaToHeigtmapXdifference = (Convert.ToSingle(importerSettings.lowerRightX.Value - heightMapUpperLeftX) - 1f) / outerRight;
                float areaToHeightmapYdifference = (Convert.ToSingle(importerSettings.lowerRightY.Value - heightMapUpperLeftY) - 1f) / outerTop;

                float heightDiff = Convert.ToSingle(importerSettings.maximumHeight.Value - importerSettings.minimumHeight.Value);
                float minHeight = Convert.ToSingle(importerSettings.minimumHeight.Value);
                importerProgress.Maximum = Convert.ToInt32(Math.Round(outerRight * outerTop));

                float areaX = 0.0f;
                while (areaX < outerRight)
                {
                    float areaY = 0.0f;
                    while (areaY < outerTop)
                    {
                        int areaOffsetX = Convert.ToInt32(Math.Round(areaToHeigtmapXdifference * areaX));
                        int areaOffsetY = Convert.ToInt32(Math.Round(areaToHeightmapYdifference * areaY));
                        int x = areaOffsetX + heightMapUpperLeftX;
                        int y = areaOffsetY + heightMapUpperLeftY;
                        float z = getHeightValue(importerSettings, heightDiff, minHeight, x, y);

                        Vector3 vector3 = new Vector3(areaX, areaY, 0.0f);
                        string texture = terrainEditor.TerrainBrushTexture;

                        string textureName = "";
                        bool paintTerrain = false;
                        if (importerSettings.paintTerrainCheckbox.Checked)
                        {
                            paintTerrain = true;
                            textureName = (string)importerSettings.fullAttributeTable[((AmfAttributeMap)importerSettings.attributeMap).GetPixel(x, y)];
                        }

                        //work all the area manipulation
                        netDisplayManager.BeginSynchronizedOperation();
                        manipulateTerrain(importerSettings, paintTerrain, texturePressure, textureInnerRadius, textureOuterRadius, nwN2AreaViewer, terrainEditor, netDisplayManager, z, ref vector3, ref texture, textureName);
                        handleWaterMap(importerSettings, nwN2AreaViewer, terrainEditor, netDisplayManager, x, y, z, ref vector3, texture);
                        handleAttributeMap(importerSettings, nwN2AreaViewer, terrainEditor, netDisplayManager, outerRight, outerTop, x, y, areaX, areaY, z, ref vector3, texture, ref textureName);
                        netDisplayManager.EndSynchronizedOperation();

                        updateProgress(importerProgress, outerTop, areaX, areaY);

                        areaY += coordinateIncrement;
                    }
                    areaX += coordinateIncrement;
                }
                importerProgress.Dispose();
                importerSettings.Dispose();
            }
            else
            {
                MessageBox.Show("You must have an area open to use this tool.", "No Area", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        private float getHeightValue(TerrainImporterSettings importerSettings, float heightDiff, float minHeight, int x, int y)
        {
            HeightMap heightMap = importerSettings.heightMap;
            decimal minimumInputHeight = importerSettings.minimumHeight.Value;
            decimal maximumInputHeight = importerSettings.maximumHeight.Value;

            float unmodifiedHeight = heightMap.GetHeight(x, y);

            Type heightMapType = heightMap.GetType();
            if (heightMapType == typeof(BmpHeightMap))
            {
                return unmodifiedHeight * heightDiff + minHeight;
            }
            else if (heightMapType == typeof(Hf2HeightMap))
            {
                if (unmodifiedHeight < 0)
                {
                    float min = ((Hf2HeightMap)heightMap).getMinimumHeight();
                    return (unmodifiedHeight / min) * (float)minimumInputHeight;
                }
                else
                {
                    float max = ((Hf2HeightMap)heightMap).getMaximumHeight();
                    return (unmodifiedHeight / max) * (float)maximumInputHeight;
                }
            }
            throw new Exception("Unknown heightmap can't find height");
        }

        private void manipulateTerrain(TerrainImporterSettings importerSettings, bool paintTerrain, float texturePressure, float textureInnerRadius, float textureOuterRadius, NWN2AreaViewer nwN2AreaViewer, NWN2TerrainEditorForm terrainEditor, SynchronousNetDisplayManager netDisplayManager, float zValue, ref Vector3 vector3, ref string texture, string textureName)
        {
            netDisplayManager.TerrainBrush(nwN2AreaViewer.AreaNetDisplayWindow.Scene, 1, 0, vector3, 1f, 1f, zValue, 0.5f, terrainEditor.TerrainBrushColor, terrainEditor.CursorColor, texture, TerrainModificationType.Flatten);
            netDisplayManager.TerrainModify(nwN2AreaViewer.AreaNetDisplayWindow.Scene, TerrainModificationType.Flatten, 0);
            if (paintTerrain)
            {
                texture = (string)importerSettings.paintTextures[textureName];
                netDisplayManager.TerrainBrush(nwN2AreaViewer.AreaNetDisplayWindow.Scene, 1, 0, vector3, textureInnerRadius, textureOuterRadius, texturePressure, 0.5f, terrainEditor.TerrainBrushColor, terrainEditor.CursorColor, texture, TerrainModificationType.Flatten);
                netDisplayManager.TerrainModify(nwN2AreaViewer.AreaNetDisplayWindow.Scene, TerrainModificationType.Texture, 0);
            }
        }

        private void handleWaterMap(TerrainImporterSettings importerSettings, NWN2AreaViewer nwN2AreaViewer, NWN2TerrainEditorForm terrainEditor, SynchronousNetDisplayManager netDisplayManager, int x, int y, float z, ref Vector3 vector3, string texture)
        {
            WaterMap waterMap = importerSettings.waterMap;
            if (importerSettings.paintWaterCheckbox.Checked)
            {
                float seaLevel = Convert.ToSingle(importerSettings.seaLevel.Value);
                if (waterMap == null && z < Convert.ToSingle(importerSettings.seaLevel.Value))
                {
                    paintWaterOnMap(nwN2AreaViewer, terrainEditor, netDisplayManager, seaLevel, vector3, texture);
                }
                else if (waterMap != null)
                {
                    bool paintWater = waterMap.PaintWater(x, y);
                    if (paintWater)
                    {
                        if (z > 0.0f)
                        {
                            paintWaterOnMap(nwN2AreaViewer, terrainEditor, netDisplayManager, z, vector3, texture);
                        }
                        else
                        {
                            paintWaterOnMap(nwN2AreaViewer, terrainEditor, netDisplayManager, seaLevel, vector3, texture);
                        }
                    }
                }
            }
        }

        private void paintWaterOnMap(NWN2AreaViewer nwN2AreaViewer, NWN2TerrainEditorForm terrainEditor, SynchronousNetDisplayManager netDisplayManager, float z, Vector3 vector3, string texture)
        {
            netDisplayManager.TerrainBrush(nwN2AreaViewer.AreaNetDisplayWindow.Scene, 1, 1, vector3, 1f, 6f, z, 0.5f, terrainEditor.TerrainBrushColor, terrainEditor.CursorColor, texture, TerrainModificationType.Water);
            terrainEditor.UpdateWaterSliders();
            netDisplayManager.TerrainModify(nwN2AreaViewer.AreaNetDisplayWindow.Scene, TerrainModificationType.Water, -1);
        }

        private void updateProgress(TerrainImporterProgress importerProgress, float outerTop, float valueX, float valueY)
        {
            int progress = Convert.ToInt32(Math.Round(valueX * outerTop + valueY));
            importerProgress.progress = progress >= importerProgress.Maximum ? importerProgress.Maximum : progress;
            Thread.Sleep(0);
        }

        private void handleAttributeMap(TerrainImporterSettings importerSettings, NWN2AreaViewer nwN2AreaViewer, NWN2TerrainEditorForm terrainEditor, SynchronousNetDisplayManager netDisplayManager, float outerRight, float outerTop, int x, int y, float areaX, float areaY, float z, ref Vector3 vector3, string texture, ref string textureName)
        {
            if (importerSettings.attributeMap != null)
            {
                AmfAttributeMap amfAttributeMap = (AmfAttributeMap)importerSettings.attributeMap;
                if (textureName == "")
                {
                    textureName = (string)importerSettings.fullAttributeTable[amfAttributeMap.GetPixel(x, y)];
                }

                paintGrass(importerSettings, nwN2AreaViewer, terrainEditor, netDisplayManager, outerRight, outerTop, areaX, areaY, vector3, texture, textureName);
                paintTrees(importerSettings, areaX, areaY, z, textureName);
            }
        }

        private void paintGrass(TerrainImporterSettings importerSettings, NWN2AreaViewer nwN2AreaViewer, NWN2TerrainEditorForm terrainEditor, SynchronousNetDisplayManager netDisplayManager, float outerRight, float outerTop, float areaX, float areaY, Vector3 vector3, string texture, string textureName)
        {
            if (importerSettings.paintGrass != null)
            {
                Hashtable hashtable = (Hashtable)importerSettings.paintGrass[textureName];
                Decimal fullRadius = (Decimal)hashtable["innerRadius"] + (Decimal)hashtable["outerRadius"];
                float floatFullRadius = Convert.ToSingle(++fullRadius);
                if ((bool)hashtable["doPaint"] && areaX > floatFullRadius && (areaX < outerRight - floatFullRadius && areaY > floatFullRadius) && areaY < outerTop - floatFullRadius)
                {
                    string[] textures = (string[])((ArrayList)hashtable["textures"]).ToArray(typeof(string));
                    netDisplayManager.GrassParameters(nwN2AreaViewer.AreaNetDisplayWindow.Scene, (float)hashtable["bladeSize"], (float)hashtable["bladeSizeVariation"], textures.Length, textures);
                    netDisplayManager.TerrainBrush(nwN2AreaViewer.AreaNetDisplayWindow.Scene, 1, 1, vector3, Convert.ToSingle(hashtable["innerRadius"]), Convert.ToSingle(hashtable["outerRadius"]), (float)hashtable["pressure"], 0.5f, terrainEditor.TerrainBrushColor, terrainEditor.CursorColor, texture, TerrainModificationType.Grass);
                    netDisplayManager.TerrainModify(nwN2AreaViewer.AreaNetDisplayWindow.Scene, TerrainModificationType.Grass, -1);                    
                }
            }
        }

        private void paintTrees(TerrainImporterSettings importerSettings, float areaX, float areaY, float z, string textureName)
        {
            if (importerSettings.paintTrees != null)
            {
                Hashtable hashtable = (Hashtable)importerSettings.paintTrees[textureName];
                if ((bool)hashtable["doTrees"] && this.rng.NextDouble() < Convert.ToDouble((Decimal)hashtable["density"]) / 100.0)
                {
                    int totalTreeSettingWeight = 0;
                    foreach (TreeSetting treeSetting in (ArrayList)hashtable["treeSettings"])
                    {
                        totalTreeSettingWeight += treeSetting.Weight;
                    }

                    int randomWeight = Convert.ToInt32(this.rng.NextDouble() * totalTreeSettingWeight);
                    int treeSettingWeight = 0;
                    int treeSettingIndex;
                    for (treeSettingIndex = 0; treeSettingIndex < ((ArrayList)hashtable["treeSettings"]).Count; ++treeSettingIndex)
                    {
                        treeSettingWeight += (int)((TreeSetting)((ArrayList)hashtable["treeSettings"])[treeSettingIndex]).Weight;
                        if (treeSettingWeight >= randomWeight)
                            break;
                    }

                    TreeSetting treeSetting1 = (TreeSetting)((ArrayList)hashtable["treeSettings"])[treeSettingIndex];

                    NWN2TreeInstance fromBlueprint = NWN2TreeInstance.CreateFromBlueprint(treeSetting1.Tree);
                    ((NWN2TreeTemplate)fromBlueprint).Scale = new Vector3(1f, 1f, 1f)
                    {
                        X = Convert.ToSingle(this.rng.NextDouble() * (treeSetting1.Scale[0, 1] - treeSetting1.Scale[0, 0]) + treeSetting1.Scale[0, 0]),
                        Y = Convert.ToSingle(this.rng.NextDouble() * (treeSetting1.Scale[1, 1] - treeSetting1.Scale[1, 0]) + treeSetting1.Scale[1, 0]),
                        Z = Convert.ToSingle(this.rng.NextDouble() * (treeSetting1.Scale[2, 1] - treeSetting1.Scale[2, 0]) + treeSetting1.Scale[2, 0])
                    };
                    fromBlueprint.Position = new Vector3(areaX, areaY, z - 0.25f);
                    NWN2ToolsetMainForm.App.AreaContents.Area.AddInstance(fromBlueprint);
                }
            }
        }
    }
}