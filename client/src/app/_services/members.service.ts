import { HttpClient, HttpResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { of } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { Member } from '../_models/member';

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

  constructor(private http: HttpClient) { }

  getMembers() {
    //use cache
    if(this.members.length>0)
      return of<Member[]>(this.members);
    //set the members  
    return this.http.get<Member[]>(this.baseUrl+"users")
      .pipe(
        map(members=>{
          this.members = members;
          return members;
        })
      );
  }

  getMember(username:string) {
    const member = this.members.find(member=>member.username===username);
    if(member != undefined)
      return of<Member>(member);
    return this.http.get<Member>(this.baseUrl+"users/"+username); // httpOptions);    
    // Cannot add a user her because it would upset the logic 
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
}
