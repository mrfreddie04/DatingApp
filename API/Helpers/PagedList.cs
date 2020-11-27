using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace API.Helpers
{
    public class PagedList<T> : List<T>
    {
        public PagedList(IEnumerable<T> items,int count, int pageNumber, int pageSize)            
        {
            CurrentPage = pageNumber;
            PageSize = pageSize;
            TotalCount = count; //items.Count();
            // var pages = TotalCount/PageSize;
            // pages += pages*PageSize < TotalCount?1:0;
            TotalPages = (int)Math.Ceiling(TotalCount/(double)PageSize);
            
            AddRange(items); //add items to the list, should be the same as :base(items)
        }

        public int CurrentPage { get; set; }   
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; } //total count for the query

        public static async Task<PagedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
        {
            var count = await source.CountAsync<T>();
            var items = await source.Skip(pageSize*(pageNumber-1)).Take(pageSize).ToListAsync<T>();
     
            return new PagedList<T>(items, count, pageNumber, pageSize);
        }
    }
}