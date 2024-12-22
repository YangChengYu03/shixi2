using System;
using System.Windows.Forms;
using ESRI.ArcGIS.esriSystem;
namespace 实习二
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            ESRI.ArcGIS.RuntimeManager.Bind(ESRI.ArcGIS.ProductCode.Desktop);
            IAoInitialize aoInitialize = new AoInitializeClass();
            esriLicenseStatus licenseStatus;
            licenseStatus = aoInitialize.Initialize(esriLicenseProductCode.esriLicenseProductCodeAdvanced);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
