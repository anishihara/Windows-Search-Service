using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using WindowsSearchExample.Models;
using System.ServiceModel.Web;
using SearchAPI;
using System.Data.OleDb;
using System.Data;
using System.Net.Http;

namespace WindowsSearchExample.Infrastructure
{
    [ServiceContract]
    public class SearchFileResources
    {
        [WebInvoke(UriTemplate = "", Method="POST")]
        public List<SearchFile> Post(HttpRequestMessage request)
        {
            var content = HttpUtility.ParseQueryString(request.Content.ReadAsString());
            string keyword = String.Format("{0}", content.Get(0));

            CSearchManager cManager;
            ISearchQueryHelper cHelper;
            OleDbConnection cConnection;
            


            cManager = new CSearchManagerClass();
            List<SearchFile> searchResults = new List<SearchFile>();

            // Atualmente, o Windows Search suporta apenas um catálogo e esse já é definido como SYSTEMINDEX
            cHelper = cManager.GetCatalog("SYSTEMINDEX").GetQueryHelper();

            cHelper.QuerySelectColumns = "System.ItemPathDisplay,System.ItemNameDisplay,System.ItemType";

            try
            {
                using (cConnection = new OleDbConnection(
                            cHelper.ConnectionString))
                {
                    // Define o número máximo de resultados
                    cHelper.QueryMaxResults = 100;

                    // Procura apenas os arquivos
                    cHelper.QueryWhereRestrictions = "AND scope='file:C:/ekdvault'";

                    cConnection.Open();
                    using (OleDbCommand cmd = new OleDbCommand(
                            cHelper.GenerateSQLFromUserQuery(keyword),
                            cConnection))
                    {
                        if (cConnection.State == ConnectionState.Open)
                        {
                            using (OleDbDataReader reader = cmd.ExecuteReader())
                            {

                                int i = 0;
                                while (!reader.IsClosed && reader.Read())
                                {
                                    SearchFile searchResult = new SearchFile
                                    {
                                        FilePath = reader[0].ToString(),
                                        FileName = reader[1].ToString(),
                                        ItemType = reader[2].ToString(),
                                    };
                                    //if (i < 3)
                                    if(searchResult.ItemType != "Directory")
                                        searchResults.Add(searchResult);
                                    i++;

                                }
                            }

                        }
                        cConnection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
            return searchResults;


        }
    }
}
