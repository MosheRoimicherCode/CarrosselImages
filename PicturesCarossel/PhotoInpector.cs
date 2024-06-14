using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraExplorerX;

namespace TerraAdds;

public static class PhotoInpector
{
    private static SGWorld80 SGWorld = new SGWorld80();
    public static Action? PositionDefined;
    private static _3DCoordinates myPos = new _3DCoordinates();
    private static Dictionary<string, double> imageList = new();
    private static double CalculateDistance(double x1, double y1, double z1, double x2, double y2, double z2)
    {
        double deltaX = x2 - x1;
        double deltaY = y2 - y1;
        double deltaZ = z2 - z1;

        return Math.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);
    }
    public static Dictionary<string, double> GetPicturesInsideSphericRadius(string? featureLayerSelected_id, double radius, string pathField)
    {
        double distance = double.MaxValue;
        imageList.Clear();
        
        if (featureLayerSelected_id == null)
            throw new Exception("Layer not found");

        IFeatureLayer80 featureLayer = SGWorld.ProjectTree.GetLayer(featureLayerSelected_id);
        //featureLayer.Streaming = false;
        
        IFeatures80 featureGroup80 = featureLayer.FeatureGroups.Point.GetCurrentFeatures();
        foreach (IFeature80 feature in featureGroup80)
        {
            //distance = CalculateDistance(mouseClickPosition, (IPoint)feature.Geometry);
            distance = CalculateDistance(myPos.X, myPos.Y, myPos.Z, ((IPoint)feature.Geometry).X, ((IPoint)feature.Geometry).Y, ((IPoint)feature.Geometry).Z);
            if (distance < radius)
            {
                imageList.Add(GetImagePathFromField(feature, pathField), distance);
            }
        }
        return imageList;
    }
    private static string GetImagePathFromField(IFeature80 feature, string fieldSelected)
    {
        return feature.FeatureAttributes.GetFeatureAttribute(fieldSelected).Value;
    }
    public static void CreateFeature(string layerName, string atribbutes, LayerGeometryType geometryType = LayerGeometryType.LGT_POINT)
    {
        var id = TerraAdds.GetLayersFromProjectTree.GetLayerId(layerName);
        IFeatureLayer80 layer = SGWorld.ProjectTree.GetLayer(id);
        layer.Streaming = false;
        IFeatureGroup80 fgroup = geometryType switch
        {
            LayerGeometryType.LGT_POINT => layer.FeatureGroups.Point,
            LayerGeometryType.LGT_POLYLINE => layer.FeatureGroups.Polyline,
            LayerGeometryType.LGT_POLYGON => layer.FeatureGroups.Polygon,
            _ => throw new ArgumentException("Invalid Layer Type")
        };
        double[] arr = new double[] { myPos.X, myPos.Y, myPos.Z };
        var geo = SGWorld.Creator.GeometryCreator.CreatePointGeometry(arr);
        fgroup.CreateFeature(geo, atribbutes);
        layer.Save();
        layer.Refresh();
    }
    public static void PickOn3DWorld()
    {
        string executablePath = AppDomain.CurrentDomain.BaseDirectory;
        string fullPath = Path.Combine(executablePath, "cursor_m.cur");
        SGWorld.Window.Cursor = fullPath;
        
        SGWorld.OnLButtonClicked += OnLButtonClicked;
        SGWorld.Command.Execute(1023,"");
    }

    private static bool OnLButtonClicked(int Flags, int X, int Y)
    {
        IPosition80 pos = SGWorld.Window.PixelToWorld(X, Y, WorldPointType.WPT_ACCURATE_CPT).Position;
        SGWorld.OnLButtonClicked -= OnLButtonClicked;

        myPos.X = pos.X;
        myPos.Y = pos.Y;
        myPos.Z = pos.Altitude;

        PositionDefined?.Invoke();
        return true;
    }

    
}
