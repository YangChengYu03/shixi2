using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geometry;

namespace 实习二
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void OpenGDB(string path) {
            IWorkspace myWorkspace;
            IWorkspaceFactory myWorkspaceFactory = new FileGDBWorkspaceFactory();
            myWorkspace = myWorkspaceFactory.OpenFromFile(path, 0);
            IEnumDataset myEnumDataset = myWorkspace.Datasets[esriDatasetType.esriDTAny];
            IDataset myDataset;
            while ((myDataset=myEnumDataset.Next())!=null)
            {
                if (myDataset is IFeatureClass)
                {
                    IGeoFeatureLayer myGeoFeatureLayer = new FeatureLayerClass();
                    myGeoFeatureLayer.FeatureClass=myDataset as IFeatureClass;
                    axMapControl1.AddLayer(myGeoFeatureLayer);
                }

                if (myDataset is IRasterDataset)
                {
                    IRasterLayer myRasterLayer = new RasterLayerClass();
                    myRasterLayer.CreateFromDataset(myDataset as IRasterDataset);
                    myRasterLayer.Name = myDataset.Name;
                    axMapControl1.AddLayer(myRasterLayer);
                    axLicenseControl1.Refresh();
                }
            }
            Marshal.ReleaseComObject(myWorkspace);
            Marshal.ReleaseComObject(myEnumDataset);
        }

        string path;
        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog FBDL = new FolderBrowserDialog();
            FBDL.ShowDialog();
            path = FBDL.SelectedPath;

            OpenGDB(path);
        }

        private List<double> queryBandValue(IMapControlEvents2_OnMouseDownEvent e) {
            List<string> Name = new List<string> { "NH4_IDW_05_ExtractF", "P_IDW_05_ExtractF", "K_IDW_05_ExtractF" };
            List<double> PNK = new List<double> { };
            for (int i = 0; i < axMapControl1.LayerCount; i++)
            {
                ILayer myLayer = axMapControl1.get_Layer(i);
                if (Name.Contains(myLayer.Name))
                {
                    IRasterLayer queryLayer = new RasterLayerClass();
                    queryLayer = myLayer as IRasterLayer;
                    IRaster2 raster = queryLayer.Raster as IRaster2;
                    int col;
                    int row;
                    raster.MapToPixel(e.mapX, e.mapY, out col, out row);
                    object result = raster.GetPixelValue(0, col, row);
                    double BandNum = Convert.ToDouble(result);
                    PNK.Add(BandNum);
                }
            }
            return PNK;
        }

        private double getFactor(double x1,double x2,double x3,double x4,double x5,double num) {
            if (num < x1)
            {
                return 1.24;
            }
            else if (x1 <= num && (num < x2))
            {
                return 1.16;
            }
            else if (x2 <= num && (num < x3))
            {
                return 1.08;
            }
            else if (x3 <= num && (num < x4))
            {
                return 1.0;
            }
            else if (x4 <= num && (num <= x5))
            {
                return 0.92;
            }
            else
            {
                return 0.84;
            }
        }

        private double getNFactor(double num)
        {
            return getFactor(30, 55, 85, 120, 150, num);
        }

        private double getPFactor(double num)
        {
            return getFactor(2, 4.5, 10, 20, 40, num);
        }

        private double getKFactor(double num)
        {
            return getFactor(30, 50, 90, 140, 200, num);
        }

        private void updateDataView1(List<double> PNK)
        {
            DataTable myDataTable = new DataTable();
            myDataTable.Columns.Add("项目", typeof(string));
            myDataTable.Columns.Add("指标", typeof(double));
            DataRow row1 = myDataTable.NewRow();
            row1["项目"] = "P";
            row1["指标"] = PNK[0];
            myDataTable.Rows.Add(row1);
            DataRow row2 = myDataTable.NewRow();
            row2["项目"] = "N";
            row2["指标"] = PNK[1];
            myDataTable.Rows.Add(row2);
            DataRow row3 = myDataTable.NewRow();
            row3["项目"] = "K";
            row3["指标"] = PNK[2];
            myDataTable.Rows.Add(row3);
            dataGridView1.DataSource = myDataTable;
        }

        private List<double> getTargetPNKValue(double Target_value) {
            List<double> listTargetValue = new List<double> { };
            if (1000 < Target_value && (Target_value <= 2000))
            {
                return new List<double> { Target_value * 0.81, Target_value * 0.9, Target_value * 0.81 };
            }
            else 
            {
                return new List<double> { Target_value * 0.85, Target_value * 0.95, Target_value * 0.85 };
            }        
        }

        private void updateDataView2(List<double> PNK,double Target_value)
        {
            DataTable myDataTable = new DataTable();
            myDataTable.Columns.Add("施肥期", typeof(string));
            myDataTable.Columns.Add("尿素", typeof(string));
            myDataTable.Columns.Add("钙镁磷", typeof(string));
            myDataTable.Columns.Add("氯化钾", typeof(string));
            double Target_P_value = getTargetPNKValue(Target_value)[0] / 100 * getPFactor(PNK[0]);
            double Target_N_value = getTargetPNKValue(Target_value)[1] / 100 * getPFactor(PNK[1]);
            double Target_K_value = getTargetPNKValue(Target_value)[2] / 100 * getPFactor(PNK[2]);
            DataRow row1 = myDataTable.NewRow();
            row1["施肥期"] = "春期";
            row1["尿素"] = Target_N_value * 0.3;
            row1["钙镁磷"] = Target_P_value * 0.3;
            row1["氯化钾"] = Target_K_value * 0.3;
            myDataTable.Rows.Add(row1);
            DataRow row2 = myDataTable.NewRow();
            row2["施肥期"] = "状果期";
            row2["尿素"] = Target_N_value * 0.4;
            row2["钙镁磷"] = Target_P_value * 0.4;
            row2["氯化钾"] = Target_K_value * 0.4;
            myDataTable.Rows.Add(row2);
            DataRow row3 = myDataTable.NewRow();
            row3["施肥期"] = "冬期";
            row3["尿素"] = Target_N_value * 0.3;
            row3["钙镁磷"] = Target_P_value * 0.3;
            row3["氯化钾"] = Target_K_value * 0.3;
            myDataTable.Rows.Add(row3);
            dataGridView2.DataSource = myDataTable;
        }

        List<double> PNK;
        private void axMapControl1_OnMouseDown(object sender, IMapControlEvents2_OnMouseDownEvent e)
        {
            PNK = queryBandValue(e);
            updateDataView1(PNK);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            double Target_value;
            if (!double.TryParse(textBox1.Text.ToString(), out Target_value))
            {
                MessageBox.Show("请输入有效数值！");
                return;
            }
            if (Target_value < 1000 || Target_value > 4000)
            {
                MessageBox.Show("请输入范围内的值！");
                return;
            }
            updateDataView2(PNK,Target_value);
        }
    }
}
