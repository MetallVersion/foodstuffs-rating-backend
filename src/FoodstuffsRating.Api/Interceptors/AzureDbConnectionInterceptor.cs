using System.Data.Common;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace FoodstuffsRating.Api.Interceptors
{
    public class AzureDbConnectionInterceptor : DbConnectionInterceptor
    {
        private const string AzureDatabaseResourceIdentifier = "https://database.windows.net";
        private readonly AzureServiceTokenProvider _azureServiceTokenProvider;

        public AzureDbConnectionInterceptor()
        {
            this._azureServiceTokenProvider = new AzureServiceTokenProvider();
        }

        public override async ValueTask<InterceptionResult> ConnectionOpeningAsync(
            DbConnection connection,
            ConnectionEventData eventData,
            InterceptionResult result,
            CancellationToken cancellationToken = default)
        {
            if (connection is SqlConnection sqlConnection)
            {
                sqlConnection.AccessToken = await this.GetAccessTokenAsync();
            }

            return result;
        }

        public override InterceptionResult ConnectionOpening(
            DbConnection connection,
            ConnectionEventData eventData,
            InterceptionResult result)
        {
            if (connection is SqlConnection sqlConnection)
            {
                sqlConnection.AccessToken = this.GetAccessTokenAsync().GetAwaiter().GetResult();
            }

            return result;
        }

        private Task<string> GetAccessTokenAsync()
        {
            return this._azureServiceTokenProvider.GetAccessTokenAsync(AzureDatabaseResourceIdentifier);
        }
    }
}
