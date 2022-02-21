using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Kuroha.Tool.QHierarchy.Editor.QHelper;

namespace Kuroha.Tool.QHierarchy.Editor.QData
{
    public class QTagTexture
    {
        public string tag;
        public Texture2D texture;
        
        public QTagTexture(string tag, Texture2D texture)
        {
            this.tag = tag;
            this.texture = texture;
        }

        public static List<QTagTexture> loadTagTextureList()
        {
            List<QTagTexture> tagTextureList = new List<QTagTexture>();
            string customTagIcon = QSettings.Instance().Get<string>(EM_QHierarchySettings.TagIconList);
            string[] customTagIconArray = customTagIcon.Split(new char[]{';'});
            List<string> tags = new List<string>(UnityEditorInternal.InternalEditorUtility.tags);
            for (int i = 0; i < customTagIconArray.Length - 1; i+=2)
            {
                string tag = customTagIconArray[i];
                if (!tags.Contains(tag)) continue;
                string texturePath = customTagIconArray[i+1];
                
                Texture2D texture = (Texture2D)AssetDatabase.LoadAssetAtPath(texturePath, typeof(Texture2D));
                if (texture != null) 
                { 
                    QTagTexture tagTexture = new QTagTexture(tag, texture);
                    tagTextureList.Add(tagTexture);
                }  
            }
            return tagTextureList;
        }

        public static void saveTagTextureList(EM_QHierarchySettings hierarchySettings, List<QTagTexture> tagTextureList)
        { 
            string result = "";
            for (int i = 0; i < tagTextureList.Count; i++)            
                result += tagTextureList[i].tag + ";" + AssetDatabase.GetAssetPath(tagTextureList[i].texture.GetInstanceID()) + ";";
            QSettings.Instance().Set(hierarchySettings, result);
        }
    }

    public class QLayerTexture
    {
        public string layer;
        public Texture2D texture;
        
        public QLayerTexture(string layer, Texture2D texture)
        {
            this.layer = layer;
            this.texture = texture;
        }
        
        public static List<QLayerTexture> loadLayerTextureList()
        {
            List<QLayerTexture> layerTextureList = new List<QLayerTexture>();
            string customTagIcon = QSettings.Instance().Get<string>(EM_QHierarchySettings.LayerIconList);
            string[] customLayerIconArray = customTagIcon.Split(new char[]{';'});
            List<string> layers = new List<string>(UnityEditorInternal.InternalEditorUtility.layers);
            for (int i = 0; i < customLayerIconArray.Length - 1; i+=2)
            {
                string layer = customLayerIconArray[i];
                if (!layers.Contains(layer)) continue;
                string texturePath = customLayerIconArray[i+1];
                
                Texture2D texture = (Texture2D)AssetDatabase.LoadAssetAtPath(texturePath, typeof(Texture2D));
                if (texture != null) 
                { 
                    QLayerTexture tagTexture = new QLayerTexture(layer, texture);
                    layerTextureList.Add(tagTexture);
                }  
            }
            return layerTextureList;
        }
        
        public static void saveLayerTextureList(EM_QHierarchySettings hierarchySettings, List<QLayerTexture> layerTextureList)
        { 
            string result = "";
            for (int i = 0; i < layerTextureList.Count; i++)            
                result += layerTextureList[i].layer + ";" + AssetDatabase.GetAssetPath(layerTextureList[i].texture.GetInstanceID()) + ";";
            QSettings.Instance().Set(hierarchySettings, result);
        }
    }

    public delegate void QSettingChangedHandler();

	public class QSettings 
	{
        // CONST
		private const string PREFS_PREFIX = "QTools.QHierarchy_";
        private const string PREFS_DARK = "Dark_";
        private const string PREFS_LIGHT = "Light_";
        public const string DEFAULT_ORDER = "0;1;2;3;4;5;6;7;8;9;10;11;12";
        public const int DEFAULT_ORDER_COUNT = 13;
        private const string SETTINGS_FILE_NAME = "QSettingsObjectAsset";

        // PRIVATE
        private QSettingsObject settingsObject;
        private Dictionary<EM_QHierarchySettings, object> defaultSettings = new Dictionary<EM_QHierarchySettings, object>();
        private HashSet<int> skinDependedSettings = new HashSet<int>();
        private Dictionary<int, QSettingChangedHandler> settingChangedHandlerList = new Dictionary<int, QSettingChangedHandler>();

        // SINGLETON
        private static QSettings instance;
        public static QSettings Instance()
        {
            return instance ??= new QSettings();
        }

