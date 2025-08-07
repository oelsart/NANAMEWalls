using RimWorld.Planet;
using System.Xml;
using Verse;

namespace NanameWalls
{
    /// <summary>
    /// Dictionary<string, T>を<key>value</key>の形で保存したい
    /// </summary>
    public static class Scribe_StringKeyDictionary
    {
        public static string ProcessingKey { get; private set; }

        public static void Look<V>(ref Dictionary<string, V> dict, string label, LookMode valueLookMode = LookMode.Undefined)
        {
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if (valueLookMode == LookMode.Reference)
                {
                    Log.Error("You need to provide working lists for the keys and values in order to be able to load such dictionary. label=" + label);
                }
            }
            List<string> keysWorkingList = null;
            List<V> valuesWorkingList = null;
            Look(ref dict, label, valueLookMode, ref keysWorkingList, ref valuesWorkingList);
        }

        public static void Look<V>(ref Dictionary<string, V> dict, string label, LookMode valueLookMode, ref List<string> keysWorkingList, ref List<V> valuesWorkingList, bool logNullErrors = true, bool saveDestroyedValues = false)
        {
            if (Scribe.mode == LoadSaveMode.Saving && dict == null)
            {
                Scribe.saver.WriteAttribute("IsNull", "True");
            }
            else
            {
                if (Scribe.mode == LoadSaveMode.LoadingVars)
                {
                    XmlAttribute xmlAttribute = Scribe.loader.curXmlParent.Attributes["IsNull"];
                    if (xmlAttribute != null && xmlAttribute.Value.Equals("true", StringComparison.InvariantCultureIgnoreCase))
                    {
                        dict = null;
                    }
                    else
                    {
                        dict = [];
                    }
                }
                if (Scribe.mode == LoadSaveMode.Saving || Scribe.mode == LoadSaveMode.LoadingVars)
                {
                    keysWorkingList = [];
                    valuesWorkingList = [];
                    if (Scribe.mode == LoadSaveMode.Saving && dict != null)
                    {
                        foreach (KeyValuePair<string, V> item in dict)
                        {
                            keysWorkingList.Add(item.Key);
                            valuesWorkingList.Add(item.Value);
                        }
                    }
                }
                if (Scribe.mode == LoadSaveMode.Saving || dict != null)
                {
                    Look(ref keysWorkingList, ref valuesWorkingList, saveDestroyedValues, label, valueLookMode);
                }
                if (Scribe.mode == LoadSaveMode.Saving)
                {
                    if (keysWorkingList != null)
                    {
                        keysWorkingList.Clear();
                        keysWorkingList = null;
                    }
                    if (valuesWorkingList != null)
                    {
                        valuesWorkingList.Clear();
                        valuesWorkingList = null;
                    }
                }
                bool flag = valueLookMode == LookMode.Reference;
                if ((flag && Scribe.mode == LoadSaveMode.ResolvingCrossRefs) || (!flag && Scribe.mode == LoadSaveMode.LoadingVars))
                {
                    BuildDictionary(dict, keysWorkingList, valuesWorkingList, label, logNullErrors);
                }
                if (Scribe.mode == LoadSaveMode.PostLoadInit)
                {
                    if (keysWorkingList != null)
                    {
                        keysWorkingList.Clear();
                        keysWorkingList = null;
                    }
                    if (valuesWorkingList != null)
                    {
                        valuesWorkingList.Clear();
                        valuesWorkingList = null;
                    }
                }
            }
            return;
        }

