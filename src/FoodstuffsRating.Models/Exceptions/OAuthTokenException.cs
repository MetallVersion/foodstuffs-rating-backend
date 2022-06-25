using System;

namespace FoodstuffsRating.Models.Exceptions
{
    /// <summary>
    /// Represents OAuth 2.0 exception.
    /// </summary>
    public class OAuthTokenException : Exception
    {
        private readonly string _oAuthTokenError;

        public OAuthTokenException(string oAuthTokenError)
        {
            this._oAuthTokenError = oAuthTokenError;
        }

        public override string Message => 
            $"OAuth token exception occurred, error: {this._oAuthTokenError}";
    }
}