        // CONSTRUCTOR
		private QSettings()
		{ 
            var paths = AssetDatabase.FindAssets(SETTINGS_FILE_NAME); 
            foreach (var path in paths)
            {
                settingsObject = (QSettingsObject)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(path), typeof(QSettingsObject));
                if (settingsObject != null) break;
            }
            if (settingsObject == null) 
            {
                settingsObject = ScriptableObject.CreateInstance<QSettingsObject>();
                var path = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(settingsObject));
                path = path.Substring(0, path.LastIndexOf("/", StringComparison.Ordinal));
                AssetDatabase.CreateAsset(settingsObject, path + "/" + SETTINGS_FILE_NAME + ".asset");
                AssetDatabase.SaveAssets();
            }

            initSetting(EM_QHierarchySettings.TreeMapShow                                , true);
            initSetting(EM_QHierarchySettings.TreeMapColor                               , "39FFFFFF", "905D5D5D");
            initSetting(EM_QHierarchySettings.TreeMapEnhanced                            , true);
            initSetting(EM_QHierarchySettings.TreeMapTransparentBackground               , true);

            initSetting(EM_QHierarchySettings.MonoBehaviourIconShow                      , true);
            initSetting(EM_QHierarchySettings.MonoBehaviourIconShowDuringPlayMode        , true);
            initSetting(EM_QHierarchySettings.MonoBehaviourIconIgnoreUnityMonoBehaviour  , true);
            initSetting(EM_QHierarchySettings.MonoBehaviourIconColor                     , "A01B6DBB");

            initSetting(EM_QHierarchySettings.SeparatorShow                              , true);
            initSetting(EM_QHierarchySettings.SeparatorShowRowShading                    , true);
            initSetting(EM_QHierarchySettings.SeparatorColor                             , "FF303030", "48666666");
            initSetting(EM_QHierarchySettings.SeparatorEvenRowShadingColor               , "13000000", "08000000");
            initSetting(EM_QHierarchySettings.SeparatorOddRowShadingColor                , "00000000", "00FFFFFF");

            initSetting(EM_QHierarchySettings.VisibilityShow                             , true);
            initSetting(EM_QHierarchySettings.VisibilityShowDuringPlayMode               , true);

            initSetting(EM_QHierarchySettings.LockShow                                   , true);
            initSetting(EM_QHierarchySettings.LockShowDuringPlayMode                     , false);
            initSetting(EM_QHierarchySettings.LockPreventSelectionOfLockedObjects        , false);

            initSetting(EM_QHierarchySettings.StaticShow                                 , true); 
            initSetting(EM_QHierarchySettings.StaticShowDuringPlayMode                   , false);

            initSetting(EM_QHierarchySettings.ErrorShow                                  , true);
            initSetting(EM_QHierarchySettings.ErrorShowDuringPlayMode                    , false);
            initSetting(EM_QHierarchySettings.ErrorShowIconOnParent                      , false);
            initSetting(EM_QHierarchySettings.ErrorShowScriptIsMissing                   , true);
            initSetting(EM_QHierarchySettings.ErrorShowReferenceIsNull                   , false);
            initSetting(EM_QHierarchySettings.ErrorShowReferenceIsMissing                , true);
            initSetting(EM_QHierarchySettings.ErrorShowStringIsEmpty                     , false);
            initSetting(EM_QHierarchySettings.ErrorShowMissingEventMethod                , true);
            initSetting(EM_QHierarchySettings.ErrorShowWhenTagOrLayerIsUndefined         , true);
            initSetting(EM_QHierarchySettings.ErrorIgnoreString                          , "");
            initSetting(EM_QHierarchySettings.ErrorShowForDisabledComponents             , true);
            initSetting(EM_QHierarchySettings.ErrorShowForDisabledGameObjects            , true);

            initSetting(EM_QHierarchySettings.RendererShow                               , false);
            initSetting(EM_QHierarchySettings.RendererShowDuringPlayMode                 , false);

            initSetting(EM_QHierarchySettings.PrefabShow                                 , false);
            initSetting(EM_QHierarchySettings.PrefabShowBrakedPrefabsOnly               , true);

