using TerraExplorerX;
using TerraAdds;
using DocumentFormat.OpenXml.Vml.Office;
using NetTopologySuite.Utilities;
using System.Windows.Forms;
using System.Diagnostics;
using static System.Windows.Forms.AxHost;
using DotSpatial.Data;

namespace PicturesCarossel;


public partial class Form1 : Form
{
    private SGWorld74 SGWorld = new();
    private IPosition74 Center;
    private double CirculeRadius;

    double topLeftX;
    double topLeftY;
    double topRightX;
    double bottomLeftY;


    private string? photoLayer;
    private string? imagesFolder;
    private string? imageName = "PicName";

    private List<Dictionary<string, string>> features = new();
    private List<string> featuresVisible = new();
    private List<PictureBox> pictureBoxes = new List<PictureBox>();

    Action action;
    action += populateImages;

    public Form1()
    {
        InitializeComponent();
        comboBox1.Items.AddRange(TerraAdds.GetLayersFromProjectTree.GetLayers().Select(tuple => tuple.Key).ToArray());

        SGWorld.OnFrame += OnFrame;
    }

    private void OnFrame()
    {
        CreatePolygonFromView();
        populateImages();
        //if (photoLayer != null && imageName != null)
        //{
        //    GetFeaturesInsidePolygon();
        //    PopulateImages();
        //}
    }

    private void button1_Click(object sender, EventArgs e)
    {
        //CreatePolygonFromView();
        //GetFeaturesInsidePolygon();
        
    }


    private void CreatePolygonFromView()
    {
        Center = SGWorld.Window.CenterPixelToWorld().Position;
        CirculeRadius = SGWorld.Navigate.GetPosition().Altitude * 0.25;

        topLeftX = Center.X - CirculeRadius;
        topLeftY = Center.Y + CirculeRadius;
        topRightX = Center.X + CirculeRadius;
        bottomLeftY = Center.Y - CirculeRadius;
    }


