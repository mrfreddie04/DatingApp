using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace API.Helpers
{
    public class PagedList<T>: List<T>
    {
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }

        public PagedList(IEnumerable<T> items, int count, int pageNumber, int pageSize)
        {
            CurrentPage = pageNumber;            
            PageSize = pageSize;
            TotalCount = count;            
            TotalPages = (int) Math.Ceiling((double)TotalCount / (double)PageSize);
            
            AddRange(items); //add to the end of the PageList
        }

        public static async Task<PagedList<T>> CreateAsync(IQueryable<T> source, int pageNumber,int pageSize) {
            var count = await source.CountAsync();
            var items = await source.Skip(pageSize * (pageNumber-1)).Take(pageSize).ToListAsync<T>();
            
            return new PagedList<T>(items, count, pageNumber, pageSize);
        }
    }
}