            initSetting(EM_QHierarchySettings.TagAndLayerShow                            , true);
            initSetting(EM_QHierarchySettings.TagAndLayerShowDuringPlayMode              , true);
            initSetting(EM_QHierarchySettings.TagAndLayerSizeShowType                    , (int)EM_QHierarchyTagAndLayerShowType.标签和层级都显示);
            initSetting(EM_QHierarchySettings.TagAndLayerType                            , (int)EM_QHierarchyTagAndLayerType.仅显示非默认名称);
            initSetting(EM_QHierarchySettings.TagAndLayerAlignment                        , (int)EM_QHierarchyTagAndLayerAlignment.Left);
            initSetting(EM_QHierarchySettings.TagAndLayerSizeValueType                   , (int)EM_QHierarchyTagAndLayerSizeType.像素值);
            initSetting(EM_QHierarchySettings.TagAndLayerSizeValuePercent                , 0.25f);
            initSetting(EM_QHierarchySettings.TagAndLayerSizeValuePixel                  , 75);
            initSetting(EM_QHierarchySettings.TagAndLayerLabelSize                       , (int)EM_QHierarchyTagAndLayerLabelSize.Normal);
            initSetting(EM_QHierarchySettings.TagAndLayerTagLabelColor                   , "FFCCCCCC", "FF333333");
            initSetting(EM_QHierarchySettings.TagAndLayerLayerLabelColor                 , "FFCCCCCC", "FF333333");
            initSetting(EM_QHierarchySettings.TagAndLayerLabelAlpha                      , 0.35f);

            initSetting(EM_QHierarchySettings.ColorShow                                  , true);
            initSetting(EM_QHierarchySettings.ColorShowDuringPlayMode                    , true);

            initSetting(EM_QHierarchySettings.GameObjectIconShow                         , false);
            initSetting(EM_QHierarchySettings.GameObjectIconShowDuringPlayMode           , true);
            initSetting(EM_QHierarchySettings.GameObjectIconSize                         , (int)EM_QHierarchySizeAll.Small);

            initSetting(EM_QHierarchySettings.TagIconShow                                , false);
            initSetting(EM_QHierarchySettings.TagIconShowDuringPlayMode                  , true);
            initSetting(EM_QHierarchySettings.TagIconListFoldout                         , false);
            initSetting(EM_QHierarchySettings.TagIconList                                , "");
            initSetting(EM_QHierarchySettings.TagIconSize                                , (int)EM_QHierarchySizeAll.Small);

            initSetting(EM_QHierarchySettings.LayerIconShow                              , false);
            initSetting(EM_QHierarchySettings.LayerIconShowDuringPlayMode                , true);
            initSetting(EM_QHierarchySettings.LayerIconListFoldout                       , false);
            initSetting(EM_QHierarchySettings.LayerIconList                              , "");
            initSetting(EM_QHierarchySettings.LayerIconSize                              , (int)EM_QHierarchySizeAll.Small);

            initSetting(EM_QHierarchySettings.ChildrenCountShow                          , false);     
            initSetting(EM_QHierarchySettings.ChildrenCountShowDuringPlayMode            , true);
            initSetting(EM_QHierarchySettings.ChildrenCountLabelSize                     , (int)EM_QHierarchySize.Normal);
            initSetting(EM_QHierarchySettings.ChildrenCountLabelColor                    , "FFCCCCCC", "FF333333");

            initSetting(EM_QHierarchySettings.VerticesAndTrianglesShow                   , false);
            initSetting(EM_QHierarchySettings.VerticesAndTrianglesShowDuringPlayMode     , false);
            initSetting(EM_QHierarchySettings.VerticesAndTrianglesCalculateTotalCount    , false);
            initSetting(EM_QHierarchySettings.VerticesAndTrianglesShowTriangles          , false);
            initSetting(EM_QHierarchySettings.VerticesAndTrianglesShowVertices           , true);
            initSetting(EM_QHierarchySettings.VerticesAndTrianglesLabelSize              , (int)EM_QHierarchySize.Normal);
            initSetting(EM_QHierarchySettings.VerticesAndTrianglesVerticesLabelColor     , "FFCCCCCC", "FF333333");
            initSetting(EM_QHierarchySettings.VerticesAndTrianglesTrianglesLabelColor    , "FFCCCCCC", "FF333333");

            initSetting(EM_QHierarchySettings.ComponentsShow                             , false);
            initSetting(EM_QHierarchySettings.ComponentsShowDuringPlayMode               , false);
            initSetting(EM_QHierarchySettings.ComponentsIconSize                         , (int)EM_QHierarchySizeAll.Small);
            initSetting(EM_QHierarchySettings.ComponentsIgnore                           , "");

            initSetting(EM_QHierarchySettings.ComponentsOrder                            , DEFAULT_ORDER);

            initSetting(EM_QHierarchySettings.AdditionalShowObjectListContent            , false);
            initSetting(EM_QHierarchySettings.AdditionalShowHiddenQHierarchyObjectList   , true);
            initSetting(EM_QHierarchySettings.AdditionalHideIconsIfNotFit                , true);
            initSetting(EM_QHierarchySettings.AdditionalIndentation                       , 0);
            initSetting(EM_QHierarchySettings.AdditionalShowModifierWarning              , true);

