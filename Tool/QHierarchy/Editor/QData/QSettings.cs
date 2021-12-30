using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Kuroha.Tool.QHierarchy.Editor.QHelper;

namespace Kuroha.Tool.QHierarchy.Editor.QData
{
    public enum QHierarchyTagAndLayerType
	{
		Always           = 0,
		OnlyIfNotDefault = 1
	}

    public enum QHierarchyTagAndLayerShowType
    {
        TagAndLayer = 0,
        Tag         = 1,
        Layer       = 2
    }

    public enum QHierarchyTagAndLayerAligment
    {
        Left   = 0,
        Center = 1,
        Right  = 2
    }

    public enum QHierarchyTagAndLayerSizeType
    {
        Pixel   = 0,
        Percent = 1
    }

    public enum QHierarchyTagAndLayerLabelSize
    {
        Normal                          = 0,
        Big                             = 1,
        BigIfSpecifiedOnlyTagOrLayer    = 2
    }

    public enum QHierarchySize
    {
        Normal  = 0,
        Big     = 1
    }
        
    public enum QHierarchySizeAll
    {
        Small   = 0,
        Normal  = 1,
        Big     = 2
    }

	public enum QHierarchyComponentEnum
	{
        LockComponent               = 0,
        VisibilityComponent         = 1,
        StaticComponent             = 2,
        ColorComponent              = 3,
        ErrorComponent              = 4,
        RendererComponent           = 5,
        PrefabComponent             = 6,
        TagAndLayerComponent        = 7,
        GameObjectIconComponent     = 8,
        TagIconComponent            = 9,
        LayerIconComponent          = 10,
        ChildrenCountComponent      = 11,
        VerticesAndTrianglesCount   = 12,
        SeparatorComponent          = 1000,
        TreeMapComponent            = 1001,
        MonoBehaviourIconComponent  = 1002,
        ComponentsComponent         = 1003
	}

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
            string customTagIcon = QSettings.Instance().Get<string>(EM_QSetting.TagIconList);
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

