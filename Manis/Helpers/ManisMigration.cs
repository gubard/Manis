using System.Collections.Frozen;

namespace Manis.Helpers;

public static class ManisMigration
{
    public static readonly FrozenDictionary<int, string> Migrations;

    static ManisMigration()
    {
        Migrations = new Dictionary<int, string>
        {
            {
                3,
                @"
CREATE TABLE IF NOT EXISTS Users (
    Id TEXT PRIMARY KEY NOT NULL,
    Login TEXT NOT NULL CHECK(length(Login) <= 255),
    Email TEXT NOT NULL CHECK(length(Email) <= 255),
    PasswordHash TEXT NOT NULL CHECK(length(PasswordHash) <= 512),
    PasswordHashMethod TEXT NOT NULL,
    PasswordSalt TEXT NOT NULL CHECK(length(PasswordSalt) <= 128),
    IsActivated INTEGER NOT NULL CHECK (IsActivated IN (0, 1)),
    ActivationCode TEXT NOT NULL CHECK(length(Login) <= 255)
);
"
            },
        }.ToFrozenDictionary();
    }
}
