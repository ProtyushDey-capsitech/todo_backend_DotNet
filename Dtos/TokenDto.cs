using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Projects.Dtos
{
    public class TokenDto
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }

         public TokenDto(string accessToken, string refreshToken)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
        }
    }
}
