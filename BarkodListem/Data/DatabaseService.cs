using BarkodListem.Models;
using SQLite;
using System.Collections.ObjectModel;
namespace BarkodListem.Data
{
    public class DatabaseService
    {
        private static SQLiteAsyncConnection _database;
        public DatabaseService(string dbPath)
        {
            _database = new SQLiteAsyncConnection(dbPath);
            _database.CreateTableAsync<BarkodModel>().Wait();
            _database.CreateTableAsync<AyarlarModel>().Wait();
            _database.CreateTableAsync<SevkiyatFisModel>().Wait();
            _database.CreateTableAsync<SSHAnaModel>().Wait();
            _database.CreateTableAsync<SSHDetayModel>().Wait();
            _database.CreateTableAsync<ResimModel>().Wait();
        }
        public async Task AyarKaydet(AyarlarModel ayar)
        {
            await _database.InsertOrReplaceAsync(ayar);
        }
        public async Task<AyarlarModel> AyarlarGetir()
        {
            return await _database.Table<AyarlarModel>().FirstOrDefaultAsync();
        }
        public async Task BarkodEkle(BarkodModel barkod)
        {
            await _database.InsertAsync(barkod);
        }
        public async Task UpdateBarkodListeAdi(string eskiListeAdi, string yeniListeAdi)
        {
            var barkodlar = await _database.Table<BarkodModel>().Where(b => b.ListeAdi == eskiListeAdi).ToListAsync();
            foreach (var b in barkodlar)
            {
                b.ListeAdi = yeniListeAdi;
                await _database.UpdateAsync(b);
            }
        }
        public Task<List<BarkodModel>> BarkodlariGetir(string listeAdi)
        {
            return _database.Table<BarkodModel>()
                .Where(b => b.ListeAdi == listeAdi)
                .OrderByDescending(b => b.Id)
                .ToListAsync();
        }
        public async Task BarkodSil(BarkodModel barkod)
        {
            if (barkod != null)
            {
                await _database.DeleteAsync(barkod);
            }
        }
        public static async Task<bool> DeleteAllBarkodsAsync()
        {
            try
            {
                await _database.DropTableAsync<BarkodModel>();
                await _database.CreateTableAsync<BarkodModel>();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Veritabanı temizleme hatası: {ex.Message}");
                return false;
            }
        }
        public async Task<List<ListeModel>> ListeGetir()
        {
            var listeler = await _database.Table<BarkodModel>().ToListAsync();
            return listeler.GroupBy(x => x.ListeAdi)
                           .Select(g => new ListeModel
                           {
                               ListeAdi = g.Key,
                               Barkodlar = new ObservableCollection<BarkodModel>(g.ToList())
                           }).ToList();
        }
        public async Task ListeSil(ListeModel liste)
        {
            await _database.Table<BarkodModel>().DeleteAsync(b => b.ListeAdi == liste.ListeAdi);
        }
        public static async Task<SQLiteAsyncConnection> GetConnectionAsync()
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "barkodlistem.db");
            var conn = new SQLiteAsyncConnection(dbPath);
            await conn.CreateTableAsync<SevkiyatFisModel>();
            await conn.CreateTableAsync<SSHAnaModel>();
            await conn.CreateTableAsync<SSHDetayModel>();
            await conn.CreateTableAsync<ResimModel>();
            return conn;
        }
    }
}
