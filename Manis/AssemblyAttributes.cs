using Manis.Models;
using Nestor.Db.Models;

[assembly: SqliteAdo(typeof(UserEntity), nameof(UserEntity.Id), false)]
[assembly: SourceEntity(typeof(UserEntity), nameof(UserEntity.Id))]
