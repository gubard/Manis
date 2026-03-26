using Manis.Models;
using Nestor.Db.LiteDb.Models;
using Nestor.Db.Models;

[assembly: Ado(typeof(UserEntity), nameof(UserEntity.Id), false)]
[assembly: AdoSourceEntity(typeof(UserEntity), nameof(UserEntity.Id))]
[assembly: EditModel(typeof(UserEntity), nameof(UserEntity.Id))]
[assembly: LiteDb(typeof(UserEntity), nameof(UserEntity.Id), false)]
[assembly: LiteDbSourceEntity(typeof(UserEntity), nameof(UserEntity.Id))]
