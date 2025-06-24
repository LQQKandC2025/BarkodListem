using $safeprojectname$.Models;
using SQLite;

namespace $safeprojectname$.Data
{
    public class DatabaseService
    {
        private readonly SQLiteAsyncConnection _database;

        public DatabaseService(string dbPath)
        {
            _database = new SQLiteAsyncConnection(dbPath);
            _database.CreateTableAsync<BarkodModel>().Wait();
        }

        public Task<int> BarkodEkle(BarkodModel barkod)
        {
            return _database.InsertAsync(barkod);
        }

        public Task<List<BarkodModel>> BarkodlariGetir()
        {
            return _database.Table<BarkodModel>().ToListAsync();
        }

        public Task<int> BarkodSil(BarkodModel barkod)
        {
            return _database.DeleteAsync(barkod);
        }
    }
}
