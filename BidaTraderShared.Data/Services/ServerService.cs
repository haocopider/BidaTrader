using BidaTraderShared.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace BidaTraderShared.Data.Services
{
    public class ServerService<T> : IService<T> where T : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public ServerService(AppDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public virtual async Task<List<T>?> GetItemsAsync(string? queryString = null)
        {
            return await _dbSet.ToListAsync();
        }

        public virtual async Task<T?> GetItemByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<bool> CreateItemAsync(T item)
        {
            await _dbSet.AddAsync(item);
            return await _context.SaveChangesAsync() > 0;
        }

        public virtual async Task<bool> UpdateItemAsync(T item)
        {
            // Logic Generic mặc định: Update toàn bộ
            _dbSet.Attach(item);
            _context.Entry(item).State = EntityState.Modified;
            return await _context.SaveChangesAsync() > 0;
        }

        public virtual async Task<bool> DeleteItemAsync(int id)
        {
            var item = await _dbSet.FindAsync(id);
            if (item == null) return false;
            _dbSet.Remove(item);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}

