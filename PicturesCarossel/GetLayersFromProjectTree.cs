using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraExplorerX;
using DotSpatial.Data;
using NetTopologySuite.Geometries;
using DotSpatial.Topology;
using System.Data;


namespace TerraAdds;

public static class GetLayersFromProjectTree
{
    private static SGWorld80 SGWorld = new SGWorld80();
    private static Dictionary<string, string>? layersDictionarie; //(name, id)
    private static int newNameInt = 0;
    public static Dictionary<string, string> GetLayers()
    {

        if (layersDictionarie == null || layersDictionarie.Count == 0)
        {
            return LoadLayerToDictionary();
        }
        return layersDictionarie;

    }
    private static Dictionary<string, string> LoadLayerToDictionary()
    {
        var root = SGWorld.ProjectTree.GetNextItem(string.Empty, ItemCode.ROOT);
        if (SGWorld.ProjectTree.GetItemName(root) == SGWorld.ProjectTree.HiddenGroupName)
            root = SGWorld.ProjectTree.GetNextItem(root, ItemCode.NEXT);

        Dictionary<string, string> temp = BuildTreeRecursive(root, 1, LayerGeometryType.LGT_POINT);
        if (temp.Count <= 0)
            throw new Exception("Project Tree Have No Layers");
        layersDictionarie = temp;
        return layersDictionarie;
    }
    private static Dictionary<string, string> BuildTreeRecursive(string current, int indent, LayerGeometryType layerGeometryType, string path = "")
    {
        Dictionary<string, string> result = new();
        // iterate over all siblings of current node
        while (string.IsNullOrEmpty(current) == false)
        {
            // append node name to the tree string

            var currentName = SGWorld.ProjectTree.GetItemName(current);
            var currentId = SGWorld.ProjectTree.FindItem(path + '\\' + currentName);

            ITerraExplorerObject80 currentObject;
            ObjectTypeCode currentTypeCode;

            try
            {
                currentObject = SGWorld.ProjectTree.GetObject(currentId);

                if (currentObject is null)
                {
                    throw (new Exception());
                }
                currentTypeCode = currentObject.ObjectType;
            }
            catch (Exception e)
            {
                currentTypeCode = ObjectTypeCode.OT_UNDEFINED;
            }

            try
            {
                if (currentTypeCode == ObjectTypeCode.OT_FEATURE_LAYER)
                {
                    if (SGWorld.ProjectTree.GetLayer(currentId).GeometryType == layerGeometryType)
                    {
                        string? value;
                        bool found = result.TryGetValue(currentName, out value);

                        if (found == false)
                            result.Add(currentName, currentId);
                        else
                        {
                            SGWorld.ProjectTree.GetLayer(currentId).TreeItem.Name = currentName + newNameInt++;
                            result.Add(SGWorld.ProjectTree.GetLayer(currentId).TreeItem.Name, currentId);
                        }
                    }
                }
                // if current node is group, recursively build tree from its first child;

                if (SGWorld.ProjectTree.IsGroup(current))
                {

                    var child = SGWorld.ProjectTree.GetNextItem(current, ItemCode.CHILD);
                    if (path == "")
                    {
                        //merge dictionaries
                        var temp = BuildTreeRecursive(child, indent + 1, layerGeometryType, path + currentName);
                        foreach (var kvp in temp)
                        {
                            if (!result.ContainsKey(kvp.Key))
                            {
                                result.Add(kvp.Key, kvp.Value);
                            }
                        }
                    }
                    else
                    {
                        //merge dictionaries
                        var temp = BuildTreeRecursive(child, indent + 1, layerGeometryType, path + '\\' + currentName);
                        foreach (var kvp in temp)
                        {
                            if (!result.ContainsKey(kvp.Key))
                            {
                                result.Add(kvp.Key, kvp.Value);
                            }
                        }
                    }

                }
            }
            catch (Exception e)
            {
                throw new Exception("Field to load Layers");
            }

            // move to next sibling
            current = SGWorld.ProjectTree.GetNextItem(current, ItemCode.NEXT);
        }

        return result;
    }
    public static Dictionary<int, string> GetFiledsBasedOnId(string LayerId)
    {
        IAttributes80 atrib = SGWorld.ProjectTree.GetLayer(LayerId).DataSourceInfo.Attributes;
        Dictionary<int, string> keyValuePairs = new Dictionary<int, string>();
        for (int i=0; i<atrib.Count; i++)
        {
            keyValuePairs.Add(i, atrib.Attribute[i].Name);
        }
        return keyValuePairs;
    }
    public static string? GetLayerId(string layerName)
    {
        return layersDictionarie![layerName];
    }
    public static List<string> GetatributtesFromField(string layer, string field)
    {

        IFeatureLayer80 featureLayer = SGWorld.ProjectTree.GetLayer(GetLayerId(layer));
        List<string> values = new();
        string condition = $"{field} <> '-999'";

        IFeatures80 features = featureLayer.ExecuteQuery(
            condition,
            -1
            )
        ;

        for (int j = 0; j < features.Count; j++)
        {
            values.Add(features[j].FeatureAttributes.GetFeatureAttribute(field).Value);
        }

        //featureLayer.Streaming = false;

        //for(int i=0; i< featureLayer.FeatureGroups.Count; i++)
        //{
        //    IFeatures80 features = featureLayer.FeatureGroups[i].Item(i).GetCurrentFeatures();
        //    for(int j=0; j< features.Count; j++)
        //    {
        //        values.Add(features[j].FeatureAttributes.GetFeatureAttribute(field).Value);
        //    }
        //}

        return values;

    }
    public static List<List<string>> GetFeatureAtributtesBasedOnFilter(string layer, string field, string filterValue = "All")
    {

        var myFields = GetFiledsBasedOnId(GetLayerId(layer)).Values;



        try
        {
            IFeatureLayer80 featureLayer = SGWorld.ProjectTree.GetLayer(GetLayerId(layer));

            List<List<string>> raws = new();
            string condition = "";
            if (filterValue == "All")
            {
                condition = $"{field} <> '{filterValue}'";
            }
            else
            {
                condition = $"{field} = '{filterValue}'";
            }


            IFeatures80 features = featureLayer.ExecuteQuery(condition, -1);
            int atributtesCount = features[0].FeatureAttributes.Count;

            var layerPath = SGWorld.ProjectTree.GetLayer(GetLayerId(layer)).DataSourceInfo.ConnectionString;
            GetShapeFileAtribbutes(GetFilePathFromConnectionString(layerPath));
            return GetShapeFileAtribbutesWithFilter(GetFilePathFromConnectionString(layerPath));

            return raws;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message, ex);
        }
        
    }
    public static List<List<string>> GetFeatureAtributtesBasedOn2Filters(string layer, string field, string field2, string filterValue = "All", string filterValue2 = "-All")
    {
        try
        {
            IFeatureLayer80 featureLayer = SGWorld.ProjectTree.GetLayer(GetLayerId(layer));

            List<List<string>> raws = new();
            string condition = "";
            if (filterValue == "All")
                
            {
                condition = $"{field2} = '{filterValue2}'";
            }
            else
            {
                condition = $"{field} = '{filterValue}' AND {field2} = '{filterValue2}'";
            }

            IFeatures80 features = featureLayer.ExecuteQuery(
                condition,
                -1
                )
            ;

            for (int j = 0; j < features.Count; j++)
            {
                List<string> values = new();

                IFeature80 feature = features[j];
                IFeatureAttributes80 featureAttribute80 = feature.FeatureAttributes;
                int atributtesCount = featureAttribute80.Count;

                for (int i = 0; i < atributtesCount; i++)
                {
                    values.Add(featureAttribute80[i].Value);
                }

                raws.Add(values);
            }
            return raws;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message, ex);
        }

    }

    /// <summary>
    /// Return all Atributtes from all features inside a shapefile using DotSpatial
    /// The atributtes include the xyz in case thefeature ait is a point feature.
    /// In case of filter using, include field and filterValue
    /// </summary>
    /// <param name="path"></param>
    /// <param name="field"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    private static List<List<string>> GetShapeFileAtribbutesWithFilter(string path, string field = null, string value = null)
    {
        IFeatureSet featureSet = FeatureSet.Open(path);

        List<List<string>> allAttributes = new List<List<string>>();

        // Loop through features
        foreach (IFeature feature in featureSet.Features)
        {

            if (field != null && value != null) //Filter
            {
                if (feature.DataRow[field].ToString() == value)
                {
                    // List to hold attributes of current feature
                    List<string> featureAttributes = new List<string>();

                    // Loop through attributes of current feature
                    for (int i = 0; i < feature.DataRow.ItemArray.Length; i++)
                    {
                        object attribute = feature.DataRow[i];
                        // Convert attribute to string and add to list
                        featureAttributes.Add(attribute.ToString());
                    }

                    NetTopologySuite.Geometries.Geometry geometry = feature.Geometry;
                    if (geometry != null)
                    {
                        
                        if (geometry is NetTopologySuite.Geometries.Point point)
                        {
                            // For point geometry
                            NetTopologySuite.Geometries.Coordinate coordinate = point.Coordinate;
                            featureAttributes.Add(coordinate.X.ToString());
                            featureAttributes.Add(coordinate.Y.ToString());
                            featureAttributes.Add(coordinate.Z.ToString());

                        }

                    }

                    // Add list of attributes to the list of all attributes
                    allAttributes.Add(featureAttributes);
                }
            }
            else //No Filter
            {
                // List to hold attributes of current feature
                List<string> featureAttributes = new List<string>();

                // Loop through attributes of current feature
                for (int i = 0; i < feature.DataRow.ItemArray.Length; i++)
                {
                    object attribute = feature.DataRow[i];
                    // Convert attribute to string and add to list
                    featureAttributes.Add(attribute.ToString());
                }
                NetTopologySuite.Geometries.Geometry geometry = feature.Geometry;
                if (geometry != null)
                {

                    if (geometry is NetTopologySuite.Geometries.Point point)
                    {
                        // For point geometry
                        NetTopologySuite.Geometries.Coordinate coordinate = point.Coordinate;
                        featureAttributes.Add(coordinate.X.ToString());
                        featureAttributes.Add(coordinate.Y.ToString());
                        featureAttributes.Add(coordinate.Z.ToString());

                    }

                }
                // Add list of attributes to the list of all attributes
                allAttributes.Add(featureAttributes);
            }
        }
        // Dispose the feature set
        featureSet.Dispose();
        return allAttributes;
    }
    
    static public string GetLayerPathFromLayerName(string name)
    {
        return GetFilePathFromConnectionString(SGWorld.ProjectTree.GetLayer(GetLayerId(name)).DataSourceInfo.ConnectionString);
    }
    static private string GetFilePathFromConnectionString(string connectionString)
    {
        string[] parts = connectionString.Split(';');
        foreach (string part in parts)
        {
            if (part.StartsWith("FileName="))
            {
                return part.Substring("FileName=".Length);
            }
        }
        return null; // If "FileName" parameter is not found
    }

    /// <summary>
    /// Reads data from a shapefile and returns it as a list of dictionaries.
    /// Each dictionary represents a feature from the shapefile, with field names as keys and attribute values as values.
    /// </summary>
    /// <param name="path">The path to the shapefile.</param>
    /// <returns>A list of dictionaries containing the attributes of each feature.</returns>
    public static List<Dictionary<string ,string>> GetShapeFileAtribbutes(string layerName)
    {
        var path = GetLayerPathFromLayerName(layerName);

        IFeatureSet featureSet = FeatureSet.Open(path);
        
        List<Dictionary<string, string>> allAttributes = new();

        var attributeTable = featureSet.DataTable; 

        int columIndex = 0;
        foreach (IFeature feature in featureSet.Features)
        {
            Dictionary<string, string> featureAttributes = new();

            // Loop through attributes of current feature
            for (int i = 0; i < feature.DataRow.ItemArray.Length; i++)
            {
                object attribute = feature.DataRow[i];
                // Convert attribute to string and add to list [column name, value]
                featureAttributes.Add(attributeTable.Columns[columIndex++].ColumnName, 
                                                                        attribute.ToString());
            }
            columIndex = 0;
            NetTopologySuite.Geometries.Geometry geometry = feature.Geometry;
            if (geometry != null)
            {
                if (geometry is NetTopologySuite.Geometries.Point point)
                {
                    // For point geometry
                    NetTopologySuite.Geometries.Coordinate coordinate = point.Coordinate;
                    featureAttributes.Add("X", coordinate.Y.ToString());
                    featureAttributes.Add("Y",coordinate.Y.ToString());
                    featureAttributes.Add("Z",coordinate.Z.ToString());
                }
            }
            allAttributes.Add(featureAttributes);
        }
        return allAttributes;
    }


    /// <summary>
    /// Filters a list of dictionaries based on multiple filter criteria.
    /// </summary>
    /// <param name="features">The list of dictionaries representing features from the shapefile.</param>
    /// <param name="filters">A dictionary where keys represent the field names to filter by, and values represent the desired filter values.</param>
    /// <returns>A list of dictionaries representing filtered features.</returns>
    static public List<Dictionary<string, string>> FilterFeatures(List<Dictionary<string, string>> features, Dictionary<string, string> filters)
    {
        List<Dictionary<string, string>> filteredFeatures = new();

        foreach (var feature in features)
        {
            bool passFilters = true;
            foreach (var filter in filters)
            {
                if (feature[filter.Key] != filter.Value)
                    passFilters = false;
            }
            if (passFilters)
                filteredFeatures.Add(feature);
        }

        return filteredFeatures;
    }
}
