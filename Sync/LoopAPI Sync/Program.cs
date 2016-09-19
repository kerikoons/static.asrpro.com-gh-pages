using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LoopAPI_Sync
{
    class Program
    {
        const String LoopUrl = "http://autoloop.us/dms/dynamicimage/TemplateEmailHeader/{0}";

        // string bookletBannerImagePath = string.Format("http://static.asrpro.com/Images/Stores/{0}/Booklet/BookletBanner.jpg", this.StoreID);

        static String OutputPath = ConfigurationManager.AppSettings["outputPath"];

        static String ConnectionString = ConfigurationManager.AppSettings["connectionString"];

        static void Main(string[] args)
        {
            Console.WriteLine("Starting Booklet Header sync!");

            var client = new WebClient();

            // Get list of Stores / LoopCompanyIds
            foreach (var store in GetStores())
            {
                string downloadTo = Path.Combine(String.Format(OutputPath, store.StoreID), "BookletBanner.jpg");

                if(!Directory.Exists(Path.Combine(String.Format(OutputPath, store.StoreID))))
                {
                    Directory.CreateDirectory(Path.Combine(String.Format(OutputPath, store.StoreID)));
                }
                else
                {
                    //Do Nothing
                }

                if(File.Exists(downloadTo))
                {
                    Console.WriteLine("File Exists for Store {0}", store.StoreID);
                }
                else
                {
                    client.DownloadFile(String.Format(LoopUrl, store.LoopCompanyID), downloadTo);

                    Console.WriteLine("Synced file for Store {0}", store.StoreID);
                }
            }

            Console.WriteLine("Completed Booklet Header sync!");

            Console.ReadKey();
        }

        static IEnumerable<Store> GetStores()
        {
            Console.WriteLine("Getting list of Loop stores in ASR");

            var stores = new List<Store>();

            using (var connection = new SqlConnection(ConnectionString))
            using(var cmd = connection.CreateCommand())
            {
                connection.Open();

                cmd.CommandText = "select sil.StoreID, sil.ID from dbo.ASR2_StoreIntegrationLoop sil join dbo.Stores s on sil.StoreID = s.StoreID where sil.EnableIntegration = 1 and s.Active = 1";
                cmd.CommandType = System.Data.CommandType.Text;

                using(var reader = cmd.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        stores.Add(new Store
                            {
                                StoreID = reader.GetInt32(0),
                                LoopCompanyID = reader.GetInt32(1)
                            });
                    }
                }
            }

            return stores;
        }

        class Store
        {
            public int StoreID { get; set; }

            public int LoopCompanyID { get; set; }
        }
    }
}
