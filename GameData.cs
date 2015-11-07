using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace GameData
{
    //public abstract class GameData
    //{
    //    public int id;

    //    protected static Dictionary<int, T> GetDataMap<T>()
    //    {
    //        Dictionary<int, T> dataMap;
    //        Stopwatch sw = new Stopwatch();
    //        sw.Start();
    //        var type = typeof(T);
    //        var fileNameField = type.GetField("fileName");
    //        if (fileNameField != null)
    //        {
    //            var fileName = fileNameField.GetValue(null) as String;
    //            var result = GameDataControler.Instance.FormatData(fileName, typeof(Dictionary<int, T>), type);
    //            dataMap = result as Dictionary<int, T>;
    //        }
    //        else
    //        {
    //            dataMap = new Dictionary<int, T>();
    //        }
    //        sw.Stop();
    //        LoggerHelper.Info(String.Concat(type, " time: ", sw.ElapsedMilliseconds));
    //        return dataMap;
    //    }
    //}
    //public abstract class GameData<T> : GameData where T : GameData<T>
    //{
    //    public static implicit operator bool(GameData<T> c)
    //    {
    //        return c != null;
    //    }

    //    private static Dictionary<int, T> m_dataMap;

    //    public static Dictionary<int, T> dataMap
    //    {
    //        get
    //        {
    //            if (m_dataMap == null)
    //                m_dataMap = GetDataMap<T>();
    //            return m_dataMap;
    //        }
    //        set { m_dataMap = value; }
    //    }
    //    public static T Get(int id)
    //    {
    //        if (dataMap != null && dataMap.ContainsKey(id))
    //            return dataMap[id];
    //        else
    //        {
    //            var fileNameField = typeof(T).GetField("fileName");
    //            if (fileNameField != null)
    //            {
    //                var fileName = fileNameField.GetValue(null) as String;
    //                string error = string.Format("Config=> 文件名：" + fileName + " error id: " + id.ToString());
    //                UnityEngine.Debug.LogWarning(error);
    //            }
    //        }
    //        return null;
    //    }
    //    public static void SaveGameData()
    //    {
    //        var fileNameField = typeof(T).GetField("fileName");
    //        if (fileNameField != null)
    //        {
    //            var fileName = fileNameField.GetValue(null) as String;
    //            var fullName = Application.dataPath + "/Resources/data/" + fileName;
    //            GameLogic.SaveGameData(dataMap, fullName);
    //            Message.FadeMsg("保存成功=>" + Fun.EnsureFileNameWithExtenision_XML(fileName));
    //        }
    //    }
    //}
    
    public class GameData
    {
        public int id;

        protected static Dictionary<int, T> GetDataMap<T>()
        {
            Dictionary<int, T> dataMap;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var type = typeof(T);
            var fileNameField = type.GetField("fileName");
            if (fileNameField != null)
            {
                var fileName = fileNameField.GetValue(null) as String;
                var result = GameDataLoader.Instance.FormatData(fileName, typeof(Dictionary<int, T>), type);
                dataMap = result as Dictionary<int, T>;
            }
            else
            {
                dataMap = new Dictionary<int, T>();
            }
            sw.Stop();
            ALog.info(String.Concat(type, " time: ", sw.ElapsedMilliseconds));
            return dataMap;
        }
    }

    public abstract class GameData<T> : GameData where T : GameData<T>
    {
        public static implicit operator bool(GameData<T> c)
        {
            return c != null;
        }

        private static Dictionary<int, T> m_dataMap;

        public static Dictionary<int, T> dataMap
        {
            get
            {
                if (m_dataMap == null)
                    m_dataMap = GetDataMap<T>();
                return m_dataMap;
            }
            set { m_dataMap = value; }
        }
        public static T Get(int id)
        {
            if (dataMap != null && dataMap.ContainsKey(id))
                return dataMap[id];
            else
            {
                var fileNameField = typeof(T).GetField("fileName");
                if (fileNameField != null)
                {
                    var fileName = fileNameField.GetValue(null) as String;
                    string error = string.Format("Config=> 文件名：" + fileName + " error id: " + id.ToString());
                    ALog.logWarning(error);
                }
            }
            return null;
        }
        private static string getFileName(string filename)
        {
            return Setting.ResourcePath + Setting.CONFIG_SUB_FOLDER + filename; ;
        }
        public static void SaveGameData()
        {
            var fileNameField = typeof(T).GetField("fileName");
            if (fileNameField != null)
            {
                var fileName = fileNameField.GetValue(null) as String;
                var fullName = getFileName(fileName);
                AResource.SaveGameData(dataMap, fullName);
                ALog.info("保存成功=>" + Fun.EnsureFileNameWithExtenision_XML(fileName));
            }
        }
    }
}