        private static void Look<T>(ref List<string> keyList, ref List<T> valueList, bool saveDestroyedThings, string label, LookMode lookMode = LookMode.Undefined, params object[] ctorArgs)
        {
            if (lookMode == LookMode.Undefined && !Scribe_Universal.TryResolveLookMode(typeof(T), out lookMode))
            {
                Log.Error("LookList call with a list of " + typeof(T)?.ToString() + " must have lookMode set explicitly.");
                return;
            }
            if (Scribe.EnterNode(label))
            {
                try
                {
                    if (Scribe.mode == LoadSaveMode.Saving)
                    {
                        if (valueList != null)
                        {
                            var num = Math.Min(keyList.Count, valueList.Count);
                            for (var i = 0; i < num; i++)
                            {
                                ProcessingKey = keyList[i];
                                if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(List<>))
                                {
                                    throw new InvalidOperationException("This case should be impossible; it should be calling the list-of-lists overload.");
                                }
                                switch (lookMode)
                                {
                                    case LookMode.Value:
                                        {
                                            T value4 = valueList[i];
                                            Scribe_Values.Look(ref value4, keyList[i], default, forceSave: true);
                                            break;
                                        }
                                    case LookMode.LocalTargetInfo:
                                        {
                                            LocalTargetInfo value3 = (LocalTargetInfo)(object)valueList[i];
                                            Scribe_TargetInfo.Look(ref value3, saveDestroyedThings, keyList[i]);
                                            break;
                                        }
                                    case LookMode.TargetInfo:
                                        {
                                            TargetInfo value2 = (TargetInfo)(object)valueList[i];
                                            Scribe_TargetInfo.Look(ref value2, saveDestroyedThings, keyList[i]);
                                            break;
                                        }
                                    case LookMode.GlobalTargetInfo:
                                        {
                                            GlobalTargetInfo value = (GlobalTargetInfo)(object)valueList[i];
                                            Scribe_TargetInfo.Look(ref value, saveDestroyedThings, keyList[i]);
                                            break;
                                        }
                                    case LookMode.Def:
                                        {
                                            Def value5 = (Def)(object)valueList[i];
                                            Scribe_Defs.Look(ref value5, keyList[i]);
                                            break;
                                        }
                                    case LookMode.BodyPart:
                                        {
                                            BodyPartRecord part = (BodyPartRecord)(object)valueList[i];
                                            Scribe_BodyParts.Look(ref part, keyList[i]);
                                            break;
                                        }
                                    case LookMode.Deep:
                                        {
                                            T target = valueList[i];
                                            Scribe_Deep.Look(ref target, saveDestroyedThings, keyList[i], ctorArgs);
                                            break;
                                        }
                                    case LookMode.Reference:
                                        {
                                            if (valueList[i] != null && valueList[i] is not ILoadReferenceable)
                                            {
                                                throw new InvalidOperationException("Cannot save reference to " + valueList[i]?.GetType()?.ToStringSafe() + " item if it is not ILoadReferenceable");
                                            }
                                            ILoadReferenceable refee = valueList[i] as ILoadReferenceable;
                                            Scribe_References.Look(ref refee, keyList[i], saveDestroyedThings);
                                            break;
                                        }
                                }
                            }
                            ProcessingKey = null;
                            return;
                        }
                        Scribe.saver.WriteAttribute("IsNull", "True");
                    }
                    else if (Scribe.mode == LoadSaveMode.LoadingVars)
                    {
                        XmlNode curXmlParent = Scribe.loader.curXmlParent;
                        XmlAttribute xmlAttribute = curXmlParent.Attributes["IsNull"];
                        if (xmlAttribute != null && xmlAttribute.Value.Equals("true", StringComparison.InvariantCultureIgnoreCase))
                        {
                            if (lookMode == LookMode.Reference)
                            {
                                Scribe.loader.crossRefs.loadIDs.RegisterLoadIDListReadFromXml(null, null);
                            }
                            keyList = null;
                            valueList = null;
                        }
                        else
                        {
                            keyList = new List<string>(curXmlParent.ChildNodes.Count);
                            switch (lookMode)
                            {
                                case LookMode.Value:
                                    valueList = new List<T>(curXmlParent.ChildNodes.Count);
                                    {
                                        foreach (XmlNode childNode in curXmlParent.ChildNodes)
                                        {
                                            ProcessingKey = childNode.Name;
                                            T item = ScribeExtractor.ValueFromNode(childNode, default(T));
                                            keyList.Add(childNode.Name);
                                            valueList.Add(item);
                                        }
                                        ProcessingKey = null;
                                        break;
                                    }
                                case LookMode.Deep:
                                    valueList = new List<T>(curXmlParent.ChildNodes.Count);
                                    {
                                        foreach (XmlNode childNode2 in curXmlParent.ChildNodes)
                                        {
                                            ProcessingKey = childNode2.Name;
                                            T item7 = ScribeExtractor.SaveableFromNode<T>(childNode2, ctorArgs);
                                            keyList.Add(childNode2.Name);
                                            valueList.Add(item7);
                                        }
                                        ProcessingKey = null;
                                        break;
                                    }
                                case LookMode.Def:
                                    valueList = new List<T>(curXmlParent.ChildNodes.Count);
                                    {
                                        foreach (XmlNode childNode3 in curXmlParent.ChildNodes)
                                        {
                                            ProcessingKey = childNode3.Name;
                                            T item6 = ScribeExtractor.DefFromNodeUnsafe<T>(childNode3);
                                            keyList.Add(childNode3.Name);
                                            valueList.Add(item6);
                                        }
                                        ProcessingKey = null;
                                        break;
                                    }
                                case LookMode.BodyPart:
                                    {
                                        valueList = new List<T>(curXmlParent.ChildNodes.Count);
                                        int num4 = 0;
                                        {
                                            foreach (XmlNode childNode4 in curXmlParent.ChildNodes)
                                            {
                                                ProcessingKey = childNode4.Name;
                                                T item5 = (T)(object)ScribeExtractor.BodyPartFromNode(childNode4, num4.ToString(), null);
                                                keyList.Add(childNode4.Name);
                                                valueList.Add(item5);
                                                num4++;
                                            }
                                            ProcessingKey = null;
                                            break;
                                        }
                                    }
                                case LookMode.LocalTargetInfo:
                                    {
                                        valueList = new List<T>(curXmlParent.ChildNodes.Count);
                                        int num3 = 0;
                                        {
                                            foreach (XmlNode childNode5 in curXmlParent.ChildNodes)
                                            {
                                                ProcessingKey = childNode5.Name;
                                                T item4 = (T)(object)ScribeExtractor.LocalTargetInfoFromNode(childNode5, num3.ToString(), LocalTargetInfo.Invalid);
                                                keyList.Add(childNode5.Name);
                                                valueList.Add(item4);
                                                num3++;
                                            }
                                            ProcessingKey = null;
                                            break;
                                        }
                                    }
                                case LookMode.TargetInfo:
                                    {
                                        valueList = new List<T>(curXmlParent.ChildNodes.Count);
                                        int num2 = 0;
                                        {
                                            foreach (XmlNode childNode6 in curXmlParent.ChildNodes)
                                            {
                                                ProcessingKey = childNode6.Name;
                                                T item3 = (T)(object)ScribeExtractor.TargetInfoFromNode(childNode6, num2.ToString(), TargetInfo.Invalid);
                                                keyList.Add(childNode6.Name);
                                                valueList.Add(item3);
                                                num2++;
                                            }
                                            ProcessingKey = null;
                                            break;
                                        }
                                    }
                                case LookMode.GlobalTargetInfo:
                                    {
                                        valueList = new List<T>(curXmlParent.ChildNodes.Count);
                                        int num = 0;
                                        {
                                            foreach (XmlNode childNode7 in curXmlParent.ChildNodes)
                                            {
                                                ProcessingKey = childNode7.Name;
                                                T item2 = (T)(object)ScribeExtractor.GlobalTargetInfoFromNode(childNode7, num.ToString(), GlobalTargetInfo.Invalid);
                                                keyList.Add(childNode7.Name);
                                                valueList.Add(item2);
                                                num++;
                                            }
                                            ProcessingKey = null;
                                            break;
                                        }
                                    }
                                case LookMode.Reference:
                                    {
                                        List<string> list2 = new(curXmlParent.ChildNodes.Count);
                                        foreach (XmlNode childNode8 in curXmlParent.ChildNodes)
                                        {
                                            ProcessingKey = childNode8.Name;
                                            keyList.Add(childNode8.Name);
                                            list2.Add(childNode8.InnerText);
                                        }
                                        ProcessingKey = null;
                                        Scribe.loader.crossRefs.loadIDs.RegisterLoadIDListReadFromXml(list2, "");
                                        break;
                                    }
                            }
                        }
                    }
                    else if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
                    {
                        switch (lookMode)
                        {
                            case LookMode.Reference:
                                valueList = Scribe.loader.crossRefs.TakeResolvedRefList<T>("");
                                break;
                            case LookMode.LocalTargetInfo:
                                if (valueList != null)
                                {
                                    for (int j = 0; j < valueList.Count; j++)
                                    {
                                        valueList[j] = (T)(object)ScribeExtractor.ResolveLocalTargetInfo((LocalTargetInfo)(object)valueList[j], j.ToString());
                                    }
                                }
                                break;
                            case LookMode.TargetInfo:
                                if (valueList != null)
                                {
                                    for (int k = 0; k < valueList.Count; k++)
                                    {
                                        valueList[k] = (T)(object)ScribeExtractor.ResolveTargetInfo((TargetInfo)(object)valueList[k], k.ToString());
                                    }
                                }
                                break;
                            case LookMode.GlobalTargetInfo:
                                if (valueList != null)
                                {
                                    for (int i = 0; i < valueList.Count; i++)
                                    {
                                        valueList[i] = (T)(object)ScribeExtractor.ResolveGlobalTargetInfo((GlobalTargetInfo)(object)valueList[i], i.ToString());
                                    }
                                }
                                break;
                        }
                    }
                    return;
                }
                finally
                {
                    ProcessingKey = null;
                    Scribe.ExitNode();
                }
            }
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if (lookMode == LookMode.Reference)
                {
                    Scribe.loader.crossRefs.loadIDs.RegisterLoadIDListReadFromXml(null, label);
                }
                keyList = null;
                valueList = null;
            }
        }

        private static void BuildDictionary<V>(Dictionary<string, V> dict, List<string> keysWorkingList, List<V> valuesWorkingList, string label, bool logNullErrors)
        {
            if (dict == null)
            {
                return;
            }
            keysWorkingList ??= [];
            valuesWorkingList ??= [];
            if (keysWorkingList.Count != valuesWorkingList.Count)
            {
                Log.Error("Keys count does not match the values count while loading a dictionary (maybe keys and values were resolved during different passes?). Some elements will be skipped. keys=" + keysWorkingList.Count + ", values=" + valuesWorkingList.Count + ", label=" + label);
            }
            int num = Math.Min(keysWorkingList.Count, valuesWorkingList.Count);
            for (int i = 0; i < num; i++)
            {
                if (keysWorkingList[i] == null)
                {
                    if (logNullErrors)
                    {
                        Log.Error("Null key while loading dictionary of " + typeof(string).ToString() + " and " + typeof(V)?.ToString() + ". label=" + label);
                    }
                    continue;
                }
                try
                {
                    if (dict.TryGetValue(keysWorkingList[i], out var value))
                    {
                        if (!Equals(value, valuesWorkingList[i]))
                        {
                            throw new InvalidOperationException("Tried to add different values for the same key.");
                        }
                    }
                    else
                    {
                        dict.Add(keysWorkingList[i], valuesWorkingList[i]);
                    }
                }
                catch (OutOfMemoryException)
                {
                    throw;
                }
                catch (Exception ex2)
                {
                    Log.Error("Exception in LookDictionary(label=" + label + "): " + ex2);
                }
            }
        }
    }
}