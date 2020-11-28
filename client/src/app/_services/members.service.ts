import { HttpClient, HttpParams, HttpResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { of } from 'rxjs';
import { map, take } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { Member } from '../_models/member';
import { PaginatedResult } from '../_models/pagination';
import { User } from '../_models/user';
import { UserParams } from '../_models/userParams';
import { AccountService } from './account.service';
import { getPaginatedResult, getPaginationHeaders } from './paginationHelper';

//set up Authorization header to pass JWT token - saved in local storage (of the browser) when we log in
// const httpOptions = {
//   headers: new HttpHeaders({
//     Authorization: "Bearer " + JSON.parse(localStorage.getItem("user"))?.token
//   })
// }

@Injectable({
  providedIn: 'root'
})
export class MembersService {
  baseUrl = environment.apiUrl;
  members: Member[] = [];
  memberCache = new Map();
  userParams: UserParams;
  user: User;

  constructor(private http: HttpClient,private accountService: AccountService) { 
    this.accountService.currentUser$.pipe(take(1)).subscribe(user=>{
      this.user = user;      
      this.userParams = new UserParams(this.user);       
      //console.log("Member Service Ctor",this.user.username,this.userParams);
    });
  }

  // setUser(){
  //   this.accountService.currentUser$.pipe(take(1)).subscribe(user=>{
  //     this.user = user;      
  //     this.userParams = new UserParams(this.user);       
  //     console.log("Member Service Ctor",this.user.username,this.userParams);
  //   });
  // }

  getMembers() {
    //console.log(Object.values(userParams).join("-"));
    //use cache - turn the caching off to implement pagination
    //if(this.members.length>0) return of<Member[]>(this.members);
    //console.log("Get Users",this.user.username,this.userParams);

    var key = Object.values(this.userParams).join("-");
    var response = this.memberCache.get(key);
    if(response) {
        console.log("Response",response);
        return of(response);
    }

    let params = getPaginationHeaders(this.userParams.pageNumber,this.userParams.pageSize);
    params = params.append("minAge",this.userParams.minAge.toString());
    params = params.append("maxAge",this.userParams.maxAge.toString());
    params = params.append("gender",this.userParams.gender);
    params = params.append("orderBy",this.userParams.orderBy);

    return getPaginatedResult<Member[]>(this.baseUrl+"users", params, this.http)
      .pipe(map(response=>{
        this.memberCache.set(key,response);
        return response;
      }));
  }

  getMember(username:string) {
    const member = [...this.memberCache.values()]
       .reduce((arr, elem)=>arr.concat(elem.result),[])
       .find((member:Member)=>member.username==username);

    if(member) {
        return of(member);
    }
    return this.http.get<Member>(this.baseUrl+"users/"+username); // httpOptions);    
    // Cannot add a user here because it would upset the logic 
    // in case a pages is refreshed, members[] would be empty, and we add a user here 
    // next time we try to ge all members the app would think we are ok and wqon;t load additional data from the back end
    //  .pipe(
    //     map(member=>{
    //       this.members.push(member);
    //       return member;  
    //     })
    //   );
  }

  updateMember(member: Member) {
    return this.http.put(this.baseUrl+"users",member)
      .pipe(
        map(()=>{
          const index = this.members.findIndex(item=>item.username===member.username);
          if(index >= 0)
            this.members[index] = member
        }
      ));
  }

  setMainPhoto(photoId: number)
  {
    return this.http.put(this.baseUrl+"users/set-main-photo/"+photoId, {});
  }

  deletePhoto(photoId: number)
  {
    return this.http.delete(this.baseUrl+"users/delete-photo/"+photoId);
  }

  addLike(username: string)
  {
    return this.http.post(this.baseUrl+"likes/"+username, {}); 
  }

  getLikes(predicate: string, pageNumber: number, pageSize: number)
  {
    let params = getPaginationHeaders(pageNumber,pageSize);
    params = params.append("predicate",predicate);   
    
    return getPaginatedResult<Partial<Member[]>>(this.baseUrl+"likes",params, this.http);
    
    //you can add query string directly, or create HttpParams ad send in in the options (the sencond parameter of http.get() call)
    //{params: httpParams}
    //user generic get<> to return Observable<Member[]> instead of Observalble <Object>
  }

  setUserParams(params: UserParams)
  {
    this.userParams = params;
  }

  getUserParams() : UserParams
  {
    return this.userParams;
  }

  resetUserParams() : UserParams
  {
    this.userParams = new UserParams(this.user);
    return this.userParams;
  }

  // private getPaginationHeaders(pageNumber: number, pageSize: number) : HttpParams
  // {
  //   let params = new HttpParams(); //use to serialize parameters, so that they could be added to the query string
  //   params = params.append("pageNumber",pageNumber.toString());
  //   params = params.append("pageSize",pageSize.toString());
      
  //   return params;
  // }

  // private getPaginatedResult<T>(url: string, params: HttpParams)
  // {
  //   //get the members  
  //   // when we use http get normally - the call will gve us response body 
  //   // when we are observing the response, then we get the full response back    
  //   const paginatedResult: PaginatedResult<T> = new PaginatedResult<T>();     
  //   return this.http.get<T>(url,{observe:"response",params:params})
  //     .pipe(
  //       map(response=> {
  //         paginatedResult.result = response.body; //we explicilty have to access the .body
  //         if(response.headers.get("Pagination") !== null) {
  //           paginatedResult.pagination = JSON.parse(response.headers.get("Pagination"));
  //         }
  //         return paginatedResult;
  //       })
  //     );    
  // }  
}