            initSetting(EM_QHierarchySettings.AdditionalBackgroundColor                  , "00383838", "00CFCFCF");
            initSetting(EM_QHierarchySettings.AdditionalActiveColor                      , "FFFFFF80", "CF363636");
            initSetting(EM_QHierarchySettings.AdditionalInactiveColor                    , "FF4F4F4F", "1E000000");
            initSetting(EM_QHierarchySettings.AdditionalSpecialColor                     , "FF2CA8CA", "FF1D78D5");
		} 

        // DESTRUCTOR
        public void OnDestroy()
        {
            skinDependedSettings = null;
            defaultSettings = null;
            settingsObject = null;
            settingChangedHandlerList = null;
            instance = null;
        }

        // PUBLIC
        public T Get<T>(EM_QHierarchySettings hierarchySettings)
        {
            return (T)settingsObject.Get<T>(GetSettingName(hierarchySettings));
        }

        public Color GetColor(EM_QHierarchySettings hierarchySettings)
        {
            var stringColor = (string)settingsObject.Get<string>(GetSettingName(hierarchySettings));
            return QHierarchyColorUtils.StringToColor(stringColor);
        }

        public void SetColor(EM_QHierarchySettings hierarchySettings, Color color)
        {
            string stringColor = QHierarchyColorUtils.ColorToString(color);
            Set(hierarchySettings, stringColor);
        }

        /// <summary>
        /// 设置配置
        /// </summary>
        /// <param name="hierarchySettings"></param>
        /// <param name="value"></param>
        /// <param name="invokeChanger"></param>
        /// <typeparam name="T"></typeparam>
        public void Set<T>(EM_QHierarchySettings hierarchySettings, T value, bool invokeChanger = true)
        {
            var settingId = (int)hierarchySettings;
            settingsObject.Set(GetSettingName(hierarchySettings), value);

            if (invokeChanger && settingChangedHandlerList.ContainsKey(settingId) && settingChangedHandlerList[settingId] != null)
            {
                settingChangedHandlerList[settingId].Invoke();
            }
            
            EditorApplication.RepaintHierarchyWindow();
        }

        public void AddEventListener(EM_QHierarchySettings hierarchySettings, QSettingChangedHandler handler)
        {
            int settingId = (int)hierarchySettings;
            
            if (!settingChangedHandlerList.ContainsKey(settingId))          
                settingChangedHandlerList.Add(settingId, null);
            
            if (settingChangedHandlerList[settingId] == null)           
                settingChangedHandlerList[settingId] = handler;
            else            
                settingChangedHandlerList[settingId] += handler;
        }
        
        public void removeEventListener(EM_QHierarchySettings hierarchySettings, QSettingChangedHandler handler)
        {
            int settingId = (int)hierarchySettings;
            
            if (settingChangedHandlerList.ContainsKey(settingId) && settingChangedHandlerList[settingId] != null)       
                settingChangedHandlerList[settingId] -= handler;
        }
        
        /// <summary>
        /// 恢复默认设置
        /// </summary>
        /// <param name="hierarchySettings"></param>
        public void Restore(EM_QHierarchySettings hierarchySettings)
        {
            Set(hierarchySettings, defaultSettings[hierarchySettings]);
        }

        // PRIVATE
        private void initSetting(EM_QHierarchySettings hierarchySettings, object defaultValueDark, object defaultValueLight)
        {
            skinDependedSettings.Add((int)hierarchySettings);
            initSetting(hierarchySettings, EditorGUIUtility.isProSkin ? defaultValueDark : defaultValueLight);
        }
        
        private void initSetting(EM_QHierarchySettings hierarchySettings, object defaultValue)
        {
            string settingName = GetSettingName(hierarchySettings);
            defaultSettings.Add(hierarchySettings, defaultValue);
            object value = settingsObject.Get(settingName, defaultValue);
            if (value == null || value.GetType() != defaultValue.GetType())
            {
                settingsObject.Set(settingName, defaultValue);
            }        
        }

        private string GetSettingName(EM_QHierarchySettings hierarchySettings)
        {
            var settingId = (int)hierarchySettings;
            var settingName = PREFS_PREFIX;
            if (skinDependedSettings.Contains(settingId))
            {
                settingName += EditorGUIUtility.isProSkin ? PREFS_DARK : PREFS_LIGHT;
            }
            settingName += hierarchySettings.ToString("G");
            return settingName;
        }
	}
}
