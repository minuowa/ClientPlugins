using Data;
using GameData;
using Mono.Xml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;

public class GameDataLoader
{
    /// <summary>
    /// 
    /// </summary>
    public Func<string, string> textLoader;

    private List<Type> mTypeList = new List<Type>();
    
    private Action mFinishCallBack;

    private static GameDataLoader mInstance;

    public static GameDataLoader Instance
    {

        get {
            if (null == mInstance)
                mInstance = new GameDataLoader();
            return mInstance; 
        }
    }

    private GameDataLoader()
    {
    }

    public void Init(Action<int, int> progress = null, Action finished = null)
    {
        mFinishCallBack = finished;
        this.LoadData(mTypeList, FormatXMLData, progress);
    }


    /// <summary>
    /// 加载数据逻辑
    /// </summary>
    /// <param name="gameDataType">加载数据列表</param>
    /// <param name="formatData">处理数据方法</param>
    /// <param name="progress">数据处理进度</param>
    private void LoadData(List<Type> gameDataType, Func<string, Type, Type, object> formatData, Action<int, int> progress)
    {
        List<string> nameList = new List<string>();

        var count = gameDataType.Count;
        var i = 1;
        foreach (var item in gameDataType)
        {
            var p = item.GetProperty("dataMap", ~BindingFlags.DeclaredOnly);
            var fileNameField = item.GetField("fileName");
            if (p != null && fileNameField != null)
            {
                var fileName = fileNameField.GetValue(null) as String;
                if (nameList.Contains(fileName))
                {
                    ALog.logError("Repeated File=> " + fileName);
                }
                else
                {
                    nameList.Add(fileName);
                    var result = formatData(Setting.ResourcePath+Setting.CONFIG_SUB_FOLDER+fileName+Setting.XMLExtension
                        , p.PropertyType, item);
                    p.GetSetMethod().Invoke(null, new object[] { result });
                }
            }
            if (progress != null)
                progress(i, count);
            i++;
        }

        if (mFinishCallBack != null)
            mFinishCallBack();
    }

    public object FormatData(string fileName, Type dicType, Type type)
    {
        return FormatXMLData(fileName, dicType, type);
    }

    #region xml
    static string GetTag(string node)
    {
        string tag;
        if (node.Length < 3)
        {
            tag = node;
        }
        else
        {
            var tagTial = node.Substring(node.Length - 2, 2);
            if (tagTial == "_i" || tagTial == "_s" || tagTial == "_f" || tagTial == "_l" || tagTial == "_k" || tagTial == "_m")
                tag = node.Substring(0, node.Length - 2);
            else
                tag = node;
        }
        return tag;
    }
    public static void ParseFromXML(SecurityElement node, Type type, out object obj)
    {
        object pval = Converter.GetValue(type, node.Text);
        if (pval != null)
        {
            obj = pval;
            return;
        }
        try
        {
            var _contor = type.GetConstructor(Type.EmptyTypes).Invoke(null);
        }
        catch (Exception)
        {
            ALog.error("FormatData Error: " + type.Name + " doesn't have an empty param ctor!");
        }


        var t = type.GetConstructor(Type.EmptyTypes).Invoke(null);


        FieldInfo[] props = type.GetFields();

        foreach (FieldInfo prop in props)
        {
            if (prop.MemberType == MemberTypes.Property || prop.MemberType == MemberTypes.Field)
            {
                Type proptype = prop.FieldType;

                if (proptype.IsGenericType && proptype.GetGenericTypeDefinition() == typeof(List<>))
                {
                    var propValue = proptype.GetConstructor(Type.EmptyTypes).Invoke(null);

                    if (node.Attributes != null)
                    {
                        string _tagname = GetTag(prop.Name);

                        if (node.Attributes.ContainsKey(_tagname))
                        {
                            string str = node.Attributes[_tagname] as string;
                            object listVal = Converter.GetValue(proptype, str);
                            if (listVal != null)
                                proptype.GetMethod("AddRange").Invoke(propValue, new object[] { listVal });
                        }
                    }
                    if (node.Children != null)
                    {
                        foreach (var item in node.Children)
                        {
                            var child = (SecurityElement)item;
                            string tag = GetTag(child.Tag);
                            if (tag == prop.Name)
                            {
                                foreach (var cchild in child.Children)
                                {
                                    Type listParamType = proptype.GetGenericArguments()[0];
                                    var listItemValue = listParamType.GetConstructor(Type.EmptyTypes).Invoke(null);
                                    object listobj;
                                    ParseFromXML((SecurityElement)cchild, listParamType, out listobj);

                                    if (listobj != null)
                                    {
                                        proptype.GetMethod("Add").Invoke(propValue, new object[] { listobj });
                                    }
                                }
                            }
                        }
                    }
                    prop.SetValue(t, propValue);
                }
                else
                {
                    if (node.Attributes != null)
                    {
                        foreach (var key in node.Attributes.Keys)
                        {
                            string tag = GetTag((string)key);
                            if (tag == prop.Name)
                            {
                                object pattr = Converter.GetValue(proptype, (string)node.Attributes[tag]);
                                if (pattr != null)
                                    prop.SetValue(t, pattr);
                            }
                        }
                    }
                    if (node.Children != null)
                    {
                        foreach (var item in node.Children)
                        {
                            var child = (SecurityElement)item;
                            string tag = GetTag(child.Tag);
                            if (tag == prop.Name)
                            {
                                object baseobj;
                                ParseFromXML(child, proptype, out baseobj);
                                prop.SetValue(t, baseobj);
                            }
                        }
                    }
                }
            }
        }


        obj = t;
    }
    Boolean LoadIntMap(String fileName, out Dictionary<Int32, SecurityElement> map)
    {
        try
        {
            SecurityElement xml = LoadOutter(fileName);
            if (xml == null)
            {
                ALog.error("File not exist: " + fileName);
                map = null;
                return false;
            }
            else
            {
                map = LoadIntMap(xml, fileName);
                return true;
            }
        }
        catch (Exception ex)
        {
            ALog.error("Load Int Map Error: " + fileName + "  " + ex.Message);
            map = null;
            return false;
        }
    }