    public  bool IsPointInsideSquare(double pointX, double pointY)
    {
        return pointX >= topLeftX && pointX <= topRightX &&
               pointY >= bottomLeftY && pointY <= topLeftY;
    }
    private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
    {
        try
        {
            features.Clear();
            photoLayer = comboBox1.SelectedItem.ToString() ?? null;
            if (photoLayer != null)
            {
                //features = GetShapeFileAtribbutesWitchIntersect(photoLayer);
                features = GetLayersFromProjectTree.GetShapeFileAtribbutes(photoLayer);
            }
            comboBox2.Items.AddRange(GetLayersFromProjectTree.GetFiledsBasedOnId(GetLayersFromProjectTree.GetLayerId(photoLayer)).Values.ToArray());
        }
        catch (Exception ex) { MessageBox.Show(ex.Message); }
    }
    private void populateImages()
    {

        var currentCursor = this.Cursor;
        //this.Cursor = Cursors.WaitCursor;

        if (imagesFolder == null)
            return;

        foreach (var box in pictureBoxes)
        {
            panel1.Controls.Remove(box);
        }

        int startX = 2;//12; // Starting X position
        int startY = 2; // Starting Y position
        int spacingX = 40; // Spacing between PictureBoxes

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        var list = GetFeaturesInsidePolygon();

        foreach (var img in list)
        {   

            PictureBox pictureBox = new PictureBox();
            pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBoxes.Add(pictureBox); //Store for future delete

            string imagePath = imagesFolder + "/" + img + " (thubNail).jpg";

            // Set location and size
            pictureBox.Location = new Point(startX, startY);
            pictureBox.Size = new Size(130, 130);

            // Store the full image path in the Tag property
            pictureBox.Tag = imagesFolder + "/" + img + ".jpg";
            ;

            // Add click event handler
            pictureBox.Click += PictureBox_Click;

            // Add PictureBox to the form
            panel1.Controls.Add(pictureBox);

            // Update startX for the next PictureBox
            startX += 130 + spacingX; 

            // Load thumbnail asynchronously
            LoadThumbnailAsync(pictureBox, imagePath);
            //} 
        }

        stopwatch.Stop();
        TimeSpan timeTaken = stopwatch.Elapsed;
        MessageBox.Show($"Time taken to load thumbnail: {timeTaken.TotalSeconds} ms", "Time Taken", MessageBoxButtons.OK, MessageBoxIcon.Information);
        this.Cursor = currentCursor;
    }
    private void LoadThumbnailAsync(PictureBox pictureBox, string imagePath)
    {
        try
        {

            //Stopwatch stopwatch = new Stopwatch();
            //stopwatch.Start();

            Image originalImage = Image.FromFile(imagePath);

            // Set the thumbnail image on the UI thread
            pictureBox.Invoke((Action)(() =>
            {
                pictureBox.Image = originalImage;
            }));

            
        }
        catch (Exception ex)
        {
            // Handle exceptions (e.g., file not found, invalid image format)
            // You can log the error or display an error message to the user
            MessageBox.Show($"Error loading thumbnail: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Open a Image viwer to the selected picture box
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void PictureBox_Click(object sender, EventArgs e)
    {
        PictureBox clickedPictureBox = sender as PictureBox;
        if (clickedPictureBox != null)
        {
            string imagePath = clickedPictureBox.Tag.ToString();
            if (!string.IsNullOrEmpty(imagePath))
            {
                try
                {
                    Process.Start(new ProcessStartInfo(imagePath) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unable to open image: " + ex.Message);
                }
            }
        }
    }
    private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
    {
        imageName = comboBox2.SelectedItem.ToString() ?? null;
    }
    private void LoadFolder_Btn_Click(object sender, EventArgs e)
    {
        try
        {
            FolderBrowserDialog folderDialog = new FolderBrowserDialog();
            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                imagesFolder = folderDialog.SelectedPath;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }
    private List<string> GetFeaturesInsidePolygon()
    {
        featuresVisible.Clear();
        foreach (var feature in features)
        {
            var x = Convert.ToDouble(feature["X"]);
            var y = Convert.ToDouble(feature["Y"]);
            var inside = IsPointInsideSquare(x, y);
            if (inside == true)
            {
                featuresVisible.Add(feature[imageName]);
            }
        }
        return featuresVisible;
    }
    public List<Dictionary<string, string>> GetShapeFileAtribbutesWitchIntersect(string layerName)
    {   
        // Create and start the stopwatch
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        var path = TerraAdds.GetLayersFromProjectTree.GetLayerPathFromLayerName(layerName);

        IFeatureSet featureSet = FeatureSet.Open(path);

        List<Dictionary<string, string>> allAttributes = new();

        var attributeTable = featureSet.DataTable;

        int columIndex = 0;
        foreach (IFeature feature in featureSet.Features)
        {
            NetTopologySuite.Geometries.Geometry geometry = feature.Geometry;

            //Check Intersection
            if (geometry is NetTopologySuite.Geometries.Point point1)
            {
                NetTopologySuite.Geometries.Coordinate coordinate = point1.Coordinate;
                if (IsPointInsideSquare(coordinate.X, coordinate.Y))
                {
                    Dictionary<string, string> featureAttributes = new();

                    // Loop through attributes of current feature
                    for (int i = 0; i < feature.DataRow.ItemArray.Length; i++)
                    {
                        object attribute = feature.DataRow[i];
                        // Convert attribute to string and add to list [column name, value]
                        var columnName = attributeTable.Columns[columIndex++].ColumnName;
                        if (columnName == "PicName")
                            featureAttributes.Add(columnName, attribute.ToString());
                    }
                    columIndex = 0;


                    if (geometry != null)
                    {
                        if (!featureAttributes.ContainsKey("X") && !featureAttributes.ContainsKey("Y"))
                        {
                            featureAttributes.Add("X", coordinate.X.ToString());
                            featureAttributes.Add("Y", coordinate.Y.ToString());
                            featureAttributes.Add("Z", coordinate.Z.ToString());
                        }
                    }
                    allAttributes.Add(featureAttributes);
                }
            }
        }

        stopwatch.Stop();
        MessageBox.Show($"Time taken: {stopwatch.ElapsedMilliseconds / 1000} s");
        return allAttributes;
    }
}
