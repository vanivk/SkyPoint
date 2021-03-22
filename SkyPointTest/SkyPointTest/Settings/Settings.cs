using System;
using System.Collections.Generic;
using System.Text;

namespace SkyPointTest.Settings
{
    public class Settings
    {
        public const string MONGO_CONNECTION_STRING = "http://localhost:27017";
        public const string DATABASE_NAME = "UserDB";
        public const string COLLECTION_NAME = "Users";
        public const string TOKEN_KEY = "Some super secret key";
    }
}
