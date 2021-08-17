import { AccountService } from 'src/app/_services/account.service';
import { PaginatedResult } from './../_models/pagination';
import { environment } from './../../environments/environment';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Member } from '../_models/member';
import { of } from 'rxjs';
import { map, switchMap, take, tap } from 'rxjs/operators';
import { UserParams } from '../_models/userParams';
import { User } from '../_models/user';
import { Like } from '../_models/like';

@Injectable({
  providedIn: 'root'
})
export class MembersService {
  public baseUrl: string = environment.apiUrl;
  public userParams: UserParams;
  public user: User;  
  members: Member[] = [];
  public memberCache = new Map<string,PaginatedResult<Member[]>>();

  constructor(
    private http: HttpClient,
    private accountService: AccountService
  ) { 
    this.accountService.currentUser$.pipe(take(1)).subscribe(user => {
      this.user = user;
      if(user) {
        this.userParams = new UserParams(this.user);
        //console.log("UP Ctor", this.userParams);  
      }
    }); 
  }

  public getMembers(userParams: UserParams) {
    //console.log(Object.values(userParams).join("-"));
    const key = Object.values(userParams).join("-");
    const response = this.memberCache.get(key);
    if(response) {
      return of(response);
    }

    //add page#, page size to QS
    let params = this.getPaginationHeaders(userParams.pageNumber, userParams.pageSize);
    params = params.append("minAge", userParams.minAge.toString());
    params = params.append("maxAge", userParams.maxAge.toString());
    params = params.append("gender", userParams.gender);
    params = params.append("orderBy", userParams.orderBy);

    return this.getPaginatedResults<Member[]>(`${this.baseUrl}users`, params).pipe(
      tap( result => {
        this.memberCache.set(key,result);
      })
    );
  }

  public getMember(username: string) {
    //caching
    const member = [...this.memberCache.values()] //returns array of PaginatedResult objects: PaginatedResult[]
        .reduce((arr: Member[], elem: PaginatedResult<Member[]>)=> arr.concat(elem.result),[] )
        .find( (member:Member) => member.username === username);
    if(member) {
      return of(member);
    }   
      
    return this.http.get<Member>(`${this.baseUrl}users/${username}`);//, httpOptions);
  }

  public updateMember(member: Member) {
    return this.http.put(`${this.baseUrl}users`, member).pipe(
      tap(() => {
        const index = this.members.findIndex(item=>item.username===member.username);
        if(index>=0)
          this.members[index] = member;
      })
    );
  }

  public setMainPhoto(photoId: number) {
    return this.http.put(`${this.baseUrl}users/set-main-photo/${photoId}`, {});
        // .pipe(
        // switchMap(() => {}),
        // tap(() => {
        //   this.
        // })
  }  

  public deletePhoto(photoId: number) {
    return this.http.delete(`${this.baseUrl}users/delete-photo/${photoId}`, {});
  }

  public addLike(username: string) {
    return this.http.post(`${this.baseUrl}likes/${username}`, {});
  }

  public getLikes(predicate: string, pageNumber: number, pageSize: number) {
    // let params = new HttpParams();
    // params = params.append("predicate", predicate);
    // return this.http.get(`${this.baseUrl}likes`, {params: params});
    //return this.http.get<Like[]>(`${this.baseUrl}likes?predicate=${predicate}`);
    let params = this.getPaginationHeaders(pageNumber, pageSize);
    params = params.append("predicate", predicate);    

    return this.getPaginatedResults<Like[]>(`${this.baseUrl}likes`,params);    
  }

  public getUserParams(): UserParams {
    return this.userParams;
  }

  public setUserParams(params: UserParams): void {
    this.userParams = params;
    //console.log("UP Set", this.userParams);
  }

  public resetUserParams(): UserParams {
    this.userParams = new UserParams(this.user);
    return this.userParams;
  }  

  public resetUserParamsLogin(user: User): void {
    this.user = user;
    this.userParams = new UserParams(user);
    //console.log("UP Login", this.userParams);
  }    

  private getPaginatedResults<T>(url: string, params: HttpParams) {
    const paginatedResult: PaginatedResult<T> = new PaginatedResult<T>();

    //pass SQ to the GET request, set observer:"response" to received the entire response object, not only the body
    return this.http.get<T>(url, { observe: "response", params }).pipe(
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

  private getPaginationHeaders(pageNumber: number, pageSize: number): HttpParams {
    let params = new HttpParams();
    
    params = params.append("PageSize", pageSize.toString());
    params = params.append("PageNumber", pageNumber.toString());    

    return params;
  }

  // private getHttpOptions() {
  //   const user: User = JSON.parse(localStorage.getItem("user"));
  //   const httpOptions = {
  //     headers: new HttpHeaders({
  //       Authorization: `Bearer ${user?.token}`
  //     })
  //   };
  //   return httpOptions;
  // }
}
