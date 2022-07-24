using App.Main;
using Next.ContentLoader;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace App.Main
{
    #region Classes

    [Serializable]
    public class FlagData
    {
        public string Headline;
        public string Subheadline;
        public MediaFile[] Media;
        public Bullet[] Bullets;
        public Widget Widget;
    }

    [Serializable]
    public class MediaFile
    {
        public Texture Image;
        public string VideoPath;
    }

    [Serializable]
    public class Widget
    {
        public string HeadlineLeft;
        public string HeadlineRight;
        public int FinalPercent;
    }

    [Serializable]
    public class Bullet
    {
        public Texture2D Logo;
        public string BulletText;
    }

    [Serializable]
    public class DotData
    {
        public string Headline;
        public float PosX;
        public float Range;
    }

    [Serializable]
    public class WorldCanvasData
    {
        public string HeadlineFlag;
        public string WarningMessage;
        public float PosXFlag;
        public float RangeFlag;
        public float PosXMessage;
        public float RangeMessage;
    }

    #endregion

    public static class DataProvider
    {
        public static WorldCanvasData GetWorldCanvasData(Scenes scene, int contentIdFlag, int contentIdFlagMessage)
        {
            WorldCanvasData canvasData = new();
            ContentLevel level = ContentLoader.GetContentLevel((int)scene);
            if (level != null)
            {
                ExcelAsset excelAsset = ContentLoader.FindLevelAsset<ExcelAsset>(level, "*Connectivity*", true);

                if (excelAsset != null)
                {
                    excelAsset.FindCellInColumnAndGetValue("Headline_Widget", 0, 0, 1, out canvasData.HeadlineFlag);
                    excelAsset.FindCellInColumnAndGetValue("Waring_Message", 0, 0, 1, out canvasData.WarningMessage);
                }

                ExcelAsset timelinePlacement = ContentLoader.GetGlobalAsset<ExcelAsset>("TimelinePlacement");
                if (timelinePlacement != null)
                {
                    if (timelinePlacement.FindCellInColumn(scene.ToString(), 1, out int rowId))
                    {
                        timelinePlacement.GetCellValue(rowId + contentIdFlag, 1, out canvasData.PosXFlag);
                        timelinePlacement.GetCellValue(rowId + contentIdFlag, 2, out canvasData.RangeFlag);
                        timelinePlacement.GetCellValue(rowId + contentIdFlagMessage, 1, out canvasData.PosXMessage);
                        timelinePlacement.GetCellValue(rowId + contentIdFlagMessage, 2, out canvasData.RangeMessage);
                    }
                }
            }
            return canvasData;
        }

        public static FlagData[] GetAllFlagData(int idScene, int idMax)
        {
            FlagData[] flagData = new FlagData[idMax];
            for (int i = 1; i <= idMax; i++)
            {
                flagData[i - 1] = ProvideFlagData(idScene, i);
            }

            return flagData;
        }

        private static FlagData ProvideFlagData(int idScene, int idContent)
        {
            FlagData flagData = new FlagData();
            ContentLevel level = ContentLoader.GetContentLevel(idScene, idContent);
            if (level != null)
            {
                ExcelAsset excelAsset = ContentLoader.FindLevelAsset<ExcelAsset>(level, "*Connectivity*", true);

                if (excelAsset != null)
                {
                    int rowId;

                    excelAsset.FindCellInColumnAndGetValue("Headline", 0, 0, 1, out flagData.Headline);
                    excelAsset.FindCellInColumnAndGetValue("Subheadline", 0, 0, 1, out flagData.Subheadline);

                    if (excelAsset.FindCellInColumn("Bullets", 0, out rowId))
                    {
                        List<string> icons = excelAsset.IterateCellValues<string>(rowId + 1, 1);
                        List<string> texts = excelAsset.IterateCellValues<string>(rowId + 1, 2);

                        if (icons.Count == texts.Count)
                        {
                            flagData.Bullets = new Bullet[icons.Count];
                            for (int i = 0; i < icons.Count; i++)
                            {
                                flagData.Bullets[i] = new Bullet();
                                if (ContentLoader.FindGlobalAsset<ImageAsset>(icons[i], true) != null)
                                    flagData.Bullets[i].Logo = ContentLoader.FindGlobalAsset<ImageAsset>(icons[i], true).GetTexture();
                                else
                                    Debug.LogError($"Could not find \"{icons[i]}\"");
                                flagData.Bullets[i].BulletText = texts[i];
                            }
                        }
                        else
                        {
                            Debug.LogError($"icons and texts count of template {idScene}.{idContent} do not match");
                        }
                    }

                    if (excelAsset.FindCellInColumn("Media", 0, out rowId))
                    {
                        List<string> media = excelAsset.IterateCellValues<string>(rowId, 1);

                        flagData.Media = new MediaFile[media.Count];

                        for (int i = 0; i < media.Count; i++)
                        {
                            flagData.Media[i] = new MediaFile();
                            if (ContentLoader.FindGlobalAsset<ImageAsset>(media[i], true) != null)
                            {
                                var bla = ContentLoader.FindGlobalAsset<ImageAsset>(media[i], true);
                                flagData.Media[i].Image = ContentLoader.FindGlobalAsset<ImageAsset>(media[i], true).GetTexture();
                            }
                            else if (ContentLoader.FindGlobalAsset<VideoAsset>(media[i], true) != null)
                            {
                                flagData.Media[i].VideoPath = ContentLoader.FindGlobalAsset<VideoAsset>(media[i], true).AssetPath;
                            }
                            else
                            {
                                flagData.Media[i] = null;
                            }
                        }
                    }

                    flagData.Widget = new Widget();
                    if (excelAsset.FindCellInColumnAndGetValue("Final_Percent", 0, 0, 1, out flagData.Widget.FinalPercent))
                    {
                        excelAsset.FindCellInColumnAndGetValue("Headline_Left", 0, 0, 1, out flagData.Widget.HeadlineLeft);
                        excelAsset.FindCellInColumnAndGetValue("Headline_Right", 0, 0, 1, out flagData.Widget.HeadlineRight);
                    }
                    else
                    {
                        flagData.Widget = null;
                    }
                }
            }
            return flagData;
        }

        public static DotData[] ProvideDotData(Scenes activeScene)
        {
            DotData[] dotData = null;
            ExcelAsset timelinePlacement = ContentLoader.GetGlobalAsset<ExcelAsset>("TimelinePlacement");
            if (timelinePlacement != null)
            {
                if (timelinePlacement.FindCellInColumn(activeScene.ToString(), 1, out int rowId))
                {
                    List<float> placements = timelinePlacement.IterateCellValues<float>(rowId + 1, 1);
                    List<float> ranges = timelinePlacement.IterateCellValues<float>(rowId + 1, 2);
                    List<ExcelAsset> excelAssets = ContentLoader.FindGlobalAssets<ExcelAsset>($"*Connectivity_{activeScene}*", true);

                    if (placements.Count == ranges.Count)
                    {
                        dotData = new DotData[placements.Count];

                        for (int i = 0; i < placements.Count; i++)
                        {
                            dotData[i] = new DotData();
                            excelAssets[i].FindCellInColumnAndGetValue("Headline_Timeline", 0, 0, 1, out dotData[i].Headline);
                            dotData[i].PosX = placements[i];
                            dotData[i].Range = ranges[i];
                        }
                    }
                    else
                    {
                        Debug.LogError($"Timeline Hotspots in {activeScene} do not match. Placement:{placements.Count} Ranges:{ranges.Count} ");
                    }
                }
            }

            return dotData;
        }
    }
}