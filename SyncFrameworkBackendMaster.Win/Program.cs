using System;
using System.Configuration;
using System.Net.Http;
using System.Windows.Forms;
using BIT.Xpo.Providers.OfflineDataSync;
using BIT.Xpo.Providers.OfflineDataSync.NetworkExtensions;
using BIT.Xpo.Providers.OfflineDataSync.UtilitiesExtensions;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Win;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using DevExpress.Xpo.DB;
using DevExpress.XtraEditors;
using Orm.Model;

namespace SyncFrameworkBackendMaster.Win {
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //HACK register SyncDataStore
            SyncDataStore.Register();
            
            //InitSyncFrameworkPull();
            DevExpress.ExpressApp.FrameworkSettings.DefaultSettingsCompatibilityMode = DevExpress.ExpressApp.FrameworkSettingsCompatibilityMode.Latest;
#if EASYTEST
            DevExpress.ExpressApp.Win.EasyTest.EasyTestRemotingRegistration.Register();
#endif
            WindowsFormsSettings.LoadApplicationSettings();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            DevExpress.Utils.ToolTipController.DefaultController.ToolTipType = DevExpress.Utils.ToolTipType.SuperTip;
            EditModelPermission.AlwaysGranted = System.Diagnostics.Debugger.IsAttached;
            if (Tracing.GetFileLocationFromSettings() == DevExpress.Persistent.Base.FileLocation.CurrentUserApplicationDataFolder)
            {
                Tracing.LocalUserAppDataPath = Application.LocalUserAppDataPath;
            }
            Tracing.Initialize();
            SyncFrameworkBackendMasterWindowsFormsApplication winApplication = new SyncFrameworkBackendMasterWindowsFormsApplication();
            if (ConfigurationManager.ConnectionStrings["ConnectionString"] != null)
            {
                winApplication.ConnectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            }
#if EASYTEST
            if(ConfigurationManager.ConnectionStrings["EasyTestConnectionString"] != null) {
                winApplication.ConnectionString = ConfigurationManager.ConnectionStrings["EasyTestConnectionString"].ConnectionString;
            }
#endif
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached && winApplication.CheckCompatibilityType == CheckCompatibilityType.DatabaseSchema)
            {
                winApplication.DatabaseUpdateMode = DatabaseUpdateMode.UpdateDatabaseAlways;
            }
#endif
            try
            {
                winApplication.Setup();
                winApplication.Start();
            }
            catch (Exception e)
            {
                winApplication.StopSplash();
                winApplication.HandleException(e);
            }
        }
        public static void InitSyncFrameworkPull()
        {
            var httpClient = new HttpClient();
            var SerializationService = new BIT.Data.Services.CompressXmlObjectSerializationService();
            var StringSerializactionService = new BIT.Data.Services.StringSerializationHelper();
            Type[] Types = new Type[] { typeof(Department), typeof(Employee), typeof(PermissionPolicyUser), typeof(PermissionPolicyRole), typeof(Person) };
            var Config = new SyncDataStoreServerConfiguration(SerializationService, StringSerializactionService, "https://7198775f8547.ngrok.io", httpClient);
            var CnxA = @"XpoProvider=SyncDataStore;DataConnectionString='Integrated Security=SSPI;Pooling=false;Data Source=(localdb)\mssqllocaldb;Initial Catalog=SyncMaster';DeltaConnectionString='Integrated Security=SSPI;Pooling=false;Data Source=(localdb)\mssqllocaldb;Initial Catalog=SyncMasterDeltas';Identity=MASTER;EnableDeltaTracking='false';ExcludedEntities=''";

            var Client = SyncDataStore.CreateProviderFromString(CnxA, AutoCreateOption.DatabaseAndSchema, out _) as ISyncDataStore;

            //if you scenario is peer to peer you should create type records for each datastore
            //else if your scenario is master slave, it should be true for the master and false for the slaves
            Client.UpdateTargetSchema(Types, true,false);
            Client.PullDeltas(Config);

        }
      }
    }
