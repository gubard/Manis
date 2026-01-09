using Manis.Models;
using Nestor.Db.Models;

[assembly: SqliteAdo(typeof(UserEntity), nameof(UserEntity.Id))]
[assembly: SourceEntity(typeof(UserEntity), nameof(UserEntity.Id))]
