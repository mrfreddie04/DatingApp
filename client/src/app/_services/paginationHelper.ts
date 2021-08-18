import { HttpClient, HttpParams } from '@angular/common/http';
import { map } from 'rxjs/operators';
import { PaginatedResult } from '../_models/pagination';

export class PaginationHelper {
  public static getPaginatedResults<T>(url: string, params: HttpParams, http: HttpClient) {
    const paginatedResult: PaginatedResult<T> = new PaginatedResult<T>();

    //pass SQ to the GET request, set observer:"response" to received the entire response object, not only the body
    return http.get<T>(url, { observe: "response", params }).pipe(
      map((response) => {
        //build PaginatedResult
        paginatedResult.result = response.body;        
        if(response.headers.get("Pagination") != null) {
          paginatedResult.pagination = JSON.parse(response.headers.get("Pagination"));
        }
        return paginatedResult;  
      })
    )
  }

  public static getPaginationHeaders(pageNumber: number, pageSize: number): HttpParams {
    let params = new HttpParams();
    
    params = params.append("PageSize", pageSize.toString());
    params = params.append("PageNumber", pageNumber.toString());    

    return params;
  }
}