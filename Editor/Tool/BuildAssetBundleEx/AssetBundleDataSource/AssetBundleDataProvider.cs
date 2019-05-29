using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AssetBundleBrowser
{
    internal class AssetBundleDataProvider
    {
        private static List<Type> s_CustomAssetBundleDataTypes;

        internal static List<Type> customAssetBundleDataTypes
        {
            get
            {
                if (s_CustomAssetBundleDataTypes == null)
                {
                    s_CustomAssetBundleDataTypes = BuildCustomAssetBundleDataTypes();
                }
                return s_CustomAssetBundleDataTypes;
            }
        }

        private static List<Type> BuildCustomAssetBundleDataTypes()
        {
            var properList = new List<Type>();
            properList.Add(null); //empty spot for "default" 
            var x = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in x)
            {
                try
                {
                    var list = new List<Type>(
                        assembly
                        .GetTypes()
                        .Where(t => t != typeof(AssetBundleData))
                        .Where(t => typeof(AssetBundleData).IsAssignableFrom(t)));


                    for (int count = 0; count < list.Count; count++)
                    {
                        if (list[count].Name == "AssetDatabaseAssetBundleData")
                            properList[0] = list[count];
                        else if (list[count] != null)
                            properList.Add(list[count]);
                    }
                }
                catch (System.Exception)
                {
                    //assembly which raises exception on the GetTypes() call - ignore it
                }
            }


            return properList;
        }
    }
}
