using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OverAudible.DbContexts;
using OverAudible.Models;
using OverAudible.Models.DTOs;

namespace OverAudible.Services
{
    public class DataService : IDataService<Item>
    {
        private readonly MainDbContextFactory _factory;

        public DataService(MainDbContextFactory factory)
        {
            _factory = factory;
        }

        public async Task<Item> Create(Item entity)
        {
            using (MainDbContext context = _factory.CreateDbContext())
            {
                var dto = new ItemDTO() 
                { 
                    Asin = entity.Asin, 
                    Item = JsonConvert.SerializeObject(entity) 
                };

                await context.OfflineLibrary.AddAsync(dto);
                await context.SaveChangesAsync();

                return entity;
            }
        }

        public async Task<bool> Delete(string id)
        {
            using (MainDbContext context = _factory.CreateDbContext())
            {
                ItemDTO? dto = await context.OfflineLibrary.FirstOrDefaultAsync(x =>
                    x.Asin == id);
                if (dto == null)
                    return false;

                context.OfflineLibrary.Remove(dto);
                await context.SaveChangesAsync();
                return true;
            }
        }

        public async Task<bool> DeleteAll()
        {
            using (MainDbContext context = _factory.CreateDbContext())
            {
                context.OfflineLibrary.RemoveRange(context.OfflineLibrary.ToList());
                await context.SaveChangesAsync();
                return true;
            }
        }

        public async Task<List<Item>> GetAll()
        {
            using (MainDbContext context = _factory.CreateDbContext())
            {
                var items = await context.OfflineLibrary.ToListAsync();

                List<Item> result = new List<Item>();

                foreach (var item in items)
                {
                    result.Add(JsonConvert.DeserializeObject<Item>(item.Item));
                }

                return result;
            }
        }

        public async Task<Item> GetById(string id)
        {
            using (MainDbContext context = _factory.CreateDbContext())
            {
                ItemDTO dto = await context.OfflineLibrary.FirstOrDefaultAsync(x => x.Asin == id);
                return JsonConvert.DeserializeObject<Item>(dto.Item);
            }
        }

        public async Task<Item> Update(string id, Item entity)
        {
            using (MainDbContext context = _factory.CreateDbContext())
            {
                entity.Asin = id;

                var dto = new ItemDTO()
                {
                    Asin = entity.Asin,
                    Item = JsonConvert.SerializeObject(entity)
                };

                context.OfflineLibrary.Update(dto);
                await context.SaveChangesAsync();

                return entity;
            }
        }
    }
}
