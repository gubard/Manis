using Manis.Models;
using Nestor.Db.Models;

[assembly: Ado(typeof(UserEntity), nameof(UserEntity.Id), false)]
[assembly: SourceEntity(typeof(UserEntity), nameof(UserEntity.Id))]
