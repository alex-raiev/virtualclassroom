using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZOOM_SDK_DOTNET_WRAP;

namespace VirtualClassroom.NET
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //init sdk
            var param = new InitParam
            {
                web_domain = "https://zoom.us",
                enable_log = true,
                language_id = SDK_LANGUAGE_ID.LANGUAGE_English,
                brand_name = "VirtualClassroom",
                sdk_dll_path = AppDomain.CurrentDomain.BaseDirectory
            };

            var err = ZOOM_SDK_DOTNET_WRAP.CZoomSDKeDotNetWrap.Instance.Initialize(param);
            if (ZOOM_SDK_DOTNET_WRAP.SDKError.SDKERR_SUCCESS != err)
            {
                MessageBox.Show($"Init SDK failed. {err}", "VirtualClassroom", MessageBoxButtons.OK);
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