        public static void saveTagTextureList(EM_QSetting setting, List<QTagTexture> tagTextureList)
        { 
            string result = "";
            for (int i = 0; i < tagTextureList.Count; i++)            
                result += tagTextureList[i].tag + ";" + AssetDatabase.GetAssetPath(tagTextureList[i].texture.GetInstanceID()) + ";";
            QSettings.Instance().Set(setting, result);
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
            string customTagIcon = QSettings.Instance().Get<string>(EM_QSetting.LayerIconList);
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
        
        public static void saveLayerTextureList(EM_QSetting setting, List<QLayerTexture> layerTextureList)
        { 
            string result = "";
            for (int i = 0; i < layerTextureList.Count; i++)            
                result += layerTextureList[i].layer + ";" + AssetDatabase.GetAssetPath(layerTextureList[i].texture.GetInstanceID()) + ";";
            QSettings.Instance().Set(setting, result);
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
        private Dictionary<int, object> defaultSettings = new Dictionary<int, object>();
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

            initSetting(EM_QSetting.TreeMapShow                                , true);
            initSetting(EM_QSetting.TreeMapColor                               , "39FFFFFF", "905D5D5D");
            initSetting(EM_QSetting.TreeMapEnhanced                            , true);
            initSetting(EM_QSetting.TreeMapTransparentBackground               , true);

            initSetting(EM_QSetting.MonoBehaviourIconShow                      , true);
            initSetting(EM_QSetting.MonoBehaviourIconShowDuringPlayMode        , true);
            initSetting(EM_QSetting.MonoBehaviourIconIgnoreUnityMonoBehaviour  , true);
            initSetting(EM_QSetting.MonoBehaviourIconColor                     , "A01B6DBB");

            initSetting(EM_QSetting.SeparatorShow                              , true);
            initSetting(EM_QSetting.SeparatorShowRowShading                    , true);
            initSetting(EM_QSetting.SeparatorColor                             , "FF303030", "48666666");
            initSetting(EM_QSetting.SeparatorEvenRowShadingColor               , "13000000", "08000000");
            initSetting(EM_QSetting.SeparatorOddRowShadingColor                , "00000000", "00FFFFFF");

            initSetting(EM_QSetting.VisibilityShow                             , true);
            initSetting(EM_QSetting.VisibilityShowDuringPlayMode               , true);

            initSetting(EM_QSetting.LockShow                                   , true);
            initSetting(EM_QSetting.LockShowDuringPlayMode                     , false);
            initSetting(EM_QSetting.LockPreventSelectionOfLockedObjects        , false);

            initSetting(EM_QSetting.StaticShow                                 , true); 
            initSetting(EM_QSetting.StaticShowDuringPlayMode                   , false);

            initSetting(EM_QSetting.ErrorShow                                  , true);
            initSetting(EM_QSetting.ErrorShowDuringPlayMode                    , false);
            initSetting(EM_QSetting.ErrorShowIconOnParent                      , false);
            initSetting(EM_QSetting.ErrorShowScriptIsMissing                   , true);
            initSetting(EM_QSetting.ErrorShowReferenceIsNull                   , false);
            initSetting(EM_QSetting.ErrorShowReferenceIsMissing                , true);
            initSetting(EM_QSetting.ErrorShowStringIsEmpty                     , false);
            initSetting(EM_QSetting.ErrorShowMissingEventMethod                , true);
            initSetting(EM_QSetting.ErrorShowWhenTagOrLayerIsUndefined         , true);
            initSetting(EM_QSetting.ErrorIgnoreString                          , "");
            initSetting(EM_QSetting.ErrorShowForDisabledComponents             , true);
            initSetting(EM_QSetting.ErrorShowForDisabledGameObjects            , true);

            initSetting(EM_QSetting.RendererShow                               , false);
            initSetting(EM_QSetting.RendererShowDuringPlayMode                 , false);

            initSetting(EM_QSetting.PrefabShow                                 , false);
            initSetting(EM_QSetting.PrefabShowBrakedPrefabsOnly               , true);

            initSetting(EM_QSetting.TagAndLayerShow                            , true);
            initSetting(EM_QSetting.TagAndLayerShowDuringPlayMode              , true);
            initSetting(EM_QSetting.TagAndLayerSizeShowType                    , (int)QHierarchyTagAndLayerShowType.TagAndLayer);
            initSetting(EM_QSetting.TagAndLayerType                            , (int)QHierarchyTagAndLayerType.OnlyIfNotDefault);
            initSetting(EM_QSetting.TagAndLayerAlignment                        , (int)QHierarchyTagAndLayerAligment.Left);
            initSetting(EM_QSetting.TagAndLayerSizeValueType                   , (int)QHierarchyTagAndLayerSizeType.Pixel);
            initSetting(EM_QSetting.TagAndLayerSizeValuePercent                , 0.25f);
            initSetting(EM_QSetting.TagAndLayerSizeValuePixel                  , 75);
            initSetting(EM_QSetting.TagAndLayerLabelSize                       , (int)QHierarchyTagAndLayerLabelSize.Normal);
            initSetting(EM_QSetting.TagAndLayerTagLabelColor                   , "FFCCCCCC", "FF333333");
            initSetting(EM_QSetting.TagAndLayerLayerLabelColor                 , "FFCCCCCC", "FF333333");
            initSetting(EM_QSetting.TagAndLayerLabelAlpha                      , 0.35f);

            initSetting(EM_QSetting.ColorShow                                  , true);
            initSetting(EM_QSetting.ColorShowDuringPlayMode                    , true);

            initSetting(EM_QSetting.GameObjectIconShow                         , false);
            initSetting(EM_QSetting.GameObjectIconShowDuringPlayMode           , true);
            initSetting(EM_QSetting.GameObjectIconSize                         , (int)QHierarchySizeAll.Small);

            initSetting(EM_QSetting.TagIconShow                                , false);
            initSetting(EM_QSetting.TagIconShowDuringPlayMode                  , true);
            initSetting(EM_QSetting.TagIconListFoldout                         , false);
            initSetting(EM_QSetting.TagIconList                                , "");
            initSetting(EM_QSetting.TagIconSize                                , (int)QHierarchySizeAll.Small);

            initSetting(EM_QSetting.LayerIconShow                              , false);
            initSetting(EM_QSetting.LayerIconShowDuringPlayMode                , true);
            initSetting(EM_QSetting.LayerIconListFoldout                       , false);
            initSetting(EM_QSetting.LayerIconList                              , "");
            initSetting(EM_QSetting.LayerIconSize                              , (int)QHierarchySizeAll.Small);

            initSetting(EM_QSetting.ChildrenCountShow                          , false);     
            initSetting(EM_QSetting.ChildrenCountShowDuringPlayMode            , true);
            initSetting(EM_QSetting.ChildrenCountLabelSize                     , (int)QHierarchySize.Normal);
            initSetting(EM_QSetting.ChildrenCountLabelColor                    , "FFCCCCCC", "FF333333");

            initSetting(EM_QSetting.VerticesAndTrianglesShow                   , false);
            initSetting(EM_QSetting.VerticesAndTrianglesShowDuringPlayMode     , false);
            initSetting(EM_QSetting.VerticesAndTrianglesCalculateTotalCount    , false);
            initSetting(EM_QSetting.VerticesAndTrianglesShowTriangles          , false);
            initSetting(EM_QSetting.VerticesAndTrianglesShowVertices           , true);
            initSetting(EM_QSetting.VerticesAndTrianglesLabelSize              , (int)QHierarchySize.Normal);
            initSetting(EM_QSetting.VerticesAndTrianglesVerticesLabelColor     , "FFCCCCCC", "FF333333");
            initSetting(EM_QSetting.VerticesAndTrianglesTrianglesLabelColor    , "FFCCCCCC", "FF333333");

            initSetting(EM_QSetting.ComponentsShow                             , false);
            initSetting(EM_QSetting.ComponentsShowDuringPlayMode               , false);
            initSetting(EM_QSetting.ComponentsIconSize                         , (int)QHierarchySizeAll.Small);
            initSetting(EM_QSetting.ComponentsIgnore                           , "");

            initSetting(EM_QSetting.ComponentsOrder                            , DEFAULT_ORDER);

            initSetting(EM_QSetting.AdditionalShowObjectListContent            , false);
            initSetting(EM_QSetting.AdditionalShowHiddenQHierarchyObjectList   , true);
            initSetting(EM_QSetting.AdditionalHideIconsIfNotFit                , true);
            initSetting(EM_QSetting.AdditionalIndentation                       , 0);
            initSetting(EM_QSetting.AdditionalShowModifierWarning              , true);

            #if UNITY_2019_1_OR_NEWER
            initSetting(EM_QSetting.AdditionalBackgroundColor                  , "00383838", "00CFCFCF");
            #else
            initSetting(QSetting.AdditionalBackgroundColor                  , "00383838", "00C2C2C2");
            #endif
            initSetting(EM_QSetting.AdditionalActiveColor                      , "FFFFFF80", "CF363636");
            initSetting(EM_QSetting.AdditionalInactiveColor                    , "FF4F4F4F", "1E000000");
            initSetting(EM_QSetting.AdditionalSpecialColor                     , "FF2CA8CA", "FF1D78D5");
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
        public T Get<T>(EM_QSetting setting)
        {
            return (T)settingsObject.Get<T>(getSettingName(setting));
        }

        public Color getColor(EM_QSetting setting)
        {
            string stringColor = (string)settingsObject.Get<string>(getSettingName(setting));
            return QColorUtils.fromString(stringColor);
        }

        public void setColor(EM_QSetting setting, Color color)
        {
            string stringColor = QColorUtils.toString(color);
            Set(setting, stringColor);
        }

        public void Set<T>(EM_QSetting setting, T value, bool invokeChanger = true)
        {
            var settingId = (int)setting;
            settingsObject.Set(getSettingName(setting), value);

            if (invokeChanger && settingChangedHandlerList.ContainsKey(settingId) && settingChangedHandlerList[settingId] != null)
            {
                settingChangedHandlerList[settingId].Invoke();
            }
            
            EditorApplication.RepaintHierarchyWindow();
        }

        public void addEventListener(EM_QSetting setting, QSettingChangedHandler handler)
        {
            int settingId = (int)setting;
            
            if (!settingChangedHandlerList.ContainsKey(settingId))          
                settingChangedHandlerList.Add(settingId, null);
            
            if (settingChangedHandlerList[settingId] == null)           
                settingChangedHandlerList[settingId] = handler;
            else            
                settingChangedHandlerList[settingId] += handler;
        }
        
        public void removeEventListener(EM_QSetting setting, QSettingChangedHandler handler)
        {
            int settingId = (int)setting;
            
            if (settingChangedHandlerList.ContainsKey(settingId) && settingChangedHandlerList[settingId] != null)       
                settingChangedHandlerList[settingId] -= handler;
        }
        
        public void restore(EM_QSetting setting)
        {
            Set(setting, defaultSettings[(int)setting]);
        }

        // PRIVATE
        private void initSetting(EM_QSetting setting, object defaultValueDark, object defaultValueLight)
        {
            skinDependedSettings.Add((int)setting);
            initSetting(setting, EditorGUIUtility.isProSkin ? defaultValueDark : defaultValueLight);
        }
        
        private void initSetting(EM_QSetting setting, object defaultValue)
        {
            string settingName = getSettingName(setting);
            defaultSettings.Add((int)setting, defaultValue);
            object value = settingsObject.Get(settingName, defaultValue);
            if (value == null || value.GetType() != defaultValue.GetType())
            {
                settingsObject.Set(settingName, defaultValue);
            }        
        }

        private string getSettingName(EM_QSetting setting)
        {
            int settingId = (int)setting;
            string settingName = PREFS_PREFIX;
            if (skinDependedSettings.Contains(settingId))            
                settingName += EditorGUIUtility.isProSkin ? PREFS_DARK : PREFS_LIGHT;            
            settingName += setting.ToString("G");
            return settingName.ToString();
        }
	}
}
