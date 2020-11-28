import { HttpClient, HttpParams, HttpResponse } from '@angular/common/http';
import { map } from 'rxjs/operators';
import { PaginatedResult } from '../_models/pagination';

export function getPaginationHeaders(pageNumber: number, pageSize: number) : HttpParams
{
  let params = new HttpParams(); //use to serialize parameters, so that they could be added to the query string
  params = params.append("pageNumber",pageNumber.toString());
  params = params.append("pageSize",pageSize.toString());
    
  return params;
}

export function getPaginatedResult<T>(url: string, params: HttpParams, http: HttpClient)
{
  //get the members  
  // when we use http get normally - the call will gve us response body 
  // when we are observing the response, then we get the full response back  
  const paginatedResult: PaginatedResult<T> = new PaginatedResult<T>();     
  return http.get<T>(url,{observe:"response",params:params})
    .pipe(
      map(response=> {
        paginatedResult.result = response.body; //we explicilty have to access the .body
        if(response.headers.get("Pagination") !== null) {
          paginatedResult.pagination = JSON.parse(response.headers.get("Pagination"));
        }
        return paginatedResult;
      })
    );    
}  