    /// <summary>
    /// 从指定的 XML 文档加载 map 数据。
    /// </summary>
    /// <param name="xml">XML 文档</param>
    /// <returns>map 数据</returns>
    Dictionary<Int32, SecurityElement> LoadIntMap(SecurityElement xml, string source)
    {
        var result = new Dictionary<Int32, SecurityElement>();

        var index = 0;
        foreach (SecurityElement subMap in xml.Children)
        {
            index++;
            if (subMap.Attributes == null || subMap.Attributes.Count == 0)
            {
                ALog.warning("empty row in row NO." + index + " of " + source);
                continue;
            }

            if (!subMap.Attributes.ContainsKey("id"))
            {
                ALog.warning("Invalid Record ID" + index + " of " + source);
                continue;
            }

            Int32 key = Int32.Parse(subMap.Attributes["id"] as string);
            if (result.ContainsKey(key))
            {
                ALog.warning(String.Format("Key {0} already exist, in {1}.", key, source));
                continue;
            }

            result.Add(key, subMap);
        }
        return result;
    }
    object FormatXMLData(string fileName, Type dicType, Type type)
    {
        object result = null;
        try
        {
            result = dicType.GetConstructor(Type.EmptyTypes).Invoke(null);
            Dictionary<Int32, SecurityElement> map;//int32 为 id, string 为 属性名, string 为 属性值
            if (LoadIntMap(fileName, out map))
            {
                foreach (var item in map)
                {
                    object obj;
                    ParseFromXML(item.Value, type, out obj);
                    if (obj != null)
                        dicType.GetMethod("Add").Invoke(result, new object[] { item.Key, obj });
                }
            }
        }
        catch (Exception ex)
        {
            ALog.error("FormatData Error: " + fileName + "  " + ex.Message);
        }

        return result;
    }
    public SecurityElement LoadOutter(String fileName)
    {
        String xmlText = textLoader.Invoke(fileName);
        if (String.IsNullOrEmpty(xmlText))
            return null;
        else
            return LoadXML(xmlText);
    }

    /// <summary>
    /// 从指定的字符串加载 XML 文档。
    /// </summary>
    /// <param name="xml">包含要加载的 XML 文档的字符串。</param>
    /// <returns>编码安全对象的 XML 对象模型。</returns>
    public static SecurityElement LoadXML(String xml)
    {
        try
        {
            SecurityParser securityParser = new SecurityParser();
            securityParser.LoadXml(xml);
            return securityParser.ToXml();
        }
        catch (Exception ex)
        {
            ALog.except(ex);
            return null;
        }
    }
    #endregion

}
