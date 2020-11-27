using System.Text.Json;
using API.Helpers;
using Microsoft.AspNetCore.Http;



namespace API.Extensions
{
    public static class HttpExtensions
    {
        public static void AddPaginationHeader(this HttpResponse response, int currentPage, 
            int itemsPerPage,  int totalItems, int totalPages)
        {
            var paginationHeader = new PaginationHeader(currentPage, itemsPerPage, totalItems, totalPages);

            var options = new JsonSerializerOptions(){
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            var json = JsonSerializer.Serialize<PaginationHeader>(paginationHeader,options);

            //give your header a sensible name (no special requirements)
            //and serialize Pagination class
            response.Headers.Add("Pagination",json);

            //Add CORS header to make "Pagination" header available
            response.Headers.Add("Access-Control-Expose-Headers", "Pagination");
        }
    }
}